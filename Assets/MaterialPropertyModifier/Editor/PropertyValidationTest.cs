using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Simple test script to validate property validation functionality
    /// </summary>
    public class PropertyValidationTest
    {
        [MenuItem("Tools/Material Property Modifier/Test Property Validation")]
        public static void TestPropertyValidation()
        {
            var modifier = new MaterialPropertyModifier();
            var standardShader = Shader.Find("Standard");
            
            if (standardShader == null)
            {
                Debug.LogError("Standard shader not found!");
                return;
            }

            Debug.Log("=== Testing Property Validation System ===");

            // Test ValidateProperty
            Debug.Log("\n--- Testing ValidateProperty ---");
            var validResult = modifier.ValidateProperty(standardShader, "_MainTex");
            Debug.Log($"Valid property '_MainTex': {validResult.IsValid}, Type: {validResult.PropertyType}");

            var invalidResult = modifier.ValidateProperty(standardShader, "_InvalidProp");
            Debug.Log($"Invalid property '_InvalidProp': {invalidResult.IsValid}, Error: {invalidResult.ErrorMessage}");

            // Test ValidatePropertyValue
            Debug.Log("\n--- Testing ValidatePropertyValue ---");
            
            // Float validation
            var floatValid = modifier.ValidatePropertyValue(ShaderPropertyType.Float, 1.5f);
            Debug.Log($"Float value 1.5f: {floatValid.IsValid}");
            
            var floatInvalid = modifier.ValidatePropertyValue(ShaderPropertyType.Float, "not_a_number");
            Debug.Log($"Float value 'not_a_number': {floatInvalid.IsValid}, Error: {floatInvalid.ErrorMessage}");

            // Color validation
            var colorValid = modifier.ValidatePropertyValue(ShaderPropertyType.Color, Color.red);
            Debug.Log($"Color value Color.red: {colorValid.IsValid}");
            
            var colorInvalid = modifier.ValidatePropertyValue(ShaderPropertyType.Color, "red");
            Debug.Log($"Color value 'red': {colorInvalid.IsValid}, Error: {colorInvalid.ErrorMessage}");

            // Texture validation
            var texture = new Texture2D(1, 1);
            var textureValid = modifier.ValidatePropertyValue(ShaderPropertyType.Texture, texture);
            Debug.Log($"Texture value Texture2D: {textureValid.IsValid}");
            Object.DestroyImmediate(texture);

            // Test ValidatePropertyAndValue
            Debug.Log("\n--- Testing ValidatePropertyAndValue ---");
            var combinedValid = modifier.ValidatePropertyAndValue(standardShader, "_Metallic", 0.5f);
            Debug.Log($"Property '_Metallic' with value 0.5f: {combinedValid.IsValid}");

            var combinedInvalid = modifier.ValidatePropertyAndValue(standardShader, "_MainTex", "not_a_texture");
            Debug.Log($"Property '_MainTex' with string value: {combinedInvalid.IsValid}, Error: {combinedInvalid.ErrorMessage}");

            Debug.Log("\n=== Property Validation Test Complete ===");
        }
    }
}