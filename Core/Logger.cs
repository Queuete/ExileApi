namespace ExileCore
{
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    public class Logger
    {
        private static ILogger _instance;

        /// <summary>
        ///     Initializes the Logging Level.
        /// </summary>
        /// <remarks>
        ///     Sinks act as filters and the value we initialize the logger at establishes the minimum level of messages we want
        ///     to capture. Setting the initial value to <see cref="LogEventLevel.Verbose"/> allows us to capture all messages.
        ///     We can then use <see cref="LoggingLevel.MinimumLevel"/> raise the minimum level and filter out messages. If you
        ///     change the initial value to a higher level, the sink will ignore any message at a lower level. Changing the
        ///     <see cref="LoggingLevel.MinimumLevel"/> to a lower level than the initial value will have no effect as those
        ///     messages are ignored.
        ///
        ///     This is not the same switch as <see cref="Core.LoggingLevel"/>. See <see cref="Loader"/>.
        /// </remarks>
        public static LoggingLevelSwitch LoggingLevel => new LoggingLevelSwitch(LogEventLevel.Verbose);

        /// <summary>
        ///     Initializes the logger.
        /// </summary>
        /// <remarks>
        ///     It is important to note that the <see cref="LoggingLevel.MinimumLevel"/> can only be raised for sinks, not
        ///     lowered. The value used to initialize the <see cref="LoggingLevelSwitch"/> establishes the
        ///     <see cref="LoggingLevel.MinimumLevel"/> of messages the sink will process during the sink lifetime. We can raise
        ///     the level to filter messages we don't want to write to a console or file, but we cannot lower it below the
        ///     initial minimum level. The initial value essentially acts as a floor. Therefore, we initialize the switch at the
        ///     Verbose level to catch everything and then filter by changing the MinimumLevel of the switch from within the app.
        ///
        ///     This is not the same log sink as <see cref="Core.Logger"/>. See <see cref="Loader" />.
        /// </remarks>
        public static ILogger Log => _instance ?? (_instance = new LoggerConfiguration()
                .MinimumLevel
                .ControlledBy(LoggingLevel)
                .WriteTo
                .File(@"Logs\Verbose-.log", rollingInterval: RollingInterval.Day).CreateLogger());
    }
}
