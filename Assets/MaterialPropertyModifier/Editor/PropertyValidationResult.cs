using UnityEngine;
using UnityEditor;


namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of property validation containing validation status and property information
    /// </summary>
    public class PropertyValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public UnityEngine.Rendering.ShaderPropertyType PropertyType { get; set; }

        public PropertyValidationResult()
        {
            IsValid = false;
            ErrorMessage = string.Empty;
        }

        public PropertyValidationResult(bool isValid, string errorMessage = "", UnityEngine.Rendering.ShaderPropertyType propertyType = UnityEngine.Rendering.ShaderPropertyType.Float)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
            PropertyType = propertyType;
        }
    }


}