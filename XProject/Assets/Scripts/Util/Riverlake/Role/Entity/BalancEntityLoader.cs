using System.Collections.Generic;
using System.Text;
using Riverlake;
using UnityEngine;

namespace RiverLake.RoleEntity
{
    /// <summary>
    /// 基于多个因数的均衡加载处理
    /// <para>注意:这里必须要实时更新中心点的位置</para>
    /// </summary>
    public class BalancEntityLoader : Singleton<BalancEntityLoader>
    {

        private List<ILoadEntityAsync> loadAsyncs = new List<ILoadEntityAsync>();
        //最后更新活跃度的时间记录
        private float lastLivenessTime;

        // 最后加载实体的时间记录
        private float lastLoadTime;

        // 实时计算获得的加载频繁
        private float loadInterval;

        private bool isEnable;
        /// <summary>
        /// 中心点的世界坐标位置,eg:主角的坐标
        /// </summary>
        public Vector3 CenterPosition { get; set; }
        /// <summary>
        /// 新增加载实体
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(ILoadEntityAsync entity)
        {
            this.loadAsyncs.Add(entity);
            isEnable = true;
        }

        /// <summary>
        /// 删除指定实体
        /// </summary>
        public void RemoveEntity(ILoadEntityAsync entity)
        {
            this.loadAsyncs.Remove(entity);
            isEnable = loadAsyncs.Count > 0;
        }

        /// <summary>
        /// 清空加载列表
        /// </summary>
        public void Clear()
        {
            this.loadAsyncs.Clear();
            isEnable = false;
        }

        protected void Update()
        {
            if (!isEnable) return;

            lastLivenessTime += Time.deltaTime;
            if (lastLivenessTime >= BalanceEntityGlobal.LivenessInteval)
            {
                lastLivenessTime = 0;

                this.UpdateLiveness(AppSystem.FPS , AppSystem.Memory , CenterPosition);
            }

            lastLoadTime += Time.deltaTime;
            if (lastLoadTime >= loadInterval)
            {
                lastLoadTime = 0;
                ILoadEntityAsync loadEntity = loadAsyncs[0];
                loadEntity.OnLoad();
                loadAsyncs.RemoveAt(0);
                isEnable = loadAsyncs.Count > 0;
            }
        }

        /// <summary>
        /// 计算活跃度
        /// </summary>
        public void UpdateLiveness(int fps, int memory , Vector3 center)
        {
            float fpsRate = BalanceEntityGlobal.FpsEntityFactor.Weight - calculatePercent(BalanceEntityGlobal.FpsEntityFactor, fps);  
            float memoryRate = calculatePercent(BalanceEntityGlobal.MemoryEntityFactor, memory);

            float totalSystemRate = fpsRate + memoryRate;

            //更新频率
            loadInterval = totalSystemRate * BalanceEntityGlobal.MaxLoadInteval;
            // impact factors accumulation
            //float entityRate = 1.0f - totalSystemRate;

            foreach (ILoadEntityAsync entity in loadAsyncs)
            {
                //entity type
                float typeRate = calculatePercent(BalanceEntityGlobal.TypeEntityFactor, entity.EntityWeight);

                // perform liveness calculation
                float distanceRate = percent(BalanceEntityGlobal.Bound.x, BalanceEntityGlobal.Bound.y, entity.GetDistance(center));

                entity.Liveness = typeRate + distanceRate;
            }

            //根据活动度从小到大排序
            loadAsyncs.Sort((x , y)=> x.Liveness.CompareTo(y.Liveness));

        }

        /// <summary>
        /// 估算参数的百分比
        /// </summary>
        /// <param name="entityFactor"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private float calculatePercent(BalanceEntityFactor entityFactor, float value)
        {
            float normalized = percent(entityFactor.Min, entityFactor.Max, value);

            return normalized * Mathf.Clamp01(entityFactor.Weight);
        }

        /// <summary>
        /// 计算百分比
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private float percent(float min, float max, float value)
        {
            if (float.Equals(min, max)) return 0.0f;

            float clamped = Mathf.Clamp(value, min, max);
            return (clamped - min) / (max - min);
        }


        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.AppendLine("loadInteval:" + loadInterval);
            for (int i = 0; i < loadAsyncs.Count; i++)
            {
                ILoadEntityAsync loadEntity = loadAsyncs[i];
                buf.AppendLine(loadEntity.ToString());
            }
            return buf.ToString();
        }
    }

}
