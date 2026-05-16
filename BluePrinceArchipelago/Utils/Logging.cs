using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BluePrinceArchipelago
{
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
                Plugin.Instance.Log.LogMessage($"[{logTag}] {message}");
        }

        public void LogWarning(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Warning))
                Plugin.Instance.Log.LogWarning($"[{logTag}] {message}");
        }

        public void LogError(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Error))
                Plugin.Instance.Log.LogError($"[{logTag}] {message}");
        }

        public void LogDebug(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Debug))
                Plugin.Instance.Log.LogDebug($"[{logTag}] {message}");
        }

        public void LogFatal(object message, [CallerMemberName] string logTag = null)
        {
            if (ShouldLog(logTag, LogLevel.Fatal))
                Plugin.Instance.Log.LogFatal($"[{logTag}] {message}");
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}
