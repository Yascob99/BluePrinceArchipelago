#if ML
using MelonLoader;
#endif
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace BluePrinceArchipelago
{
    /// <summary>
    ///     The Static class that handles logging globally
    /// </summary>
    public static class Logging {

        public static Logger Logger { get; } = new Logger();
        public static void Log(object message, [CallerMemberName] string logTag = null) => Logger.Log(message, logTag);


        public static void LogWarning(object message, [CallerMemberName] string logTag = null) => Logger.LogWarning(message, logTag);

        public static void LogError(object message, [CallerMemberName] string logTag = null) => Logger.LogError(message, logTag);

        public static void LogDebug(object message, [CallerMemberName] string logTag = null) => Logger.LogDebug(message, logTag);

        public static void LogFatal(object message, [CallerMemberName] string logTag = null) => Logger.LogFatal(message, logTag);

        public static void SetLogLevel(string category, LogLevel level)
        {
            Logger.LogLevels[category] = level;
        }
    }

    /// <summary>
    ///     The Logger instance.
    /// </summary>
    public class Logger
    {
        public Dictionary<string, LogLevel> LogLevels { get; } = [];
        public LogLevel DefaultLogLevel { get; set; } = LogLevel.Warning;

        private bool ShouldLog(string category, LogLevel level)
        {
            if (LogLevels.TryGetValue(category, out var categoryLevel))
            {
                return level >= categoryLevel;
            }
            return level >= DefaultLogLevel;
        }

        public void Log(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Info))
#if Bep
                Plugin.Instance.Log.LogMessage($"[{logTag}] {message}");
#endif
#if ML
                MelonLogger.Msg($"[{logTag}] {message}");
#endif
        }

        public void LogWarning(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Warning))
#if Bep
                Plugin.Instance.Log.LogWarning($"[{logTag}] {message}");
#endif
#if ML
                MelonLogger.Warning($"[{logTag}] {message}");
#endif
        }

        public void LogError(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Error))
#if Bep
                Plugin.Instance.Log.LogError($"[{logTag}] {message}");
#endif
#if ML
                MelonLogger.Error($"[{logTag}] {message}");
#endif
        }

        public void LogDebug(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Debug))
#if Bep
                Plugin.Instance.Log.LogDebug($"[{logTag}] {message}");
#endif
#if ML
                MelonLogger.Warning($"[{logTag}] {message}");
#endif
        }

        public void LogFatal(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Fatal))
#if Bep
                Plugin.Instance.Log.LogFatal($"[{logTag}] {message}");
#endif
#if ML
                MelonLogger.Error($"[{logTag}] {message}");
#endif
        }
    }

    /// <summary>
    ///     The log levels.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}
