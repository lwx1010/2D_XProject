using System.Collections;
using System.IO;
using LuaFramework;
using LuaInterface;
using RSG;
using UnityEngine;
using AL.Resources;

namespace CinemaDirector
{
    /// <summary>
    /// The ActorTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("Actor Track Group", TimelineTrackGenre.ActorTrack)]
    public class ActorTrackGroup : TrackGroup
    {
        public enum ActorType
        {
            Static,
            Dynamic
        }

        [SerializeField] protected ActorType actorType = ActorType.Static;

        [SerializeField] protected Transform actor;

        [SerializeField] protected string assetPath;

        private bool isLoaded;
        public ActorType ActorTrackType
        {
            get { return actorType; }
        }

        public string AssetPath
        {
            get { return assetPath; }
        }

        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public virtual Transform Actor
        {
            get { return actor; }
            set { actor = value; }
        }

        private static string EffectPath = "Prefab/Other/Gen";

        public override void Optimize()
        {
            base.Optimize();

            if (actorType == ActorType.Dynamic && assetPath.CustomEndsWith(".prefab") && Application.isPlaying)
            {

                Transform actorTrans = transform.Find("_Entity");

                if (actorTrans == null)
                {
                    GameObject actorObj = new GameObject("_Entity");
                    ////Util.SetParent(actorObj, this.gameObject);
                    actor = actorObj.transform;
                }

                if (!isLoaded && !assetPath.CustomStartsWith(EffectPath))
                {
                    isLoaded = true;
                    //ResourceManager resMgr = AppFacade.Instance.GetManager<ResourceManager>();
                    //resMgr.StartCoroutine(loadActor());
                }
            }
        }


        //private IEnumerator loadActor()
        //{
        //    ResourceManager resMgr = AppFacade.Instance.GetManager<ResourceManager>();
        //    ALoadOperation operation = ResourceManager.LoadAssetAsync(assetPath.Replace(".prefab", ""));
        //    yield return resMgr.StartCoroutine(operation);

        //    GameObject resObj = operation.GetAsset<GameObject>();
        //    if (resObj == null)
        //    {
        //        Debugger.LogWarning("Cant load actor resource! " + assetPath);
        //        yield break;
        //    }
        //    GameObject instanceObj = GameObject.Instantiate(resObj) as GameObject;
        //    ////Util.SetParent(instanceObj, this.Actor.gameObject);
        //    instanceObj.SetActive(true);

        //    PrefabLoader prefabLoader = instanceObj.GetComponentInChildren<PrefabLoader>();
        //    if(prefabLoader)    prefabLoader.SetEffectLayer("Plot");

        //    Animator animator = instanceObj.GetComponent<Animator>();
        //    if (animator) animator.enabled = true;

        //    EntityTrackGroup.AddFastShadow(instanceObj);

        //}
    }
}