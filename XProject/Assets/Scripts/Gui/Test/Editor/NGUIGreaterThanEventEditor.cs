using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CustomEditor(typeof(NGUIGreaterThanEvent))]
public class NGUIGreaterThanEventEditor : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty value;
    private SerializedProperty position;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.value = serObj.FindProperty("value");
        this.position = serObj.FindProperty("position");
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        NGUIGreaterThanEvent mouseEvent = target as NGUIGreaterThanEvent;

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
        EditorGUILayout.PropertyField(value);
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
