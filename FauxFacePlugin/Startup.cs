using FauxFace.Db;
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
using FauxFacePlugin;
using Griffeye;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            services.AddHttpClient<Client>(ConfigureHttpClient);
            services
                .AddHttpClient<UserActionClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>();
            services
                .AddHttpClient<FileClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>();
            services
                .AddHttpClient<FileBookmarkClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>();
            services
                .AddHttpClient<FileMetadataClient>(ConfigureHttpClient)
                .AddHttpMessageHandler<ConnectMessageHandler>();

            services.AddSingleton<IFauxFaceDb, FauxFaceDb>();
            services.AddHttpContextAccessor();
            services.AddMvc(o =>
            {
                o.OutputFormatters.Add(new HtmlOutputFormatter());
                o.EnableEndpointRouting = false;
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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
