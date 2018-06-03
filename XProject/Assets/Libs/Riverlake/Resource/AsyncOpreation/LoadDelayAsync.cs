using System;
using LuaFramework;
using LuaInterface;
using UnityEngine;

namespace AL.Resources
{
    /// <summary>
    /// 加载延迟时间 
    /// </summary>
    public class LoadDelayAsync : ALoadOperation
    {
        private float delayTime;

        private float time;

        public LoadDelayAsync(float time) : base("")
        {
            this.delayTime = time;
            if(time <= 0)
                throw new ArgumentException("时间必须大于零！");
        }

        public override void OnLoad()
        {
            time = Time.realtimeSinceStartup;
        }

        public override bool IsDone()
        {
            float elapseTime = Time.realtimeSinceStartup - time;
            progress = elapseTime/delayTime;
            bool isDone = elapseTime >= delayTime;

            if (isDone) this.onFinishEvent();

            return isDone;
        }

        [NoToLua]
        public override T GetAsset<T>()
        {
            return default(T);
        }
    }
}