using System.Collections.Generic;
using UnityEngine;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Result of material discovery operation with comprehensive error information
    /// </summary>
    public class MaterialDiscoveryResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<Material> Materials { get; set; }
        public string FolderPath { get; set; }
        public Shader TargetShader { get; set; }

        public MaterialDiscoveryResult()
        {
            IsSuccess = false;
            ErrorMessage = string.Empty;
            Materials = new List<Material>();
        }
    }
}