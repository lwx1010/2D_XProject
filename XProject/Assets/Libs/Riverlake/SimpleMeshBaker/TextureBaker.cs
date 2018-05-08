//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using Riverlake.SMB.Core;

namespace Riverlake.SMB
{
    /// <summary>
    /// Component that handles baking materials into a combined material.
    /// 
    /// The result of the material baking process is a MB2_TextureBakeResults object, which 
    /// becomes the input for the mesh baking.
    /// 
    /// This class uses the MB_TextureCombiner to do the combining.
    /// 
    /// This class is a Component (MonoBehavior) so it is serialized and found using GetComponent. If
    /// you want to access the texture baking functionality without creating a Component then use MB_TextureCombiner
    /// directly.
    /// </summary>
    public class TextureBaker : MeshBakerRoot
    {
        public MBLogLevel LOG_LEVEL = MBLogLevel.info;

    
        protected TextureBakeResults _textureBakeResults;
        public override TextureBakeResults textureBakeResults
        {
            get { return _textureBakeResults; }
            set { _textureBakeResults = value; }
        }

    
        protected int _atlasPadding = 1;
        public virtual int atlasPadding
        {
            get { return _atlasPadding; }
            set { _atlasPadding = value; }
        }


        protected int _maxAtlasSize = 1024;
        public virtual int maxAtlasSize
        {
            get { return _maxAtlasSize; }
            set { _maxAtlasSize = value; }
        }


        protected bool _fixOutOfBoundsUVs = false;
        public virtual bool fixOutOfBoundsUVs
        {
            get { return _fixOutOfBoundsUVs; }
            set { _fixOutOfBoundsUVs = value; }
        }


        protected int _maxTilingBakeSize = 512;
        public virtual int maxTilingBakeSize
        {
            get { return _maxTilingBakeSize; }
            set { _maxTilingBakeSize = value; }
        }
    
        protected List<ShaderTextureProperty> _customShaderProperties = new List<ShaderTextureProperty>();
    
        public virtual List<ShaderTextureProperty> customShaderProperties
        {
            get { return _customShaderProperties; }
            set { _customShaderProperties = value; }
        }


        protected List<MeshBakerCommon> bakerCommons = new List<MeshBakerCommon>();

        public List<MeshBakerCommon> BakerCommons
        {
            get { return bakerCommons;}
        }

        //this is depricated
        protected List<string> _customShaderPropNames_Depricated = new List<string>();
        public virtual List<string> customShaderPropNames
        {
            get { return _customShaderPropNames_Depricated; }
            set { _customShaderPropNames_Depricated = value; }
        }
        
        protected bool _doMultiMaterial;
        public virtual bool doMultiMaterial
        {
            get { return _doMultiMaterial; }
            set { _doMultiMaterial = value; }
        }
        
        protected Material _resultMaterial;
        public virtual Material resultMaterial
        {
            get { return _resultMaterial; }
            set { _resultMaterial = value; }
        }

        public MultiMaterial[] resultMaterials = new MultiMaterial[0];

        public List<GameObject> objsToMesh; //todo make this Renderer

        public override List<GameObject> GetObjectsToCombine()
        {
            if (objsToMesh == null) objsToMesh = new List<GameObject>();
            return objsToMesh;
        }
    
        public AtlasesAndRects[] OnCombinedTexturesCoroutineAtlasesAndRects;

        public delegate void OnCombinedTexturesCoroutineSuccess(TextureBaker baker);
    //    public delegate void OnCombinedTexturesCoroutineFail();
        public OnCombinedTexturesCoroutineSuccess onBuiltAtlasesSuccess;
    //    public OnCombinedTexturesCoroutineFail onBuiltAtlasesFail;
        /*
        bool _CreateAtlasesCoroutineSuccess = false;
        public bool CreateAtlasesCoroutineSuccess
        {
            get { return _CreateAtlasesCoroutineSuccess; }
        }
        bool _CreateAtlasesCoroutineIsFinished = false;
        public bool CreateAtlasesCoroutineIsFinished
        {
            get { return _CreateAtlasesCoroutineIsFinished; }
        }
        */
        public class CreateAtlasesCoroutineResult
        {
            public bool success = true;
            public bool isFinished = false;
        }

        public IEnumerator CreateAtlasesCoroutine()
        {
            this.OnCombinedTexturesCoroutineAtlasesAndRects = null;
            MBValidationLevel vl =  MBValidationLevel.quick ;
            if (!DoCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, vl))
            {
                yield break;
            }
            if (_doMultiMaterial && !_ValidateResultMaterials())
            {
                yield break;
            }
            else if (!_doMultiMaterial)
            {
                if (_resultMaterial == null)
                {
                    Debug.LogError("Combined Material is null please create and assign a result material.");
                    yield break;
                }
                Shader targShader = _resultMaterial.shader;
                for (int i = 0; i < objsToMesh.Count; i++)
                {
                    Material[] ms = MeshBakerUtility.GetGOMaterials(objsToMesh[i]);
                    for (int j = 0; j < ms.Length; j++)
                    {
                        Material m = ms[j];
                        if (m != null && m.shader != targShader)
                        {
                            Debug.LogWarning("Game object " + objsToMesh[i] + " does not use shader " + targShader + " it may not have the required textures. If not small solid color textures will be generated.");
                        }

                    }
                }
            }

            for (int i = 0; i < objsToMesh.Count; i++)
            {
                Material[] ms = MeshBakerUtility.GetGOMaterials(objsToMesh[i]);
                for (int j = 0; j < ms.Length; j++)
                {
                    Material m = ms[j];
                    if (m == null)
                    {
                        Debug.LogError("Game object " + objsToMesh[i] + " has a null material. Can't build atlases");
                        yield break;
                    }

                }
            }

            TextureCombiner combiner = new TextureCombiner();
            combiner.LOG_LEVEL = LOG_LEVEL;
            combiner.atlasPadding = _atlasPadding;
            combiner.maxAtlasSize = _maxAtlasSize;
            combiner.customShaderPropNames = _customShaderProperties;
            combiner.fixOutOfBoundsUVs = _fixOutOfBoundsUVs;
            combiner.maxTilingBakeSize = _maxTilingBakeSize;

            //initialize structure to store results
            int numResults = 1;
            if (_doMultiMaterial) numResults = resultMaterials.Length;
            OnCombinedTexturesCoroutineAtlasesAndRects = new AtlasesAndRects[numResults];
            for (int i = 0; i < OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
            {
                OnCombinedTexturesCoroutineAtlasesAndRects[i] = new AtlasesAndRects();
            }

            //Do the material combining.
            for (int i = 0; i < OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
            {
                Material resMatToPass = null;
                List<Material> sourceMats = null;
                if (_doMultiMaterial)
                {
                    sourceMats = resultMaterials[i].sourceMaterials;
                    resMatToPass = resultMaterials[i].combinedMaterial;
                }
                else
                {
                    resMatToPass = _resultMaterial;
                }
                Debug.Log(string.Format("Creating atlases for result material {0} using shader {1}", resMatToPass, resMatToPass.shader));
                TextureCombiner.CombineTexturesIntoAtlasesResult result2 = new TextureCombiner.CombineTexturesIntoAtlasesResult();
                yield return combiner.CombineTexturesIntoAtlases(OnCombinedTexturesCoroutineAtlasesAndRects[i], 
                                                                 resMatToPass, objsToMesh, sourceMats, result2);
            
                if (!result2.success)
                    yield break;
            
            }

            //Save the results
            textureBakeResults.doMultiMaterial = _doMultiMaterial;
            textureBakeResults.resultMaterial = _resultMaterial;
            textureBakeResults.resultMaterials = resultMaterials;
            textureBakeResults.fixOutOfBoundsUVs = combiner.fixOutOfBoundsUVs;
            unpackMat2RectMap(textureBakeResults);

            //set the texture bake resultAtlasesAndRects on the Mesh Baker component if it exists
            MeshBakerCommon[] mb = bakerCommons.ToArray();
            for (int i = 0; i < mb.Length; i++)
            {
                mb[i].textureBakeResults = textureBakeResults;
                mb[i].TexBaker = this;
            }

            if (LOG_LEVEL >= MBLogLevel.info) Debug.Log("Created Atlases");

            if (onBuiltAtlasesSuccess != null)
            {
                onBuiltAtlasesSuccess(this);
            }
        }
    
        /// <summary>
        /// Creates the atlases.
        /// </summary>
        /// <returns>
        /// The atlases.
        /// </returns>
        public AtlasesAndRects[] CreateAtlases()
        {
            AtlasesAndRects[] mAndAs = null;
            try
            {
                //mAndAs = _CreateAtlases(progressInfo, saveAtlasesAsAssets, editorMethods);
                CreateAtlasesCoroutineResult result = new CreateAtlasesCoroutineResult();
                TextureCombiner.RunCorutineWithoutPause(CreateAtlasesCoroutine(), 0);
                if (result.success && textureBakeResults != null) {
                    mAndAs = this.OnCombinedTexturesCoroutineAtlasesAndRects;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return mAndAs;
        }

        void unpackMat2RectMap(TextureBakeResults tbr)
        {
            List<Material> ms = new List<Material>();
            List<MaterialAndUVRect> mss = new List<MaterialAndUVRect>();
            List<Rect> rs = new List<Rect>();
            for (int i = 0; i < OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
            {
                AtlasesAndRects newMesh = this.OnCombinedTexturesCoroutineAtlasesAndRects[i];
                List<MaterialAndUVRect> map = newMesh.mat2rect_map;
                for (int j = 0; j < map.Count; j++)
                {
                    mss.Add(map[j]);
                    ms.Add(map[j].material);
                    rs.Add(map[j].atlasRect);
                }
            }
            tbr.materialsAndUVRects = mss.ToArray();
            tbr.materials = ms.ToArray();
            //tbr.prefabUVRects = rs.ToArray();
        }


        string PrintSet(HashSet<Material> s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Material m in s)
            {
                sb.Append(m + ",");
            }
            return sb.ToString();
        }

        bool _ValidateResultMaterials()
        {
            HashSet<Material> allMatsOnObjs = new HashSet<Material>();
            for (int i = 0; i < objsToMesh.Count; i++)
            {
                if (objsToMesh[i] != null)
                {
                    Material[] ms = MeshBakerUtility.GetGOMaterials(objsToMesh[i]);
                    for (int j = 0; j < ms.Length; j++)
                    {
                        if (ms[j] != null) allMatsOnObjs.Add(ms[j]);
                    }
                }
            }
            HashSet<Material> allMatsInMapping = new HashSet<Material>();
            for (int i = 0; i < resultMaterials.Length; i++)
            {
                MultiMaterial mm = resultMaterials[i];
                if (mm.combinedMaterial == null)
                {
                    Debug.LogError("Combined Material is null please create and assign a result material.");
                    return false;
                }
                Shader targShader = mm.combinedMaterial.shader;
                for (int j = 0; j < mm.sourceMaterials.Count; j++)
                {
                    if (mm.sourceMaterials[j] == null)
                    {
                        Debug.LogError("There are null entries in the list of Source Materials");
                        return false;
                    }
                    if (targShader != mm.sourceMaterials[j].shader)
                    {
                        Debug.LogWarning("Source material " + mm.sourceMaterials[j] + " does not use shader " + targShader + " it may not have the required textures. If not empty textures will be generated.");
                    }
                    if (allMatsInMapping.Contains(mm.sourceMaterials[j]))
                    {
                        Debug.LogError("A Material " + mm.sourceMaterials[j] + " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique.");
                        return false;
                    }
                    allMatsInMapping.Add(mm.sourceMaterials[j]);
                }
            }

            if (allMatsOnObjs.IsProperSubsetOf(allMatsInMapping))
            {
                allMatsInMapping.ExceptWith(allMatsOnObjs);
                Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + PrintSet(allMatsInMapping));
            }
            if (allMatsInMapping.IsProperSubsetOf(allMatsOnObjs))
            {
                allMatsOnObjs.ExceptWith(allMatsInMapping);
                Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + PrintSet(allMatsOnObjs));
                return false;
            }
            return true;
        }
    }

}



