using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The UTestTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("UTest Track Group", TimelineTrackGenre.UTestTrack)]
    public class UTestTrackGroup : TrackGroup
    {
        
        [SerializeField] protected Transform actor;

        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public virtual Transform Actor
        {
            get { return actor; }
            set { actor = value; }
        }

        public override void Initialize()
        {
            base.Initialize();

        }
    }
}