using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BundleChecker.ResoucreAttribute
{
    public class MaterialAttribute : ABaseResource
    {
        public const string DEP_SHADER = "depShader";
        public const string DEP_TEXTURES = "depTextures";

        public string Shader { get; private set; }

        public string[] TextureNames { get; private set; }

        public MaterialAttribute(ResoucresBean res) : base(res)
        {
            Object[] depObjArr = EditorUtility.CollectDependencies(res.mainObjs);

            List<string> depTexs = new List<string>();
            foreach (Object obj in depObjArr)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);

                if(obj is Texture2D)
                    depTexs.Add(Path.GetFileName(assetPath));
                if (assetPath.EndsWith(".shader"))
                    Shader = Path.GetFileName(assetPath);
            }
            TextureNames = depTexs.ToArray();
        }

        protected override string[] getPropertyValue(string property)
        {
            if (property == DEP_TEXTURES) return TextureNames;
            if (property == DEP_SHADER) return new[] {Shader};
            
            return base.getPropertyValue(property);
        }
    }
}