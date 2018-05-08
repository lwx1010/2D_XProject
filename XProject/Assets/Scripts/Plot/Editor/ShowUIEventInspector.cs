using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ShowUIEvent))]
public class ShowUIEventInspector : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty uiNameProperty;
    private SerializedProperty argsProperty;
    private SerializedProperty editorRevert;
    private SerializedProperty runtimeRevert;
    private SerializedProperty pauseCutsceneProperty;
    private List<GUIContent> paneltSelectionList = new List<GUIContent>();

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.uiNameProperty = serObj.FindProperty("UIName");
        this.argsProperty = serObj.FindProperty("Args");
        this.editorRevert = serObj.FindProperty("editorRevertMode");
        this.runtimeRevert = serObj.FindProperty("runtimeRevertMode");
        this.pauseCutsceneProperty = serObj.FindProperty("PauseCutscene");


        for (int i = 0; i < PlotUIPanels.PlotPanels.Length; i++)
        {
            paneltSelectionList.Add(new GUIContent(PlotUIPanels.PlotPanels[i]));
        }
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();
        
        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
        EditorGUILayout.PropertyField(this.pauseCutsceneProperty);
        int selection = 0;
        if (!string.IsNullOrEmpty(uiNameProperty.stringValue))
        {
            string curUIName = uiNameProperty.stringValue;
            for (int i = 0; i < PlotUIPanels.PlotPanels.Length; i++)
            {
                if (curUIName == PlotUIPanels.PlotPanels[i])
                {
                    selection = i;
                    break;
                }
            }            
        }

        selection = EditorGUILayout.Popup(new GUIContent("UI Name"), selection, paneltSelectionList.ToArray());
        
        if (selection == 0)
        {
            if(ArrayUtility.Contains(PlotUIPanels.PlotPanels, uiNameProperty.stringValue))
                uiNameProperty.stringValue = "";
            EditorGUILayout.PropertyField(uiNameProperty , new GUIContent("Other UI"));
        }
        else
        {
            uiNameProperty.stringValue = PlotUIPanels.PlotPanels[selection];
        }

        EditorGUILayout.PropertyField(argsProperty);

        EditorGUILayout.PropertyField(editorRevert);
        EditorGUILayout.PropertyField(runtimeRevert);

        serObj.ApplyModifiedProperties();
    }
}
