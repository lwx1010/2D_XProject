using System.Collections.Generic;
using CinemaDirector.Helpers;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Detaches all children in hierarchy from this Parent.
    /// </summary>
    [CutsceneItemAttribute("Common", "Set Position", CutsceneItemGenre.ActorItem)]
    public class SetPositionEvent : CinemaActorEvent, IRevertable
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        /// <summary>
        /// Cache the state of all actors related to this event.
        /// </summary>
        /// <returns></returns>
        public RevertInfo[] CacheState()
        {
            List<Transform> actors = new List<Transform>(GetActors());
            List<RevertInfo> reverts = new List<RevertInfo>();
            foreach (Transform go in actors)
            {
                if (go != null)
                {
                    Transform t = go.GetComponent<Transform>();
                    if (t != null)
                    {
                        reverts.Add(new RevertInfo(this, t, "position", t.position));
                        reverts.Add(new RevertInfo(this, t, "rotation", t.rotation));
                        reverts.Add(new RevertInfo(this, t, "localScale", t.localScale));
                    }
                }
            }

            return reverts.ToArray();
        }

        public override void Trigger(GameObject actor)
        {
            if (actor != null)
            {
                actor.transform.position = Position;
                actor.transform.rotation = Quaternion.Euler(Rotation);
                actor.transform.localScale = Scale;
            }
        }

        public override void Reverse(GameObject actor)
        {
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Editor.
        /// </summary>
        public RevertMode EditorRevertMode
        {
            get { return editorRevertMode; }
            set { editorRevertMode = value; }
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Runtime.
        /// </summary>
        public RevertMode RuntimeRevertMode
        {
            get { return runtimeRevertMode; }
            set { runtimeRevertMode = value; }
        }
    }
}