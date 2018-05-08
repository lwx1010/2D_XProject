using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using System;

namespace LuaFramework {
    public static class LuaHelper
    {
        private static NetworkManager netMgr;
        /// <summary>
        /// getType
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static System.Type GetType(string classname) {
            Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
            System.Type t = null;
            t = assb.GetType(classname); ;
            if (t == null) {
                t = assb.GetType(classname);
            }
            return t;
        }

        /// <summary>
        /// 网络管理器
        /// </summary>
        public static NetworkManager GetNetManager() {
            if(netMgr == null)
                netMgr = AppFacade.Instance.GetManager<NetworkManager>();
            return netMgr;
        }

        /// <summary>
        /// 音乐管理器
        /// </summary>
        public static SoundManager GetSoundManager() {
            return AppFacade.Instance.GetManager<SoundManager>();
        }

        /// <summary>
        /// 计时器管理器
        /// </summary>
        /// <returns></returns>
        public static TimerManager GetTimerManager()
        {
            return AppFacade.Instance.GetManager<TimerManager>();
        }

        /// <summary>
        /// 游戏管理器
        /// </summary>
        /// <returns></returns>
        public static GameManager GetGameManager()
        {
            return AppFacade.Instance.GetManager<GameManager>();
        }

        public static SceneStageManager GetSceneManager()
        {
            return AppFacade.Instance.GetManager<SceneStageManager>();
        }

        public static Action Action(LuaFunction func) {
            Action action = () => {
                func.Call();
            };
            return action;
        }

        //public static UIEventListener.VoidDelegate VoidDelegate(LuaFunction func) {
        //    UIEventListener.VoidDelegate action = (go) => {
        //        func.Call(go);
        //    };
        //    return action;
        //}

        /// <summary>
        /// pbc/pblua函数回调
        /// </summary>
        /// <param name="func"></param>
        public static void OnCallLuaFunc(LuaByteBuffer data, LuaFunction func) {
            if (func != null) func.Call(data);
            Debug.LogWarning(string.Format("OnCallLuaFunc length:>>{0}", data.buffer.Length));
        }

        /// <summary>
        /// cjson函数回调
        /// </summary>
        /// <param name="data"></param>
        /// <param name="func"></param>
        public static void OnJsonCallFunc(string data, LuaFunction func) {
            Debug.LogWarning(string.Format("OnJsonCallback data:>>{0} lenght:>>{1}", data, data.Length));
            if (func != null) func.Call(data);
        }
    }
}