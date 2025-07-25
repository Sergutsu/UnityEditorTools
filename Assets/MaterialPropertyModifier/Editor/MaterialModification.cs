using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Represents a single material modification with current and target values
    /// </summary>
    public class MaterialModification
    {
        public Material TargetMaterial { get; set; }
        public object CurrentValue { get; set; }
        public object TargetValue { get; set; }
        public bool WillBeModified { get; set; }
        public string MaterialPath { get; set; }
        public ShaderPropertyType PropertyType { get; set; }

        public MaterialModification()
        {
            WillBeModified = false;
        }

        public MaterialModification(Material material, object currentValue, object targetValue, bool willModify, string path, ShaderPropertyType propertyType)
        {
            TargetMaterial = material;
            CurrentValue = currentValue;
            TargetValue = targetValue;
            WillBeModified = willModify;
            MaterialPath = path;
            PropertyType = propertyType;
        }
    }
}