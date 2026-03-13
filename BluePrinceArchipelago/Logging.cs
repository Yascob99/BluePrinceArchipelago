using UnityEngine;

namespace BluePrinceArchipelago
{
    public static class Logging {
        public static void Log(object message) => Plugin.Instance.Log.LogMessage(message);


        public static void LogWarning(object message) => Plugin.Instance.Log.LogWarning(message);

        public static void LogError(object message) => Plugin.Instance.Log.LogError(message);

        public static void LogDebug(object message) => Plugin.Instance.Log.LogDebug(message);

        public static void LogFatal(object message) => Plugin.Instance.Log.LogFatal(message);

    }
}
