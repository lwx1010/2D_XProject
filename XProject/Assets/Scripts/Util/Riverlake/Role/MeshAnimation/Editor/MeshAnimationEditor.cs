using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Object = UnityEngine.Object;

namespace FSG.MeshAnimator
{
    public class MeshAnimationEditor : EditorWindow
    {
        [System.Serializable]
        public class MeshBakePreferences
        {
            public int fps { get; set; }
            public int previousGlobalBake { get; set; }
            public int globalBake { get; set; }
            public int smoothMeshAngle { get; set; }
            public string[] customClips { get; set; }
            public bool customCompression { get; set; }
            public int rootMotionMode { get; set; }
            public string animController { get; set; }
            public string animAvatar { get; set; }
            public bool combineMeshes { get; set; }
            public string[] exposedTransforms { get; set; }
            public int[] lodDistanceKeys { get; set; }
            public float[] lodDistanceValues { get; set; }
        }

        const int menuIndex = 9990;

        [MenuItem("Tools/Mesh Animator New", false, menuIndex)]
        static void MakeWindow()
        {
            window = GetWindow(typeof (MeshAnimationEditor)) as MeshAnimationEditor;
            window.oColor = GUI.contentColor;
        }

        private static MeshAnimationEditor window;
        private Color oColor;
        private Vector2 scrollpos;
        
        private List<KeyValuePair<int, float>> lodDistances = new List<KeyValuePair<int, float>>();

        [SerializeField] private Vector2 scroll;
        [SerializeField] private int fps = 30;
        [SerializeField] private int previousGlobalBake = 1;
        [SerializeField] private int globalBake = 1;
        [SerializeField] private int smoothMeshAngle = -1;
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject previousPrefab;
        [SerializeField] private List<AnimationClip> customClips = new List<AnimationClip>();
        
        [SerializeField] private bool customCompression = false;
        
        [SerializeField] private List<string> exposedTransforms = new List<string>();
        [SerializeField] private bool exposedOutfold = false;

        private MeshAnimationBake animBaker = new MeshAnimationBake();

        private const string modelDir = "Assets/Models";
        private const string resDir = "Assets/Res";

        private void OnEnable()
        {
            if (prefab == null && Selection.activeGameObject)
            {
                prefab = Selection.activeGameObject;
                animBaker.OnPrefabChanged(prefab);
                previousPrefab = prefab;
                AutoPopulateExposedTransforms();
                LoadPreferencesForAsset();
            }
        }

        private void OnDisable()
        {
            animBaker.OnDisable();
        }

        private string GetAssetPath(string s)
        {
            string path = s;
            string[] split = path.Split('\\');
            path = string.Empty;
            int startIndex = 0;
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i] == "Assets")
                    break;
                startIndex++;
            }
            for (int i = startIndex; i < split.Length; i++)
                path += split[i] + "\\";
            path = path.TrimEnd("\\".ToCharArray());
            path = path.Replace("\\", "/");
            return path;
        }

        private void OnGUI()
        {
            GUI.skin.label.richText = true;
            if (Selection.objects != null && Selection.objects.Length > 1)
            {
                if (GUILayout.Button("Batch Bake Selected Objects"))
                {
                    batchBakes(Selection.gameObjects);
                }
            }

            scroll = GUILayout.BeginScrollView(scroll);
            {
                EditorGUI.BeginChangeCheck();
                prefab = EditorGUILayout.ObjectField("Asset to Bake", prefab, typeof (GameObject), true) as GameObject;
                if (prefab)
                {
                    if (string.IsNullOrEmpty(GetPrefabPath(prefab)))
                    {
                        DrawText("Cannot find asset path, are you sure this object is a prefab?",
                            Color.red + Color.white*0.5f);
                        return;
                    }

                    if (previousPrefab != prefab)
                    {
                        animBaker.OnPrefabChanged(prefab);
                        previousPrefab = prefab;
                        AutoPopulateExposedTransforms();
                        LoadPreferencesForAsset();
                    }
                        
                    for (int i = 0; i < animBaker.skinnedRenderers.Count; i++)
                    {
                        bool remove = false;
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.ObjectField(new GUIContent("Skinned Mesh " + i), animBaker.skinnedRenderers[i],
                                typeof (SkinnedMeshRenderer), true);
                            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                                remove = true;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (remove)
                        {
                            animBaker.skinnedRenderers.RemoveAt(i);
                            break;
                        }
                    }

                    if (GUILayout.Button("+ Add SkinnedMeshRenderer"))
                        animBaker.skinnedRenderers.Add(null);

                    // exposed transforms
                    exposedOutfold = EditorGUILayout.Foldout(exposedOutfold, "Exposed Transforms");
                    if (exposedOutfold)
                    {
                        for (int i = 0; i < exposedTransforms.Count; i++)
                        {
                            bool remove = false;
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(exposedTransforms[i]);
                                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                                    remove = true;
                            }
                            GUILayout.EndHorizontal();
                            if (remove)
                            {
                                exposedTransforms.RemoveAt(i);
                                break;
                            }
                        }
                    }


                    fps = EditorGUILayout.IntSlider("Bake FPS", fps, 1, 500);

                    int[] optionsValues = new int[5] {1, 10, 100, 1000, 10000};
                    string[] options = new string[5] {"0", "0.1", "0.01", "0.001", "0.0001"};
                    int selected = 3;
                    for (int i = 0; i < optionsValues.Length; i++)
                    {
                        if (optionsValues[i] == DeltaCompressedFrameData.compressionAccuracy)
                            selected = i;
                    }
                    customCompression = EditorGUILayout.Toggle("Custom Compression", customCompression);
                    if (customCompression == false)
                        DeltaCompressedFrameData.compressionAccuracy =
                            optionsValues[EditorGUILayout.Popup("Compression Accuracy", selected, options)];
                    else
                    {
                        DeltaCompressedFrameData.compressionAccuracy = EditorGUILayout.Slider("Compression Accuracy",
                            DeltaCompressedFrameData.compressionAccuracy, 1, 10000);
                        EditorGUILayout.TextArea("Lower values = Less accuracy");
                    }
                    bool useFasterNormalsCalc = smoothMeshAngle == -1;
                    var content = new GUIContent("Fast Normal Calculation",
                        "Slower normal calculation, but better results");
                    useFasterNormalsCalc = EditorGUILayout.Toggle(content, useFasterNormalsCalc);
                    if (useFasterNormalsCalc)
                    {
                        smoothMeshAngle = -1;
                    }
                    else
                    {
                        if (smoothMeshAngle == -1)
                            smoothMeshAngle = 60;
                        smoothMeshAngle = EditorGUILayout.IntField("Smoothing Angle", smoothMeshAngle);
                    }

                    bool usingLOD = lodDistances.Count > 0;
                    usingLOD = EditorGUILayout.Toggle("Use LOD", usingLOD);
                    if (usingLOD == false)
                    {
                        lodDistances.Clear();
                    }
                    else
                    {
                        if (GUILayout.Button("Add LOD Level") || lodDistances.Count == 0)
                        {
                            lodDistances.Add(new KeyValuePair<int, float>(fps, 20));
                        }
                        for (int l = 0; l < lodDistances.Count; l++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("X"))
                                {
                                    lodDistances.RemoveAt(l);
                                    GUILayout.EndHorizontal();
                                    break;
                                }
                                int key = EditorGUILayout.IntField("Playback FPS", lodDistances[l].Key);
                                float value = EditorGUILayout.FloatField("Distance", lodDistances[l].Value);
                                lodDistances[l] = new KeyValuePair<int, float>(key, value);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    globalBake = EditorGUILayout.IntSlider("Global Frame Skip", globalBake, 1, fps);
                    bool bChange = globalBake != previousGlobalBake;
                    previousGlobalBake = globalBake;

                    EditorGUILayout.LabelField("Custom Clips");
                    for (int i = 0; i < customClips.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            customClips[i] = (AnimationClip)EditorGUILayout.ObjectField(customClips[i], typeof(AnimationClip), false);
                            if (GUILayout.Button("X", GUILayout.Width(32)))
                            {
                                customClips.RemoveAt(i);
                                GUILayout.EndHorizontal();
                                break;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("Add Custom Animation Clip"))
                        customClips.Add(null);
                    if (GUILayout.Button("Add Selected Animation Clips"))
                    {
                        foreach (var o in Selection.objects)
                        {
                            string p = AssetDatabase.GetAssetPath(o);
                            if (string.IsNullOrEmpty(p) == false)
                            {
                                AnimationClip[] clipsToAdd = AssetDatabase.LoadAllAssetRepresentationsAtPath(p).Where(q => q is AnimationClip).Cast<AnimationClip>().ToArray();
                                customClips.AddRange(clipsToAdd);
                            }
                        }
                    }

                    var clips = animBaker.GetClips(customClips);
                    Dictionary<string, bool> bakeAnims = animBaker.BakeAnims;
                    string[] clipNames = bakeAnims.Keys.ToArray();

                    bool modified = false;
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Animation Options:", GUILayout.Width(150));

                        if (GUILayout.Button("Select All", GUILayout.Width(100)))
                        {
                            foreach (var clipName in clipNames)
                                bakeAnims[clipName] = true;
                        }
                        if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                        {
                            foreach (var clipName in clipNames)
                                bakeAnims[clipName] = false;
                        }
                    }
                    GUILayout.EndHorizontal();

                    
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Bake Animation");
                        GUILayout.Label("Frame Skip");
                    }
                    GUILayout.EndHorizontal();

                    scrollpos = GUILayout.BeginScrollView(scrollpos, GUILayout.MinHeight(100), GUILayout.MaxHeight(1000));
                    try
                    {

                        EditorGUI.indentLevel++;
                        Dictionary<string, int> frameSkips = animBaker.FrameSkips;
                        foreach (var clipName in clipNames)
                        {
                            if (frameSkips.ContainsKey(clipName) == false)
                                frameSkips.Add(clipName, globalBake);
                            AnimationClip clip = clips.Find(q => q.name == clipName);
                            int framesToBake = clip ? (int) (clip.length*fps/frameSkips[clipName]) : 0;
                            GUILayout.BeginHorizontal();
                            {
                                bakeAnims[clipName] = EditorGUILayout.Toggle(string.Format("{0} ({1} frames)", clipName, framesToBake),
                                                                              bakeAnims[clipName]);
                                GUI.enabled = bakeAnims[clipName];
                                frameSkips[clipName] = Mathf.Clamp(EditorGUILayout.IntField(frameSkips[clipName]), 1,fps);
                                GUI.enabled = true;
                            }
                            GUILayout.EndHorizontal();
                            if (framesToBake > 500)
                            {
                                GUI.skin.label.richText = true;
                                EditorGUILayout.LabelField(
                                    "<color=red>Long animations degrade performance, consider using a higher frame skip value.</color>",
                                    GUI.skin.label);
                            }
                            if (bChange) frameSkips[clipName] = globalBake;
                            if (frameSkips[clipName] != 1)
                                modified = true;
                        }
                        EditorGUI.indentLevel--;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                    GUILayout.EndScrollView();
                    if (modified)
                        DrawText(
                            "Skipping more frames during baking will result in a smaller asset size, but potentially degrade animation quality.",
                            Color.yellow);

                    GUILayout.Space(10);
                    int bakeCount = bakeAnims.Count(q => q.Value);
                    GUI.enabled = bakeCount > 0;
                    if (GUILayout.Button(string.Format("Generate {0} animation{1}", 
                                                        bakeCount,bakeCount > 1 ? "s" : string.Empty)))
                    {
                        animBaker.CreateSnapshots(GetPrefabPath(prefab), customClips, exposedTransforms ,  fps , 
                                                  smoothMeshAngle , lodDistances);
                    }
                    GUI.enabled = true;
                    GUILayout.Space(10);
                    SavePreferencesForAsset();
                }
                else // end if valid prefab
                {
                    DrawText("Specify a asset to bake.", Color.red + Color.white*0.5f);
                }
                EditorGUI.EndChangeCheck();
                if (GUI.changed)
                    Repaint();
            }
            GUILayout.EndScrollView();
        }



        private void DrawText(string text)
        {
            GUILayout.TextArea(text);
        }

        private void DrawText(string text, Color color)
        {
            GUI.contentColor = color;
            GUILayout.TextArea(text);
            GUI.contentColor = oColor;
        }

        private static string GetPrefabPath(GameObject prefab)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Object parentObject = PrefabUtility.GetPrefabParent(prefab);
                assetPath = AssetDatabase.GetAssetPath(parentObject);
            }
            assetPath = assetPath.Replace(modelDir, resDir);
            return assetPath;
        }

        /// <summary>
        /// 批量烘焙采样模型内的数据
        /// </summary>
        private void batchBakes(GameObject[] objs)
        {
            previousPrefab = null;
            foreach (var obj in objs)
            {
                try
                {
                    animBaker.OnPrefabChanged(obj);
                    previousPrefab = prefab;
                    AutoPopulateExposedTransforms();

                    var frameSkips = animBaker.FrameSkips;
                    var toBakeClips = animBaker.GetClips(null);
                    foreach (var clip in toBakeClips)
                    {
                        frameSkips[clip.name] = 1;
                    }
                    animBaker.CreateSnapshots(GetPrefabPath(obj) , null , exposedTransforms);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }


        /// <summary>
        /// 批量烘焙采样模型内的数据
        /// </summary>
        public static void BatchBakes(string[] assets)
        {
            MeshAnimationBake animBaker = new MeshAnimationBake();
            for (int i = 0; i < assets.Length; i++)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assets[i]);
                try
                {
                    animBaker.OnPrefabChanged(obj);
                    List<string> exposedTransforms = AutoPopulateExposedTransforms(animBaker , obj);
                    var frameSkips = animBaker.FrameSkips;
                    var toBakeClips = animBaker.GetClips(null);
                    foreach (var clip in toBakeClips)
                    {
                        frameSkips[clip.name] = 1;
                    }
                    animBaker.CreateSnapshots(GetPrefabPath(obj), null, exposedTransforms);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        public static void Bake(GameObject obj)
        {
            MeshAnimationBake animBaker = new MeshAnimationBake();
            try
            {
                animBaker.OnPrefabChanged(obj);
                List<string> exposedTransforms = AutoPopulateExposedTransforms(animBaker, obj);
                var frameSkips = animBaker.FrameSkips;
                var toBakeClips = animBaker.GetClips(null);
                foreach (var clip in toBakeClips)
                {
                    frameSkips[clip.name] = 1;
                }
                animBaker.CreateSnapshots(GetPrefabPath(obj), null, exposedTransforms);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static string[] GetExtraExposedTranforms(List<ModelImporter> importers)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                if (importer.optimizeGameObjects)
                {
                    var paths = importer.extraExposedTransformPaths;
                    for (int j = 0; j < paths.Length; j++)
                    {
                        var split = paths[j].Split('/');
                        paths[j] = split[split.Length - 1];
                    }

                    output.AddRange(paths);
                }
            }
            return output.Distinct().ToArray();
        }

        private string[] GetAllTranforms()
        {
            if (prefab == null)
                return new string[0];
            var importers = GetAllImporters(animBaker , prefab);
            List<string> output = new List<string>();
            for (int i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                output.AddRange(importer.transformPaths);
            }
            return output.Distinct().ToArray();
        }

        /// <summary>
        /// 获得所有绑点结点
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllJointTransforms(List<ModelImporter> importers)
        {
            List<string> output = new List<string>();
            string joint = "exp_";
            for (int i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                var paths = importer.optimizeGameObjects ? importer.extraExposedTransformPaths : importer.transformPaths;
                if (importer.optimizeGameObjects)
                {
                    for (int j = 0; j < paths.Length; j++)
                    {
                        var split = paths[j].Split('/');
                        paths[j] = split[split.Length - 1];
                    }
                }

                for (int j = 0; j < paths.Length; j++)
                {
                    var split = paths[j].Split('/');
                    string transName = split[split.Length - 1];
                    if (transName.StartsWith(joint))
                        output.Add(paths[j]);
                }
            }
            return output.Distinct().ToArray();
        }

        private static void CleanExposedTransforms(List<string> exposedTransforms , List<ModelImporter> importers)
        {
            if (IsOptimizedAnimator(importers))
            {
                for (int j = 0; j < exposedTransforms.Count; j++)
                {
                    var split = exposedTransforms[j].Split('/');
                    exposedTransforms[j] = split[split.Length - 1];
                }
            }
        }

        private static bool IsOptimizedAnimator(List<ModelImporter> importers)
        {
            if (importers.Count > 0)
                return importers.Any(q => q.optimizeGameObjects);
            return false;
        }

        private static ModelImporter GetImporter(string p)
        {
            return ModelImporter.GetAtPath(p) as ModelImporter;
        }

        private static List<ModelImporter> GetAllImporters(MeshAnimationBake animBaker , GameObject prefab)
        {
            List<ModelImporter> importers = new List<ModelImporter>();
            importers.Add(GetImporter(GetPrefabPath(prefab)));
            foreach (var sr in animBaker.skinnedRenderers)
            {
                if (sr && sr.sharedMesh)
                {
                    importers.Add(GetImporter(AssetDatabase.GetAssetPath(sr.sharedMesh)));
                }
            }
            importers.RemoveAll(q => q == null);
            importers = importers.Distinct().ToList();
            return importers;
        }

        private void AutoPopulateExposedTransforms()
        {
            exposedTransforms.Clear();

            List<ModelImporter> importers = GetAllImporters(animBaker, prefab);
            string[] extraTrans = GetExtraExposedTranforms(importers);
            string[] allTrans = GetAllJointTransforms(importers);

            string[] tempArr = new string[extraTrans.Length + allTrans.Length];
            Array.Copy(extraTrans , tempArr , extraTrans.Length);
            Array.Copy(allTrans , 0 , tempArr , extraTrans.Length , allTrans.Length);

            for (int i = 0; i < tempArr.Length; i++)
            {
                var split = tempArr[i].Split('/');
                string transName = split[split.Length - 1];
                if(exposedTransforms.Contains(transName))
                    exposedTransforms.Add(tempArr[i]);
            }
            CleanExposedTransforms(exposedTransforms , importers);
        }


        private static List<string> AutoPopulateExposedTransforms(MeshAnimationBake animBaker , GameObject prefab)
        {
            List<string> exposedTransforms = new List<string>();
            List<ModelImporter> importers = GetAllImporters(animBaker, prefab);
            string[] extraTrans = GetExtraExposedTranforms(importers);
            string[] allTrans = GetAllJointTransforms(importers);

            string[] tempArr = new string[extraTrans.Length + allTrans.Length];
            Array.Copy(extraTrans, tempArr, extraTrans.Length);
            Array.Copy(allTrans, 0, tempArr, extraTrans.Length, allTrans.Length);

            for (int i = 0; i < tempArr.Length; i++)
            {
                var split = tempArr[i].Split('/');
                string transName = split[split.Length - 1];
                if (exposedTransforms.Contains(transName))
                    exposedTransforms.Add(tempArr[i]);
            }
            CleanExposedTransforms(exposedTransforms, importers);
            return exposedTransforms;
        }

        private void LoadPreferencesForAsset()
        {
            try
            {
                string path = GetPrefabPath(prefab);
                if (string.IsNullOrEmpty(path))
                    return;
                string guid = AssetDatabase.AssetPathToGUID(path);
                string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
                prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
                MeshBakePreferences bakePrefs = null;
                using (FileStream fs = new FileStream(prefsPath, FileMode.Open))
                {
                    BinaryFormatter br = new BinaryFormatter();
                    bakePrefs = (MeshBakePreferences)br.Deserialize(fs);
                }
                customClips.AddRange(
                    bakePrefs.customClips.Select(
                        q =>AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(q))));
                customClips = customClips.Distinct().ToList();
                customCompression = bakePrefs.customCompression;
                exposedTransforms.AddRange(bakePrefs.exposedTransforms);
                exposedTransforms = exposedTransforms.Distinct().ToList();
                fps = bakePrefs.fps;
                globalBake = bakePrefs.globalBake;
                previousGlobalBake = bakePrefs.previousGlobalBake;
                smoothMeshAngle = bakePrefs.smoothMeshAngle;
                for (int i = 0; i < bakePrefs.lodDistanceKeys.Length; i++)
                {
                    lodDistances.Add(new KeyValuePair<int, float>(bakePrefs.lodDistanceKeys[i],
                        bakePrefs.lodDistanceValues[i]));
                }
            }
            catch
            {
            }
        }

        private void SavePreferencesForAsset()
        {
            try
            {
                string path = GetPrefabPath(prefab);
                if (string.IsNullOrEmpty(path))
                    return;
                string guid = AssetDatabase.AssetPathToGUID(path);
                MeshBakePreferences preferences = new MeshBakePreferences();
                preferences.customClips =
                    customClips.Select(q => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(q))).ToArray();
                preferences.customCompression = customCompression;
                preferences.exposedTransforms = exposedTransforms.ToArray();
                preferences.fps = fps;
                preferences.globalBake = globalBake;
                preferences.previousGlobalBake = previousGlobalBake;
                preferences.smoothMeshAngle = smoothMeshAngle;
                preferences.lodDistanceKeys = new int[lodDistances.Count];
                preferences.lodDistanceValues = new float[lodDistances.Count];
                for (int i = 0; i < lodDistances.Count; i++)
                {
                    preferences.lodDistanceKeys[i] = lodDistances[i].Key;
                    preferences.lodDistanceValues[i] = lodDistances[i].Value;
                }
                string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
                prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
                // save prefs
                using (FileStream fs = new FileStream(prefsPath, FileMode.OpenOrCreate))
                {
                    BinaryFormatter br = new BinaryFormatter();
                    br.Serialize(fs, preferences);
                }
            }
            catch
            {
            }
        }
    }
}