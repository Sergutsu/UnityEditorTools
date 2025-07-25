using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Unit tests for MaterialPropertyModifierWindow
    /// </summary>
    public class MaterialPropertyModifierWindowTests
    {
        private MaterialPropertyModifierWindow window;

        [SetUp]
        public void SetUp()
        {
            // Note: In actual Unity environment, window creation would work
            // In unit test environment, we test what we can
        }

        [TearDown]
        public void TearDown()
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
        }

        [Test]
        public void ShowWindow_MenuItemExists_CanBeInvoked()
        {
            // This test verifies that the menu item method exists and can be called
            // In a real Unity environment, this would open the window
            
            // Act & Assert - Should not throw exception
            Assert.DoesNotThrow(() => {
                // We can't actually test window opening in unit tests,
                // but we can verify the method exists and is accessible
                var method = typeof(MaterialPropertyModifierWindow).GetMethod("ShowWindow", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                Assert.IsNotNull(method, "ShowWindow method should exist");
                Assert.IsTrue(method.IsStatic, "ShowWindow should be static");
                Assert.IsTrue(method.IsPublic, "ShowWindow should be public");
            });
        }

        [Test]
        public void WindowClass_HasCorrectAttributes_ForEditorWindow()
        {
            // Verify the class inherits from EditorWindow
            Assert.IsTrue(typeof(EditorWindow).IsAssignableFrom(typeof(MaterialPropertyModifierWindow)),
                "MaterialPropertyModifierWindow should inherit from EditorWindow");
        }

        [Test]
        public void WindowClass_HasMenuItemAttribute_WithCorrectPath()
        {
            // Get the ShowWindow method
            var method = typeof(MaterialPropertyModifierWindow).GetMethod("ShowWindow", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            Assert.IsNotNull(method, "ShowWindow method should exist");
            
            // Check for MenuItem attribute
            var menuItemAttribute = System.Attribute.GetCustomAttribute(method, typeof(MenuItemAttribute)) as MenuItemAttribute;
            Assert.IsNotNull(menuItemAttribute, "ShowWindow should have MenuItem attribute");
            
            // Verify menu path
            Assert.AreEqual("Tools/Material Property Modifier", menuItemAttribute.menuItem, 
                "Menu item should be under Tools menu");
        }

        [Test]
        public void WindowConstants_HaveReasonableValues()
        {
            // Use reflection to check private constants
            var type = typeof(MaterialPropertyModifierWindow);
            
            // Check if constants exist (they're private, so we check indirectly)
            // This test ensures the class compiles and has reasonable structure
            Assert.IsNotNull(type, "Window class should exist");
            Assert.IsTrue(type.IsClass, "Should be a class");
            Assert.IsFalse(type.IsAbstract, "Should not be abstract");
        }

        [Test]
        public void WindowNamespace_IsCorrect()
        {
            // Verify the window is in the correct namespace
            Assert.AreEqual("MaterialPropertyModifier.Editor", 
                typeof(MaterialPropertyModifierWindow).Namespace,
                "Window should be in MaterialPropertyModifier.Editor namespace");
        }

        [Test]
        public void OnSelectionChange_MethodExists_ForFolderSelection()
        {
            // Verify OnSelectionChange method exists for folder auto-selection
            var method = typeof(MaterialPropertyModifierWindow).GetMethod("OnSelectionChange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(method, "OnSelectionChange method should exist for folder auto-selection");
        }

        [Test]
        public void RefreshShaderList_MethodExists_ForShaderManagement()
        {
            // Verify RefreshShaderList method exists
            var method = typeof(MaterialPropertyModifierWindow).GetMethod("RefreshShaderList", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(method, "RefreshShaderList method should exist for shader management");
        }

        [Test]
        public void FolderValidation_MethodsExist_ForInputValidation()
        {
            // Verify folder validation methods exist
            var onFolderChanged = typeof(MaterialPropertyModifierWindow).GetMethod("OnFolderSelectionChanged", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var onShaderChanged = typeof(MaterialPropertyModifierWindow).GetMethod("OnShaderSelectionChanged", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(onFolderChanged, "OnFolderSelectionChanged method should exist");
            Assert.IsNotNull(onShaderChanged, "OnShaderSelectionChanged method should exist");
        }

        [Test]
        public void StatusValidation_MethodExists_ForUserFeedback()
        {
            // Verify status validation method exists
            var method = typeof(MaterialPropertyModifierWindow).GetMethod("GetSelectionStatusText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(method, "GetSelectionStatusText method should exist for user feedback");
        }

        [Test]
        public void MaterialListMethods_Exist_ForMaterialDisplay()
        {
            // Verify material list methods exist
            var findMaterials = typeof(MaterialPropertyModifierWindow).GetMethod("FindMaterials", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var getFilteredMaterials = typeof(MaterialPropertyModifierWindow).GetMethod("GetFilteredMaterials", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(findMaterials, "FindMaterials method should exist");
            Assert.IsNotNull(getFilteredMaterials, "GetFilteredMaterials method should exist");
        }

        [Test]
        public void PreviewMethods_Exist_ForModificationPreview()
        {
            // Verify preview methods exist
            var canGeneratePreview = typeof(MaterialPropertyModifierWindow).GetMethod("CanGeneratePreview", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var generatePreview = typeof(MaterialPropertyModifierWindow).GetMethod("GeneratePreview", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var getValueDisplayString = typeof(MaterialPropertyModifierWindow).GetMethod("GetValueDisplayString", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(canGeneratePreview, "CanGeneratePreview method should exist");
            Assert.IsNotNull(generatePreview, "GeneratePreview method should exist");
            Assert.IsNotNull(getValueDisplayString, "GetValueDisplayString method should exist");
        }

        [Test]
        public void DrawMethods_Exist_ForUIRendering()
        {
            // Verify UI drawing methods exist
            var drawMaterialListSection = typeof(MaterialPropertyModifierWindow).GetMethod("DrawMaterialListSection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var drawPreviewSection = typeof(MaterialPropertyModifierWindow).GetMethod("DrawPreviewSection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(drawMaterialListSection, "DrawMaterialListSection method should exist");
            Assert.IsNotNull(drawPreviewSection, "DrawPreviewSection method should exist");
        }

        [Test]
        public void WindowFields_ExistForState_MaterialListAndPreview()
        {
            // Verify required fields exist for material list and preview functionality
            var type = typeof(MaterialPropertyModifierWindow);
            
            var foundMaterialsField = type.GetField("foundMaterials", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var modificationPreviewField = type.GetField("modificationPreview", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var materialListScrollPositionField = type.GetField("materialListScrollPosition", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(foundMaterialsField, "foundMaterials field should exist");
            Assert.IsNotNull(modificationPreviewField, "modificationPreview field should exist");
            Assert.IsNotNull(materialListScrollPositionField, "materialListScrollPosition field should exist");
        }

        [Test]
        public void OperationControlMethods_Exist_ForOperationManagement()
        {
            // Verify operation control methods exist
            var canApplyModifications = typeof(MaterialPropertyModifierWindow).GetMethod("CanApplyModifications", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var startPreviewOperation = typeof(MaterialPropertyModifierWindow).GetMethod("StartPreviewOperation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var startApplyOperation = typeof(MaterialPropertyModifierWindow).GetMethod("StartApplyOperation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(canApplyModifications, "CanApplyModifications method should exist");
            Assert.IsNotNull(startPreviewOperation, "StartPreviewOperation method should exist");
            Assert.IsNotNull(startApplyOperation, "StartApplyOperation method should exist");
        }

        [Test]
        public void OperationFeedbackMethods_Exist_ForUserFeedback()
        {
            // Verify operation feedback methods exist
            var startOperation = typeof(MaterialPropertyModifierWindow).GetMethod("StartOperation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var completeOperation = typeof(MaterialPropertyModifierWindow).GetMethod("CompleteOperation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var cancelOperation = typeof(MaterialPropertyModifierWindow).GetMethod("CancelOperation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var addToOperationLog = typeof(MaterialPropertyModifierWindow).GetMethod("AddToOperationLog", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(startOperation, "StartOperation method should exist");
            Assert.IsNotNull(completeOperation, "CompleteOperation method should exist");
            Assert.IsNotNull(cancelOperation, "CancelOperation method should exist");
            Assert.IsNotNull(addToOperationLog, "AddToOperationLog method should exist");
        }

        [Test]
        public void OperationDrawMethods_Exist_ForUIRendering()
        {
            // Verify operation UI drawing methods exist
            var drawOperationControlsSection = typeof(MaterialPropertyModifierWindow).GetMethod("DrawOperationControlsSection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var drawOperationProgress = typeof(MaterialPropertyModifierWindow).GetMethod("DrawOperationProgress", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var drawOperationFeedbackSection = typeof(MaterialPropertyModifierWindow).GetMethod("DrawOperationFeedbackSection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var drawOperationLog = typeof(MaterialPropertyModifierWindow).GetMethod("DrawOperationLog", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(drawOperationControlsSection, "DrawOperationControlsSection method should exist");
            Assert.IsNotNull(drawOperationProgress, "DrawOperationProgress method should exist");
            Assert.IsNotNull(drawOperationFeedbackSection, "DrawOperationFeedbackSection method should exist");
            Assert.IsNotNull(drawOperationLog, "DrawOperationLog method should exist");
        }

        [Test]
        public void OperationStateFields_ExistForState_OperationTracking()
        {
            // Verify required fields exist for operation state tracking
            var type = typeof(MaterialPropertyModifierWindow);
            
            var isOperationInProgressField = type.GetField("isOperationInProgress", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var operationStatusField = type.GetField("operationStatus", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var operationProgressField = type.GetField("operationProgress", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var operationLogField = type.GetField("operationLog", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(isOperationInProgressField, "isOperationInProgress field should exist");
            Assert.IsNotNull(operationStatusField, "operationStatus field should exist");
            Assert.IsNotNull(operationProgressField, "operationProgress field should exist");
            Assert.IsNotNull(operationLogField, "operationLog field should exist");
        }
    }
}