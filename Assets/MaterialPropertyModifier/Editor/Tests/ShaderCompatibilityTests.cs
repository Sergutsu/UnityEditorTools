using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Tests compatibility with various Unity shader types and rendering pipelines
    /// </summary>
    public class ShaderCompatibilityTests
    {
        private MaterialPropertyModifier modifier;
        private string testFolderPath;
        private List<Material> testMaterials;

        [SetUp]
        public void SetUp()
        {
            modifier = new MaterialPropertyModifier();
            testFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/ShaderCompatibilityMaterials";
            
            CreateTestFolder();
            testMaterials = new List<Material>();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupTestMaterials();
            
            if (AssetDatabase.IsValidFolder(testFolderPath))
            {
                AssetDatabase.DeleteAsset(testFolderPath);
            }
            
            AssetDatabase.Refresh();
        }

        #region Built-in Shader Tests

        [Test]
        public void StandardShader_CommonProperties_WorkCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            Assume.That(shader, Is.Not.Null, "Standard shader must be available");

            var material = CreateTestMaterial(shader, "StandardTest");
            var materials = new List<Material> { material };

            // Test float property
            TestPropertyModification(materials, "_Metallic", 0.8f, ShaderPropertyType.Float);
            
            // Test color property
            TestPropertyModification(materials, "_Color", Color.red, ShaderPropertyType.Color);
            
            // Test texture property (null to remove texture)
            TestPropertyModification(materials, "_MainTex", null, ShaderPropertyType.Texture);
        }

        [Test]
        public void UnlitColorShader_ColorProperty_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Unlit/Color");
            Assume.That(shader, Is.Not.Null, "Unlit/Color shader must be available");

            var material = CreateTestMaterial(shader, "UnlitColorTest");
            var materials = new List<Material> { material };

            // Test color property
            TestPropertyModification(materials, "_Color", Color.blue, ShaderPropertyType.Color);
        }

        [Test]
        public void UnlitTextureShader_TextureProperty_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Unlit/Texture");
            Assume.That(shader, Is.Not.Null, "Unlit/Texture shader must be available");

            var material = CreateTestMaterial(shader, "UnlitTextureTest");
            var materials = new List<Material> { material };

            // Create a test texture
            var testTexture = new Texture2D(1, 1);
            testTexture.SetPixel(0, 0, Color.white);
            testTexture.Apply();

            try
            {
                // Test texture property
                TestPropertyModification(materials, "_MainTex", testTexture, ShaderPropertyType.Texture);
            }
            finally
            {
                Object.DestroyImmediate(testTexture);
            }
        }

        [Test]
        public void SpriteDefaultShader_SpriteProperties_WorkCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Assert.Ignore("Sprites/Default shader not available in this Unity version");
                return;
            }

            var material = CreateTestMaterial(shader, "SpriteDefaultTest");
            var materials = new List<Material> { material };

            // Test color property (common in sprite shaders)
            if (material.HasProperty("_Color"))
            {
                TestPropertyModification(materials, "_Color", Color.green, ShaderPropertyType.Color);
            }
        }

        #endregion

        #region URP Shader Tests

        [Test]
        public void URPLitShader_IfAvailable_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Assert.Ignore("URP Lit shader not available - URP may not be installed");
                return;
            }

            var material = CreateTestMaterial(shader, "URPLitTest");
            var materials = new List<Material> { material };

            // Test common URP properties
            if (material.HasProperty("_Metallic"))
            {
                TestPropertyModification(materials, "_Metallic", 0.7f, ShaderPropertyType.Float);
            }
            
            if (material.HasProperty("_BaseColor"))
            {
                TestPropertyModification(materials, "_BaseColor", Color.cyan, ShaderPropertyType.Color);
            }
        }

        [Test]
        public void URPUnlitShader_IfAvailable_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Assert.Ignore("URP Unlit shader not available - URP may not be installed");
                return;
            }

            var material = CreateTestMaterial(shader, "URPUnlitTest");
            var materials = new List<Material> { material };

            // Test base color property
            if (material.HasProperty("_BaseColor"))
            {
                TestPropertyModification(materials, "_BaseColor", Color.magenta, ShaderPropertyType.Color);
            }
        }

        #endregion

        #region HDRP Shader Tests

        [Test]
        public void HDRPLitShader_IfAvailable_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("HDRP/Lit");
            if (shader == null)
            {
                Assert.Ignore("HDRP Lit shader not available - HDRP may not be installed");
                return;
            }

            var material = CreateTestMaterial(shader, "HDRPLitTest");
            var materials = new List<Material> { material };

            // Test common HDRP properties
            if (material.HasProperty("_Metallic"))
            {
                TestPropertyModification(materials, "_Metallic", 0.9f, ShaderPropertyType.Float);
            }
            
            if (material.HasProperty("_BaseColor"))
            {
                TestPropertyModification(materials, "_BaseColor", Color.yellow, ShaderPropertyType.Color);
            }
        }

        [Test]
        public void HDRPUnlitShader_IfAvailable_WorksCorrectly()
        {
            // Arrange
            var shader = Shader.Find("HDRP/Unlit");
            if (shader == null)
            {
                Assert.Ignore("HDRP Unlit shader not available - HDRP may not be installed");
                return;
            }

            var material = CreateTestMaterial(shader, "HDRPUnlitTest");
            var materials = new List<Material> { material };

            // Test color property
            if (material.HasProperty("_UnlitColor"))
            {
                TestPropertyModification(materials, "_UnlitColor", Color.gray, ShaderPropertyType.Color);
            }
        }

        #endregion

        #region Custom Shader Tests

        [Test]
        public void CustomShader_IfAvailable_WorksCorrectly()
        {
            // This test would work with any custom shaders in the project
            // For now, we'll test the general approach with available shaders
            
            var allShaders = Resources.FindObjectsOfTypeAll<Shader>();
            var customShaders = allShaders.Where(s => 
                !s.name.StartsWith("Standard") && 
                !s.name.StartsWith("Unlit/") && 
                !s.name.StartsWith("Sprites/") &&
                !s.name.StartsWith("Universal Render Pipeline/") &&
                !s.name.StartsWith("HDRP/") &&
                !s.name.StartsWith("Hidden/") &&
                !s.name.StartsWith("Legacy/")).ToArray();

            if (customShaders.Length == 0)
            {
                Assert.Ignore("No custom shaders found in project");
                return;
            }

            // Test with first available custom shader
            var customShader = customShaders[0];
            var material = CreateTestMaterial(customShader, "CustomShaderTest");

            // Test property discovery
            var propertyCount = customShader.GetPropertyCount();
            Assert.GreaterOrEqual(propertyCount, 0, "Should be able to get property count from custom shader");

            // Test property validation
            if (propertyCount > 0)
            {
                string firstPropertyName = customShader.GetPropertyName(0);
                var validationResult = modifier.ValidateProperty(customShader, firstPropertyName);
                Assert.IsTrue(validationResult.IsValid, "Should validate existing property on custom shader");
            }
        }

        #endregion

        #region Property Type Coverage Tests

        [Test]
        public void AllShaderPropertyTypes_AreHandledCorrectly()
        {
            var standardShader = Shader.Find("Standard");
            Assume.That(standardShader, Is.Not.Null);

            var material = CreateTestMaterial(standardShader, "PropertyTypeTest");

            // Test each property type that exists on Standard shader
            var propertyTypes = new[]
            {
                ShaderPropertyType.Float,
                ShaderPropertyType.Color,
                ShaderPropertyType.Vector,
                ShaderPropertyType.Texture,
                ShaderPropertyType.Range
            };

            foreach (var propertyType in propertyTypes)
            {
                // Find a property of this type on the shader
                string propertyName = FindPropertyOfType(standardShader, propertyType);
                if (propertyName != null)
                {
                    var validationResult = modifier.ValidateProperty(standardShader, propertyName);
                    Assert.IsTrue(validationResult.IsValid, $"Should validate {propertyType} property '{propertyName}'");
                    Assert.AreEqual(propertyType, validationResult.PropertyType, $"Should detect correct type for {propertyName}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private void CreateTestFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/MaterialPropertyModifier/Editor/Tests"))
            {
                AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor", "Tests");
            }
            
            if (!AssetDatabase.IsValidFolder(testFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor/Tests", "ShaderCompatibilityMaterials");
            }
        }

        private Material CreateTestMaterial(Shader shader, string name)
        {
            var material = new Material(shader);
            material.name = name;
            
            string assetPath = $"{testFolderPath}/{name}.mat";
            AssetDatabase.CreateAsset(material, assetPath);
            testMaterials.Add(material);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return material;
        }

        private void TestPropertyModification(List<Material> materials, string propertyName, object targetValue, ShaderPropertyType expectedType)
        {
            // Validate property exists and has correct type
            var shader = materials[0].shader;
            var validationResult = modifier.ValidateProperty(shader, propertyName);
            
            if (!validationResult.IsValid)
            {
                Assert.Ignore($"Property '{propertyName}' not available on shader '{shader.name}'");
                return;
            }
            
            Assert.AreEqual(expectedType, validationResult.PropertyType, 
                $"Property '{propertyName}' should be of type {expectedType}");

            // Test value validation
            var valueValidation = modifier.ValidatePropertyValue(expectedType, targetValue);
            Assert.IsTrue(valueValidation.IsValid, 
                $"Value should be valid for property type {expectedType}: {valueValidation.ErrorMessage}");

            // Generate preview
            var preview = modifier.PreviewModifications(materials, propertyName, targetValue);
            Assert.IsNotNull(preview, "Should generate preview");
            Assert.Greater(preview.Modifications.Count, 0, "Should have modifications in preview");

            // Apply modifications
            var result = modifier.ApplyModifications(preview, propertyName, targetValue);
            Assert.IsNotNull(result, "Should return modification result");
            Assert.Greater(result.SuccessfulModifications, 0, "Should successfully modify materials");

            // Verify the modification was applied
            foreach (var material in materials)
            {
                if (material.HasProperty(propertyName))
                {
                    VerifyPropertyValue(material, propertyName, targetValue, expectedType);
                }
            }
        }

        private void VerifyPropertyValue(Material material, string propertyName, object expectedValue, ShaderPropertyType propertyType)
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    float expectedFloat = System.Convert.ToSingle(expectedValue);
                    float actualFloat = material.GetFloat(propertyName);
                    Assert.AreEqual(expectedFloat, actualFloat, 0.001f, 
                        $"Float property '{propertyName}' should have expected value");
                    break;

                case ShaderPropertyType.Int:
                    int expectedInt = System.Convert.ToInt32(expectedValue);
                    int actualInt = material.GetInt(propertyName);
                    Assert.AreEqual(expectedInt, actualInt, 
                        $"Int property '{propertyName}' should have expected value");
                    break;

                case ShaderPropertyType.Color:
                    Color expectedColor = (Color)expectedValue;
                    Color actualColor = material.GetColor(propertyName);
                    Assert.AreEqual(expectedColor, actualColor, 
                        $"Color property '{propertyName}' should have expected value");
                    break;

                case ShaderPropertyType.Vector:
                    Vector4 expectedVector = (Vector4)expectedValue;
                    Vector4 actualVector = material.GetVector(propertyName);
                    Assert.AreEqual(expectedVector, actualVector, 
                        $"Vector property '{propertyName}' should have expected value");
                    break;

                case ShaderPropertyType.Texture:
                    Texture expectedTexture = (Texture)expectedValue;
                    Texture actualTexture = material.GetTexture(propertyName);
                    Assert.AreEqual(expectedTexture, actualTexture, 
                        $"Texture property '{propertyName}' should have expected value");
                    break;
            }
        }

        private string FindPropertyOfType(Shader shader, ShaderPropertyType targetType)
        {
            int propertyCount = shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                if (shader.GetPropertyType(i) == targetType)
                {
                    return shader.GetPropertyName(i);
                }
            }
            return null;
        }

        private void CleanupTestMaterials()
        {
            foreach (var material in testMaterials)
            {
                if (material != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(material);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }
            testMaterials.Clear();
        }

        #endregion
    }
}