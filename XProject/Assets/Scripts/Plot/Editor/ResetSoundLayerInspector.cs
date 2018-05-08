using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CustomEditor(typeof(ResetSoundLayerEvent))]
public class ResetSoundLayerInspector : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty soundLayerProperty;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    private GUIContent soundLayerContent = new GUIContent("SoundLayer");

    private string[] soundLayers = new string[]
    {
        "BGM","SKILL","TALK","UI","EFFECT","Story",
    };
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.soundLayerProperty = serObj.FindProperty("SoundLayer");
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);

        soundLayerProperty.intValue = EditorGUILayout.MaskField(soundLayerContent, soundLayerProperty.intValue, soundLayers);

        serObj.ApplyModifiedProperties();
    }
    
}
