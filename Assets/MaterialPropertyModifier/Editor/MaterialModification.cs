using UnityEngine;

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

        public MaterialModification()
        {
            WillBeModified = false;
        }

        public MaterialModification(Material targetMaterial, object currentValue, object targetValue, bool willBeModified)
        {
            TargetMaterial = targetMaterial;
            CurrentValue = currentValue;
            TargetValue = targetValue;
            WillBeModified = willBeModified;
        }
    }
}