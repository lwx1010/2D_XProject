-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

MiniMapPanel = {}
local this = MiniMapPanel

--- 由LuaBehaviour自动调用
function MiniMapPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="BtnVerysamll",path="zhujiemian_map.DiTuMoKuai.BtnVerysamll",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll",path="zhujiemian_map.DiTuMoKuai.BtnVerysamll",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="ChangAnCheng",path="zhujiemian_map.DiTuMoKuai.DiBiao.ChangAnCheng",src=LuaText},
		{field="_100,100_",path="zhujiemian_map.DiTuMoKuai.DiBiao._100,100_",src=LuaText},

	}
	LuaUIHelper.bind(this.gameObject , MiniMapPanel )
end

function MiniMapPanel.show( )
    createPanel("Prefab/Gui/main/MiniMapPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function MiniMapPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , MiniMapPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function MiniMapPanel._registeEvents(event)
    
end

--- 关闭界面
function MiniMapPanel._onClickClose( )
    panelMgr:ClosePanel("MiniMapPanel")
end

--- 由LuaBehaviour自动调用
function MiniMapPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function MiniMapPanel.OnDestroy()

end