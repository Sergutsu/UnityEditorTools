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
        /// Draw the main content area (placeholder for now)
        /// </summary>
        private void DrawMainContent()
        {
            // Placeholder content - will be expanded in subsequent tasks
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "This is the main Material Property Modifier window.\n\n" +
                "Features will be added in the following order:\n" +
                "• Folder and shader selection\n" +
                "• Property configuration\n" +
                "• Material list display\n" +
                "• Preview and apply controls",
                MessageType.Info
            );
            
            GUILayout.Space(SECTION_SPACING);
            
            // Status information
            DrawStatusSection();
            
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
    }
}