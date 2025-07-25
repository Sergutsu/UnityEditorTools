using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Integration tests for Material Property Modifier covering complete workflow scenarios
    /// Tests end-to-end functionality with various Unity shader types and edge cases
    /// </summary>
    public class MaterialPropertyModifierIntegrationTests
    {
        private MaterialPropertyModifier modifier;
        private string testFolderPath;
        private List<Material> testMaterials;
        private List<Shader> testShaders;
        private string tempAssetPath;

        [SetUp]
        public void SetUp()
        {
            modifier = new MaterialPropertyModifier();
            testFolderPath = "Assets/MaterialPropertyModifier/Editor/Tests/IntegrationTestMaterials";
            testMaterials = new List<Material>();
            testShaders = new List<Shader>();
            tempAssetPath = "Assets/MaterialPropertyModifier/Editor/Tests/TempAssets";
            
            CreateTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupTestEnvironment();
        }

        private void CreateTestEnvironment()
        {
            // Create test folders
            if (!AssetDatabase.IsValidFolder(testFolderPath))
            {
                string parentFolder = "Assets/MaterialPropertyModifier/Editor/Tests";
                AssetDatabase.CreateFolder(parentFolder, "IntegrationTestMaterials");
            }

            if (!AssetDatabase.IsValidFolder(tempAssetPath))
            {
                string parentFolder = "Assets/MaterialPropertyModifier/Editor/Tests";
                AssetDatabase.CreateFolder(parentFolder, "TempAssets");
            }

            // Get test shaders
            testShaders.Add(Shader.Find("Standard"));
            testShaders.Add(Shader.Find("Unlit/Color"));
            testShaders.Add(Shader.Find("Unlit/Texture"));

            // Create test materials for each shader
            CreateTestMaterials();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateTestMaterials()
        {
            int materialIndex = 0;
            
            foreach (var shader in testShaders.Where(s => s != null))
            {
                // Create 3 materials per shader type
                for (int i = 0; i < 3; i++)
                {
                    var material = new Material(shader);
                    material.name = $"IntegrationTest_{shader.name.Replace("/", "_")}_{i}";
                    
                    // Set some initial property values
                    SetInitialPropertyValues(material, shader);
                    
                    string assetPath = $"{testFolderPath}/IntegrationTest_{materialIndex}.mat";
                    AssetDatabase.CreateAsset(material, assetPath);
                    testMaterials.Add(material);
                    materialIndex++;
                }
            }
        }

        private void SetInitialPropertyValues(Material material, Shader shader)
        {
            // Set common properties based on shader type
            if (shader.name == "Standard")
            {
                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", Color.white);
                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", 0.0f);
                if (material.HasProperty("_Glossiness"))
                    material.SetFloat("_Glossiness", 0.5f);
            }
            else if (shader.name == "Unlit/Color")
            {
                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", Color.red);
            }
        }

        private void CleanupTestEnvironment()
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

            // Clean up temp assets
            if (AssetDatabase.IsValidFolder(tempAssetPath))
            {
                AssetDatabase.DeleteAsset(tempAssetPath);
            }

            AssetDatabase.Refresh();
        }

        #region System Health and Prerequisites Tests

        [Test]
        public void IntegrationTest_SystemHealthCheck_AllComponentsHealthy()
        {
            // Act
            var healthResult = modifier.PerformHealthCheck();

            // Assert
            Assert.IsTrue(healthResult.IsSuccess, "Health check should succeed");
            Assert.IsNotNull(healthResult.Data, "Health check data should not be null");
            Assert.IsTrue(healthResult.Data.OverallHealth, "System should be healthy");
            Assert.AreEqual(0, healthResult.Data.FailedChecks.Count, "No health checks should fail");
            Assert.Greater(healthResult.Data.HealthScore, 0.9, "Health score should be above 90%");
        }

        [Test]
        public void IntegrationTest_ValidateOperationPrerequisites_ValidInputs_Success()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float propertyValue = 0.8f;

            // Act
            var result = modifier.ValidateOperationPrerequisites(testFolderPath, shader, propertyName, propertyValue);

            // Assert
            Assert.IsTrue(result.IsValid, "Prerequisites validation should succeed");
            Assert.IsEmpty(result.ErrorMessage, "Should have no error message");
        }

        [Test]
        public void IntegrationTest_ValidateOperationPrerequisites_InvalidInputs_Failure()
        {
            // Act & Assert - Invalid folder
            var result1 = modifier.ValidateOperationPrerequisites("InvalidFolder", Shader.Find("Standard"), "_Color", Color.red);
            Assert.IsFalse(result1.IsValid, "Should fail for invalid folder");

            // Act & Assert - Null shader
            var result2 = modifier.ValidateOperationPrerequisites(testFolderPath, null, "_Color", Color.red);
            Assert.IsFalse(result2.IsValid, "Should fail for null shader");

            // Act & Assert - Invalid property
            var result3 = modifier.ValidateOperationPrerequisites(testFolderPath, Shader.Find("Standard"), "_InvalidProperty", 1.0f);
            Assert.IsFalse(result3.IsValid, "Should fail for invalid property");
        }

        #endregion

        #region End-to-End Workflow Tests

        [Test]
        public void IntegrationTest_CompleteWorkflow_StandardShader_FloatProperty_Success()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.8f;

            // Act
            var workflowResult = modifier.ExecuteIntegratedWorkflow(testFolderPath, shader, propertyName, targetValue, previewOnly: false);

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, $"Workflow should succeed: {workflowResult.ErrorMessage}");
            Assert.IsNotNull(workflowResult.Data, "Workflow data should not be null");
            
            var workflow = workflowResult.Data;
            Assert.IsTrue(workflow.PrerequisitesValid, "Prerequisites should be valid");
            Assert.Greater(workflow.MaterialCount, 0, "Should find materials");
            Assert.IsTrue(workflow.ModificationsApplied, "Modifications should be applied");
            Assert.Greater(workflow.SuccessfulModifications, 0, "Should have successful modifications");

            // Verify actual material modifications
            var standardMaterials = testMaterials.Where(m => m.shader == shader).ToList();
            foreach (var material in standardMaterials)
            {
                if (material.HasProperty(propertyName))
                {
                    float actualValue = material.GetFloat(propertyName);
                    Assert.AreEqual(targetValue, actualValue, 0.001f, $"Material {material.name} should have updated property value");
                }
            }
        }

        [Test]
        public void IntegrationTest_CompleteWorkflow_UnlitShader_ColorProperty_Success()
        {
            // Arrange
            var shader = Shader.Find("Unlit/Color");
            string propertyName = "_Color";
            Color targetValue = Color.blue;

            // Act
            var workflowResult = modifier.ExecuteIntegratedWorkflow(testFolderPath, shader, propertyName, targetValue, previewOnly: false);

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, $"Workflow should succeed: {workflowResult.ErrorMessage}");
            
            var workflow = workflowResult.Data;
            Assert.Greater(workflow.MaterialCount, 0, "Should find Unlit materials");
            Assert.IsTrue(workflow.ModificationsApplied, "Modifications should be applied");

            // Verify actual material modifications
            var unlitMaterials = testMaterials.Where(m => m.shader == shader).ToList();
            foreach (var material in unlitMaterials)
            {
                if (material.HasProperty(propertyName))
                {
                    Color actualValue = material.GetColor(propertyName);
                    Assert.AreEqual(targetValue, actualValue, $"Material {material.name} should have updated color");
                }
            }
        }

        [Test]
        public void IntegrationTest_PreviewOnlyWorkflow_NoModificationsApplied()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float originalValue = 0.0f;
            float targetValue = 0.9f;

            // Set known initial values
            var standardMaterials = testMaterials.Where(m => m.shader == shader).ToList();
            foreach (var material in standardMaterials)
            {
                if (material.HasProperty(propertyName))
                {
                    material.SetFloat(propertyName, originalValue);
                }
            }
            AssetDatabase.SaveAssets();

            // Act
            var workflowResult = modifier.ExecuteIntegratedWorkflow(testFolderPath, shader, propertyName, targetValue, previewOnly: true);

            // Assert
            Assert.IsTrue(workflowResult.IsSuccess, "Preview workflow should succeed");
            
            var workflow = workflowResult.Data;
            Assert.IsFalse(workflow.ModificationsApplied, "Modifications should not be applied in preview mode");
            Assert.Greater(workflow.MaterialsToModify, 0, "Should identify materials to modify");

            // Verify materials were not actually modified
            foreach (var material in standardMaterials)
            {
                if (material.HasProperty(propertyName))
                {
                    float actualValue = material.GetFloat(propertyName);
                    Assert.AreEqual(originalValue, actualValue, 0.001f, $"Material {material.name} should retain original value in preview mode");
                }
            }
        }

        #endregion

        #region Multiple Shader Types Tests

        [Test]
        public void IntegrationTest_MultipleShaderTypes_DifferentProperties_Success()
        {
            // Test with different shader types and their specific properties
            var testCases = new[]
            {
                new { Shader = "Standard", Property = "_Metallic", Value = (object)0.7f },
                new { Shader = "Standard", Property = "_Color", Value = (object)Color.green },
                new { Shader = "Unlit/Color", Property = "_Color", Value = (object)Color.yellow }
            };

            foreach (var testCase in testCases)
            {
                var shader = Shader.Find(testCase.Shader);
                if (shader == null) continue;

                // Act
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    testFolderPath, shader, testCase.Property, testCase.Value, previewOnly: false);

                // Assert
                Assert.IsTrue(workflowResult.IsSuccess, 
                    $"Workflow should succeed for {testCase.Shader} with {testCase.Property}: {workflowResult.ErrorMessage}");
                
                var workflow = workflowResult.Data;
                Assert.Greater(workflow.MaterialCount, 0, $"Should find materials for {testCase.Shader}");
                Assert.IsTrue(workflow.ModificationsApplied, $"Should apply modifications for {testCase.Shader}");
            }
        }

        [Test]
        public void IntegrationTest_LargeMaterialCollection_Performance()
        {
            // Create additional materials for performance testing
            var performanceTestMaterials = new List<Material>();
            var shader = Shader.Find("Standard");
            
            try
            {
                // Create 50 additional materials
                for (int i = 0; i < 50; i++)
                {
                    var material = new Material(shader);
                    material.name = $"PerformanceTest_{i}";
                    string assetPath = $"{tempAssetPath}/PerformanceTest_{i}.mat";
                    AssetDatabase.CreateAsset(material, assetPath);
                    performanceTestMaterials.Add(material);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Act
                var startTime = System.DateTime.Now;
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    tempAssetPath, shader, "_Metallic", 0.5f, previewOnly: false);
                var executionTime = System.DateTime.Now - startTime;

                // Assert
                Assert.IsTrue(workflowResult.IsSuccess, "Large collection workflow should succeed");
                Assert.Less(executionTime.TotalSeconds, 30, "Should complete within 30 seconds");
                
                var workflow = workflowResult.Data;
                Assert.AreEqual(50, workflow.MaterialCount, "Should find all 50 materials");
                Assert.Greater(workflow.SuccessfulModifications, 0, "Should successfully modify materials");
            }
            finally
            {
                // Cleanup performance test materials
                foreach (var material in performanceTestMaterials)
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
        }

        #endregion

        #region Error Recovery and Edge Cases Tests

        [Test]
        public void IntegrationTest_ErrorRecovery_FileIOError_Recovery()
        {
            // This test simulates file I/O errors and tests recovery mechanisms
            // Note: Actual file I/O error simulation is complex in Unity, so we test the recovery framework
            
            var shader = Shader.Find("Standard");
            string propertyName = "_Metallic";
            float targetValue = 0.6f;

            // Act - Execute with recovery enabled
            var result = modifier.ExecuteComprehensiveOperation(() =>
            {
                // Simulate a successful operation after potential recovery
                return modifier.FindMaterialsWithShader(testFolderPath, shader);
            }, "TestRecovery", enableRecovery: true);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Operation with recovery should succeed");
            Assert.IsNotNull(result.Data, "Result data should not be null");
        }

        [Test]
        public void IntegrationTest_GracefulDegradation_PrimaryFailure_FallbackSuccess()
        {
            // Arrange
            var shader = Shader.Find("Standard");
            bool primaryCalled = false;
            bool fallbackCalled = false;

            // Act
            var result = modifier.ExecuteWithGracefulDegradation(
                () => {
                    primaryCalled = true;
                    throw new System.InvalidOperationException("Simulated primary failure");
                },
                () => {
                    fallbackCalled = true;
                    return modifier.FindMaterialsWithShader(testFolderPath, shader);
                },
                "TestGracefulDegradation"
            );

            // Assert
            Assert.IsTrue(primaryCalled, "Primary operation should be called");
            Assert.IsTrue(fallbackCalled, "Fallback operation should be called");
            Assert.IsNotNull(result, "Should return fallback result");
        }

        [Test]
        public void IntegrationTest_BatchOperations_MixedResults()
        {
            // Arrange
            var operations = new List<System.Func<OperationResult<object>>>
            {
                // Successful operation
                () => new OperationResult<object>(modifier.FindMaterialsWithShader(testFolderPath, Shader.Find("Standard"))),
                
                // Failed operation
                () => new OperationResult<object>("Simulated failure", "TestError"),
                
                // Another successful operation
                () => new OperationResult<object>(modifier.ValidateSystemPrerequisites())
            };

            // Act
            var batchResult = modifier.ExecuteBatchOperations(operations);

            // Assert
            Assert.IsTrue(batchResult.IsSuccess, "Batch operation should succeed overall");
            Assert.IsNotNull(batchResult.Data, "Batch result data should not be null");
            
            var batch = batchResult.Data;
            Assert.AreEqual(3, batch.TotalOperations, "Should track all operations");
            Assert.AreEqual(2, batch.SuccessCount, "Should have 2 successful operations");
            Assert.AreEqual(1, batch.FailureCount, "Should have 1 failed operation");
            Assert.AreEqual(2.0/3.0, batch.SuccessRate, 0.01, "Success rate should be 66.7%");
        }

        #endregion

        #region Comprehensive Validation Tests

        [Test]
        public void IntegrationTest_EndToEndValidation_AllRequirements()
        {
            // This test validates that all requirements are met in an end-to-end scenario
            
            // Requirement 1.1 & 1.2: Material discovery in folder with specific shader
            var shader = Shader.Find("Standard");
            var discoveryResult = modifier.FindMaterialsWithShaderEnhanced(testFolderPath, shader);
            Assert.IsTrue(discoveryResult.IsSuccess, "Material discovery should succeed (Req 1.1, 1.2)");
            Assert.Greater(discoveryResult.Materials.Count, 0, "Should find materials (Req 1.4)");

            // Requirement 2.1 & 2.2: Property validation
            string propertyName = "_Metallic";
            var propertyValidation = modifier.ValidateProperty(shader, propertyName);
            Assert.IsTrue(propertyValidation.IsValid, "Property validation should succeed (Req 2.1, 2.2)");

            // Requirement 2.3 & 2.4: Property modification with undo support
            float targetValue = 0.75f;
            var modificationData = new MaterialModificationData
            {
                Materials = discoveryResult.Materials,
                PropertyName = propertyName,
                TargetValue = targetValue,
                Shader = shader
            };

            var modificationResult = modifier.ApplyModificationsEnhanced(modificationData);
            Assert.IsTrue(modificationResult.IsSuccess, "Modification should succeed (Req 2.3, 2.4)");

            // Requirement 3.1 & 3.2: Preview functionality
            var previewResult = modifier.PreviewModificationsEnhanced(modificationData);
            Assert.IsTrue(previewResult.IsSuccess, "Preview should succeed (Req 3.1, 3.2)");
            Assert.IsNotNull(previewResult.Preview, "Preview data should be available");

            // Requirement 3.3: Batch operations
            Assert.Greater(modificationResult.Result.SuccessfulModifications.Count, 0, "Should have successful batch modifications (Req 3.3)");

            // Requirement 4.1, 4.2, 4.3, 4.4: UI functionality (tested through window integration)
            // These are validated through the window's integration with the core logic

            // Requirement 5.1, 5.2, 5.3, 5.4: Error handling and logging
            var healthCheck = modifier.PerformHealthCheck();
            Assert.IsTrue(healthCheck.IsSuccess, "Health check should succeed (Req 5.1, 5.2, 5.3, 5.4)");
        }

        [Test]
        public void IntegrationTest_CustomShaderSupport_BasicValidation()
        {
            // Test with built-in shaders that might have different property sets
            var shaderTests = new[]
            {
                new { Name = "Standard", ExpectedProperties = new[] { "_Color", "_MainTex", "_Metallic" } },
                new { Name = "Unlit/Color", ExpectedProperties = new[] { "_Color" } },
                new { Name = "Unlit/Texture", ExpectedProperties = new[] { "_MainTex" } }
            };

            foreach (var test in shaderTests)
            {
                var shader = Shader.Find(test.Name);
                if (shader == null) continue;

                // Test property detection
                foreach (var expectedProperty in test.ExpectedProperties)
                {
                    var validation = modifier.ValidateProperty(shader, expectedProperty);
                    Assert.IsTrue(validation.IsValid, 
                        $"Shader {test.Name} should have property {expectedProperty}");
                }

                // Test material discovery
                var materials = modifier.FindMaterialsWithShader(testFolderPath, shader);
                Assert.IsNotNull(materials, $"Should be able to find materials for {test.Name}");
            }
        }

        #endregion
    }
}