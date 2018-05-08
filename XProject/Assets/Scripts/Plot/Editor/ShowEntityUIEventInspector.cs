using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ShowEntityUIEvent))]
public class ShowEntityUIEventInspector : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty uiNameProperty;
    private SerializedProperty argsProperty;
    private SerializedProperty dirProperty;
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
        this.dirProperty = serObj.FindProperty("Direction");

        for (int i = 0; i < PlotUIPanels.EntityPanels.Length; i++)
        {
            paneltSelectionList.Add(new GUIContent(PlotUIPanels.EntityPanels[i]));
        }
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();
        
        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);

        int selection = 0;
        if (!string.IsNullOrEmpty(uiNameProperty.stringValue))
        {
            string curUIName = uiNameProperty.stringValue;
            for (int i = 0; i < PlotUIPanels.EntityPanels.Length; i++)
            {
                if (curUIName == PlotUIPanels.EntityPanels[i])
                {
                    selection = i;
                    break;
                }
            }            
        }

        selection = EditorGUILayout.Popup(new GUIContent("UI Name"), selection, paneltSelectionList.ToArray());
        uiNameProperty.stringValue = PlotUIPanels.EntityPanels[selection];
//        EditorGUILayout.PropertyField(uiNameProperty);

        EditorGUILayout.PropertyField(argsProperty);

        EditorGUILayout.PropertyField(dirProperty);

        serObj.ApplyModifiedProperties();
    }
}
