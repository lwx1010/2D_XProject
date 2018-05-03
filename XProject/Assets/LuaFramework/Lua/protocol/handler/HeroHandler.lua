require('Common/EventType')
local HERO = HERO
local Network = Network

Network["S2c_hero_info_nil"] = function(pb)
	print("-------S2c_hero_info_nil--------")
	HERO[pb.key] = nil

	local attrs = {}
	attrs[pb.key] = nil

	if (Event.EventExist(EventType.Hero_Attrs)) then
		Event.Brocast(EventType.Hero_Attrs, attrs)
	end
end

Network["S2c_hero_info_int"] = function(pb)
	local attrs = {}
	for i, subPb in ipairs(pb.info) do
		HERO[subPb.key] = subPb.value
		attrs[subPb.key] = subPb.value
	end

	if (Event.EventExist(EventType.Hero_Attrs)) then
		Event.Brocast(EventType.Hero_Attrs, attrs)
	end
end

Network["S2c_hero_enter_info"] = function(pb)
	--log(TableToString(pb))
	HERO.initPlayerInfo(pb)
	-- MergeActPanel.OpenTime = pb.merge_time
	--print("=-----------------S2c_hero_enter_info------------------------------", HERO.Id)
end

Network["S2c_hero_info_string"] = function(pb)
	local attrs = {}
	for i, subPb in ipairs(pb.info) do
		HERO[subPb.key] = subPb.value
		attrs[subPb.key] = subPb.value
		-- if subPb.key == "ClubId" then
		-- 	print("-------ClubId--------", subPb.value)
		-- end
	end

	if (Event.EventExist(EventType.Hero_Attrs)) then
		Event.Brocast(EventType.Hero_Attrs, attrs)
	end
end

Network["S2c_hero_gradetips"] = function(pb)
	ITEMLOGIC.RefreshBazheEquipTip()
end

Network["S2c_hero_dz_addexp"] = function(pb)
	FIGHTMGR.AddShowExp(pb.aexp)
end

Network["S2c_hero_info"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).ShowPlayerInfo(pb)
end

Network["S2c_hero_equiptips"] = function(pb)
	local CharacterEquipTipUiLogic = require('Logic/CharacterEquipTipUiLogic')
	local jewelList = {}
	for i=1, #pb.jewels, 2 do
		jewelList[pb.jewels[i]] = pb.jewels[i+1]
	end
	CharacterEquipTipUiLogic.NewCharacterTip(pb.itemno, 1, pb.pos, pb.score, pb.quality, pb.qualitygrade, pb.strenggrade, jewelList, pb.jingliantimes[1], pb.jingliantimes[2], pb.jingliantimes[3], true, pb.grade, pb.zhuhun[1], pb.zhuhun[2], pb.zhuhun[3])
end

Network["S2c_hero_mounttips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_jingmaitips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_lingqitips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_lingqintips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_lingyitips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_pettips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_shenjiantips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_shenyitips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_thugtips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

Network["S2c_hero_thughorsetips"] = function(pb)
	CtrlManager.GetCtrl(CtrlNames.PlayerCheck).PlayerHorseCtrl.RecieveSystemInfo(pb)
end

---------------------------------------------------------------------------------------
Network["S2c_hero_canflyshoe"] = function (pb)
	--print("------S2c_hero_canflyshoe----", pb.can)
	local MAPLOGIC = MAPLOGIC
	MAPLOGIC.DealFlyCallBack(pb.can)
	if pb.can == 1 then
		MAPLOGIC.StartFlyShow(pb.can)
	else
		CtrlManager.PopUpNotifyText(pb.msg)
		MAPLOGIC.ClearFlyShoeInfo()
	end
end

Network["S2c_hero_flyshoe_error"] = function(pb)
	--print("------S2c_hero_flyshoe_error----")

	if pb.msg and string.len(pb.msg) > 0 then
		CtrlManager.PopUpNotifyText(pb.msg)
	end

	local MAPLOGIC = MAPLOGIC
	MAPLOGIC.ClearFlyShoeInfo()
end
---------------------------------------------------------------------------------------
--主界面提示
Network["S2c_hero_redtips"] = function(pb)
	PROMPTTIPICONLOGIC.RefreshRedTips(pb)
end

--获得新物品
Network['S2c_hero_itemtips'] = function(pb)
	PROMPTTIPICONLOGIC.RefreshNewItemUiTips(pb)
end

Network['S2c_hero_uiflow'] = function(pb)
	local MainCtrl = CtrlManager.GetCtrl("MainCtrl")
	MainCtrl.RefreshShowFight(pb)
	
end

--弹功能提示框
Network['S2c_hero_funtips'] = function(pb)
	PROMPTTIPICONLOGIC.RefreshFunctionUi(pb)
end

Network['S2c_hero_itemgift'] = function(pb)
	PROMPTTIPICONLOGIC.RefreshPackageUi(pb)
end

--离线挂机返回
Network['S2c_hero_offline_exp'] = function(pb)
	local rewardhallctrl = CtrlManager.GetCtrl('RewardHallUiCtrl')
	rewardhallctrl.LOGIC.RefreshOfflineInfo(pb)
end

--开服天数
Network['S2c_hero_engineday'] = function(pb)
	HERO.ServerDay = pb.day
	local MainCtrl = CtrlManager.GetCtrl("MainCtrl")
	MainCtrl.OnBroadcastWarmtip(true)
end