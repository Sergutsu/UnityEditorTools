using System.Collections.Generic;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Contains the results of applying material modifications
    /// </summary>
    public class ModificationResult
    {
        public int TotalProcessed { get; set; }
        public int SuccessfulModifications { get; set; }
        public int SkippedMaterials { get; set; }
        public int FailedModifications { get; set; }
        public List<string> ErrorMessages { get; set; }
        public List<string> SuccessMessages { get; set; }
        public bool IsSuccess => FailedModifications == 0 && TotalProcessed > 0;

        public ModificationResult()
        {
            ErrorMessages = new List<string>();
            SuccessMessages = new List<string>();
            TotalProcessed = 0;
            SuccessfulModifications = 0;
            SkippedMaterials = 0;
            FailedModifications = 0;
        }
    }
}