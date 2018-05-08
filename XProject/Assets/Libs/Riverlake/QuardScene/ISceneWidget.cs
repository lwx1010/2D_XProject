using UnityEngine;

namespace Riverlake.Scene
{
    public interface ISceneWidget
    {

        Rect Bounds { get; }

        Bounds Bounds3 { get; }

        GameObject Widget { get; }

        /// <summary>
        /// 目标进入分区
        /// </summary>
        void OnBoundEnter();

        /// <summary>
        /// 目标退出分区
        /// </summary>
        void OnBoundExit();

        /// <summary>
        /// 是否已全部进入
        /// </summary>
        /// <returns>true表示进入操作全部完成</returns>
        bool IsEnterCompleted();

        /// <summary>
        /// 是否已全部退出
        /// </summary>
        /// <returns>true表示退出操作全部完成</returns>
        bool IsExitCompleted();
    }
}