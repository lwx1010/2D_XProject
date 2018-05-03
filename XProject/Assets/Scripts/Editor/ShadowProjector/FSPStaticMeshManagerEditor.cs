#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
[InitializeOnLoad]
public class FSPStaticMeshManagerEditor
{
	public static List<ShadowReceiver> currentStaticReceivers;

	static FSPStaticMeshManagerEditor() {
//		EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
	}

	static void OnPlaymodeStateChanged()
	{
		if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
			TraverseReceivers();
			RecreateFSPStaticHolder();
		}
	}


	static void TraverseReceivers() {
		currentStaticReceivers = new List<ShadowReceiver>();

		foreach (ShadowReceiver receiver in Object.FindObjectsOfType(typeof(ShadowReceiver)))
		{
			if (receiver.gameObject.isStatic) {
				currentStaticReceivers.Add(receiver);
			}
		}
	}

	static void RecreateFSPStaticHolder() {
		bool staticsExist = false;

		foreach (ShadowReceiver receiver in currentStaticReceivers) {
			if (receiver.gameObject.isStatic) {
				staticsExist = true;
				break;
			}
		}

		GameObject staticHolder = (GameObject)GameObject.Find("_FSPStaticHolder");

		if (staticHolder != null) {
			GameObject.DestroyImmediate(staticHolder);
		}

		if (!staticsExist) {
			return;
		}
			
		staticHolder = new GameObject("_FSPStaticHolder");
		staticHolder.AddComponent<FSPStaticMeshHolder>();
		staticHolder.isStatic = false;

		MeshFilter meshFilter;
		Mesh mesh;
		Mesh meshCopy;
		int id = 0;

		foreach (ShadowReceiver receiver in currentStaticReceivers) {
			meshFilter = receiver.GetComponent<MeshFilter>();
			receiver._id = id++;

			EditorUtility.SetDirty(receiver);

			if (meshFilter != null) {
				mesh = meshFilter.sharedMesh;
				meshCopy = new Mesh();
				meshCopy.vertices = mesh.vertices;
				meshCopy.triangles = mesh.triangles;
				meshCopy.normals = mesh.normals;
				meshCopy.name = "_copy";
		
			//	meshCopy.MarkDynamic();

				GameObject meshLinker = new GameObject(receiver._id.ToString());
				meshLinker.isStatic = false;
				meshLinker.transform.parent = staticHolder.transform;
				meshLinker.transform.position = receiver.transform.position;
				meshLinker.AddComponent<MeshFilter>().mesh = meshCopy;
			    meshLinker.transform.rotation = receiver.transform.rotation;
			    meshLinker.transform.localScale = receiver.transform.lossyScale;

                MeshRenderer meshRenderer = meshLinker.AddComponent<MeshRenderer>();
				meshRenderer.enabled = false;
			}
		}
	}

    [MenuItem("ArtTools/Shadow/ Clear")]
    private static void ClearSceneReceiver()
    {
        List<ShadowReceiver> receivers = new List<ShadowReceiver>();
        receivers.AddRange(Object.FindObjectsOfType<ShadowReceiver>());

        for (int i = receivers.Count - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(receivers[i]);
        }
    }

    [MenuItem("ArtTools/Shadow/ Add Scene")]
    private static void AddCurrentSceneShadows()
    {
        string sceneName = EditorSceneManager.GetActiveScene().name;

        addSceneShadowReceiver(sceneName);

    }

    [MenuItem("ArtTools/Shadow/ Add All Scene")]
    private static void AddAllSceneShadows()
    {
        string defaultScene = EditorSceneManager.GetActiveScene().name;
        
        int sceneCount = EditorBuildSettings.scenes.Length;
        for (int i = 0; i < sceneCount; i++)
        {
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            if (sceneName == defaultScene)  defaultScene = scene.path;

            EditorUtility.DisplayProgressBar("处理中", "正在处理场景" + sceneName + "..." , i / (float)sceneCount);
            int sceneInt = 0;
            if (!Int32.TryParse(sceneName, out sceneInt))
                continue;
            
            Scene curScene = EditorSceneManager.OpenScene(scene.path);

            EditorSceneManager.MarkSceneDirty(curScene);

            ClearSceneReceiver();

            addSceneShadowReceiver(sceneName);
        }

        EditorSceneManager.OpenScene(defaultScene);
        EditorUtility.ClearProgressBar();
    }

    private static void addSceneShadowReceiver(string sceneName)
    {
        GameObject sceneEnvirment = GameObject.Find(sceneName);

        if (sceneEnvirment == null)
        {
            Debug.LogError("无法找到场景中的地表！" + sceneName);
            return;
        }

        //暂时遍历两层
        Transform navigationTrans = null;
        foreach (Transform childTrans1 in sceneEnvirment.transform)
        {
            if (childTrans1.gameObject.name.Contains("navigation"))
            {
                navigationTrans = childTrans1;
                break;
            }

            foreach (Transform childTrans2 in childTrans1)
            {
                if (childTrans2.gameObject.name.Contains("navigation"))
                {
                    navigationTrans = childTrans2;
                    break;
                }
            }
        }

        if (navigationTrans == null)
        {
            Debug.LogError("无法找到场景中的行走面导航网格！" + sceneName);
            return;
        }

        List<MeshFilter> meshArr = new List<MeshFilter>();
        meshArr.Add(navigationTrans.GetComponent<MeshFilter>());
        meshArr.AddRange(navigationTrans.GetComponentsInChildren<MeshFilter>());

        foreach (MeshFilter meshFilter in meshArr)
        {
            if(meshFilter == null)  continue;

            MeshRenderer meshRenderer = meshFilter.transform.GetOrAddComponent<MeshRenderer>();
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.gameObject.isStatic = false;
            meshRenderer.enabled = false;

            meshFilter.transform.GetOrAddComponent<ShadowReceiver>();
        }

        GameObject meshHolder = GameObject.Find("_FSPStaticHolder");
        if (meshHolder != null)
            GameObject.DestroyImmediate(meshHolder);
        
        EditorSceneManager.SaveOpenScenes();
    }



}
#endif