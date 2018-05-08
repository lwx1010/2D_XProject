using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

public class AtMeToolEditor
{
    [MenuItem("Tools/UI/生成图集Atlas", false, 2)]
    static public void BuildAtlas()
    {
        Debug.Log("Building Atlas...");
        string textureDir = Application.dataPath + "/Textures/UI";
        DirectoryInfo textureDirInfo = new DirectoryInfo(textureDir);
        foreach (DirectoryInfo dirInfo in textureDirInfo.GetDirectories())
        {
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                string packTagName = subDirInfo.FullName.Substring(dirInfo.FullName.Length + 1);
                string[] patterns = { "*.jpg", "*.png" };
                foreach (string pattern in patterns)
                {
                    foreach (FileInfo pngFile in subDirInfo.GetFiles(pattern, SearchOption.AllDirectories))
                    {
                        string allPath = pngFile.FullName;
                        string assetPath = allPath.Substring(allPath.IndexOf("Assets"));

                        TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                        textureImporter.textureType = TextureImporterType.Default;
                        textureImporter.maxTextureSize = 1024;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.filterMode = FilterMode.Bilinear;
                        textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                        textureImporter.spritePackingTag = packTagName;
                        AssetDatabase.ImportAsset(assetPath);
                        
                    }
                }
                Debug.Log(string.Format("Create Altas: {0}", packTagName));
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/UI/切割图片(One texture to mult sprite)", false, 3)]
    static public void ToMultSprite() {
        Debug.Log("Generating ToMultSprite...");
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/Textures/UI/ToMultSprite");
        if (dirInfo != null) {
            string[] patterns = { "*.jpg", "*.png" };

            foreach (string pattern in patterns) {
                foreach (FileInfo picture in dirInfo.GetFiles(pattern)) {
                    string allPath = picture.FullName;
                    Debug.Log(allPath);
                    string rootPath = "Assets/Textures/UI/ToMultSprite";
                    string path = rootPath + "/" + picture.Name;//图片路径名称
                    Debug.Log(path);
                    Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);//获取旋转的对象
                    
                    TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (Directory.Exists(rootPath + "/" + image.name)) { //删除文件夹
                        Directory.Delete(rootPath + "/" + image.name, true);
                    }
                    Directory.CreateDirectory(rootPath + "/" + image.name);

                    foreach (SpriteMetaData metaData in texImp.spritesheet) {//遍历小图集
                        Texture2D myimage = new Texture2D((int)metaData.rect.width, (int)metaData.rect.height);

                        for (int y = (int)metaData.rect.y; y < metaData.rect.y + metaData.rect.height; y++) {//Y轴像素
                            for (int x = (int)metaData.rect.x; x < metaData.rect.x + metaData.rect.width; x++)
                                myimage.SetPixel(x - (int)metaData.rect.x, y - (int)metaData.rect.y, image.GetPixel(x, y));
                        }

                        //转换纹理到EncodeToPNG兼容格式
                        if (myimage.format != TextureFormat.ARGB32 && myimage.format != TextureFormat.RGB24) {
                            Texture2D newTexture = new Texture2D(myimage.width, myimage.height);
                            newTexture.SetPixels(myimage.GetPixels(0), 0);
                            myimage = newTexture;
                        }
                        var pngData = myimage.EncodeToPNG();
                        File.WriteAllBytes(rootPath + "/" + image.name + "/" + metaData.name + ".png", pngData);
                    }
                }
            }
        }
    }

    [MenuItem("Tools/场景输出工具", false, 6)]
    static void SceneOutputTools()
    {
        GameObject go = new GameObject("Scene Output Tool");
        go.AddComponent<SceneViewCustom>();
        Selection.activeObject = go;
    }

    [MenuItem("ArtTools/Scene/Add Box", false, 4)]
    static public void addboxcolider()
    {
        GameObject go = (GameObject)Selection.activeObject;
        GameObject[] objs = GetAllChildGameObject(go.transform);
        foreach (var obj in objs)
        {
            if (obj.tag.Equals("Terrain"))
            {
                obj.transform.GetOrAddComponent<BoxCollider>();
            }
        }
        AssetDatabase.SaveAssets();
    }

    static GameObject[] GetAllChildGameObject(Transform root)
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (var trans in root.GetAllChildren())
        {
            objs.Add(trans.gameObject);
            if (trans.childCount > 0)
            {
                objs.AddRange(GetAllChildGameObject(trans));
            }
        }
        return objs.ToArray();
    }

    [MenuItem("Tools/UI/设置uishader")]
    static void SetShader()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/Effect/Prefab/UI");
        foreach (var file in dirInfo.GetFiles("*.prefab", SearchOption.TopDirectoryOnly))
        {
            string assetPath = file.FullName.Substring(file.FullName.IndexOf("Assets")).Replace('\\', '/');
            GameObject go = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            Renderer[] renders = GetRender(go.transform);
            foreach (var render in renders)
            {
                string name = render.sharedMaterial.shader.name;
                if (name.Contains("1") || (!name.Contains("Effect") && !name.Contains("FXMaker")))
                    continue;
                string newName = name + "1";
                if (name.Contains("FXMaker"))
                    newName = name + " 1";
                render.sharedMaterial.shader = Shader.Find(newName);
            }
            AssetDatabase.SaveAssets();
        }
        AssetDatabase.Refresh();
    }

    #region -------游戏Layer自动检测-----------

    private static string[] layers = new[]
    {
        "SceneEntity", "Ground", "TransparentBuilding", "UIModel", "Plot", "Preloading",
        "Self" , "Role" , "Monster" , "Rune" , "Selectable","Jump" , "Partner" , "Npc" , 
        "RoleEffect" , "Globale"
    };

    /// <summary>
    /// 自动生成Layer
    /// </summary>
    [MenuItem("Tools/Add Tags+Layers ")]
    public static void AutoGenLayer()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        for (int j = 0; j < layers.Length; j++)
        {
            for (int i = 8 + j; i < layersProp.arraySize; i++)
            {
                string layerName = layersProp.GetArrayElementAtIndex(i).stringValue;
                if (layerName != layers[j])
                {
                    layersProp.GetArrayElementAtIndex(i).stringValue = layers[j];
                    break;
                }
            }
        }
        tagManager.ApplyModifiedProperties();
        
    }
    #endregion

    static Renderer[] GetRender(Transform root)
    {
        List<Renderer> renders = new List<Renderer>();
        foreach (var trans in root.GetAllChildren())
        {
            Renderer render = trans.GetComponent<Renderer>();
            if (render != null)
                renders.Add(render);
            if (trans.childCount > 0)
            {
                Renderer[] temps = GetRender(trans);
                if (temps.Length > 0)
                    renders.AddRange(temps);
            }
        }
        return renders.ToArray();
    }
}

