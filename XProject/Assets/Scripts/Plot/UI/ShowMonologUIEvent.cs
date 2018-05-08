using System;
using CinemaDirector;
using CinemaDirector.Helpers;
using LuaFramework;
using LuaInterface;
using UnityEngine;


/// <summary>
/// 显示与模型有关的对话界面
/// </summary>
[CutsceneItemAttribute("GUI", "Show Monolog UI", CutsceneItemGenre.ActorItem)]
public class ShowMonologUIEvent : CinemaActorAction
{

    public string TalkID;

    public EDirection Dir;

    private string uiName = "MonologPanel";
    private ShotTrack shotTrack;
    private LuaFunction updateTimeFunc;
    private double handler;

    public override void Trigger(GameObject actor)
    {
        if (!Application.isPlaying) return;

        shotTrack = Cutscene.GetComponentInChildren<ShotTrack>();

        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        handler = TimeManager.TotalMilliSeconds(DateTime.Now);

        //if (!panelMgr.IsPanelVisible(uiName))
        //{
        //    LuaFunction func = luaMgr.mainLua.GetFunction(uiName + ".show");
        //    if (func == null) return;

        //    func.BeginPCall();
        //    func.Push(handler);
        //    func.Push(actor);
        //    func.Push(TalkID);
        //    func.Push((int)Dir);
        //    func.PCall();
        //    func.EndPCall();
        //}
        //else
        //{
        //    LuaFunction func = luaMgr.mainLua.GetFunction(uiName + ".addItem");
        //    if (func == null) return;

        //    func.BeginPCall();
        //    func.Push(handler);
        //    func.Push(actor);
        //    func.Push(TalkID);
        //    func.Push((int)Dir);
        //    func.PCall();
        //    func.EndPCall();
        //}
        //updateTimeFunc = luaMgr.mainLua.GetFunction(uiName + ".updateTime");
    }

    public override void UpdateTime(GameObject Actor, float time, float deltaTime)
    {
        if (!Application.isPlaying || updateTimeFunc == null) return;

        updateTimeFunc.BeginPCall();
        updateTimeFunc.Push(handler);
        updateTimeFunc.Push(shotTrack.CurrentCamera);
        updateTimeFunc.PCall();
        updateTimeFunc.EndPCall();
    }

    public override void End(GameObject Actor)
    {
        if (!Application.isPlaying) return;

        LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        LuaFunction func = luaMgr.mainLua.GetFunction(uiName + ".onCloseHandler");
        if (func == null) return;

        func.BeginPCall();
        func.Push(handler);
        func.PCall();
        func.EndPCall();
    }


    public enum EDirection
    {
        Right = 0, Left
    }
}
