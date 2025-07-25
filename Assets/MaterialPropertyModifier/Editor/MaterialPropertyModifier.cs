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
    }
}