using IAmTheServiceNow;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((serviceProvider, loggerConfiguration) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var logsDirectory = @"logs";
    
    loggerConfiguration.ReadFrom.Configuration(configuration);
    loggerConfiguration.WriteTo.Console();
    
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

// DEBUG
builder.Services.AddOptions<ServiceProfileOptions>()
    .BindConfiguration("");

builder.Services.AddHostedService<ServiceManager>();

builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();