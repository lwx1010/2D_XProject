using UnityEngine;

namespace Riverlake.RoleEntity
{
    /// <summary>
    /// 均衡因子
    /// </summary>
    public class BalanceEntityFactor
    {
        public float Min = 0.0f;
        public float Max = 1.0f;
        public float Weight = 1.0f;
    }

    /// <summary>
    /// 公共设置
    /// </summary>
    public class BalanceEntityGlobal
    {
        /// <summary>
        /// 运行帧率
        /// </summary>
        public static BalanceEntityFactor FpsEntityFactor = new BalanceEntityFactor()
        {
            Min = 10, Max = 45 , Weight = 0.2f
        };
        /// <summary>
        /// 运行内存
        /// </summary>
        public static BalanceEntityFactor MemoryEntityFactor = new BalanceEntityFactor()
        {
            Min = 150, Max = 500 , Weight = 0.2f
        };
        /// <summary>
        /// 角色类型的权重
        /// </summary>
        public static BalanceEntityFactor TypeEntityFactor = new BalanceEntityFactor()
        {
            Min = 0,
            Max = 10,
            Weight = 0.2f
        };
        /// <summary>
        /// 距离区域，X=Min , Y=Max
        /// </summary>
        public static Vector2 Bound = new Vector2()
        {
            x = 0.1f , y = 15
        };

        /// <summary>
        /// 活跃度更新频率,单位为秒
        /// </summary>
        public static float LivenessInteval = 3f;
        /// <summary>
        /// 加载间隔,单位为秒
        /// </summary>
        public static float MaxLoadInteval = 2f;
    }
}



