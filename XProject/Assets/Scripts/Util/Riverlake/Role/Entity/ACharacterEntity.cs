using UnityEngine;

namespace RiverLake.RoleEntity
{
    /// <summary>
    /// 角色实体控制基类
    /// </summary>
    public abstract class ACharacterEntity : ILoadEntityAsync
    {
        public float Liveness { get; set; }
        public int EntityWeight { get; set; }

        protected Transform mainTrans;

        public Transform MainTransform
        {
            get { return mainTrans; }
        }

        public virtual void OnLoad()
        {
            throw new System.NotImplementedException();
        }

        public virtual float GetDistance(Vector3 center)
        {
            return mainTrans == null ? float.MaxValue : Vector3.Distance(center, mainTrans.position);
        }

        #region Animation Controller

        /// <summary>
        /// 交叉渐变切换动画
        /// </summary>
        /// <param name="anim">动画名称</param>
        public abstract void Crossfade(string anim);

        /// <summary>
        /// 交叉渐变切换动画队列
        /// </summary>
        /// <param name="anim">动画名称</param>
        public abstract void CrossfadeQueue(string anim);
        /// <summary>
        /// 当前是否正在播放指定动画
        /// </summary>
        /// <param name="anim">动画名称</param>
        /// <returns>true表示正在播放</returns>
        public abstract bool IsPlaying(string anim);
        /// <summary>
        /// 立即播放指定动画
        /// </summary>
        public abstract void Play(string anim);
        /// <summary>
        /// 将指定动画加入播放队列
        /// </summary>
        public abstract void PlayQueue(string anim);

        public abstract void Stop();

        public abstract void Pause();

        #endregion
    }
}