using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(SceneViewCustom))]
public class SceneViewCustomEditor : Editor
{
#if UNITY_EDITOR
    SceneViewCustom sceneView;

    private SerializedProperty modelRootGo;
    private SerializedProperty effectRootGo;
    private SerializedProperty terrainRootGo;

    void OnEnable()
    {
        sceneView = target as SceneViewCustom;
        modelRootGo = serializedObject.FindProperty("modelRootGo");
        effectRootGo = serializedObject.FindProperty("effectRootGo");
        terrainRootGo = serializedObject.FindProperty("terrainRootGo");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(modelRootGo, new GUIContent("物件根节点GameObject"));
        EditorGUILayout.PropertyField(effectRootGo, new GUIContent("特效根节点GameObject"));
        EditorGUILayout.PropertyField(terrainRootGo, new GUIContent("地表根节点GameObject"));

        if (GUILayout.Button("输出场景物件prefab集合"))
            sceneView.CreateSceneObjectsPrefabs();
        if (GUILayout.Button("输出场景特效prefab集合"))
            sceneView.CreateSceneEffectsPrefabs();
        if (GUILayout.Button("输出场景物件json配置"))
            sceneView.GenerateSceneObjectsInfoJson();
        if (GUILayout.Button("输出场景特效json配置"))
            sceneView.GenerateSceneEffectsInfoJson();
        if (GUILayout.Button("输出场景地表json配置"))
            sceneView.GenerateSceneTerrainInfoJson();

        serializedObject.ApplyModifiedProperties();
    }
#endif
}
