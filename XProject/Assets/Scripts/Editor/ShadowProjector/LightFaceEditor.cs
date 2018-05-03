using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightFace))] 
public class LightFaceEditor : Editor {

    string[] _ShadowResOptions = new string[] { "Very Low", "Low", "Medium", "High", "Very High" };

    string[] _CullingOptions = new string[] { "None", "Caster bounds", "Projection volume" };

    public override void OnInspectorGUI() {

        LightFace lightFace = (LightFace)target;

        GUI.changed = false;
        SerializedProperty projectorDir = serializedObject.FindProperty("_GlobalProjectionDir");
        projectorDir.vector3Value = EditorGUILayout.Vector3Field("Global light direction", projectorDir.vector3Value, null);
        

        SerializedProperty shadowResolution = serializedObject.FindProperty("_GlobalShadowResolution");
        shadowResolution.intValue = EditorGUILayout.Popup("Global shadow resolution", shadowResolution.intValue, _ShadowResOptions);

        SerializedProperty shadowCullingMode = serializedObject.FindProperty("_GlobalShadowCullingMode");
        shadowCullingMode.enumValueIndex = EditorGUILayout.Popup("Global culling mode",shadowCullingMode.enumValueIndex, _CullingOptions);

        lightFace.EnableCutOff = EditorGUILayout.BeginToggleGroup("Cutoff shadow by distance?", lightFace.EnableCutOff);
        SerializedProperty cutOffDistance = serializedObject.FindProperty("_GlobalCutOffDistance");
        cutOffDistance.floatValue = EditorGUILayout.Slider("Global cutoff distance", cutOffDistance.floatValue, 1.0f, 10000.0f);
        EditorGUILayout.EndToggleGroup();
        
        SerializedProperty projectMaterial = serializedObject.FindProperty("_ProjectorMaterialShadow");
        EditorGUILayout.PropertyField(projectMaterial, new GUIContent("Projector Material"));

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            lightFace.OnProjectionDirChange();
            lightFace.OnShadowResolutionChange(shadowResolution.intValue);
        }
    }
}