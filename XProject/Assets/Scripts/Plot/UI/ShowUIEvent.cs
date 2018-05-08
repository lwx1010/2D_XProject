using CinemaDirector;
using CinemaDirector.Helpers;
using LuaFramework;
using LuaInterface;
using UnityEngine;


/// <summary>
/// 显示指定的界面
/// </summary>
[CutsceneItemAttribute("GUI", "Show UI", CutsceneItemGenre.GlobalItem)]
public class ShowUIEvent : CinemaGlobalEvent, IRevertable
{
    public string UIName;

    public string Args;
    //  The cutscene will be pause when open ui 
    public bool PauseCutscene = true;

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

        if(PauseCutscene)   this.Cutscene.Pause();

        LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        LuaFunction func = luaMgr.mainLua.GetFunction(UIName + ".show");
        if (func == null) return;

        func.BeginPCall();
        func.Push(this.Cutscene);
        if(!string.IsNullOrEmpty(Args))
            func.Push(Args);
        func.PCall();
        func.EndPCall();
    }


    /// <summary>
    /// Reverse this Event and put the ui GameObject into its' previous state.
    /// </summary>
    public override void Reverse()
    {
        if (!Application.isPlaying) return;

        if (PauseCutscene) this.Cutscene.Play();

        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        panelMgr.ClosePanel(UIName.Replace("Panel", ""));
    }

    public RevertInfo[] CacheState()
    {
        return null;
    }
}
