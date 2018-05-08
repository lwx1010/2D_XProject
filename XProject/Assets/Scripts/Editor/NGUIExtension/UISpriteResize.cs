using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class UISpriteResize : EditorWindow {

    static public UISpriteResize instance;

    private string spriteName = "";
    private int orignalWidth = 1920;
    private int orignalHeight = 1080;
    private int resizeWidth = 2600;
    private int resizeHeight = 1300;

    private List<string> atlasList = new List<string>();
    private List<string> resultsList = new List<string>();

    private string resultsStr = "";
    private string prefabStr = "";

    private List<string> refObjList = new List<string>();

    private Vector2 scrollPos;
    void OnEnable()
    {
        instance = this;
        OnSelectionChange();
    }
    void OnDisable() { instance = null; }

    void OnGUI()
    {
        GUILayout.Label("精灵图片名称:", GUILayout.Width(200));
        spriteName = GUILayout.TextField(spriteName, GUILayout.Width(200));
        GUILayout.Label("精灵原宽：", GUILayout.Width(100));
        orignalWidth = EditorGUILayout.IntField(orignalWidth, GUILayout.Width(100));
        GUILayout.Label("精灵原高：", GUILayout.Width(100));
        orignalHeight = EditorGUILayout.IntField(orignalHeight, GUILayout.Width(100));
        GUILayout.Label("精灵宽：", GUILayout.Width(100));
        resizeWidth = EditorGUILayout.IntField(resizeWidth, GUILayout.Width(100));
        GUILayout.Label("精灵高：", GUILayout.Width(100));
        resizeHeight = EditorGUILayout.IntField(resizeHeight, GUILayout.Width(100));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("查找", GUILayout.Width(100)))
        {
            FindSprite();
            GetAtlasRefrence();
            FindPrefabName();
            DisplayResult();
        }
        if (GUILayout.Button("清除", GUILayout.Width(100)))
        {
            resultsStr = "";
            spriteName = "";
            resizeWidth = 0;
            resizeHeight = 0;
        }
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(resultsStr))
        {
            GUILayout.TextArea(resultsStr, GUILayout.Width(400), GUILayout.Height(600));
        }

    }

    void OnSelectionChange()
    {
        if (Selection.activeObject != null)
        {
            spriteName = Selection.activeObject.name;
            this.Repaint();
        }
    }

    void FindSprite()
    {
        resultsStr = "";
        string[] atlas = Directory.GetFiles("Assets/Res/Atlas/", "*.prefab", SearchOption.AllDirectories);
        resultsList.Clear();
        atlasList.Clear();
        atlasList.AddRange(atlas);
        for (int i = 0; i < atlas.Length; ++i)
        {
            string file = atlasList[i].Replace('\\', '/');

            EditorUtility.DisplayProgressBar("查找精灵中", file, (float)i / (float)atlasList.Count);

            var uiAtlas = AssetDatabase.LoadAssetAtPath<UIAtlas>(file);
            if (uiAtlas != null)
            {
                for (int j = 0; j < uiAtlas.spriteList.Count; ++j)
                {
                    if (uiAtlas.spriteList[j].name.Equals(spriteName))
                    {
                        resultsList.Add(file);
                    }
                }
            }
        }
        EditorUtility.DisplayProgressBar("查找精灵中", "find finished", 1);
        EditorUtility.ClearProgressBar();
    }

    void FindPrefabName()
    {
        prefabStr = "";
        ShowProgress(0, "查找预设引用位置");
        for (int i = 0; i < refObjList.Count; ++i)
        {
            string file = refObjList[i];
            ShowProgress((float)i / (float)refObjList.Count, "查找预设引用位置");
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
                        if (sprite.atlas != null && spriteName.Equals(sprite.spriteName) && sprite.width >= orignalWidth && sprite.height >= orignalHeight)
                        {
                            prefabStr += file + "\n";
                            prefabStr += "    " + temps[j].name + "   [UISprite]\n";
                            sprite.width = resizeWidth;
                            sprite.height = resizeHeight;
                            PrefabUtility.ReplacePrefab(instance, obj);
                        }
                    }
                    var font = temps[j].GetComponent<UIFont>();
                    if (font != null)
                    {
                        if (font.atlas != null && spriteName.Equals(font.spriteName))
                        {
                            prefabStr += file + "\n";
                            prefabStr += "    " + temps[j].name + "   [UIFont]\n";
                        }
                    }
                    var popupList = temps[j].GetComponent<UIPopupList>();
                    if (popupList != null)
                    {
                        if (popupList.atlas != null && (spriteName.Equals(popupList.backgroundSprite) || spriteName.Equals(popupList.highlightSprite)))
                        {
                            prefabStr += file + "\n";
                            prefabStr += "    " + temps[j].name + "   [UIPopupList]\n";
                        }
                    }
                }
                GameObject.DestroyImmediate(instance);
            }
        }
        ShowProgress(1f, "查找预设引用位置");
        EditorUtility.ClearProgressBar();
    }

    void DisplayResult()
    {
        resultsStr = "图集名:";
        for (int i = 0; i < resultsList.Count; ++i)
        {
            resultsStr += Path.GetFileNameWithoutExtension(resultsList[i]) + ",";
        }
        resultsStr.TrimEnd(',');
        resultsStr += "\n";
        resultsStr += prefabStr;
        Debug.Log("查找结束");
    }

    void GetAtlasRefrence()
    {
        refObjList.Clear();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string[] files = Directory.GetFiles("Assets/Res/Prefab/Gui", "*.prefab", SearchOption.AllDirectories);
        ShowProgress(0, "查找精灵图片预设引用中");
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace('\\', '/');
            ShowProgress((float)i / (float)files.Length, "查找精灵图片预设引用中");
            for (int j = 0; j < resultsList.Count; ++j)
            {
                string guid = AssetDatabase.AssetPathToGUID(resultsList[j]);
                if (Regex.IsMatch(File.ReadAllText(file), guid) && !refObjList.Contains(file))
                {
                    refObjList.Add(file);
                }
            }
        }
        ShowProgress(1f, "查找合并图集资源引用中");
        EditorUtility.ClearProgressBar();
    }

    void ShowProgress(float val, string message)
    {
        EditorUtility.DisplayProgressBar("精灵查找", message, val);
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
