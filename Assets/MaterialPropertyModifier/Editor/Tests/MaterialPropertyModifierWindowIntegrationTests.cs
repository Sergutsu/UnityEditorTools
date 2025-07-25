using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Integration tests for MaterialPropertyModifierWindow UI functionality
    /// Tests window behavior, user interactions, and UI-to-core logic integration
    /// </summary>
    public class MaterialPropertyModifierWindowIntegrationTests
    {
        private MaterialPropertyModifierWindow window;
        private string testFolderPath;
        private List<Material> testMaterials;

        [SetUp]
        public void SetUp()
        {
            // Create test window
            window = EditorWindow.GetWindow<MaterialPropertyModifierWindow>("Test Window");
            
            testFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/WindowTestMaterials";
            testMaterials = new List<Material>();
            
            CreateTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupTestEnvironment();
            
            if (window != null)
            {
                window.Close();
            }
        }

        private void CreateTestEnvironment()
        {
            // Create test folder
            if (!AssetDatabase.IsValidFolder(testFolderPath))
            {
                string parentFolder = "Assets/MaterialPropertyModifier/Editor/Tests";
                AssetDatabase.CreateFolder(parentFolder, "WindowTestMaterials");
            }

            // Create test materials
            var standardShader = Shader.Find("Standard");
            for (int i = 0; i < 3; i++)
            {
                var material = new Material(standardShader);
                material.name = $"WindowTest_{i}";
                material.SetFloat("_Metallic", 0.0f);
                
                string assetPath = $"{testFolderPath}/WindowTest_{i}.mat";
                AssetDatabase.CreateAsset(material, assetPath);
                testMaterials.Add(material);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CleanupTestEnvironment()
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
            AssetDatabase.Refresh();
        }

        #region Window Initialization Tests

        [Test]
        public void WindowIntegrationTest_WindowInitialization_Success()
        {
            // Act - Window should initialize without errors
            window.Show();
            
            // Assert - Window should be created and visible
            Assert.IsNotNull(window, "Window should be created");
            Assert.IsTrue(window.hasFocus || window.docked, "Window should be visible or docked");
        }

        [Test]
        public void WindowIntegrationTest_SystemHealthCheck_MenuIntegration()
        {
            // This test validates that the menu integration works
            // Act - Simulate menu item execution
            MaterialPropertyModifierWindow.PerformSystemHealthCheck();
            
            // Assert - No exceptions should be thrown
            // The actual dialog display is tested through the core logic
            Assert.Pass("System health check menu item executed without errors");
        }

        #endregion

        #region UI Component Integration Tests

        [Test]
        public void WindowIntegrationTest_FolderSelection_ValidFolder()
        {
            // This test simulates folder selection through the UI
            // Note: Direct UI interaction testing in Unity is limited, so we test the underlying logic
            
            var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(testFolderPath);
            Assert.IsNotNull(folderAsset, "Test folder should exist as asset");
            
            // Verify the folder can be used for material discovery
            var modifier = new MaterialPropertyModifier();
            var materials = modifier.FindMaterialsWithShader(testFolderPath, Shader.Find("Standard"));
            Assert.Greater(materials.Count, 0, "Should find materials in selected folder");
        }

        [Test]
        public void WindowIntegrationTest_ShaderSelection_ValidShader()
        {
            // Test shader selection and property detection
            var standardShader = Shader.Find("Standard");
            Assert.IsNotNull(standardShader, "Standard shader should be available");
            
            // Test property validation for common Standard shader properties
            var modifier = new MaterialPropertyModifier();
            var properties = new[] { "_Color", "_MainTex", "_Metallic", "_Glossiness" };
            
            foreach (var property in properties)
            {
                var validation = modifier.ValidateProperty(standardShader, property);
                Assert.IsTrue(validation.IsValid, $"Standard shader should have {property} property");
            }
        }

        [Test]
        public void WindowIntegrationTest_PropertyConfiguration_DifferentTypes()
        {
            // Test property configuration for different property types
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            
            var testCases = new[]
            {
                new { Property = "_Metallic", Value = (object)0.8f, Type = ShaderPropertyType.Float },
                new { Property = "_Color", Value = (object)Color.red, Type = ShaderPropertyType.Color },
                new { Property = "_MainTex", Value = (object)null, Type = ShaderPropertyType.Texture }
            };

            foreach (var testCase in testCases)
            {
                var propertyValidation = modifier.ValidateProperty(shader, testCase.Property);
                Assert.IsTrue(propertyValidation.IsValid, $"Property {testCase.Property} should be valid");
                Assert.AreEqual(testCase.Type, propertyValidation.PropertyType, $"Property {testCase.Property} should have correct type");
                
                var valueValidation = modifier.ValidatePropertyValue(testCase.Type, testCase.Value);
                Assert.IsTrue(valueValidation.IsValid, $"Value for {testCase.Property} should be valid");
            }
        }

        #endregion

        #region Operation Integration Tests

        [Test]
        public void WindowIntegrationTest_MaterialDiscovery_Integration()
        {
            // Test the complete material discovery workflow as it would be used by the window
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            
            // Act - Perform discovery as the window would
            var discoveryResult = modifier.FindMaterialsWithShaderEnhanced(testFolderPath, shader);
            
            // Assert
            Assert.IsTrue(discoveryResult.IsSuccess, "Material discovery should succeed");
            Assert.AreEqual(testMaterials.Count, discoveryResult.Materials.Count, "Should find all test materials");
            
            // Verify material paths are correct
            var materialPaths = modifier.GetMaterialAssetPaths(discoveryResult.Materials);
            Assert.AreEqual(discoveryResult.Materials.Count, materialPaths.Count, "Should get paths for all materials");
            
            foreach (var kvp in materialPaths)
            {
                Assert.IsTrue(kvp.Value.StartsWith(testFolderPath), "Material paths should be in test folder");
            }
        }

        [Test]
        public void WindowIntegrationTest_PreviewGeneration_Integration()
        {
            // Test preview generation as it would be used by the window
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.7f;

            // Discover materials first
            var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
            
            // Generate preview
            var modificationData = new MaterialModificationData
            {
                Materials = materials,
                PropertyName = propertyName,
                TargetValue = targetValue,
                Shader = shader
            };

            var previewResult = modifier.PreviewModificationsEnhanced(modificationData);
            
            // Assert
            Assert.IsTrue(previewResult.IsSuccess, "Preview generation should succeed");
            Assert.IsNotNull(previewResult.Preview, "Preview data should be available");
            Assert.Greater(previewResult.Preview.MaterialsToModify.Count, 0, "Should have materials to modify");
            Assert.AreEqual(materials.Count, previewResult.Preview.TotalMaterials, "Total materials should match");
        }

        [Test]
        public void WindowIntegrationTest_ModificationApplication_Integration()
        {
            // Test modification application as it would be used by the window
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float originalValue = 0.0f;
            float targetValue = 0.9f;

            // Set known initial values
            foreach (var material in testMaterials)
            {
                material.SetFloat(propertyName, originalValue);
            }
            AssetDatabase.SaveAssets();

            // Discover materials and apply modifications
            var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
            var modificationData = new MaterialModificationData
            {
                Materials = materials,
                PropertyName = propertyName,
                TargetValue = targetValue,
                Shader = shader
            };

            var modificationResult = modifier.ApplyModificationsEnhanced(modificationData);
            
            // Assert
            Assert.IsTrue(modificationResult.IsSuccess, "Modification should succeed");
            Assert.Greater(modificationResult.Result.SuccessfulModifications.Count, 0, "Should have successful modifications");
            
            // Verify actual material changes
            foreach (var material in testMaterials)
            {
                float actualValue = material.GetFloat(propertyName);
                Assert.AreEqual(targetValue, actualValue, 0.001f, $"Material {material.name} should have updated value");
            }
        }

        #endregion

        #region Error Handling Integration Tests

        [Test]
        public void WindowIntegrationTest_ErrorHandling_InvalidFolder()
        {
            // Test error handling for invalid folder selection
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            
            // Act - Try to discover materials in invalid folder
            var discoveryResult = modifier.FindMaterialsWithShaderEnhanced("Assets/NonExistentFolder", shader);
            
            // Assert
            Assert.IsFalse(discoveryResult.IsSuccess, "Discovery should fail for invalid folder");
            Assert.IsNotEmpty(discoveryResult.ErrorMessage, "Should have error message");
            Assert.AreEqual(0, discoveryResult.Materials.Count, "Should find no materials");
        }

        [Test]
        public void WindowIntegrationTest_ErrorHandling_InvalidProperty()
        {
            // Test error handling for invalid property configuration
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
            
            var modificationData = new MaterialModificationData
            {
                Materials = materials,
                PropertyName = "_NonExistentProperty",
                TargetValue = 1.0f,
                Shader = shader
            };

            // Act - Try to apply modifications with invalid property
            var modificationResult = modifier.ApplyModificationsEnhanced(modificationData);
            
            // Assert
            Assert.IsFalse(modificationResult.IsSuccess, "Modification should fail for invalid property");
            Assert.IsNotEmpty(modificationResult.ErrorMessage, "Should have error message");
        }

        [Test]
        public void WindowIntegrationTest_ErrorHandling_IncompatibleValue()
        {
            // Test error handling for incompatible property values
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
            
            var modificationData = new MaterialModificationData
            {
                Materials = materials,
                PropertyName = "_Metallic", // Float property
                TargetValue = "invalid_string_value", // String value for float property
                Shader = shader
            };

            // Act - Try to apply modifications with incompatible value
            var modificationResult = modifier.ApplyModificationsEnhanced(modificationData);
            
            // Assert
            Assert.IsFalse(modificationResult.IsSuccess, "Modification should fail for incompatible value");
            Assert.IsNotEmpty(modificationResult.ErrorMessage, "Should have error message");
        }

        #endregion

        #region Performance Integration Tests

        [Test]
        public void WindowIntegrationTest_Performance_LargeOperations()
        {
            // Test performance with larger operations as they would be used by the window
            var modifier = new MaterialPropertyModifier();
            var shader = Shader.Find("Standard");
            
            // Create additional materials for performance testing
            var performanceMaterials = new List<Material>();
            string perfTestFolder = "Assets/MaterialPropertyModifier/Editor/Tests/PerfTest";
            
            try
            {
                if (!AssetDatabase.IsValidFolder(perfTestFolder))
                {
                    AssetDatabase.CreateFolder("Assets/MaterialPropertyModifier/Editor/Tests", "PerfTest");
                }

                // Create 20 materials for performance testing
                for (int i = 0; i < 20; i++)
                {
                    var material = new Material(shader);
                    material.name = $"PerfTest_{i}";
                    string assetPath = $"{perfTestFolder}/PerfTest_{i}.mat";
                    AssetDatabase.CreateAsset(material, assetPath);
                    performanceMaterials.Add(material);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Act - Perform complete workflow
                var startTime = System.DateTime.Now;
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    perfTestFolder, shader, "_Metallic", 0.5f, previewOnly: false);
                var executionTime = System.DateTime.Now - startTime;

                // Assert
                Assert.IsTrue(workflowResult.IsSuccess, "Performance test workflow should succeed");
                Assert.Less(executionTime.TotalSeconds, 10, "Should complete within 10 seconds");
                
                var workflow = workflowResult.Data;
                Assert.AreEqual(20, workflow.MaterialCount, "Should process all 20 materials");
            }
            finally
            {
                // Cleanup
                foreach (var material in performanceMaterials)
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
                
                if (AssetDatabase.IsValidFolder(perfTestFolder))
                {
                    AssetDatabase.DeleteAsset(perfTestFolder);
                }
            }
        }

        #endregion

        #region User Workflow Simulation Tests

        [Test]
        public void WindowIntegrationTest_TypicalUserWorkflow_Complete()
        {
            // Simulate a complete typical user workflow
            var modifier = new MaterialPropertyModifier();
            
            // Step 1: User selects folder and shader
            var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(testFolderPath);
            var shader = Shader.Find("Standard");
            Assert.IsNotNull(folderAsset, "User should be able to select folder");
            Assert.IsNotNull(shader, "User should be able to select shader");

            // Step 2: User configures property
            string propertyName = "_Metallic";
            float targetValue = 0.6f;
            
            var propertyValidation = modifier.ValidateProperty(shader, propertyName);
            Assert.IsTrue(propertyValidation.IsValid, "Property should be valid for selected shader");
            
            var valueValidation = modifier.ValidatePropertyValue(propertyValidation.PropertyType, targetValue);
            Assert.IsTrue(valueValidation.IsValid, "Value should be valid for property type");

            // Step 3: User finds materials
            var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
            Assert.Greater(materials.Count, 0, "User should find materials");

            // Step 4: User generates preview
            var modificationData = new MaterialModificationData
            {
                Materials = materials,
                PropertyName = propertyName,
                TargetValue = targetValue,
                Shader = shader
            };

            var previewResult = modifier.PreviewModificationsEnhanced(modificationData);
            Assert.IsTrue(previewResult.IsSuccess, "Preview should be generated successfully");
            Assert.Greater(previewResult.Preview.MaterialsToModify.Count, 0, "Preview should show materials to modify");

            // Step 5: User applies modifications
            var modificationResult = modifier.ApplyModificationsEnhanced(modificationData);
            Assert.IsTrue(modificationResult.IsSuccess, "Modifications should be applied successfully");
            Assert.Greater(modificationResult.Result.SuccessfulModifications.Count, 0, "Should have successful modifications");

            // Step 6: Verify results
            foreach (var material in materials)
            {
                if (material.HasProperty(propertyName))
                {
                    float actualValue = material.GetFloat(propertyName);
                    Assert.AreEqual(targetValue, actualValue, 0.001f, $"Material {material.name} should have correct final value");
                }
            }
        }

        #endregion
    }
}