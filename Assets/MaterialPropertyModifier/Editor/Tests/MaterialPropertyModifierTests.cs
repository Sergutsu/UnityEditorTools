using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Unit tests for MaterialPropertyModifier core functionality
    /// </summary>
    public class MaterialPropertyModifierTests
    {
        private MaterialPropertyModifier modifier;
        private string testFolderPath;
        private Shader testShader;
        private Shader alternativeShader;
        private List<Material> testMaterials;

        [SetUp]
        public void SetUp()
        {
            modifier = new MaterialPropertyModifier();
            testFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/TestMaterials";
            
            // Create test folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(testFolderPath))
            {
                string parentFolder = "Assets/MaterialPropertyModifier/Editor/Tests";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor", "Tests");
                }
                AssetDatabase.CreateFolder(parentFolder, "TestMaterials");
            }

            // Get reference shaders for testing
            testShader = Shader.Find("Standard");
            alternativeShader = Shader.Find("Unlit/Color");
            
            testMaterials = new List<Material>();
            CreateTestMaterials();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test materials
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
            
            AssetDatabase.Refresh();
        }

        private void CreateTestMaterials()
        {
            // Create materials with target shader
            var material1 = new Material(testShader);
            material1.name = "TestMaterial1";
            AssetDatabase.CreateAsset(material1, $"{testFolderPath}/TestMaterial1.mat");
            testMaterials.Add(material1);

            var material2 = new Material(testShader);
            material2.name = "TestMaterial2";
            AssetDatabase.CreateAsset(material2, $"{testFolderPath}/TestMaterial2.mat");
            testMaterials.Add(material2);

            // Create material with different shader
            var material3 = new Material(alternativeShader);
            material3.name = "TestMaterial3";
            AssetDatabase.CreateAsset(material3, $"{testFolderPath}/TestMaterial3.mat");
            testMaterials.Add(material3);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Test]
        public void FindMaterialsWithShader_ValidFolderAndShader_ReturnsMatchingMaterials()
        {
            // Act
            var result = modifier.FindMaterialsWithShader(testFolderPath, testShader);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count, "Should find exactly 2 materials with the target shader");
            
            foreach (var material in result)
            {
                Assert.AreEqual(testShader, material.shader, "All returned materials should use the target shader");
            }
        }

        [Test]
        public void FindMaterialsWithShader_EmptyFolderPath_ReturnsEmptyList()
        {
            // Act
            var result = modifier.FindMaterialsWithShader("", testShader);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count, "Should return empty list for empty folder path");
        }

        [Test]
        public void FindMaterialsWithShader_NullShader_ReturnsEmptyList()
        {
            // Act
            var result = modifier.FindMaterialsWithShader(testFolderPath, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count, "Should return empty list for null shader");
        }

        [Test]
        public void FindMaterialsWithShader_NonExistentFolder_ReturnsEmptyList()
        {
            // Act
            var result = modifier.FindMaterialsWithShader("Assets/NonExistentFolder", testShader);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count, "Should return empty list for non-existent folder");
        }

        [Test]
        public void FindMaterialsWithShader_FolderWithoutAssetsPrefix_HandlesCorrectly()
        {
            // Act - Test with folder path without "Assets/" prefix
            string folderWithoutPrefix = testFolderPath.Substring("Assets/".Length);
            var result = modifier.FindMaterialsWithShader(folderWithoutPrefix, testShader);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count, "Should handle folder paths without Assets/ prefix");
        }

        [Test]
        public void ValidateProperty_ValidProperty_ReturnsTrue()
        {
            // Arrange
            string validProperty = "_MainTex"; // Standard shader has this property

            // Act
            var result = modifier.ValidateProperty(testShader, validProperty);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate existing property as valid");
            Assert.IsEmpty(result.ErrorMessage, "Should have no error message for valid property");
        }

        [Test]
        public void ValidateProperty_InvalidProperty_ReturnsFalse()
        {
            // Arrange
            string invalidProperty = "_NonExistentProperty";

            // Act
            var result = modifier.ValidateProperty(testShader, invalidProperty);

            // Assert
            Assert.IsFalse(result.IsValid, "Should validate non-existent property as invalid");
            Assert.IsNotEmpty(result.ErrorMessage, "Should have error message for invalid property");
        }

        [Test]
        public void ValidateProperty_NullShader_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidateProperty(null, "_MainTex");

            // Assert
            Assert.IsFalse(result.IsValid, "Should return false for null shader");
            Assert.IsNotEmpty(result.ErrorMessage, "Should have error message for null shader");
        }

        [Test]
        public void GetPropertyType_ValidProperty_ReturnsCorrectType()
        {
            // Arrange
            string textureProperty = "_MainTex"; // Should be Texture type

            // Act
            var result = modifier.GetPropertyType(testShader, textureProperty);

            // Assert
            Assert.AreEqual(ShaderPropertyType.Texture, result, "Should return correct property type for texture property");
        }

        [Test]
        public void GetPropertyType_InvalidProperty_ReturnsFloat()
        {
            // Arrange
            string invalidProperty = "_NonExistentProperty";

            // Act
            var result = modifier.GetPropertyType(testShader, invalidProperty);

            // Assert
            Assert.AreEqual(ShaderPropertyType.Float, result, "Should return Float as default for invalid property");
        }

        [Test]
        public void GetMaterialAssetPath_ValidMaterial_ReturnsCorrectPath()
        {
            // Arrange
            var material = testMaterials[0]; // Use first test material

            // Act
            var result = modifier.GetMaterialAssetPath(material);

            // Assert
            Assert.IsNotEmpty(result, "Should return non-empty path for valid material");
            Assert.IsTrue(result.StartsWith("Assets/"), "Path should start with Assets/");
            Assert.IsTrue(result.EndsWith(".mat"), "Path should end with .mat extension");
        }

        [Test]
        public void GetMaterialAssetPath_NullMaterial_ReturnsEmptyString()
        {
            // Act
            var result = modifier.GetMaterialAssetPath(null);

            // Assert
            Assert.AreEqual(string.Empty, result, "Should return empty string for null material");
        }

        [Test]
        public void GetMaterialAssetPaths_ValidMaterials_ReturnsCorrectPaths()
        {
            // Arrange
            var materialsWithTargetShader = testMaterials.Where(m => m.shader == testShader).ToList();

            // Act
            var result = modifier.GetMaterialAssetPaths(materialsWithTargetShader);

            // Assert
            Assert.IsNotNull(result, "Should return non-null dictionary");
            Assert.AreEqual(materialsWithTargetShader.Count, result.Count, "Should return path for each material");
            
            foreach (var kvp in result)
            {
                Assert.IsNotEmpty(kvp.Value, "Each path should be non-empty");
                Assert.IsTrue(kvp.Value.StartsWith("Assets/"), "Each path should start with Assets/");
            }
        }
    }
}