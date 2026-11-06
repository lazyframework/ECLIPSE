// Letakkan di folder: Assets/Editor/HUDBuilderEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HUDBuilder))]
public class HUDBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var b = (HUDBuilder)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Build / Rebuild HUD (Editor)", GUILayout.Height(32))) b.Build();
        if (GUILayout.Button("Clear HUD")) b.Clear();
        EditorGUILayout.HelpBox("Setelah Build, HUD tersimpan di scene. Edit manual di Hierarchy: HUD_Canvas > HUD_Root.\n" +
                                "Rebuild akan menimpa perubahan manual.", MessageType.Info);
    }
}

public static class HUDBuilderMenu
{
    [MenuItem("GameObject/UI/Project Eclipse HUD", false, 10)]
    static void Create(MenuCommand cmd)
    {
        var go = new GameObject("HUDBuilder");
        GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create HUD");
        var b = go.AddComponent<HUDBuilder>();
        b.buildOnAwake = false;
        b.Build();
        Selection.activeGameObject = go;
    }
}
#endif
