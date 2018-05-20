-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

MainUiTaskPanel = {}
local this = MainUiTaskPanel

--- 由LuaBehaviour自动调用
function MainUiTaskPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {

	}
	LuaUIHelper.bind(this.gameObject , MainUiTaskPanel )
end

function MainUiTaskPanel.show( )
    createPanel("Prefab/Gui/main/MainUiTaskPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function MainUiTaskPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , MainUiTaskPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function MainUiTaskPanel._registeEvents(event)
    
end

--- 关闭界面
function MainUiTaskPanel._onClickClose( )
    panelMgr:ClosePanel("MainUiTaskPanel")
end

--- 由LuaBehaviour自动调用
function MainUiTaskPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function MainUiTaskPanel.OnDestroy()

end