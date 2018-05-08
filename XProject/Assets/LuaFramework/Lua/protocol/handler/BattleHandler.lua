local ClientFightManager = require("Logic/ClientFightManager")

Network["S2c_battle_fight_start"] = function(pb)
	--print("----S2c_battle_fight_start--")
	ClientFightManager.FightLuaInit(pb)
end


Network["S2c_battle_fight_end"] = function(pb)
	-- print("----S2c_battle_fight_end--")
	if pb.fid ~= FIGHTMGR.fightId then 
		return 
	end	

	--玩家动作停止
	local HEROSKILLMGR = HEROSKILLMGR
	HEROSKILLMGR.StopAction()

	ClientFightManager.StopFight()

	--先关闭所有的弹出界面，然后在弹出结算
	panelMgr:CloseAllPopedPanels()
	CtrlManager.PopUpPanel("FuBenJieSuanUi", false)
	local FuBenJieSuanUiCtrl = require("Controller/fuben/FuBenJieSuanUiCtrl")
	FuBenJieSuanUiCtrl.ShowFightEnd(pb)

	--副本战斗结束处理
	local FuBenLogic = require("Logic/FuBenLogic")
	local GAMECONST = GAMECONST
	--print("-=---------TYPE_JINGYAN-------", FuBenLogic.curFubenFtype, GAMECONST.FuBenFType.TYPE_JINGYAN)
	if FuBenLogic.curFubenFtype == GAMECONST.FuBenFType.TYPE_JINGYAN then
		local GuideLogic = require("Logic/GuideLogic")
		GuideLogic.ClearNuQiGuide()
	end
end


Network["S2c_battle_fight_end_error"] = function(pb)
	--print("----S2c_battle_fight_end_error--")
	print("战斗验证有问题", pb.fid, pb.e_type)
end

--爬塔战斗下一波怪
Network["S2c_battle_pt_next"] = function(pb)
	--print("----S2c_battle_pt_next--")

	--爬塔战斗处理
	ClientFightManager.CreatePaTaFuBenFight(pb)

	--更新时间
	local MainCtrl = require "Controller/MainCtrl"
	MainCtrl.SetFightLeftTime(pb.btime, TimeManager.GetRealTimeSinceStartUp())

	--主界面右上角奖励显示
	--print("----------", pb.ftype, pb.gateno, pb.layer, pb.rewardtips)
	MainCtrl.UpdateFuBenBarInfo(pb.ftype, pb.gateno, pb.layer, pb.rewardtips)
end

--塔防战斗下一波怪
Network["S2c_battle_tf_next"] = function(pb)
	-- print("----S2c_battle_tf_next--")

	--塔防战斗处理
	ClientFightManager.CreateTaFangFuBenFight(pb)
end