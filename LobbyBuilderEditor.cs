using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LobbyBuilder))]
public class LobbyBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        if (GUILayout.Button("BUILD LOBBY", GUILayout.Height(36)))
        {
            foreach (var t in targets)
            {
                var b = (LobbyBuilder)t;
                Undo.RegisterFullObjectHierarchyUndo(b.gameObject, "Build Lobby");
                b.Build();
                EditorUtility.SetDirty(b);
                EditorSceneManager.MarkSceneDirty(b.gameObject.scene);
            }
        }

        if (GUILayout.Button("CLEAR"))
        {
            foreach (var t in targets)
            {
                var b = (LobbyBuilder)t;
                Undo.RegisterFullObjectHierarchyUndo(b.gameObject, "Clear Lobby");
                b.Clear();
                EditorSceneManager.MarkSceneDirty(b.gameObject.scene);
            }
        }
    }

    [MenuItem("Tools/Lobby/Create Lobby In Scene")]
    static void CreateInScene()
    {
        var go = new GameObject("LobbyRoot");
        var b = go.AddComponent<LobbyBuilder>();
        Undo.RegisterCreatedObjectUndo(go, "Create Lobby");
        b.Build();
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
}
