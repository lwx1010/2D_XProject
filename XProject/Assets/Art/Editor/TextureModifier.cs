#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

class TextureModifier : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (assetImporter as TextureImporter);
        if (assetPath.Contains("Atlas/"))
        {
            DealUITextures(importer);
        }
        else if (assetPath.Contains("Models/") || assetPath.Contains("Effect/"))
        {
            if (assetPath.EndsWith(".dds"))
            {
                Debug.LogError("发现dds文件 " + assetPath);
                return;
            }
            if (assetPath.Contains("T4MOBJ"))
            {
                DealT4MTextures(importer);
            }else if (assetPath.Contains("RoleModels/Players") || assetPath.Contains("RoleModels/Weapons"))
            {
                DealPlayerTextures(importer);
            }else if (assetPath.ToLower().Contains("normal"))
            {
                DealNormalMapTextures(importer , !assetPath.Contains("SceneTextures"));
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                DealNormalTextures(importer);
            }
        }
        else if (assetPath.Contains("Assets/Scenes/") && assetPath.CustomEndsWith(".exr"))
        {
            DealLightmapTextures(importer);
        }
        else if (assetPath.Contains("Assets/Textures/"))
        {
            DealUISrcTextures(importer);
        }
    }

    void DealUISingleTexture(TextureImporter importer)
    {
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 9;
        importer.isReadable = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        if (importer.assetPath.ToLower().EndsWith(".jpg"))
        {
#if UNITY_IOS
            importer.npotScale = TextureImporterNPOTScale.ToLarger;
#else
            importer.npotScale = TextureImporterNPOTScale.ToNearest;
#endif
            int maxSize = 0;
            TextureImporterFormat format = TextureImporterFormat.ETC2_RGB4;
            if (!importer.GetPlatformTextureSettings("Android", out maxSize, out format) || format != TextureImporterFormat.ETC2_RGB4)
                setAndroidTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.ETC2_RGB4);

            format = TextureImporterFormat.PVRTC_RGB4;
            if (!importer.GetPlatformTextureSettings("iPhone", out maxSize, out format) || format != TextureImporterFormat.PVRTC_RGB4)
                setiPhoneTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.PVRTC_RGB4);
        }
        else
        {
            importer.npotScale = TextureImporterNPOTScale.ToLarger;
            int maxSize = 0;
            TextureImporterFormat format;

            if (!importer.GetPlatformTextureSettings("Android", out maxSize, out format))
                setAndroidTextureSetting(importer , importer.maxTextureSize, TextureImporterFormat.ETC2_RGBA8);
            if (!importer.GetPlatformTextureSettings("iPhone", out maxSize, out format))
                setiPhoneTextureSetting(importer , importer.maxTextureSize,TextureImporterFormat.PVRTC_RGBA4);
        }
    }

    void DealUITextures(TextureImporter importer)
    {
        importer.maxTextureSize = 2048;
        importer.mipmapEnabled = false;
        string[] temps = Path.GetDirectoryName(importer.assetPath).Split('/');
        if (temps.Length > 1)
            importer.spritePackingTag = temps[temps.Length - 1];
        else
            importer.spritePackingTag = temps[0];

        setAndroidTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.ETC2_RGBA8);
        setiPhoneTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);
    }

    void DealNormalTextures(TextureImporter importer)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
        if (texture == null)
        {
            return;
        }

        int size = texture.height > texture.width ? texture.height : texture.width;
        if (!assetPath.Contains("Terrian"))
        {
            if (size > 1024)
            {
                Debug.LogError("发现超过1024贴图！！！" + assetPath, AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                importer.maxTextureSize = 2048;
            } 
            else importer.maxTextureSize = 1024;
            //importer.mipmapEnabled = false;
        }
        else
        {
            importer.maxTextureSize = size;
            //importer.mipmapEnabled = true;
        }
        importer.isReadable = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 9;

        int maxSize = 0;
        TextureImporterFormat format = TextureImporterFormat.ETC2_RGB4;
        if (importer.assetPath.ToLower().EndsWith(".jpg"))
        {
            if (!importer.GetPlatformTextureSettings("Android", out maxSize, out format) || format != TextureImporterFormat.ETC2_RGB4)
                setAndroidTextureSetting(importer, 1024, TextureImporterFormat.ETC2_RGB4);

            format = TextureImporterFormat.PVRTC_RGB4;
            if (!importer.GetPlatformTextureSettings("iPhone", out maxSize, out format) || format != TextureImporterFormat.PVRTC_RGB4)
                setiPhoneTextureSetting(importer, 1024, TextureImporterFormat.PVRTC_RGB4);
        }
        else
        {
            setAndroidTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.ETC2_RGBA8);
            setiPhoneTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);
        }
    }

    void DealLightmapTextures(TextureImporter importer)
    {
        importer.anisoLevel = 4;
        importer.isReadable = false;
        
        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
        setAndroidTextureSetting(importer, 1024, TextureImporterFormat.ETC2_RGBA8);
        setiPhoneTextureSetting(importer, 1024, TextureImporterFormat.PVRTC_RGB4);
    }
    /// <summary>
    /// 处理法线贴图
    /// </summary>
    void DealNormalMapTextures(TextureImporter importer , bool isReadable)
    {
        importer.textureType = TextureImporterType.NormalMap;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 4;
        importer.isReadable = isReadable;

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
        setAndroidTextureSetting(importer, 1024, TextureImporterFormat.ETC2_RGBA8);
        setiPhoneTextureSetting(importer, 1024, TextureImporterFormat.PVRTC_RGB4);
    }

    void DealUISrcTextures(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePackingTag = string.Empty;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        string[] all_sprites = Directory.GetFiles("Assets/Res/Atlas", "*.png", SearchOption.AllDirectories);
        foreach (var file in all_sprites)
        {
            string tmpName = Path.GetFileNameWithoutExtension(file);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(file);

            foreach (var tmp in sprites)
            {
                if (tmp is Sprite)
                {
                    var sprite = tmp as Sprite;
                    if (sprite.name == Path.GetFileNameWithoutExtension(importer.assetPath))
                    {
                        importer.spriteBorder = sprite.border;
                        break;
                    }
                }
            }
        }
    }

    void DealT4MTextures(TextureImporter importer)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
        if (texture != null)
        {
            int size = texture.height > texture.width ? texture.height : texture.width;
            if (size > 1024)
                Debug.LogError("发现超过1024贴图！！！" + assetPath, AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            importer.maxTextureSize = size > 1024 ? 2048 : size;
        }
        importer.mipmapEnabled = true;
        
        importer.isReadable = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 9;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        setAndroidTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.ETC2_RGBA8);
        setiPhoneTextureSetting(importer, importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);
    }

    /// <summary>
    /// 处理换装角色的身体部件、武器的贴图设置
    /// </summary>
    /// <param name="importer"></param>
    void DealPlayerTextures(TextureImporter importer)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
        if (texture != null)
        {
            int size = texture.height > texture.width ? texture.height : texture.width;
            if (size > 1024)
                Debug.LogError("发现超过1024贴图！！！" + assetPath, AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            importer.maxTextureSize = size > 1024 ? 2048 : size;
        }
        importer.mipmapEnabled = false;

        importer.isReadable = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 9;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }

    private void setAndroidTextureSetting(TextureImporter importer , int maxTextureSize , TextureImporterFormat format)
    {
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
        if (!settings.overridden)
        {
            settings.maxTextureSize = maxTextureSize;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.allowsAlphaSplitting = false;
            settings.format = format;
            settings.overridden = true;
            settings.compressionQuality = (int)TextureCompressionQuality.Best;
            importer.SetPlatformTextureSettings(settings);
        }
    }

    private void setiPhoneTextureSetting(TextureImporter importer, int maxTextureSize, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("iPhone");
        if (!settings.overridden)
        {
            settings.maxTextureSize = maxTextureSize;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.allowsAlphaSplitting = false;
            settings.format = format;
            settings.overridden = true;
            settings.compressionQuality = (int)TextureCompressionQuality.Best;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}

#endif