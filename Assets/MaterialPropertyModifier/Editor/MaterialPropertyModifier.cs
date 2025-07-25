using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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
            var foundMaterials = new List<Material>();
            
            if (string.IsNullOrEmpty(folderPath) || targetShader == null)
            {
                return foundMaterials;
            }

            // Ensure the folder path starts with "Assets/"
            if (!folderPath.StartsWith("Assets/"))
            {
                folderPath = "Assets/" + folderPath.TrimStart('/');
            }

            try
            {
                // Find all material assets in the specified folder and its subfolders recursively
                string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
                
                Debug.Log($"Scanning folder '{folderPath}' for materials with shader '{targetShader.name}'...");
                Debug.Log($"Found {materialGuids.Length} total materials in folder and subfolders");
                
                foreach (string guid in materialGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    
                    if (material != null && material.shader == targetShader)
                    {
                        foundMaterials.Add(material);
                        Debug.Log($"Found matching material: {material.name} at {assetPath}");
                    }
                }
                
                Debug.Log($"Found {foundMaterials.Count} materials using shader '{targetShader.name}'");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error finding materials with shader in folder '{folderPath}': {ex.Message}");
            }

            return foundMaterials;
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
            var result = new ModificationResult();
            
            if (preview == null || preview.Modifications == null)
            {
                result.ErrorMessages.Add("Invalid preview data provided");
                return result;
            }

            result.TotalProcessed = preview.Modifications.Count;
            result.SkippedMaterials = preview.SkippedCount;

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
                Undo.RecordObjects(materialsToModify.ToArray(), $"Modify {propertyName} property");
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
                        result.ErrorMessages.Add("Null material reference");
                        continue;
                    }

                    bool success = SetMaterialProperty(modification.TargetMaterial, propertyName, targetValue, modification.PropertyType);
                    
                    if (success)
                    {
                        // Mark material as dirty for saving
                        EditorUtility.SetDirty(modification.TargetMaterial);
                        result.SuccessfulModifications++;
                        result.SuccessMessages.Add($"Modified {modification.TargetMaterial.name}: {modification.CurrentValue} → {modification.TargetValue}");
                    }
                    else
                    {
                        result.FailedModifications++;
                        result.ErrorMessages.Add($"Failed to set property on {modification.TargetMaterial.name}");
                    }
                }
                catch (System.Exception ex)
                {
                    result.FailedModifications++;
                    string materialName = modification.TargetMaterial?.name ?? "Unknown";
                    result.ErrorMessages.Add($"Error modifying {materialName}: {ex.Message}");
                    Debug.LogError($"Error applying modification to {materialName}: {ex.Message}");
                }
            }

            // Save assets if any modifications were successful
            if (result.SuccessfulModifications > 0)
            {
                try
                {
                    AssetDatabase.SaveAssets();
                    result.SuccessMessages.Add($"Saved {result.SuccessfulModifications} modified materials");
                }
                catch (System.Exception ex)
                {
                    result.ErrorMessages.Add($"Error saving assets: {ex.Message}");
                    Debug.LogError($"Error saving assets: {ex.Message}");
                }
            }

            return result;
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