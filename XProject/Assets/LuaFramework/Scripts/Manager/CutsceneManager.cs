using System;
using System.Collections;
using System.Collections.Generic;
using CinemaDirector;
using LuaInterface;
using AL.Plot;
using AL.Resources;
using RSG;
using UnityEngine;

namespace LuaFramework
{
    /// <summary>
    /// 剧情动画管理器
    /// </summary>
    public class CutsceneManager : Manager
    {
        private List<CutsceneTrigger> cutTriggers = new List<CutsceneTrigger>();

        private int mainCamCullmask = 0;
        private Cutscene curCutscene;
        private float fadeMainCamera;
        /// <summary>
        /// 已触发的记录
        /// </summary>
        private HashSet<int> triggered = new HashSet<int>();
         
        /// <summary>
        /// 缓存列表
        /// </summary>
        private Dictionary<int , Cutscene> catchCutscenes = new Dictionary<int ,Cutscene>(); 

        private List<GameObject>  caches = new List<GameObject>();
        /// <summary>
        /// 当前正在播放的剧情
        /// </summary>
        public Cutscene CurCutscene
        {
            get { return curCutscene; }
        }
        /// <summary>
        /// 当剧情动画开始播放时处理
        /// </summary>
        private void playCutsceneEvent(object obj , CutsceneEventArgs args)
        {
            Debugger.Log(string.Format("play cutscene : {0}", curCutscene.name));
            
            this.OnCutsceneEvent();

            PlotCamera[] plotCameraArr = curCutscene.GetComponentsInChildren<PlotCamera>(true);
            for (int i = 0; i < plotCameraArr.Length; i++)
            {
                if (plotCameraArr[i].IsMainCamera)
                {
                    plotCameraArr[i].Enable(fadeMainCamera);
                    break;
                }
            }

            if (curCutscene.IsDebug) return;

            //发送触发协议保存记录
            LuaFunction sendFunc = LuaManager.mainLua.GetFunction("CutsceneHandler.SendTrigger");
            if (sendFunc != null && !curCutscene.IsLooping)
            {
                sendFunc.BeginPCall();
                string cutsceneId = curCutscene.gameObject.name.Replace("Cutscene", "");
                cutsceneId = cutsceneId.Replace("(Clone)", "").Trim();
                sendFunc.Push(Convert.ToInt32(cutsceneId));
                sendFunc.PCall();
                sendFunc.EndPCall();
            }
        }

        /// <summary>
        /// 切换到剧情状态
        /// </summary>
        public void OnCutsceneEvent()
        {
            if (mainCamCullmask != 0) return;

            //UICamera.list[0].eventReceiverMask &= ~(1 << LayerMask.NameToLayer("UI"));
            //CameraUtil.HideLayerName(UICamera.list[0].cachedCamera, "UI", "UIModel");
            //CameraUtil.ShowLayerName(UICamera.list[0].cachedCamera, "Plot");
            //UICamera.list[0].cachedCamera.depth += 1;

            //Camera mainCamera = Camera.main;
            //mainCamCullmask = mainCamera.cullingMask;
            //mainCamera.cullingMask = 0;
            //CameraFadeEffect fadeCam = mainCamera.gameObject.GetComponent<CameraFadeEffect>();
            //if (fadeCam != null)    GameObject.Destroy(fadeCam);

            ////隐藏SceneEntity Shadow
            //SetSceneShadow(false);

            //SceneEntity mainRole = RoleManager.Instance.mainRole;
            //if (mainRole != null)
            //{
            //    mainRole.controller.CanMovePosition = false;
            //    mainRole.move.StopPath();
            //    Util.CallMethod("HEROSKILLMGR", "SetHeroGuaJi", 0, false);
            //}
        }

        /// <summary>
        /// 当剧情动画暂停播放时处理
        /// </summary>
        private void pauseCutsceneEvent(object obj, CutsceneEventArgs args)
        {


        }

        /// <summary>
        /// 当剧情动画完成时处理
        /// </summary>
        private void finishCutsceneEvent(object obj, CutsceneEventArgs args)
        {
            curCutscene = null;

            this.BackNomalEvent();

            Cutscene cutscene = obj as Cutscene;
            int cutsceneId = Convert.ToInt32(cutscene.gameObject.name.Substring(8));
            caches.Remove(cutscene.gameObject);
            GameObject.Destroy(cutscene.gameObject);
            catchCutscenes.Remove(cutsceneId);

            LuaTable triggerCtrl = LuaManager.mainLua.GetTable("CutsceneCtrl.inst");
            LuaFunction finishFunc = triggerCtrl.GetLuaFunction("finishCutscene");
            finishFunc.BeginPCall();
            finishFunc.Push(triggerCtrl);
            finishFunc.Push(cutsceneId);
            finishFunc.PCall();
            finishFunc.EndPCall();
        }

        /// <summary>
        /// 返回常规模式
        /// </summary>
        public void BackNomalEvent()
        {
            fadeMainCamera = 0;

            Camera.main.cullingMask = mainCamCullmask;
            mainCamCullmask = 0;

            //UICamera.list[0].eventReceiverMask |= 1 << LayerMask.NameToLayer("UI");
            //CameraUtil.ShowLayerName(UICamera.list[0].cachedCamera, "UI", "UIModel");
            //CameraUtil.HideLayerName(UICamera.list[0].cachedCamera, "Plot");
            //UICamera.list[0].cachedCamera.depth -= 1;
            
            //SetSceneShadow(true);

            ////PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
            ////panelMgr.ClearAllCutscenePanels();

            //SceneEntity mainRole = RoleManager.Instance.mainRole;
            //if (mainRole != null)
            //{
            //    mainRole.controller.CanMovePosition = true;
            //}

            SoundManager soundMgr = AppFacade.Instance.GetManager<SoundManager>();
            soundMgr.StopSound(SoundManager.SoundType.Story);
        }


        /// <summary>
        /// 设置场景中的所有SceneEntity是否渲染阴影
        /// </summary>
        /// <param name="isVisiable"></param>
        public void SetSceneShadow(bool isVisiable)
        {
            //LightFace lightFace = LightFace.Get();
            //if (lightFace != null)
            //{
            //    if (isVisiable)
            //    {
            //        this.StartCoroutine(this.restartProjecterShadow());
            //    }
            //    else
            //    {
            //        lightFace.ShadowsOn = true;
            //        LightFace.GlobalShadowCullingMode = LightFace.ProjectionCulling.ProjectorBounds;
            //        lightFace.ProCamera.ClearAllLayer();
            //        lightFace.OnShadowResolutionChange(3);
            //        lightFace.ProCamera.ShowLayerName("Plot");
            //    }
            //}
            
            //this.setSceneEntityShadow(RoleManager.npcs , isVisiable);
            //this.setSceneEntityShadow(RoleManager.monsters , isVisiable);
            //this.setSceneEntityShadow(RoleManager.roles , isVisiable);
            //this.setSceneEntityShadow(RoleManager.roleParts, isVisiable);
            //this.setSceneEntityShadow(RoleManager.xunLuoEntities , isVisiable);
            //this.setSceneEntityShadow(RoleManager.aiderNpcs, isVisiable);
        }

        private IEnumerator restartProjecterShadow()
        {
            //LightFace lightFace = LightFace.Get();
            //lightFace.ProCamera.MainCamera = Camera.main;
            yield return Yielders.EndOfFrame;
            //RoleManager.Instance.UpdateBlockSetting();
        }


        //private void setSceneEntityShadow(List<SceneEntity> entitys, bool isVisable)
        //{
        //    if (entitys == null || entitys.Count <= 0) return;

        //    for (int i = 0; i < entitys.Count; i++)
        //    {
        //        SceneEntity se = entitys[i];
        //        if (se == null) continue;
        //        se.SetShadowVisiable(isVisable);
        //    }
        //}

        public void AddTrigger(CutsceneTrigger trigger)
        {
            if (triggered.Contains(Convert.ToInt32(trigger.CutName)))
            {
                trigger.StartMethod = StartMethod.None;
                GameObject.Destroy(trigger.gameObject);
                return;
            }

            if (cutTriggers.Contains(trigger)) return;

            cutTriggers.Add(trigger);

            trigger.CutscenePaused += pauseCutsceneEvent;
            trigger.CutsceneFinished += finishCutsceneEvent;
        }

        public void RemoveTrigger(CutsceneTrigger trigger)
        {
            if (!cutTriggers.Contains(trigger))
                return;
            cutTriggers.Remove(trigger);
        }
        /// <summary>
        /// 切换场景时清空
        /// </summary>
        public void Clear()
        {
            cutTriggers.Clear();
            for (int i = caches.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(caches[i]);
            }
            caches.Clear();

            catchCutscenes.Clear();
        }
        /// <summary>
        /// 添加已触发完成的记录
        /// </summary>
        /// <param name="cutsceneId"></param>
        public void AddTriggerLog(int cutsceneId)
        {
            if(!triggered.Contains(cutsceneId))
                triggered.Add(cutsceneId);
        }

        /// <summary>
        /// 是否已触发过剧情
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <returns></returns>
        public bool IsTrigged(int cutsceneId)
        {
            return triggered.Contains(cutsceneId);
        }


        /// <summary>
        /// 根据指定值触发
        /// </summary>
        /// <param name="cutsceneId">过场动画ID</param>
        public void Trigger(int cutsceneId)
        {
            LuaTable triggerCtrl = LuaManager.mainLua.GetTable("CutsceneTriggerCtrl.inst");
            LuaFunction isTrigger = triggerCtrl.GetLuaFunction("isTrigger");
            isTrigger.BeginPCall();
            isTrigger.Push(triggerCtrl);
            isTrigger.Push(cutsceneId);
            isTrigger.PCall();
            bool result = isTrigger.CheckBoolean();
            isTrigger.EndPCall();

            if (!result)
            {
                return;
            }

            //处理预加载
            if (catchCutscenes.ContainsKey(cutsceneId))
            {
                PlayCutscene(catchCutscenes[cutsceneId]);
                return;
            }
            this.StartCoroutine(this.playCutscene(cutsceneId));
        }

        
        /// <summary>
        /// 播放指定的过场动画
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <returns></returns>
        private IEnumerator playCutscene(int cutsceneId)
        {
            yield return this.StartCoroutine(loadCutsceneAsync(cutsceneId));

            if(curCutscene == null) yield break;

            curCutscene.CutscenePlayed += playCutsceneEvent;

            curCutscene.Play();

            LuaManager.CallFunction("EventManager.SendEvent", "onPlayCutscene", cutsceneId);

            if(!curCutscene.IsLooping)
                this.AddTriggerLog(cutsceneId);  //添加记录

        }

        /// <summary>
        /// 异步加载剧情片段
        /// </summary>
        /// <param name="cutsceneId"></param>
        /// <returns></returns>
        private IEnumerator loadCutsceneAsync(int cutsceneId)
        {
            string cutscenePath = string.Concat("Prefab/Cutscene/Cutscene", cutsceneId);
            ALoadOperation operation = ResourceManager.LoadBundleAsync(cutscenePath);
            yield return this.StartCoroutine(operation);

            GameObject gObj = operation.GetAsset<GameObject>();

            if (gObj == null)
            {
                Debug.LogError(string.Format("找不到加载的剧情片段资源！路径：{0}", cutscenePath));
                yield break;
            }

            gObj = GameObject.Instantiate(gObj);
            gObj.name = gObj.name.Replace("(Clone)", "");
            GameObject.DontDestroyOnLoad(gObj);

            PlotCamera[] plotCameraArr = gObj.GetComponentsInChildren<PlotCamera>();
            for (int i = 0; i < plotCameraArr.Length; i++)
            {
                plotCameraArr[i].gameObject.SetActive(false);
            }

            yield return null;

            curCutscene = gObj.GetComponent<Cutscene>();
            curCutscene.Optimize();

            yield return null;
            
            curCutscene.CutscenePaused += pauseCutsceneEvent;
            curCutscene.CutsceneFinished += finishCutsceneEvent;
        }


        public void LoadCutscene(int cutsceneId)
        {
            this.StartCoroutine(loadCutsceneAsync(cutsceneId));
        }


        public void PlayCutscene(Cutscene cutscene)
        {
            if (cutscene == null)
            {
                Debug.LogError("并没有预加载的剧情片段！");
                return;
            }
            cutscene.gameObject.SetActive(true);
            int cutsceneId = Convert.ToInt32(cutscene.gameObject.name.Substring(8));

            cutscene.Play();
            curCutscene = cutscene;

            LuaManager.CallFunction("EventManager.SendEvent", "onPlayCutscene", cutsceneId);

            if (!cutscene.IsLooping)
                this.AddTriggerLog(cutsceneId);  //添加记录

            playCutsceneEvent(cutscene, null);
        }

        /// <summary>
        /// 预加载场景中的资源
        /// </summary>
        /// <param name="triggerGroup"></param>
        public void PreLoadCutscene(GameObject triggerGroup , LoadStageAsync stageLoader)
        {
            if (triggerGroup == null) return;

            GameObject.DontDestroyOnLoad(triggerGroup);
            caches.Add(triggerGroup);

            Transform triggerGroupTrans = triggerGroup.transform;
            List<string> cutsenes = new List<string>();
            for (int i = 0 , count = triggerGroupTrans.childCount ; i < count; i++)
            {
                Transform childTrans = triggerGroupTrans.GetChild(i);
                if(!childTrans.gameObject.activeSelf)   continue;

                string cutsceneId = childTrans.name.Substring(7);   //name = Trigger + id
                if(!triggered.Contains(Convert.ToInt32(cutsceneId)))
                    cutsenes.Add(cutsceneId);
            }

            cutsenes.Sort();

            for (int i = 0; i < cutsenes.Count; i++)
            {
                string resPath = string.Concat("Prefab/Cutscene/Cutscene", cutsenes[i]);
                ALoadOperation loadAssetAsync = ResourceManager.LoadBundleAsync(resPath);
                loadAssetAsync.OnFinish = instanceCutscene;
                stageLoader.AddLoader(loadAssetAsync, 5);
            }
        }

        /// <summary>
        /// 资源载入的资源
        /// </summary>
        /// <param name="loader"></param>
        private void instanceCutscene(ALoadOperation loader)
        {
            GameObject gObj = loader.GetAsset<GameObject>();

            if (gObj == null)
            {
                Debug.LogError(string.Format("找不到加载的剧情片段资源！路径：{0}", loader.assetPath));
                return;
            }

            gObj = GameObject.Instantiate(gObj);
            gObj.name = gObj.name.Replace("(Clone)", "");
            GameObject.DontDestroyOnLoad(gObj);
            caches.Add(gObj);

            PlotCamera[] plotCameraArr = gObj.GetComponentsInChildren<PlotCamera>();
            for (int i = 0; i < plotCameraArr.Length; i++)
            {
                plotCameraArr[i].gameObject.SetActive(false);
            }

            Cutscene cutscene = gObj.GetComponent<Cutscene>();
            cutscene.Optimize();

            cutscene.CutscenePaused += pauseCutsceneEvent;
            cutscene.CutsceneFinished += finishCutsceneEvent;

            int cutsceneId = Convert.ToInt32(gObj.name.Substring(8));
            this.catchCutscenes[cutsceneId] = cutscene;
            gObj.SetActive(false);
        }


        public void MainCameraFade(float start , float to , float duration)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            CameraFadeEffect fadeCam = mainCam.transform.GetOrAddComponent<CameraFadeEffect>();
            fadeCam.Fade(start , to , duration);
            fadeMainCamera = duration;
        }

        public void ClearAll()
        {
            this.Clear();
            this.triggered.Clear();
        }

    }
}