using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CustomEditor(typeof(NGUIMouseEvent))]
public class NGUIMouseEventEditor : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty position;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.position = serObj.FindProperty("inputPosition");

    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        NGUIMouseEvent mouseEvent = target as NGUIMouseEvent;

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
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
