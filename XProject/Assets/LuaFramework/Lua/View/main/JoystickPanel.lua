-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

JoystickPanel = {}
local this = JoystickPanel

--- 由LuaBehaviour自动调用
function JoystickPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {

	}
	LuaUIHelper.bind(this.gameObject , JoystickPanel )
end

function JoystickPanel.show( )
    createPanel("Prefab/Gui/main/JoystickPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function JoystickPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , JoystickPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function JoystickPanel._registeEvents(event)
    
end

--- 关闭界面
function JoystickPanel._onClickClose( )
    panelMgr:ClosePanel("JoystickPanel")
end

--- 由LuaBehaviour自动调用
function JoystickPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function JoystickPanel.OnDestroy()

end