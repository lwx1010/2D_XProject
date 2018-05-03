using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class UIPrefabAtlasReplace : EditorWindow
{

    static public UIPrefabAtlasReplace instance;

    private Object targetAtlas = null;
    private Object srcAtlas = null;

    private string replaceSpriteName = "";

    private List<string> refObjList = new List<string>();

    void OnEnable()
    {
        instance = this;
    }
    void OnDisable() { instance = null; }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("目标图集:", GUILayout.Width(100));
        targetAtlas = EditorGUILayout.ObjectField(targetAtlas, typeof(UIAtlas), false, GUILayout.Width(150));
        EditorGUILayout.LabelField("图片名称:", GUILayout.Width(100));
        replaceSpriteName = EditorGUILayout.TextField(replaceSpriteName, GUILayout.Width(140));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("源图集:", GUILayout.Width(100));
        srcAtlas = EditorGUILayout.ObjectField(srcAtlas, typeof(UIAtlas), false, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("替换", GUILayout.Width(100)))
        {
            CheckMergedAtlasRefrence();
            Replace();
        }
    }

    void CheckMergedAtlasRefrence()
    {
        refObjList.Clear();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        ShowProgress(0, "查找替换图集引用中");
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace('\\', '/');
            ShowProgress((float)i / (float)files.Length, "查找替换图集引用中...");
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetAtlas));
            if (Regex.IsMatch(File.ReadAllText(file), guid) && !refObjList.Contains(file))
            {
                refObjList.Add(file);
            }
        }
        ShowProgress(1f, "查找完成");
    }

    void Replace()
    {
        ShowProgress(0, "更新替换图集引用");
        for (int i = 0; i < refObjList.Count; ++i)
        {
            string file = refObjList[i];
            ShowProgress((float)i / (float)refObjList.Count, "更新替换图集引用");
            var obj = AssetDatabase.LoadAssetAtPath<Object>(file);
            var instance = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            if (instance != null)
            {
                var children = GetAllChildren(instance.transform);
                List<Transform> temps = new List<Transform>();
                temps.AddRange(children);
                temps.Add(instance.transform);
                for (int j = 0; j < temps.Count; ++j)
                {
                    var sprite = temps[j].GetComponent<UISprite>();
                    if (sprite != null)
                    {
                        if (sprite.atlas != null && sprite.spriteName == replaceSpriteName && !sprite.atlas.name.Equals(srcAtlas.name))
                        {
                            sprite.atlas = srcAtlas as UIAtlas;
                            Debug.Log("<color=yellow>替换成功: " + sprite.spriteName + ", " + temps[j].name + ", " + file + "</color>");
                        }
                    }
                    var font = temps[j].GetComponent<UIFont>();
                    if (font != null)
                    {
                        if (font.atlas != null && font.spriteName == replaceSpriteName && !font.atlas.name.Equals(srcAtlas.name))
                        {
                            font.atlas = srcAtlas as UIAtlas;
                            Debug.Log("<color=yellow>替换成功: " + font.spriteName + ", " + temps[j].name + ", " + file + "</color>");
                        }
                    }
                }
                PrefabUtility.ReplacePrefab(instance, obj);
                GameObject.DestroyImmediate(instance);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
        ShowProgress(1f, "更新替换图集相关引用");
        EditorUtility.ClearProgressBar();
        Debug.Log("替换完成");
    }

    void ShowProgress(float val, string message)
    {
        EditorUtility.DisplayProgressBar("图集替换", message, val);
    }

    Transform[] GetAllChildren(Transform root)
    {
        List<Transform> objs = new List<Transform>();
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform trans = root.GetChild(i);
            objs.Add(trans);
            if (trans.childCount > 0)
            {
                objs.AddRange(GetAllChildren(trans));
            }
        }
        return objs.ToArray();
    }
}
