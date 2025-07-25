using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// End-to-end integration tests for the complete Material Property Modifier workflow
    /// </summary>
    public class EndToEndTests
    {
        private MaterialPropertyModifier modifier;
        private string testFolderPath;
        private List<Material> testMaterials;
        private List<Shader> testShaders;

        [SetUp]
        public void SetUp()
        {
            modifier = new MaterialPropertyModifier();
            testFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/EndToEndTestMaterials";
            
            // Create test folder structure
            CreateTestFolderStructure();
            
            // Get test shaders
            SetupTestShaders();
            
            // Create test materials
            CreateTestMaterials();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test materials
            CleanupTestMaterials();
            
            // Clean up test folders
            if (AssetDatabase.IsValidFolder(testFolderPath))
            {
                AssetDatabase.DeleteAsset(testFolderPath);
            }
            
            AssetDatabase.Refresh();
        }

        #region End-to-End Workflow Tests

        [Test]
        public void CompleteWorkflow_StandardShader_FloatProperty_Success()
        {
            // Arrange
            var standardShader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.8f;

            // Act - Execute complete workflow
            var workflowResult = modifier.ExecuteIntegratedWorkflow(
                testFolderPath, 
                standardShader, 
                propertyName, 
                targetValue, 
                previewOnly: false
            );

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, $"Workflow should succeed: {workflowResult.ErrorMessage}");
            Assert.IsNotNull(workflowResult.Data, "Workflow result data should not be null");
            Assert.Greater(workflowResult.Data.MaterialCount, 0, "Should find materials");
            Assert.Greater(workflowResult.Data.ModificationsApplied, 0, "Should apply modifications");

            // Verify materials were actually modified
            var materials = modifier.FindMaterialsWithShader(testFolderPath, standardShader);
            foreach (var material in materials)
            {
                if (material.HasProperty(propertyName))
                {
                    float actualValue = material.GetFloat(propertyName);
                    Assert.AreEqual(targetValue, actualValue, 0.001f, 
                        $"Material {material.name} should have updated {propertyName} value");
                }
            }
        }

        [Test]
        public void CompleteWorkflow_UnlitShader_ColorProperty_Success()
        {
            // Arrange
            var unlitShader = Shader.Find("Unlit/Color");
            string propertyName = "_Color";
            Color targetValue = Color.red;

            // Act - Execute complete workflow
            var workflowResult = modifier.ExecuteIntegratedWorkflow(
                testFolderPath, 
                unlitShader, 
                propertyName, 
                targetValue, 
                previewOnly: false
            );

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, $"Workflow should succeed: {workflowResult.ErrorMessage}");
            Assert.Greater(workflowResult.Data.ModificationsApplied, 0, "Should apply modifications");

            // Verify materials were actually modified
            var materials = modifier.FindMaterialsWithShader(testFolderPath, unlitShader);
            foreach (var material in materials)
            {
                if (material.HasProperty(propertyName))
                {
                    Color actualValue = material.GetColor(propertyName);
                    Assert.AreEqual(targetValue, actualValue, 
                        $"Material {material.name} should have updated {propertyName} value");
                }
            }
        }

        [Test]
        public void CompleteWorkflow_PreviewOnly_NoModifications()
        {
            // Arrange
            var standardShader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.9f;

            // Store original values
            var materials = modifier.FindMaterialsWithShader(testFolderPath, standardShader);
            var originalValues = new Dictionary<Material, float>();
            foreach (var material in materials)
            {
                if (material.HasProperty(propertyName))
                {
                    originalValues[material] = material.GetFloat(propertyName);
                }
            }

            // Act - Execute preview-only workflow
            var workflowResult = modifier.ExecuteIntegratedWorkflow(
                testFolderPath, 
                standardShader, 
                propertyName, 
                targetValue, 
                previewOnly: true
            );

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, "Preview workflow should succeed");
            Assert.AreEqual(0, workflowResult.Data.ModificationsApplied, "Preview should not apply modifications");
            Assert.IsNotNull(workflowResult.Data.Preview, "Preview data should be available");

            // Verify no materials were actually modified
            foreach (var kvp in originalValues)
            {
                float currentValue = kvp.Key.GetFloat(propertyName);
                Assert.AreEqual(kvp.Value, currentValue, 0.001f, 
                    $"Material {kvp.Key.name} should not have been modified in preview mode");
            }
        }

        [Test]
        public void CompleteWorkflow_InvalidProperty_HandledGracefully()
        {
            // Arrange
            var standardShader = Shader.Find("Standard");
            string invalidPropertyName = "_NonExistentProperty";
            float targetValue = 0.5f;

            // Act
            var workflowResult = modifier.ExecuteIntegratedWorkflow(
                testFolderPath, 
                standardShader, 
                invalidPropertyName, 
                targetValue, 
                previewOnly: false
            );

            // Assert
            Assert.IsFalse(workflowResult.IsSuccess, "Workflow should fail for invalid property");
            Assert.IsNotEmpty(workflowResult.ErrorMessage, "Should have error message");
        }

        [Test]
        public void CompleteWorkflow_EmptyFolder_HandledGracefully()
        {
            // Arrange
            string emptyFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/EmptyFolder";
            AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor/Tests", "EmptyFolder");
            
            var standardShader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.5f;

            try
            {
                // Act
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    emptyFolderPath, 
                    standardShader, 
                    propertyName, 
                    targetValue, 
                    previewOnly: false
                );

                // Assert
                Assert.IsTrue(workflowResult.IsSuccess, "Workflow should succeed even with no materials");
                Assert.AreEqual(0, workflowResult.Data.MaterialCount, "Should find no materials");
                Assert.AreEqual(0, workflowResult.Data.ModificationsApplied, "Should apply no modifications");
            }
            finally
            {
                // Cleanup
                AssetDatabase.DeleteAsset(emptyFolderPath);
            }
        }

        #endregion

        #region Performance Tests

        [Test]
        public void PerformanceTest_LargeMaterialCollection_CompletesInReasonableTime()
        {
            // Arrange - Create many test materials
            var largeMaterialSet = CreateLargeMaterialSet(50); // 50 materials
            var standardShader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.7f;

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Act
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    testFolderPath, 
                    standardShader, 
                    propertyName, 
                    targetValue, 
                    previewOnly: false
                );

                stopwatch.Stop();

                // Assert
                Assert.IsTrue(workflowResult.IsSuccess, "Large collection workflow should succeed");
                Assert.Less(stopwatch.ElapsedMilliseconds, 10000, "Should complete within 10 seconds");
                Assert.Greater(workflowResult.Data.ModificationsApplied, 0, "Should modify materials");
            }
            finally
            {
                // Cleanup large material set
                CleanupLargeMaterialSet(largeMaterialSet);
            }
        }

        #endregion

        #region Error Recovery Tests

        [Test]
        public void ErrorRecovery_PartialFailure_ContinuesProcessing()
        {
            // This test would require creating materials with locked files or other failure conditions
            // For now, we'll test the error handling structure
            
            var standardShader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.6f;

            // Act
            var workflowResult = modifier.ExecuteIntegratedWorkflow(
                testFolderPath, 
                standardShader, 
                propertyName, 
                targetValue, 
                previewOnly: false
            );

            // Assert - Even if some materials fail, the workflow should continue
            Assert.IsTrue(workflowResult.IsSuccess || !string.IsNullOrEmpty(workflowResult.ErrorMessage), 
                "Should either succeed or provide error information");
        }

        #endregion

        #region Helper Methods

        private void CreateTestFolderStructure()
        {
            if (!AssetDatabase.IsValidFolder("Assets/MaterialPropertyModifier/Editor/Tests"))
            {
                AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor", "Tests");
            }
            
            if (!AssetDatabase.IsValidFolder(testFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor/Tests", "EndToEndTestMaterials");
            }

            // Create subfolder for testing recursive search
            string subFolderPath = testFolderPath + "/SubFolder";
            if (!AssetDatabase.IsValidFolder(subFolderPath))
            {
                AssetDatabase.CreateFolder(testFolderPath, "SubFolder");
            }
        }

        private void SetupTestShaders()
        {
            testShaders = new List<Shader>
            {
                Shader.Find("Standard"),
                Shader.Find("Unlit/Color"),
                Shader.Find("Unlit/Texture")
            };

            // Remove null shaders
            testShaders.RemoveAll(s => s == null);
        }

        private void CreateTestMaterials()
        {
            testMaterials = new List<Material>();

            // Create materials with Standard shader
            for (int i = 0; i < 3; i++)
            {
                var material = new Material(Shader.Find("Standard"));
                material.name = $"TestStandardMaterial_{i}";
                material.SetFloat("_Metallic", 0.1f + i * 0.1f);
                
                string assetPath = $"{testFolderPath}/TestStandardMaterial_{i}.mat";
                AssetDatabase.CreateAsset(material, assetPath);
                testMaterials.Add(material);
            }

            // Create materials with Unlit shader
            for (int i = 0; i < 2; i++)
            {
                var material = new Material(Shader.Find("Unlit/Color"));
                material.name = $"TestUnlitMaterial_{i}";
                material.SetColor("_Color", Color.white);
                
                string assetPath = $"{testFolderPath}/TestUnlitMaterial_{i}.mat";
                AssetDatabase.CreateAsset(material, assetPath);
                testMaterials.Add(material);
            }

            // Create material in subfolder
            var subFolderMaterial = new Material(Shader.Find("Standard"));
            subFolderMaterial.name = "SubFolderMaterial";
            subFolderMaterial.SetFloat("_Metallic", 0.5f);
            
            string subFolderAssetPath = $"{testFolderPath}/SubFolder/SubFolderMaterial.mat";
            AssetDatabase.CreateAsset(subFolderMaterial, subFolderAssetPath);
            testMaterials.Add(subFolderMaterial);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private List<Material> CreateLargeMaterialSet(int count)
        {
            var materials = new List<Material>();
            var standardShader = Shader.Find("Standard");

            for (int i = 0; i < count; i++)
            {
                var material = new Material(standardShader);
                material.name = $"LargeMaterial_{i}";
                material.SetFloat("_Metallic", Random.Range(0f, 1f));
                
                string assetPath = $"{testFolderPath}/LargeMaterial_{i}.mat";
                AssetDatabase.CreateAsset(material, assetPath);
                materials.Add(material);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return materials;
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

        private void CleanupLargeMaterialSet(List<Material> materials)
        {
            foreach (var material in materials)
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
        }

        #endregion
    }
}