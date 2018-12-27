using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DockerSmtpTest
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var logDir = Configuration.GetValue<string>("logDirectory");
            if(!string.IsNullOrWhiteSpace(logDir))
            {
                var config = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Filter.ByIncludingOnly((e) =>
                    {
                        return e.Level >= Serilog.Events.LogEventLevel.Error;
                    })
                    .WriteTo.RollingFile(string.Concat(logDir, "/{Date}.txt"));

                var serilog = config.CreateLogger();
                loggerFactory.AddSerilog(serilog);
                appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
            }

            app.UseMvc();
        }
    }
}
