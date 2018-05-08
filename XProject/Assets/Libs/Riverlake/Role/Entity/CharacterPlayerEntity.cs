using System;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using Riverlake.Resources;
using Riverlake.SMB;
using Riverlake.SMB.Core;
using UnityEngine;

namespace Riverlake.RoleEntity
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
        //人物使用的Shader
        public string ShaderName = "Standard";

        private Transform modelRootTrans;
        private Transform skeletonTrans;

        private Animation anim;
        private List<DynamicBoneCollider> dynBoneColliders;

        #region 公共属性

        public Transform SkeletonTrans
        {
            get { return skeletonTrans; }
        }

        #endregion

        public CharacterPlayerEntity()
        {

        }

        /// <summary>
        /// 初始化角色
        /// </summary>
        public void InitEntity(string skeleton, string header, string face, string body, string hand, string feet, string weapon)
        {
            //animation root
            GameObject enityObj = new GameObject("CharacterEntity");
            mainTrans = enityObj.transform;

            //model mesh root
            GameObject modelsRoot = new GameObject("ModelsRoot");
            modelRootTrans = modelsRoot.transform;
            modelRootTrans.SetParent(mainTrans);

            GameObject combineMeshRoot = new GameObject("CombineMesh");
            combineMeshRoot.transform.SetParent(mainTrans);

            this.skeleton = skeleton;
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
                //                root.GetChild(i).gameObject.SetActive(false);
                GameObject.Destroy(root.GetChild(i).gameObject);
            }
        }



        private Transform findParent(Transform root, string destName)
        {
            if (root.name.Equals(destName)) return root;

            foreach (Transform child in root)
            {
                Transform destTrans = findParent(child, destName);
                if (destTrans == null) continue;
                return destTrans;
            }
            return null;
        }

        #region 网格材质合并

        /// <summary>
        /// 提交合并操作
        /// </summary>
        private void onCombine(SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            Transform combineMeshRoot = mainTrans.Find("CombineMesh");
            //clearChild(combineMeshRoot);

            GameObject[] combineRenderer = new GameObject[skinnedMeshRenderers.Length];
            for (int i = 0; i < combineRenderer.Length; i++)
            {
                combineRenderer[i] = skinnedMeshRenderers[i].gameObject;
            }
            this.GenSkinnedCombine(skeletonTrans.gameObject, combineMeshRoot.gameObject, combineRenderer);
        }

        /// <summary>
        /// 合并指定SkinnedMeshRenderer集合的网格及材质
        /// </summary>
        /// <returns>合并完成的新SkinnedMeshRenderer对象</returns>
        public void GenSkinnedCombine(GameObject skeleton, GameObject parent, GameObject[] combinds)
        {
            // Fetch all bones of the skeleton
            List<Transform> skeletonBones = getSkeletonBips(skeleton);

            TextureBaker textureBaker = new TextureBaker();
            MeshBaker meshBaker = new MeshBaker();
            meshBaker.meshCombiner.resultSceneObject = parent;
            meshBaker.meshCombiner.renderType = MB_RenderType.skinnedMeshRenderer;
            textureBaker.BakerCommons.Add(meshBaker);

            textureBaker.GetObjectsToCombine().AddRange(combinds);
            //These can be assets configured at runtime or you can create them
            // on the fly like this
            textureBaker.onBuiltAtlasesSuccess = onBuiltAtlasesSuccess;
            textureBaker.textureBakeResults = new TextureBakeResults();
            textureBaker.resultMaterial = new Material(Shader.Find(ShaderName));

            TextureCombiner.RunCorutineWithoutPause(asyncCombine(skeletonBones, textureBaker), 0);
        }

        private void onBuiltAtlasesSuccess(TextureBaker baker)
        {
            //            Debug.Log("Calling success callback. baking meshes");
            MeshBaker meshbaker = baker.BakerCommons[0] as MeshBaker;
            //elapsedTime = Time.realtimeSinceStartup - t1;
            meshbaker.ClearMesh(); //only necessary if your not sure whats in the combined mesh
            meshbaker.textureBakeResults = baker.textureBakeResults;
            //Add the objects to the combined mesh
            meshbaker.AddDeleteGameObjects(baker.GetObjectsToCombine().ToArray(), null, true);
            meshbaker.Apply();

            //            Debug.Log("Completed baking textures on frame " + Time.frameCount);
        }
        /// <summary>
        /// 异常开始合并网格及材质，并更新Bone骨骼
        /// </summary>
        /// <param name="skeletonBones"></param>
        /// <param name="textureBaker"></param>
        /// <returns></returns>
        private IEnumerator asyncCombine(List<Transform> skeletonBones, TextureBaker textureBaker)
        {
            yield return textureBaker.CreateAtlasesCoroutine();

            MeshBaker meshbaker = textureBaker.BakerCommons[0] as MeshBaker;
            SkinnedMeshRenderer bakerRenderer = meshbaker.meshCombiner.resultSceneObject.GetComponentInChildren<SkinnedMeshRenderer>();
            
            // Collect bones
            List<Transform> bones = new List<Transform>();
            bones.AddRange(bakerRenderer.bones);
            for (int j = 0; j < bakerRenderer.bones.Length; j++)
            {
                for (int tBase = 0; tBase < skeletonBones.Count; tBase++)
                {
                    if (bakerRenderer.bones[j].name.Equals(skeletonBones[tBase].name))
                    {
                        bones[j] = skeletonBones[tBase];
                        break;
                    }
                }
            }
            bakerRenderer.bones = bones.ToArray();

            yield return null;

            rebindDynmacBones(skeletonBones);

            Debug.Log("clear temporal resources !");
            // Delete temporal resources
            clearChild(modelRootTrans);

            //重新查找绑定点
            this.searchJoints(this.skeletonTrans.gameObject);
            
            if (OnLoadFinish != null)
                OnLoadFinish.Invoke(this);

            Util.ClearMemory();
        }

        /// <summary>
        /// 获得纯Bip骨骼
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        private List<Transform> getSkeletonBips(GameObject skeleton)
        {
            List<Transform> childs = new List<Transform>();
            childs.AddRange(skeleton.GetComponentsInChildren<Transform>());

            string exp = "exp_";
            string dyn = "dyn_";

            for (int i = childs.Count - 1; i >= 0; i--)
            {
                Transform childTrans = childs[i];
                if (childTrans.name.StartsWith(exp))
                    childs.RemoveAt(i);
                else if (childTrans.name.StartsWith(dyn))
                {
                    GameObject.Destroy(childTrans.gameObject);
                    childs.RemoveAt(i);
                }
            }
            return childs;
        }

        /// <summary>
        /// 重新绑定动态的骨骼
        /// </summary>
        private void rebindDynmacBones(List<Transform> skeletonBips)
        {
            Transform[] subChilds = modelRootTrans.GetComponentsInChildren<Transform>();
            string exp = "exp_";
            string dyn = "dyn_";
            for (int i = 0; i < subChilds.Length; i++)
            {
                Transform childTrans = subChilds[i];
                if (!childTrans.name.StartsWith(exp))
                    continue;

                string parentName = childTrans.parent.name;
                for (int j = 0; j < skeletonBips.Count; j++)
                {
                    if (parentName.Equals(skeletonBips[j].name))
                    {
                        childTrans.SetParent(skeletonBips[j]);
                        break;
                    }
                }
                childTrans.name = childTrans.name.Replace(exp, dyn);

                //绑定Dynamic Collider
                DynamicBone dynBone = childTrans.GetComponent<DynamicBone>();
                if (dynBone != null && dynBone.m_Colliders != null)
                {
                    for (int q = 0; q < dynBone.m_Colliders.Count; q++)
                    {
                        DynamicBoneCollider dbCollider = dynBone.m_Colliders[i];
                        for (int w = 0; w < dynBoneColliders.Count; w++)
                        {
                            if (dynBoneColliders[w].name.Equals(dbCollider.name))
                            {
                                dynBone.m_Colliders[q] = dynBoneColliders[w];
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 异步加载接口实现

        public override void OnLoad()
        {
            AsyncContain container = new AsyncContain();

            if (skeletonTrans == null)
            {
                ALoadOperation loader = ResourceManager.LoadAssetAsync(skeleton);
                loader.OnFinish = initSkeleton;
                container.AddLoader(loader, 2);
            }

            string[] entityModelPaths = new[]
            {
                this.Header, this.Face , this.Body , this.Hand , this.Feet , this.Weapon
            };

            //            Debug.Log(string.Join(",", entityModelPaths));

            foreach (string modelPath in entityModelPaths)
            {
                if (string.IsNullOrEmpty(modelPath)) continue;
                //async load
                ALoadOperation loader = ResourceManager.LoadAssetAsync(modelPath);
                loader.OnFinish = delegate (ALoadOperation operation)
                {
                    GameObject resObj = operation.GetAsset<GameObject>();
                    if (resObj == null)
                    {
                        Debug.LogError("<<CharacterPlayEntity>> Load Asset Faile ! " + operation.assetPath);
                        return;
                    }
                    GameObject modelIns = GameObject.Instantiate(resObj);
                    modelIns.transform.SetParent(modelRootTrans);
                    modelIns.transform.localPosition = Vector3.zero;
                    modelIns.transform.localRotation = Quaternion.identity;
                    modelIns.transform.localScale = Vector3.one;
                };
                container.AddLoader(loader, 3);
            }

            //async load all assets
            AppSystem.Instance.StartCoroutine(loadAllAssetsAsync(container));
        }


        private void initSkeleton(ALoadOperation loader)
        {
            GameObject resObj = loader.GetAsset<GameObject>();
            if (resObj == null)
            {
                Debug.LogError("CharacterPlayEntity Load Skeleton Faile !" + skeleton);
                return;
            }

            GameObject skeletonGO = GameObject.Instantiate(resObj);
            skeletonGO.name = "Skeleton";
            skeletonTrans = skeletonGO.transform;
            skeletonTrans.SetParent(mainTrans);
            skeletonTrans.localPosition = Vector3.zero;
            skeletonTrans.localRotation = Quaternion.identity;
            skeletonTrans.localScale = Vector3.one;
            anim = skeletonGO.GetComponentInChildren<Animation>();
            if (anim == null)
                Debug.LogError("CharacterPlayerEntity can find animation on skeleton game object.");

            //cache dynmaic bone collider
            dynBoneColliders = new List<DynamicBoneCollider>();
            DynamicBoneCollider[] dbcs = skeletonGO.GetComponentsInChildren<DynamicBoneCollider>();
            dynBoneColliders.AddRange(dbcs);
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
            this.onCombine(smrs);

        }

        #endregion

        #region Animation Controller

        public override void Crossfade(string clip)
        {
            if (this.anim == null) return;
            this.anim.CrossFade(clip);
        }
        public override void CrossfadeQueue(string clip)
        {
            if (this.anim == null) return;
            this.anim.CrossFadeQueued(clip);
        }
        public override bool IsPlaying(string clip)
        {
            if (this.anim == null) return false;
            return this.anim.IsPlaying(clip);
        }

        public override void Play(string clip)
        {
            if (this.anim == null) return;
            this.anim.Play(clip);
        }

        public override void PlayQueue(string clip)
        {
            if (this.anim == null) return;
            this.anim.PlayQueued(clip);
        }

        public override void Stop()
        {
            if (this.anim == null) return;
            this.anim.Stop();
        }

        public override void Pause()
        {
            throw new NotSupportedException("CharacterPlayerEnity not supported Pause! Are you sure ?!");
        }

        public override float GetClipLength(string clip)
        {
            foreach (AnimationState animState in anim)
            {
                if(!animState.name.Equals(clip))    continue;

                return animState.length;
            }
            return -1;
        }

        #endregion
    }
}