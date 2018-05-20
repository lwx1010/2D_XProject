-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

EnemyPlayerHeadPanel = {}
local this = EnemyPlayerHeadPanel

--- 由LuaBehaviour自动调用
function EnemyPlayerHeadPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="WanJiaTouXiang",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="KuangShiQiCaiChenDuX",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.KuangShiQiCaiChenDuX",src=LuaText},
		{field="116",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.DengJi.116",src=LuaText},
		{field="WanJiaTouXiang",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="KuangShiQiCaiChenDuX",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.KuangShiQiCaiChenDuX",src=LuaText},
		{field="116",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.DengJi.116",src=LuaText},
		{field="WanJiaTouXiang",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="KuangShiQiCaiChenDuX",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.KuangShiQiCaiChenDuX",src=LuaText},
		{field="116",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang.DengJi.116",src=LuaText},
		{field="WanJiaTouXiang",path="zhujiemian_diduiwanj.DiDuiWanJia.WanJiaTouXiang",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TiShi",path="zhujiemian_diduiwanj.DiDuiWanJia.TiShi",src="GameObject"},

	}
	LuaUIHelper.bind(this.gameObject , EnemyPlayerHeadPanel )
end

function EnemyPlayerHeadPanel.show( )
    createPanel("Prefab/Gui/main/EnemyPlayerHeadPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function EnemyPlayerHeadPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , EnemyPlayerHeadPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function EnemyPlayerHeadPanel._registeEvents(event)
    
end

--- 关闭界面
function EnemyPlayerHeadPanel._onClickClose( )
    panelMgr:ClosePanel("EnemyPlayerHeadPanel")
end

--- 由LuaBehaviour自动调用
function EnemyPlayerHeadPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function EnemyPlayerHeadPanel.OnDestroy()

end