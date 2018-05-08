using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CustomEditor(typeof(NGUIEqualsTextEvent))]
public class NGUIEqualsTextEventEditor : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty inputTex;
    private SerializedProperty position;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    GUIContent textContent = new GUIContent("Text");
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.inputTex = serObj.FindProperty("inputText");
        this.position = serObj.FindProperty("position");

    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        NGUIEqualsTextEvent mouseEvent = target as NGUIEqualsTextEvent;

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
        EditorGUILayout.PropertyField(inputTex , textContent);
        EditorGUILayout.PropertyField(position);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set"))
        {
            Transform actorTrans = ((UTestTrackGroup)mouseEvent.TimelineTrack.TrackGroup).Actor;
            actorTrans.localPosition = this.position.vector3Value;
        }
        GUI.color = Color.green;
        if (GUILayout.Button("Current"))
        {
            Transform actorTrans = ((UTestTrackGroup)mouseEvent.TimelineTrack.TrackGroup).Actor;
            this.position.vector3Value = actorTrans.localPosition;
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        serObj.ApplyModifiedProperties();
    }
}
