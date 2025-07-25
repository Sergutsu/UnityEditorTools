using System.Collections.Generic;
using UnityEngine;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Holds the current state and settings for material property modification operations
    /// </summary>
    public class MaterialModificationData
    {
        public string SelectedFolderPath { get; set; }
        public Shader TargetShader { get; set; }
        public string PropertyName { get; set; }
        public object TargetValue { get; set; }
        public List<Material> FoundMaterials { get; set; }
        public ModificationPreview Preview { get; set; }
        public bool IsValid { get; set; }

        public MaterialModificationData()
        {
            FoundMaterials = new List<Material>();
            IsValid = false;
        }
    }
}