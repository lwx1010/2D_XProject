/************************************************************************************
 *  @Author : LiangZG
 *  @Email ： game.liangzg@foxmail.com
 *  @Date : 2017-04-27
 ***********************************************************************************/

using System.Collections;
using LuaInterface;
using Riverlake.Resources;
using UnityEngine.SceneManagement;
using System;

namespace LuaFramework
{
    /// <summary>
    /// 游戏场景管理
    /// </summary>
    public sealed class SceneStageManager : Manager
    {
        public LuaTable curStage { get; private set; }
        /// <summary>
        /// 异步场景加载器
        /// </summary>
        public LoadStageAsync stageLoader { get; private set; }

        public enum LoadingType
        {
            normal = 1,
            wulindahui = 2,
            k3v3 = 3,
        }

        public static int loadingType = (int)LoadingType.normal;
        private LuaManager luaMgr;
        private string nextSceneName;
        /// <summary>
        /// 下一个场景的名称
        /// </summary>
        public string NextSceneName
        {
            get { return nextSceneName; }
        }

        void Awake()
        {
            luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        }

        public void LoadScene(LuaTable newStage)
        {
            string sceneName = newStage.GetStringField("stageName");
            luaMgr.StartCoroutine(loadScene(sceneName , newStage));
        }

        private IEnumerator loadScene(string sceneName , LuaTable newStage)
        {
            LuaTable lastStage = curStage;
            curStage = newStage;

            
            CallFunction(lastStage, "onExit");
            
            stageLoader = new LoadStageAsync();
            CallFunction(newStage, "onEnter" , stageLoader);
            
            while (stageLoader.MoveNext())
                yield return null;

            SceneManager.LoadScene(sceneName);

            while (SceneManager.GetActiveScene().name != sceneName)
                yield return null;

            //wait a frame 
            yield return null;
     
            this.OnCompleted();

            CallFunction(newStage, "onShow");
        }

        public void LoadSceneViaPreloading(LuaTable newStage )
        {
            LoadSceneViaPreloading(newStage , true);
        }
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="newStage">场景Lua状态</param>
        /// <param name="activeImmediate">场景加载完成后是否立即切换</param>
        public void LoadSceneViaPreloading(LuaTable newStage , bool activeImmediate)
        {
            LuaTable lastStage = curStage;
            curStage = newStage;

            //下个场景名
            string sceneName = newStage.GetStringField("stageName");
            nextSceneName = sceneName;

            //过滤场景名
            string transitScene = newStage.GetStringField("transitScene");
            stageLoader = new LoadStageAsync(sceneName, activeImmediate);
            
            CallFunction(lastStage, "onExit");
            
            SceneManager.LoadScene(transitScene);
        }


        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="newStage">场景Lua状态</param>
        /// <param name="activeImmediate">场景加载完成后是否立即切换</param>
        public void LoadSceneChunk(LuaTable newStage, bool activeImmediate)
        {
            LuaTable lastStage = curStage;
            curStage = newStage;

            //下个场景名
            string sceneName = newStage.GetStringField("stageName");
            nextSceneName = sceneName;
            
            stageLoader = new LoadStageAsync();

            CallFunction(lastStage, "onExit");

            SceneManager.LoadScene(sceneName + "_Base");
        }

        public static void CallFunction(LuaTable luaTab, string func , params object[] args)
        {
            if (luaTab == null) return;

            LuaFunction luaFunc = luaTab.GetLuaFunction(func);
            luaFunc.BeginPCall();
            luaFunc.Push(luaTab);
            luaFunc.PushArgs(args);
            luaFunc.PCall();
            luaFunc.EndPCall();
        }
        /// <summary>
        /// 设置主场景Camera抗锯齿
        /// </summary>
        public void UpdateSceneCameraSSAA()
        {
            //Camera mainSceneCam = Camera.main;
            //// UICamera不挂载
            //if (mainSceneCam != null && mainSceneCam.orthographic) return;

            //if (mainSceneCam == null || User_Config.quality == 0)
            //{
            //    if (mainSceneCam != null)
            //    {
            //        SuperSampling_SSAA oldssaa = mainSceneCam.GetComponent<SuperSampling_SSAA>();
            //        if(oldssaa != null) oldssaa.enabled = false;
            //    }
            //    return;
            //}

            //SuperSampling_SSAA ssaa = mainSceneCam.transform.GetOrAddComponent<SuperSampling_SSAA>();
            //ssaa.Filter = SSAAFilter.NearestNeighbor;
            //ssaa.renderTextureFormat = RenderTextureFormat.ARGB32;
            //ssaa.Scale = User_Config.quality == 1 ? 1.5f : 1.8f;

            //ssaa.enabled = false;
            //ssaa.enabled = true;

            //动态阴影
            //            UnityEngine.Object ligthCamObj = Resources.Load("Other/LightCamera");
            //            GameObject lightCamGo = GameObject.Instantiate(ligthCamObj) as GameObject;
//            LightFace.Get();
//            LightFace.GlobalProjectionDir = new Vector3(0.95f ,-1,0);
            
        }


        public void OnCompleted()
        {
            //stageLoader = null; //清空

            UpdateSceneCameraSSAA();
            
        }
    }
}
