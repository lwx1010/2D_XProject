using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class SceneViewCustom : MonoBehaviour
{
#if UNITY_EDITOR

    public GameObject modelRootGo;
    public GameObject effectRootGo;
    public GameObject terrainRootGo;

    void Start()
    {

    }

    void ResetParent(Transform root, GameObject[] goes)
    {
        foreach (var go in goes)
        {
            go.transform.parent = root;
        }
    }

    GameObject[] GetAllMeshChildGameObject(Transform root)
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (var trans in root.GetAllChildren())
        {
            if (trans.gameObject.activeSelf && trans.GetComponent<MeshFilter>() && trans.GetComponent<MeshRenderer>())
            {
                objs.Add(trans.gameObject);
            }
            if (trans.childCount > 0 && trans.gameObject.activeInHierarchy)
            {
                objs.AddRange(GetAllMeshChildGameObject(trans));
            }
        }
        return objs.ToArray();
    }

    [ContextMenu("创建场景物件prefab集合")]
    public void CreateSceneObjectsPrefabs()
    {
        GameObject selectObj = modelRootGo;
        string rootDir = "/Prefabs/Models/Scene/" + selectObj.transform.parent.name;
        string fullRootDir = Application.dataPath + "/" + rootDir;
        if (!Directory.Exists(fullRootDir))
            Directory.CreateDirectory(fullRootDir);

        foreach (var file in Directory.GetFiles("Assets/Prefabs/Models/Scene/" + selectObj.transform.parent.name, "*.prefab"))
        {
            File.Delete(file);
        }

        GameObject[] goes = GetAllMeshChildGameObject(selectObj.transform);
        List<GameObject> addGoes = new List<GameObject>();
        foreach (var go in goes)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mf && mr && mf.sharedMesh && go.activeSelf)
            {
                bool exist = false;
                foreach (var obj in addGoes)
                {
                    if (obj.GetComponent<MeshFilter>().sharedMesh.name.Equals(mf.sharedMesh.name)
                        && obj.GetComponent<MeshRenderer>().sharedMaterial.name.Equals(mr.sharedMaterial.name))
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist) continue;
                Vector3 scale = go.transform.localScale;
                Vector3 position = go.transform.localPosition;
                Quaternion rotation = go.transform.localRotation;
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                UnityEditor.PrefabUtility.CreatePrefab("Assets" + rootDir + "/" + go.name + ".prefab", go);
                go.transform.localScale = scale;
                go.transform.localPosition = position;
                go.transform.localRotation = rotation;
                addGoes.Add(go);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("场景物件prefab输出完毕 => " + Application.dataPath + rootDir);
    }


    [ContextMenu("创建场景特效prefab集合")]
    public void CreateSceneEffectsPrefabs()
    {
        GameObject selectObj = effectRootGo;
        string rootDir = "/Prefabs/Effects/Scene/" + selectObj.transform.parent.name;
        string fullRootDir = Application.dataPath + "/" + rootDir;
        if (!Directory.Exists(fullRootDir))
            Directory.CreateDirectory(fullRootDir);

        foreach (var file in Directory.GetFiles("Assets/Prefabs/Effects/Scene/" + selectObj.transform.parent.name, "*.prefab"))
        {
            File.Delete(file);
        }

        List<string> addGoes = new List<string>();
        foreach (var child in selectObj.transform.GetAllChildren())
        {
            if (addGoes.Contains(child.name))
                continue;

            Vector3 scale = child.localScale;
            Vector3 position = child.localPosition;
            Quaternion rotation = child.localRotation;
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            UnityEditor.PrefabUtility.CreatePrefab("Assets" + rootDir + "/" + child.name + ".prefab", child.gameObject);
            child.localScale = scale;
            child.localPosition = position;
            child.localRotation = rotation;
            addGoes.Add(child.name);
            UnityEditor.AssetDatabase.Refresh();
        }
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("场景特效prefab输出完毕 => " + Application.dataPath + rootDir);
    }

    [ContextMenu("输出场景物件json配置")]
    public void GenerateSceneObjectsInfoJson()
    {
        GameObject selectObj = modelRootGo;
        string rootDir = "/TextAssets/SceneConfig/" + selectObj.transform.parent.name;
        if (!Directory.Exists(Application.dataPath + rootDir))
            Directory.CreateDirectory(Application.dataPath + rootDir);

        rootDir += "/object.json";

        GameObject[] goes = GetAllMeshChildGameObject(selectObj.transform);
        ResetParent(selectObj.transform, goes);
        var json = MiniJSON.Json.Serialize(new Hashtable()
        {
            { "root", selectObj.name },
            { "number", goes.Length },
            { "parentscaleX", selectObj.transform.parent.transform.localScale.x },
            { "parentscaleY", selectObj.transform.parent.transform.localScale.y },
            { "parentscaleZ", selectObj.transform.parent.transform.localScale.z },
            { "transform", new Hashtable() 
                {
                    { "positionX", selectObj.transform.localPosition.x },
                    { "positionY", selectObj.transform.localPosition.y },
                    { "positionZ", selectObj.transform.localPosition.z },
                    { "scaleX", selectObj.transform.localScale.x },
                    { "scaleY", selectObj.transform.localScale.y },
                    { "scaleZ", selectObj.transform.localScale.z },
                    { "rotationX", selectObj.transform.localRotation.x },
                    { "rotationY", selectObj.transform.localRotation.y },
                    { "rotationZ", selectObj.transform.localRotation.z },
                    { "rotationW", selectObj.transform.localRotation.w },
                } 
            },
            { "objects", System.Array.ConvertAll(goes, i => 
                {
                    GameObject go = (GameObject)i;
                    string instantiate_name = "";
                    foreach (var file in Directory.GetFiles("Assets/Prefabs/Models/Scene/" + selectObj.transform.parent.name, "*.prefab"))
                    {
                        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(file, typeof(GameObject)) as GameObject;
                        if (go.GetComponent<MeshFilter>().sharedMesh.name.Equals(prefab.GetComponent<MeshFilter>().sharedMesh.name)
                            && go.GetComponent<MeshRenderer>().sharedMaterial.name.Equals(prefab.GetComponent<MeshRenderer>().sharedMaterial.name))
                        {
                            instantiate_name = prefab.name;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(instantiate_name))
                    {
                        foreach (var file in Directory.GetFiles("Assets/Prefabs/Models/Scene/Public", "*.prefab"))
                        {
                            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(file, typeof(GameObject)) as GameObject;
                            if (go.GetComponent<MeshFilter>().sharedMesh.name.Equals(prefab.GetComponent<MeshFilter>().sharedMesh.name)
                                && go.GetComponent<MeshRenderer>().sharedMaterial.name.Equals(prefab.GetComponent<MeshRenderer>().sharedMaterial.name))
                            {
                                instantiate_name = prefab.name;
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(instantiate_name))
                            Debug.LogError("no object equals: " + go.name);
                    }
                    BoxCollider collider = go.GetComponent<BoxCollider>();
                    return new Hashtable()
                    {
                        { "instantiateName", instantiate_name },
                        { "name", go.name },
                        { "tag", go.transform.tag },
                        { "layer", go.layer },
                        { "transform", new Hashtable() 
                            {
                                { "positionX", go.transform.localPosition.x },
                                { "positionY", go.transform.localPosition.y },
                                { "positionZ", go.transform.localPosition.z },
                                { "scaleX", go.transform.localScale.x },
                                { "scaleY", go.transform.localScale.y },
                                { "scaleZ", go.transform.localScale.z },
                                { "rotationX", go.transform.localRotation.x },
                                { "rotationY", go.transform.localRotation.y },
                                { "rotationZ", go.transform.localRotation.z },
                                { "rotationW", go.transform.localRotation.w },
                            } 
                        },
                        { "boxcollider", new Hashtable()
                            {
                                { "addbox", collider != null },
                                { "centerX", collider ? collider.center.x : 0 },
                                { "centerY", collider ? collider.center.y : 0 },
                                { "centerZ", collider ? collider.center.z : 0 },
                                { "sizeX", collider ? collider.size.x : 0 },
                                { "sizeY", collider ? collider.size.y : 0 },
                                { "sizeZ", collider ? collider.size.z : 0 },
                            }
                        },
                    }; 
                }) 
            },
        });
        UnityEditor.PrefabUtility.RevertPrefabInstance(modelRootGo.transform.parent.gameObject);
        if (File.Exists(Application.dataPath + rootDir))
            File.Delete(Application.dataPath + rootDir);
        File.WriteAllText(Application.dataPath + rootDir, json);
        Debug.Log("场景物件json配置输出完毕 => " + Application.dataPath + rootDir);
    }

    [ContextMenu("输出场景特效json配置")]
    public void GenerateSceneEffectsInfoJson()
    {
        GameObject selectObj = effectRootGo;
        string rootDir = "/TextAssets/SceneConfig/" + selectObj.transform.parent.name;
        if (!Directory.Exists(Application.dataPath + rootDir))
            Directory.CreateDirectory(Application.dataPath + rootDir);

        rootDir += "/effect.json";

        List<GameObject> goes = new List<GameObject>();
        foreach (var child in selectObj.transform.GetAllChildren())
        {
            goes.Add(child.gameObject);
        }
        var json = MiniJSON.Json.Serialize(new Hashtable()
        {
            { "root", selectObj.name },
            { "number", goes.Count },
            { "transform", new Hashtable() 
                {
                    { "positionX", selectObj.transform.localPosition.x },
                    { "positionY", selectObj.transform.localPosition.y },
                    { "positionZ", selectObj.transform.localPosition.z },
                    { "scaleX", selectObj.transform.localScale.x },
                    { "scaleY", selectObj.transform.localScale.y },
                    { "scaleZ", selectObj.transform.localScale.z },
                    { "rotationX", selectObj.transform.localRotation.x },
                    { "rotationY", selectObj.transform.localRotation.y },
                    { "rotationZ", selectObj.transform.localRotation.z },
                    { "rotationW", selectObj.transform.localRotation.w },
                } 
            },
            { "effects", System.Array.ConvertAll(goes.ToArray(), i => 
                {
                    GameObject go = (GameObject)i;
                    BoxCollider collider = go.GetComponent<BoxCollider>();
                    return new Hashtable()
                    {
                        { "name", go.name },
                        { "tag", go.transform.tag },
                        { "layer", go.layer },
                        { "transform", new Hashtable() 
                            {
                                { "positionX", go.transform.localPosition.x },
                                { "positionY", go.transform.localPosition.y },
                                { "positionZ", go.transform.localPosition.z },
                                { "scaleX", go.transform.localScale.x },
                                { "scaleY", go.transform.localScale.y },
                                { "scaleZ", go.transform.localScale.z },
                                { "rotationX", go.transform.localRotation.x },
                                { "rotationY", go.transform.localRotation.y },
                                { "rotationZ", go.transform.localRotation.z },
                                { "rotationW", go.transform.localRotation.w },
                            } 
                        },
                        { "boxcollider", new Hashtable()
                            {
                                { "addbox", collider != null },
                                { "centerX", collider ? collider.center.x : 0 },
                                { "centerY", collider ? collider.center.y : 0 },
                                { "centerZ", collider ? collider.center.z : 0 },
                                { "sizeX", collider ? collider.size.x : 0 },
                                { "sizeY", collider ? collider.size.y : 0 },
                                { "sizeZ", collider ? collider.size.z : 0 },
                            }
                        },
                    }; 
                }) 
            },
        });
        if (File.Exists(Application.dataPath + rootDir))
            File.Delete(Application.dataPath + rootDir);
        File.WriteAllText(Application.dataPath + rootDir, json);
        Debug.Log("场景特效json配置输出完毕 => " + Application.dataPath + rootDir);
    }

    [ContextMenu("输出场景地表json配置")]
    public void GenerateSceneTerrainInfoJson()
    {
        GameObject selectObj = terrainRootGo;
        string rootDir = "/TextAssets/SceneConfig/" + selectObj.transform.parent.name;
        if (!Directory.Exists(Application.dataPath + rootDir))
            Directory.CreateDirectory(Application.dataPath + rootDir);

        rootDir += "/terrain.json";
        Dictionary<GameObject, string> dictGos = new Dictionary<GameObject, string>();
        foreach (var child in selectObj.transform.GetAllChildren())
        {
            if (child.childCount > 0)
            {
                foreach (var trans in child.GetAllChildren())
                {
                    dictGos.Add(trans.gameObject, child.name);
                }
            }
            else
            {
                dictGos.Add(child.gameObject, "public");
            }
        }
        List<GameObject> goes = new List<GameObject>();
        foreach (var item in dictGos)
        {
            item.Key.transform.parent = selectObj.transform;
            goes.Add(item.Key);
        }
        
        var json = MiniJSON.Json.Serialize(new Hashtable()
        {
            { "root", selectObj.name },
            { "number", dictGos.Count },
            { "transform", new Hashtable() 
                {
                    { "positionX", selectObj.transform.localPosition.x },
                    { "positionY", selectObj.transform.localPosition.y },
                    { "positionZ", selectObj.transform.localPosition.z },
                    { "scaleX", selectObj.transform.localScale.x },
                    { "scaleY", selectObj.transform.localScale.y },
                    { "scaleZ", selectObj.transform.localScale.z },
                    { "rotationX", selectObj.transform.localRotation.x },
                    { "rotationY", selectObj.transform.localRotation.y },
                    { "rotationZ", selectObj.transform.localRotation.z },
                    { "rotationW", selectObj.transform.localRotation.w },
                } 
            },
            { "terrain", System.Array.ConvertAll(goes.ToArray(), i => 
                {
                    GameObject go = (GameObject)i;
                    BoxCollider collider = go.GetComponent<BoxCollider>();
                    return new Hashtable()
                    {
                        { "name", go.name },
                        { "tag", go.transform.tag },
                        { "area", dictGos[go] },
                        { "transform", new Hashtable() 
                            {
                                { "positionX", go.transform.localPosition.x },
                                { "positionY", go.transform.localPosition.y },
                                { "positionZ", go.transform.localPosition.z },
                                { "scaleX", go.transform.localScale.x },
                                { "scaleY", go.transform.localScale.y },
                                { "scaleZ", go.transform.localScale.z },
                                { "rotationX", go.transform.localRotation.x },
                                { "rotationY", go.transform.localRotation.y },
                                { "rotationZ", go.transform.localRotation.z },
                                { "rotationW", go.transform.localRotation.w },
                            } 
                        },
                        { "boxcollider", new Hashtable()
                            {
                                { "centerX", collider.center.x },
                                { "centerY", collider.center.y },
                                { "centerZ", collider.center.z },
                                { "sizeX", collider.size.x },
                                { "sizeY", collider.size.y },
                                { "sizeZ", collider.size.z },
                            }
                        },
                    }; 
                }) 
            },
        });
        UnityEditor.PrefabUtility.RevertPrefabInstance(terrainRootGo.transform.parent.gameObject);
        if (File.Exists(Application.dataPath + rootDir))
            File.Delete(Application.dataPath + rootDir);
        File.WriteAllText(Application.dataPath + rootDir, json);
        Debug.Log("场景地表json配置输出完毕 => " + Application.dataPath + rootDir);
    }
#endif
}
