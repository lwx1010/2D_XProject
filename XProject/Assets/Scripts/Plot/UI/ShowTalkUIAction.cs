using UnityEngine;
using CinemaDirector;
using LuaFramework;
using LuaInterface;

/// <summary>
/// 屏幕对话UI ，可用于人型、非人型实体对象
/// </summary>
[CutsceneItemAttribute("GUI", "Talk UI", CutsceneItemGenre.GlobalItem)]
public class ShowTalkUIAction : CinemaGlobalEvent
{
    /// <summary>
    /// 起启的对话ID
    /// </summary>
    public string TalkID;
    
    public override void Trigger()
    {
        this.Cutscene.Pause();

        if (!Application.isPlaying)
        {
            return;
        }
        LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        LuaFunction func = luaMgr.mainLua.GetFunction("PlotTalkPanel.show");
        if (func == null) return;

        func.BeginPCall();
        func.Push(TalkID);
        func.Push(this.Cutscene);
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
