-- @Author: liangzg
-- @Date:   2017-04-27 11:41:06
-- @Last Modified time: 2018-04-03 16:02:30

local BaseStage = class("BaseStage")

function BaseStage:ctor( stageName , transitSceneName)
    self.stageName = stageName
	self.transitScene = transitSceneName   --过滤场景名称
end


--- 场景舞台进入时调用
-- @param transiter ASceneLoadingTransition  场景过滤策略器
-- @param stageLoader LoadStageAsync 场景加载器
function BaseStage:onEnter ( transiter , stageLoader)

end


--- 场景舞台加载完成后，显示时调用
function BaseStage:onShow( )
    -- body
end

--- 场景舞台退出时调用
function BaseStage:onExit( )
    -- body
end

return BaseStage