using System.Collections.Generic;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Contains preview information for material modifications before they are applied
    /// </summary>
    public class ModificationPreview
    {
        public List<MaterialModification> Modifications { get; set; }
        public List<string> SkippedMaterials { get; set; }
        public int TotalCount { get; set; }

        public ModificationPreview()
        {
            Modifications = new List<MaterialModification>();
            SkippedMaterials = new List<string>();
            TotalCount = 0;
        }

        public ModificationPreview(List<MaterialModification> modifications, List<string> skippedMaterials)
        {
            Modifications = modifications ?? new List<MaterialModification>();
            SkippedMaterials = skippedMaterials ?? new List<string>();
            TotalCount = Modifications.Count + SkippedMaterials.Count;
        }
    }
}