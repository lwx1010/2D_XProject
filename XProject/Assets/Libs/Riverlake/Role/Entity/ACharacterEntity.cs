using System;
using System.Collections.Generic;
using UnityEngine;

namespace Riverlake.RoleEntity
{
    /// <summary>
    /// 角色实体控制基类
    /// </summary>
    public abstract class ACharacterEntity : ILoadEntityAsync
    {
        public float Liveness { get; set; }
        public int EntityWeight { get; set; }
        /// <summary>
        /// 加载合并完成时调用
        /// </summary>
        public Action<ACharacterEntity> OnLoadFinish; 

        protected Transform mainTrans;
        /// <summary>
        /// 挂点缓存
        /// </summary>
        protected Dictionary<string, Transform> joints = new Dictionary<string, Transform>(); 

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

        /// <summary>
        /// 初始资源完成后，搜索Entity的绑点
        /// </summary>
        protected void searchJoints(GameObject root)
        {
            joints.Clear();
            Transform[] childs = root.GetComponentsInChildren<Transform>();
            for (int i = 0; i < childs.Length; i++)
            {
                Transform childTrans = childs[i];
                if (childTrans.name.StartsWith("exp_"))
                {
                    joints[childTrans.name] = childTrans;
                }
            }
        }

        /// <summary>
        /// 获得指定名称的绑点对象
        /// </summary>
        /// <param name="joint">绑点名称</param>
        /// <returns>null表示无指定对象</returns>
        public Transform GetJointTransform(string joint)
        {
            Transform jointTrans = null;
            joints.TryGetValue(joint, out jointTrans);
            return jointTrans;
        }

        #region Animation Controller

        /// <summary>
        /// 交叉渐变切换动画
        /// </summary>
        /// <param name="clip">动画名称</param>
        public abstract void Crossfade(string clip);

        /// <summary>
        /// 交叉渐变切换动画队列
        /// </summary>
        /// <param name="clip">动画名称</param>
        public abstract void CrossfadeQueue(string clip);
        /// <summary>
        /// 当前是否正在播放指定动画
        /// </summary>
        /// <param name="clip">动画名称</param>
        /// <returns>true表示正在播放</returns>
        public abstract bool IsPlaying(string clip);
        /// <summary>
        /// 立即播放指定动画
        /// </summary>
        public abstract void Play(string clip);
        /// <summary>
        /// 将指定动画加入播放队列
        /// </summary>
        public abstract void PlayQueue(string clip);

        public abstract void Stop();

        public abstract void Pause();

        /// <summary>
        /// 获得指定动画片段的时长
        /// </summary>
        /// <param name="clip">片段的名称</param>
        /// <returns>如果找不到，则返回-1</returns>
        public abstract float GetClipLength(string clip);

        #endregion
    }
}