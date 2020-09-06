using System;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

namespace OpenTracingSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services
                .AddSingleton(serviceProvider =>
                {
                    var serviceName = serviceProvider.GetRequiredService<IWebHostEnvironment>().ApplicationName;
                    Environment.SetEnvironmentVariable(Jaeger.Configuration.JaegerServiceName, serviceName);
                    Environment.SetEnvironmentVariable(Jaeger.Configuration.JaegerSamplerType, ConstSampler.Type);
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    var config = Jaeger.Configuration.FromEnv(loggerFactory);
                    var tracer = config.GetTracer();
                    GlobalTracer.RegisterIfAbsent(tracer);
                    return tracer;
                })
                .AddOpenTracing();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
