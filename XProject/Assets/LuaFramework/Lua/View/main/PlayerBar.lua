-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

PlayerBar = {}
local this = PlayerBar

--- 由LuaBehaviour自动调用
function PlayerBar.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="17803Wan",path="zhujiemian_touxiang.WanJiaTouXiang.ZhanDouLi.17803Wan",src=LuaText},
		{field="116",path="zhujiemian_touxiang.WanJiaTouXiang.DengJiXinXi.116",src=LuaText},
		{field="V",path="zhujiemian_touxiang.WanJiaTouXiang.VIPXinXi.V",src=LuaText},
		{field="12",path="zhujiemian_touxiang.WanJiaTouXiang.VIPXinXi.12",src=LuaText},
		{field="BtnVerysamll",path="zhujiemian_touxiang.WanJiaTouXiang.VIPXinXi.BtnVerysamll",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="ChongWuTouXiang",path="zhujiemian_touxiang.ChongWuTouXiang",src="GameObject"},
		{field="bg",path="zhujiemian_touxiang.ChongWuTouXiang.bg",src=LuaImage},
		{field="116",path="zhujiemian_touxiang.ChongWuTouXiang.DengJi.116",src=LuaText},
		{field="bossTouXiang",path="zhujiemian_touxiang.bossTouXiang",src="GameObject"},
		{field="bg",path="zhujiemian_touxiang.bossTouXiang.bg",src=LuaImage},
		{field="bg",path="zhujiemian_touxiang.bossTouXiang.bg",src=LuaImage},
		{field="WuDuJiaoZhu90Ji",path="zhujiemian_touxiang.bossTouXiang.WuDuJiaoZhu90Ji",src=LuaText},

	}
	LuaUIHelper.bind(this.gameObject , PlayerBar )
end

function PlayerBar.show( )
    createPanel("Prefab/Gui/main/PlayerBar", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function PlayerBar.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , PlayerBar)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function PlayerBar._registeEvents(event)
    
end

--- 关闭界面
function PlayerBar._onClickClose( )
    panelMgr:ClosePanel("PlayerBar")
end

--- 由LuaBehaviour自动调用
function PlayerBar.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function PlayerBar.OnDestroy()

end