using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class UIAtlasMerger : EditorWindow
{
    static public UIAtlasMerger instance;

    private Object[] destObjs;

    private Object srcObj;

    private int selectIndex = 1;

    private int size = 1;

    private List<string> refObjList = new List<string>();

    private const string info = @"说明:
1. 拖入图集到目标图集栏中
2. 设置图集大小(最大2048)
3. 设置需要合并图集的数量(默认1)
4. 拖入需要合并的图集
5. 点击合并按钮将自动合并选择的图集到目标图集中
6. 合并同时会查找所有引用了合并图集的预设并进行图集替换";

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }

    void OnGUI()
    {
        NGUIEditorTools.SetLabelWidth(120f);

        EditorGUILayout.LabelField("目标图集", GUILayout.Width(220));
        srcObj = EditorGUILayout.ObjectField(srcObj, typeof(UIAtlas), false, GUILayout.Width(200));
        EditorGUILayout.LabelField("Texture size", GUILayout.Width(220));
        selectIndex = EditorGUILayout.Popup(selectIndex, new string[2] { "1024", "2048" }, GUILayout.Width(200));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("需要合并的图集", GUILayout.Width(150));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("数量", GUILayout.Width(30));
        size = EditorGUILayout.IntField(size, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        if (size > 0)
        {
            if (destObjs == null)
            {
                destObjs = new Object[size];
            }
            else if (destObjs.Length != size)
            {
                List<Object> temps = new List<Object>();
                temps.AddRange(destObjs);
                destObjs = new Object[size];
                if (temps.Count > 0)
                {
                    for (int i = 0; i < size; ++i)
                    {
                        if (i < temps.Count)
                        {
                            destObjs[i] = temps[i];
                        }
                    }
                }
            }
            for (int i = 0; i < size; ++i)
            {
                destObjs[i] = EditorGUILayout.ObjectField(destObjs[i], typeof(UIAtlas), false, GUILayout.Width(200));
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("合并", GUILayout.Width(100)))
            {
                Merge();
            }
            if (GUILayout.Button("清除", GUILayout.Width(100)))
            {
                size = 1;
                srcObj = null;
                if (destObjs.Length > 0)
                {
                    for (int i = 0; i < destObjs.Length; ++i)
                    {
                        destObjs[i] = null;
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        var style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 15;
        EditorGUILayout.LabelField(info, style, GUILayout.Width(400), GUILayout.Height(100));
    }

    void Merge()
    {
        if (srcObj == null)
        {
            Debug.LogError("请将目标图集拖入面板中");
            return;
        }
        for (int i = 0; i < destObjs.Length; ++i)
        {
            if (destObjs[i] == null)
            {
                Debug.LogError("请先将需要合并的图集拖入面板中");
                return;
            }
        }
        CheckMergedAtlasRefrence();
        GatherSpritesToTarget();
    }

    void CheckMergedAtlasRefrence()
    {
        refObjList.Clear();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        ShowProgress(0, "查找合并图集资源引用中");
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace('\\', '/');
            ShowProgress((float)i / (float)files.Length, "查找合并图集资源引用中");
            for (int j = 0; j < destObjs.Length; ++j)
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(destObjs[j]));
                if (Regex.IsMatch(File.ReadAllText(file), guid) && !refObjList.Contains(file))
                {
                    refObjList.Add(file);
                }
            }
        }
        ShowProgress(1f, "查找合并图集资源引用中");
    }

    void GatherSpritesToTarget()
    {
        int atlasSize = 2048;
        if (selectIndex == 0) atlasSize = 1024;
        UIAtlasMaker.maxAtlasSize = atlasSize;
        var target = srcObj as UIAtlas;
        List<UIAtlasMaker.SpriteEntry> finalSprites = new List<UIAtlasMaker.SpriteEntry>();
        NGUISettings.atlas = target;
        UIAtlasMaker.ExtractSprites(target, finalSprites);
        Dictionary<string, NGUIMenuExtension.AtlasData> textures = new Dictionary<string, NGUIMenuExtension.AtlasData>();
        for (int i = 0; i < destObjs.Length; ++i)
        {
            var temps = new List<UIAtlasMaker.SpriteEntry>();
            var destAtlas = destObjs[i] as UIAtlas;
            UIAtlasMaker.ExtractSprites(destAtlas, temps);
            for (int j = 0; j < temps.Count; ++j)
            {
                bool exist = false;
                for (int k = 0; k < finalSprites.Count; ++k)
                {
                    if (finalSprites[k].name.Equals(temps[j].name))
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    finalSprites.Add(temps[j]);
                    if (!textures.ContainsKey(temps[j].name))
                    {
                        NGUIMenuExtension.AtlasData data = new NGUIMenuExtension.AtlasData();
                        data.tex = null;
                        data.data = new UISpriteData();
                        data.data.CopyFrom(temps[j]);
                        textures.Add(temps[j].name, data);
                    }
                }
            }
        }
        if (UIAtlasMaker.UpdateTexture(target, finalSprites))
        {
            UIAtlasMaker.ReplaceSprites(target, finalSprites);
            ModifyDestAtlasRefrence();
        }

        NGUIMenuExtension.ModifyAtlasSpritesPadding(target, textures);
        UIAtlasMaker.ReleaseSprites(finalSprites);
        AssetDatabase.Refresh();
    }

    void ModifyDestAtlasRefrence()
    {
        ShowProgress(0, "更新合并图集相关引用");
        for (int i = 0; i < refObjList.Count; ++i)
        {
            string file = refObjList[i];
            ShowProgress((float)i / (float)refObjList.Count, "更新合并图集相关引用");
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
                        for (int k = 0; k < destObjs.Length; ++k)
                        {
                            var destAtlas = destObjs[k];
                            if (sprite.atlas != null && destAtlas.name.Equals(sprite.atlas.name))
                            {
                                sprite.atlas = srcObj as UIAtlas;
                            }
                        }
                    }
                    var font = temps[j].GetComponent<UIFont>();
                    if (font != null)
                    {
                        for (int k = 0; k < destObjs.Length; ++k)
                        {
                            var destAtlas = destObjs[k];
                            if (font.atlas != null && destAtlas.name.Equals(font.atlas.name))
                            {
                                font.atlas = srcObj as UIAtlas;
                            }
                        }
                    }
                    var popupList = temps[j].GetComponent<UIPopupList>();
                    if (popupList != null)
                    {
                        for (int k = 0; k < destObjs.Length; ++k)
                        {
                            var destAtlas = destObjs[k];
                            if (popupList.atlas != null && destAtlas.name.Equals(popupList.atlas.name))
                            {
                                popupList.atlas = srcObj as UIAtlas;
                            }
                        }
                    }
                }
                PrefabUtility.ReplacePrefab(instance, obj);
                GameObject.DestroyImmediate(instance);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
        ShowProgress(1f, "更新合并图集相关引用");
        EditorUtility.ClearProgressBar();
        Debug.Log("合并完成");
    }

    void ShowProgress(float val, string message)
    {
        EditorUtility.DisplayProgressBar("合并图集", message, val);
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
