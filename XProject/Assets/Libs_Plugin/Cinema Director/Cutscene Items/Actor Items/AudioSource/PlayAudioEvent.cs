using UnityEngine;
using System.Collections;
using System.IO;
using LuaFramework;

namespace CinemaDirector
{
//    [CutsceneItemAttribute("Audio Source", "Play Audio", CutsceneItemGenre.ActorItem)]
    public class PlayAudioEvent : CinemaActorAction
    {
        public bool loop = false;

        private bool wasPlaying = false;



        public override void Trigger(GameObject Actor)
        {
            if (Application.isPlaying)
            {
                SoundManager soundMgr = AppFacade.Instance.GetManager<SoundManager>();
                soundMgr.PlaySound(this.ActorTrackGroup.AssetPath, 0, loop);
                return;
            }

            AudioSource audio = Actor.GetComponentInChildren<AudioSource>();
            if (!audio) return;

            audio.playOnAwake = false;

            audio.time = 0.0f;
            audio.loop = loop;
            audio.Play();
        }

        public override void UpdateTime(GameObject Actor, float runningTime, float deltaTime)
        {
            AudioSource audio = Actor.GetComponentInChildren<AudioSource>();
            if (!audio) return;

            if (audio.isPlaying)
                return;

            audio.time = deltaTime;


            audio.Play();

        }

        public override void Resume(GameObject Actor)
        {
            AudioSource audio = Actor.GetComponentInChildren<AudioSource>();
            if (!audio)
                return;

            audio.time = Cutscene.RunningTime - Firetime;

            if (wasPlaying)
                audio.Play();
        }

        public override void Pause(GameObject Actor)
        {
            AudioSource audio = Actor.GetComponentInChildren<AudioSource>();

            wasPlaying = false;
            if (audio && audio.isPlaying)
                wasPlaying = true;

            if (audio)
                audio.Pause();
        }

        public override void End(GameObject Actor)
        {
            AudioSource audio = Actor.GetComponentInChildren<AudioSource>();
            if (audio)
                audio.Stop();
        }

    }

}