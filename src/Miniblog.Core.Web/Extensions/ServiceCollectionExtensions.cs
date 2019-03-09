using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Miniblog.Core.Data.Repositories;
using Miniblog.Core.Framework.Common;
using Miniblog.Core.Framework.Localization;
using Miniblog.Core.Framework.Users;
using Miniblog.Core.Framework.Web.Users;
using Miniblog.Core.Migration;
using Miniblog.Core.Service.Services;
using Miniblog.Core.Service.Settings;
using Miniblog.Core.Web.Filters;
using WilderMinds.MetaWeblog;
using MetaWeblogService = Miniblog.Core.Service.Services.MetaWeblogService;

namespace Miniblog.Core.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTranslations(this IServiceCollection services, IConfiguration configuration, string traslationFolderPath) 
		{
			services.AddSingleton<ITranslationLoader, TranslationLoader>(s =>
			{
				var hostingEnv = s.GetService<IHostingEnvironment>();
				return new TranslationLoader(Path.Combine(hostingEnv.ContentRootPath, traslationFolderPath));
			});
			services.AddSingleton<ITranslationStore, TranslationStore>();
			services.AddSingleton<ITranslationProvider, TranslationProvider>();

			var localizationSettings = configuration.GetSection("Localization").Get<LocalizationSettings>();
			
			// Request localization
			services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new RequestCulture(localizationSettings.DefaultCulture);
				options.SupportedCultures = localizationSettings.SupportedCultures.Select(CultureInfo.GetCultureInfo).ToList();
				options.RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() };
			});
		}

		public static void AddFiltersAsServices(this IServiceCollection services)
		{
			services.AddScoped(typeof(TransactionPerRequestFilter), typeof(TransactionPerRequestFilter));
		}

		public static IMvcBuilder AddMvcWithFilters(this IServiceCollection services, DevelopmentSettings developmentSettings, BlogSettings blogSettings)
		{
			return services.AddMvc(opts =>
				{
					if(developmentSettings.AuthenticationDisabled)
						opts.Filters.Add(new AllowAnonymousFilter());
					
					if(blogSettings.StorageType == StorageType.SQLite)
						opts.Filters.AddService<TransactionPerRequestFilter>();
				});
		}

		public static void AddIdentityAuthentication(this IServiceCollection services)
		{
			services.AddSingleton<IUserRoleResolver, IdentityUserRoleResolver>();
		}

		public static void ForceAlwaysAuthenticated(this IServiceCollection services)
		{
			services.AddSingleton<IUserRoleResolver, AdminUserRoleResolver>();
		}

		public static void UseXmlStorage(this IServiceCollection services)
		{
			services.AddSingleton<IBlogRepository, XmlFileBlogRepository>(s =>
			{
				var hostingEnv = s.GetService<IHostingEnvironment>();
				var userResolver = s.GetService<IUserRoleResolver>();
				var dateTimeProvider = s.GetService<IDateTimeProvider>();
				return new XmlFileBlogRepository(hostingEnv.WebRootPath, userResolver, dateTimeProvider);
			});
		}

		public static void UseSqliteStorage(this IServiceCollection services, string connectionStringName)
		{
			services.AddScoped<IDbConnection>(s => new SqliteConnection(connectionStringName));
			services.AddScoped<IDbTransaction>(s =>
            {
                var connection = s.GetService<IDbConnection>();
                connection.Open();

                return connection.BeginTransaction();
            });			
		}

		public static void UseMigrations(this IServiceCollection services, IConfiguration configuration, string connectionStringName)
		{
			var connectionString = configuration.GetConnectionString(connectionStringName);

			services.AddFluentMigratorCore()
				.ConfigureRunner(rb => rb
					.AddSQLite()
					.WithGlobalConnectionString(connectionString)
					.ScanIn(typeof(MigrationRoot).Assembly).For.Migrations());
		}

		public static void AddBlog(this IServiceCollection services, IConfiguration configuration)
		{
			var blogSection = configuration.GetSection("blog");
			var blogSettings = blogSection.Get<BlogSettings>();

			services.AddMetaWeblog<MetaWeblogService>();
			services.AddSingleton<IUserService, BlogUserService>();
			services.AddSingleton<IFilePersisterService, FilePersisterService>();
			services.AddSingleton<IRenderService, HtmlRenderService>();
			services.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
			services.AddSingleton<IRouteService, BlogRouteService>();

			services.AddScoped<IBlogService, BlogService>();
			services.Configure<BlogSettings>(blogSection);

			if(blogSettings.StorageType == StorageType.XML)
				services.UseXmlStorage();

			if(blogSettings.StorageType == StorageType.SQLite)
				services.UseSqliteStorage(blogSettings.ConnectionStringName);
		}
    }
}