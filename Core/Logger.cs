using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ExileCore
{
    public class Logger
    {
        private static ILogger _instance;
        public static ILogger Log =>
            _instance = _instance ?? new LoggerConfiguration()
                .MinimumLevel
                .ControlledBy(new LoggingLevelSwitch(LogEventLevel.Verbose))
                .WriteTo
                .File(@"Logs\Verbose-.log", rollingInterval: RollingInterval.Day).CreateLogger();
    }
}