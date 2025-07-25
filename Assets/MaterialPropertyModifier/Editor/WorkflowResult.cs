using System.Collections.Generic;
using UnityEngine;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of the integrated workflow containing all operation results
    /// </summary>
    public class WorkflowResult
    {
        public bool PrerequisitesValid { get; set; }
        public List<Material> DiscoveredMaterials { get; set; }
        public int MaterialCount { get; set; }
        public ModificationPreview Preview { get; set; }
        public int MaterialsToModify { get; set; }
        public int MaterialsToSkip { get; set; }
        public bool ModificationsApplied { get; set; }
        public ModificationResult ModificationResult { get; set; }
        public int SuccessfulModifications { get; set; }
        public int FailedModifications { get; set; }

        public WorkflowResult()
        {
            DiscoveredMaterials = new List<Material>();
            PrerequisitesValid = false;
            ModificationsApplied = false;
        }
    }

    /// <summary>
    /// Result of batch operations with detailed success/failure tracking
    /// </summary>
    public class BatchOperationResult
    {
        public List<object> SuccessfulOperations { get; set; }
        public List<string> FailedOperations { get; set; }
        public int TotalOperations { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public bool WasCancelled { get; set; }

        public BatchOperationResult()
        {
            SuccessfulOperations = new List<object>();
            FailedOperations = new List<string>();
            WasCancelled = false;
        }

        public double SuccessRate => TotalOperations > 0 ? (double)SuccessCount / TotalOperations : 0.0;
    }

    /// <summary>
    /// Comprehensive health check result for system validation
    /// </summary>
    public class HealthCheckResult
    {
        public Dictionary<string, bool> CheckResults { get; set; }
        public List<string> FailedChecks { get; set; }
        public bool OverallHealth { get; set; }

        public HealthCheckResult()
        {
            CheckResults = new Dictionary<string, bool>();
            FailedChecks = new List<string>();
            OverallHealth = false;
        }

        public int PassedChecks => CheckResults.Count - FailedChecks.Count;
        public int TotalChecks => CheckResults.Count;
        public double HealthScore => TotalChecks > 0 ? (double)PassedChecks / TotalChecks : 0.0;
    }
}