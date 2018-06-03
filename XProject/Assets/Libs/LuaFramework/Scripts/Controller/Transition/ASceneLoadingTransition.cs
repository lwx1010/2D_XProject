using System;
using System.Collections;
using LuaFramework;
using AL.Resources;
using UnityEngine;

namespace AL.LuaFramework.Controller.Transition
{
    /// <summary>
    /// 场景加载过滤策略
    /// </summary>
    public abstract class ASceneLoadingTransition : MonoBehaviour
    {
        [HideInInspector]
        public Action<float> ProcessAction;


        protected LoadStageAsync stagerLoader;

        private SceneStageManager ssm;

        
        #region Unity 标准API

        protected virtual void Awake()
        {
            ssm = AppFacade.Instance.GetManager<SceneStageManager>();
            stagerLoader = ssm.stageLoader;
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {
            
        }

        #endregion
        
        public IEnumerator OnLoading()
        {
            SceneStageManager.CallFunction(ssm.curStage, "onEnter", this, stagerLoader);

            float progress = 0f;
            while (stagerLoader.MoveNext())
            {
                progress = Mathf.Lerp(progress, stagerLoader.Progress, Time.deltaTime * 10f);

                if (ProcessAction != null)
                    ProcessAction.Invoke(progress);

                yield return null;
            }

            if (ProcessAction != null) ProcessAction.Invoke(1);

            while (!stagerLoader.IsSceneDone())
                yield return null;

            //wait a frame 
            yield return null;

            ssm.OnCompleted();
            SceneStageManager.CallFunction(ssm.curStage, "onShow");
        }
    }
}