﻿namespace Scaffold.WebApi
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prometheus;
    using Scaffold.Repositories.PostgreSQL;
    using Scaffold.WebApi.Extensions;
    using Scaffold.WebApi.Filters;
    using Scaffold.WebApi.Middleware;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<BucketContext>();

            services
                .AddHttpClients()
                .AddOptions(this.Configuration)
                .AddRepositories(this.Configuration)
                .AddServices();

            services.AddControllers(options => options.Filters.Add<ExceptionFilter>())
                .AddCustomJsonOptions()
                .AddCustomXmlFormatters();

            services.AddApiDocumentation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app
                .UseMiddleware<RequestLoggingMiddleware>()
                .UseMiddleware<OpenTracingSpanTaggingMiddleware>()
                .UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Scaffold.WebApi v1");
                });

            app
                .UseRouting()
                .UseHttpMetrics()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    IEndpointConventionBuilder healthCheckEndpoint = endpoints.MapHealthChecks("/health");

                    if (this.Configuration["HealthCheckPort"] != null)
                    {
                        healthCheckEndpoint.RequireHost($"*:{this.Configuration["HealthCheckPort"]}");
                    }

                    IEndpointConventionBuilder metricsEndpoint = endpoints.MapMetrics();

                    if (this.Configuration["MetricsPort"] != null)
                    {
                        metricsEndpoint.RequireHost($"*:{this.Configuration["MetricsPort"]}");
                    }

                    endpoints.MapControllers();
                });
        }
    }
}
