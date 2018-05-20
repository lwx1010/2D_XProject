-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

local Network = Network

TestUiPanel = {}
local this = TestUiPanel

--- 由LuaBehaviour自动调用
function TestUiPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="btnClose",path="btnClose",src=LuaButton, onClick = function (gObj)  this._onClickClose() end },
		{field="inputTest",path="cmdInput",src=LuaInput},
		{field="btnSend",path="btnSend",src=LuaButton, onClick = function (gObj)  this.OnSendBtnClick() end },
		{field="btnSend",path="btnPCS",src=LuaButton, onClick = function (gObj)  this.OnPCSClick()  end },
		{field="btnSend",path="btnPCR",src=LuaButton, onClick = function (gObj)  this.OnPCRClick() end },

	}
	LuaUIHelper.bind(this.gameObject , TestUiPanel )
end

function TestUiPanel.show( )
    createPanel("Prefab/Gui/main/TestUiPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function TestUiPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , TestUiPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function TestUiPanel._registeEvents(event)
    
end

--- 关闭界面
function TestUiPanel._onClickClose( )
    panelMgr:ClosePanel("TestUiPanel")
end

--- 由LuaBehaviour自动调用
function TestUiPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function TestUiPanel.OnDestroy()

end

-------------------------------------------------------------------------------------
--
function TestUiPanel.OnSendBtnClick()
	local cmd = {}
	cmd.wizcmd = this.inputTest.text
	print("================", cmd.wizcmd)
	Network.send("C2s_wizcmd", cmd)
end

function TestUiPanel.OnPCSClick()
	Network.PrintCache(1)
end	

function TestUiPanel.OnPCRClick()
	Network.PrintCache(2)
end
