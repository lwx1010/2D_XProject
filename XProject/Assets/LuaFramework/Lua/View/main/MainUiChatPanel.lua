-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

MainUiChatPanel = {}
local this = MainUiChatPanel

--- 由LuaBehaviour自动调用
function MainUiChatPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="btnChatWnd",path="zhujiemian_chat.LiaoTianMoKuai.LiaoTian",src=LuaButton, onClick = function (gObj) this.OnChatWndClick() end },
		{field="HaoYou",path="zhujiemian_chat.LiaoTianMoKuai.HaoYou",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="YuYinZhuangTai",path="zhujiemian_chat.LiaoTianMoKuai.YuYinZhuangTai",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
	}
	LuaUIHelper.bind(this.gameObject , MainUiChatPanel )
end

function MainUiChatPanel.show( )
    createPanel("Prefab/Gui/main/MainUiChatPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function MainUiChatPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , MainUiChatPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function MainUiChatPanel._registeEvents(event)
    
end

--- 关闭界面
function MainUiChatPanel._onClickClose( )
    panelMgr:ClosePanel("MainUiChatPanel")
end

--- 由LuaBehaviour自动调用
function MainUiChatPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function MainUiChatPanel.OnDestroy()

end

------------------------------------------------------
--点击
function MainUiChatPanel.OnChatWndClick()
	print("======MainUiChatPanel========", TestUiPanel)
	TestUiPanel.show()
end