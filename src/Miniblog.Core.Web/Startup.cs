using System;
using AutoMapper;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Miniblog.Core.Service.Settings;
using Miniblog.Core.Web.Extensions;
using WebEssentials.AspNetCore.OutputCaching;
using WebEssentials.AspNetCore.Pwa;
using WebMarkupMin.AspNetCore2;
using WebMarkupMin.Core;
using WilderMinds.MetaWeblog;

using IWmmLogger = WebMarkupMin.Core.Loggers.ILogger;
using WmmNullLogger = WebMarkupMin.Core.Loggers.NullLogger;

namespace Miniblog.Core.Web
{
	public class Startup
	{
		private readonly DevelopmentSettings _developmentSettings;
		private readonly BlogSettings _blogSettings;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_developmentSettings = Configuration.GetSection("Development").Get<DevelopmentSettings>();
			_blogSettings = Configuration.GetSection("blog").Get<BlogSettings>();
		}

		public static void Main(string[] args)
		{
			var webHost = CreateWebHostBuilder(args).Build();
			UpdateDatabase(webHost.Services);
			webHost.Run();
		}

		private static void UpdateDatabase(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
				runner.MigrateUp();
			}
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseKestrel(a => a.AddServerHeader = false);

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAutoMapper();

			services.AddFiltersAsServices();
			services.AddMvcWithFilters(_developmentSettings, _blogSettings)
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.UseMigrations(Configuration, "DefaultConnection");
			services.AddTranslations(Configuration, "App_Data/content/i18n");
			services.AddBlog(Configuration);

			if (_developmentSettings.AuthenticationDisabled)
			{
				services.ForceAlwaysAuthenticated();
			}
			else
			{
				services.AddIdentityAuthentication();
			}

			services.Configure<DevelopmentSettings>(Configuration.GetSection("Development"));
			services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			// Progressive Web Apps https://github.com/madskristensen/WebEssentials.AspNetCore.ServiceWorker
			services.AddProgressiveWebApp(new PwaOptions
			{
				Strategy = ServiceWorkerStrategy.NetworkFirst,
				RoutesToPreCache = "/",
				OfflineRoute = "/shared/offline/"
			});

			// Output caching (https://github.com/madskristensen/WebEssentials.AspNetCore.OutputCaching)
			if (!_developmentSettings.OutputCachingDisabled)
			{
				services.AddOutputCaching(options =>
				{
					options.Profiles["default"] = new OutputCacheProfile
					{
						Duration = 3600
					};
				});
			}


			// Cookie authentication.
			services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/login/";
					options.LogoutPath = "/logout/";
				});

			// HTML minification (https://github.com/Taritsyn/WebMarkupMin)
			if(!_developmentSettings.MinificationDisabled)
			{
				services
					.AddWebMarkupMin(options =>
					{
						options.AllowMinificationInDevelopmentEnvironment = true;
						options.DisablePoweredByHttpHeaders = true;
					})
					.AddHtmlMinification(options =>
					{
						options.MinificationSettings.RemoveOptionalEndTags = false;
						options.MinificationSettings.WhitespaceMinificationMode = WhitespaceMinificationMode.Safe;
					});
			}
			
			services.AddSingleton<IWmmLogger, WmmNullLogger>(); // Used by HTML minifier

			// Bundling, minification and Sass transpilation (https://github.com/ligershark/WebOptimizer)
			services.AddWebOptimizer(pipeline =>
			{
				if(!_developmentSettings.MinificationDisabled)
				{
					pipeline.MinifyJsFiles();
				}				
				pipeline.CompileScssFiles()
						.InlineImages(1);
			});
		}



		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Shared/Error");
				app.UseHsts();
			}

			app.UseRequestLocalization();

			app.Use((context, next) =>
			{
				context.Response.Headers["X-Content-Type-Options"] = "nosniff";
				return next();
			});

			app.UseStatusCodePagesWithReExecute("/Shared/Error");
			app.UseWebOptimizer();

			app.UseStaticFilesWithCache();

			if (Configuration.GetValue<bool>("forcessl"))
			{
				app.UseHttpsRedirection();
			}

			app.UseMetaWeblog("/metaweblog");
			app.UseAuthentication();

			if (!_developmentSettings.OutputCachingDisabled)
			{
				app.UseOutputCaching();
			}

			if (!_developmentSettings.MinificationDisabled)
			{
				app.UseWebMarkupMin();
			}

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Blog}/{action=Index}/{id?}");
			});
		}
	}
}
