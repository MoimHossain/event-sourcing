
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperNova.Api.Supports;
using SuperNova.Shared.Configs;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Repositories;
using SuperNova.Storage;
using SuperNova.Storage.EventStore;

namespace SuperNova.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ConfigStore), (sp) =>
            {
                return new ConfigStore(
                    sp.GetService<IHostingEnvironment>().IsDevelopment(),
                    (key) => Configuration[key],
                    sp.GetService<ILoggerFactory>());
            });
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddCors();
            }
            // Add framework services.
            services.AddMvc();

            services.AddSingleton<IEventStore, EventStore>();
            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddSwaggerGen(sg => sg.SwaggerDoc(Constants.SwaggerInfo.Version, Constants.SwaggerInfo));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                // Allowing CORS for local development
                app.UseCors(options => options
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin());

                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
            }

            app.UseMvc();
            app.UseSwagger((swaggerDoc) =>
            {
             
            });

            app.UseSwaggerUI((uiOptions) =>
            {
                var prefix = env.IsDevelopment() ? string.Empty : Constants.BaseUrlPath;

                uiOptions.SwaggerEndpoint($"{prefix}/swagger/{Constants.SwaggerInfo.Version}/swagger.json", Constants.SwaggerInfo.Title);
            });
        }
    }
}
