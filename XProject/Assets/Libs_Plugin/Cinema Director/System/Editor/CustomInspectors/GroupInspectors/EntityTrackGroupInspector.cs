using System;
using UnityEditor;
using UnityEngine;
using CinemaDirector;
using CinemaDirectorControl.Utility;
using LuaFramework;
using RSG;

/// <summary>
/// A custom inspector for a cutscene.
/// </summary>
[CustomEditor(typeof(EntityTrackGroup), true)]
public class EntityTrackGroupInspector : Editor
{
    private SerializedProperty selfProperty;
    private SerializedProperty isWeaponProperty;
    private SerializedProperty modelProperty;
    private SerializedProperty wingsProperty;
    private SerializedProperty weaponProperty;
    private SerializedProperty weaponPostionProperty;
    private SerializedProperty weaponRotationProperty;
    private SerializedProperty horseProperty;
    private SerializedProperty optimizable;

    private bool containerFoldout = true;
    private Texture inspectorIcon = null;

    #region Language
    //GUIContent ordinalContent = new GUIContent("Ordinal", "The ordinal value of this container, for sorting containers in the timeline.");
    GUIContent addTrackContent = new GUIContent("Add New Track", "Add a new track to this actor track group.");

    GUIContent reflushEntity = new GUIContent("Reflush", "Load a new Model.");

    GUIContent tracksContent = new GUIContent("Actor Tracks", "The tracks associated with this Actor Group.");
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
        this.selfProperty = base.serializedObject.FindProperty("Self");
        this.isWeaponProperty = base.serializedObject.FindProperty("IsWeapon");
        this.modelProperty = base.serializedObject.FindProperty("Model");
        this.wingsProperty = base.serializedObject.FindProperty("Wings");
        this.weaponProperty = base.serializedObject.FindProperty("Weapon");
        this.weaponPostionProperty = base.serializedObject.FindProperty("WeaponPos");
        this.weaponRotationProperty = base.serializedObject.FindProperty("WeaponeRotation"); 
        this.horseProperty = base.serializedObject.FindProperty("Horse");
        this.optimizable = serializedObject.FindProperty("canOptimize");
    }

    /// <summary>
    /// Draw the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        EntityTrackGroup actorGroup = base.serializedObject.targetObject as EntityTrackGroup;
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

        GUI.enabled = !isCutsceneActive;
        EditorGUILayout.PropertyField(selfProperty, new GUIContent("Self"));
        if (!selfProperty.boolValue)
        {
            EditorGUILayout.PropertyField(modelProperty , new GUIContent("Model"));
            EditorGUILayout.PropertyField(wingsProperty, new GUIContent("Wings"));
            EditorGUILayout.PropertyField(weaponProperty, new GUIContent("Weapone"));
            if (!string.IsNullOrEmpty(this.weaponProperty.stringValue))
            {
                EditorGUILayout.PropertyField(weaponPostionProperty, new GUIContent("Weapon Postion"));
                EditorGUILayout.PropertyField(weaponRotationProperty, new GUIContent("Weapone Rotation"));
            }
    //        EditorGUILayout.PropertyField(horseProperty, new GUIContent("Horse"));            
        }
        else
        {
            EditorGUILayout.PropertyField(isWeaponProperty, new GUIContent("ShowWeapon"));
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

        base.serializedObject.ApplyModifiedProperties();
        if ( GUILayout.Button(reflushEntity))
        {
            Initialize(actorGroup , modelProperty.stringValue , wingsProperty.stringValue , weaponProperty.stringValue , horseProperty.stringValue);
        }
        
    }


    public static void Initialize(EntityTrackGroup actorGroup , string Model , string Wings , string Weapon , string Horse)
    {
        
        Transform transform = actorGroup.transform;
        Transform entityTrans = transform.Find("_Entity");
        if (entityTrans != null)
        {
            int childCount = entityTrans.childCount;
            for(int i = childCount - 1 ; i >= 0 ; i -- )
            {
                Transform childTrans = entityTrans.GetChild(i);
                GameObject.DestroyImmediate(childTrans.gameObject);
            }
            actorGroup.Actor = entityTrans;
        }
        else
        {
            GameObject entityGO = new GameObject("_Entity");
            //Util.SetParent(entityGO , transform.gameObject);
            actorGroup.Actor = entityGO.transform;           
        }


        if (actorGroup.Self)
        {
            GameObject modelGO = Resources.Load<GameObject>("Other/bucket");
            modelGO = GameObject.Instantiate(modelGO);
            //Util.SetParent(modelGO, actorGroup.Actor.gameObject);
            modelGO.transform.localRotation = Quaternion.Euler(-90,0,0);
            //EntityTrackGroup.AddFastShadow(modelGO);
            return;
        }
        
        new Promise<GameObject>((s, j) =>
        {
            GameObject modelGO = LoadPrefab("Prefab/" + Model);
            if (modelGO == null)
            {
                Debug.LogError("找不到模型资源，路径不正确！请检测路径!!! eg:npc/001或player/001");
                return;
            }
            modelGO = GameObject.Instantiate(modelGO);
            Animator animator = modelGO.GetComponentInChildren<Animator>();
            if(animator)    animator.enabled = true;
            //Util.SetParent(modelGO, actorGroup.Actor.gameObject);
            //EntityTrackGroup.AddFastShadow(modelGO);

            s.Invoke(modelGO);
        }).Then((go) =>
        {
            
            if (!string.IsNullOrEmpty(Wings))
            {
                GameObject wingeGO = LoadPrefab("Prefab/Model/wings/" + Wings);
                if (wingeGO == null)
                {
                    Debug.LogError("找不到翅膀资源，请检测路径!!!" + Wings);
                    return;
                }
                wingeGO = GameObject.Instantiate(wingeGO);
                Animator animator = wingeGO.GetComponentInChildren<Animator>();
                animator.enabled = true;
                //Util.SetParent(wingeGO , go.transform.Find("chibang01").gameObject);
                wingeGO.transform.localRotation = Quaternion.Euler(0, -90, 0);
            }
        }).Then((go) =>
        {
            if (!string.IsNullOrEmpty(Weapon))
            {
                GameObject weaponGO = LoadPrefab("Prefab/Model/weapon/" + Weapon);
                if (weaponGO == null)
                {
                    Debug.LogError("找不到武器资源，请检测路径!!!" + Weapon);
                    return;
                }
                weaponGO = GameObject.Instantiate(weaponGO);
                //Util.SetParent(weaponGO , go.transform.Find("wuqi01").gameObject);
                weaponGO.transform.localPosition = actorGroup.WeaponPos;
                weaponGO.transform.localRotation = Quaternion.Euler(actorGroup.WeaponeRotation);
            }
        }).Then((go) =>
        {
            //坐骑
            //                if(!string.IsNullOrEmpty())
        });
    }

    private static GameObject LoadPrefab(string assetName)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/" + assetName + ".prefab");
        return go;
    }
}
