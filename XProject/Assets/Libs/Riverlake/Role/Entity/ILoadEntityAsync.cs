using UnityEngine;

namespace Riverlake.RoleEntity
{
    /// <summary>
    /// 动态实体异步加载接口
    /// <para>主要处理人物、武器、座骑等非建筑模型的实体</para>
    /// </summary>
    public interface ILoadEntityAsync
    {
        /// <summary>
        /// 活跃度
        /// </summary>
        float Liveness { get; set; }

        /// <summary>
        /// 角色类型的权重,权重值越大，活跃度越低
        /// <para>不同类型的角色，加载优先级不同</para>
        /// </summary>
        int EntityWeight { get; set; }
        /// <summary>
        /// 启动加载
        /// </summary>
        void OnLoad();

        /// <summary>
        /// 距离中心点的数值
        /// </summary>
        float GetDistance(Vector3 center);
    }

}

