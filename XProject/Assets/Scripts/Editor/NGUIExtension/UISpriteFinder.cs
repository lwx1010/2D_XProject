using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class UISpriteFinder : EditorWindow {

    static public UISpriteFinder instance;

    private string spriteName = "";

    private List<string> atlasList = new List<string>();
    private List<string> resultsList = new List<string>();

    private int startIndex = 0;

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

        GUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width * 0.25f);
        if (GUILayout.Toggle(startIndex == 0, "精灵图片(UISprite)", "ButtonLeft")) startIndex = 0;
        if (GUILayout.Toggle(startIndex == 1, "大贴图(UITexture)", "ButtonRight")) startIndex = 1;
        GUILayout.Space(Screen.width * 0.25f);
        GUILayout.EndHorizontal();

        if (startIndex == 0)
        {
            GUILayout.Label("精灵图片名称:", GUILayout.Width(200));
            spriteName = GUILayout.TextField(spriteName, GUILayout.Width(200));
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
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(resultsStr))
            {
                GUILayout.TextArea(resultsStr, GUILayout.Width(400), GUILayout.Height(600));
            }
        }
        else if (startIndex == 1)
        {
            GUILayout.Label("资源名称:", GUILayout.Width(200));
            spriteName = GUILayout.TextField(spriteName, GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查找", GUILayout.Width(100)))
            {
                findTextureRefence();
            }
            if (GUILayout.Button("清除", GUILayout.Width(100)))
            {
                resultsStr = "";
                spriteName = "";
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(resultsStr))
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos , false , true);
                GUILayout.TextArea(resultsStr);
                GUILayout.EndScrollView();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("打开所有界面"))
                {
                    string[] uiPanelArr = resultsStr.Split('\n');
                    foreach (string uiPanelPath in uiPanelArr)
                    {
                        if(string.IsNullOrEmpty(uiPanelPath.Trim()))    continue;

                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(uiPanelPath.Trim());
                        PrefabUtility.InstantiatePrefab(obj);
                    }
                }
                if (GUILayout.Button("镜像替换所有界面"))
                {
                    string[] uiPanelArr = resultsStr.Split('\n');
                    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                    foreach (string uiPanelPath in uiPanelArr)
                    {
                        if (string.IsNullOrEmpty(uiPanelPath.Trim())) continue;

                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(uiPanelPath.Trim());
                        GameObject gObj = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                        bool isReflush = MirrorTexture(gObj, path);
                        if (isReflush)
                        {
                            PrefabUtility.ReplacePrefab(gObj, obj);
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }
    }


    private void findTextureRefence()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();

        string[] uiPrfabArr = Directory.GetFiles("Assets/Res/Prefab/Gui/", "*.prefab", SearchOption.AllDirectories);
        StringBuilder buf = new StringBuilder();
        float count = uiPrfabArr.Length;
        for (int i = 0; i < uiPrfabArr.Length; i++)
        {
            GameObject uiPanel = AssetDatabase.LoadAssetAtPath<GameObject>(uiPrfabArr[i]);

            UITexture[] textureArr = uiPanel.GetComponentsInChildren<UITexture>(true);
            for (int j = 0; j < textureArr.Length; j++)
            {
                if (textureArr[j].texturePath.ToLower().Equals(path))
                {
                    buf.AppendLine(uiPrfabArr[i]);
                    break;
                }
            }
            EditorUtility.DisplayProgressBar("查找中" , "正在搜索..." , i / count);
        }
        EditorUtility.ClearProgressBar();
        resultsStr = buf.ToString();
    }
    /// <summary>
    /// 对称镜像贴图
    /// </summary>
    /// <param name="gObj"></param>
    /// <param name="targetTexture"></param>
    private bool MirrorTexture(GameObject gObj , string targetTexture)
    {
        UITexture[] texArr = gObj.GetComponentsInChildren<UITexture>(true);
        for (int i = 0; i < texArr.Length; i++)
        {
            if (texArr[i].texturePath.Equals(targetTexture))
            {
                UITexture curTex = texArr[i];
                curTex.pivot = UIWidget.Pivot.Left ;
                curTex.width = curTex.width/2;
                if (curTex.transform.childCount > 0)
                {
                    Debug.LogError(string.Format("{0}: have some child ! child count is {1}" , gObj.name , curTex.transform.childCount));
                    return false;
                }
                GameObject copy = GameObject.Instantiate(curTex.gameObject) as GameObject;
                copy.name = copy.name.Replace("(Clone)", "(Mirror)");
                copy.transform.SetParent(curTex.transform);
                copy.SetActive(true);

                UITexture copyTex = copy.GetComponent<UITexture>();
                copyTex.pivot = UIWidget.Pivot.Right;

                copy.transform.localPosition = new Vector3(curTex.width , 0 , 0);
                copy.transform.localScale = new Vector3(-1 , 1, 1);
            }
        }
        return true;
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
                        if (sprite.atlas != null && spriteName.Equals(sprite.spriteName))
                        {
                            prefabStr += file + "\n";
                            prefabStr += "    " + temps[j].name + "   [UISprite]\n";
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
