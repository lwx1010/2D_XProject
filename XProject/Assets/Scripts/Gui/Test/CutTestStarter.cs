using System;
using System.Collections;
using LuaFramework;
using LuaInterface;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// A sample behaviour for triggering Cutscenes.
    /// </summary>
    public class CutTestStarter : MonoBehaviour
    {
        public Cutscene cutTest;
        public float Delay = 0f;
        private bool hasTriggered = false;
        
        // Event fired when Cutscene's runtime reaches it's duration.
        public event CutsceneHandler CutsceneFinished;

        // Event fired when Cutscene has been paused.
        public event CutsceneHandler CutscenePaused;

        private bool havError = false;
        /// <summary>
        /// When the trigger is loaded, optimize the Cutscene.
        /// </summary>
        void Awake()
        {
            if (cutTest == null)
                cutTest = this.GetComponent<Cutscene>();
            
            Application.logMessageReceived += onLogMessageReceived;
        }

        private void onLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                cutTest.Pause();

                havError = true;

                LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
                LuaFunction func = luaMgr.mainLua.GetFunction("ExceptionPanel.show");
                if (func == null) return;

                func.BeginPCall();
                func.Push(this.cutTest);
                func.Push(condition);
                func.Push(stackTrace);
                func.PCall();
                func.EndPCall();
            }
        }

        // When the scene starts trigger the Cutscene if necessary.
        void Start()
        {
            if (cutTest != null)
            {
                hasTriggered = true;
                cutTest.Optimize();
                cutTest.Play();

                cutTest.CutsceneFinished += onCutsceneFinished;
            }
        }

        private void onCutsceneFinished(object sender, CutsceneEventArgs cutsceneEventArgs)
        {
            if (havError) return;

            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
            LuaFunction func = luaMgr.mainLua.GetFunction("TIPLOGIC.PopupTip");
            if (func == null) return;

            func.BeginPCall();
            func.Push("[42c06c]恭喜，您已完成全部的测试流程[-]");
            func.Push("提示");
            func.PCall();
            func.EndPCall();
        }


        private void OnDestroy()
        {
        }
        
    }


}