using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class NGUIMenuExtension
{
    [MenuItem("NGUI/Open/Extension/图集精灵查找", false, 1)]
    [MenuItem("Assets/NGUI/Extension/图集精灵查找", false, 1)]
    static public void OpenTextureFinder()
    {
        EditorWindow.GetWindow<UISpriteFinder>(false, "精灵查找", true).Show();
    }

    [MenuItem("NGUI/Open/Extension/图集精灵重设大小", false, 1)]
    [MenuItem("Assets/NGUI/Extension/图集精灵重设大小", false, 1)]
    static public void OpenTextureResize()
    {
        EditorWindow.GetWindow<UISpriteResize>(false, "精灵Resize", true).Show();
    }

    [MenuItem("NGUI/Open/Extension/图集合并", false, 1)]
    [MenuItem("Assets/NGUI/Extension/图集合并", false, 1)]
    static public void OpenAtlasMerger()
    {
        EditorWindow.GetWindow<UIAtlasMerger>(false, "图集合并", true).Show();
    }

    [MenuItem("NGUI/Open/Extension/替换指定图片图集", false, 1)]
    [MenuItem("Assets/NGUI/Extension/替换指定图片图集", false, 1)]
    static public void OpenAtlasReplacer()
    {
        EditorWindow.GetWindow<UIPrefabAtlasReplace>(false, "图集替换", true).Show();
    }

    [MenuItem("NGUI/Open/Extension/更新选中图集", false, 1)]
    [MenuItem("Assets/NGUI/Extension/更新选中图集", false, 1)]
    static public void ModifySelectedAtlas()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogError("请先选中需要更新的图集");
            return;
        }
        var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        var obj = AssetDatabase.LoadAssetAtPath<UIAtlas>(path);
        if (obj != null)
            ModifyAtlas(obj);
        else
            Debug.LogError("请先选中一个图集!!");
    }

    static List<UIAtlas> tempAtlas = new List<UIAtlas>();

    [MenuItem("NGUI/Open/Extension/一键更新所有图集", false, 1)]
    [MenuItem("Assets/NGUI/Extension/一键更新所有图集", false, 1)]
    static public void ModifyAllAtlas()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogError("请先选中需要更新的图集");
            return;
        }
        string[] files = Directory.GetFiles("Assets/Res/Atlas/", "*.prefab", SearchOption.AllDirectories);
        tempAtlas.Clear();
        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i].Replace('\\', '/');
            var obj = AssetDatabase.LoadAssetAtPath<UIAtlas>(file);
            if (obj != null)
            {
                if (tempAtlas.Count > 0) tempAtlas.RemoveAt(0);
                tempAtlas.Add(obj);
                ModifyAtlas(tempAtlas[0]);
            }
        }
    }

    [MenuItem("NGUI/Open/Extension/释放选中图集图片", false, 1)]
    [MenuItem("Assets/NGUI/Extension/释放选中图集图片", false, 1)]
    static public void ExtractAllSpritesFromSelectedAtlas()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogError("请先选中需要更新的图集");
            return;
        }
        var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        var obj = AssetDatabase.LoadAssetAtPath<UIAtlas>(path);
        if (obj != null)
        {
            var saveFolder = EditorUtility.SaveFolderPanel("保存释放图片目录", "Assets/Textures", obj.name);
            ExtractAllSpritesFromAtlas(obj, saveFolder);
        }
        else
            Debug.LogError("请先选中一个图集!!");
    }

    [MenuItem("NGUI/Open/Extension/一键释放所有图集图片", false, 1)]
    [MenuItem("Assets/NGUI/Extension/一键释放所有图集图片", false, 1)]
    static public void ExtractAllSpritesFromAllAtlas()
    {
        string[] files = Directory.GetFiles("Assets/Res/Atlas/", "*.prefab", SearchOption.AllDirectories);
        tempAtlas.Clear();
        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i].Replace('\\', '/');
            var obj = AssetDatabase.LoadAssetAtPath<UIAtlas>(file);
            if (obj != null)
            {
                var saveFolder = Application.dataPath + "/Textures/" + obj.name;
                if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
                if (tempAtlas.Count > 0) tempAtlas.RemoveAt(0);
                tempAtlas.Add(obj);
                ExtractAllSpritesFromAtlas(tempAtlas[0], saveFolder);
            }
        }
    }

    struct atlasSt
    {
        public string name;
        public int width;
        public int height;
    }

    [MenuItem("NGUI/Open/Extension/检查图集图片重复使用", false, 1)]
    [MenuItem("Assets/NGUI/Extension/检查图集图片重复使用", false, 1)]
    static public void CheckRecursiveSprites()
    {
        string[] allAtlas = Directory.GetFiles("Assets/Res/Atlas", "*.prefab", SearchOption.AllDirectories);
        Dictionary<string, atlasSt> checkDict = new Dictionary<string, atlasSt>();
        for (int i = 0; i < allAtlas.Length; ++i)
        {
            var relativePath = ABPackHelper.GetRelativeAssetsPath(allAtlas[i]);
            var atlas = AssetDatabase.LoadAssetAtPath<UIAtlas>(relativePath);
            if (atlas == null) continue;
            EditorUtility.DisplayProgressBar("检查图集: " + atlas.name, "check: " + relativePath, (float)i / (float)allAtlas.Length);
            for (int j = 0; j < atlas.spriteList.Count; ++j)
            {
                var spriteData = atlas.spriteList[j];
                if (!checkDict.ContainsKey(spriteData.name))
                {
                    atlasSt temp = new atlasSt();
                    temp.name = atlas.name;
                    temp.width = spriteData.width;
                    temp.height = spriteData.height;
                    checkDict.Add(spriteData.name, temp);
                }
                else
                {
                    if (checkDict[spriteData.name].width == spriteData.width
                        && checkDict[spriteData.name].height == spriteData.height)
                    {
                        Debug.LogError("<color=yellow>发现重复sprites: " + spriteData.name +
                        ", 重复图集: " + atlas.name + ", " + checkDict[spriteData.name].name + "</color>");
                    }
                }
            }
        }
        EditorUtility.DisplayProgressBar("检查图集", "Check finished", 1);
        EditorUtility.ClearProgressBar();
    }

    public struct AtlasData
    {
        public Texture tex;
        public UISpriteData data;
    }

    static void ModifyAtlas(UIAtlas atlas)
    {
        string[] allTextures = Directory.GetFiles("Assets/Textures/", "*.*", SearchOption.AllDirectories);
        Dictionary<string, AtlasData> textures = new Dictionary<string, AtlasData>();
        for (int i =  0; i < allTextures.Length; ++i)
        {
            var file = allTextures[i].Replace('\\', '/');
            EditorUtility.DisplayProgressBar("更新图集: " + atlas.name, file, (float)i / (float)allTextures.Length);
            for (int j = 0; j < atlas.spriteList.Count; ++j)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(atlas.spriteList[j].name))
                {
                    Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(file);
                    if (tex != null && !textures.ContainsKey(tex.name))
                    {
                        AtlasData data = new AtlasData();
                        data.tex = tex;
                        data.data = new UISpriteData();
                        data.data.CopyFrom(atlas.spriteList[j]);
                        textures.Add(tex.name, data);
                    }
                }
            }
        }
        if (atlas.spriteList.Count != textures.Values.Count)
        {
            for (int i = 0; i < atlas.spriteList.Count; ++i)
            {
                if (!textures.ContainsKey(atlas.spriteList[i].name))
                    Debug.LogWarning("找不到sprite本地文件路径: " + atlas.spriteList[i].name + ", 图集名称: " + atlas.name);
            }
            return;
        }
        var texList = new List<Texture>();
        foreach (var val in textures.Values)
        {
            texList.Add(val.tex);
        }
        var sprites = UIAtlasMaker.CreateSprites(texList);

        if (sprites.Count > 0 && atlas != null)
        {
            if (UIAtlasMaker.UpdateTexture(atlas, sprites))
            {
                UIAtlasMaker.ReplaceSprites(atlas, sprites);
            }

            ModifyAtlasSpritesPadding(atlas, textures);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log("更新完成!! " + atlas.name);
    }

    static public void ModifyAtlasSpritesPadding(UIAtlas atlas, Dictionary<string, AtlasData> textures)
    {
        for (int i = 0; i < atlas.spriteList.Count; ++i)
        {
            foreach (var val in textures.Values)
            {
                if (atlas.spriteList[i].name.Equals(val.data.name) && (val.data.borderLeft != 0 || val.data.paddingLeft != 0))
                {
                    atlas.spriteList[i].SetBorder(val.data.borderLeft, val.data.borderBottom, val.data.borderRight, val.data.borderTop);
                    atlas.spriteList[i].SetPadding(val.data.paddingLeft, val.data.paddingBottom, val.data.paddingRight, val.data.paddingTop);
                }
            }
        }
    }

    static void ExtractAllSpritesFromAtlas(UIAtlas atlas, string saveFolder)
    {
        if (!string.IsNullOrEmpty(saveFolder))
        {
            for (int i = 0; i < atlas.spriteList.Count; ++i)
            {
                var saveName = atlas.spriteList[i].name;
                string path = saveFolder + "/" + saveName + ".png";
                var relativePath = ABPackHelper.GetRelativeAssetsPath(path);
                EditorUtility.DisplayProgressBar("Extract " + atlas.name + "'s sprites", "Extract " + relativePath + "...", (float)i / (float)atlas.spriteList.Count);
                UIAtlasMaker.SpriteEntry se = UIAtlasMaker.ExtractSprite(atlas, saveName);
                if (se != null)
                {
                    byte[] bytes = se.tex.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.ImportAsset(relativePath);
                    if (se.temporaryTexture) GameObject.DestroyImmediate(se.tex);
                }
            }
            EditorUtility.DisplayProgressBar("Extract sprites", "Extract finished", 1);
            EditorUtility.ClearProgressBar();
        }
    }
}
