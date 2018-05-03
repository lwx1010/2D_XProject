using System.Runtime.Remoting;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// A timeline action that has some paired object that has a fixed length.
    /// This is ideal for items like Audio clips and Animation clips.
    /// </summary>
    public abstract class TimelineActionFixed : TimelineAction
    {
        [SerializeField] private float inTime = 0f;
        [SerializeField] private float outTime = 1f;
        [SerializeField] private float itemLength = 0f;

        public float InTime
        {
            get { return inTime; }
            set
            {
                inTime = value;
                Duration = outTime - inTime;
            }
        }

        public float OutTime
        {
            get { return outTime; }
            set
            {
                outTime = value;
                Duration = outTime - inTime;
            }
        }

        public float ItemLength
        {
            get { return itemLength; }
            set { itemLength = value; }
        }

        /// <summary>
        /// Called when the running time of the cutscene hits the firetime of the action.
        /// </summary>
        /// <remarks>
        /// <c>Trigger()</c> is only called when the cutscene hits the firetime while moving forward, either through regular playback or by scrubbing. For when the playback is reversed, see <see cref="ReverseTrigger"/>.
        /// </remarks>
        public abstract void Trigger();


        /// <summary>
        /// Pause any action as necessary. Called when the cutscene is paused.
        /// </summary>
        public virtual void Pause() { }


        /// <summary>
        /// Resume from paused.  Called when the cutscene is unpaused.
        /// </summary>
        public virtual void Resume() { }
        /// <summary>
        /// Called when the running time of the cutscene exceeds the duration of the action
        /// </summary>
        /// <remarks>
        /// <c>End()</c> is only called when the cutscene hits the endtime while moving forward, either through regular playback or by scrubbing. For when the playback is reversed, see <see cref="ReverseEnd"/>.
        /// </remarks>
        public abstract void End();

        /// <summary>
        /// Called at each update when the action is to be played.
        /// </summary>
        /// <param name="time">The new running time of the action.</param>
        /// <param name="deltaTime">The deltaTime since the last update call.</param>
        public virtual void UpdateTime(float time, float deltaTime) { }

        public virtual void SetTime(float audioTime)
        {
        }
    }
}