using System.Collections;
using System.Collections.Generic;
using FSG.MeshAnimator;
using Riverlake;
using Riverlake.Resources;
using UnityEngine;

namespace RiverLake.RoleEntity
{
    /// <summary>
    /// 非玩家(换装)类型的模型实体处理
    /// </summary>
    public class CharacterNormalEntity : ACharacterEntity
    {
        private MeshAnimator animator;
        //资源路径
        private string assetPath;

        #region Async Load

        public CharacterNormalEntity(string assetPath)
        {
            this.assetPath = assetPath;
        }

        public override void OnLoad()
        {
            AppSystem.Instance.StartCoroutine(loadAssetAsync());
        }

        private IEnumerator loadAssetAsync()
        {
            ALoadOperation loader = ResourceManager.LoadAssetAsync(assetPath);
            yield return AppSystem.Instance.StartCoroutine(loader);

            GameObject prefab = loader.GetAsset<GameObject>();
            if (prefab == null)
            {
                Debug.LogError("Cant load normal entity ! Path is " + assetPath);
                yield break;
            }

            GameObject insObj = GameObject.Instantiate(prefab);
            mainTrans = insObj.transform;
            animator = insObj.GetComponentInChildren<MeshAnimator>();
            if(animator == null)
                Debug.LogError("CharactorNormalEntity Cant find Mesh Animator!");
        }
        

        #endregion

        #region Animation Controller

        public override void Crossfade(string anim)
        {
            animator.Crossfade(anim);
        }

        public override void CrossfadeQueue(string anim)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsPlaying(string anim)
        {
            return animator.currentAnimation.animationName == anim;
        }

        public override void Play(string anim)
        {
            animator.Play(anim);
        }

        public override void PlayQueue(string anim)
        {
            animator.PlayQueued(anim);
        }

        public override void Stop()
        {
            animator.RestartAnim();
            animator.Pause();
        }

        public override void Pause()
        {
            animator.Pause();
        }

        #endregion
    }
}