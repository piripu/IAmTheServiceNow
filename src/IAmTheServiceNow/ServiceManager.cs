using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace IAmTheServiceNow;

public class ServiceManager : BackgroundService
{
    private readonly ILogger<ServiceManager> _logger;
    private readonly ILogger _serviceLogger;
    private readonly IOptions<ServiceProfileOptions> _options;

    public ServiceManager(ILoggerFactory loggerFactory, IOptions<ServiceProfileOptions> options)
    {
        _logger = loggerFactory.CreateLogger<ServiceManager>();
        _serviceLogger = loggerFactory.CreateLogger(Constants.ServiceManagerLogger);
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = _options.Value.WorkingDirectory,
                FileName = "cmd.exe",
                Arguments = $"/c {_options.Value.PathToExecutable} {_options.Value.Arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;
        
        process.Start();
        
        
        while (stoppingToken.IsCancellationRequested is false)
        {
            await Task.Delay(1000, stoppingToken);
            
            // TODO: handle restarts and checks
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }
        
#pragma warning disable CA2254 // Pass through logging
        _serviceLogger.LogInformation(e.Data);
#pragma warning restore CA2254
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }
        
#pragma warning disable CA2254 // Pass through logging
        _serviceLogger.LogError(e.Data);
#pragma warning restore CA2254
    }
}