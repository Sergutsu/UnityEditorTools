using System.Collections.Generic;
using System.Linq;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Contains preview information for material modifications
    /// </summary>
    public class ModificationPreview
    {
        public List<MaterialModification> Modifications { get; set; }
        public List<string> SkippedMaterials { get; set; }
        public int TotalCount { get; set; }
        public int ModifiedCount => Modifications?.Count ?? 0;
        public int SkippedCount => SkippedMaterials?.Count ?? 0;
        
        // Properties expected by the UI and other code
        public List<MaterialModification> MaterialsToModify => Modifications?.Where(m => m.WillBeModified).ToList() ?? new List<MaterialModification>();
        public List<MaterialModification> MaterialsToSkip => Modifications?.Where(m => !m.WillBeModified).ToList() ?? new List<MaterialModification>();
        
        // Additional properties for compatibility
        public int TotalMaterials => TotalCount;
        public string PropertyName { get; set; }
        public object TargetValue { get; set; }
        public ShaderPropertyType PropertyType { get; set; }

        public ModificationPreview()
        {
            Modifications = new List<MaterialModification>();
            SkippedMaterials = new List<string>();
            TotalCount = 0;
        }
    }
}