namespace ThreeDGeneration.Core
{
    /// <summary>
    /// Interface for logging services
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogInfo(string message);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogError(string message);

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogDebug(string message);
    }
}