using Microsoft.Extensions.Options;
using Serilog;

namespace IAmTheServiceNow.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    
            loggerConfiguration.ReadFrom.Configuration(configuration);
            loggerConfiguration.WriteTo.Console();
    
            var serviceProfileOptions = serviceProvider.GetRequiredService<IOptions<ServiceProfileOptions>>();
            var logsDirectory = Path.Combine(serviceProfileOptions.Value.WorkingDirectory, "Logs");
            
            loggerConfiguration.WriteTo.Conditional(
                (logEvent) => logEvent.IsServiceManagerLogEvent() is false,
                sink =>
                {
                    sink.File(Path.Combine(logsDirectory, "logs.txt"));
                });
    
            loggerConfiguration.WriteTo.Conditional(
                (logEvent) => logEvent.IsServiceManagerLogEvent(Serilog.Events.LogEventLevel.Information),
                sink =>
                {
                    sink.File(Path.Combine(logsDirectory, "output.txt"),
                        outputTemplate: Constants.LogFormat);
                });
    
            loggerConfiguration.WriteTo.Conditional(
                (logEvent) => logEvent.IsServiceManagerLogEvent(Serilog.Events.LogEventLevel.Error),
                sink =>
                {
                    sink.File(Path.Combine(logsDirectory, "error.txt"),
                        outputTemplate: Constants.LogFormat);
                });
        });

        return services;
    }
}