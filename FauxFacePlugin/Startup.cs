using FauxFace.Db;
using FauxFacePlugin;
using Griffeye;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FauxFace
{
    public class HtmlOutputFormatter : StringOutputFormatter
    {
        public HtmlOutputFormatter()
        {
            SupportedMediaTypes.Add("text/html");
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            void ConfigureHttpClient(HttpClient httpClient)
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            services.AddTransient<ConnectMessageHandler>();
            services.AddHttpClient<Client>(ConfigureHttpClient)
                .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler());
            services
                .AddHttpClient<UserActionClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>()
                .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler());
            services
                .AddHttpClient<FileClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>()
                .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler());
            services
                .AddHttpClient<FileBookmarkClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>()
                .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler());
            services
                .AddHttpClient<FileMetadataClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>()
                .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler());

            services.AddSingleton<IFauxFaceDb, FauxFaceDb>();
            services.AddHttpContextAccessor();
            services.AddMvc(o =>
            {
                o.OutputFormatters.Add(new HtmlOutputFormatter());
                o.EnableEndpointRouting = false;
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Allow self-signed certificates
            static Func<HttpMessageHandler> ConfigureHttpMessageHandler()
            {
                return () =>
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                    };

                    return handler;
                };
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            Task.Run(async () =>
            {
                try
                {
                    var plugin = new Plugin(app.ApplicationServices.GetService<UserActionClient>());
                    await plugin.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize plugin: {ex.Message}");
                    Console.WriteLine(ex);
                }
            }).Wait();

            app.UseMvc();
        }
    }
}