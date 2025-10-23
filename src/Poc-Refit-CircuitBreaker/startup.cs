using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Refit;

namespace Poc_Refit_CircuitBreaker
{
    public class startup
    {
        public startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRefitClient<IPocRefit>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000");
                    c.Timeout = TimeSpan.FromMilliseconds(150);
                });
            
            services.AddRefitClient<IPocRefitResiliente>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000");
                    c.Timeout = TimeSpan.FromMilliseconds(150);
                })
                .AddPolicyHandler(circuitBreaker.CreatePolicy());
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Poc_Refit_CircuitBreaker", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Poc_Refit_CircuitBreaker v1"));
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

public class ExternalRequest
{
    public string Anything { get; set; }
}

public interface IPocRefit
{
    [Post("/api/poc/external")]
    Task<object> External([Body] ExternalRequest request);
}

public interface IPocRefitResiliente
{
    [Post("/api/poc/external")]
    Task<object> External([Body] ExternalRequest request);
}