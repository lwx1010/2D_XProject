using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(FlowLightEffect), true)]
public class FlowLightEffectInspector : Editor {

    public override void OnInspectorGUI()
    {
        GUI.changed = false;
        EditorGUIUtility.labelWidth = 120f;
        //NGUIEditorTools.DrawProperty("Auto Cache", serializedObject, "autoCache");

        //NGUIEditorTools.DrawProperty("FlowTexure", serializedObject, "FlowTex");

//        EditorGUILayout.BeginHorizontal();
//        SerializedProperty texPathSP = serializedObject.FindProperty("FlowTexurePath");
//        EditorGUILayout.LabelField("Flow Texture Path" , GUILayout.Width(115f));
//        EditorGUILayout.TextField(texPathSP.stringValue);
//        if (GUILayout.Button("..." , GUILayout.Width(30)))
//        {
//            string path = EditorUtility.OpenFilePanel("选择", Application.dataPath + "/Resources/", "*.*");
//            if (texPathSP.stringValue != path)
//            {
//                string exs = Path.GetExtension(path);
//                path = path.Replace(Application.dataPath+"/Resources/", "").Replace(exs, "");
//                texPathSP.stringValue = path;
//            }
//        }
//        EditorGUILayout.EndHorizontal();

        SerializedProperty flowPowerSP = serializedObject.FindProperty("FlowPower");
        EditorGUILayout.Slider(flowPowerSP, 0.01f, 2, "Flow Light Power");

        SerializedProperty flowType = serializedObject.FindProperty("Type");
        //FlowLightEffect.FlowType ft =
        //    (FlowLightEffect.FlowType) EditorGUILayout.EnumPopup(new GUIContent("Direction"), (FlowLightEffect.FlowType)flowType.enumValueIndex);
        //flowType.enumValueIndex = (int) ft;
//        SerializedProperty starSP = serializedObject.FindProperty("mUvStart");
//        EditorGUILayout.Slider(starSP, 0f, 0.9f, "Start Position");
//
//        SerializedProperty maxSP = serializedObject.FindProperty("mUvMax_X");
//        EditorGUILayout.Slider(maxSP, 0.1f, 1, "Max Position");

        SerializedProperty speedSP = serializedObject.FindProperty("mUvSpeed");
        EditorGUILayout.Slider(speedSP, 0.01f, 2, "Speed");


        
        //NGUIEditorTools.DrawProperty("Time Inteval", serializedObject, "mTimeInteval" , GUILayout.Width(180f));

        if (GUI.changed)
            serializedObject.ApplyModifiedProperties();
    }
}
