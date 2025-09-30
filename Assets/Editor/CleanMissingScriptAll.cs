#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

public static class CleanupMissingScripts {
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Open Scenes")]
    static void CleanOpenScenes() {
        int removed = 0;
        for (int s = 0; s < EditorSceneManager.sceneCount; s++) {
            var scene = EditorSceneManager.GetSceneAt(s);
            foreach (var root in scene.GetRootGameObjects()) {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            }
            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log($"[Cleanup] Removed {removed} missing scripts in open scenes.");
    }

    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Prefabs")]
    static void CleanPrefabs() {
        var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.EndsWith(".prefab"));
        int removedTotal = 0;
        foreach (var path in paths) {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0) {
                removedTotal += removed;
                EditorUtility.SetDirty(go);
            }
        }
        if (removedTotal > 0) AssetDatabase.SaveAssets();
        Debug.Log($"[Cleanup] Removed {removedTotal} missing scripts in prefabs.");
    }
}
#endif
