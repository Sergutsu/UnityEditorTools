using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using ShaderPropertyType = UnityEditor.ShaderPropertyType;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Core logic class for material discovery, property validation, and modification operations
    /// </summary>
    public class MaterialPropertyModifier
    {
        /// <summary>
        /// Finds all materials in the specified folder path that use the target shader
        /// Scans the folder and all subfolders recursively using AssetDatabase.FindAssets
        /// </summary>
        /// <param name="folderPath">The folder path to search in (relative to Assets/)</param>
        /// <param name="targetShader">The shader to filter materials by</param>
        /// <returns>List of materials that use the target shader</returns>
        public List<Material> FindMaterialsWithShader(string folderPath, Shader targetShader)
        {
            return OperationMonitor.MonitorOperation("FindMaterialsWithShader", () =>
            {
                return SystemIntegration.ExecuteWithErrorHandling(() =>
                {
                var foundMaterials = new List<Material>();
                var failures = new List<string>();
                
                if (string.IsNullOrEmpty(folderPath) || targetShader == null)
                {
                    LogWithContext(LogLevel.Warning, "MaterialDiscovery", "Invalid parameters", 
                        $"FolderPath: {folderPath ?? "null"}, Shader: {targetShader?.name ?? "null"}");
                    return foundMaterials;
                }

                // Ensure the folder path starts with "Assets/"
                if (!folderPath.StartsWith("Assets/"))
                {
                    folderPath = "Assets/" + folderPath.TrimStart('/');
                }

                // Validate folder exists
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    LogWithContext(LogLevel.Warning, "MaterialDiscovery", "Invalid folder", folderPath);
                    return foundMaterials;
                }

                // Find all material assets in the specified folder and its subfolders recursively
                string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
                
                LogWithContext(LogLevel.Info, "MaterialDiscovery", "Starting scan", 
                    $"Folder: '{folderPath}', Shader: '{targetShader.name}', Total materials: {materialGuids.Length}");
                
                foreach (string guid in materialGuids)
                {
                    try
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                        
                        if (material != null && material.shader == targetShader)
                        {
                            foundMaterials.Add(material);
                            LogWithContext(LogLevel.Info, "MaterialDiscovery", "Found match", 
                                $"{material.name} at {assetPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = $"Failed to process material GUID {guid}: {ex.Message}";
                        failures.Add(error);
                        LogWithContext(LogLevel.Error, "MaterialDiscovery", "Material processing failed", error);
                    }
                }
                
                // Handle partial failures
                if (failures.Count > 0)
                {
                    SystemIntegration.HandlePartialFailure("Material Discovery", failures, 
                        foundMaterials.Select(m => m.name).ToList());
                }
                
                LogWithContext(LogLevel.Info, "MaterialDiscovery", "Scan complete", 
                    $"Found {foundMaterials.Count} materials using shader '{targetShader.name}'");
                
                return foundMaterials;
                }, 
                "Find Materials With Shader", 
                new List<Material>()).Data;
            }, $"Folder: {folderPath}, Shader: {targetShader?.name}");
        }

        /// <summary>
        /// Validates if a property exists on the given shader
        /// </summary>
        /// <param name="shader">The shader to check</param>
        /// <param name="propertyName">The property name to validate</param>
        /// <returns>PropertyValidationResult containing validation status and property information</returns>
        public PropertyValidationResult ValidateProperty(Shader shader, string propertyName)
        {
            if (shader == null || string.IsNullOrEmpty(propertyName))
            {
                return new PropertyValidationResult(false, "Shader or property name is null/empty");
            }

            try
            {
                int propertyCount = shader.GetPropertyCount();
                for (int i = 0; i < propertyCount; i++)
                {
                    if (shader.GetPropertyName(i) == propertyName)
                    {
                        var propertyType = shader.GetPropertyType(i);
                        return new PropertyValidationResult(true, "", propertyType);
                    }
                }
                
                return new PropertyValidationResult(false, $"Property '{propertyName}' not found on shader '{shader.name}'");
            }
            catch (System.Exception ex)
            {
                return new PropertyValidationResult(false, $"Error validating property: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the property type for a specific property on the shader
        /// </summary>
        /// <param name="shader">The shader to check</param>
        /// <param name="propertyName">The property name to get the type for</param>
        /// <returns>The ShaderPropertyType, or Float if not found</returns>
        public ShaderPropertyType GetPropertyType(Shader shader, string propertyName)
        {
            if (shader == null || string.IsNullOrEmpty(propertyName))
            {
                return ShaderPropertyType.Float;
            }

            try
            {
                int propertyCount = shader.GetPropertyCount();
                for (int i = 0; i < propertyCount; i++)
                {
                    if (shader.GetPropertyName(i) == propertyName)
                    {
                        return shader.GetPropertyType(i);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error getting property type for '{propertyName}' on shader '{shader.name}': {ex.Message}");
            }

            return ShaderPropertyType.Float;
        }

        /// <summary>
        /// Gets the asset path for a material, useful for displaying file paths in UI
        /// </summary>
        /// <param name="material">The material to get the path for</param>
        /// <returns>The asset path of the material, or empty string if not found</returns>
        public string GetMaterialAssetPath(Material material)
        {
            if (material == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(material);
        }

        /// <summary>
        /// Gets asset paths for a list of materials
        /// </summary>
        /// <param name="materials">List of materials to get paths for</param>
        /// <returns>Dictionary mapping materials to their asset paths</returns>
        public Dictionary<Material, string> GetMaterialAssetPaths(List<Material> materials)
        {
            var pathDictionary = new Dictionary<Material, string>();
            
            if (materials == null)
            {
                return pathDictionary;
            }

            foreach (var material in materials)
            {
                if (material != null)
                {
                    pathDictionary[material] = AssetDatabase.GetAssetPath(material);
                }
            }

            return pathDictionary;
        }

        /// <summary>
        /// Validates that a target value is appropriate for the given property type
        /// </summary>
        /// <param name="propertyType">The shader property type</param>
        /// <param name="targetValue">The value to validate</param>
        /// <returns>PropertyValidationResult indicating if the value is valid for the property type</returns>
        public PropertyValidationResult ValidatePropertyValue(ShaderPropertyType propertyType, object targetValue)
        {
            if (targetValue == null)
            {
                return new PropertyValidationResult(false, "Target value cannot be null");
            }

            try
            {
                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        if (targetValue is float || targetValue is int || targetValue is double)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        if (float.TryParse(targetValue.ToString(), out _))
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        return new PropertyValidationResult(false, $"Value '{targetValue}' is not a valid float for property type {propertyType}");

                    case ShaderPropertyType.Int:
                        if (targetValue is int)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        if (int.TryParse(targetValue.ToString(), out _))
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        return new PropertyValidationResult(false, $"Value '{targetValue}' is not a valid integer for property type {propertyType}");

                    case ShaderPropertyType.Color:
                        if (targetValue is Color)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        if (targetValue is Vector4)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        return new PropertyValidationResult(false, $"Value '{targetValue}' is not a valid Color or Vector4 for property type {propertyType}");

                    case ShaderPropertyType.Vector:
                        if (targetValue is Vector4 || targetValue is Vector3 || targetValue is Vector2)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        return new PropertyValidationResult(false, $"Value '{targetValue}' is not a valid Vector for property type {propertyType}");

                    case ShaderPropertyType.Texture:
                        if (targetValue == null) // Null textures are valid (removes texture)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        if (targetValue is Texture || targetValue is Texture2D || targetValue is Texture3D || targetValue is Cubemap)
                        {
                            return new PropertyValidationResult(true, "", propertyType);
                        }
                        return new PropertyValidationResult(false, $"Value '{targetValue}' is not a valid Texture for property type {propertyType}");

                    default:
                        return new PropertyValidationResult(false, $"Unknown property type: {propertyType}");
                }
            }
            catch (System.Exception ex)
            {
                return new PropertyValidationResult(false, $"Error validating property value: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates both property existence and value compatibility
        /// </summary>
        /// <param name="shader">The shader to check</param>
        /// <param name="propertyName">The property name to validate</param>
        /// <param name="targetValue">The value to validate</param>
        /// <returns>PropertyValidationResult indicating if both property and value are valid</returns>
        public PropertyValidationResult ValidatePropertyAndValue(Shader shader, string propertyName, object targetValue)
        {
            // First validate the property exists
            var propertyValidation = ValidateProperty(shader, propertyName);
            if (!propertyValidation.IsValid)
            {
                return propertyValidation;
            }

            // Then validate the value is appropriate for the property type
            var valueValidation = ValidatePropertyValue(propertyValidation.PropertyType, targetValue);
            if (!valueValidation.IsValid)
            {
                return valueValidation;
            }

            return new PropertyValidationResult(true, "", propertyValidation.PropertyType);
        }

        /// <summary>
        /// Comprehensive error handling wrapper for all operations
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="defaultResult">Default result to return on error</param>
        /// <returns>Operation result or default on error</returns>
        private T ExecuteWithErrorHandling<T>(Func<T> operation, string operationName, T defaultResult = default(T))
        {
            try
            {
                return operation();
            }
            catch (ArgumentNullException ex)
            {
                Debug.LogError($"[MaterialPropertyModifier] Null argument in {operationName}: {ex.Message}");
                return defaultResult;
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"[MaterialPropertyModifier] Invalid argument in {operationName}: {ex.Message}");
                return defaultResult;
            }
            catch (UnityException ex)
            {
                Debug.LogError($"[MaterialPropertyModifier] Unity error in {operationName}: {ex.Message}");
                return defaultResult;
            }
            catch (System.IO.IOException ex)
            {
                Debug.LogError($"[MaterialPropertyModifier] File I/O error in {operationName}: {ex.Message}");
                return defaultResult;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifier] Unexpected error in {operationName}: {ex.Message}\nStack trace: {ex.StackTrace}");
                return defaultResult;
            }
        }

        /// <summary>
        /// Validate system prerequisites before operations
        /// </summary>
        /// <returns>Validation result with detailed error information</returns>
        public PropertyValidationResult ValidateSystemPrerequisites()
        {
            return SystemIntegration.ExecuteWithErrorHandling(() =>
            {
                // Check Unity Editor state
                if (!Application.isEditor)
                {
                    return new PropertyValidationResult(false, "Material Property Modifier can only run in Unity Editor");
                }

                // Use comprehensive system state validation
                var systemState = SystemIntegration.ValidateSystemState();
                if (!systemState.IsValid)
                {
                    return new PropertyValidationResult(false, $"System state validation failed: {systemState.Message}");
                }

                // Check AssetDatabase availability
                if (!AssetDatabase.IsValidFolder("Assets"))
                {
                    return new PropertyValidationResult(false, "AssetDatabase is not available or Assets folder is invalid");
                }

                // Check if we can access shader system
                var testShader = Shader.Find("Standard");
                if (testShader == null)
                {
                    return new PropertyValidationResult(false, "Cannot access Unity shader system - Standard shader not found");
                }

                return new PropertyValidationResult(true, "System prerequisites validated successfully");
            }, 
            "System Prerequisites Validation", 
            new PropertyValidationResult(false, "System validation failed due to unexpected error")).Data;
        }

        /// <summary>
        /// Comprehensive validation of all operation prerequisites
        /// </summary>
        /// <param name="folderPath">Target folder path</param>
        /// <param name="shader">Target shader</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="propertyValue">Property value</param>
        /// <returns>Validation result with detailed error information</returns>
        public PropertyValidationResult ValidateOperationPrerequisites(string folderPath, Shader shader, string propertyName, object propertyValue)
        {
            return ExecuteWithErrorHandling(() =>
            {
                // System validation
                var systemValidation = ValidateSystemPrerequisites();
                if (!systemValidation.IsValid)
                {
                    return systemValidation;
                }

                // Folder validation
                if (string.IsNullOrEmpty(folderPath))
                {
                    return new PropertyValidationResult(false, "Folder path cannot be empty");
                }

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    return new PropertyValidationResult(false, $"Invalid folder path: {folderPath}");
                }

                // Shader validation
                if (shader == null)
                {
                    return new PropertyValidationResult(false, "Shader cannot be null");
                }

                // Property validation
                var propertyValidation = ValidatePropertyAndValue(shader, propertyName, propertyValue);
                if (!propertyValidation.IsValid)
                {
                    return propertyValidation;
                }

                return new PropertyValidationResult(true, "All operation prerequisites validated successfully");
            }, "ValidateOperationPrerequisites", new PropertyValidationResult(false, "Validation failed due to unexpected error"));
        }

        /// <summary>
        /// Enhanced logging with categorization and detailed context
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="category">Log category</param>
        /// <param name="message">Log message</param>
        /// <param name="context">Additional context object</param>
        public void LogWithContext(LogLevel level, string category, string message, object context = null)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            string contextInfo = context != null ? $" | Context: {context}" : "";
            string logMessage = $"[{timestamp}] [{category}] {message}{contextInfo}";

            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log($"[MaterialPropertyModifier] {logMessage}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"[MaterialPropertyModifier] {logMessage}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[MaterialPropertyModifier] {logMessage}");
                    break;
            }
        }

        /// <summary>
        /// Graceful degradation handler for partial operation failures
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="fallbackOperation">Fallback operation if main operation fails</param>
        /// <param name="operationName">Name for logging</param>
        /// <returns>Operation result</returns>
        public T ExecuteWithGracefulDegradation<T>(Func<T> operation, Func<T> fallbackOperation, string operationName)
        {
            try
            {
                LogWithContext(LogLevel.Info, "Operation", $"Starting {operationName}");
                var result = operation();
                LogWithContext(LogLevel.Info, "Operation", $"Successfully completed {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                LogWithContext(LogLevel.Warning, "Operation", $"Primary operation {operationName} failed, attempting fallback", ex.Message);
                
                try
                {
                    var fallbackResult = fallbackOperation();
                    LogWithContext(LogLevel.Info, "Operation", $"Fallback operation for {operationName} succeeded");
                    return fallbackResult;
                }
                catch (Exception fallbackEx)
                {
                    LogWithContext(LogLevel.Error, "Operation", $"Both primary and fallback operations failed for {operationName}", $"Primary: {ex.Message}, Fallback: {fallbackEx.Message}");
                    throw new AggregateException($"Both primary and fallback operations failed for {operationName}", ex, fallbackEx);
                }
            }
        }

        /// <summary>
        /// Comprehensive operation wrapper with full error handling, logging, and recovery
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="operationName">Operation name for logging</param>
        /// <param name="enableRecovery">Whether to attempt recovery on failure</param>
        /// <returns>Operation result with comprehensive error information</returns>
        public OperationResult<T> ExecuteComprehensiveOperation<T>(Func<T> operation, string operationName, bool enableRecovery = true)
        {
            var result = new OperationResult<T>();
            var startTime = System.DateTime.Now;
            
            try
            {
                LogWithContext(LogLevel.Info, "ComprehensiveOperation", $"Starting {operationName}");
                
                // Pre-operation validation
                var systemValidation = ValidateSystemPrerequisites();
                if (!systemValidation.IsValid)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"System validation failed: {systemValidation.ErrorMessage}";
                    result.ErrorCategory = "SystemValidation";
                    return result;
                }

                // Execute operation
                result.Data = operation();
                result.IsSuccess = true;
                result.ExecutionTime = System.DateTime.Now - startTime;
                
                LogWithContext(LogLevel.Info, "ComprehensiveOperation", $"Successfully completed {operationName} in {result.ExecutionTime.TotalMilliseconds:F2}ms");
            }
            catch (ArgumentException ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.ErrorCategory = "InvalidArgument";
                result.Exception = ex;
                LogWithContext(LogLevel.Error, "ComprehensiveOperation", $"Argument error in {operationName}", ex.Message);
            }
            catch (UnityException ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.ErrorCategory = "UnityEngine";
                result.Exception = ex;
                LogWithContext(LogLevel.Error, "ComprehensiveOperation", $"Unity error in {operationName}", ex.Message);
            }
            catch (System.IO.IOException ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.ErrorCategory = "FileIO";
                result.Exception = ex;
                LogWithContext(LogLevel.Error, "ComprehensiveOperation", $"File I/O error in {operationName}", ex.Message);
                
                // Attempt recovery for file I/O errors if enabled
                if (enableRecovery)
                {
                    try
                    {
                        LogWithContext(LogLevel.Info, "Recovery", $"Attempting recovery for {operationName}");
                        AssetDatabase.Refresh();
                        System.Threading.Thread.Sleep(100); // Brief pause
                        result.Data = operation();
                        result.IsSuccess = true;
                        result.WasRecovered = true;
                        LogWithContext(LogLevel.Info, "Recovery", $"Successfully recovered {operationName}");
                    }
                    catch (Exception recoveryEx)
                    {
                        LogWithContext(LogLevel.Error, "Recovery", $"Recovery failed for {operationName}", recoveryEx.Message);
                        result.RecoveryAttempted = true;
                        result.RecoveryErrorMessage = recoveryEx.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.ErrorCategory = "Unexpected";
                result.Exception = ex;
                result.ExecutionTime = System.DateTime.Now - startTime;
                LogWithContext(LogLevel.Error, "ComprehensiveOperation", $"Unexpected error in {operationName}", $"{ex.Message}\n{ex.StackTrace}");
            }
            
            result.ExecutionTime = System.DateTime.Now - startTime;
            return result;
        }

        /// <summary>
        /// Integrated workflow method that combines discovery, validation, preview, and modification with comprehensive error handling
        /// </summary>
        /// <param name="folderPath">Target folder path</param>
        /// <param name="shader">Target shader</param>
        /// <param name="propertyName">Property name to modify</param>
        /// <param name="propertyValue">Target property value</param>
        /// <param name="previewOnly">If true, only generates preview without applying changes</param>
        /// <returns>Comprehensive workflow result</returns>
        public OperationResult<WorkflowResult> ExecuteIntegratedWorkflow(string folderPath, Shader shader, string propertyName, object propertyValue, bool previewOnly = false)
        {
            return ExecuteComprehensiveOperation(() =>
            {
                var workflowResult = new WorkflowResult();
                
                // Step 1: Validate prerequisites
                LogWithContext(LogLevel.Info, "Workflow", "Step 1: Validating prerequisites");
                var prerequisiteValidation = ValidateOperationPrerequisites(folderPath, shader, propertyName, propertyValue);
                if (!prerequisiteValidation.IsValid)
                {
                    throw new ArgumentException($"Prerequisites validation failed: {prerequisiteValidation.ErrorMessage}");
                }
                workflowResult.PrerequisitesValid = true;

                // Step 2: Discover materials
                LogWithContext(LogLevel.Info, "Workflow", "Step 2: Discovering materials");
                var discoveryResult = FindMaterialsWithShaderEnhanced(folderPath, shader);
                if (!discoveryResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Material discovery failed: {discoveryResult.ErrorMessage}");
                }
                workflowResult.DiscoveredMaterials = discoveryResult.Materials;
                workflowResult.MaterialCount = discoveryResult.Materials.Count;

                // Step 3: Generate preview
                LogWithContext(LogLevel.Info, "Workflow", "Step 3: Generating modification preview");
                var modificationData = new MaterialModificationData
                {
                    Materials = discoveryResult.Materials,
                    PropertyName = propertyName,
                    TargetValue = propertyValue,
                    Shader = shader
                };

                var previewResult = PreviewModificationsEnhanced(modificationData);
                if (!previewResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Preview generation failed: {previewResult.ErrorMessage}");
                }
                workflowResult.Preview = previewResult.Preview;
                workflowResult.MaterialsToModify = previewResult.Preview.MaterialsToModify.Count;
                workflowResult.MaterialsToSkip = previewResult.Preview.MaterialsToSkip.Count;

                // Step 4: Apply modifications (if not preview-only)
                if (!previewOnly)
                {
                    LogWithContext(LogLevel.Info, "Workflow", "Step 4: Applying modifications");
                    var modificationResult = ApplyModificationsEnhanced(modificationData);
                    if (!modificationResult.IsSuccess)
                    {
                        throw new InvalidOperationException($"Modification application failed: {modificationResult.ErrorMessage}");
                    }
                    workflowResult.ModificationResult = modificationResult.Result;
                    workflowResult.ModificationsApplied = true;
                    workflowResult.SuccessfulModifications = modificationResult.Result.SuccessfulModifications.Count;
                    workflowResult.FailedModifications = modificationResult.Result.FailedModifications.Count;
                }

                LogWithContext(LogLevel.Info, "Workflow", $"Integrated workflow completed successfully. Materials: {workflowResult.MaterialCount}, To Modify: {workflowResult.MaterialsToModify}, Applied: {workflowResult.ModificationsApplied}");
                return workflowResult;
            }, "IntegratedWorkflow");
        }

        /// <summary>
        /// Batch operation with progress tracking and cancellation support
        /// </summary>
        /// <param name="operations">List of operations to execute</param>
        /// <param name="progressCallback">Progress callback (current, total, description)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Batch operation result</returns>
        public OperationResult<BatchOperationResult> ExecuteBatchOperations(
            List<Func<OperationResult<object>>> operations, 
            Action<int, int, string> progressCallback = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return ExecuteComprehensiveOperation(() =>
            {
                var batchResult = new BatchOperationResult();
                var successfulOperations = new List<object>();
                var failedOperations = new List<string>();

                for (int i = 0; i < operations.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        LogWithContext(LogLevel.Warning, "BatchOperation", $"Batch operation cancelled at step {i + 1}/{operations.Count}");
                        batchResult.WasCancelled = true;
                        break;
                    }

                    progressCallback?.Invoke(i + 1, operations.Count, $"Executing operation {i + 1}");

                    try
                    {
                        var operationResult = operations[i]();
                        if (operationResult.IsSuccess)
                        {
                            successfulOperations.Add(operationResult.Data);
                        }
                        else
                        {
                            failedOperations.Add(operationResult.ErrorMessage);
                            LogWithContext(LogLevel.Warning, "BatchOperation", $"Operation {i + 1} failed", operationResult.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedOperations.Add(ex.Message);
                        LogWithContext(LogLevel.Error, "BatchOperation", $"Operation {i + 1} threw exception", ex.Message);
                    }
                }

                batchResult.SuccessfulOperations = successfulOperations;
                batchResult.FailedOperations = failedOperations;
                batchResult.TotalOperations = operations.Count;
                batchResult.SuccessCount = successfulOperations.Count;
                batchResult.FailureCount = failedOperations.Count;

                LogWithContext(LogLevel.Info, "BatchOperation", $"Batch operation completed. Success: {batchResult.SuccessCount}, Failed: {batchResult.FailureCount}, Total: {batchResult.TotalOperations}");
                return batchResult;
            }, "BatchOperations");
        }

        /// <summary>
        /// Health check method to validate all system components
        /// </summary>
        /// <returns>Comprehensive health check result</returns>
        public OperationResult<HealthCheckResult> PerformHealthCheck()
        {
            return ExecuteComprehensiveOperation(() =>
            {
                var healthResult = new HealthCheckResult();
                var checks = new List<(string Name, Func<bool> Check, string ErrorMessage)>
                {
                    ("Unity Editor", () => Application.isEditor, "Not running in Unity Editor"),
                    ("AssetDatabase", () => AssetDatabase.IsValidFolder("Assets"), "AssetDatabase not available"),
                    ("Standard Shader", () => Shader.Find("Standard") != null, "Standard shader not found"),
                    ("Shader Property Access", () => {
                        var shader = Shader.Find("Standard");
                        return shader != null && shader.GetPropertyCount() > 0;
                    }, "Cannot access shader properties"),
                    ("Material Creation", () => {
                        try
                        {
                            var testMaterial = new Material(Shader.Find("Standard"));
                            Object.DestroyImmediate(testMaterial);
                            return true;
                        }
                        catch { return false; }
                    }, "Cannot create materials"),
                };

                foreach (var (name, check, errorMessage) in checks)
                {
                    try
                    {
                        bool passed = check();
                        healthResult.CheckResults[name] = passed;
                        if (!passed)
                        {
                            healthResult.FailedChecks.Add($"{name}: {errorMessage}");
                        }
                        LogWithContext(LogLevel.Info, "HealthCheck", $"{name}: {(passed ? "PASS" : "FAIL")}");
                    }
                    catch (Exception ex)
                    {
                        healthResult.CheckResults[name] = false;
                        healthResult.FailedChecks.Add($"{name}: Exception - {ex.Message}");
                        LogWithContext(LogLevel.Error, "HealthCheck", $"{name}: EXCEPTION", ex.Message);
                    }
                }

                healthResult.OverallHealth = healthResult.FailedChecks.Count == 0;
                LogWithContext(LogLevel.Info, "HealthCheck", $"Health check completed. Overall: {(healthResult.OverallHealth ? "HEALTHY" : "UNHEALTHY")}, Failed checks: {healthResult.FailedChecks.Count}");
                
                return healthResult;
            }, "HealthCheck");
        }

        /// <summary>
        /// Enhanced material discovery with comprehensive error handling
        /// </summary>
        /// <param name="folderPath">The folder path to search in</param>
        /// <param name="targetShader">The shader to filter materials by</param>
        /// <returns>List of materials with detailed error information</returns>
        public MaterialDiscoveryResult FindMaterialsWithShaderEnhanced(string folderPath, Shader targetShader)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = new MaterialDiscoveryResult();
                
                // Validate inputs
                if (string.IsNullOrEmpty(folderPath))
                {
                    result.ErrorMessage = "Folder path cannot be null or empty";
                    return result;
                }

                if (targetShader == null)
                {
                    result.ErrorMessage = "Target shader cannot be null";
                    return result;
                }

                // Validate system prerequisites
                var systemValidation = ValidateSystemPrerequisites();
                if (!systemValidation.IsValid)
                {
                    result.ErrorMessage = systemValidation.ErrorMessage;
                    return result;
                }

                // Normalize folder path
                if (!folderPath.StartsWith("Assets/"))
                {
                    folderPath = "Assets/" + folderPath.TrimStart('/');
                }

                // Validate folder exists
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    result.ErrorMessage = $"Folder '{folderPath}' does not exist or is not valid";
                    return result;
                }

                // Perform material discovery
                result.Materials = FindMaterialsWithShader(folderPath, targetShader);
                result.IsSuccess = true;
                result.FolderPath = folderPath;
                result.TargetShader = targetShader;
                
                Debug.Log($"[MaterialPropertyModifier] Successfully found {result.Materials.Count} materials in '{folderPath}' using shader '{targetShader.name}'");
                
                return result;
            }, 
            "Enhanced Material Discovery", 
            new MaterialDiscoveryResult { ErrorMessage = "Material discovery failed due to unexpected error" });
        }

        /// <summary>
        /// Enhanced modification preview with comprehensive error handling
        /// </summary>
        /// <param name="data">Material modification data</param>
        /// <returns>Preview result with detailed error information</returns>
        public ModificationPreviewResult PreviewModificationsEnhanced(MaterialModificationData data)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = new ModificationPreviewResult();
                
                // Validate input data
                var dataValidation = ValidateModificationData(data);
                if (!dataValidation.IsValid)
                {
                    result.ErrorMessage = dataValidation.ErrorMessage;
                    return result;
                }

                // Generate preview using existing method
                var preview = PreviewModifications(data);
                if (preview == null)
                {
                    result.ErrorMessage = "Failed to generate modification preview";
                    return result;
                }

                result.Preview = preview;
                result.IsSuccess = true;
                
                Debug.Log($"[MaterialPropertyModifier] Successfully generated preview: {preview.MaterialsToModify.Count} to modify, {preview.MaterialsToSkip.Count} to skip");
                
                return result;
            },
            "Enhanced Preview Generation",
            new ModificationPreviewResult { ErrorMessage = "Preview generation failed due to unexpected error" });
        }

        /// <summary>
        /// Enhanced material modification with comprehensive error handling
        /// </summary>
        /// <param name="data">Material modification data</param>
        /// <returns>Modification result with detailed error information</returns>
        public MaterialModificationResult ApplyModificationsEnhanced(MaterialModificationData data)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = new MaterialModificationResult();
                
                // Validate input data
                var dataValidation = ValidateModificationData(data);
                if (!dataValidation.IsValid)
                {
                    result.ErrorMessage = dataValidation.ErrorMessage;
                    return result;
                }

                // Apply modifications using existing method
                var modificationResult = ApplyModifications(data);
                if (modificationResult == null)
                {
                    result.ErrorMessage = "Failed to apply modifications";
                    return result;
                }

                result.ModificationResult = modificationResult;
                result.IsSuccess = true;
                
                Debug.Log($"[MaterialPropertyModifier] Successfully applied modifications");
                
                return result;
            },
            "Enhanced Material Modification",
            new MaterialModificationResult { ErrorMessage = "Material modification failed due to unexpected error" });
        }

        /// <summary>
        /// Validate material modification data
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>Validation result</returns>
        private PropertyValidationResult ValidateModificationData(MaterialModificationData data)
        {
            if (data == null)
            {
                return new PropertyValidationResult(false, "Material modification data cannot be null");
            }

            if (string.IsNullOrEmpty(data.TargetFolder))
            {
                return new PropertyValidationResult(false, "Target folder cannot be null or empty");
            }

            if (data.TargetShader == null)
            {
                return new PropertyValidationResult(false, "Target shader cannot be null");
            }

            if (string.IsNullOrEmpty(data.PropertyName))
            {
                return new PropertyValidationResult(false, "Property name cannot be null or empty");
            }

            if (data.PropertyValue == null)
            {
                return new PropertyValidationResult(false, "Property value cannot be null");
            }

            // Validate property exists on shader
            var propertyValidation = ValidateProperty(data.TargetShader, data.PropertyName);
            if (!propertyValidation.IsValid)
            {
                return propertyValidation;
            }

            // Validate property value compatibility
            var valueValidation = ValidatePropertyValue(propertyValidation.PropertyType, data.PropertyValue);
            if (!valueValidation.IsValid)
            {
                return valueValidation;
            }

            return new PropertyValidationResult(true, "Material modification data is valid");
        }

        /// <summary>
        /// Generates a preview of modifications that would be applied to the given materials
        /// </summary>
        /// <param name="materials">List of materials to preview modifications for</param>
        /// <param name="propertyName">The property name to modify</param>
        /// <param name="targetValue">The target value to set</param>
        /// <returns>ModificationPreview containing details of what would be modified</returns>
        public ModificationPreview PreviewModifications(List<Material> materials, string propertyName, object targetValue)
        {
            var preview = new ModificationPreview();
            
            if (materials == null || materials.Count == 0)
            {
                return preview;
            }

            preview.TotalCount = materials.Count;

            foreach (var material in materials)
            {
                if (material == null)
                {
                    preview.SkippedMaterials.Add("Null material reference");
                    continue;
                }

                try
                {
                    string materialPath = GetMaterialAssetPath(material);
                    
                    // Validate property exists on this material's shader
                    var propertyValidation = ValidateProperty(material.shader, propertyName);
                    if (!propertyValidation.IsValid)
                    {
                        preview.SkippedMaterials.Add($"{material.name} ({materialPath}): {propertyValidation.ErrorMessage}");
                        continue;
                    }

                    // Get current value
                    object currentValue = GetMaterialPropertyValue(material, propertyName, propertyValidation.PropertyType);
                    
                    // Check if values are different (only modify if different)
                    bool willModify = !AreValuesEqual(currentValue, targetValue, propertyValidation.PropertyType);
                    
                    var modification = new MaterialModification(
                        material, 
                        currentValue, 
                        targetValue, 
                        willModify, 
                        materialPath, 
                        propertyValidation.PropertyType
                    );
                    
                    preview.Modifications.Add(modification);
                }
                catch (System.Exception ex)
                {
                    string materialPath = GetMaterialAssetPath(material);
                    preview.SkippedMaterials.Add($"{material.name} ({materialPath}): Error - {ex.Message}");
                }
            }

            return preview;
        }

        /// <summary>
        /// Gets the current value of a property from a material
        /// </summary>
        /// <param name="material">The material to get the value from</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="propertyType">The property type</param>
        /// <returns>The current property value</returns>
        private object GetMaterialPropertyValue(Material material, string propertyName, ShaderPropertyType propertyType)
        {
            if (material == null || !material.HasProperty(propertyName))
            {
                return null;
            }

            try
            {
                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        return material.GetFloat(propertyName);
                    
                    case ShaderPropertyType.Int:
                        return material.GetInt(propertyName);
                    
                    case ShaderPropertyType.Color:
                        return material.GetColor(propertyName);
                    
                    case ShaderPropertyType.Vector:
                        return material.GetVector(propertyName);
                    
                    case ShaderPropertyType.Texture:
                        return material.GetTexture(propertyName);
                    
                    default:
                        return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error getting property value for '{propertyName}' on material '{material.name}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Compares two values to determine if they are equal for the given property type
        /// </summary>
        /// <param name="currentValue">The current value</param>
        /// <param name="targetValue">The target value</param>
        /// <param name="propertyType">The property type</param>
        /// <returns>True if values are considered equal</returns>
        private bool AreValuesEqual(object currentValue, object targetValue, ShaderPropertyType propertyType)
        {
            if (currentValue == null && targetValue == null)
                return true;
            
            if (currentValue == null || targetValue == null)
                return false;

            try
            {
                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        float currentFloat = System.Convert.ToSingle(currentValue);
                        float targetFloat = System.Convert.ToSingle(targetValue);
                        return Mathf.Approximately(currentFloat, targetFloat);
                    
                    case ShaderPropertyType.Int:
                        int currentInt = System.Convert.ToInt32(currentValue);
                        int targetInt = System.Convert.ToInt32(targetValue);
                        return currentInt == targetInt;
                    
                    case ShaderPropertyType.Color:
                        Color currentColor = (Color)currentValue;
                        Color targetColor;
                        if (targetValue is Vector4 vec4)
                            targetColor = new Color(vec4.x, vec4.y, vec4.z, vec4.w);
                        else
                            targetColor = (Color)targetValue;
                        return currentColor == targetColor;
                    
                    case ShaderPropertyType.Vector:
                        Vector4 currentVector = (Vector4)currentValue;
                        Vector4 targetVector;
                        if (targetValue is Vector3 vec3)
                            targetVector = new Vector4(vec3.x, vec3.y, vec3.z, 0);
                        else if (targetValue is Vector2 vec2)
                            targetVector = new Vector4(vec2.x, vec2.y, 0, 0);
                        else
                            targetVector = (Vector4)targetValue;
                        return currentVector == targetVector;
                    
                    case ShaderPropertyType.Texture:
                        return currentValue == targetValue; // Reference equality for textures
                    
                    default:
                        return currentValue.Equals(targetValue);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error comparing values: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Applies modifications to materials based on the provided preview
        /// </summary>
        /// <param name="preview">The modification preview containing materials to modify</param>
        /// <param name="propertyName">The property name to modify</param>
        /// <param name="targetValue">The target value to set</param>
        /// <returns>ModificationResult containing the results of the operation</returns>
        public ModificationResult ApplyModifications(ModificationPreview preview, string propertyName, object targetValue)
        {
            return SystemIntegration.ExecuteWithErrorHandling(() =>
            {
                var result = new ModificationResult();
                var failures = new List<string>();
                var successes = new List<string>();
                
                if (preview == null || preview.Modifications == null)
                {
                    result.ErrorMessages.Add("Invalid preview data provided");
                    return result;
                }

                result.TotalProcessed = preview.Modifications.Count;
                result.SkippedMaterials = preview.SkippedCount;

                LogWithContext(LogLevel.Info, "ApplyModifications", "Starting modifications", 
                    $"Property: {propertyName}, Total: {result.TotalProcessed}");

                // Record undo for all materials that will be modified
                var materialsToModify = new List<Material>();
                foreach (var modification in preview.Modifications)
                {
                    if (modification.WillBeModified && modification.TargetMaterial != null)
                    {
                        materialsToModify.Add(modification.TargetMaterial);
                    }
                }

                if (materialsToModify.Count > 0)
                {
                    try
                    {
                        Undo.RecordObjects(materialsToModify.ToArray(), $"Modify {propertyName} property");
                        LogWithContext(LogLevel.Info, "ApplyModifications", "Undo recorded", 
                            $"Materials: {materialsToModify.Count}");
                    }
                    catch (Exception ex)
                    {
                        LogWithContext(LogLevel.Warning, "ApplyModifications", "Undo recording failed", ex.Message);
                    }
                }

                // Apply modifications
                foreach (var modification in preview.Modifications)
                {
                    try
                    {
                        if (!modification.WillBeModified)
                        {
                            result.SuccessMessages.Add($"Skipped {modification.TargetMaterial.name} (value unchanged)");
                            continue;
                        }

                        if (modification.TargetMaterial == null)
                        {
                            result.FailedModifications++;
                            string error = "Null material reference";
                            result.ErrorMessages.Add(error);
                            failures.Add(error);
                            continue;
                        }

                        bool success = SetMaterialProperty(modification.TargetMaterial, propertyName, targetValue, modification.PropertyType);
                        
                        if (success)
                        {
                            // Mark material as dirty for saving
                            EditorUtility.SetDirty(modification.TargetMaterial);
                            result.SuccessfulModifications++;
                            string successMsg = $"Modified {modification.TargetMaterial.name}: {modification.CurrentValue} → {modification.TargetValue}";
                            result.SuccessMessages.Add(successMsg);
                            successes.Add(modification.TargetMaterial.name);
                        }
                        else
                        {
                            result.FailedModifications++;
                            string error = $"Failed to set property on {modification.TargetMaterial.name}";
                            result.ErrorMessages.Add(error);
                            failures.Add(error);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedModifications++;
                        string materialName = modification.TargetMaterial?.name ?? "Unknown";
                        string error = $"Error modifying {materialName}: {ex.Message}";
                        result.ErrorMessages.Add(error);
                        failures.Add(error);
                        LogWithContext(LogLevel.Error, "ApplyModifications", "Material modification failed", error);
                    }
                }

                // Handle partial failures
                if (failures.Count > 0)
                {
                    SystemIntegration.HandlePartialFailure("Material Modifications", failures, successes);
                }

                // Save assets if any modifications were successful
                if (result.SuccessfulModifications > 0)
                {
                    SystemIntegration.EnsureAssetConsistency();
                    result.SuccessMessages.Add($"Saved {result.SuccessfulModifications} modified materials");
                }

                LogWithContext(LogLevel.Info, "ApplyModifications", "Modifications complete", 
                    $"Successful: {result.SuccessfulModifications}, Failed: {result.FailedModifications}");

                return result;
            }, 
            "Apply Material Modifications", 
            new ModificationResult { ErrorMessages = { "Failed to apply modifications due to unexpected error" } }).Data;
        }

        /// <summary>
        /// Applies modifications to a list of materials (alternative method without preview)
        /// </summary>
        /// <param name="materials">List of materials to modify</param>
        /// <param name="propertyName">The property name to modify</param>
        /// <param name="targetValue">The target value to set</param>
        /// <returns>ModificationResult containing the results of the operation</returns>
        public ModificationResult ApplyModifications(List<Material> materials, string propertyName, object targetValue)
        {
            // Generate preview first, then apply
            var preview = PreviewModifications(materials, propertyName, targetValue);
            return ApplyModifications(preview, propertyName, targetValue);
        }

        /// <summary>
        /// Sets a property value on a material using the appropriate method based on property type
        /// </summary>
        /// <param name="material">The material to modify</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="value">The value to set</param>
        /// <param name="propertyType">The property type</param>
        /// <returns>True if the property was set successfully</returns>
        private bool SetMaterialProperty(Material material, string propertyName, object value, ShaderPropertyType propertyType)
        {
            if (material == null || !material.HasProperty(propertyName))
            {
                return false;
            }

            try
            {
                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        float floatValue = System.Convert.ToSingle(value);
                        material.SetFloat(propertyName, floatValue);
                        return true;
                    
                    case ShaderPropertyType.Int:
                        int intValue = System.Convert.ToInt32(value);
                        material.SetInt(propertyName, intValue);
                        return true;
                    
                    case ShaderPropertyType.Color:
                        Color colorValue;
                        if (value is Vector4 vec4)
                            colorValue = new Color(vec4.x, vec4.y, vec4.z, vec4.w);
                        else
                            colorValue = (Color)value;
                        material.SetColor(propertyName, colorValue);
                        return true;
                    
                    case ShaderPropertyType.Vector:
                        Vector4 vectorValue;
                        if (value is Vector3 vec3)
                            vectorValue = new Vector4(vec3.x, vec3.y, vec3.z, 0);
                        else if (value is Vector2 vec2)
                            vectorValue = new Vector4(vec2.x, vec2.y, 0, 0);
                        else
                            vectorValue = (Vector4)value;
                        material.SetVector(propertyName, vectorValue);
                        return true;
                    
                    case ShaderPropertyType.Texture:
                        Texture textureValue = value as Texture;
                        material.SetTexture(propertyName, textureValue); // null is valid (removes texture)
                        return true;
                    
                    default:
                        Debug.LogWarning($"Unsupported property type: {propertyType}");
                        return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error setting property '{propertyName}' on material '{material.name}': {ex.Message}");
                return false;
            }
        }
    }
}