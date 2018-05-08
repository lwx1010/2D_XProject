using CinemaDirector;
using LuaFramework;
using LuaInterface;
using UnityEngine;

namespace Assets.Scripts.Plot.UI
{
    /// <summary>
    /// 显示电影幕布界面
    /// </summary>
    [CutsceneItemAttribute("GUI", "Movie Curtain", CutsceneItemGenre.GlobalItem)]
    public class ShowCurtainPanelEvent : CinemaGlobalEvent
    {

        public override void Trigger()
        {
            if (!Application.isPlaying) return;

            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
            LuaFunction func = luaMgr.mainLua.GetFunction("CurtainPanel.show");
            if (func == null) return;

            func.BeginPCall();
            func.PCall();
            func.EndPCall();
        }

        public override void Reverse()
        {
            if (!Application.isPlaying) return;

            PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
            panelMgr.ClosePanel("PlotTalk");
        }
    }
}