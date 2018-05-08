using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CustomEditor(typeof(SetPositionEvent))]
public class SetPositionInspector : Editor
{
    // Properties
    private SerializedObject setPostion;
    private SerializedProperty fireTime;
    private SerializedProperty position;
    private SerializedProperty rotation;
    private SerializedProperty scale;
    private SerializedProperty editorRevert;
    private SerializedProperty runtimeRevert;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    #endregion

    public void OnEnable()
    {
        setPostion = new SerializedObject(this.target);
        this.fireTime = setPostion.FindProperty("firetime");
        this.position = setPostion.FindProperty("Position");
        this.rotation = setPostion.FindProperty("Rotation");
        this.scale = setPostion.FindProperty("Scale");

        this.editorRevert = setPostion.FindProperty("editorRevertMode");
        this.runtimeRevert = setPostion.FindProperty("runtimeRevertMode");
    }

    public override void OnInspectorGUI()
    {
        setPostion.Update();
        
        SetPositionEvent soe = target as SetPositionEvent;

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
        EditorGUILayout.PropertyField(position);
        EditorGUILayout.PropertyField(rotation);
        EditorGUILayout.PropertyField(scale);
        EditorGUILayout.PropertyField(editorRevert);
        EditorGUILayout.PropertyField(runtimeRevert);

        if (GUILayout.Button("To Current"))
        {
            Transform actorTrans = soe.ActorTrackGroup.Actor;
            this.position.vector3Value = actorTrans.position;
            this.rotation.vector3Value = actorTrans.rotation.eulerAngles;
            this.scale.vector3Value = actorTrans.localScale;
        }
        setPostion.ApplyModifiedProperties();
    }
}
