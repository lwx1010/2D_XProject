using UnityEditor;
using UnityEngine;
using Assets.Scripts.Plot;
using CinemaDirector;

[CustomEditor(typeof(CombindBoneEvent))]
public class CombindBoneInspector : Editor
{
    // Properties
    private SerializedObject serObj;
    private SerializedProperty fireTime;
    private SerializedProperty targetProperty;
    private SerializedProperty boneNameProperty;
    private SerializedProperty positionProperty;
    private SerializedProperty rotationProperty;

    #region Language
    GUIContent firetimeContent = new GUIContent("Firetime", "The time in seconds at which this event is fired.");
    #endregion

    public void OnEnable()
    {
        serObj = new SerializedObject(this.target);
        this.fireTime = serObj.FindProperty("firetime");
        this.targetProperty = serObj.FindProperty("Target");
        this.boneNameProperty = serObj.FindProperty("BoneName");
        this.positionProperty = serObj.FindProperty("OffsetPosition");
        this.rotationProperty = serObj.FindProperty("OffsetRotation");
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        EditorGUILayout.PropertyField(this.fireTime, firetimeContent);
        
        EditorGUILayout.PropertyField(targetProperty);
        EditorGUILayout.PropertyField(boneNameProperty);
        EditorGUILayout.PropertyField(positionProperty);
        EditorGUILayout.PropertyField(rotationProperty);


        if (GUILayout.Button("Form Current"))
        {
            CombindBoneEvent cbe = this.target as CombindBoneEvent;
            if (cbe.Target == null) return;

            Transform targetActorTrans = cbe.GetTragetActorTransform();
            positionProperty.vector3Value = targetActorTrans.localPosition;
            rotationProperty.vector3Value = targetActorTrans.localRotation.eulerAngles;
        }

        if (GUILayout.Button("To Current"))
        {
            CombindBoneEvent cbe = this.target as CombindBoneEvent;
            if (cbe.Target == null) return;

            Transform targetActorTrans = cbe.GetTragetActorTransform();
            Transform selfActorTrans = GetActorTransform(cbe.TimelineTrack.TrackGroup);

            targetActorTrans.SetParent(cbe.findChild(selfActorTrans , cbe.BoneName));
            targetActorTrans.localPosition = positionProperty.vector3Value ;
            targetActorTrans.localRotation = Quaternion.Euler(rotationProperty.vector3Value);
        }

        serObj.ApplyModifiedProperties();
    }


    public Transform GetActorTransform(TrackGroup targetGroup)
    {
        ActorTrackGroup targetTrackGroup = targetGroup as ActorTrackGroup;
        if (targetTrackGroup) return targetTrackGroup.Actor;

        EntityTrackGroup entityTrackGroup = targetGroup as EntityTrackGroup;
        if (entityTrackGroup) return entityTrackGroup.Actor;

        Debug.LogError("找不到目标对应的演员！");
        return null;
    }
}
