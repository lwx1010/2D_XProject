using System.Collections;
using FSG.MeshAnimator;
using Riverlake.Resources;
using UnityEngine;

namespace Riverlake.RoleEntity
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

            //搜索缓存绑定点
            this.searchJoints(insObj);

            if (OnLoadFinish != null)
                OnLoadFinish.Invoke(this);
        }
        

        #endregion

        #region Animation Controller

        public override void Crossfade(string clip)
        {
            animator.Crossfade(clip);
        }

        public override void CrossfadeQueue(string clip)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsPlaying(string clip)
        {
            return animator.currentAnimation.animationName == clip;
        }

        public override void Play(string clip)
        {
            animator.Play(clip);
        }

        public override void PlayQueue(string clip)
        {
            animator.PlayQueued(clip);
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

        public override float GetClipLength(string clip)
        {
            MeshAnimation animClip = animator.GetClip(clip);
            if (animClip == null) return -1;
            return animClip.length;
        }

        #endregion
    }
}