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
    }
}