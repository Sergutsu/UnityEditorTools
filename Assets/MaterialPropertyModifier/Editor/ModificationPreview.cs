using System.Collections.Generic;

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

        public ModificationPreview()
        {
            Modifications = new List<MaterialModification>();
            SkippedMaterials = new List<string>();
            TotalCount = 0;
        }
    }
}