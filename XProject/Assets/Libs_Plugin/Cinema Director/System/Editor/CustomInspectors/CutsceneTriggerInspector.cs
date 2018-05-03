using UnityEditor;
using UnityEngine;
using CinemaDirector;

/// <summary>
/// A custom inspector for cutscene triggers.
/// </summary>
[CustomEditor(typeof(CutsceneTrigger), true)]
public class CutsceneTriggerInspector : Editor
{
    private SerializedObject trigger;

    private SerializedProperty startMethod;
    //    private SerializedProperty cutscene;
    private SerializedProperty loop;
    private SerializedProperty cutName;
//    private SerializedProperty skipButton;
    private SerializedProperty triggerValue;

    #region 
    private static GUIContent cutsceneIdContent = new GUIContent("Cutscene ID");
    private const string startMethodContent = "Start Method";
    private const string TriggerTypeContent = "Trigger Type";
    private const string TriggerValueContent = "Trigger Value";
    #endregion

    /// <summary>
    /// On inspector enable, load the serialized properties
    /// </summary>
    private void OnEnable()
    {
        trigger = new SerializedObject(this.target);

        startMethod = trigger.FindProperty("StartMethod");
        //        cutscene = trigger.FindProperty("Cutscene");
        loop = trigger.FindProperty("Loop");
        cutName = trigger.FindProperty("CutName"); 
        triggerValue = trigger.FindProperty("TriggerValue");
//        skipButton = trigger.FindProperty("SkipButtonName");
    }

    /// <summary>
    /// Draw the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        trigger.Update();

        EditorGUILayout.PropertyField(cutName , cutsceneIdContent);
//        EditorGUILayout.PropertyField(cutscene);
//
//        if (string.IsNullOrEmpty(cutName.stringValue) && cutscene.objectReferenceValue != null)
//        {
//            cutName.stringValue = cutscene.objectReferenceValue.name;
//        }
        StartMethod newStartMethod = (StartMethod) EditorGUILayout.EnumPopup(startMethodContent, (StartMethod)startMethod.enumValueIndex);
        
        if (newStartMethod != (StartMethod)startMethod.enumValueIndex)
        {
            startMethod.enumValueIndex = (int)newStartMethod;

            if (newStartMethod == StartMethod.OnTrigger)
            {
                CutsceneTrigger cutsceneTrigger = (this.target as CutsceneTrigger);
                Collider collider = cutsceneTrigger.gameObject.GetComponent<BoxCollider>();
                if (collider == null)
                    cutsceneTrigger.gameObject.AddComponent<BoxCollider>();
            }
            else
            {
                // Can't cleanly destroy collider yet.
                CutsceneTrigger cutsceneTrigger = (this.target as CutsceneTrigger);
                Collider collider = cutsceneTrigger.gameObject.GetComponent<BoxCollider>();
                if (collider != null)   collider.enabled = false;
            }
        }

        if (newStartMethod == StartMethod.OnTrigger)
        {
            CutsceneTrigger cutsceneTrigger = (this.target as CutsceneTrigger);
            Collider collider = cutsceneTrigger.gameObject.GetComponent<BoxCollider>();
            collider.enabled = true;
        }

        EditorGUILayout.PropertyField(loop);
//        EditorGUILayout.PropertyField(skipButton);

        trigger.ApplyModifiedProperties();
    }
}
