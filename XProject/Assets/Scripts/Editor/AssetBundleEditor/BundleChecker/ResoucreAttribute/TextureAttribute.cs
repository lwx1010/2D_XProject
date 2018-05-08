using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BundleChecker.ResoucreAttribute
{
    public class TextureAttribute : ABaseResource
    {

        #region -----------------可查询的属性-------------------------------

        public const string WIDTH = "width";
        public const string HEIGHT = "height";
        public const string MEMORYSIZE = "memorySize";
        public const string FORMAT = "format";
        public const string MIPMAP = "mipmap";
        public const string READWRITE = "rw";
        #endregion

        public int Width { get; private set; }

        public int Height { get; private set; }

        public float MemorySize { get; private set; }

        public string Format { get; private set; }

        public bool MipMap { get; private set; }

        public bool RW { get; private set; }
        public TextureAttribute(ResoucresBean res) : base(res)
        {
            TextureImporter textImporter = AssetImporter.GetAtPath(res.AssetPath) as TextureImporter;
            
            Texture2D tex = res.mainObjs[0] as Texture2D;
            this.Width = tex.width;
            this.Height = tex.height;
            this.MemorySize = getTextureMemorySize(textImporter , tex);
//            GameObject.DestroyImmediate(tex);

            this.Format = textImporter.textureFormat.ToString();
            this.MipMap = textImporter.mipmapEnabled;
            this.RW = textImporter.isReadable;
        }


        private float getTextureMemorySize(TextureImporter import, Texture2D text)
        {
            int pixls = 4;
            switch (import.textureFormat)
            {
                    case TextureImporterFormat.ARGB16:
                    pixls = 2;
                    break;
            }
            return text.width*text.height*pixls / 1024;
        }

        protected override string[] getPropertyValue(string property)
        {
            if (property == WIDTH) return new[] { Convert.ToString(this.Width)};
            if (property == HEIGHT) return new[] { Convert.ToString(this.Height)};
            if (property == MEMORYSIZE)
            {
                float mb = this.MemorySize/1024;
                if (mb >= 1) return new[] { string.Format("{0:F}MB", mb)};
                return new[] { string.Format("{0:F}KB" ,this.MemorySize)};
            }
            if (property == FORMAT) return new[] { this.Format};
            if (property == MIPMAP) return new[] { Convert.ToString(this.MipMap)};
            if (property == READWRITE) return new[] { Convert.ToString(this.RW)};

            return base.getPropertyValue(property);
        }
    }
}