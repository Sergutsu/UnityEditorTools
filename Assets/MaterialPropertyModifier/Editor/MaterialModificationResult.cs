using UnityEngine;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of material modification operation with comprehensive error information
    /// </summary>
    public class MaterialModificationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public MaterialModification ModificationResult { get; set; }

        public MaterialModificationResult()
        {
            IsSuccess = false;
            ErrorMessage = string.Empty;
            ModificationResult = null;
        }
    }
}