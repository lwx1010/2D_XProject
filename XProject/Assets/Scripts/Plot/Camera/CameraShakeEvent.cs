using DG.Tweening;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Enable the Actor related to this event.
    /// </summary>
    [CutsceneItemAttribute("Animation", "Camera Shake", CutsceneItemGenre.ActorItem)]
    public class CameraShakeEvent : CinemaActorAction
    {
        //The shake strength on each axis
        public Vector3 strength = Vector3.one;

        //Indicates how much will the shake vibrate
        public int vibrato = 50;

        //Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware).
        [Range(0 , 180)]
        public float randomness = 90; 
        public override void SetDefaults()
        {
            base.SetDefaults();
            this.duration = 0.5f;
        }

        /// <summary>
        /// Enable the given actor.
        /// </summary>
        /// <param name="actor">The actor to be enabled.</param>
        public override void Trigger(GameObject actor)
        {
            if (actor != null)
            {
                actor.transform.DOShakePosition(Duration , strength, vibrato, randomness, false , false);
            }
        }

        public override void End(GameObject Actor)
        {
            
        }
    }
}