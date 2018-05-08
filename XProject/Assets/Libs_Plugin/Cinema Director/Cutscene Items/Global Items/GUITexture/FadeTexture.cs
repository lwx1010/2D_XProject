// Cinema Suite

using LuaFramework;
using LuaInterface;
using UnityEngine;
using Riverlake;

namespace CinemaDirector
{
    /// <summary>
    /// An action that fades in a texture over the first 25% of length, shows for 50% of time length
    /// and fades away over the final 25%.
    /// </summary>
    [CutsceneItem("GUI", "Fade", CutsceneItemGenre.GlobalItem)]
    public class FadeTexture : CinemaGlobalAction
    {
        public float from = 1;

        // Optional Tint
        public float to = 0;

        /// <summary>
        /// Trigger this event, enable the texture and make it clear.
        /// </summary>
        public override void Trigger()
        {
            if (!Application.isPlaying) return;

            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
            LuaFunction func = luaMgr.mainLua.GetFunction("FadePanel.show");
            if (func == null) return;

            func.BeginPCall();
            func.Push(from);
            func.Push(to);
            func.Push(this.Duration);
            func.PCall();
            func.EndPCall();
        }

        /// <summary>
        /// End this action and disable the texture.
        /// </summary>
        public override void End()
        {
            if (!Application.isPlaying) return;

            //PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
            PanelManager.Instance.ClosePanel("Fade");
        }
    }
}