using Serilog.Events;

namespace IAmTheServiceNow;

internal static class Extensions
{
    private const string SourceContext = "SourceContext";
    
    internal static bool IsServiceManagerLogEvent(this LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(SourceContext, out var sourceContextProperty) is false)
        {
            return false;
        }
        
        var sourceContext = sourceContextProperty.ToString().Trim('\"');
        return sourceContext.Equals(Constants.ServiceManagerLogger, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsServiceManagerLogEvent(this LogEvent logEvent, LogEventLevel logLevel)
    {
        return logEvent.IsServiceManagerLogEvent()
            && logEvent.Level == logLevel;
    }
}