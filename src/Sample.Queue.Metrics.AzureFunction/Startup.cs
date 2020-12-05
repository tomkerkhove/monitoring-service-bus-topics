using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Queue.Metrics.AzureFunction;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Sample.Queue.Metrics.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddCorrelation();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("Deadletter Monitor")
                .Enrich.WithVersion()
                .Enrich.WithCorrelationInfo()
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsights(instrumentationKey)
                .CreateLogger();
            
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProvidersExceptFunctionProviders();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }
}
