# Implementation Plan

- [x] 1. Set up project structure and core data models





  - Create Editor folder structure under Assets/MaterialPropertyModifier/Editor
  - Define MaterialModificationData class with all required properties
  - Create supporting data classes (PropertyValidationResult, ModificationPreview, MaterialModification)
  - _Requirements: 4.1, 4.2_

- [ ] 2. Implement core material discovery functionality
  - Create MaterialPropertyModifier class with FindMaterialsWithShader method
  - Implement recursive folder scanning using AssetDatabase.FindAssets
  - Add shader matching logic to filter materials by target shader
  - Write unit tests for material discovery with mock data
  - _Requirements: 1.1, 1.2, 1.4_

- [ ] 3. Implement property validation system
  - Add ValidateProperty method to check if property exists on shader
  - Implement GetPropertyType method using Shader API
  - Create property type validation for different value types (float, color, texture)
  - Write unit tests for property validation with various shader types
  - _Requirements: 2.1, 2.2, 5.3_

- [ ] 4. Create modification preview functionality
  - Implement PreviewModifications method to generate modification preview
  - Add logic to compare current vs target property values
  - Create preview data structure showing which materials will be modified/skipped
  - Write tests for preview generation with different material configurations
  - _Requirements: 3.1, 3.2_

- [ ] 5. Implement material property modification system
  - Create ApplyModifications method with proper error handling
  - Add property setting logic using appropriate Material.SetFloat/SetColor/etc methods
  - Implement asset dirty marking and saving functionality
  - Add undo support using Undo.RecordObject
  - Write tests for modification application and rollback scenarios
  - _Requirements: 2.3, 2.4, 3.3, 5.1, 5.2, 5.4_

- [ ] 6. Create the main editor window UI
  - Implement MaterialPropertyModifierWindow class extending EditorWindow
  - Add menu item registration under Tools menu
  - Create basic window layout with folder selection field
  - Implement window opening and initialization logic
  - _Requirements: 4.1, 4.2_

- [ ] 7. Implement folder and shader selection UI
  - Add folder selection using EditorGUILayout.ObjectField with folder constraint
  - Create shader dropdown populated from project shaders
  - Implement OnSelectionChange to update folder when Project selection changes
  - Add input validation and error display for folder/shader selection
  - _Requirements: 1.1, 1.2, 4.3, 5.3_

- [ ] 8. Create property configuration UI
  - Add property name input field with validation feedback
  - Implement dynamic property value input based on detected property type
  - Add real-time validation display for property name and value
  - Create helper UI to show available properties for selected shader
  - _Requirements: 2.1, 2.2, 4.3, 5.3_

- [ ] 9. Implement material list display and preview UI
  - Create scrollable list showing found materials with file paths
  - Add preview section displaying current vs target values
  - Implement material count display and filtering options
  - Add progress bar for long-running operations
  - _Requirements: 1.4, 3.1, 3.2, 4.3_

- [ ] 10. Add operation controls and feedback
  - Implement Preview and Apply buttons with proper state management
  - Add operation progress tracking and cancellation support
  - Create success/failure message display with detailed results
  - Implement error dialog for critical failures with retry options
  - _Requirements: 3.3, 4.4, 5.1, 5.2, 5.3_

- [ ] 11. Integrate all components and add comprehensive error handling
  - Wire together UI components with core logic classes
  - Add comprehensive try-catch blocks around all operations
  - Implement graceful degradation for partial failures
  - Add logging for debugging and user feedback
  - _Requirements: 5.1, 5.2, 5.4_

- [ ] 12. Create integration tests and validate end-to-end functionality
  - Write integration tests covering complete workflow scenarios
  - Test with various Unity shader types (Built-in, URP, HDRP, custom)
  - Validate performance with large material collections
  - Test error recovery and edge case handling
  - _Requirements: All requirements validation_