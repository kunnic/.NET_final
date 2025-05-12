using NLog;
using System;

namespace news_project_mvc.Services
{
    public class LoggingService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public static void LogWarning(string message)
        {
            _logger.Warn(message);
        }

        public static void LogError(string message, Exception exception = null)
        {
            if (exception != null)
            {
                _logger.Error(exception, message);
            }
            else
            {
                _logger.Error(message);
            }
        }

        public static void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public static void LogFatal(string message, Exception exception = null)
        {
            if (exception != null)
            {
                _logger.Fatal(exception, message);
            }
            else
            {
                _logger.Fatal(message);
            }
        }

        public static void LogTrace(string message)
        {
            _logger.Trace(message);
        }
    }
}
