-- @Author: luweixing
-- @Date:   2017年5月11日 14:58:15
-- @Last Modified time: 2017年5月11日 14:58:24
-- @Desc :  游戏场景跳回登陆界面前的场景，用来重置luastate


local PreLoginStage = class("PreLoginStage" , BaseStage)

function PreLoginStage:ctor( )
    PreLoginStage.super.ctor(self , "PreLoginScene" , "")
end

function PreLoginStage:onEnter (transiter, stageLoader)
    print("-------> PreLoginStage onEnter:" .. self.stageName)
end


function PreLoginStage:onShow(  )
    gameMgr:Restart()
end


function PreLoginStage:onExit( )
    
end

return PreLoginStage