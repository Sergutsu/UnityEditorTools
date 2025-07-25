using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Monitors and logs operations for debugging and performance analysis
    /// </summary>
    public class OperationMonitor
    {
        private static readonly Dictionary<string, Stopwatch> _activeOperations = new Dictionary<string, Stopwatch>();
        private static readonly List<OperationLog> _operationHistory = new List<OperationLog>();
        private const int MAX_HISTORY_SIZE = 100;

        /// <summary>
        /// Starts monitoring an operation
        /// </summary>
        public static void StartOperation(string operationName, string context = null)
        {
            try
            {
                string key = $"{operationName}_{context ?? "default"}";
                
                if (_activeOperations.ContainsKey(key))
                {
                    UnityEngine.Debug.LogWarning($"[OperationMonitor] Operation '{key}' is already being monitored");
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                _activeOperations[key] = stopwatch;
                
                UnityEngine.Debug.Log($"[OperationMonitor] Started monitoring: {key}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OperationMonitor] Failed to start monitoring '{operationName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Stops monitoring an operation and logs the result
        /// </summary>
        public static void EndOperation(string operationName, string context = null, bool success = true, string errorMessage = null)
        {
            try
            {
                string key = $"{operationName}_{context ?? "default"}";
                
                if (!_activeOperations.TryGetValue(key, out Stopwatch stopwatch))
                {
                    UnityEngine.Debug.LogWarning($"[OperationMonitor] No active monitoring found for: {key}");
                    return;
                }

                stopwatch.Stop();
                _activeOperations.Remove(key);

                var operationLog = new OperationLog
                {
                    OperationName = operationName,
                    Context = context,
                    Duration = stopwatch.Elapsed,
                    Success = success,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.Now
                };

                AddToHistory(operationLog);
                LogOperationResult(operationLog);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OperationMonitor] Failed to end monitoring '{operationName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Executes an operation with automatic monitoring
        /// </summary>
        public static T MonitorOperation<T>(string operationName, Func<T> operation, string context = null)
        {
            StartOperation(operationName, context);
            
            try
            {
                var result = operation();
                EndOperation(operationName, context, true);
                return result;
            }
            catch (Exception ex)
            {
                EndOperation(operationName, context, false, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets performance statistics for operations
        /// </summary>
        public static PerformanceStats GetPerformanceStats(string operationName = null)
        {
            var stats = new PerformanceStats();
            var relevantLogs = string.IsNullOrEmpty(operationName) 
                ? _operationHistory 
                : _operationHistory.FindAll(log => log.OperationName == operationName);

            if (relevantLogs.Count == 0)
            {
                return stats;
            }

            stats.TotalOperations = relevantLogs.Count;
            stats.SuccessfulOperations = relevantLogs.FindAll(log => log.Success).Count;
            stats.FailedOperations = stats.TotalOperations - stats.SuccessfulOperations;

            var durations = relevantLogs.ConvertAll(log => log.Duration.TotalMilliseconds);
            stats.AverageDuration = durations.Count > 0 ? durations.Average() : 0;
            stats.MinDuration = durations.Count > 0 ? durations.Min() : 0;
            stats.MaxDuration = durations.Count > 0 ? durations.Max() : 0;

            return stats;
        }

        /// <summary>
        /// Gets recent operation history
        /// </summary>
        public static List<OperationLog> GetRecentHistory(int count = 10)
        {
            int startIndex = Math.Max(0, _operationHistory.Count - count);
            return _operationHistory.GetRange(startIndex, _operationHistory.Count - startIndex);
        }

        /// <summary>
        /// Clears operation history
        /// </summary>
        public static void ClearHistory()
        {
            _operationHistory.Clear();
            UnityEngine.Debug.Log("[OperationMonitor] Operation history cleared");
        }

        /// <summary>
        /// Generates a performance report
        /// </summary>
        [MenuItem("Tools/Material Property Modifier/Performance Report")]
        public static void GeneratePerformanceReport()
        {
            try
            {
                var report = "=== Material Property Modifier Performance Report ===\n\n";
                
                var overallStats = GetPerformanceStats();
                report += $"Overall Statistics:\n";
                report += $"  Total Operations: {overallStats.TotalOperations}\n";
                report += $"  Successful: {overallStats.SuccessfulOperations}\n";
                report += $"  Failed: {overallStats.FailedOperations}\n";
                report += $"  Success Rate: {(overallStats.TotalOperations > 0 ? (overallStats.SuccessfulOperations * 100.0 / overallStats.TotalOperations):0):F1}%\n";
                report += $"  Average Duration: {overallStats.AverageDuration:F2}ms\n";
                report += $"  Min Duration: {overallStats.MinDuration:F2}ms\n";
                report += $"  Max Duration: {overallStats.MaxDuration:F2}ms\n\n";

                // Operation-specific stats
                var operationNames = new HashSet<string>();
                foreach (var log in _operationHistory)
                {
                    operationNames.Add(log.OperationName);
                }

                foreach (var opName in operationNames)
                {
                    var opStats = GetPerformanceStats(opName);
                    report += $"{opName}:\n";
                    report += $"  Operations: {opStats.TotalOperations}\n";
                    report += $"  Success Rate: {(opStats.TotalOperations > 0 ? (opStats.SuccessfulOperations * 100.0 / opStats.TotalOperations):0):F1}%\n";
                    report += $"  Avg Duration: {opStats.AverageDuration:F2}ms\n\n";
                }

                // Recent failures
                var recentFailures = _operationHistory.FindAll(log => !log.Success && 
                    DateTime.Now - log.Timestamp < TimeSpan.FromMinutes(30));
                
                if (recentFailures.Count > 0)
                {
                    report += "Recent Failures (last 30 minutes):\n";
                    foreach (var failure in recentFailures)
                    {
                        report += $"  {failure.Timestamp:HH:mm:ss} - {failure.OperationName}: {failure.ErrorMessage}\n";
                    }
                }

                UnityEngine.Debug.Log(report);
                EditorUtility.DisplayDialog("Performance Report", "Performance report generated. Check the Console for details.", "OK");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[OperationMonitor] Failed to generate performance report: {ex.Message}");
            }
        }

        private static void AddToHistory(OperationLog log)
        {
            _operationHistory.Add(log);
            
            // Maintain history size limit
            while (_operationHistory.Count > MAX_HISTORY_SIZE)
            {
                _operationHistory.RemoveAt(0);
            }
        }

        private static void LogOperationResult(OperationLog log)
        {
            string message = $"[OperationMonitor] {log.OperationName}";
            if (!string.IsNullOrEmpty(log.Context))
            {
                message += $" ({log.Context})";
            }
            message += $" - Duration: {log.Duration.TotalMilliseconds:F2}ms";

            if (log.Success)
            {
                UnityEngine.Debug.Log($"{message} - SUCCESS");
            }
            else
            {
                UnityEngine.Debug.LogError($"{message} - FAILED: {log.ErrorMessage}");
            }
        }
    }

    /// <summary>
    /// Represents a logged operation
    /// </summary>
    public class OperationLog
    {
        public string OperationName { get; set; }
        public string Context { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Performance statistics for operations
    /// </summary>
    public class PerformanceStats
    {
        public int TotalOperations { get; set; }
        public int SuccessfulOperations { get; set; }
        public int FailedOperations { get; set; }
        public double AverageDuration { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
    }

    /// <summary>
    /// Extension methods for easier monitoring
    /// </summary>
    public static class MonitoringExtensions
    {
        public static double Average(this List<double> values)
        {
            return values.Count > 0 ? values.Sum() / values.Count : 0;
        }

        public static double Sum(this List<double> values)
        {
            double sum = 0;
            foreach (var value in values)
            {
                sum += value;
            }
            return sum;
        }

        public static double Min(this List<double> values)
        {
            if (values.Count == 0) return 0;
            double min = values[0];
            foreach (var value in values)
            {
                if (value < min) min = value;
            }
            return min;
        }

        public static double Max(this List<double> values)
        {
            if (values.Count == 0) return 0;
            double max = values[0];
            foreach (var value in values)
            {
                if (value > max) max = value;
            }
            return max;
        }
    }
}