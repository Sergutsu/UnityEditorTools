using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace MaterialPropertyModifier.Editor.Tests
{
    /// <summary>
    /// Comprehensive test runner for Material Property Modifier
    /// Provides automated testing and validation of all functionality
    /// </summary>
    public class ComprehensiveTestRunner
    {
        /// <summary>
        /// Menu item to run all comprehensive tests
        /// </summary>
        [MenuItem("Tools/Material Property Modifier/Run Comprehensive Tests")]
        public static void RunAllTests()
        {
            Debug.Log("=== Starting Material Property Modifier Comprehensive Tests ===");
            
            var testRunner = new ComprehensiveTestRunner();
            var results = testRunner.ExecuteAllTests();
            
     