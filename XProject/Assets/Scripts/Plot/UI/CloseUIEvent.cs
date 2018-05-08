
using CinemaDirector;
using CinemaDirector.Helpers;
using LuaFramework;
using UnityEngine;
/// <summary>
/// 显示指定的界面
/// </summary>
[CutsceneItemAttribute("GUI", "Close UI", CutsceneItemGenre.GlobalItem)]
public class CloseUIEvent : CinemaGlobalEvent, IRevertable
{
    public string UIName;

    // Options for reverting in editor.
    [SerializeField]
    private RevertMode editorRevertMode = RevertMode.Revert;

    // Options for reverting during runtime.
    [SerializeField]
    private RevertMode runtimeRevertMode = RevertMode.Revert;

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

    public override void Trigger()
    {
        if (!Application.isPlaying) return;

        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        panelMgr.ClosePanel(UIName.Replace("Panel" , ""));
    }

    /// <summary>
    /// Reverse this Event and put the GameObject into its' previous state.
    /// </summary>
    public override void Reverse()
    {
        if (!Application.isPlaying) return;

        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        //panelMgr.CreatePanel(UIName);
    }

    public RevertInfo[] CacheState()
    {
        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        return new RevertInfo[] { new RevertInfo(this, panelMgr, "ClosePanel", UIName) };
    }
}
