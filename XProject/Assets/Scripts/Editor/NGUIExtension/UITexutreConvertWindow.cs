using System.Collections.Generic;
using System.IO;
using System.Text;
using LuaFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Editor.UITools
{
    /// <summary>
    /// UITexture替换工具
    /// </summary>
    public class UITexutreConvertWindow : EditorWindow
    {
        public const string BIG_TEXTURE_FOLDER = "Assets/Res/Texture/";

        public static string[] UIFOLDERS = new[]
        {
            "Assets/Res/Prefab/Gui",
        };

        public static string[] IGNORE_FOLDERS = new[]
        {
            "Assets/Res/Texture/common"
        };

        private List<string> texPrefabs;
        private Dictionary<string, List<string>> depTextures = new Dictionary<string, List<string>>(); 
        private Vector2 scrollPos;
        private string searchFilter;
        private HashSet<string> openSub = new HashSet<string>();
        #region --------------------------目录--------------------------------------------
        //[MenuItem("NGUI//", false, 31)]
        public static void SpaceMenu() { }

        [MenuItem("NGUI/UITextureReplace" , false , 32)]
        public static void ShowEditor()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float) prefabArr.Length;
            List<string> texturePrefabs = new List<string>();
            Dictionary<string , List<string>> depTextureDic = new Dictionary<string, List<string>>();

            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索" , "正在查找包含UITexture的资源..." , i / count);

                string[] depAssets = AssetDatabase.GetDependencies(path);
                bool hasTexture = false;
                for (int j = 0; j < depAssets.Length; j++)
                {
                    if (isFilter(depAssets[j]))   continue;

                    if (!hasTexture) {
                        texturePrefabs.Add(path);
                        depTextureDic[path] = new List<string>();
                        hasTexture = true;
                    }

                    depTextureDic[path].Add(depAssets[j]);
                }
            }
            EditorUtility.ClearProgressBar();

            texturePrefabs.Sort((x , y)=> depTextureDic[x].Count.CompareTo(depTextureDic[y].Count));

            UITexutreConvertWindow texRep = GetWindow<UITexutreConvertWindow>("UITexture转换器");
            texRep.texPrefabs = texturePrefabs;
            texRep.depTextures = depTextureDic;
            texRep.Show();
        }


        /// <summary>
        /// 添加所有指定目录的Prefab组件的Sound音频
        /// </summary>
        [MenuItem("NGUI/Add UISound", false, 33)]
        public static void SpawnUISound()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float)prefabArr.Length;
            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索", "正在查找包含可点击的资源...", i / count);

                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                AddUISound(instance);

                PrefabUtility.ReplacePrefab(instance, prefab);
                GameObject.DestroyImmediate(instance);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        public static void AddUISound(GameObject gObj)
        {
            byte soundType = 0;
#if NGUI
            UIButton btn = gObj.GetComponent<UIButton>();
            if (btn == null)
            {
                UIButtonScale btnScale = gObj.GetComponent<UIButtonScale>();
                soundType = (byte)(btnScale != null ? 1 : 0);
            }else
                soundType = 1;

            if (soundType == 0)
            {
                UIToggle tog = gObj.GetComponent<UIToggle>();
                if(tog != null)
                    soundType = (byte) (tog.@group != 0 ? 2 : 1);
            }
#else
            Button btn = gObj.GetComponent<Button>();
            soundType = (byte)(btn == null ? 0 : 1);

            if(soundType == 0)
            {
                Toggle tog = gObj.GetComponent<Toggle>();
                if (tog != null)
                    soundType = (byte)(tog.group == null ? 1 : 2);
            }
#endif
            if (soundType != 0)
            {
                UISound sound = gObj.GetComponent<UISound>();
                if (sound == null)
                {
                    sound = gObj.AddComponent<UISound>();
                    sound.audioName = soundType == 1 ? AppConst.UISoundConfig[0] : AppConst.UISoundConfig[1];
                }
                
            }

            Transform t = gObj.transform;
            for (int i = 0 , max = t.childCount; i < max; i++)
            {
                AddUISound(t.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 给特定组件添加UIMask
        /// </summary>
        [MenuItem("NGUI/Add UIMask", false, 34)]
        public static void SpawnUIMask()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float)prefabArr.Length;
            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索", "正在查找UIMask的资源...", i / count);

                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                if(addUIMask(instance))
                    PrefabUtility.ReplacePrefab(instance, prefab);
                GameObject.DestroyImmediate(instance);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        private const string mask = "mask";
        private static bool addUIMask(GameObject gObj)
        {
            if (gObj.name.ToLower().StartsWith(mask))
            {
                gObj.transform.GetOrAddComponent<UIMask>();
                return true;
            }

            Transform t = gObj.transform;
            for (int i = 0, max = t.childCount; i < max; i++)
            {
                if (addUIMask(t.GetChild(i).gameObject)) return true;
            }
            return false;
        }

        /// <summary>
        /// 将UILabel的keepcrisp属性的值改成Never
        /// </summary>
        [MenuItem("NGUI/Label Keepcrisp Never", false, 35)]
        public static void SpawnLabelKeepcrisp()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float)prefabArr.Length;
            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索", "正在查找包含UILabel...", i / count);

                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                if (ChangeKeepcrispToNever(instance))
                    PrefabUtility.ReplacePrefab(instance, prefab);

                GameObject.DestroyImmediate(instance);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        private static bool ChangeKeepcrispToNever(GameObject go)
        {
            bool needReplace = false;
            UILabel lab = go.GetComponent<UILabel>();
            if (lab != null && lab.keepCrispWhenShrunk != UILabel.Crispness.Never)
            {
                lab.keepCrispWhenShrunk = UILabel.Crispness.Never;
                needReplace = true;
            }

            Transform t = go.transform;
            for (int i = 0, max = t.childCount; i < max; i++)
            {
                if (ChangeKeepcrispToNever(t.GetChild(i).gameObject))
                    needReplace = true;
            }

            return needReplace;
        }
#endregion
        /// <summary>
        /// 是否过滤
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static bool isFilter(string asset)
        {
            if (!asset.StartsWith(BIG_TEXTURE_FOLDER, System.StringComparison.OrdinalIgnoreCase)) return true;

            for (int i = 0; i < IGNORE_FOLDERS.Length; i++)
            {
                if (asset.StartsWith(IGNORE_FOLDERS[i], System.StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
        /// <summary>
        /// 替换资源
        /// </summary>
        private static void replaceUITexture(List<string> texturePrefabs)
        {
            float count = (float)texturePrefabs.Count;
            for (int i = 0; i < texturePrefabs.Count; i++)
            {
                string path = texturePrefabs[i];

                EditorUtility.DisplayProgressBar("操作", "正在转换包含UITexture的资源...", i / count);
               
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                GameObject gObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (updateUITexture(gObj, false))
                {
                    PrefabUtility.ReplacePrefab(gObj, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }
                GameObject.DestroyImmediate(gObj);
            }
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 转换所有指定目录的Prefab
        /// </summary>
        public static void CovertUITexture()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float)prefabArr.Length;
            List<string> texturePrefabs = new List<string>();

            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索", "正在查找包含UITexture的资源...", i / count);

                string[] depAssets = AssetDatabase.GetDependencies(path);
                for (int j = 0; j < depAssets.Length; j++)
                {
                    if (isFilter(depAssets[j])) continue;
                    texturePrefabs.Add(path);
                    break;
                }
            }
            
            //替换
            replaceUITexture(texturePrefabs);
        }

        [MenuItem("NGUI/Convert UITexture Path Lower")]
        public static void CovertUITexturePathLower()
        {
            string[] prefabArr = AssetDatabase.FindAssets("t:GameObject", UIFOLDERS);

            float count = (float)prefabArr.Length;
            List<string> texturePrefabs = new List<string>();

            for (int i = 0; i < prefabArr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabArr[i]);

                EditorUtility.DisplayProgressBar("搜索", "正在查找包含UITexture的资源...", i / count);

                texturePrefabs.Add(path);
            }

            //替换
            replaceUITexture(texturePrefabs);
        }

        private static bool updateUITexture(GameObject gObj , bool isDirty)
        {
            UITexture uiTex = gObj.GetComponent<UITexture>();
            if (formatTexture(uiTex)) isDirty = true;

            Transform trans = gObj.transform;
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (updateUITexture(trans.GetChild(i).gameObject, isDirty))
                    isDirty = true;
            }
            return isDirty;
        }


        private static void showAssetDepence(string assetPath)
        {
            string[] depArr = AssetDatabase.GetDependencies(assetPath);
            StringBuilder buf = new StringBuilder();
            foreach (string dep in depArr)
            {
                buf.AppendLine(dep);
            }
            Debug.Log(buf.ToString());
        }


        private static bool formatTexture(UITexture tex)
        {
            if (tex == null) return false;

            if (tex.baseTexture != null)
            {
                tex.texturePath = AssetDatabase.GetAssetPath(tex.baseTexture).ToLower();
                tex.baseTexture = null;               
            }
            tex.texturePath = tex.texturePath.ToLower();

            return true;
        }
        
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(false , "名称", "ButtonLeft" , GUILayout.MinWidth(200));
            GUILayout.Toggle(false, "数量", "ButtonMid", GUILayout.Width(80));
            GUILayout.Toggle(false, "操作", "ButtonRight", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            // Search field
            GUILayout.BeginHorizontal();
            {
                string after = EditorGUILayout.TextField("", searchFilter, "SearchTextField", GUILayout.Width(Screen.width - 20f));

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    after = "";
                    GUIUtility.keyboardControl = 0;
                }

                if (searchFilter != after)
                {
                    NGUISettings.searchField = after;
                    searchFilter = after;
                }
            }
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            string selection = null;
            int index = 0;
            for (int i = texPrefabs.Count - 1; i >= 0; i--)
            {
                string fileName = Path.GetFileName(texPrefabs[i]);
                if (!string.IsNullOrEmpty(searchFilter) && !fileName.Contains(searchFilter)) continue;

                bool highlight = index % 2 == 0;
                index++;

                GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                bool isOpenSub = openSub.Contains(texPrefabs[i]);
                int count = depTextures[texPrefabs[i]].Count;
                if (count > 0)
                {
                    if (GUILayout.Button(isOpenSub ? "-" : "+", GUILayout.Width(20)))
                    {
                        if (isOpenSub) openSub.Remove(texPrefabs[i]);
                        else
                        {
                            isOpenSub = true;
                            openSub.Add(texPrefabs[i]);
                        }
                            
                    }   
                }

                if (GUILayout.Button(texPrefabs[i], "OL TextField", GUILayout.MinWidth(200f), GUILayout.Height(20f)))
                {
                    selection = texPrefabs[i];
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(texPrefabs[i]);
                }

                GUILayout.Label(count.ToString() , GUILayout.Width(80));

                if (GUILayout.Button("X" , GUILayout.Width(30)))
                    texPrefabs.RemoveAt(i);

                GUILayout.Space(30);
                GUILayout.EndHorizontal();

                //详细的Texture
                if (isOpenSub)
                {
                    EditorGUI.indentLevel ++;
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);
                    foreach (string depTex in depTextures[texPrefabs[i]])
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50 * EditorGUI.indentLevel);
                        if (GUILayout.Button(depTex, "OL TextField", GUILayout.Height(20f)))
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(depTex);
                        GUILayout.EndHorizontal();
                    }
                    GUI.backgroundColor = Color.white;
                    EditorGUI.indentLevel --;                    
                }

            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            if (GUILayout.Button("Convert" , GUILayout.Height(30)))
                replaceUITexture(texPrefabs);
            GUILayout.Space(50);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }
}