using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;

namespace LuaFramework {
    [Obsolete("TimerInfo is obsolete, do not use it anymore")]
    public sealed class TimerInfo {
        public long tick;
        public bool stop;
        public bool delete;
        public UnityEngine.Object target;
        public string className;

        public TimerInfo(string className, UnityEngine.Object target) {
            this.className = className;
            this.target = target;
            delete = false;
        }
    }

    public sealed class TimerManager : Manager {
        #region LuaFramework框架自带定时器(已过时)
        [Obsolete("interval is obsolete, do not use it anymore")]
        private float interval = 0;
        [Obsolete("objects is obsolete, do not use it anymore")]
        private List<TimerInfo> objects = new List<TimerInfo>();

        [Obsolete("Interval is obsolete, do not use it anymore")]
        public float Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        [Obsolete("StartTimer is obsolete, see AddTimer or AddRepeatingTimer")]
        /// <summary>
        /// 启动计时器
        /// </summary>
        /// <param name="interval"></param>
        public void StartTimer(float value)
        {
            interval = value;
            InvokeRepeating("Run", 0, interval);
        }

        [Obsolete("StopTimer is obsolete, use RemoveTimer instead")]
        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer()
        {
            CancelInvoke("Run");
        }

        [Obsolete("AddTimerEvent is obsolete, do not use it anymore")]
        /// <summary>
        /// 添加计时器事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        public void AddTimerEvent(TimerInfo info)
        {
            if (!objects.Contains(info))
            {
                objects.Add(info);
            }
        }

        [Obsolete("RemoveTimerEvent is obsolete, do not use it anymore")]
        /// <summary>
        /// 删除计时器事件
        /// </summary>
        /// <param name="name"></param>
        public void RemoveTimerEvent(TimerInfo info)
        {
            if (objects.Contains(info) && info != null)
            {
                info.delete = true;
            }
        }

        [Obsolete("StopTimerEvent is obsolete, do not use it anymore")]
        /// <summary>
        /// 停止计时器事件
        /// </summary>
        /// <param name="info"></param>
        public void StopTimerEvent(TimerInfo info)
        {
            if (objects.Contains(info) && info != null)
            {
                info.stop = true;
            }
        }

        [Obsolete("ResumeTimerEvent is obsolete, do not use it anymore")]
        /// <summary>
        /// 继续计时器事件
        /// </summary>
        /// <param name="info"></param>
        public void ResumeTimerEvent(TimerInfo info)
        {
            if (objects.Contains(info) && info != null)
            {
                info.delete = false;
            }
        }

        [Obsolete("Run is obsolete, do not use it anymore")]
        /// <summary>
        /// 计时器运行
        /// </summary>
        void Run()
        {
            if (objects.Count == 0) return;
            for (int i = 0; i < objects.Count; i++)
            {
                TimerInfo o = objects[i];
                if (o.delete || o.stop) { continue; }
                ITimerBehaviour timer = o.target as ITimerBehaviour;
                timer.TimerUpdate();
                o.tick++;
            }
            /////////////////////////清除标记为删除的事件///////////////////////////
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i].delete) { objects.Remove(objects[i]); }
            }
        }
        #endregion

        /*************************************************************************/
        #region 自定义计时器
        public delegate void UpdateFunc();
        private Dictionary<string, LuaTimerBehaviour> timers = new Dictionary<string, LuaTimerBehaviour>();

        public void AddRepeatingTimer(GameObject go, string timerName, float delay, float interval, UpdateFunc func)
        {
#if UNITY_EDITOR
            Debugger.Log(string.Format("AddRepeatingTimer: {0}", timerName));
#endif
            if (!timers.ContainsKey(timerName))
            {
                var timer = go.AddComponent<LuaTimerBehaviour>();
                timer.StartRepeatingTimer(delay, interval, func);
                timers.Add(timerName, timer);
            }
        }

        public void RemoveRepeatingTimer(string timerName)
        {
#if UNITY_EDITOR
            Debugger.Log(string.Format("RemoveRepeatingTimer: {0}", timerName));
#endif
            if (timers.ContainsKey(timerName))
            {
                timers[timerName].StopRepeatingTimer();
                timers.Remove(timerName);
            }
        }

        public void AddTimer(GameObject go, string timerName, float delay, UpdateFunc func)
        {
            if (!timers.ContainsKey(timerName))
            {
                var timer = go.transform.GetOrAddComponent<LuaTimerBehaviour>();
                timer.StartTimer(delay, func);
                timers.Add(timerName, timer);
            }
            else
            {
                timers[timerName].StartTimer(delay, func);
            }
        }

        public void RemoveTimer(string timerName)
        {
            if (timers.ContainsKey(timerName))
                timers.Remove(timerName);
        }
        #endregion
    }

    public sealed class LuaTimerBehaviour : MonoBehaviour
    {
        private TimerManager.UpdateFunc repeatFunc;
        private TimerManager.UpdateFunc func;

        public void StartTimer(float delay, TimerManager.UpdateFunc func)
        {
            this.func = func;
            Invoke("TimerExcute", delay);
        }

        public void StartRepeatingTimer(float delay, float interval, TimerManager.UpdateFunc func)
        {
            this.repeatFunc = func;
            InvokeRepeating("TimerUpdate", delay, interval);
        }

        public void StopRepeatingTimer()
        {
            CancelInvoke("TimerUpdate");
            Destroy(this);
        }

        void TimerUpdate()
        {
            if (repeatFunc != null) repeatFunc();
        }

        void TimerExcute()
        {
            if (func != null) func();
            //Destroy(this);
        }
    }
}