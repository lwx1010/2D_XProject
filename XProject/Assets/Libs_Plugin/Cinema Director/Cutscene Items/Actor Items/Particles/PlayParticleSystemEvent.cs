using LuaFramework;
using UnityEngine;
using AL.Resources;

namespace CinemaDirector
{
    /// <summary>
    /// Enable the Actor related to this event.
    /// </summary>
    [CutsceneItemAttribute("Particle System", "Play", CutsceneItemGenre.ActorItem)]
    public class PlayParticleSystemEvent : CinemaActorEvent
    {
        /// <summary>
        /// Trigger the particle system to play.
        /// </summary>
        /// <param name="actor">The actor to be triggered.</param>
        public override void Trigger(GameObject actor)
        {
            if (this.ActorTrackGroup.ActorTrackType == ActorTrackGroup.ActorType.Dynamic)
            {
                if (Application.isPlaying && actor.transform.childCount == 0)
                {
                    GameObject newObj = ResourceManager.LoadPrefabBundle(this.ActorTrackGroup.AssetPath.Replace(".prefab" , ""));
                    newObj = GameObject.Instantiate(newObj) as GameObject;
                    ////Util.SetParent(newObj , actor);   
                }
            }

            if (actor != null)
            {
                actor.SetActive(true);

                ParticleSystem ps = actor.GetComponentInChildren<ParticleSystem>(true);
                if (ps != null)
                {
                    ps.Play(true);
                }
            }
        }

        /// <summary>
        /// Reverse this event and stop the particle system.
        /// </summary>
        /// <param name="actor">The actor to reverse this event on.</param>
        public override void Reverse(GameObject actor)
        {
            if (actor != null)
            {
                if (actor.transform.childCount > 0)
                    actor.transform.GetChild(0).gameObject.SetActive(false);

                ParticleSystem ps = actor.GetComponentInChildren<ParticleSystem>(true);
                if (ps != null)
                {
                    ps.Stop(true);
                }
            }
        }
    }
}