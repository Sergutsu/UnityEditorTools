using UnityEngine;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of modification preview operation with comprehensive error information
    /// </summary>
    public class ModificationPreviewResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public ModificationPreview Preview { get; set; }

        public ModificationPreviewResult()
        {
            IsSuccess = false;
            ErrorMessage = string.Empty;
            Preview = null;
        }
    }
}