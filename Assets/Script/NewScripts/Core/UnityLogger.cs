using UnityEngine;

namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Implementation of ILogger that uses Unity's Debug class
    /// </summary>
    public class UnityLogger : ILogger
    {
        private readonly string _prefix;

        public UnityLogger(string prefix = "")
        {
            _prefix = string.IsNullOrEmpty(prefix) ? "" : $"[{prefix}] ";
        }

        public void LogInfo(string message)
        {
            Debug.Log($"{_prefix}{message}");
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning($"{_prefix}{message}");
        }

        public void LogError(string message)
        {
            Debug.LogError($"{_prefix}{message}");
        }

        public void LogDebug(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"{_prefix}[DEBUG] {message}");
#endif
        }
    }
}