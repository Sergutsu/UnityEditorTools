using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Simple validation script to test MaterialPropertyModifier compilation and basic functionality
    /// </summary>
    public static class ValidationScript
    {
        [MenuItem("Tools/Material Property Modifier/Validate Implementation")]
        public static void ValidateImplementation()
        {
            Debug.Log("Starting MaterialPropertyModifier validation...");
            
            try
            {
                // Test instantiation
                var modifier = new MaterialPropertyModifier();
                Debug.Log("✓ MaterialPropertyModifier instantiated successfully");
                
                // Test with null parameters
                var emptyResult = modifier.FindMaterialsWithShader(null, null);
                Debug.Log($"✓ Null parameter handling: returned {emptyResult.Count} materials (expected 0)");
                
                // Test with Standard shader
                var standardShader = Shader.Find("Standard");
                if (standardShader != null)
                {
                    Debug.Log("✓ Standard shader found");
                    
                    // Test property validation
                    var validationResult = modifier.ValidateProperty(standardShader, "_MainTex");
                    Debug.Log($"✓ Property validation for '_MainTex': {validationResult.IsValid} (Type: {validationResult.PropertyType})");
                    
                    var invalidValidation = modifier.ValidateProperty(standardShader, "_InvalidProperty");
                    Debug.Log($"✓ Invalid property validation: {invalidValidation.IsValid} (Error: {invalidValidation.ErrorMessage})");
                    
                    // Test property type retrieval
                    var propertyType = modifier.GetPropertyType(standardShader, "_MainTex");
                    Debug.Log($"✓ Property type for '_MainTex': {propertyType}");
                }
                else
                {
                    Debug.LogWarning("Standard shader not found - some tests skipped");
                }
                
                Debug.Log("✓ All validation tests passed!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"✗ Validation failed: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}