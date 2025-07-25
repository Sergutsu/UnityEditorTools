using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabChildReplacer : EditorWindow
{
    private GameObject rootPrefab;
    private GameObject replacementPrefab;
    private string searchFilter = "";
    private Vector2 scrollPosition;
    private List<string> previewPaths = new List<string>();
    private bool showPreview = false;

    [MenuItem("Window/Prefab Child Replacer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabChildReplacer>("Prefab Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Child Replacement Tool", EditorStyles.boldLabel);
        
        rootPrefab = (GameObject)EditorGUILayout.ObjectField("Root Prefab", rootPrefab, typeof(GameObject), false);
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab", replacementPrefab, typeof(GameObject), false);
        searchFilter = EditorGUILayout.TextField("Child Name Filter", searchFilter);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Preview Replacements"))
        {
            PreviewReplacements();
        }
        
        if (GUILayout.Button("Execute Replacement"))
        {
            ExecuteReplacement();
        }

        // Preview section
        if (showPreview)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Matching Objects (Preview):");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string path in previewPaths)
            {
                EditorGUILayout.LabelField(path);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void PreviewReplacements()
    {
        previewPaths.Clear();
        showPreview = false;

        if (rootPrefab == null)
        {
            Debug.LogError("Root prefab is not assigned!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(rootPrefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(prefabRoot.transform);
        
        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            
            if (current.name.Contains(searchFilter))
            {
                previewPaths.Add(GetHierarchyPath(current));
            }

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        PrefabUtility.UnloadPrefabContents(prefabRoot);
        showPreview = true;
        Debug.Log($"Found {previewPaths.Count} matching children in prefab");
    }

    private void ExecuteReplacement()
    {
        if (rootPrefab == null)
        {
            Debug.LogError("Root prefab is not assigned!");
            return;
        }
        
        if (replacementPrefab == null)
        {
            Debug.LogError("Replacement prefab is not assigned!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(rootPrefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        List<Transform> matches = new List<Transform>();
        bool modified = false;

        // Find all matching transforms
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(prefabRoot.transform);
        
        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            
            if (current.name.Contains(searchFilter))
            {
                matches.Add(current);
            }

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        if (matches.Count == 0)
        {
            Debug.Log("No matching children found");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }

        // Replace matches
        foreach (Transform original in matches)
        {
            Transform parent = original.parent;
            Vector3 position = original.position;
            Quaternion rotation = original.rotation;
            Vector3 scale = original.localScale;
            int siblingIndex = original.GetSiblingIndex();

            // Create replacement
            GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
            newInstance.transform.SetParent(parent);
            newInstance.transform.SetSiblingIndex(siblingIndex);
            newInstance.transform.SetPositionAndRotation(position, rotation);
            newInstance.transform.localScale = scale;
            
            // Destroy original
            DestroyImmediate(original.gameObject);
            modified = true;
        }

        if (modified)
        {
            // Save changes back to prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Debug.Log($"Replaced {matches.Count} children in prefab");
        }

        PrefabUtility.UnloadPrefabContents(prefabRoot);
        showPreview = false;
        previewPaths.Clear();
    }

    private string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

}