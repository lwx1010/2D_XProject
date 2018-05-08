using UnityEngine;
using System.Collections;

namespace CinemaDirector
{
    [CutsceneItemAttribute("Animator", "Play Mecanim Animation", CutsceneItemGenre.ActorItem, CutsceneItemGenre.MecanimItem)]
    public class PlayAnimatorEvent : CinemaActorAction
    {
        public string StateName;
        public int Layer = -1;
        float Normalizedtime = float.NegativeInfinity;

        private AnimationClip animationClip = null;

        public override void Trigger(GameObject actor)
        {
            Animator animator = actor.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return;
            }

            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            int frameCount = 0;
            for (int i = 0; i < ac.animationClips.Length; i++)
            {
                AnimationClip clip = ac.animationClips[i];
                if (clip.name == StateName)
                {
                    animationClip = clip;
                    frameCount = (int)(clip.length * clip.frameRate);
                    break;
                }
            }

            if (animationClip == null)
            {
                Debug.LogError("Cant find Animation ! Animation name is " + (string.IsNullOrEmpty(StateName) ? "Null" : StateName) 
                    + " in " + animator.gameObject.name);
                return;
            }
            actor.SetActive(true);
            animator.Play(StateName, Layer, Normalizedtime);
            animator.Update(0);

            if (!Application.isPlaying)
            {
                animator.Rebind();
                animator.StopPlayback();
                animator.recorderStartTime = 0;

                animator.StartRecording(frameCount);
                const float frameRate = 1.0f / 30;
                for (int i = 0; i < frameCount; i++)
                {
                    animator.Update(frameRate);  //��¼ÿһ֡
                }
                animator.StopRecording();
                Debug.LogFormat("Recording:{0},{1}", animator.recorderStartTime, animator.recorderStopTime);

                animator.StartPlayback();                
            }     
        }

//        public void Update()
//        {
//            if (animationClip == null) return;
//            // Loop clips can be of any length, other wrap modes can not be longer than clip, but can be shorter
//            if (!animationClip.isLooping)
//            {
//                if (base.Duration > animationClip.length)
//                {
//                    base.Duration = animationClip.length;
//                }
//            }
//        }


        public override void UpdateTime(GameObject Actor, float runningTime, float deltaTime)
        {
            Animator animation = Actor.GetComponentInChildren<Animator>();

            if (!animation)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                animation.playbackTime = runningTime;
                animation.Update(0);
            }else if (!animation.GetCurrentAnimatorStateInfo(0).IsName(StateName))
            {
                animation.Play(StateName, Layer, Normalizedtime);
                animation.Update(0);
            }
        }

        public override void End(GameObject Actor)
        {
            Animator animator = Actor.GetComponentInChildren<Animator>();
            if (animator && !Application.isPlaying)
            {
                animator.StopPlayback();
                //animation.Play("idle");
            }
            //animator.Stop();
            animationClip = null;
        }



    }
}