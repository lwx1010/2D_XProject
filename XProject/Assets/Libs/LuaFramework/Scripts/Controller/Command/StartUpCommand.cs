using UnityEngine;
using System.Collections;
using LuaFramework;

public class StartUpCommand : ControllerCommand {

    public override void Execute(IMessage message) {

        GameObject gameMgr = GameObject.Find("GlobalGenerator");
        if (gameMgr != null) {
            AppView appView = gameMgr.AddComponent<AppView>();
        }
        //-----------------关联命令-----------------------
        AppFacade.Instance.RegisterCommand(NotiConst.DISPATCH_MESSAGE, typeof(SocketCommand));

        //-----------------初始化管理器-----------------------
        AppFacade.Instance.AddManager<ObjectPoolManager>();
        AppFacade.Instance.AddManager<LuaManager>();
        AppFacade.Instance.AddManager<SoundManager>();
        AppFacade.Instance.AddManager<TimerManager>();
        AppFacade.Instance.AddManager<NetworkManager>();
        AppFacade.Instance.AddManager<ThreadManager>();
        AppFacade.Instance.AddManager<GameManager>();
        AppFacade.Instance.AddManager<SceneStageManager>();
        //AppFacade.Instance.AddManager<CutsceneManager>();
        //AppFacade.Instance.AddManager<ApkManager>();
    }
}