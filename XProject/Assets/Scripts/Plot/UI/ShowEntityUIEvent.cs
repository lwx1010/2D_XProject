using System;
using CinemaDirector;
using CinemaDirector.Helpers;
using LuaFramework;
using LuaInterface;
using UnityEngine;


/// <summary>
/// 显示与场景实体相关的界面
/// </summary>
[CutsceneItemAttribute("GUI", "Show Entity UI", CutsceneItemGenre.ActorItem)]
public class ShowEntityUIEvent : CinemaActorAction
{
    public string UIName;

    public string Args;

    public ShowMonologUIEvent.EDirection Direction;

    private ShotTrack shotTrack;
    private LuaFunction updateTimeFunc;
    private double handler;
    
    public override void Trigger(GameObject actor)
    {
        if (!Application.isPlaying) return;

        shotTrack = Cutscene.GetComponentInChildren<ShotTrack>();

        //PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        //LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        //handler = TimeManager.TotalMilliSeconds(DateTime.Now);

        //if (!panelMgr.IsPanelVisible(UIName))
        //{
        //    LuaFunction func = luaMgr.mainLua.GetFunction(UIName + ".show");
        //    if (func == null) return;

        //    func.BeginPCall();
        //    func.Push(handler);
        //    func.Push(actor);
        //    func.Push((int)Direction);
        //    if(!string.IsNullOrEmpty(Args))
        //        func.Push(Args);
        //    func.PCall();
        //    func.EndPCall();
        //}
        //else
        //{
        //    LuaFunction func = luaMgr.mainLua.GetFunction(UIName + ".addItem");
        //    if (func == null) return;

        //    func.BeginPCall();
        //    func.Push(handler);
        //    func.Push(actor);
        //    func.Push((int)Direction);
        //    if (!string.IsNullOrEmpty(Args))
        //        func.Push(Args);
        //    func.PCall();
        //    func.EndPCall();
        //}
        //updateTimeFunc = luaMgr.mainLua.GetFunction(UIName + ".updateTime");
    }

    public override void UpdateTime(GameObject Actor, float time, float deltaTime)
    {
        if (!Application.isPlaying || updateTimeFunc == null ) return;
        
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
        LuaFunction func = luaMgr.mainLua.GetFunction(UIName + ".onCloseHandler");
        if (func == null) return;

        func.BeginPCall();
        func.Push(handler);
        func.PCall();
        func.EndPCall();
    }

}
