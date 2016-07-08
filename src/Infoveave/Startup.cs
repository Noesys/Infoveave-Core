/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
 * -------------
 * This file is part of Infoveave.
 * Infoveave is dual licensed under Infoveave Commercial License and AGPL v3  
 * -------------
 * You should have received a copy of the GNU Affero General Public License v3
 * along with this program (Infoveave)
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the Infoveave without
 * disclosing the source code of your own applications.
 * -------------
 * Authors: Naresh Jois <naresh@noesyssoftware.com>, et al.
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Infoveave.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog;
using Microsoft.Extensions.PlatformAbstractions;
using System.Linq;
namespace Infoveave
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"settings.{env.EnvironmentName.ToLower()}.json", optional: false)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
            environment = env;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", PlatformServices.Default.Application.ApplicationVersion)
                .WriteTo.RollingFile(System.IO.Path.Combine(Configuration["Logging:Path"],"Logs-{Date}.txt")).CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        private IHostingEnvironment environment { get; set; }

        private RsaSecurityKey RsaSecurityKey { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.Configure<ApplicationConfiguration>(Configuration);

            services.AddCors();

            var policy = new Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicy();
            policy.Headers.Add("*");
            policy.Methods.Add("*");
            policy.Origins.Add("*");

            services.AddCors(x => x.AddPolicy("corsGlobalPolicy", policy));

            services.AddSingleton<Data.Interfaces.ITenantContext, Data.Implementations.TenantContext>();
            services.AddSingleton<CacheProvider.ICacheProvider, CacheProvider.CacheProvider>();
            services.AddSingleton<Mailer.IMailer, Mailer.SmtpMailer>();

            var securityFile = System.IO.Path.Combine(environment.ContentRootPath, "securityParams.json");
            if (!System.IO.File.Exists(securityFile))
            {
                throw new Exception($"Secutiry File Missing : {securityFile}");
                // RSAKeyUtilities.GenerateKeyAndSave(securityFile);
            }
            this.RsaSecurityKey = new RsaSecurityKey(RSAKeyUtilities.GetKeyParameters(securityFile));
            SigningCredentials SigningParamters = new SigningCredentials(this.RsaSecurityKey, SecurityAlgorithms.RsaSha256Signature);
            services.AddSingleton<SigningCredentials>(SigningParamters);
            services.ConfigureSwaggerGen(c =>
            {
                var xmlFile = string.Empty;
                if (environment.IsDevelopment())
                {
                    xmlFile = System.IO.Path.Combine(environment.ContentRootPath,"bin","Debug","netCoreApp1.0","Infoveave.xml");
                }
                else
                {
                    xmlFile = System.IO.Path.Combine(environment.ContentRootPath, "Infoveave.xml");
                }
                
                c.IncludeXmlComments(xmlFile);
            });
            services.AddSwaggerGen(c =>
            {
                c.MultipleApiVersions(
                    new[]
                    {
                        new Swashbuckle.Swagger.Model.Info {
                            Version = "v2",
                            Title = "API Version 2.0",
                            Description ="Version 2 of the Infoveave API, The hosted API has to have a tenant",
                            License = new Swashbuckle.Swagger.Model.License() { Name = "Commercial", Url = "http://infoveave.com/license" },
                            Contact = new Swashbuckle.Swagger.Model.Contact() { Name = "Naresh Jois", Email = "naresh@noesyssoftware.com", Url = "http://infoveave.com" }
                        }
                    },
                    ResolveVersionSupportByVersionsConstraint
                );
                c.DocumentFilter<SetVersionInPaths>();
                c.OperationFilter<CancellationTokenOperationFilter>();

            });


            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new HandleApiExceptionAttribute());
                options.Filters.Add(new ValidateModelAttribute());
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<ApplicationConfiguration> configuration, Data.Interfaces.ITenantContext tenantContext, CacheProvider.ICacheProvider cacheProvider, Mailer.IMailer mailer)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseSwagger((httpRequest, swaggerDoc) =>
            {
                swaggerDoc.Host = httpRequest.Host.Value;
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            tenantContext.SetDatabaseConnection(useMultiTenancy: false, baseConnectionString: configuration.Value.Application.TenantDatabaseLocation);

            cacheProvider.ConfigureCaching(configuration.Value.Application.Caching.Enabled, configuration.Value.Application.Caching.Endpoint, configuration.Value.Application.Caching.Port);
            app.UseCors("corsGlobalPolicy");
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = this.RsaSecurityKey,
                    ValidIssuer = "Infoveave",
                    ValidAudience = "Infoveave",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                },
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,

            });
            Dictionary<string, AdapterFramework.IOlapAdapter> OlapAdapters = new Dictionary<string, AdapterFramework.IOlapAdapter>();
            OlapAdapters.Add("mondrianService", new Adapters.MondrianService.OlapAdapter(loggerFactory));

#if NET461
            OlapAdapters.Add("googleAnalytics", new Adapters.GoogleAnalytics.OlapAdapter(loggerFactory, System.IO.File.ReadAllText(System.IO.Path.Combine(environment.ContentRootPath, "googleAnalytics.json"))));
#endif

            Helpers.Adapters.OlapAdapters = OlapAdapters;

            Dictionary<string, AdapterFramework.ISQLAdapter> SQLAdapters = new Dictionary<string, AdapterFramework.ISQLAdapter>();
            SQLAdapters.Add("mssql", new Adapters.Sql.SQLAdapter(loggerFactory));
            #if NET461
            SQLAdapters.Add("sqlite", new Adapters.SQLite.SQLAdapter(loggerFactory));
#endif
            Helpers.Adapters.SQLAdapters = SQLAdapters;



            mailer.Configure(environment, loggerFactory, configuration.Value.Application.Mailer.SmtpServer,
            configuration.Value.Application.Mailer.SmtpPort, configuration.Value.Application.Mailer.SmtpUser, configuration.Value.Application.Mailer.SmtpPassword,
            configuration.Value.Application.Mailer.CopyAll.Split(';'), configuration.Value.Application.Mailer.FromName,
            configuration.Value.Application.Mailer.FromAddress);


            var logger = loggerFactory.CreateLogger("Core");
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMvc();
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                    logger.LogError("Exception Occured", error.Error);
                    if (error != null && error.Error is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = 401;
                    }
                    else await next();
                });
            });
        }


        private static bool ResolveVersionSupportByVersionsConstraint(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDesc, string version)
        {
            var versionAttribute = apiDesc.ActionDescriptor.ActionConstraints.OfType<VersionsAttribute>()
                .FirstOrDefault();
            if (versionAttribute == null) return true;

            return versionAttribute.AcceptedVersions.Contains(version);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member