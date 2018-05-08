using UnityEditor;
using UnityEngine;
using CinemaDirector;
using System;
using System.IO;
using CinemaDirectorControl.Utility;

/// <summary>
/// A custom inspector for a cutscene.
/// </summary>
[CustomEditor(typeof(Cutscene))]
public class CutsceneInspector : Editor
{
    private SerializedProperty debug;
    private SerializedProperty duration;
    private SerializedProperty isLooping;
    private SerializedProperty isSkippable;
    private SerializedProperty canOptimize;

    //private SerializedProperty runningTime;
    //private SerializedProperty playbackSpeed;
    private bool containerFoldout = true;

    private Texture inspectorIcon = null;

    #region Language
        GUIContent debugContent = new GUIContent("Debug", "The debug of the cutscene when play.");
        GUIContent durationContent = new GUIContent("Duration", "The duration of the cutscene in seconds.");
        GUIContent loopingContent = new GUIContent("Loop", "Will the Cutscene loop when finished playing.");
        GUIContent skippableContent = new GUIContent("Skippable", "Can the Cutscene be skipped.");
        GUIContent optimizeContent = new GUIContent("Optimize", "Enable when Cutscene does not have Track Groups added/removed during playtime.");

        GUIContent groupsContent = new GUIContent("Track Groups", "Organizational units of a cutscene.");
        GUIContent addGroupContent = new GUIContent("Add Group", "Add a new container to the cutscene.");
        GUIContent reflushEntityContent = new GUIContent("Reflush Entitys", "Reflush every actor.");
        GUIContent resetContent = new GUIContent("Toggle Entitys", "Reset every actor to begin state.");
    GUIContent clearContent = new GUIContent("Clear", "Clear Cutscene .");
    GUIContent exportContent = new GUIContent("Export", "Export Cutscene to local prefab.");
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
        this.debug = base.serializedObject.FindProperty("debug");
        this.duration = base.serializedObject.FindProperty("duration");
        this.isLooping = base.serializedObject.FindProperty("isLooping");
        this.isSkippable = base.serializedObject.FindProperty("isSkippable");
        this.canOptimize = base.serializedObject.FindProperty("canOptimize");
    }

    /// <summary>
    /// Draw the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel(new GUIContent("Director"));
        if (GUILayout.Button("Open Director"))
        {
            DirectorWindow window = EditorWindow.GetWindow(typeof(DirectorWindow)) as DirectorWindow;
            window.FocusCutscene(base.serializedObject.targetObject as Cutscene);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(this.debug, debugContent);
        EditorGUILayout.PropertyField(this.duration, durationContent);
        EditorGUILayout.PropertyField(this.isLooping, loopingContent);
        EditorGUILayout.PropertyField(this.isSkippable, skippableContent);
        EditorGUILayout.PropertyField(this.canOptimize, optimizeContent);

        containerFoldout = EditorGUILayout.Foldout(containerFoldout, groupsContent);

        if (containerFoldout)
        {
            EditorGUI.indentLevel++;
            Cutscene c = base.serializedObject.targetObject as Cutscene;

            foreach (TrackGroup container in c.TrackGroups)
            {
                EditorGUILayout.BeginHorizontal();
                
                container.name = EditorGUILayout.TextField(container.name);
                //GUILayout.Button("add", GUILayout.Width(16));
                if (GUILayout.Button(inspectorIcon, GUILayout.Width(24)))
                {
                    Selection.activeObject = container;
                }
                //GUILayout.Button("u", GUILayout.Width(16));
                //GUILayout.Button("d", GUILayout.Width(16));
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.ObjectField(container.name, container, typeof(TrackGroup), true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(addGroupContent , GUILayout.Width(100)))
            {
                CutsceneControlHelper.ShowAddTrackGroupContextMenu(c);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button(reflushEntityContent))
//        {
//            Cutscene cutscene = base.target as Cutscene;
//            EntityTrackGroup[] actorGroupArr = cutscene.GetComponentsInChildren<EntityTrackGroup>();
//
//            foreach (EntityTrackGroup actorGroup in actorGroupArr)
//            {
//                EntityTrackGroupInspector.Initialize(actorGroup, actorGroup.Model,actorGroup.Wings, 
//                                                     actorGroup.Weapon, actorGroup.Horse);
//            }
//        }

        if (GUILayout.Button(resetContent))
        {
            Cutscene cutscene = base.target as Cutscene;
            resetCutscene(cutscene);
        }

        if (GUILayout.Button(clearContent))
        {
            clearTrackGroups((Cutscene) base.target);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(exportContent))
        {
            exportPrefab();
        }
        base.serializedObject.ApplyModifiedProperties();
    }



    #region -----编辑状态reset/reflush------
    /// <summary>
    /// 重置Cutscene的子结点数据
    /// </summary>
    /// <param name="cutscene"></param>
    private void resetCutscene(Cutscene cutscene)
    {
        ActorTrackGroup[] actorGroupArr = cutscene.GetComponentsInChildren<ActorTrackGroup>();
        
        foreach (ActorTrackGroup actorGroup in actorGroupArr)
        {
            if(actorGroup is EntityTrackGroup)
                this.resetEnityTrackGroup((EntityTrackGroup)actorGroup);
            else
                ActorTrackGroupInspector.Initialize(actorGroup);
        }

        this.resetAudioTrackGroup(cutscene);
    }


    private void clearTrackGroups(Cutscene cutscene)
    {
        ActorTrackGroup[] actorGroupArr = cutscene.GetComponentsInChildren<ActorTrackGroup>();

        foreach (ActorTrackGroup actorGroup in actorGroupArr)
        {
            if(actorGroup.Actor == null)    continue;

            Transform actorTrans = actorGroup.Actor;
            for (int i = actorTrans.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(actorTrans.GetChild(i).gameObject);
            }
        }
    }

    private void resetEnityTrackGroup(EntityTrackGroup actorGroup)
    {
        if (actorGroup.Actor == null) return;
        Vector3 resetPos = Vector3.one * 5555;
        Transform _temp = actorGroup.Actor.parent.Find("_temp");
        if (_temp == null)
        {
            _temp = new GameObject("_temp").transform;
            _temp.SetParent(actorGroup.Actor.parent);
        }

        if (actorGroup.Actor.localPosition == resetPos)
        {
            actorGroup.Actor.localPosition = _temp.localPosition;
            EntityTrackGroupInspector.Initialize(actorGroup, actorGroup.Model, actorGroup.Wings,
                                actorGroup.Weapon, actorGroup.Horse);
        }
        else
        {
            _temp.localPosition = actorGroup.Actor.localPosition;
            actorGroup.Actor.localPosition = resetPos;
            clearTransform(actorGroup.transform, "_Entity", true);
        }
    }

    private void resetAudioTrackGroup(Cutscene cutscene)
    {
        AudioTrack[] audioTracks = cutscene.GetComponentsInChildren<AudioTrack>();
        
        foreach (AudioTrack audioGroup in audioTracks)
        {
            foreach (TimelineItem timelineItem in audioGroup.GetTimelineItems())
            {
                CinemaAudio audio = timelineItem as CinemaAudio;
                CinemaAudioInspector.Initialize(audio);
            }
        }
    }
    #endregion

    /// <summary>
    /// 导出预设到本地磁盘
    /// </summary>
    private void exportPrefab()
    {
        Cutscene cutscene = base.target as Cutscene;
        string prefabPath = string.Concat("Assets/Res/Prefab/Cutscene/", cutscene.name, ".prefab");

        if (File.Exists(prefabPath))
        {
            bool result = EditorUtility.DisplayDialog("警告", "本地存在相同的文件,是否覆盖?", "覆盖", "取消");
            if (result)
            {
                exportPrefab(cutscene , prefabPath);
            }
            return;
        }
        exportPrefab(cutscene, prefabPath);
    }

    /// <summary>
    /// Export prefab to local desk 
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="path"></param>
    private void exportPrefab(Cutscene cutscene , string path)
    {
        GameObject instanceObj = GameObject.Instantiate(cutscene.gameObject);
        instanceObj.layer = LayerMask.NameToLayer("Plot");

        CinemaShot[] shotArr = instanceObj.GetComponentsInChildren<CinemaShot>();
        foreach (CinemaShot shot in shotArr)
        {
            PlotCamera plotCam = shot.shotCamera.GetComponent<PlotCamera>();
            plotCam.IsMainCamera = shot.Firetime == 0;
            
        }

        ActorTrackGroup[] actorGroupArr = instanceObj.GetComponentsInChildren<ActorTrackGroup>();
        
        foreach (ActorTrackGroup actorGroup in actorGroupArr)
        {
            Transform trackTrans = actorGroup.transform;
            clearTransform(trackTrans, "_temp");
            clearTransform(trackTrans, "_Entity" , true);
        }

        //clear audio source 
        AudioTrack[] audioTracks = instanceObj.GetComponentsInChildren<AudioTrack>();
        foreach (AudioTrack audioGroup in audioTracks)
        {
            foreach (TimelineItem timelineItem in audioGroup.GetTimelineItems())
            {
                CinemaAudio audio = timelineItem as CinemaAudio;
                if (audio)
                {
                     AudioSource audioSrc = audio.GetComponent<AudioSource>();
                    GameObject.DestroyImmediate(audioSrc);                   
                }
            }
        }
    

        PrefabUtility.CreatePrefab(path, instanceObj);
        GameObject.DestroyImmediate(instanceObj);
        AssetDatabase.ImportAsset(path);
    }


    private void clearTransform(Transform trans, string targetTrans , bool isChild = false)
    {
        foreach (Transform childTrans in trans)
        {
            if (childTrans.name != targetTrans) continue;

            if (isChild)
            {
                int childCount = childTrans.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                    GameObject.DestroyImmediate(childTrans.GetChild(i).gameObject);
            }
            else
                GameObject.DestroyImmediate(childTrans.gameObject);
            return;
        }
    }

}
