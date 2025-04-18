using IAmTheServiceNow;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((serviceProvider, loggerConfiguration) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    loggerConfiguration.ReadFrom.Configuration(configuration);
    loggerConfiguration.WriteTo.Console();
    loggerConfiguration.WriteTo.File(@"logs.txt");
});

// DEBUG
builder.Services.AddOptions<ServiceProfileOptions>()
    .BindConfiguration("");

builder.Services.AddHostedService<ServiceManager>();

builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();