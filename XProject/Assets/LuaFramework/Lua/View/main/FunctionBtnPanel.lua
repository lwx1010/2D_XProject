-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

FunctionBtnPanel = {}
local this = FunctionBtnPanel

--- 由LuaBehaviour自动调用
function FunctionBtnPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="GongNengZhanKai",path="zhujiemian_icons.GongNengZhanKai",src="GameObject"},
		{field="BangPai",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.BangPai",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="JiNeng",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.JiNeng",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="JiNeng",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.JiNeng",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="JiNeng",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.JiNeng",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="ChongWu",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.ChongWu",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TuJian",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.TuJian",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TuJian",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.TuJian",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TuJian",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.TuJian",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TuJian",path="zhujiemian_icons.GongNengZhanKai.GongNengRuKou.TuJian",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="HuoDongRuKou",path="zhujiemian_icons.GongNengTuBiao.HuoDongRuKou",src="GameObject"},
		{field="bossRuKou",path="zhujiemian_icons.GongNengTuBiao.bossRuKou",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BeiBao",path="zhujiemian_icons.GongNengTuBiao.BeiBao",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="TiSheng",path="zhujiemian_icons.GongNengTuBiao.TiSheng",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="FuBen",path="zhujiemian_icons.GongNengTuBiao.FuBen",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="8:00KaiQi",path="zhujiemian_icons.GongNengTuBiao.FuBen.8:00KaiQi",src=LuaText},
		{field="FuBen",path="zhujiemian_icons.GongNengTuBiao.FuBen",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="8:00KaiQi",path="zhujiemian_icons.GongNengTuBiao.FuBen.8:00KaiQi",src=LuaText},
		{field="ChongZhi",path="zhujiemian_icons.CiJiRuKou.ChongZhi",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="QiangHua",path="zhujiemian_icons.CiJiRuKou.QiangHua",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="PaiXing",path="zhujiemian_icons.CiJiRuKou.PaiXing",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="QieHuanAnNiu",path="zhujiemian_icons.QieHuanAnNiu",src=LuaButton, onClick = function (gObj)  this.OnSwapBtnClicked(gObj)  end },
		---custom extendsion
		{field="BtnVerysamll",path="zhujiemian_icons.CiJiRuKou.BtnVerysamll",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="SwapBtn",path="zhujiemian_icons.SwapBtn",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll1",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll1",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll2",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll2",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll3",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll3",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll4",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll4",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll5",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll5",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll6",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll6",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll7",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll7",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },
		{field="BtnVerysamll8",path="_root_.GongNengZhanKai.GongNengRuKou.BtnVerysamll8",src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end },

	}
	LuaUIHelper.bind(this.gameObject , FunctionBtnPanel )
end

function FunctionBtnPanel.show( )
    createPanel("Prefab/Gui/main/FunctionBtnPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function FunctionBtnPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , FunctionBtnPanel)
	this._registeEvents(this.event)
end

-- 注册界面事件监听
function FunctionBtnPanel._registeEvents(event)
    
end

--- 关闭界面
function FunctionBtnPanel._onClickClose( )
    panelMgr:ClosePanel("FunctionBtnPanel")
end

--- 由LuaBehaviour自动调用
function FunctionBtnPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function FunctionBtnPanel.OnDestroy()

end

function FunctionBtnPanel.OnSwapBtnClicked(gObj)
	this.QieHuanAnNiu.transform:DOLocalRotate(Vector3.New(0, 0, 135), 0.2, RotateMode.LocalAxisAdd)
	if this.GongNengZhanKai.gameObject.activeSelf then
		this.GongNengZhanKai.gameObject:SetActive(false)
		SkillUiPanel.ShowHide(true)
	else
		this.GongNengZhanKai.gameObject:SetActive(true)
		SkillUiPanel.ShowHide(false)
	end
end