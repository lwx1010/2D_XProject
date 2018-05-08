using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetChecker
{
    public class TextureOverviewPanel : IEditorPanel
    {

        private int mTab;
        private Vector2 scrollPos;
        private List<TextureBean> assetList;

        private TextureSettingView ruleView = new TextureSettingView();
        private Dictionary<string, TextureSettingBean> settingMap = new Dictionary<string, TextureSettingBean>();

        private int selectAssetType;
        private string[] AssetAllTypes; 
        private int sortName = 1, sortWidth = 1, sortHeight = 1, sortMaxSize = 1, sortMemorySize = 1;

        public void Initizalize()
        {
            if (assetList != null) return;

            OverviewSetting.Instance.ReadTextureSettings();
            findAllTextures();
        }
        /// <summary>
        /// 查找模型文件
        /// </summary>
        private void findAllTextures()
        {
            scrollPos = Vector2.zero;
            List<TextureSettingBean> settings = OverviewSetting.Instance.TextureSettings;

            List<string> files = new List<string>();
            Dictionary<TextureSettingBean, string[]> fileMaps = new Dictionary<TextureSettingBean, string[]>();

            List<string> assetTypeList = new List<string>();
            assetTypeList.Add("ALL");

            float fileCount = 0;
            if (settings != null)
            {
                settingMap.Clear();
                for (int i = 0; i < settings.Count; i++)
                {
                    settingMap[settings[i].AssetDesc] = settings[i];
                    files.Clear();
                    for (int j = 0; j < settings[i].Folder.Count; j++)
                    {
                        string rootFolder = settings[i].Folder[j];
                        if (string.IsNullOrEmpty(rootFolder)) continue;
                        string[] fileArr = Directory.GetFiles(rootFolder, "*.jpg", SearchOption.AllDirectories);
                        files.AddRange(fileArr);
                        fileCount += fileArr.Length;

                        fileArr = Directory.GetFiles(rootFolder, "*.png", SearchOption.AllDirectories);
                        files.AddRange(fileArr);
                        fileCount += fileArr.Length;
                    }
                    fileMaps[settings[i]] = files.ToArray();
                    assetTypeList.Add(settings[i].AssetDesc);
                }
            }

            AssetAllTypes = assetTypeList.ToArray();
            assetList = new List<TextureBean>();
            int curFileIndex = 0;
            foreach (TextureSettingBean msb in fileMaps.Keys)
            {
                string[] childFiles = fileMaps[msb];
                for (int i = 0; i < childFiles.Length; i++)
                {
                    curFileIndex++;
                    EditorUtility.DisplayProgressBar("分析中", "正在分析贴图数据...", curFileIndex / fileCount);
                    TextureBean asset = parseEffectAsset(childFiles[i]);
                    asset.AssetDesc = msb.AssetDesc;
                    assetList.Add(asset);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 分析粒子特效
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private TextureBean parseEffectAsset(string filePath)
        {
            TextureBean eb = new TextureBean();
            eb.Name = Path.GetFileName(filePath);
            eb.FilePath = filePath;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            eb.Width = tex.width;
            eb.Height = tex.height;
            eb.Format = tex.format.ToString();
            
            TextureImporter texImp = AssetImporter.GetAtPath(filePath) as TextureImporter;
            eb.MipMaps = texImp.mipmapEnabled;
            eb.MaxSize = texImp.maxTextureSize;
            eb.MemorySize = CalculateTextureSizeBytes(tex) / 1024.0f;
            eb.MemoryText = eb.MemorySize >= 1024 ? string.Format("{0:F}MB", eb.MemorySize/1024)
                            : string.Format("{0:F}KB", eb.MemorySize);
            return eb;
        }

        int GetBitsPerPixel(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8: //	 Alpha-only texture format.
                    return 8;
                case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                    return 16;
                case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
                    return 16;
                case TextureFormat.RGB24:   // A color texture format.
                    return 24;
                case TextureFormat.RGBA32:  //Color with an alpha channel texture format.
                    return 32;
                case TextureFormat.ARGB32:  //Color with an alpha channel texture format.
                    return 32;
                case TextureFormat.RGB565:  //	 A 16 bit color texture format.
                    return 16;
                case TextureFormat.DXT1:    // Compressed color texture format.
                    return 4;
                case TextureFormat.DXT5:    // Compressed color with alpha channel texture format.
                    return 8;
                /*
                case TextureFormat.WiiI4:	// Wii texture format.
                case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
                case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
                case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
                case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
                case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
                case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
                case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
                    return 0;  //Not supported yet
                */
                case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
                    return 2;
                case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                    return 2;
                case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
                    return 4;
                case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                    return 4;
                case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                    return 4;
                case TextureFormat.ATC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
                    return 4;
                case TextureFormat.ATC_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                    return 8;
                case TextureFormat.BGRA32://	 Format returned by iPhone camera
                    return 32;
#if !UNITY_5
			case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
			case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
			case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
			return 0; //Not supported yet  
#endif
            }
            return 0;
        }

        int CalculateTextureSizeBytes(Texture tTexture)
        {

            int tWidth = tTexture.width;
            int tHeight = tTexture.height;
            if (tTexture is Texture2D)
            {
                Texture2D tTex2D = tTexture as Texture2D;
                int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                int mipMapCount = tTex2D.mipmapCount;
                int mipLevel = 1;
                int tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = tWidth / 2;
                    tHeight = tHeight / 2;
                    mipLevel++;
                }
                return tSize;
            }
#if UNITY_5_4
            if (tTexture is Texture2DArray)
            {
                Texture2DArray tTex2D = tTexture as Texture2DArray;
                int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                int mipMapCount = 10;
                int mipLevel = 1;
                int tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = tWidth / 2;
                    tHeight = tHeight / 2;
                    mipLevel++;
                }
                return tSize * ((Texture2DArray)tTex2D).depth;
            }
#endif
            if (tTexture is Cubemap)
            {
                Cubemap tCubemap = tTexture as Cubemap;
                int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
                return tWidth * tHeight * 6 * bitsPerPixel / 8;
            }
            return 0;
        }

        public void OnGUI()
        {
            int newTab = mTab;
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width * 0.25f);
            if (GUILayout.Toggle(newTab == 0, "总 览", "ButtonLeft")) newTab = 0;
            if (GUILayout.Toggle(newTab == 1, "设 置", "ButtonRight")) newTab = 1;
            GUILayout.Space(Screen.width * 0.25f);
            GUILayout.EndHorizontal();

            mTab = newTab;
            if (newTab == 0)
                drawOverviewGUI();
            else if (newTab == 1)
                ruleView.OnGUI();
        }

        public void drawOverviewGUI()
        {
            NGUIEditorTools.DrawHeader("文件详情");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("筛选：");
                selectAssetType = EditorGUILayout.Popup(selectAssetType, AssetAllTypes);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("关闭MipMaps" , GUILayout.Width(100F)))
                {
                    List<TextureBean> _textures = new List<TextureBean>();
                    for (int i = 0; i < assetList.Count; i++)
                    {
                        if (selectAssetType != 0 && !assetList[i].AssetDesc.Equals(AssetAllTypes[selectAssetType])) continue;
                        if (!assetList[i].MipMaps) continue;
                        _textures.Add(assetList[i]);
                    }
                    
                    for (int i = 0; i < _textures.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Hold On" ,"正在自动处理..." , i / (float)_textures.Count);
                        TextureImporter texImp = AssetImporter.GetAtPath(_textures[i].FilePath) as TextureImporter;
                        texImp.mipmapEnabled = false;
                        _textures[i].MipMaps = false;
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                if (GUILayout.Button("刷新", GUILayout.Width(80)))
                {
                    findAllTextures();
                }
                GUILayout.Space(10);
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            if (GUILayout.Toggle(false, "名称", "ButtonLeft", GUILayout.MaxWidth(200f)))
            {
                sortName *= -1;
                assetList.Sort((x, y) => x.Name.CompareTo(y.Name) * sortName);
            }
            GUILayout.Toggle(false, "资源类型", "ButtonMid", GUILayout.MinWidth(100F));
            if (GUILayout.Toggle(false, "宽度", "ButtonMid", GUILayout.MinWidth(80f)))
            {
                sortWidth *= -1;
                assetList.Sort((x, y) => x.Width.CompareTo(y.Width) * sortWidth);
            }
            if (GUILayout.Toggle(false, "高度", "ButtonMid", GUILayout.MinWidth(80f)))
            {
                sortHeight *= -1;
                assetList.Sort((x, y) => x.Height.CompareTo(y.Height) * sortHeight);
            }
            GUILayout.Toggle(false, "MipMaps", "ButtonMid", GUILayout.MinWidth(100f));
            if (GUILayout.Toggle(false, "MaxSize", "ButtonMid", GUILayout.MinWidth(80F)))
            {
                sortMaxSize *= -1;
                assetList.Sort((x, y) => x.MaxSize.CompareTo(y.MaxSize) * sortMaxSize);
            }
            GUILayout.Toggle(false, "压缩格式", "ButtonMid", GUILayout.MinWidth(100F));
            if (GUILayout.Toggle(false, "内存消耗", "ButtonRight", GUILayout.MinWidth(100F)))
            {
                sortMemorySize *= -1;
                assetList.Sort((x, y) => x.MemorySize.CompareTo(y.MemorySize) * sortMemorySize);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int index = 0;
            for (int i = 0; i < assetList.Count; i++)
            {
                if (selectAssetType != 0 && !assetList[i].AssetDesc.Equals(AssetAllTypes[selectAssetType])) continue;

                GUI.backgroundColor = index % 2 == 1 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                drawRow(index , assetList[i]);

                GUILayout.EndHorizontal();
                index++;
            }
            GUILayout.EndScrollView();
        }


        private void drawRow(int index , TextureBean asset)
        {
            if (asset.FilePath == GUISetting.SelectFilePath)
                GUI.color = GUISetting.SelectColor;

            if(GUILayout.Button(string.Format("{0}. {1}", index + 1, asset.Name), "OL TextField", GUILayout.MaxWidth(200f)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(asset.FilePath);
                GUISetting.SelectFilePath = asset.FilePath;
            }

            GUILayout.Space(50);
            GUILayout.Label(asset.AssetDesc, GUILayout.MinWidth(100f));

            GUILayout.Label(asset.Width.ToString(), GUILayout.MinWidth(80F));
            
            GUILayout.Label(asset.Height.ToString(), GUILayout.MinWidth(80f));

            GUI.color = asset.MipMaps != settingMap[asset.AssetDesc].MipMaps ? Color.red : Color.white;
            GUILayout.Label(asset.MipMaps.ToString(), GUILayout.MinWidth(100F));
            GUI.color = Color.white;

            GUILayout.Label(asset.MaxSize.ToString(), GUILayout.MinWidth(80F));
            GUILayout.Label(asset.Format , GUILayout.MinWidth(100F));
            
            GUILayout.Label(asset.MemoryText, GUILayout.MinWidth(100F));
            GUI.color = Color.white;
        }

        public void OnDestroy()
        {

        }
#region ------------------模块数量信息-------------------------

        private class TextureBean
        {
            public string Name;
            public string FilePath;
            public string AssetDesc;

            public int Width;
            public int Height;

            public bool MipMaps;
            public int MaxSize;
            public string Format;

            public float MemorySize;
            public string MemoryText;
        }

#endregion
    }
}