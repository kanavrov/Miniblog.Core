using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Miniblog.Core.Data.Repositories;
using Miniblog.Core.Framework.Localization;
using Miniblog.Core.Framework.Users;
using Miniblog.Core.Framework.Web.Users;
using Miniblog.Core.Migration;
using Miniblog.Core.Service.Services;
using Miniblog.Core.Service.Settings;
using WebEssentials.AspNetCore.OutputCaching;
using WebMarkupMin.AspNetCore2;
using WebMarkupMin.Core;
using WilderMinds.MetaWeblog;

using IWmmLogger = WebMarkupMin.Core.Loggers.ILogger;
using MetaWeblogService = Miniblog.Core.Service.Services.MetaWeblogService;
using WmmNullLogger = WebMarkupMin.Core.Loggers.NullLogger;

namespace Miniblog.Core.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
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
			var localizationSettings = Configuration.GetSection("Localization").Get<LocalizationSettings>();
			var developmentSettings = Configuration.GetSection("Development").Get<DevelopmentSettings>();

			services.AddAutoMapper();

			IMvcBuilder mvc;
			if (developmentSettings.AuthenticationDisabled)
			{
				mvc = services.AddMvc(opts =>
				{
					opts.Filters.Add(new AllowAnonymousFilter());
				});
			}
			else
			{
				mvc = services.AddMvc();
			}
			mvc.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			ConfigureMigrations(services);

			services.AddSingleton<ITranslationLoader, TranslationLoader>(s =>
			{
				var hostingEnv = s.GetService<IHostingEnvironment>();
				return new TranslationLoader(Path.Combine(hostingEnv.ContentRootPath, "App_Data/content/i18n"));
			});
			services.AddSingleton<ITranslationStore, TranslationStore>();
			services.AddSingleton<ITranslationProvider, TranslationProvider>();

			services.AddSingleton<IUserServices, BlogUserServices>();
			services.AddSingleton<IFilePersisterService, FilePersisterService>();
			services.AddSingleton<IRenderService, HtmlRenderService>();
			services.AddSingleton<IBlogRepository, XmlFileBlogRepository>(s =>
			{
				var hostingEnv = s.GetService<IHostingEnvironment>();
				var userResolver = s.GetService<IUserRoleResolver>();
				return new XmlFileBlogRepository(hostingEnv.WebRootPath, userResolver);
			});

			if(developmentSettings.AuthenticationDisabled)
			{
				services.AddSingleton<IUserRoleResolver, AdminUserRoleResolver>();
			}
			else
			{
				services.AddSingleton<IUserRoleResolver, IdentityUserRoleResolver>();
			}
			
			services.AddScoped<IBlogService, BlogService>();
			services.AddSingleton<IRouteService, BlogRouteService>();
			services.Configure<BlogSettings>(Configuration.GetSection("blog"));
			services.Configure<DevelopmentSettings>(Configuration.GetSection("Development"));
			services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddMetaWeblog<MetaWeblogService>();

			// Progressive Web Apps https://github.com/madskristensen/WebEssentials.AspNetCore.ServiceWorker
			services.AddProgressiveWebApp(new WebEssentials.AspNetCore.Pwa.PwaOptions
			{
				OfflineRoute = "/shared/offline/"
			});

			// Output caching (https://github.com/madskristensen/WebEssentials.AspNetCore.OutputCaching)
			services.AddOutputCaching(options =>
			{
				options.Profiles["default"] = new OutputCacheProfile
				{
					Duration = 3600
				};
			});

			// Cookie authentication.
			services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = "/login/";
					options.LogoutPath = "/logout/";
				});

			// HTML minification (https://github.com/Taritsyn/WebMarkupMin)
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
			services.AddSingleton<IWmmLogger, WmmNullLogger>(); // Used by HTML minifier

			// Bundling, minification and Sass transpilation (https://github.com/ligershark/WebOptimizer)
			services.AddWebOptimizer(pipeline =>
			{
				pipeline.MinifyJsFiles();
				pipeline.CompileScssFiles()
						.InlineImages(1);
			});

			// Request localization
			services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new RequestCulture(localizationSettings.DefaultCulture);
				options.SupportedCultures = localizationSettings.SupportedCultures.Select(CultureInfo.GetCultureInfo).ToList();
				options.RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() };
			});
		}

		private void ConfigureMigrations(IServiceCollection services)
		{
			var connectionString = Configuration.GetConnectionString("DefaultConnection");

			services.AddFluentMigratorCore()
				.ConfigureRunner(rb => rb
					.AddSQLite()
					.WithGlobalConnectionString(connectionString)
					.ScanIn(typeof(MigrationRoot).Assembly).For.Migrations());
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

			app.UseOutputCaching();
			app.UseWebMarkupMin();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Blog}/{action=Index}/{id?}");
			});
		}
	}
}
