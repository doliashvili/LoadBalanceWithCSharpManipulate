using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace MultiBaseAddressHttpClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped<CallApiService>();
            //services.AddScoped<HttpRandomHandler>();
            var httpClientsConfigs = Configuration.GetSection("HttpClientsConfigs").Get<HttpClientsConfigs>();

            services.AddTransient(p => new HttpRandomHandler(httpClientsConfigs));

            services.AddHttpClient<CallApiService>("name").AddHttpMessageHandler<HttpRandomHandler>();

            //foreach (var item in httpClientsConfigs.HttpClientInfo)
            //{
            //    services.AddHttpClient(item.Name, c =>
            //    {
            //        c.BaseAddress = new Uri(item.BaseAddress);
            //        c.DefaultRequestHeaders.Add("Accept", "application/json");
            //    }).AddHttpMessageHandler<HttpRandomHandler>();
            //}

            services.AddControllers();
            services.AddCors();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiBaseAddressHttpClient", Version = "v1" });
            });

            services.Configure<HttpClientsConfigs>(Configuration.GetSection("HttpClientsConfigs"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiBaseAddressHttpClient v1"));
            }

            app.UseCors(x =>
            {
                x.AllowAnyOrigin();
                x.AllowAnyHeader();
                x.AllowAnyMethod();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
