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
        /// Initialize the window when it's first opened
        /// </summary>
        private void OnEnable()
        {
            InitializeWindow();
        }

        /// <summary>
        /// Initialize window components and state
        /// </summary>
        private void InitializeWindow()
        {
            if (modifier == null)
            {
                modifier = new MaterialPropertyModifier();
            }
            
            // Initialize shader list
            RefreshShaderList();
            
            // Initialize folder from current selection if applicable
            InitializeFolderFromSelection();
            
            isInitialized = true;
            
            // Set window title and icon
            titleContent = new GUIContent("Material Property Modifier", "Batch modify material properties");
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
    }
}