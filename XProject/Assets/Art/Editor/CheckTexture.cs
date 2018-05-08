using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CheckTexture
{
    public static void CheckReloadTex(UnityEngine.Object obj)
    {
        int none = 0;
        string path = AssetDatabase.GetAssetPath(obj);
        List<string> uiTexture = new List<string>();
        string texPath = path.Substring(0, path.LastIndexOf('/'));
        string imageType = "*.png.meta|*.jpg.meta";
        string[] imagetype = imageType.Split('|');
        string[] imageLines;
        //string normalTexturesPath = "Assets/Models/Npc";
        GetTexture(texPath, imagetype, uiTexture);
        for (int j = 0; j < uiTexture.Count; ++j)
        {
            imageLines = File.ReadAllLines(uiTexture[j]);
            for (int i = 0; i < imageLines.Length; ++i)
            {
                if(imageLines[i].IndexOf("buildTarget: Android") > 1)
                {
                    none = none + 1;
                }                
            }
            if(none == 0)
            {
                string imagePath = uiTexture[j].Substring(0, uiTexture[j].IndexOf(".meta"));
                Debug.Log("重新导入" + imagePath);
                AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.Default);
            }
            none = 0;
        }

    }
    
    static void GetTexture(string path, string[] imagetype, List<string> uiTexture)
    {
        for (int i = 0; i < imagetype.Length; ++i)
        {
            Debug.Log("类型：" + imagetype[i]);
            string[] image = Directory.GetFiles(path, imagetype[i], SearchOption.AllDirectories);
            Debug.Log("数量：" + image.Length);
            for (int j = 0; j < image.Length; ++j)
            {
                uiTexture.Add(image[j]);
            }
        }
    }

}


