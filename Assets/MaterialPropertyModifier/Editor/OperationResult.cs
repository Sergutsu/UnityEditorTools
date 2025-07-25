using System;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Log levels for categorized logging
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Comprehensive operation result with detailed error information and recovery status
    /// </summary>
    /// <typeparam name="T">Type of the operation result data</typeparam>
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCategory { get; set; }
        public Exception Exception { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool WasRecovered { get; set; }
        public bool RecoveryAttempted { get; set; }
        public string RecoveryErrorMessage { get; set; }

        public OperationResult()
        {
            IsSuccess = false;
            WasRecovered = false;
            RecoveryAttempted = false;
        }

        public OperationResult(T data)
        {
            IsSuccess = true;
            Data = data;
            WasRecovered = false;
            RecoveryAttempted = false;
        }

        public OperationResult(string errorMessage, string errorCategory = "General")
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            ErrorCategory = errorCategory;
            WasRecovered = false;
            RecoveryAttempted = false;
        }
    }
}