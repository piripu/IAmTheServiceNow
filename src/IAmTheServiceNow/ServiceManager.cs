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

        try
        {
            process.Start();

            if (process.HasExited)
            {
                _logger.LogError("Process failed to start. Exit Code: {ExitCode}", process.ExitCode);

                var output = await process.StandardOutput.ReadToEndAsync(stoppingToken);
                _serviceLogger.LogInformation(output);

                var error = await process.StandardError.ReadToEndAsync(stoppingToken);
                _serviceLogger.LogError(error);
            }

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            // TODO: Get all child processes and put add to job

            while (stoppingToken.IsCancellationRequested is false)
            {
                await Task.Delay(1000, stoppingToken);

                // TODO: handle restarts and checks
            }

            KillProcess(process);
        }
        catch (TaskCanceledException e)
        {
            _logger.LogWarning(e, "Service requested to stop.");
            KillProcess(process);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Service encountered an error.");
            throw;
        }
    }

    private void KillProcess(Process process)
    {
        _logger.LogDebug("Sending kill command...");
        process.Kill();
        process.WaitForExit();
        _logger.LogInformation("Service stopped.");
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