using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using CinemaDirector;
using CinemaDirectorControl.Utility;
using LuaFramework;
using RSG;

/// <summary>
/// A custom inspector for a cutscene.
/// </summary>
[CustomEditor(typeof(ActorTrackGroup), true)]
public class ActorTrackGroupInspector : Editor
{
    private SerializedProperty actorTypeProperty;
    private SerializedProperty actorProperty;
    private SerializedProperty assetPathProperty;
    private SerializedProperty optimizable;

    private bool containerFoldout = true;
    private Texture inspectorIcon = null;

    #region Language
    //GUIContent ordinalContent = new GUIContent("Ordinal", "The ordinal value of this container, for sorting containers in the timeline.");
    GUIContent addTrackContent = new GUIContent("Add New Track", "Add a new track to this actor track group.");

    GUIContent reflushEntity = new GUIContent("Reflush", "Load a new Model.");

    GUIContent tracksContent = new GUIContent("Actor Tracks", "The tracks associated with this Actor Group.");

    private const string RES_PATH = "Assets/Res/";
    private const string EFFECT_SRC_PATH = "Assets/Effect/";
    private const string GEN_EFFECT_PATH = "Assets/Res/Prefab/Other/Gen/";
    #endregion

    /// <summary>
    /// Load texture assets on awake.
    /// </summary>
    private void Awake()
    {
        if (inspectorIcon == null)
        {
            inspectorIcon = Resources.Load<Texture>("Director_InspectorIcon");
        }
        if (inspectorIcon == null)
        {
            Debug.Log("Inspector icon missing from Resources folder.");
        }
    }

    /// <summary>
    /// On inspector enable, load the serialized properties
    /// </summary>
    private void OnEnable()
    {
        this.actorTypeProperty = base.serializedObject.FindProperty("actorType");
        this.actorProperty = base.serializedObject.FindProperty("actor");
        this.assetPathProperty = base.serializedObject.FindProperty("assetPath");
        this.optimizable = serializedObject.FindProperty("canOptimize");
    }

    /// <summary>
    /// Draw the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        ActorTrackGroup actorGroup = base.serializedObject.targetObject as ActorTrackGroup;
        TimelineTrack[] tracks = actorGroup.GetTracks();

        Cutscene cutscene = actorGroup.Cutscene;

        bool isCutsceneActive = false;
        if (cutscene == null)
        {
            EditorGUILayout.HelpBox("Track Group must be a child of a Cutscene in the hierarchy", MessageType.Error);
        }
        else
        {
            isCutsceneActive = !(cutscene.State == Cutscene.CutsceneState.Inactive);
            if (isCutsceneActive)
            {
                EditorGUILayout.HelpBox("Cutscene is Active. Actors cannot be altered at the moment.", MessageType.Info);
            }
        }

        int actorType = this.actorTypeProperty.enumValueIndex;
        ActorTrackGroup.ActorType newActorType = (ActorTrackGroup.ActorType)EditorGUILayout.EnumPopup(new GUIContent("Actor Type") , 
                                                    (ActorTrackGroup.ActorType) actorType);
        this.actorTypeProperty.enumValueIndex = (int)newActorType;

        GUI.enabled = !isCutsceneActive;
        
        if (newActorType == ActorTrackGroup.ActorType.Static)
            EditorGUILayout.PropertyField(actorProperty, new GUIContent("actor"));
        else
        {
            UnityEngine.Object obj = EditorGUILayout.ObjectField(new GUIContent("Asset") , actorProperty.objectReferenceValue, typeof(UnityEngine.Object) , false);

            if (obj != null && obj != actorProperty.objectReferenceValue)
            {
                string srcPath = AssetDatabase.GetAssetPath(obj);
                string fileName = Path.GetFileName(srcPath);

                string destPath = srcPath;
                if (destPath.StartsWith(EFFECT_SRC_PATH))
                {
                    if (!Directory.Exists(GEN_EFFECT_PATH))
                        Directory.CreateDirectory(GEN_EFFECT_PATH);

                    destPath = string.Concat(GEN_EFFECT_PATH, fileName);
                    File.Copy(srcPath, destPath, true);
                    AssetDatabase.ImportAsset(destPath);
                    AssetDatabase.Refresh();
                }
                string relativePath = destPath.Replace(RES_PATH, "");
                bool isGen = this.assetPathProperty.stringValue != relativePath;
                this.assetPathProperty.stringValue = relativePath;

                if (isGen)
                {
                    Initialize(actorGroup);

                    actorProperty.objectReferenceValue = actorGroup.Actor;
                }          
            }
            EditorGUILayout.PropertyField(assetPathProperty);
        }
        GUI.enabled = true;

        EditorGUILayout.PropertyField(optimizable);
        if (tracks.Length > 0)
        {
            containerFoldout = EditorGUILayout.Foldout(containerFoldout, tracksContent);
            if (containerFoldout)
            {
                EditorGUI.indentLevel++;

                foreach (TimelineTrack track in tracks)
                {
                    EditorGUILayout.BeginHorizontal();
                    track.name = EditorGUILayout.TextField(track.name);
                    if (GUILayout.Button(inspectorIcon, GUILayout.Width(24)))
                    {
                        Selection.activeObject = track;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }
        if (GUILayout.Button(addTrackContent))
        {
            CutsceneControlHelper.ShowAddTrackContextMenu(actorGroup);
        }

        if (newActorType == ActorTrackGroup.ActorType.Dynamic && GUILayout.Button(reflushEntity))
        {
            Initialize(actorGroup);
        }
        base.serializedObject.ApplyModifiedProperties();        
    }

    public static void Initialize(ActorTrackGroup actorGroup)
    {
        if (actorGroup.ActorTrackType == ActorTrackGroup.ActorType.Static) return;

        Transform actorTrans = actorGroup.transform.Find("_Entity");

        if (actorTrans != null)
        {
            int childCount = actorTrans.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform childTrans = actorTrans.GetChild(i);
                GameObject.DestroyImmediate(childTrans.gameObject);
            }
            actorGroup.Actor = actorTrans;
        }

        if (actorTrans == null)
        {
            GameObject actorObj = new GameObject("_Entity");
            ////Util.SetParent(actorObj, actorGroup.gameObject);
            actorGroup.Actor = actorObj.transform;
        }

        string destPath = string.Concat(RES_PATH, actorGroup.AssetPath);
        
        if (destPath.EndsWith(".prefab"))
        {
            GameObject actor = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);
            actor = GameObject.Instantiate(actor) as GameObject;
            ////Util.SetParent(actor, actorGroup.Actor.gameObject);
            actor.SetActive(true);

            Animator animator = actor.GetComponent<Animator>();
            if (animator) animator.enabled = true;

            //EntityTrackGroup.AddFastShadow(actor);
        }
        else if (destPath.EndsWith(".ogg"))
        {
            string fileName = Path.GetFileNameWithoutExtension(destPath);
            GameObject audioObj = new GameObject(fileName);
            AudioSource audioSrc = audioObj.AddComponent<AudioSource>();
            audioSrc.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destPath);
            //Util.SetParent(audioObj, actorGroup.Actor.gameObject);

            PlayAudioEvent[] playAudios = actorGroup.GetComponentsInChildren<PlayAudioEvent>();
            foreach (PlayAudioEvent playAudio in playAudios)
            {
                playAudio.Duration = audioSrc.clip.length;
            }
        }
    }
}
