using System;
using System.Collections;
using System.Collections.Generic;
using Riverlake;
using Riverlake.Resources;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RiverLake.RoleEntity
{
    /// <summary>
    /// 实体玩家角色控制
    /// <para>在修改实体部件属性后，需要手动提交加载!调用OnLoad函数</para>
    /// </summary>
    public class CharacterPlayerEntity : ACharacterEntity
    {
        private string skeleton;
        // 头部属性
        public string Header;
        // 脸部
        public string Face;
        // 身体
        public string Body;
        // 手臂
        public string Hand;
        // 脚/腿部
        public string Feet;
        // 武器
        public string Weapon;

        private Transform modelRootTrans;
        private Transform skeletonTrans;

        private Animation anim;
        #region 公共属性

        #endregion

        public CharacterPlayerEntity()
        {
            
        }

        /// <summary>
        /// 初始化角色
        /// </summary>
        public void InitEntity(string skeleton , string header, string face , string body, string hand , string feet , string weapon)
        {
            //animation root
            GameObject enityObj = new GameObject("CharacterEntity");
            mainTrans = enityObj.transform;

            Object resObj = ResourceManager.LoadPrefab(skeleton);
            if (resObj == null)
            {
                Debug.LogError("CharacterPlayEntity Load Skeleton Faile !" + skeleton);
                return;
            }

            GameObject skeletonGO = GameObject.Instantiate(resObj) as GameObject;
            skeletonGO.name = "Skeleton";
            skeletonTrans = skeletonGO.transform;
            skeletonTrans.SetParent(mainTrans);
            skeletonTrans.localPosition = Vector3.zero;
            skeletonTrans.localScale = Vector3.one;
            anim = skeletonGO.GetComponentInChildren<Animation>();
            if(anim == null)
                Debug.LogError("CharacterPlayerEntity can find animation on skeleton game object.");
            //model mesh root
            GameObject modelsRoot = new GameObject("ModelsRoot");
            modelRootTrans = modelsRoot.transform;
            modelRootTrans.SetParent(mainTrans);

            this.Header = header;
            this.Face = face;
            this.Body = body;
            this.Hand = hand;
            this.Feet = feet;
            this.Weapon = weapon;
            
            //comit balance load
            BalancEntityLoader.Instance.AddEntity(this);
        }

        public void OnDestory()
        {
            //销毁时,检测执行,避免多余的加载
            BalancEntityLoader.Instance.RemoveEntity(this);
        }

        private void clearChild(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(root.GetChild(i).gameObject);
            }
        }



        private Transform findParent(Transform root, string destName)
        {
            if (root.name.Equals(destName)) return root;

            foreach (Transform child in root)
            {
                Transform destTrans = findParent(child , destName);
                if(destTrans == null)   continue;
                return destTrans;
            }
            return null;
        }

        #region 网格材质合并

        /// <summary>
        /// 提交合并操作
        /// </summary>
        public void OnCombine(SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            Transform oldCombineMesh = mainTrans.Find("CombineMesh");
            if (oldCombineMesh != null)
                GameObject.Destroy(oldCombineMesh.gameObject);
            
            GameObject obj = this.GenSkinnedCombine(skeletonTrans.gameObject, skinnedMeshRenderers);
            Transform combineMesh = obj.transform;
            combineMesh.parent = mainTrans;
            combineMesh.localRotation = Quaternion.identity;
            combineMesh.localScale = Vector3.one;
            combineMesh.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 合并指定SkinnedMeshRenderer集合的网格及材质
        /// </summary>
        /// <returns>合并完成的新SkinnedMeshRenderer对象</returns>
        public GameObject GenSkinnedCombine(GameObject skeleton, SkinnedMeshRenderer[] srtSkinneds)
        {
            // Fetch all bones of the skeleton
            List<Transform> skeletonTrans = new List<Transform>();
            skeletonTrans.AddRange(skeleton.GetComponentsInChildren<Transform>(true));

            List<CombineInstance> combineIns = new List<CombineInstance>();
            List<Transform> bones = new List<Transform>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<int> meshVertexs = new List<int>();
            List<Material> mats = new List<Material>();

            int boneOffset = 0;
            for (int i = 0; i < srtSkinneds.Length; i++)
            {
                SkinnedMeshRenderer smr = srtSkinneds[i];
                if (smr == null || smr.sharedMesh == null) continue;

                mats.AddRange(smr.materials);

                // Collect bones
                for (int j = 0; j < smr.bones.Length; j++)
                {
                    for (int tBase = 0; tBase < skeletonTrans.Count; tBase++)
                    {
                        if (smr.bones[j].name.Equals(skeletonTrans[tBase].name))
                        {
                            bones.Add(skeletonTrans[tBase]);
                            break;
                        }
                    }
                }

                BoneWeight[] boneWeightArr = smr.sharedMesh.boneWeights;
                for (int j = 0; j < boneWeightArr.Length; j++)
                {
                    BoneWeight bw = boneWeightArr[j];

                    bw.boneIndex0 += boneOffset;
                    bw.boneIndex1 += boneOffset;
                    bw.boneIndex2 += boneOffset;
                    bw.boneIndex3 += boneOffset;

                    boneWeights.Add(bw);
                }
                boneOffset += bones.Count;

                for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = smr.sharedMesh;
                    ci.subMeshIndex = sub;
//                    ci.transform = smr.transform.localToWorldMatrix;
                    combineIns.Add(ci);
                    meshVertexs.Add(ci.mesh.vertexCount);
                }
            }


            //        List<Matrix4x4> bindpose = new List<Matrix4x4>();
            //        Matrix4x4 rootWorldToLocalMatrix = combineMesh.worldToLocalMatrix;
            //        for (int i = 0; i < bones.Count; i++)
            //        {
            //            bindpose.Add(bones[i].worldToLocalMatrix * rootWorldToLocalMatrix);
            //        }

            //合并网格
            GameObject obj = new GameObject("CombineMesh");
            SkinnedMeshRenderer newSMR = obj.AddComponent<SkinnedMeshRenderer>();
            newSMR.sharedMesh = new Mesh();

            // 合并材质
            newSMR.sharedMesh.CombineMeshes(combineIns.ToArray(), true, false);
            combineMatrial(newSMR, mats, meshVertexs);

            //                newSMR.sharedMesh.CombineMeshes(combineIns.ToArray(), false, false);
            //                newSMR.materials = mats.ToArray();

            newSMR.bones = bones.ToArray();
            //        newSMR.sharedMesh.boneWeights = boneWeights.ToArray();
            //        newSMR.sharedMesh.bindposes = bindpose.ToArray();
            newSMR.sharedMesh.RecalculateBounds();

            return obj;
        }
        /// <summary>
        /// 合并材质
        /// </summary>
        /// <param name="newSMR">最终的蒙皮网格对象</param>
        /// <param name="mats">需要被合并的材质集合</param>
        /// <param name="meshVertexs">当前网格对象的顶点数量集合</param>
        private void combineMatrial(SkinnedMeshRenderer newSMR, List<Material> mats, List<int> meshVertexs)
        {
            if (mats.Count <= 0) return;

            List<Texture2D> mainTexs = new List<Texture2D>();
            for (int q = 0; q < mats.Count; q++)
            {
                mainTexs.Add(mats[q].mainTexture as Texture2D);
            }

            Texture2D atlasTex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
            Rect[] packingResult = atlasTex.PackTextures(mainTexs.ToArray(), 0, 1024);

            Vector2[] originalUVs = newSMR.sharedMesh.uv;
            Vector2[] atlasUvs = new Vector2[originalUVs.Length];

            int rectIndex = 0;
            int vertTracker = 0;
            for (int i = 0; i < atlasUvs.Length; i++)
            {
                atlasUvs[i].x = Mathf.Lerp(packingResult[rectIndex].xMin, packingResult[rectIndex].xMax, originalUVs[i].x);
                atlasUvs[i].y = Mathf.Lerp(packingResult[rectIndex].yMin, packingResult[rectIndex].yMax, originalUVs[i].y);

                if (i >= meshVertexs[rectIndex] + vertTracker)
                {
                    vertTracker += meshVertexs[rectIndex];
                    rectIndex++;
                }
            }

            // 恢复Renderer信息
            Material newAltasMat = new Material(mats[0].shader);
            newAltasMat.mainTexture = atlasTex;

            newSMR.sharedMesh.uv = atlasUvs;
            newSMR.material = newAltasMat;
        }

        #endregion

        #region 异步加载接口实现
        
        public override void OnLoad()
        {
            string[] entityModelPaths = new[]
            {
                this.Header, this.Face , this.Body , this.Hand , this.Feet , this.Weapon
            };

//            Debug.Log(string.Join(",", entityModelPaths));

            AsyncContain container = new AsyncContain();
            foreach (string modelPath in entityModelPaths)
            {
                if(string.IsNullOrEmpty(modelPath)) continue;
                //async load
                ALoadOperation loader = ResourceManager.LoadAssetAsync(modelPath);
                loader.OnFinish = delegate(ALoadOperation operation)
                {
                    GameObject modelIns = operation.GetAsset<GameObject>();
                    if (modelIns == null)
                    {
                        Debug.LogError("<<CharacterPlayEntity>> Load Asset Faile ! " + operation.assetPath);
                        return;
                    }
                    modelIns.transform.SetParent(modelRootTrans);
                    modelIns.transform.localPosition = Vector3.zero;
                    modelIns.transform.localScale = Vector3.one;
                };
                container.AddLoader(loader , 1);
            }

            //async load all assets
            AppSystem.Instance.StartCoroutine(loadAllAssetsAsync(container));
        }
        /// <summary>
        /// 异步加载实体所需资源,主要涉及实体的身体部件
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private IEnumerator loadAllAssetsAsync(AsyncContain container)
        {
            while (container.MoveNext())
            {
                yield return null;
            }

            yield return null;

            SkinnedMeshRenderer[] smrs = modelRootTrans.GetComponentsInChildren<SkinnedMeshRenderer>();
            this.OnCombine(smrs);

            // Delete temporal resources
            clearChild(modelRootTrans);
        } 
        
        #endregion

        #region Animation Controller

        public override void Crossfade(string anim)
        {
            this.anim.CrossFade(anim);
        }
        public override void CrossfadeQueue(string anim)
        {
            this.anim.CrossFadeQueued(anim);
        }
        public override bool IsPlaying(string anim)
        {
            return this.anim.IsPlaying(anim);
        }

        public override void Play(string anim)
        {
            this.anim.Play(anim);
        }

        public override void PlayQueue(string anim)
        {
            this.anim.PlayQueued(anim);
        }

        public override void Stop()
        {
            this.anim.Stop();
        }

        public override void Pause()
        {
            throw new NotSupportedException("CharacterPlayerEnity not supported Pause! Are you sure ?!");
        }

        #endregion
    }
}