using UnityEngine;
using UnityEditor;

namespace MaterialPropertyModifier.Editor
{
    /// <summary>
    /// Main editor window for the Material Property Modifier tool
    /// Provides UI for material discovery, property modification, and batch operations
    /// </summary>
    public class MaterialPropertyModifierWindow : EditorWindow
    {
        private MaterialPropertyModifier modifier;
        private Vector2 scrollPosition;
        
        // Window state
        private bool isInitialized = false;
        
        // Selection state
        private DefaultAsset selectedFolder;
        private string folderPath = "";
        private Shader selectedShader;
        private string[] availableShaders;
        private int selectedShaderIndex = 0;
        
        // Validation state
        private bool isFolderValid = false;
        private bool isShaderValid = false;
        private string folderValidationMessage = "";
        private string shaderValidationMessage = "";
        
        // Property configuration state
        private string propertyName = "";
        private object propertyValue;
        private ShaderPropertyType propertyType = ShaderPropertyType.Float;
        private bool isPropertyValid = false;
        private string propertyValidationMessage = "";
        
        // Property value inputs
        private float floatValue = 0f;
        private int intValue = 0;
        private Color colorValue = Color.white;
        private Vector4 vectorValue = Vector4.zero;
        private Texture textureValue;
        
        // Available properties for selected shader
        private string[] availableProperties;
        private ShaderPropertyType[] availablePropertyTypes;
        private bool showPropertyHelper = false;
        
        // Material list and preview state
        private System.Collections.Generic.List<Material> foundMaterials;
        private ModificationPreview modificationPreview;
        private Vector2 materialListScrollPosition;
        private bool showMaterialList = false;
        private bool showPreview = false;
        private string materialSearchFilter = "";
        private bool isSearching = false;
        
        // Operation controls and feedback state
        private bool isOperationInProgress = false;
        private bool isPreviewInProgress = false;
        private bool isApplyInProgress = false;
        private string operationStatus = "";
        private float operationProgress = 0f;
        private bool showOperationResults = false;
        private string operationResultMessage = "";
        private MessageType operationResultType = MessageType.Info;
        private System.Collections.Generic.List<string> operationLog;
        private Vector2 operationLogScrollPosition;
        private bool operationCancelled = false;
        
        // UI Layout constants
        private const float WINDOW_MIN_WIDTH = 400f;
        private const float WINDOW_MIN_HEIGHT = 500f;
        private const float SECTION_SPACING = 10f;
        private const float BUTTON_HEIGHT = 25f;

        /// <summary>
        /// Menu item to open the Material Property Modifier window
        /// </summary>
        [MenuItem("Tools/Material Property Modifier")]
        public static void ShowWindow()
        {
            MaterialPropertyModifierWindow window = GetWindow<MaterialPropertyModifierWindow>("Material Property Modifier");
            window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
            window.Show();
        }

        /// <summary>
        /// Menu item to perform system health check
        /// </summary>
        [MenuItem("Tools/Material Property Modifier/System Health Check")]
        public static void PerformSystemHealthCheck()
        {
            var modifier = new MaterialPropertyModifier();
            var healthCheckResult = modifier.PerformHealthCheck();
            
            if (healthCheckResult.IsSuccess)
            {
                var health = healthCheckResult.Data;
                string healthMessage = $"System Health Check Results:\n\n";
                healthMessage += $"Overall Health: {(health.OverallHealth ? "HEALTHY" : "UNHEALTHY")}\n";
                healthMessage += $"Health Score: {health.HealthScore:P1}\n";
                healthMessage += $"Passed Checks: {health.PassedChecks}/{health.TotalChecks}\n\n";

                if (health.FailedChecks.Count > 0)
                {
                    healthMessage += "Failed Checks:\n";
                    foreach (var failedCheck in health.FailedChecks)
                    {
                        healthMessage += $"• {failedCheck}\n";
                    }
                }

                if (health.OverallHealth)
                {
                    EditorUtility.DisplayDialog("Health Check - Healthy", healthMessage, "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Health Check - Issues Found", healthMessage, "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Health Check Failed", $"Could not perform system health check:\n\n{healthCheckResult.ErrorMessage}", "OK");
            }
        }

        /// <summary>
        /// Initialize the window when it's first opened
        /// </summary>
        private void OnEnable()
        {
            try
            {
                InitializeWindow();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifierWindow] Failed to initialize window: {ex.Message}\n{ex.StackTrace}");
                ShowErrorDialog("Window Initialization Error", $"Failed to initialize Material Property Modifier window:\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Initialize window components and state
        /// </summary>
        private void InitializeWindow()
        {
            try
            {
                if (modifier == null)
                {
                    modifier = new MaterialPropertyModifier();
                }
                
                // Validate system prerequisites
                var systemValidation = modifier.ValidateSystemPrerequisites();
                if (!systemValidation.IsValid)
                {
                    ShowErrorDialog("System Validation Error", systemValidation.ErrorMessage);
                    return;
                }
                
                // Initialize shader list
                RefreshShaderList();
                
                // Initialize folder from current selection if applicable
                InitializeFolderFromSelection();
                
                isInitialized = true;
                
                // Set window title and icon
                titleContent = new GUIContent("Material Property Modifier", "Batch modify material properties");
                
                Debug.Log("[MaterialPropertyModifierWindow] Window initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifierWindow] Window initialization failed: {ex.Message}\n{ex.StackTrace}");
                ShowErrorDialog("Initialization Error", $"Failed to initialize window:\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Main GUI rendering method
        /// </summary>
        private void OnGUI()
        {
            if (!isInitialized)
            {
                InitializeWindow();
            }

            // Begin scroll view for the entire window
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            try
            {
                DrawWindowHeader();
                
                GUILayout.Space(SECTION_SPACING);
                
                DrawMainContent();
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox($"Error rendering UI: {ex.Message}", MessageType.Error);
                Debug.LogError($"MaterialPropertyModifierWindow GUI Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// Draw the window header with title and description
        /// </summary>
        private void DrawWindowHeader()
        {
            // Title
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            GUILayout.Label("Material Property Modifier", titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            // Description
            GUIStyle descriptionStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            
            GUILayout.Label("Batch modify properties on materials using a specific shader", descriptionStyle);
            
            // Separator line
            GUILayout.Space(5);
            DrawHorizontalLine();
        }

        /// <summary>
        /// Draw the main content area
        /// </summary>
        private void DrawMainContent()
        {
            DrawSelectionSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            DrawPropertyConfigurationSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            DrawMaterialListSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            DrawPreviewSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            DrawOperationControlsSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            DrawOperationFeedbackSection();
            
            GUILayout.Space(SECTION_SPACING);
            
            // Status information
            DrawStatusSection();
        }

        /// <summary>
        /// Draw the folder and shader selection section
        /// </summary>
        private void DrawSelectionSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Selection", EditorStyles.boldLabel);
            
            // Folder selection
            DrawFolderSelection();
            
            GUILayout.Space(5);
            
            // Shader selection
            DrawShaderSelection();
            
            GUILayout.Space(5);
            
            // Refresh button
            DrawRefreshControls();
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw folder selection UI
        /// </summary>
        private void DrawFolderSelection()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Target Folder:", GUILayout.Width(100));
            
            // Folder object field
            DefaultAsset newFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                selectedFolder, 
                typeof(DefaultAsset), 
                false,
                GUILayout.ExpandWidth(true)
            );
            
            // Handle folder selection change
            if (newFolder != selectedFolder)
            {
                selectedFolder = newFolder;
                OnFolderSelectionChanged();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show folder path
            if (!string.IsNullOrEmpty(folderPath))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(105); // Align with label
                GUILayout.Label($"Path: {folderPath}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            // Show validation message
            if (!string.IsNullOrEmpty(folderValidationMessage))
            {
                MessageType messageType = isFolderValid ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(folderValidationMessage, messageType);
            }
        }

        /// <summary>
        /// Draw shader selection UI
        /// </summary>
        private void DrawShaderSelection()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Target Shader:", GUILayout.Width(100));
            
            if (availableShaders != null && availableShaders.Length > 0)
            {
                // Shader dropdown
                int newIndex = EditorGUILayout.Popup(selectedShaderIndex, availableShaders);
                
                if (newIndex != selectedShaderIndex)
                {
                    selectedShaderIndex = newIndex;
                    OnShaderSelectionChanged();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No shaders available", MessageType.Warning);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show shader validation message
            if (!string.IsNullOrEmpty(shaderValidationMessage))
            {
                MessageType messageType = isShaderValid ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(shaderValidationMessage, messageType);
            }
        }

        /// <summary>
        /// Draw refresh controls
        /// </summary>
        private void DrawRefreshControls()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh Shaders", GUILayout.Width(120), GUILayout.Height(BUTTON_HEIGHT)))
            {
                RefreshShaderList();
            }
            
            if (GUILayout.Button("Use Project Selection", GUILayout.Width(150), GUILayout.Height(BUTTON_HEIGHT)))
            {
                InitializeFolderFromSelection();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the property configuration section
        /// </summary>
        private void DrawPropertyConfigurationSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Property Configuration", EditorStyles.boldLabel);
            
            // Only show if shader is selected
            if (isShaderValid && selectedShader != null)
            {
                DrawPropertyNameInput();
                
                GUILayout.Space(5);
                
                DrawPropertyValueInput();
                
                GUILayout.Space(5);
                
                DrawPropertyHelper();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a shader to configure properties", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw property name input and validation
        /// </summary>
        private void DrawPropertyNameInput()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Property Name:", GUILayout.Width(100));
            
            string newPropertyName = EditorGUILayout.TextField(propertyName);
            
            if (newPropertyName != propertyName)
            {
                propertyName = newPropertyName;
                OnPropertyNameChanged();
            }
            
            // Helper button
            if (GUILayout.Button("?", GUILayout.Width(25), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                showPropertyHelper = !showPropertyHelper;
                if (showPropertyHelper)
                {
                    RefreshAvailableProperties();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show validation message
            if (!string.IsNullOrEmpty(propertyValidationMessage))
            {
                MessageType messageType = isPropertyValid ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(propertyValidationMessage, messageType);
            }
        }

        /// <summary>
        /// Draw property value input based on detected property type
        /// </summary>
        private void DrawPropertyValueInput()
        {
            if (!isPropertyValid)
            {
                EditorGUILayout.HelpBox("Enter a valid property name to configure value", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Property Value:", GUILayout.Width(100));
            
            // Draw appropriate input based on property type
            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    float newFloatValue = EditorGUILayout.FloatField(floatValue);
                    if (newFloatValue != floatValue)
                    {
                        floatValue = newFloatValue;
                        propertyValue = floatValue;
                    }
                    break;
                
                case ShaderPropertyType.Int:
                    int newIntValue = EditorGUILayout.IntField(intValue);
                    if (newIntValue != intValue)
                    {
                        intValue = newIntValue;
                        propertyValue = intValue;
                    }
                    break;
                
                case ShaderPropertyType.Color:
                    Color newColorValue = EditorGUILayout.ColorField(colorValue);
                    if (newColorValue != colorValue)
                    {
                        colorValue = newColorValue;
                        propertyValue = colorValue;
                    }
                    break;
                
                case ShaderPropertyType.Vector:
                    Vector4 newVectorValue = EditorGUILayout.Vector4Field("", vectorValue);
                    if (newVectorValue != vectorValue)
                    {
                        vectorValue = newVectorValue;
                        propertyValue = vectorValue;
                    }
                    break;
                
                case ShaderPropertyType.Texture:
                    Texture newTextureValue = (Texture)EditorGUILayout.ObjectField(
                        textureValue, typeof(Texture), false);
                    if (newTextureValue != textureValue)
                    {
                        textureValue = newTextureValue;
                        propertyValue = textureValue;
                    }
                    break;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show property type info
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(105); // Align with label
            GUILayout.Label($"Type: {propertyType}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw property helper showing available properties
        /// </summary>
        private void DrawPropertyHelper()
        {
            if (!showPropertyHelper)
                return;
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Available Properties", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                RefreshAvailableProperties();
            }
            EditorGUILayout.EndHorizontal();
            
            if (availableProperties != null && availableProperties.Length > 0)
            {
                EditorGUILayout.BeginVertical("box");
                
                for (int i = 0; i < availableProperties.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(availableProperties[i], EditorStyles.linkLabel))
                    {
                        propertyName = availableProperties[i];
                        OnPropertyNameChanged();
                        showPropertyHelper = false;
                    }
                    
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(availablePropertyTypes[i].ToString(), EditorStyles.miniLabel, GUILayout.Width(80));
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No properties found for selected shader", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the material list section
        /// </summary>
        private void DrawMaterialListSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Material Discovery", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Search button
            GUI.enabled = isFolderValid && isShaderValid;
            if (GUILayout.Button("Find Materials", GUILayout.Width(100), GUILayout.Height(BUTTON_HEIGHT)))
            {
                FindMaterials();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (!isFolderValid || !isShaderValid)
            {
                EditorGUILayout.HelpBox("Select folder and shader to find materials", MessageType.Info);
            }
            else if (isSearching)
            {
                EditorGUILayout.HelpBox("Searching for materials...", MessageType.Info);
            }
            else if (foundMaterials != null)
            {
                DrawMaterialListContent();
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the material list content
        /// </summary>
        private void DrawMaterialListContent()
        {
            // Material count and filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Found {foundMaterials.Count} materials", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Search filter
            GUILayout.Label("Filter:", GUILayout.Width(40));
            string newFilter = EditorGUILayout.TextField(materialSearchFilter, GUILayout.Width(150));
            if (newFilter != materialSearchFilter)
            {
                materialSearchFilter = newFilter;
            }
            
            // Toggle list visibility
            string toggleText = showMaterialList ? "Hide List" : "Show List";
            if (GUILayout.Button(toggleText, GUILayout.Width(80)))
            {
                showMaterialList = !showMaterialList;
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showMaterialList && foundMaterials.Count > 0)
            {
                DrawMaterialList();
            }
        }

        /// <summary>
        /// Draw the scrollable material list
        /// </summary>
        private void DrawMaterialList()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Material Name", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("File Path", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            DrawHorizontalLine();
            
            // Scrollable list
            materialListScrollPosition = EditorGUILayout.BeginScrollView(
                materialListScrollPosition, 
                GUILayout.MaxHeight(200)
            );
            
            var filteredMaterials = GetFilteredMaterials();
            
            foreach (var material in filteredMaterials)
            {
                DrawMaterialListItem(material);
            }
            
            if (filteredMaterials.Count == 0 && !string.IsNullOrEmpty(materialSearchFilter))
            {
                EditorGUILayout.HelpBox($"No materials match filter '{materialSearchFilter}'", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a single material list item
        /// </summary>
        private void DrawMaterialListItem(Material material)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Material name (clickable to select in project)
            if (GUILayout.Button(material.name, EditorStyles.linkLabel, GUILayout.Width(200)))
            {
                Selection.activeObject = material;
                EditorGUIUtility.PingObject(material);
            }
            
            // File path
            string assetPath = AssetDatabase.GetAssetPath(material);
            GUILayout.Label(assetPath, EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the preview section
        /// </summary>
        private void DrawPreviewSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Modification Preview", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Generate preview button
            GUI.enabled = CanGeneratePreview();
            if (GUILayout.Button("Generate Preview", GUILayout.Width(120), GUILayout.Height(BUTTON_HEIGHT)))
            {
                GeneratePreview();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (!CanGeneratePreview())
            {
                EditorGUILayout.HelpBox("Configure all settings and find materials to generate preview", MessageType.Info);
            }
            else if (modificationPreview != null)
            {
                DrawPreviewContent();
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the preview content
        /// </summary>
        private void DrawPreviewContent()
        {
            // Preview summary
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preview Summary:", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label($"Total Materials: {modificationPreview.TotalMaterials}");
            GUILayout.Label($"Will be Modified: {modificationPreview.MaterialsToModify.Count}");
            GUILayout.Label($"Will be Skipped: {modificationPreview.MaterialsToSkip.Count}");
            GUILayout.Label($"Property: {modificationPreview.PropertyName} ({modificationPreview.PropertyType})");
            GUILayout.Label($"Target Value: {GetValueDisplayString(modificationPreview.TargetValue)}");
            
            EditorGUILayout.EndVertical();
            
            // Toggle detailed preview
            EditorGUILayout.BeginHorizontal();
            string detailToggleText = showPreview ? "Hide Details" : "Show Details";
            if (GUILayout.Button(detailToggleText, GUILayout.Width(100)))
            {
                showPreview = !showPreview;
            }
            EditorGUILayout.EndHorizontal();
            
            if (showPreview)
            {
                DrawDetailedPreview();
            }
        }

        /// <summary>
        /// Draw detailed preview showing current vs target values
        /// </summary>
        private void DrawDetailedPreview()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Materials to modify
            if (modificationPreview.MaterialsToModify.Count > 0)
            {
                GUILayout.Label("Materials to Modify:", EditorStyles.boldLabel);
                
                foreach (var modification in modificationPreview.MaterialsToModify)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(modification.Material.name, EditorStyles.linkLabel, GUILayout.Width(150)))
                    {
                        Selection.activeObject = modification.Material;
                        EditorGUIUtility.PingObject(modification.Material);
                    }
                    
                    GUILayout.Label($"Current: {GetValueDisplayString(modification.CurrentValue)}", GUILayout.Width(120));
                    GUILayout.Label("→", GUILayout.Width(20));
                    GUILayout.Label($"Target: {GetValueDisplayString(modification.TargetValue)}", GUILayout.Width(120));
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            // Materials to skip
            if (modificationPreview.MaterialsToSkip.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Materials to Skip:", EditorStyles.boldLabel);
                
                foreach (var skipInfo in modificationPreview.MaterialsToSkip)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(skipInfo.Material.name, EditorStyles.linkLabel, GUILayout.Width(150)))
                    {
                        Selection.activeObject = skipInfo.Material;
                        EditorGUIUtility.PingObject(skipInfo.Material);
                    }
                    
                    GUILayout.Label($"Reason: {skipInfo.Reason}", EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw operation controls section
        /// </summary>
        private void DrawOperationControlsSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Operations", EditorStyles.boldLabel);
            
            // Operation buttons
            EditorGUILayout.BeginHorizontal();
            
            // Preview button
            GUI.enabled = CanGeneratePreview() && !isOperationInProgress;
            if (GUILayout.Button("Preview Changes", GUILayout.Height(30)))
            {
                StartPreviewOperation();
            }
            
            // Apply button
            GUI.enabled = CanApplyModifications() && !isOperationInProgress;
            if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
            {
                StartApplyOperation();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Operation progress
            if (isOperationInProgress)
            {
                DrawOperationProgress();
            }
            
            // Cancel button during operations
            if (isOperationInProgress)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Cancel Operation", GUILayout.Width(120), GUILayout.Height(25)))
                {
                    CancelOperation();
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw operation progress indicator
        /// </summary>
        private void DrawOperationProgress()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Status text
            if (!string.IsNullOrEmpty(operationStatus))
            {
                GUILayout.Label(operationStatus, EditorStyles.boldLabel);
            }
            
            // Progress bar
            Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(progressRect, operationProgress, $"{(operationProgress * 100):F0}%");
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw operation feedback section
        /// </summary>
        private void DrawOperationFeedbackSection()
        {
            if (!showOperationResults && (operationLog == null || operationLog.Count == 0))
            {
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Operation Results", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear Results", GUILayout.Width(100)))
            {
                ClearOperationResults();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Operation result message
            if (showOperationResults && !string.IsNullOrEmpty(operationResultMessage))
            {
                EditorGUILayout.HelpBox(operationResultMessage, operationResultType);
            }
            
            // Operation log
            if (operationLog != null && operationLog.Count > 0)
            {
                DrawOperationLog();
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw operation log
        /// </summary>
        private void DrawOperationLog()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Operation Log:", EditorStyles.boldLabel);
            
            operationLogScrollPosition = EditorGUILayout.BeginScrollView(
                operationLogScrollPosition, 
                GUILayout.MaxHeight(150)
            );
            
            foreach (string logEntry in operationLog)
            {
                GUILayout.Label(logEntry, EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw status information section
        /// </summary>
        private void DrawStatusSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Status", EditorStyles.boldLabel);
            
            // Show initialization status
            string statusText = isInitialized ? "✓ Window initialized" : "⚠ Initializing...";
            MessageType statusType = isInitialized ? MessageType.Info : MessageType.Warning;
            
            EditorGUILayout.HelpBox(statusText, statusType);
            
            // Show selection status
            string selectionStatus = GetSelectionStatusText();
            MessageType selectionType = (isFolderValid && isShaderValid) ? MessageType.Info : MessageType.Warning;
            
            EditorGUILayout.HelpBox(selectionStatus, selectionType);
            
            // Show property configuration status
            string propertyStatus = GetPropertyStatusText();
            MessageType propertyType = isPropertyValid ? MessageType.Info : MessageType.Warning;
            
            EditorGUILayout.HelpBox(propertyStatus, propertyType);
            
            // Show core component status
            string coreStatus = modifier != null ? "✓ Core logic ready" : "✗ Core logic not available";
            MessageType coreType = modifier != null ? MessageType.Info : MessageType.Error;
            
            EditorGUILayout.HelpBox(coreStatus, coreType);
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a horizontal separator line
        /// </summary>
        private void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        /// <summary>
        /// Handle window focus events
        /// </summary>
        private void OnFocus()
        {
            // Refresh window state when focused
            if (!isInitialized)
            {
                InitializeWindow();
            }
        }

        /// <summary>
        /// Handle window destruction
        /// </summary>
        private void OnDestroy()
        {
            // Cleanup if needed
            modifier = null;
            isInitialized = false;
        }

        /// <summary>
        /// Handle window disable
        /// </summary>
        private void OnDisable()
        {
            // Save any pending state if needed
        }

        /// <summary>
        /// Handle selection change events from Unity's selection system
        /// </summary>
        private void OnSelectionChange()
        {
            // Auto-update folder selection when user selects a folder in Project window
            if (Selection.activeObject is DefaultAsset folder)
            {
                string path = AssetDatabase.GetAssetPath(folder);
                if (AssetDatabase.IsValidFolder(path))
                {
                    selectedFolder = folder;
                    OnFolderSelectionChanged();
                    Repaint(); // Refresh the window
                }
            }
        }

        /// <summary>
        /// Refresh the list of available shaders
        /// </summary>
        private void RefreshShaderList()
        {
            try
            {
                // Find all shader assets in the project
                string[] shaderGuids = AssetDatabase.FindAssets("t:Shader");
                var shaderNames = new System.Collections.Generic.List<string>();
                
                // Add built-in shaders
                shaderNames.Add("Standard");
                shaderNames.Add("Unlit/Color");
                shaderNames.Add("Unlit/Texture");
                shaderNames.Add("Legacy Shaders/Diffuse");
                shaderNames.Add("Legacy Shaders/Specular");
                
                // Add project shaders
                foreach (string guid in shaderGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    
                    if (shader != null && !shaderNames.Contains(shader.name))
                    {
                        shaderNames.Add(shader.name);
                    }
                }
                
                // Sort and convert to array
                shaderNames.Sort();
                availableShaders = shaderNames.ToArray();
                
                // Reset selection if current index is invalid
                if (selectedShaderIndex >= availableShaders.Length)
                {
                    selectedShaderIndex = 0;
                }
                
                // Update selected shader
                OnShaderSelectionChanged();
                
                Debug.Log($"Refreshed shader list: {availableShaders.Length} shaders found");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error refreshing shader list: {ex.Message}");
                availableShaders = new string[] { "Standard" };
                selectedShaderIndex = 0;
            }
        }

        /// <summary>
        /// Initialize folder selection from current Unity selection
        /// </summary>
        private void InitializeFolderFromSelection()
        {
            if (Selection.activeObject is DefaultAsset folder)
            {
                string path = AssetDatabase.GetAssetPath(folder);
                if (AssetDatabase.IsValidFolder(path))
                {
                    selectedFolder = folder;
                    OnFolderSelectionChanged();
                    return;
                }
            }
            
            // Default to Assets folder if no valid selection
            selectedFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
            OnFolderSelectionChanged();
        }

        /// <summary>
        /// Handle folder selection changes
        /// </summary>
        private void OnFolderSelectionChanged()
        {
            if (selectedFolder != null)
            {
                folderPath = AssetDatabase.GetAssetPath(selectedFolder);
                
                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    isFolderValid = true;
                    folderValidationMessage = $"✓ Valid folder selected";
                }
                else
                {
                    isFolderValid = false;
                    folderValidationMessage = "⚠ Selected asset is not a folder";
                }
            }
            else
            {
                folderPath = "";
                isFolderValid = false;
                folderValidationMessage = "⚠ No folder selected";
            }
        }

        /// <summary>
        /// Handle shader selection changes
        /// </summary>
        private void OnShaderSelectionChanged()
        {
            if (availableShaders != null && selectedShaderIndex >= 0 && selectedShaderIndex < availableShaders.Length)
            {
                string shaderName = availableShaders[selectedShaderIndex];
                selectedShader = Shader.Find(shaderName);
                
                if (selectedShader != null)
                {
                    isShaderValid = true;
                    shaderValidationMessage = $"✓ Shader '{shaderName}' found";
                }
                else
                {
                    isShaderValid = false;
                    shaderValidationMessage = $"⚠ Shader '{shaderName}' not found";
                }
            }
            else
            {
                selectedShader = null;
                isShaderValid = false;
                shaderValidationMessage = "⚠ No shader selected";
            }
        }

        /// <summary>
        /// Get status text for selection validation
        /// </summary>
        private string GetSelectionStatusText()
        {
            if (isFolderValid && isShaderValid)
            {
                return "✓ Folder and shader selected";
            }
            else if (isFolderValid)
            {
                return "⚠ Folder selected, shader needed";
            }
            else if (isShaderValid)
            {
                return "⚠ Shader selected, folder needed";
            }
            else
            {
                return "⚠ Folder and shader selection required";
            }
        }

        /// <summary>
        /// Get status text for property configuration validation
        /// </summary>
        private string GetPropertyStatusText()
        {
            if (!isShaderValid)
            {
                return "⚠ Select shader to configure properties";
            }
            else if (string.IsNullOrEmpty(propertyName))
            {
                return "⚠ Enter property name";
            }
            else if (isPropertyValid)
            {
                return $"✓ Property '{propertyName}' configured ({propertyType})";
            }
            else
            {
                return "⚠ Invalid property name";
            }
        }

        /// <summary>
        /// Handle property name changes and validation
        /// </summary>
        private void OnPropertyNameChanged()
        {
            if (selectedShader == null || string.IsNullOrEmpty(propertyName))
            {
                isPropertyValid = false;
                propertyValidationMessage = "Enter a property name";
                return;
            }

            try
            {
                var validation = modifier.ValidateProperty(selectedShader, propertyName);
                isPropertyValid = validation.IsValid;
                propertyValidationMessage = validation.IsValid ? 
                    $"✓ Property '{propertyName}' found" : 
                    validation.ErrorMessage;

                if (validation.IsValid)
                {
                    propertyType = validation.PropertyType;
                    InitializePropertyValue();
                }
            }
            catch (System.Exception ex)
            {
                isPropertyValid = false;
                propertyValidationMessage = $"Error validating property: {ex.Message}";
                Debug.LogError($"Property validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize property value based on property type
        /// </summary>
        private void InitializePropertyValue()
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    propertyValue = floatValue;
                    break;
                
                case ShaderPropertyType.Int:
                    propertyValue = intValue;
                    break;
                
                case ShaderPropertyType.Color:
                    propertyValue = colorValue;
                    break;
                
                case ShaderPropertyType.Vector:
                    propertyValue = vectorValue;
                    break;
                
                case ShaderPropertyType.Texture:
                    propertyValue = textureValue;
                    break;
            }
        }

        /// <summary>
        /// Refresh the list of available properties for the selected shader
        /// </summary>
        private void RefreshAvailableProperties()
        {
            if (selectedShader == null)
            {
                availableProperties = new string[0];
                availablePropertyTypes = new ShaderPropertyType[0];
                return;
            }

            try
            {
                int propertyCount = selectedShader.GetPropertyCount();
                availableProperties = new string[propertyCount];
                availablePropertyTypes = new ShaderPropertyType[propertyCount];

                for (int i = 0; i < propertyCount; i++)
                {
                    availableProperties[i] = selectedShader.GetPropertyName(i);
                    availablePropertyTypes[i] = selectedShader.GetPropertyType(i);
                }

                Debug.Log($"Refreshed properties for shader '{selectedShader.name}': {propertyCount} properties found");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error refreshing shader properties: {ex.Message}");
                availableProperties = new string[0];
                availablePropertyTypes = new ShaderPropertyType[0];
            }
        }

        /// <summary>
        /// Find materials using the selected folder and shader
        /// </summary>
        private void FindMaterials()
        {
            if (!isFolderValid || !isShaderValid)
            {
                AddToOperationLog("Cannot find materials: Invalid folder or shader selection");
                return;
            }

            try
            {
                isSearching = true;
                Repaint();

                // Use enhanced method with comprehensive error handling
                var discoveryResult = modifier.FindMaterialsWithShaderEnhanced(folderPath, selectedShader);
                
                if (discoveryResult.IsSuccess)
                {
                    foundMaterials = discoveryResult.Materials;
                    AddToOperationLog($"Successfully found {foundMaterials.Count} materials using shader '{selectedShader.name}' in folder '{folderPath}'");
                    Debug.Log($"[MaterialPropertyModifierWindow] Found {foundMaterials.Count} materials");
                }
                else
                {
                    foundMaterials = new System.Collections.Generic.List<Material>();
                    AddToOperationLog($"Material discovery failed: {discoveryResult.ErrorMessage}");
                    ShowErrorDialog("Material Discovery Error", discoveryResult.ErrorMessage);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifierWindow] Unexpected error finding materials: {ex.Message}\n{ex.StackTrace}");
                foundMaterials = new System.Collections.Generic.List<Material>();
                AddToOperationLog($"Unexpected error during material discovery: {ex.Message}");
                ShowErrorDialog("Unexpected Error", $"An unexpected error occurred while finding materials:\n\n{ex.Message}");
            }
            finally
            {
                isSearching = false;
                Repaint();
            }
        }

        /// <summary>
        /// Get filtered materials based on search filter
        /// </summary>
        private System.Collections.Generic.List<Material> GetFilteredMaterials()
        {
            if (foundMaterials == null)
            {
                return new System.Collections.Generic.List<Material>();
            }

            if (string.IsNullOrEmpty(materialSearchFilter))
            {
                return foundMaterials;
            }

            var filtered = new System.Collections.Generic.List<Material>();
            string filterLower = materialSearchFilter.ToLower();

            foreach (var material in foundMaterials)
            {
                if (material.name.ToLower().Contains(filterLower))
                {
                    filtered.Add(material);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Check if preview can be generated
        /// </summary>
        private bool CanGeneratePreview()
        {
            return isFolderValid && isShaderValid && isPropertyValid && 
                   foundMaterials != null && foundMaterials.Count > 0 && 
                   propertyValue != null;
        }

        /// <summary>
        /// Generate modification preview
        /// </summary>
        private void GeneratePreview()
        {
            if (!CanGeneratePreview())
            {
                AddToOperationLog("Cannot generate preview: Prerequisites not met");
                return;
            }

            try
            {
                var materialModificationData = new MaterialModificationData
                {
                    TargetFolder = folderPath,
                    TargetShader = selectedShader,
                    PropertyName = propertyName,
                    PropertyValue = propertyValue,
                    PropertyType = propertyType
                };

                // Use enhanced method with comprehensive error handling
                var previewResult = modifier.PreviewModificationsEnhanced(materialModificationData);
                
                if (previewResult.IsSuccess)
                {
                    modificationPreview = previewResult.Preview;
                    AddToOperationLog($"Successfully generated preview: {modificationPreview.MaterialsToModify.Count} to modify, {modificationPreview.MaterialsToSkip.Count} to skip");
                    Debug.Log($"[MaterialPropertyModifierWindow] Generated preview successfully");
                }
                else
                {
                    modificationPreview = null;
                    AddToOperationLog($"Preview generation failed: {previewResult.ErrorMessage}");
                    ShowErrorDialog("Preview Generation Error", previewResult.ErrorMessage);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifierWindow] Unexpected error generating preview: {ex.Message}\n{ex.StackTrace}");
                modificationPreview = null;
                AddToOperationLog($"Unexpected error during preview generation: {ex.Message}");
                ShowErrorDialog("Unexpected Error", $"An unexpected error occurred while generating preview:\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Get display string for a property value
        /// </summary>
        private string GetValueDisplayString(object value)
        {
            if (value == null)
            {
                return "null";
            }

            switch (value)
            {
                case float f:
                    return f.ToString("F3");
                case int i:
                    return i.ToString();
                case Color c:
                    return $"RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})";
                case Vector4 v:
                    return $"({v.x:F2}, {v.y:F2}, {v.z:F2}, {v.w:F2})";
                case Vector3 v3:
                    return $"({v3.x:F2}, {v3.y:F2}, {v3.z:F2})";
                case Vector2 v2:
                    return $"({v2.x:F2}, {v2.y:F2})";
                case Texture tex:
                    return tex != null ? tex.name : "null";
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Check if modifications can be applied
        /// </summary>
        private bool CanApplyModifications()
        {
            return CanGeneratePreview() && modificationPreview != null && 
                   modificationPreview.MaterialsToModify.Count > 0;
        }

        /// <summary>
        /// Start preview operation
        /// </summary>
        private void StartPreviewOperation()
        {
            if (isOperationInProgress)
                return;

            StartOperation("Generating preview...");
            isPreviewInProgress = true;

            try
            {
                // Simulate progress for preview generation
                for (int i = 0; i <= 100; i += 20)
                {
                    if (operationCancelled)
                    {
                        CompleteOperation("Preview cancelled", MessageType.Warning);
                        return;
                    }
                    
                    operationProgress = i / 100f;
                    operationStatus = $"Generating preview... {i}%";
                    Repaint();
                    
                    // Small delay to show progress
                    System.Threading.Thread.Sleep(100);
                }

                GeneratePreview();
                
                if (modificationPreview != null)
                {
                    string message = $"Preview generated successfully. {modificationPreview.MaterialsToModify.Count} materials will be modified, {modificationPreview.MaterialsToSkip.Count} will be skipped.";
                    CompleteOperation(message, MessageType.Info);
                    
                    AddToOperationLog($"Preview generated at {System.DateTime.Now:HH:mm:ss}");
                    AddToOperationLog($"Materials to modify: {modificationPreview.MaterialsToModify.Count}");
                    AddToOperationLog($"Materials to skip: {modificationPreview.MaterialsToSkip.Count}");
                }
                else
                {
                    CompleteOperation("Failed to generate preview", MessageType.Error);
                }
            }
            catch (System.Exception ex)
            {
                CompleteOperation($"Preview generation failed: {ex.Message}", MessageType.Error);
                Debug.LogError($"Preview generation error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                isPreviewInProgress = false;
            }
        }

        /// <summary>
        /// Start apply operation
        /// </summary>
        private void StartApplyOperation()
        {
            if (isOperationInProgress || modificationPreview == null)
                return;

            // Confirmation dialog
            if (!EditorUtility.DisplayDialog(
                "Apply Material Modifications",
                $"This will modify {modificationPreview.MaterialsToModify.Count} materials.\n\nThis action cannot be undone automatically. Make sure you have backups.\n\nContinue?",
                "Apply Changes",
                "Cancel"))
            {
                return;
            }

            StartOperation("Applying modifications...");
            isApplyInProgress = true;

            try
            {
                var materialModificationData = new MaterialModificationData
                {
                    TargetFolder = folderPath,
                    TargetShader = selectedShader,
                    PropertyName = propertyName,
                    PropertyValue = propertyValue,
                    PropertyType = propertyType
                };

                int totalMaterials = modificationPreview.MaterialsToModify.Count;
                int processedMaterials = 0;

                AddToOperationLog($"Starting modification of {totalMaterials} materials at {System.DateTime.Now:HH:mm:ss}");

                foreach (var modification in modificationPreview.MaterialsToModify)
                {
                    if (operationCancelled)
                    {
                        CompleteOperation("Operation cancelled", MessageType.Warning);
                        return;
                    }

                    try
                    {
                        // Apply modification using enhanced core logic
                        var result = modifier.ApplyModificationsEnhanced(materialModificationData);
                        
                        processedMaterials++;
                        operationProgress = (float)processedMaterials / totalMaterials;
                        operationStatus = $"Applying modifications... {processedMaterials}/{totalMaterials}";
                        
                        AddToOperationLog($"✓ Modified {modification.Material.name}");
                        
                        Repaint();
                        
                        // Small delay to show progress
                        System.Threading.Thread.Sleep(50);
                    }
                    catch (System.Exception ex)
                    {
                        AddToOperationLog($"✗ Failed to modify {modification.Material.name}: {ex.Message}");
                        Debug.LogError($"Failed to modify material {modification.Material.name}: {ex.Message}");
                    }
                }

                string message = $"Applied modifications to {processedMaterials} materials successfully.";
                CompleteOperation(message, MessageType.Info);
                
                AddToOperationLog($"Operation completed at {System.DateTime.Now:HH:mm:ss}");
                AddToOperationLog($"Successfully modified: {processedMaterials} materials");
            }
            catch (System.Exception ex)
            {
                CompleteOperation($"Apply operation failed: {ex.Message}", MessageType.Error);
                Debug.LogError($"Apply operation error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                isApplyInProgress = false;
            }
        }

        /// <summary>
        /// Start an operation with progress tracking
        /// </summary>
        private void StartOperation(string status)
        {
            isOperationInProgress = true;
            operationCancelled = false;
            operationStatus = status;
            operationProgress = 0f;
            
            if (operationLog == null)
            {
                operationLog = new System.Collections.Generic.List<string>();
            }
        }

        /// <summary>
        /// Complete an operation with result message
        /// </summary>
        private void CompleteOperation(string message, MessageType messageType)
        {
            isOperationInProgress = false;
            isPreviewInProgress = false;
            isApplyInProgress = false;
            operationStatus = "";
            operationProgress = 0f;
            
            showOperationResults = true;
            operationResultMessage = message;
            operationResultType = messageType;
            
            Repaint();
        }

        /// <summary>
        /// Cancel the current operation
        /// </summary>
        private void CancelOperation()
        {
            operationCancelled = true;
            AddToOperationLog($"Operation cancelled by user at {System.DateTime.Now:HH:mm:ss}");
        }

        /// <summary>
        /// Add entry to operation log
        /// </summary>
        private void AddToOperationLog(string message)
        {
            if (operationLog == null)
            {
                operationLog = new System.Collections.Generic.List<string>();
            }
            
            operationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            
            // Keep log size manageable
            if (operationLog.Count > 100)
            {
                operationLog.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clear operation results and log
        /// </summary>
        private void ClearOperationResults()
        {
            showOperationResults = false;
            operationResultMessage = "";
            operationLog?.Clear();
        }

        /// <summary>
        /// Show error dialog with comprehensive error information
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Error message</param>
        /// <param name="exception">Optional exception for detailed information</param>
        private void ShowErrorDialog(string title, string message, System.Exception exception = null)
        {
            try
            {
                string fullMessage = message;
                
                if (exception != null)
                {
                    fullMessage += $"\n\nTechnical Details:\n{exception.Message}";
                    
                    if (exception.InnerException != null)
                    {
                        fullMessage += $"\n\nInner Exception:\n{exception.InnerException.Message}";
                    }
                }

                // Log the error for debugging
                if (exception != null)
                {
                    Debug.LogError($"[MaterialPropertyModifierWindow] {title}: {message}\nException: {exception}");
                }
                else
                {
                    Debug.LogError($"[MaterialPropertyModifierWindow] {title}: {message}");
                }

                // Show dialog with options
                int choice = EditorUtility.DisplayDialogComplex(
                    title,
                    fullMessage,
                    "OK",
                    "Copy to Clipboard",
                    "Show Details"
                );

                switch (choice)
                {
                    case 1: // Copy to Clipboard
                        EditorGUIUtility.systemCopyBuffer = $"{title}\n\n{fullMessage}";
                        Debug.Log("Error details copied to clipboard");
                        break;
                    case 2: // Show Details
                        ShowDetailedErrorDialog(title, message, exception);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaterialPropertyModifierWindow] Failed to show error dialog: {ex.Message}");
                // Fallback to simple dialog
                EditorUtility.DisplayDialog("Error", $"{title}\n\n{message}", "OK");
            }
        }

        /// <summary>
        /// Show detailed error dialog with full exception information
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception details</param>
        private void ShowDetailedErrorDialog(string title, string message, System.Exception exception)
        {
            string detailedMessage = $"Error: {message}\n\n";
            
            if (exception != null)
            {
                detailedMessage += $"Exception Type: {exception.GetType().Name}\n";
                detailedMessage += $"Message: {exception.Message}\n";
                detailedMessage += $"Stack Trace:\n{exception.StackTrace}\n";
                
                if (exception.InnerException != null)
                {
                    detailedMessage += $"\nInner Exception: {exception.InnerException.GetType().Name}\n";
                    detailedMessage += $"Inner Message: {exception.InnerException.Message}\n";
                }
            }

            // Create a scrollable text area for long error messages
            var detailWindow = EditorWindow.GetWindow<ErrorDetailWindow>("Error Details");
            detailWindow.SetErrorDetails(title, detailedMessage);
            detailWindow.Show();
        }

        /// <summary>
        /// Enhanced operation execution with comprehensive error handling and user feedback
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="operationName">Name of the operation for logging</param>
        /// <param name="showProgressBar">Whether to show progress bar</param>
        /// <returns>True if operation succeeded</returns>
        private bool ExecuteOperationWithFeedback(System.Action operation, string operationName, bool showProgressBar = true)
        {
            try
            {
                if (showProgressBar)
                {
                    EditorUtility.DisplayProgressBar("Material Property Modifier", $"Executing {operationName}...", 0.5f);
                }

                AddToOperationLog($"Starting {operationName}...");
                modifier.LogWithContext(LogLevel.Info, "WindowOperation", $"Starting {operationName}");

                operation();

                AddToOperationLog($"✓ {operationName} completed successfully");
                modifier.LogWithContext(LogLevel.Info, "WindowOperation", $"{operationName} completed successfully");
                
                return true;
            }
            catch (System.Exception ex)
            {
                AddToOperationLog($"✗ {operationName} failed: {ex.Message}");
                modifier.LogWithContext(LogLevel.Error, "WindowOperation", $"{operationName} failed", ex.Message);
                ShowErrorDialog($"{operationName} Failed", $"An error occurred while executing {operationName}:", ex);
                return false;
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// Perform comprehensive health check and display results
        /// </summary>
        private void PerformHealthCheck()
        {
            ExecuteOperationWithFeedback(() =>
            {
                var healthCheckResult = modifier.PerformHealthCheck();
                
                if (healthCheckResult.IsSuccess)
                {
                    var health = healthCheckResult.Data;
                    string healthMessage = $"System Health Check Results:\n\n";
                    healthMessage += $"Overall Health: {(health.OverallHealth ? "HEALTHY" : "UNHEALTHY")}\n";
                    healthMessage += $"Health Score: {health.HealthScore:P1}\n";
                    healthMessage += $"Passed Checks: {health.PassedChecks}/{health.TotalChecks}\n\n";

                    if (health.FailedChecks.Count > 0)
                    {
                        healthMessage += "Failed Checks:\n";
                        foreach (var failedCheck in health.FailedChecks)
                        {
                            healthMessage += $"• {failedCheck}\n";
                        }
                    }

                    if (health.OverallHealth)
                    {
                        EditorUtility.DisplayDialog("Health Check - Healthy", healthMessage, "OK");
                    }
                    else
                    {
                        ShowErrorDialog("Health Check - Issues Found", healthMessage);
                    }
                }
                else
                {
                    ShowErrorDialog("Health Check Failed", "Could not perform system health check", healthCheckResult.Exception);
                }
            }, "Health Check", true);
        }

        /// <summary>
        /// Execute integrated workflow with comprehensive error handling
        /// </summary>
        /// <param name="previewOnly">Whether to only generate preview</param>
        private void ExecuteIntegratedWorkflow(bool previewOnly = false)
        {
            ExecuteOperationWithFeedback(() =>
            {
                var workflowResult = modifier.ExecuteIntegratedWorkflow(
                    folderPath, 
                    selectedShader, 
                    propertyName, 
                    propertyValue, 
                    previewOnly
                );

                if (workflowResult.IsSuccess)
                {
                    var workflow = workflowResult.Data;
                    
                    // Update UI with results
                    foundMaterials = workflow.DiscoveredMaterials;
                    modificationPreview = workflow.Preview;
                    
                    string resultMessage = $"Integrated Workflow Results:\n\n";
                    resultMessage += $"Materials Found: {workflow.MaterialCount}\n";
                    resultMessage += $"Materials to Modify: {workflow.MaterialsToModify}\n";
                    resultMessage += $"Materials to Skip: {workflow.MaterialsToSkip}\n";
                    
                    if (!previewOnly && workflow.ModificationsApplied)
                    {
                        resultMessage += $"Successful Modifications: {workflow.SuccessfulModifications}\n";
                        resultMessage += $"Failed Modifications: {workflow.FailedModifications}\n";
                    }

                    AddToOperationLog($"Integrated workflow completed: {resultMessage.Replace("\n", " | ")}");
                    
                    if (!previewOnly && workflow.ModificationsApplied)
                    {
                        EditorUtility.DisplayDialog("Workflow Complete", resultMessage, "OK");
                    }
                }
                else
                {
                    ShowErrorDialog("Integrated Workflow Failed", workflowResult.ErrorMessage, workflowResult.Exception);
                }
            }, previewOnly ? "Integrated Preview" : "Integrated Workflow", true);
        }
    }

    /// <summary>
    /// Separate window for displaying detailed error information
    /// </summary>
    public class ErrorDetailWindow : EditorWindow
    {
        private string errorTitle;
        private string errorDetails;
        private Vector2 scrollPosition;

        public void SetErrorDetails(string title, string details)
        {
            errorTitle = title;
            errorDetails = details;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(errorTitle, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(errorDetails, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Copy to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = $"{errorTitle}\n\n{errorDetails}";
                Debug.Log("Error details copied to clipboard");
            }
            
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}