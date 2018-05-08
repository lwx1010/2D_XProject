using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class FBXAnimationEditor : EditorWindow
{
    private string modelPath = Application.dataPath + "/Models";      //模型路径
    private string jsonPath = Application.dataPath + "/Models/FBXAnimations.json";
    public static string[] filePaths;
    private Vector2 deltaPosition;
    public static AnimModel curAnimModel;     //当前模型
    private GUIStyle leftStyel = new GUIStyle();
    private GUIStyle rightStyle = new GUIStyle();
    private string searchFilter = string.Empty;
    public static Hashtable jsonMap = new Hashtable();

    [MenuItem("ArtTools/模型动画剪辑")]
    static void OpenEditor()
    {
        var window = GetWindow<FBXAnimationEditor>();
        window.Show();
    }

    void OnEnable()
    {
        leftStyel.fixedWidth = 500;
        //leftStyel.fixedHeight = 500;
        rightStyle.fixedWidth = 600;
        //rightStyle.fixedHeight = 500;
        LoadJson();
        GetAllModelPath();
    }

    private void OnDestroy()
    {
        filePaths = null;
        curAnimModel = null;
        jsonMap.Clear();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("一键动画转配置", GUILayout.Width(100)))
        {
            AnimationToJson();
            EditorUtility.DisplayDialog("提示", "配置生成成功", "确定");
        }
        if (GUILayout.Button("一键配置转动画", GUILayout.Width(100)))
        {
            JsonToAnimation();
            EditorUtility.DisplayDialog("提示", "动画生成成功", "确定");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("", searchFilter, "SearchTextField");
        if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
        {
            searchFilter = "";
            GUIUtility.keyboardControl = 0;
        }
        EditorGUILayout.EndHorizontal();
        //////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        deltaPosition = EditorGUILayout.BeginScrollView(deltaPosition);
        for (int i = 0; i < filePaths.Length; i++)
        {
            EditorGUILayout.BeginVertical();
            string path = filePaths[i].Replace(Application.dataPath + "/", "");
            if (path.ToLower().Contains(searchFilter.ToLower()))
            {
                ModelPathData data = new ModelPathData(i, path);
                data.ShowGUI();
            }
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (curAnimModel != null)
        {
            curAnimModel.ShowGUI();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        //////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存配置", GUILayout.Width(80)))
        {
            SaveJson();
            EditorUtility.DisplayDialog("提示", "配置保存成功", "确定");
        }
        EditorGUILayout.EndHorizontal();
    }

    private void GetAllModelPath()
    {
        List<string> withExtensions = new List<string>() { ".fbx" };
        filePaths = Directory.GetFiles(modelPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
    }

    private void LoadJson()
    {
        FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamReader sr = new StreamReader(fs, Encoding.UTF8);
        string jsonString = sr.ReadToEnd();
        sr.Close();
        fs.Close();
        object jsonParsed = MiniJSON.Json.Deserialize(jsonString);
        if (jsonParsed != null)
        {
            jsonMap = jsonParsed as Hashtable;
        }
    }

    private void SaveJson()
    {
        string jsonString = MiniJSON.Json.Serialize(jsonMap);
        FileStream fs = new FileStream(jsonPath, FileMode.Open, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write(jsonString);
        sw.Close();
        fs.Close();
    }

    private void AnimationToJson()
    {
        jsonMap = new Hashtable();
        EditorUtility.DisplayProgressBar("Hold on", "", 0);
        for (int i = 0; i < filePaths.Length; i++)
        {
            string path = filePaths[i].Replace(Application.dataPath + "/", "");

            List<AnimClip> list = GetAnimationClip(path);
            Hashtable modelTable = new Hashtable();
            for (int j = 0; j < list.Count; j++)
            {
                Hashtable clipTable = new Hashtable();
                clipTable.Add("name", list[j].name);
                clipTable.Add("startFrame", list[j].startFrame);
                clipTable.Add("endFrame", list[j].endFrame);
                clipTable.Add("loopTime", list[j].loopTime);
                modelTable.Add(j.ToString(), clipTable);
            }
            jsonMap.Add(path, modelTable);
            EditorUtility.DisplayProgressBar("Hold on", "", (float)i / (float)filePaths.Length);
        }
        SaveJson();
        EditorUtility.ClearProgressBar();
    }

    private void JsonToAnimation()
    {
        EditorUtility.DisplayProgressBar("Hold on", "", 0);
        for (int i = 0; i < filePaths.Length; i++)
        {
            string path = filePaths[i].Replace(Application.dataPath + "/", "");
            ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath("Assets/" + path);
            if (jsonMap.ContainsKey(path))
            {
                if (modelImporter.importAnimation == false)
                {
                    modelImporter.importAnimation = true;
                }
                Hashtable modelTable = jsonMap[path] as Hashtable;
                List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();
                for (int j = 0; j < modelTable.Count; j++)
                {
                    Hashtable clipTable = modelTable[j.ToString()] as Hashtable;
                    ModelImporterClipAnimation mica = new ModelImporterClipAnimation();
                    if (clipTable.ContainsKey("name"))
                    {
                        mica.name = clipTable["name"].ToString();
                    }
                    if (clipTable.ContainsKey("startFrame"))
                    {
                        mica.firstFrame = float.Parse(clipTable["startFrame"].ToString());
                    }
                    if (clipTable.ContainsKey("endFrame"))
                    {
                        mica.lastFrame = float.Parse(clipTable["endFrame"].ToString());
                    }
                    if (clipTable.ContainsKey("loopTime"))
                    {
                        mica.loopTime = bool.Parse(clipTable["loopTime"].ToString());
                    }
                    clips.Add(mica);
                }
                modelImporter.clipAnimations = clips.ToArray();
                modelImporter.SaveAndReimport();
                //AssetDatabase.WriteImportSettingsIfDirty("Assets/" + path);
                //AssetDatabase.ImportAsset("Assets/" + path);
            }
            EditorUtility.DisplayProgressBar("Hold on", "", (float)i / (float)filePaths.Length);
        }
        EditorUtility.ClearProgressBar();
    }

    public static List<AnimClip> GetAnimationClip(string path)
    {
        List<AnimClip> clipList = new List<AnimClip>();
        ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath("Assets/" + path);

        for (int i = 0; i < modelImporter.clipAnimations.Length; i++)
        {
            AnimClip clip = new AnimClip();
            clip.name = modelImporter.clipAnimations[i].name;
            clip.startFrame = modelImporter.clipAnimations[i].firstFrame;
            clip.endFrame = modelImporter.clipAnimations[i].lastFrame;
            clip.loopTime = modelImporter.clipAnimations[i].loopTime;
            clipList.Add(clip);
        }
        return clipList;
    }

    public class ModelPathData
    {
        public int index = 0;
        public string name = string.Empty;

        public ModelPathData(int index, string name)
        {
            this.index = index;
            this.name = name;
        }

        public void ShowGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (jsonMap.ContainsKey(name))
                GUI.contentColor = Color.white;
            else
                GUI.contentColor = Color.red;
            EditorGUILayout.LabelField(name);
            GUILayout.Space(10);
            GUI.contentColor = Color.white;
            if (GUILayout.Button("编辑", GUILayout.Width(80)))
            {
                AnimModel model = new AnimModel();
                model.Load(name);
                curAnimModel = model;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class AnimModel
    {
        public string name = string.Empty;          //模型名称
        public List<AnimClip> clipList = new List<AnimClip>();
        private Vector2 deltaPosition;
        private string path;

        public bool ContainByName(string name)
        {
            for (int i = 0; i < clipList.Count; i++)
            {
                if (clipList[i].name.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public void AddClip(AnimClip clip)
        {
            if (ContainByName(clip.name))
            {
                return;
            }
            clipList.Add(clip);
        }

        public void RemoveClip(AnimClip clip)
        {
            clipList.Remove(clip);
        }

        public void RemoveClip(int index)
        {
            clipList.RemoveAt(index);
        }

        public void Load(string path)
        {
            this.path = path;
            int startIndex = path.LastIndexOf("\\");
            int endIndex = path.LastIndexOf(".");
            name = path.Substring(startIndex + 1, endIndex - startIndex - 1);
            //如果配置中有，优先读配置数据
            if (jsonMap.ContainsKey(path))
            {
                Hashtable modelTable = jsonMap[path] as Hashtable;
                for (int i = 0; i < modelTable.Count; i++)
                {
                    Hashtable clipTable = modelTable[i.ToString()] as Hashtable;
                    AnimClip clip = new AnimClip();
                    if (clipTable.ContainsKey("name"))
                    {
                        clip.name = clipTable["name"].ToString();
                    }
                    if (clipTable.ContainsKey("startFrame"))
                    {
                        clip.startFrame = float.Parse(clipTable["startFrame"].ToString());
                    }
                    if (clipTable.ContainsKey("endFrame"))
                    {
                        clip.endFrame = float.Parse(clipTable["endFrame"].ToString());
                    }
                    if (clipTable.ContainsKey("loopTime"))
                    {
                        clip.loopTime = bool.Parse(clipTable["loopTime"].ToString());
                    }
                    clipList.Add(clip);
                }
            }
            else
            {
                clipList = GetAnimationClip(path);
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/" + path, typeof(GameObject));
        }

        public void ShowGUI()
        {
            EditorGUILayout.LabelField("模型名称：" + name);
            deltaPosition = EditorGUILayout.BeginScrollView(deltaPosition);
            for (int i = 0; i < clipList.Count; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ClipName:", GUILayout.Width(70));
                clipList[i].name = EditorGUILayout.TextField(clipList[i].name, GUILayout.Width(100));
                EditorGUILayout.LabelField("StartFrame:", GUILayout.Width(80));
                clipList[i].startFrame = EditorGUILayout.FloatField(clipList[i].startFrame, GUILayout.Width(40));
                EditorGUILayout.LabelField("EndFrame:", GUILayout.Width(70));
                clipList[i].endFrame = EditorGUILayout.FloatField(clipList[i].endFrame, GUILayout.Width(40));
                EditorGUILayout.LabelField("LoopTime:", GUILayout.Width(70));
                clipList[i].loopTime = EditorGUILayout.Toggle(clipList[i].loopTime, GUILayout.Width(20));
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    RemoveClip(i);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("读取模型数据", GUILayout.Width(80)))
            {
                clipList = GetAnimationClip(path);
                Hashtable modelTable = new Hashtable();
                for (int i = 0; i < clipList.Count; i++)
                {
                    Hashtable clipTable = new Hashtable();
                    clipTable.Add("name", clipList[i].name);
                    clipTable.Add("startFrame", clipList[i].startFrame);
                    clipTable.Add("endFrame", clipList[i].endFrame);
                    clipTable.Add("loopTime", clipList[i].loopTime);
                    modelTable.Add(i.ToString(), clipTable);
                }
                jsonMap[path] = modelTable;

                Debug.Log("读取成功");
            }
            if (GUILayout.Button("添加", GUILayout.Width(80)))
            {
                AnimClip clip = new AnimClip();
                clip.name = "idle" + clipList.Count;
                clipList.Add(clip);
            }
            if (GUILayout.Button("生成", GUILayout.Width(80)))
            {
                SaveAnimationClip();
                SaveJson();
                Debug.Log("生成成功");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SaveAnimationClip()
        {
            for (int i = 0; i < clipList.Count - 1; i++)
            {
                for (int j = 0; j < clipList.Count; j++)
                {
                    if (i != j)
                    {
                        if (clipList[i].name.ToLower() == clipList[j].name.ToLower())
                        {
                            EditorUtility.DisplayDialog("警告", "存在相同的名称", "确定");
                            return;
                        }
                    }
                }
            }
            ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath("Assets/" + path);
            if (modelImporter.importAnimation == false)
            {
                modelImporter.importAnimation = true;
            }
            List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();
            for (int i = 0; i < clipList.Count; i++)
            {
                ModelImporterClipAnimation mica = new ModelImporterClipAnimation();
                mica.name = clipList[i].name;
                mica.firstFrame = clipList[i].startFrame;
                mica.lastFrame = clipList[i].endFrame;
                mica.loopTime = clipList[i].loopTime;
                clips.Add(mica);
            }
            modelImporter.clipAnimations = clips.ToArray();
            modelImporter.SaveAndReimport();
            //AssetDatabase.WriteImportSettingsIfDirty("Assets/" + path);
            //AssetDatabase.ImportAsset("Assets/" + path);
        }

        private void SaveJson()
        {
            Hashtable modelTable = new Hashtable();
            for (int j = 0; j < clipList.Count; j++)
            {
                Hashtable clipTable = new Hashtable();
                clipTable.Add("name", clipList[j].name);
                clipTable.Add("startFrame", clipList[j].startFrame);
                clipTable.Add("endFrame", clipList[j].endFrame);
                clipTable.Add("loopTime", clipList[j].loopTime);
                modelTable.Add(j.ToString(), clipTable);
            }
            jsonMap[path] = modelTable;
        }
    }

    public class AnimClip
    {
        public string name = string.Empty;
        public float startFrame = 0;
        public float endFrame = 0;
        public bool loopTime = false;
    }
}
