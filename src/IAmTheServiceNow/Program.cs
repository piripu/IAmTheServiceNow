using IAmTheServiceNow;
using IAmTheServiceNow.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Services.ConfigureLogging();

// DEBUG
builder.Services.AddOptions<ServiceProfileOptions>()
    .BindConfiguration("");

builder.Services.AddHostedService<ServiceManager>();

builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();