using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Riverlake.Editor.I18N
{
    /// <summary>
    /// 语言本地化主窗口
    /// </summary>
    public class I18NWindow : EditorWindow
    {
       
        private enum Language
        {
            SimpleChinese , TraditionalChinese , English , Japan , Korean , Vietnam
        }
        private string[] languageStrArr = new[] { "简体中文", "繁体中文", "美式英文", "日文", "韩文" , "越南"};
        private string[] outArr = new[] { "zh", "cht", "en", "jp", "kor" , "vie" };

        private Language curLanguage = Language.SimpleChinese;
        private Language targetLanguage = Language.English;

        //导出的中文KEY路径
        private static string OutPath = Application.dataPath + "/Res/I18N/out_{0}.txt";
        private static string editorFoldersPath = "Assets/Scripts/Editor/I18N/Folders.txt";

        private string Lua_Script = ".lua";
        private string CSharp_Script = ".cs";
        private string Prefab = ".prefab";

        private Dictionary<string , ITranslater> translaters = new Dictionary<string, ITranslater>();
        private TranslaterOnline translaterOnline;

        private bool isFoldCShape;
        private bool isFoldLua;
        private bool isFoldPrefab;
        private Vector2 scrollPos;

        private Dictionary<string, List<FolderGUIState>> folders = new Dictionary<string, List<FolderGUIState>>();

        private GUIStyle textButton = new GUIStyle() {normal = new GUIStyleState() {textColor = new Color(0.8f , 0.8f ,0.8f)} };


        #region -------------------

        private class FolderGUIState
        {
            public bool Toggle = true;
            public string Folder;
        }
        #endregion
        #region -----工具栏目录
        private const string MENU_MAIN_WINDOW = "Tools/I18N多语言";

        [MenuItem(MENU_MAIN_WINDOW)]
        private static void ShowMainWindow()
        {
            I18NWindow window = EditorWindow.GetWindow<I18NWindow>("I18N多语言");
            window.Initlize();
            window.Show();
        }
        #endregion

        public void Initlize()
        {
            translaters[CSharp_Script] = new CScriptTranslater();
            translaters[Prefab] = new PrefabTranslater();
            translaters[Lua_Script] = new LuaTranslater();

            folders[Prefab] = new List<FolderGUIState>();
            folders[CSharp_Script] = new List<FolderGUIState>();
            folders[Lua_Script] = new List<FolderGUIState>();

            //读取记录
            if (File.Exists(editorFoldersPath))
            {
                string[] lines = File.ReadAllLines(editorFoldersPath);
                List<FolderGUIState> folderGuiStates = null;
                foreach (string line in lines)
                {
                    if (line.StartsWith("key:"))
                    {
                        folderGuiStates = folders[line.Replace("key:", "")];
                    }else
                        folderGuiStates.Add(new FolderGUIState() {Folder = line});
                }                
            }
        }

        private void OnGUI()
        {
            NGUIEditorTools.DrawHeader("Import");
            
            this.importFoldersGUI();

            NGUIEditorTools.DrawHeader("Option");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("翻译：" , GUILayout.Width(80));

            targetLanguage = (Language)EditorGUILayout.EnumPopup("", targetLanguage);
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Export")) //, GUILayout.Width(80)
            {
                this.exportSourcesLanguage();
            }

            if (GUILayout.Button("Export + Online")) //, GUILayout.Width(80)
            {
                this.exportSourcesAndOnlineLanguage();
            }
            //            NGUIEditorTools.DrawHeader("Translater");
            
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Translater" , GUILayout.Height(30)))
            {
                this.onTranlateAssets();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
        }

        /// <summary>
        /// 导入文件夹
        /// </summary>
        private void importFoldersGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            isFoldCShape = foldoutFolder(isFoldCShape, "C#脚本目录" , folders[CSharp_Script]);
            
            isFoldLua = foldoutFolder(isFoldLua, "Lua脚本目录" , folders[Lua_Script]);

            isFoldPrefab = foldoutFolder(isFoldPrefab, "Prefab目录" , folders[Prefab]);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            if (GUILayout.Button("Save Import Folders"))
            {
                StringBuilder buf = new StringBuilder();
                foreach (string key in folders.Keys)
                {
                    buf.AppendLine("key:" + key);
                    foreach (FolderGUIState folderState in folders[key])
                    {
                        if(!string.IsNullOrEmpty(folderState.Folder))
                            buf.AppendLine(folderState.Folder);
                    }                    
                }
                File.WriteAllText(editorFoldersPath , buf.ToString());

                AssetDatabase.ImportAsset(editorFoldersPath);

                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(editorFoldersPath);
            }
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();
        }


        private bool foldoutFolder(bool isFoldout, string title, List<FolderGUIState> folders)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            isFoldout = EditorGUILayout.Foldout(isFoldout, title);

            GUILayout.FlexibleSpace();

            if (isFoldout && GUILayout.Button("+" , GUILayout.Width(85)))
            {
                folders.Add(new FolderGUIState());
            }

            EditorGUILayout.EndHorizontal();


            if (isFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(55);
                bool isAll = false;
                isAll = GUILayout.Button("全选", textButton, GUILayout.Width(50));

                bool isAllCancle = false;
                isAllCancle = GUILayout.Button("全取消", textButton, GUILayout.Width(50));

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < folders.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    if(isAll) folders[i].Toggle = true;
                    if (isAllCancle) folders[i].Toggle = false;
                    folders[i].Toggle = EditorGUILayout.Toggle(folders[i].Toggle, GUILayout.Width(30));

                    EditorGUILayout.TextField(folders[i].Folder);

                    if (GUILayout.Button("..." , GUILayout.Width(50)))
                    {
                        string folderPath = string.IsNullOrEmpty(folders[i].Folder)
                            ? Application.dataPath: folders[i].Folder;
                        folderPath = EditorUtility.OpenFolderPanel("选择", folderPath, "Res");
                        if(!string.IsNullOrEmpty(folderPath))
                            folders[i].Folder = folderPath.Replace(Application.dataPath, "Assets");
                    }

                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        folders.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            return isFoldout;
        }
        /// <summary>
        /// 导出源语言
        /// </summary>
        private void exportSourcesAndOnlineLanguage()
        {
            if (curLanguage == targetLanguage || targetLanguage == Language.SimpleChinese)  return;

            List<TranslateMapper> transMaps = new List<TranslateMapper>();
            List<string> files = searchSelectFiles();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                
                ITranslater translater = getTranslater(file);
                if(translater == null)  continue;

                transMaps.Add(translater.Export(file.Replace("\\" , "/")));

                EditorUtility.DisplayProgressBar("导出", "正在查找文件...", i / (float)files.Count);
            }
            EditorUtility.ClearProgressBar();

            //最终把提取的中文生成出来
            string textPath = string.Format(OutPath , outArr[(int)targetLanguage]);
            if (System.IO.File.Exists(textPath))
            {
                //读取已翻译的内容
                string[] lines = File.ReadAllLines(textPath);
                string fileStart = "-------";
                TranslateMapper tm = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(fileStart))
                    {
                        string filePath = lines[i].Replace("-", "").Trim();
                        TranslateMapper hasTm = null;
                        for (int j = 0; j < transMaps.Count; j++)
                        {
                            if (transMaps[j].FilePath.Equals(filePath))
                            {
                                hasTm = transMaps[j];
                                tm = hasTm;
                                break;
                            }
                        }

                        if (hasTm == null)
                        {
                            //没找到文件，保留原翻译记录
                            tm = new TranslateMapper(filePath);
                            transMaps.Add(tm);                            
                        }
                    }
                    else
                    {
                        string[] worlds = lines[i].Trim().Split('=');
                        if(worlds.Length > 1)
                            tm.SetTranslate(worlds[0].Trim() , worlds[1].Trim());
                    }
                }
            }

            // 去掉多文件重复
            HashSet<string> map = new HashSet<string>();
            float totalWorldCount = 0;
            for (int i = 0, count = transMaps.Count; i < count; i++)
            {
                TranslateMapper tm = transMaps[i];
                for (int j = tm.Worlds.Count - 1; j >= 0; j--)
                {
                    TranslatorWorld transWorld = tm.Worlds[j];
                    if (map.Contains(transWorld.Source))
                        tm.Worlds.RemoveAt(j);
                    else
                    {
                        totalWorldCount += 1;
                        map.Add(transWorld.Source);
                    }
                        
                }
            }

            TranslaterOnline translaterOnline = new TranslaterOnline();
            StringBuilder buf = new StringBuilder();
            int totalWorldIndex = 0;
            for (int i = 0, count = transMaps.Count; i < count; i++)
            {
                TranslateMapper tm = transMaps[i];
                if (tm.Worlds.Count == 0) continue;

                buf.AppendLine(string.Format("--------------------{0}", tm.FilePath));
                for (int j = 0, worldCount = tm.Worlds.Count; j < worldCount; j++)
                {
                    TranslatorWorld transWorld = tm.Worlds[j];

                    if(string.IsNullOrEmpty(transWorld.Dest))
                         transWorld.Dest = translaterOnline.Translater(transWorld.Source, outArr[(int)targetLanguage]);
                        
                    buf.AppendLine(string.Format("{0} = {1}", transWorld.Source, transWorld.Dest));

                    totalWorldIndex++;
                    EditorUtility.DisplayProgressBar("翻译", "正在翻译文件...", totalWorldIndex / totalWorldCount);
                }
            }

            using (StreamWriter writer = new StreamWriter(textPath, false, System.Text.Encoding.UTF8))
            {
                writer.Write(buf.ToString());
            }
            EditorUtility.ClearProgressBar();

            string assetFilePath = textPath.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(assetFilePath);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFilePath);
            EditorUtility.DisplayDialog("提示", "已经成功导出为:" + languageStrArr[(int) targetLanguage], "确定");
        }

        /// <summary>
        /// 导出源语言
        /// </summary>
        private void exportSourcesLanguage()
        {
            if (curLanguage == targetLanguage || targetLanguage == Language.SimpleChinese) return;

            List<TranslateMapper> transMaps = new List<TranslateMapper>();
            List<string> files = searchSelectFiles();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                ITranslater translater = getTranslater(file);
                if (translater == null) continue;

                transMaps.Add(translater.Export(file.Replace("\\", "/")));

                EditorUtility.DisplayProgressBar("匹配", "正在匹配中文...", i / (float)files.Count);
            }
            EditorUtility.ClearProgressBar();

            //最终把提取的中文生成出来
            string textPath = string.Format(OutPath, outArr[(int)targetLanguage]);
            if (System.IO.File.Exists(textPath))
            {
                //读取已翻译的内容
                string[] lines = File.ReadAllLines(textPath);
                string fileStart = "-------";
                TranslateMapper tm = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(fileStart))
                    {
                        string filePath = lines[i].Replace("-", "").Trim();
                        TranslateMapper hasTm = null;
                        for (int j = 0; j < transMaps.Count; j++)
                        {
                            if (transMaps[j].FilePath.Equals(filePath))
                            {
                                hasTm = transMaps[j];
                                tm = hasTm;
                                break;
                            }
                        }

                        if (hasTm == null)
                        {
                            //没找到文件，保留原翻译记录
                            tm = new TranslateMapper(filePath);
                            transMaps.Add(tm);
                        }
                    }
                    else
                    {
                        string[] worlds = lines[i].Trim().Split('=');
                        if (worlds.Length > 1)
                            tm.SetTranslate(worlds[0].Trim(), worlds[1].Trim());
                    }
                }
            }

            // 去掉多文件重复
            HashSet<string> map = new HashSet<string>();
            float totalWorldCount = 0;
            for (int i = 0, count = transMaps.Count; i < count; i++)
            {
                TranslateMapper tm = transMaps[i];
                for (int j = tm.Worlds.Count - 1; j >= 0; j--)
                {
                    TranslatorWorld transWorld = tm.Worlds[j];
                    if (map.Contains(transWorld.Source))
                        tm.Worlds.RemoveAt(j);
                    else
                    {
                        totalWorldCount += 1;
                        map.Add(transWorld.Source);
                    }

                }
            }

            StringBuilder buf = new StringBuilder();
            int totalWorldIndex = 0;
            for (int i = 0, count = transMaps.Count; i < count; i++)
            {
                TranslateMapper tm = transMaps[i];
                if (tm.Worlds.Count == 0) continue;

                buf.AppendLine(string.Format("--------------------{0}", tm.FilePath));
                for (int j = 0, worldCount = tm.Worlds.Count; j < worldCount; j++)
                {
                    TranslatorWorld transWorld = tm.Worlds[j];

                    buf.AppendLine(string.Format("{0} = {1}", transWorld.Source, transWorld.Dest));

                    totalWorldIndex++;
                    EditorUtility.DisplayProgressBar("翻译", "正在导出文件...", totalWorldIndex / totalWorldCount);
                }
            }

            using (StreamWriter writer = new StreamWriter(textPath, false, System.Text.Encoding.UTF8))
            {
                writer.Write(buf.ToString());
            }
            EditorUtility.ClearProgressBar();

            string assetFilePath = textPath.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(assetFilePath);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFilePath);
            EditorUtility.DisplayDialog("提示", "已经成功导出为:" + languageStrArr[(int)targetLanguage], "确定");
        }
        /// <summary>
        /// 翻译资源
        /// </summary>
        private void onTranlateAssets()
        {
            string textPath = string.Format(OutPath, outArr[(int)targetLanguage]);
            if (!System.IO.File.Exists(textPath))
            {
                EditorUtility.DisplayDialog("错误", "找不到对应的翻译文件，目标语言:" + languageStrArr[(int)targetLanguage], "确定");
                return;
            }

            Dictionary<string , TranslateMapper> transMaps = new Dictionary<string , TranslateMapper>();

            //读取已翻译的内容
            string[] lines = File.ReadAllLines(textPath);
            string fileStart = "-------";
            TranslateMapper tm = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(fileStart))
                {
                    string filePath = lines[i].Replace("-", "").Trim();
                    tm = new TranslateMapper(filePath);
                    transMaps[tm.FilePath] = tm;
                }
                else
                {
                    string[] worlds = lines[i].Trim().Split('=');
                    if (worlds.Length > 1)
                        tm.SetTranslate(worlds[0].Trim(), worlds[1].Trim());
                }
            }

            //翻译转换资源
            List<string> files = searchSelectFiles();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                ITranslater translater = getTranslater(file);
                if (translater == null) continue;

                string filePath = file.Replace("\\", "/");
                if (!transMaps.ContainsKey(filePath))
                {
                    transMaps[filePath] = new TranslateMapper(filePath);
                }
                translater.Translater(transMaps[filePath]);

                EditorUtility.DisplayProgressBar("翻译", "正在翻译文件...", i / (float)files.Count);
            }
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", "已经完成翻译转换", "确定");
        }

        /// <summary>
        /// 获得对应类型的翻译器
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private ITranslater getTranslater(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (!translaters.ContainsKey(extension))
            {
                Debug.LogError("找不到对应类型的翻译器");
                return null;
            }
            return translaters[extension];
        }

        /// <summary>
        /// 查找所有已选择的目录文件
        /// </summary>
        /// <returns></returns>
        private List<string> searchSelectFiles(bool all = false)
        {
            List<string> files = new List<string>();
            int index = 0;
            float count = 0;
            foreach (List<FolderGUIState> folderStates in folders.Values)
                count += folderStates.Count;
            count = Mathf.Max(count, 1);

            foreach (string key in folders.Keys)
            {
                List<FolderGUIState> folderStates = folders[key];
                foreach (FolderGUIState fdState in folderStates)
                {
                    EditorUtility.DisplayProgressBar("查询" , "正在查找文件..." , index++ / count);
                    if(!fdState.Toggle && !all)     continue;

                    files.AddRange(seachFolder(fdState, string.Concat("*" , key)));
                }
            }

            EditorUtility.ClearProgressBar();
            return files;
        } 

        private List<string> seachFolder(FolderGUIState folder, string searchPattern)
        {
            List<string> files = new List<string>();
            
            if(string.IsNullOrEmpty(folder.Folder)) return files;

            string[] fileArr = Directory.GetFiles(folder.Folder, searchPattern , SearchOption.AllDirectories);
            files.AddRange(fileArr);
            
            return files;
        } 
    }
}