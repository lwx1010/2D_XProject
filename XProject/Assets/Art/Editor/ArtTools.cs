using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class ArtTools : Editor {

    [MenuItem("Assets/导出资源包")]
    static void Build()
    {
        if (Selection.objects == null) return;
        List<string> paths = new List<string>();
        string exportName = string.Empty;
        foreach (UnityEngine.Object o in Selection.objects)
        {
            paths.Add(AssetDatabase.GetAssetPath(o));
            exportName += o.name + "&&";
        }

        exportName = exportName.Substring(0, exportName.Length - 2);
        AssetDatabase.ExportPackage(paths.ToArray(), "Assets/ExportPackage/" + exportName + ".unitypackage", ExportPackageOptions.IncludeDependencies);
        AssetDatabase.Refresh();
        Debug.Log("Build all Done!");
    }

    [MenuItem("Assets/Find References", false, 100)]
    static private void Find()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path))
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;

            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    Debug.Log(file + "\n" + guid, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                }

            };
        }
    }

    [MenuItem("Assets/Find References", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }


    [MenuItem("ArtTools/添加摄像机移动脚本")]
    static void AddCameraMove()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Mistake", "请启动游戏后再添加脚本", "确定");
            return;
        }
        Camera.main.GetOrAddComponent<CameraMove>();
    }

    static GameObject[] getAllChildren(Transform root)
    {
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform child = root.GetChild(i);
            if (child.childCount > 0)
            {
                objs.AddRange(getAllChildren(child));
            }
            objs.Add(child.gameObject);
        }
        return objs.ToArray();
    }

    [MenuItem("ArtTools/Scene/重命名场景物件")]
    static void RenameSceneModel()
    {
        GameObject go = (GameObject)Selection.activeObject;
        if (go)
        {
            GameObject[] objs = getAllChildren(go.transform);
            foreach (var obj in objs)
            {
                if (obj.GetComponent<Renderer>() == null)
                    continue;
                int index = 0;
                foreach (var tempObj in objs)
                {
                    if (obj.GetComponent<Renderer>() == null)
                        continue;
                    if (obj != tempObj && obj.name.Equals(tempObj.name))
                    {
                        tempObj.name = string.Format("{0}_{1}", tempObj.name, index++);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("ArtTools/Scene/删除场景没用的MeshCollider和Animation")]
    static public void Remove()
    {
        //获取当前场景里的所有游戏对象
        GameObject[] rootObjects = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
        //遍历游戏对象
        foreach (GameObject go in rootObjects)
        {
            //如果发现Render的shader是Diffuse并且颜色是白色，那么将它的shader修改成Mobile/Diffuse
            if (go != null && go.transform.parent != null)
            {
                Renderer render = go.GetComponent<Renderer>();
                if (render != null && render.sharedMaterial != null && render.sharedMaterial.shader.name == "Diffuse" && render.sharedMaterial.color == Color.white)
                {
                    render.sharedMaterial.shader = Shader.Find("Mobile/Diffuse");
                }
            }

            //删除所有的MeshCollider
            foreach (MeshCollider collider in UnityEngine.Object.FindObjectsOfType(typeof(MeshCollider)))
            {
                if (!collider.gameObject.name.Contains("navigation") && !collider.transform.parent.name.Contains("navigation"))
                    GameObject.DestroyImmediate(collider);
            }

            //删除没有用的动画组件
            foreach (Animation animation in UnityEngine.Object.FindObjectsOfType(typeof(Animation)))
            {
                if (animation.clip == null && animation.GetClipCount() <= 0)
                    GameObject.DestroyImmediate(animation);
            }

            //应该没有人用Animator吧？ 避免美术弄错我都全部删除了。
            foreach (Animator animator in UnityEngine.Object.FindObjectsOfType(typeof(Animator)))
            {
                if (animator.runtimeAnimatorController == null)
                    GameObject.DestroyImmediate(animator);
            }
        }
        //保存
        AssetDatabase.SaveAssets();
    }

    [MenuItem("ArtTools/Tool/Delete Mesh")]
    static public void deleteMesh()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            GameObject[] childArr = getAllChildren(go.transform);
            foreach (GameObject child in childArr )
            {
                if(child.GetComponent<MeshRenderer>() != null)
                    DestroyImmediate(child.GetComponent<MeshRenderer>());

                if(child.GetComponent<MeshFilter>() != null)
                    DestroyImmediate(child.GetComponent<MeshFilter>());
            }
        }
    }

    [MenuItem("ArtTools/Scene/保存RenderSetting")]
    static void SaveRenderSetting()
    {
        string outputDir = Application.dataPath + "/TextAssets/SceneConfig";
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var json = MiniJSON.Json.Serialize(new Hashtable()
                {
                    { "fog", RenderSettings.fog },
                    { "fogColor", new Hashtable() 
                        {
                            { "a", RenderSettings.fogColor.a },
                            { "r", RenderSettings.fogColor.r },
                            { "g", RenderSettings.fogColor.g },
                            { "b", RenderSettings.fogColor.b },
                        }
                    },
                    { "fogMode", (int)RenderSettings.fogMode },
                    { "fogDensity", RenderSettings.fogDensity },
                    { "fogStart", RenderSettings.fogStartDistance },
                    { "fogEnd", RenderSettings.fogEndDistance },
                    { "ambientLight", new Hashtable() 
                        {
                            { "a", RenderSettings.ambientLight.a },
                            { "r", RenderSettings.ambientLight.r },
                            { "g", RenderSettings.ambientLight.g },
                            { "b", RenderSettings.ambientLight.b },
                        } 
                    },
                    { "haloStrength", RenderSettings.haloStrength },
                    { "flareStrength", RenderSettings.flareStrength },
                    { "flareFadeSpeed", RenderSettings.flareFadeSpeed },
                });

        string sceneName = EditorSceneManager.GetActiveScene().name;
        string curSceneName = sceneName.Substring(sceneName.IndexOf("Scenes") + 7).Split('.')[0];
        string finalName = outputDir + "/" + curSceneName;
        if (!Directory.Exists(finalName))
            Directory.CreateDirectory(finalName);
        finalName += "/rendersetting.json";
        if (File.Exists(finalName))
            File.Delete(finalName);
        File.WriteAllText(finalName, json, new System.Text.UTF8Encoding(false));
        Debug.Log("Save RenderSetting info to: " + finalName);
        AssetDatabase.SaveAssets();
    }
}