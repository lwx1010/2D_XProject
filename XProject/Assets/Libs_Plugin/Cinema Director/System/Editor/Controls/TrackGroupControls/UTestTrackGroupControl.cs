using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CutsceneTrackGroupAttribute(typeof(UTestTrackGroup))]
public class UTestTrackGroupControl : GenericTrackGroupControl
{
    public override void Initialize()
    {
        base.Initialize();
        LabelPrefix = styles.ActorGroupIcon.normal.background;
    }

    protected override void updateHeaderControl5(Rect position)
    {
        Transform actor = (TrackGroup.Behaviour as UTestTrackGroup).Actor;

        Color temp = GUI.color;

        GUI.color = (actor == null) ? Color.red : Color.green;
        int controlID = GUIUtility.GetControlID("UTestTrackGroup".GetHashCode(), FocusType.Passive, position);

        GUI.enabled = !(state.IsInPreviewMode && (actor == null));
        if (GUI.Button(position, string.Empty, styles.pickerStyle))
        {
            if (actor == null)
            {
                EditorGUIUtility.ShowObjectPicker<Transform>(actor, true, string.Empty, controlID);
            }
            else
            {
                Selection.activeGameObject = actor.gameObject;
            }
        }
        GUI.enabled = true;

        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            if (EditorGUIUtility.GetObjectPickerControlID() == controlID)
            {
                GameObject pickedObject = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                if (pickedObject != null)
                {
                    UTestTrackGroup atg = (TrackGroup.Behaviour as UTestTrackGroup);
                    Undo.RecordObject(atg, string.Format("Changed {0}", atg.name));
                    atg.Actor = pickedObject.transform;
                }
            }
        }
        GUI.color = temp;
    }

    private void focusActor()
    {
        Selection.activeTransform = (TrackGroup.Behaviour as UTestTrackGroup).Actor;
    }
}