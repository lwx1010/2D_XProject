using System.IO;
using UnityEngine;
using LuaFramework;

public class PlotStartUpCommand : ControllerCommand
{

    private LuaManager luaMgr;

    public override void Execute(IMessage message) {

        //-----------------关联命令-----------------------
        PlotFacade.Instance.RegisterCommand(NotiConst.DISPATCH_MESSAGE, typeof(SocketCommand));

        //-----------------初始化管理器-----------------------
        luaMgr = PlotFacade.Instance.AddManager<LuaManager>();
        PlotFacade.Instance.AddManager<SoundManager>();
        PlotFacade.Instance.AddManager<TimerManager>();
        PlotFacade.Instance.AddManager<NetworkManager>();
        PlotFacade.Instance.AddManager<ThreadManager>();
        //        AppFacade.Instance.AddManager<GameManager>();
        PlotFacade.Instance.AddManager<SceneStageManager>();
        //PlotFacade.Instance.AddManager<CutsceneManager>();

        LuaStart();
    }

    private void LuaStart()
    {
        luaMgr.InitStart(() => {
            luaMgr.DoFile("Logic/Network");         //加载网络
            luaMgr.DoFile("Common/define");
            luaMgr.DoFile("Logic/Game");            //加载游戏

            Util.CallMethod("Game", "OnInitOK");          //初始化完成
            RegisterHandlers();                         //注册协议处理
        });
    }

    public void RegisterHandlers()
    {
        string handlePath = "";
        if (AppConst.LuaBundleMode)
            handlePath = Util.DataPath + "lua/protocol/handlers.txt";
        else
            handlePath = AppConst.FrameworkRoot + "/Lua/protocol/handlers.txt";
        string text = File.ReadAllText(handlePath);
        string[] handlers = text.Split(';');
        for (int i = 0; i < handlers.Length; ++i)
        {
            if (string.IsNullOrEmpty(handlers[i]))
                continue;
            string fileName = string.Format("protocol/handler/{0}", handlers[i]);
            luaMgr.DoFile(fileName);
        }
    }
}