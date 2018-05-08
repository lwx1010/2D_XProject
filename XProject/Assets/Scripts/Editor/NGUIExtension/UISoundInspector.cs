using UnityEngine;
using System.Collections;
using LuaFramework;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UISound))]
public class UISoundInspector : Editor {

    public override void OnInspectorGUI()
    {
        SerializedProperty audioNameSP = serializedObject.FindProperty("audioName");
        int selectIndex = 0;
        for (int i = 0; i < AppConst.UISoundConfig.Length; i++)
        {
            if (AppConst.UISoundConfig[i] == audioNameSP.stringValue)
            {
                selectIndex = i;
                break;
            }
        }

        string lastName = audioNameSP.stringValue;
        EditorGUIUtility.labelWidth = 120f;
        GUI.changed = false;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("AudioName" , GUILayout.Width(115f));
        selectIndex = EditorGUILayout.Popup(selectIndex, AppConst.UISoundConfig);
        audioNameSP.stringValue = AppConst.UISoundConfig[selectIndex];
        EditorGUILayout.EndHorizontal();
        
        SerializedProperty triggerSP = serializedObject.FindProperty("trigger");
        EditorGUILayout.PropertyField(triggerSP, new GUIContent("Trigger"));

        if(GUI.changed)
            serializedObject.ApplyModifiedProperties();
    }
}
