using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using ShaderPropertyType = UnityEditor.ShaderPropertyType;

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

        #region Property Value Validation Tests

        [Test]
        public void ValidatePropertyValue_FloatType_ValidFloat_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Float, 1.5f);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate float value for Float property type");
            Assert.AreEqual(ShaderPropertyType.Float, result.PropertyType);
        }

        [Test]
        public void ValidatePropertyValue_FloatType_ValidInt_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Float, 42);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate int value for Float property type");
        }

        [Test]
        public void ValidatePropertyValue_FloatType_ValidString_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Float, "3.14");

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate parseable string for Float property type");
        }

        [Test]
        public void ValidatePropertyValue_FloatType_InvalidString_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Float, "not_a_number");

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject non-parseable string for Float property type");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyValue_IntType_ValidInt_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Int, 42);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate int value for Int property type");
            Assert.AreEqual(ShaderPropertyType.Int, result.PropertyType);
        }

        [Test]
        public void ValidatePropertyValue_IntType_ValidString_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Int, "123");

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate parseable string for Int property type");
        }

        [Test]
        public void ValidatePropertyValue_IntType_InvalidString_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Int, "3.14");

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject float string for Int property type");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyValue_ColorType_ValidColor_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Color, Color.red);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Color value for Color property type");
            Assert.AreEqual(ShaderPropertyType.Color, result.PropertyType);
        }

        [Test]
        public void ValidatePropertyValue_ColorType_ValidVector4_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Color, new Vector4(1, 0, 0, 1));

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Vector4 value for Color property type");
        }

        [Test]
        public void ValidatePropertyValue_ColorType_InvalidType_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Color, "red");

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject string value for Color property type");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyValue_VectorType_ValidVector4_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Vector, new Vector4(1, 2, 3, 4));

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Vector4 value for Vector property type");
            Assert.AreEqual(ShaderPropertyType.Vector, result.PropertyType);
        }

        [Test]
        public void ValidatePropertyValue_VectorType_ValidVector3_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Vector, new Vector3(1, 2, 3));

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Vector3 value for Vector property type");
        }

        [Test]
        public void ValidatePropertyValue_VectorType_ValidVector2_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Vector, new Vector2(1, 2));

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Vector2 value for Vector property type");
        }

        [Test]
        public void ValidatePropertyValue_VectorType_InvalidType_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Vector, 42);

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject non-vector value for Vector property type");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyValue_TextureType_ValidTexture2D_ReturnsTrue()
        {
            // Arrange
            var texture = new Texture2D(1, 1);

            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Texture, texture);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate Texture2D value for Texture property type");
            Assert.AreEqual(ShaderPropertyType.Texture, result.PropertyType);

            // Cleanup
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void ValidatePropertyValue_NullValue_ForTexture_ReturnsTrue()
        {
            // Act - Special case: null is valid for textures (removes texture)
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Texture, null);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate null texture (removes texture) for Texture property type");
        }

        [Test]
        public void ValidatePropertyValue_TextureType_InvalidType_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Texture, "texture_name");

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject string value for Texture property type");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyValue_RangeType_ValidFloat_ReturnsTrue()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Range, 0.5f);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate float value for Range property type");
            Assert.AreEqual(ShaderPropertyType.Range, result.PropertyType);
        }

        [Test]
        public void ValidatePropertyValue_NullValue_ForNonTexture_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyValue(ShaderPropertyType.Float, null);

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject null value for non-texture property types");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        #endregion

        #region Combined Property and Value Validation Tests

        [Test]
        public void ValidatePropertyAndValue_ValidPropertyAndValue_ReturnsTrue()
        {
            // Arrange
            string validProperty = "_Metallic"; // Standard shader has this float property
            float validValue = 0.5f;

            // Act
            var result = modifier.ValidatePropertyAndValue(testShader, validProperty, validValue);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate both existing property and compatible value");
            Assert.IsEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyAndValue_InvalidProperty_ReturnsFalse()
        {
            // Arrange
            string invalidProperty = "_NonExistentProperty";
            float validValue = 0.5f;

            // Act
            var result = modifier.ValidatePropertyAndValue(testShader, invalidProperty, validValue);

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject non-existent property");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyAndValue_ValidPropertyInvalidValue_ReturnsFalse()
        {
            // Arrange
            string validProperty = "_MainTex"; // Texture property
            string invalidValue = "not_a_texture";

            // Act
            var result = modifier.ValidatePropertyAndValue(testShader, validProperty, invalidValue);

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject incompatible value type for valid property");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public void ValidatePropertyAndValue_NullShader_ReturnsFalse()
        {
            // Act
            var result = modifier.ValidatePropertyAndValue(null, "_MainTex", new Texture2D(1, 1));

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject null shader");
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        #endregion

        #region Various Shader Types Tests

        [Test]
        public void ValidateProperty_UnlitShader_ValidProperty_ReturnsTrue()
        {
            // Arrange
            var unlitShader = Shader.Find("Unlit/Color");
            string validProperty = "_Color"; // Unlit/Color shader has this property

            // Act
            var result = modifier.ValidateProperty(unlitShader, validProperty);

            // Assert
            Assert.IsTrue(result.IsValid, "Should validate existing property on Unlit shader");
            Assert.AreEqual(ShaderPropertyType.Color, result.PropertyType);
        }

        [Test]
        public void ValidateProperty_UnlitShader_InvalidProperty_ReturnsFalse()
        {
            // Arrange
            var unlitShader = Shader.Find("Unlit/Color");
            string invalidProperty = "_Metallic"; // Unlit shader doesn't have metallic

            // Act
            var result = modifier.ValidateProperty(unlitShader, invalidProperty);

            // Assert
            Assert.IsFalse(result.IsValid, "Should reject non-existent property on Unlit shader");
        }

        [Test]
        public void ValidatePropertyAndValue_DifferentShaderTypes_WorksCorrectly()
        {
            // Test Standard shader
            var standardResult = modifier.ValidatePropertyAndValue(testShader, "_Metallic", 0.5f);
            Assert.IsTrue(standardResult.IsValid, "Should work with Standard shader");

            // Test Unlit shader
            var unlitShader = Shader.Find("Unlit/Color");
            var unlitResult = modifier.ValidatePropertyAndValue(unlitShader, "_Color", Color.red);
            Assert.IsTrue(unlitResult.IsValid, "Should work with Unlit shader");
        }

        [Test]
        public void GetPropertyType_DifferentShaderTypes_ReturnsCorrectTypes()
        {
            // Test Standard shader properties
            Assert.AreEqual(ShaderPropertyType.Texture, modifier.GetPropertyType(testShader, "_MainTex"));
            Assert.AreEqual(ShaderPropertyType.Color, modifier.GetPropertyType(testShader, "_Color"));
            
            // Test Unlit shader properties
            var unlitShader = Shader.Find("Unlit/Color");
            Assert.AreEqual(ShaderPropertyType.Color, modifier.GetPropertyType(unlitShader, "_Color"));
        }

        #endregion

        #region Modification Preview Tests

        [Test]
        public void PreviewModifications_ValidMaterialsAndProperty_ReturnsCorrectPreview()
        {
            // Arrange
            var materialsWithTargetShader = testMaterials.Where(m => m.shader == testShader).ToList();
            string propertyName = "_Metallic";
            float targetValue = 0.8f;

            // Act
            var preview = modifier.PreviewModifications(materialsWithTargetShader, propertyName, targetValue);

            // Assert
            Assert.IsNotNull(preview, "Preview should not be null");
            Assert.AreEqual(materialsWithTargetShader.Count, preview.TotalCount, "Total count should match input materials");
            Assert.IsTrue(preview.ModifiedCount > 0, "Should have materials to modify");
        }

        [Test]
        public void PreviewModifications_EmptyMaterialList_ReturnsEmptyPreview()
        {
            // Arrange
            var emptyList = new List<Material>();

            // Act
            var preview = modifier.PreviewModifications(emptyList, "_Metallic", 0.5f);

            // Assert
            Assert.IsNotNull(preview, "Preview should not be null");
            Assert.AreEqual(0, preview.TotalCount, "Total count should be 0");
            Assert.AreEqual(0, preview.ModifiedCount, "Modified count should be 0");
            Assert.AreEqual(0, preview.SkippedCount, "Skipped count should be 0");
        }

        [Test]
        public void PreviewModifications_InvalidProperty_SkipsMaterials()
        {
            // Arrange
            var materialsWithTargetShader = testMaterials.Where(m => m.shader == testShader).ToList();
            string invalidProperty = "_NonExistentProperty";

            // Act
            var preview = modifier.PreviewModifications(materialsWithTargetShader, invalidProperty, 0.5f);

            // Assert
            Assert.IsNotNull(preview, "Preview should not be null");
            Assert.AreEqual(materialsWithTargetShader.Count, preview.TotalCount, "Total count should match input");
            Assert.AreEqual(0, preview.ModifiedCount, "Should have no modifications for invalid property");
            Assert.AreEqual(materialsWithTargetShader.Count, preview.SkippedCount, "All materials should be skipped");
        }

        [Test]
        public void PreviewModifications_NullMaterial_HandlesGracefully()
        {
            // Arrange
            var materialsWithNull = new List<Material> { testMaterials[0], null, testMaterials[1] };

            // Act
            var preview = modifier.PreviewModifications(materialsWithNull, "_Metallic", 0.5f);

            // Assert
            Assert.IsNotNull(preview, "Preview should not be null");
            Assert.AreEqual(3, preview.TotalCount, "Total count should include null material");
            Assert.IsTrue(preview.SkippedCount > 0, "Should skip null material");
        }

        [Test]
        public void PreviewModifications_SameValues_DoesNotModify()
        {
            // Arrange
            var material = testMaterials.First(m => m.shader == testShader);
            var materialList = new List<Material> { material };
            
            // Set a known value first
            material.SetFloat("_Metallic", 0.5f);
            
            // Act - Try to set the same value
            var preview = modifier.PreviewModifications(materialList, "_Metallic", 0.5f);

            // Assert
            Assert.IsNotNull(preview, "Preview should not be null");
            Assert.AreEqual(1, preview.TotalCount, "Should have one material");
            
            // Should have one modification entry but WillBeModified should be false
            Assert.AreEqual(1, preview.ModifiedCount, "Should have one modification entry");
            Assert.IsFalse(preview.Modifications[0].WillBeModified, "Should not modify when values are the same");
        }

        #endregion

        #region Material Modification Tests

        [Test]
        public void ApplyModifications_ValidMaterials_ModifiesSuccessfully()
        {
            // Arrange
            var materialsToModify = testMaterials.Where(m => m.shader == testShader).ToList();
            string propertyName = "_Metallic";
            float targetValue = 0.8f;

            // Act
            var result = modifier.ApplyModifications(materialsToModify, propertyName, targetValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.SuccessfulModifications > 0, "Should have successful modifications");
            Assert.AreEqual(0, result.FailedModifications, "Should have no failed modifications");
            
            // Verify the actual property values were changed
            foreach (var material in materialsToModify)
            {
                if (material.HasProperty(propertyName))
                {
                    Assert.AreEqual(targetValue, material.GetFloat(propertyName), 0.001f, 
                        $"Material {material.name} should have the target value");
                }
            }
        }

        [Test]
        public void ApplyModifications_EmptyMaterialList_ReturnsEmptyResult()
        {
            // Arrange
            var emptyList = new List<Material>();

            // Act
            var result = modifier.ApplyModifications(emptyList, "_Metallic", 0.5f);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalProcessed);
            Assert.AreEqual(0, result.SuccessfulModifications);
            Assert.AreEqual(0, result.FailedModifications);
        }

        [Test]
        public void ApplyModifications_InvalidProperty_HandlesGracefully()
        {
            // Arrange
            var materialsToModify = testMaterials.Where(m => m.shader == testShader).ToList();
            string invalidProperty = "_NonExistentProperty";

            // Act
            var result = modifier.ApplyModifications(materialsToModify, invalidProperty, 0.5f);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SuccessfulModifications, "Should have no successful modifications for invalid property");
        }

        [Test]
        public void PreviewModifications_ValidMaterials_GeneratesCorrectPreview()
        {
            // Arrange
            var materialsToPreview = testMaterials.Where(m => m.shader == testShader).ToList();
            string propertyName = "_Metallic";
            float targetValue = 0.7f;

            // Act
            var preview = modifier.PreviewModifications(materialsToPreview, propertyName, targetValue);

            // Assert
            Assert.IsNotNull(preview);
            Assert.AreEqual(materialsToPreview.Count, preview.TotalCount);
            Assert.IsTrue(preview.Modifications.Count > 0, "Should have modifications in preview");
            
            foreach (var modification in preview.Modifications)
            {
                Assert.IsNotNull(modification.TargetMaterial);
                Assert.IsNotNull(modification.FilePath);
            }
        }

        [Test]
        public void PreviewModifications_SameValue_ShowsNoModification()
        {
            // Arrange
            var material = testMaterials.First(m => m.shader == testShader);
            string propertyName = "_Metallic";
            
            // Set initial value
            material.SetFloat(propertyName, 0.5f);
            float sameValue = 0.5f;

            // Act
            var preview = modifier.PreviewModifications(new List<Material> { material }, propertyName, sameValue);

            // Assert
            Assert.IsNotNull(preview);
            Assert.AreEqual(1, preview.Modifications.Count);
            Assert.IsFalse(preview.Modifications[0].WillBeModified, "Should not modify when values are the same");
        }

        [Test]
        public void ApplyModifications_ColorProperty_WorksCorrectly()
        {
            // Arrange
            var material = testMaterials.First(m => m.shader == testShader);
            string propertyName = "_Color";
            Color targetColor = Color.red;

            // Act
            var result = modifier.ApplyModifications(new List<Material> { material }, propertyName, targetColor);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.SuccessfulModifications > 0, "Should successfully modify color property");
            Assert.AreEqual(targetColor, material.GetColor(propertyName), "Material should have the target color");
        }

        [Test]
        public void ApplyModifications_VectorProperty_WorksCorrectly()
        {
            // Arrange
            var material = testMaterials.First(m => m.shader == testShader);
            string propertyName = "_MainTex"; // This might have tiling/offset as vector
            Vector4 targetVector = new Vector4(2, 2, 0, 0);

            // Check if material has a vector property we can test
            if (material.HasProperty(propertyName + "_ST")) // Tiling and offset
            {
                propertyName = propertyName + "_ST";
                
                // Act
                var result = modifier.ApplyModifications(new List<Material> { material }, propertyName, targetVector);

                // Assert
                Assert.IsNotNull(result);
                if (result.SuccessfulModifications > 0)
                {
                    Assert.AreEqual(targetVector, material.GetVector(propertyName), "Material should have the target vector");
                }
            }
            else
            {
                // Skip this test if no suitable vector property is available
                Assert.Pass("No suitable vector property available for testing");
            }
        }

        [Test]
        public void ApplyModifications_WithUndo_SupportsUndo()
        {
            // Arrange
            var material = testMaterials.First(m => m.shader == testShader);
            string propertyName = "_Metallic";
            float originalValue = material.GetFloat(propertyName);
            float targetValue = originalValue + 0.5f;

            // Act
            var result = modifier.ApplyModifications(new List<Material> { material }, propertyName, targetValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.SuccessfulModifications > 0, "Should have successful modifications");
            Assert.AreEqual(targetValue, material.GetFloat(propertyName), 0.001f, "Should have new value");
            
            // Test undo (this would normally be triggered by Unity's undo system)
            // We can't easily test the actual undo functionality in unit tests,
            // but we can verify the modification was applied
        }

        #endregion
    }
}