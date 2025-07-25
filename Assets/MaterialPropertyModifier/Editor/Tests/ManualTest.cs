using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Manual test script to verify MaterialPropertyModifier functionality
    /// </summary>
    public static class ManualTest
    {
        [MenuItem("Tools/Material Property Modifier/Run Manual Test")]
        public static void RunManualTest()
        {
            var modifier = new MaterialPropertyModifier();
            
            // Test with a known folder (create some test materials first)
            string testFolder = "Assets/MaterialPropertyModifier/Editor/Tests/TestMaterials";
            Shader standardShader = Shader.Find("Standard");
            
            if (standardShader == null)
            {
                Debug.LogError("Standard shader not found!");
                return;
            }
            
            // Create test folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(testFolder))
            {
                string parentFolder = "Assets/MaterialPropertyModifier/Editor/Tests";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor", "Tests");
                }
                AssetDatabase.CreateFolder(parentFolder, "TestMaterials");
            }
            
            // Create a test material
            var testMaterial = new Material(standardShader);
            testMaterial.name = "ManualTestMaterial";
            AssetDatabase.CreateAsset(testMaterial, $"{testFolder}/ManualTestMaterial.mat");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Test FindMaterialsWithShader
            var foundMaterials = modifier.FindMaterialsWithShader(testFolder, standardShader);
            Debug.Log($"Found {foundMaterials.Count} materials with Standard shader in {testFolder}");
            
            foreach (var material in foundMaterials)
            {
                Debug.Log($"- {material.name} at {AssetDatabase.GetAssetPath(material)}");
            }
            
            // Test property validation
            var validationResult = modifier.ValidateProperty(standardShader, "_MainTex");
            Debug.Log($"Property '_MainTex' validation: {validationResult.IsValid}, Type: {validationResult.PropertyType}");
            
            var invalidValidationResult = modifier.ValidateProperty(standardShader, "_NonExistentProperty");
            Debug.Log($"Property '_NonExistentProperty' validation: {invalidValidationResult.IsValid}, Error: {invalidValidationResult.ErrorMessage}");
            
            // Test GetPropertyType
            var propertyType = modifier.GetPropertyType(standardShader, "_MainTex");
            Debug.Log($"Property '_MainTex' type: {propertyType}");
            
            Debug.Log("Manual test completed!");
        }
    }
}