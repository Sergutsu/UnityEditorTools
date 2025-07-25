using UnityEngine;
using UnityEditor;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;


namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of property validation containing validation status and property information
    /// </summary>
    public class PropertyValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public ShaderPropertyType PropertyType { get; set; }

        public PropertyValidationResult()
        {
            IsValid = false;
            ErrorMessage = string.Empty;
        }

        public PropertyValidationResult(bool isValid, string errorMessage = "", ShaderPropertyType propertyType = ShaderPropertyType.Float)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
            PropertyType = propertyType;
        }
    }


}