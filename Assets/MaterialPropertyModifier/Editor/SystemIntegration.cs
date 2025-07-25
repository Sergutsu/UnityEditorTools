using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Provides system integration utilities and comprehensive error handling
    /// </summary>
    public static class SystemIntegration
    {
        private static readonly Dictionary<string, DateTime> _lastLogTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan _logThrottleInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Executes an operation with comprehensive error handling and logging
        /// </summary>
        public static OperationResult<T> ExecuteWithErrorHandling<T>(
            Func<T> operation, 
            string operationName, 
            T defaultValue = default(T),
            bool showUserDialog = false)
        {
            try
            {
                LogWithThrottling($"Starting {operationName}", LogLevel.Info);
                
                var result = operation();
                
                LogWithThrottling($"Completed {operationName} successfully", LogLevel.Info);
                return new OperationResult<T>(result);
            }
            catch (ArgumentNullException ex)
            {
                string errorMsg = $"Null argument in {operationName}: {ex.ParamName}";
                LogError(operationName, errorMsg, ex);
                if (showUserDialog) ShowErrorToUser("Invalid Input", errorMsg);
                return new OperationResult<T>(errorMsg) { Exception = ex };
            }
            catch (ArgumentException ex)
            {
                string errorMsg = $"Invalid argument in {operationName}: {ex.Message}";
                LogError(operationName, errorMsg, ex);
                if (showUserDialog) ShowErrorToUser("Invalid Input", errorMsg);
                return new OperationResult<T>(errorMsg) { Exception = ex };
            }
            catch (UnityException ex)
            {
                string errorMsg = $"Unity error in {operationName}: {ex.Message}";
                LogError(operationName, errorMsg, ex);
                if (showUserDialog) ShowErrorToUser("Unity Error", errorMsg);
                return new OperationResult<T>(errorMsg) { Exception = ex };
            }
            catch (System.IO.IOException ex)
            {
                string errorMsg = $"File I/O error in {operationName}: {ex.Message}";
                LogError(operationName, errorMsg, ex);
                if (showUserDialog) ShowErrorToUser("File Error", errorMsg);
                return new OperationResult<T>(errorMsg) { Exception = ex };
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error in {operationName}: {ex.Message}";
                LogError(operationName, errorMsg, ex);
                if (showUserDialog) ShowErrorToUser("Unexpected Error", errorMsg);
                return new OperationResult<T>(errorMsg) { Exception = ex };
            }
        }

        /// <summary>
        /// Validates system prerequisites and dependencies
        /// </summary>
        public static ValidationResult ValidateSystemState()
        {
            var issues = new List<string>();

            // Check Unity version compatibility
            if (!IsUnityVersionSupported())
            {
                issues.Add("Unity version may not be fully supported");
            }

            // Check AssetDatabase availability
            if (!IsAssetDatabaseAvailable())
            {
                issues.Add("AssetDatabase is not available");
            }

            // Check shader compilation state
            if (ShaderUtil.anythingCompiling)
            {
                issues.Add("Shaders are currently compiling - some operations may fail");
            }

            // Check for asset import in progress
            if (EditorApplication.isCompiling)
            {
                issues.Add("Scripts are compiling - operations may be unstable");
            }

            bool isValid = issues.Count == 0;
            string message = isValid ? "System state is healthy" : string.Join("; ", issues);

            return new ValidationResult(isValid, message);
        }

        /// <summary>
        /// Performs graceful degradation when operations partially fail
        /// </summary>
        public static void HandlePartialFailure(string operationName, List<string> failures, List<string> successes)
        {
            if (failures.Count == 0) return;

            string message = $"{operationName} completed with {failures.Count} failures out of {failures.Count + successes.Count} total operations";
            
            Debug.LogWarning($"[SystemIntegration] {message}");
            
            // Log individual failures
            foreach (var failure in failures)
            {
                Debug.LogWarning($"[SystemIntegration] Failure: {failure}");
            }

            // Show user notification for significant failures
            if (failures.Count > successes.Count)
            {
                EditorUtility.DisplayDialog(
                    "Operation Partially Failed", 
                    $"{message}\n\nCheck the console for detailed error information.", 
                    "OK"
                );
            }
        }

        /// <summary>
        /// Ensures assets are properly saved and refreshed after operations
        /// </summary>
        public static void EnsureAssetConsistency()
        {
            try
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SystemIntegration] Asset consistency ensured");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemIntegration] Failed to ensure asset consistency: {ex.Message}");
            }
        }

        /// <summary>
        /// Provides recovery suggestions for common error scenarios
        /// </summary>
        public static string GetRecoverySuggestion(Exception exception)
        {
            switch (exception)
            {
                case ArgumentNullException _:
                    return "Please ensure all required fields are filled out before proceeding.";
                
                case UnityException _ when exception.Message.Contains("asset"):
                    return "Try refreshing the Asset Database (Assets > Refresh) and try again.";
                
                case System.IO.IOException _:
                    return "Check that files are not locked by another application and you have write permissions.";
                
                case InvalidOperationException _ when exception.Message.Contains("shader"):
                    return "Wait for shader compilation to complete and try again.";
                
                default:
                    return "Try restarting Unity if the problem persists.";
            }
        }

        /// <summary>
        /// Logs with throttling to prevent spam
        /// </summary>
        private static void LogWithThrottling(string message, LogLevel level)
        {
            string key = $"{level}:{message}";
            DateTime now = DateTime.Now;

            if (_lastLogTimes.ContainsKey(key) && 
                now - _lastLogTimes[key] < _logThrottleInterval)
            {
                return; // Throttle this log message
            }

            _lastLogTimes[key] = now;

            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log($"[SystemIntegration] {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"[SystemIntegration] {message}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[SystemIntegration] {message}");
                    break;
            }
        }

        /// <summary>
        /// Logs errors with full context
        /// </summary>
        private static void LogError(string operationName, string message, Exception exception)
        {
            Debug.LogError($"[SystemIntegration] {operationName} failed: {message}");
            Debug.LogError($"[SystemIntegration] Exception details: {exception}");
            
            // Log stack trace for debugging
            if (exception.StackTrace != null)
            {
                Debug.LogError($"[SystemIntegration] Stack trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Shows error to user with recovery suggestions
        /// </summary>
        private static void ShowErrorToUser(string title, string message)
        {
            EditorApplication.delayCall += () =>
            {
                EditorUtility.DisplayDialog(
                    title,
                    $"{message}\n\nSuggestion: Check the Console window for more details.",
                    "OK"
                );
            };
        }

        /// <summary>
        /// Checks if Unity version is supported
        /// </summary>
        private static bool IsUnityVersionSupported()
        {
            // Support Unity 2019.4 and later
            return Application.unityVersion.CompareTo("2019.4") >= 0;
        }

        /// <summary>
        /// Checks if AssetDatabase is available
        /// </summary>
        private static bool IsAssetDatabaseAvailable()
        {
            try
            {
                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }



    /// <summary>
    /// Validation result for system state checks
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}