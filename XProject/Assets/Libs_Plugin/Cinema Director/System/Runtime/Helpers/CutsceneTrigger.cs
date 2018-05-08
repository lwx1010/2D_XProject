using System;
using System.Collections;
using LuaFramework;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// A sample behaviour for triggering Cutscenes.
    /// </summary>
    public class CutsceneTrigger : MonoBehaviour
    {
        public StartMethod StartMethod;
        public string CutName;
        public bool Loop;  //every trigger is true while be play
//        public string SkipButtonName = "Jump";
        public float Delay = 0f;
        public string TriggerValue;
        private const string TriggerTag = "Self";
        
        // Event fired when Cutscene's runtime reaches it's duration.
        public event CutsceneHandler CutsceneFinished;

        // Event fired when Cutscene has been paused.
        public event CutsceneHandler CutscenePaused;

        /// <summary>
        /// When the trigger is loaded, optimize the Cutscene.
        /// </summary>
        void Awake()
        {
            CutsceneManager cutsceneMgr = AppFacade.Instance.GetManager<CutsceneManager>();
            if(cutsceneMgr != null) cutsceneMgr.AddTrigger(this);  
        }

        // When the scene starts trigger the Cutscene if necessary.
        void Start()
        {
            if (StartMethod == StartMethod.OnStart)
            {
                CutsceneManager cutsceneMgr = AppFacade.Instance.GetManager<CutsceneManager>();
                if (cutsceneMgr != null)
                    cutsceneMgr.Trigger(Convert.ToInt32(CutName));
            }
        }

//        void Update()
//        {
//            if (SkipButtonName != null || SkipButtonName != string.Empty)
//            {
//                // Check if the user wants to skip.
//                if (Input.GetButtonDown(SkipButtonName))
//                {
//                    if (Cutscene != null && Cutscene.State == CinemaDirector.Cutscene.CutsceneState.Playing)
//                    {
//                        Cutscene.Skip();
//                    }
//                }
//            }
//        }


        /// <summary>
        /// If Cutscene is setup to play on trigger, watch for the trigger event.
        /// </summary>
        /// <param name="other">The other collider.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == TriggerTag)
            {
                CutsceneManager cutsceneMgr = AppFacade.Instance.GetManager<CutsceneManager>();
                if (cutsceneMgr != null)
                    cutsceneMgr.Trigger(Convert.ToInt32(CutName));
            }
        }

        private void OnDestroy()
        {
            CutsceneManager cutsceneMgr = AppFacade.Instance.GetManager<CutsceneManager>();
            if (cutsceneMgr != null) cutsceneMgr.RemoveTrigger(this);
        }
        
    }

    public enum StartMethod
    {
        OnStart,
        OnTrigger,
        None
    }


}