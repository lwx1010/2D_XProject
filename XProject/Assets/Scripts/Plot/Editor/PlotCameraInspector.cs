using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlotCamera))]
public class PlotCameraInspector : Editor {

    public override void OnInspectorGUI()
    {
        SerializedProperty copyMainCam = this.serializedObject.FindProperty("IsMainCamera");

        GUI.changed = false;

        EditorGUILayout.PropertyField(copyMainCam, new GUIContent("Main Camera"));

//        SerializedProperty cullingMask = this.serializedObject.FindProperty("CullingMask");
//        EditorGUILayout.PropertyField(cullingMask, new GUIContent("Culling Mask"));
//        
        if(GUI.changed)
            this.serializedObject.ApplyModifiedProperties();
    }
}
