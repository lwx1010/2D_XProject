-- @Author: LiangZG
-- @Date:   2017-04-27 11:40:32
-- @Last Modified time: 2017-12-13 14:57:03
-- @Desc :  登录场景切换逻辑


local LoginStage = class("LoginStage" , BaseStage)

function LoginStage:ctor( )
	LoginStage.super.ctor(self , "LoginScene" , "PreLoadingScene")
end

function LoginStage:onEnter( transiter , stageLoader )
    print("-------> LoginState onEnter:" .. self.stageName)

    ITEMLOGIC.Release()
    COMMONCTRL.RemoveQuanQuan()
    --Util.LoadScene("LoginScene")
    --sceneMgr:LoadScene(LoginStage.new("LoginScene"))

    networkMgr:Close()

    --清除任务
    local TaskLogic = require('Logic/TaskLogic')
    TaskLogic.Clear()
    --清除聊天内容
    CHATLOGIC.Clear()

    --清除游戏运行是弹提示是UI
    CtrlManager.GetCtrl(CtrlNames.Main).IsLoadShowTip = nil
    --清除剧情记录
    --cutsceneMgr:ClearAll()
end


function LoginStage:onShow(  )
    CtrlManager.GetCtrl(CtrlNames.Main).reLogin = true

    local ctrl = CtrlManager.GetCtrl(CtrlNames.Login);
    if ctrl ~= nil then
        ctrl:Awake()
    end

    soundMgr:PlayBGM("1005", 1)
    if Game.loginCallback ~= nil then
        Game.loginCallback()
        Game.loginCallback = nil
    end
    HERO.Clear()

    CtrlManager.curIndex = 2
end


function LoginStage:onExit( )
    print("LoginState onExit <----")

    COMMONCTRL.RemoveQuanQuan()
    --删除Login背景
    --panelMgr:Clear()
    --soundMgr:StopBGM()

end

return LoginStage