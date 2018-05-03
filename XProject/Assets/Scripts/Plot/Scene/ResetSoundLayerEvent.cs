using LuaFramework;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// 重置音频层
    /// </summary>
    [CutsceneItem("Audio", "Reset Sound Layer", CutsceneItemGenre.GlobalItem)]
    public class ResetSoundLayerEvent : CinemaGlobalEvent
    {
        /// <summary>
        /// 音频层
        /// </summary>
        public int SoundLayer = -1;

        public override void Trigger()
        {
            SoundManager soundMgr = AppFacade.Instance.GetManager<SoundManager>();
            soundMgr.SetActiveLayer(SoundLayer , true);
        }

    }
}