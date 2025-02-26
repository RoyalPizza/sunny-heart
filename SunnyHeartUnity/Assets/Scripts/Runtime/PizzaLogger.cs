#if UNITY_EDITOR
#define LOGGING_ENABLED
#endif

using UnityEngine;

namespace Pizza.Runtime
{
    /// <summary>
    /// A wrapper for the unity logger.
    /// </summary>
    /// <remarks>
    /// The main purpose is so that informational and warning messages can be disabled when in unity editor.
    /// </remarks>
    public static class PizzaLogger
    {
        /* Note: 
		 * The Unity Debug.Log has a seperate function for logging object vs string.
		 * I assume calling the string version when possible is more performant. So
		 * we copy them and have one function for using the string and one for the object.
		*/

        public static void Log(string message)
        {
#if LOGGING_ENABLED
            Debug.Log(message);
#endif
        }

        public static void Log(string message, Object context)
        {
#if LOGGING_ENABLED
            Debug.Log(message, context);
#endif
        }

        public static void Log(object message)
        {
#if LOGGING_ENABLED
            Debug.Log(message);
#endif
        }

        public static void Log(object message, Object context)
        {
#if LOGGING_ENABLED
            Debug.Log(message, context);
#endif
        }

        public static void LogWarning(string message)
        {
#if LOGGING_ENABLED
            Debug.LogWarning(message);
#endif
        }

        public static void LogWarning(string message, Object context)
        {
#if LOGGING_ENABLED
            Debug.LogWarning(message, context);
#endif
        }

        public static void LogWarning(object message)
        {
#if LOGGING_ENABLED
            Debug.LogWarning(message);
#endif
        }

        public static void LogWarning(object message, Object context)
        {
#if LOGGING_ENABLED
            Debug.LogWarning(message, context);
#endif
        }

        public static void LogError(string message)
        {
#if LOGGING_ENABLED
            Debug.LogError(message);
#endif
        }

        public static void LogError(string message, Object context)
        {
#if LOGGING_ENABLED
            Debug.LogError(message, context);
#endif
        }

        public static void LogError(object message)
        {
#if LOGGING_ENABLED
            Debug.LogError(message);
#endif
        }

        public static void LogError(object message, Object context)
        {
#if LOGGING_ENABLED
            Debug.LogError(message, context);
#endif
        }

        public static void LogException(System.Exception exception)
        {
#if LOGGING_ENABLED
            Debug.LogException(exception);
#endif
        }

        public static void LogException(System.Exception exception, Object context)
        {
#if LOGGING_ENABLED
            Debug.LogException(exception, context);
#endif
        }

        public static void LogNotImplemented()
        {
            LogWarning("Not Implemented");
        }
    }
}