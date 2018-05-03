local CommonCtrl = {}
local this = CommonCtrl
local BAZHEMERGER_NPC = 10146
local GCZ_REWARD_NPC1 = 10303
local GCZ_REWARD_NPC2 = 10307
local MARRIAGE_TYPE = 3
-- require('Logic/MarriageLogic')
function CommonCtrl.CreateQuanQuan( transform)
	if CtrlManager.GetCtrl("NetConnecting") == nil then
		local netConnectPrefab = resMgr.LoadPrefab("Prefab/Gui/NetConnecting")
		local netConnectGo = newObject(netConnectPrefab)
		--netConnectGo.transform:SetParent(transform)
		--netConnectGo.transform.localScale = Vector3.New(1, 1, 1)
		--netConnectGo.transform.localPosition = Vector3.zero
		CtrlManager.AddCtrl("NetConnecting", netConnectGo)

		local autoDestroy = netConnectGo:GetComponent(typeof(AutoDestroy))
		autoDestroy.onDestroyCallback = function()
			this.RemoveQuanQuan()
		end
    end
end

function CommonCtrl.RemoveQuanQuan()
	local netConnectGo = CtrlManager.GetCtrl("NetConnecting")
	if netConnectGo then
		destroy(netConnectGo)
		CtrlManager.RemoveCtrl("NetConnecting")
	end
end

function CommonCtrl.PopUpNotifyText(str)
	print("PopUpText: ", str)
end

--点击npc，发送对话协议s
function CommonCtrl.NpcClick(npcId, npcNo)
	
	local TaskLogic = require('Logic/TaskLogic')
	local HusongLogic = require('Logic/HusongLogic')
	local NpcXls = require('xlsdata/Npc/StaticNpcXls')
	local BangHuiLogic = require("Logic/BangHuiLogic")
	local FactionTransmitNpcXls = require("xlsdata/FactionTransmitNpcXls")
	local npc = NpcXls[npcNo]
	--print(npcId, npcNo,npc.NpcType)
	this.PlayNpcSound(npcNo)
	if npc and npc.NpcType and npc.NpcType == 3 then
		if npcNo == 10317 then
			require "View/XinChunAct/YanHuaPanel"
			YanHuaPanel.show()
		end
	end

	if not npc then 
		local xunLuoNpcXls = require("xlsdata/Npc/XunLuoNpcXls")
		for k, v in pairs(xunLuoNpcXls) do
			if v.NpcNo == npcNo then
				local obj = roleMgr:GetXunLuoNpc(npcNo)
				if obj then
					obj:XunLuoNpcClick(v.Say)
				end
				return true
			end
		end
		return false 
	end

	if npcNo == HusongLogic.ACCEPT_NPC and  FUNCOPENDDATA.JudgeFuncOpenState(FUNCOPENDDATA.FUNC_INDEX.HUSONG) then --李绩，护送Npc
		HusongLogic.ContinueHusong()
	elseif npcNo == HusongLogic.TARGET_NPC and FUNCOPENDDATA.JudgeFuncOpenState(FUNCOPENDDATA.FUNC_INDEX.HUSONG) then --袁天罡，护送Npc
		Network.send('C2s_menu_clicknpc', {npc_no = npcNo})
	elseif npcNo == BAZHEMERGER_NPC or npcNo == GCZ_REWARD_NPC1 or npcNo == GCZ_REWARD_NPC2 then   --霸者神装合成
		Network.send('C2s_menu_clicknpc', {npc_no = npcNo})
	elseif TaskLogic.JudgeNpcHasTask(npcNo) then
		local Game = Game
        Game.OnMainQuestContinue()
        return npc.NpcType == 0
	elseif ImperialExamManager.state == 1 and npcNo == ImperialExamManager.curNpcNo then
		Network.send('C2s_menu_clicknpc', {npc_no = npcNo})
	elseif BangHuiLogic.isInFactionScene and npcNo == FactionTransmitNpcXls[1].NpcNo then
		FactionTransmitHandler.RequestMembersList()
	else
		if npc.NpcType == 1 then --采集物
			CollectPanel.SetEntityId(npcId)
			Event.Brocast(EventName.COLLECT_CLICK_SHOW, npcId)
			return false
		elseif npc.NpcType == 2 then	--不冒泡不转身
			Event.Brocast(EventName.RUNE_CLICK_EVENT, npcId)
			return false
		elseif npc.NpcType == 0 then
			local obj = roleMgr:GetSceneEntityById(npcId, 0)
			if obj then
				obj:Say(npc.Say)
			end

			local BPLDZLogic = require("Logic/BPLDZLogic")
			BPLDZLogic.LDZExchangeNpcClicked(npcNo)
		elseif npc.NpcType == MARRIAGE_TYPE then  --婚宴采集
			CollectPanel.SetEntityId(npcId)
			Event.Brocast(EventName.MARRIAGE_CAIJITYPE , npcNo)
			Event.Brocast(EventName.MARRIAGE_CAIJICLICK, npcId)
			return false
		end
	end
	return true
end

function CommonCtrl.GetNpcBubble(npcNo, showtype)
	if showtype ~= 2 then
		local NpcXls = require('xlsdata/Npc/StaticNpcXls')
		local npc = NpcXls[npcNo]
		if npc then return npc.Say end
	else
		local BattleNpcXls = require("xlsdata/Npc/BattleNpcXls")
   		if npcNo and BattleNpcXls[npcNo] then
   			return BattleNpcXls[npcNo].Say
   		end
	end
	return ''
end

function CommonCtrl.PlayNpcSound(npcNo)
	local NpcSoundXls = require 'xlsdata/Npc/NpcSoundXls'
	local NpcXls = require 'xlsdata/Npc/StaticNpcXls'
	local sound = NpcSoundXls[npcNo]
	local npc = NpcXls[npcNo]
	local pos = nil
	if npc then
		pos = npc.Position
	end
	if sound and pos then
		local realPos = Util.Convert2RealPosition(pos[1], pos[3], pos[2])
		soundMgr:PlayTalkSound(sound.Name, sound.Volume, sound.Delay, realPos)
	end
end

function CommonCtrl.JudgeNpcNeedTaskFlag(npcNo)
	local TaskLogic = require('Logic/TaskLogic')
	local NpcXls = require('xlsdata/Npc/StaticNpcXls')
	return NpcXls[npcNo].NpcType == 0 and TaskLogic.JudgeNpcHasTask(npcNo)
end

--通过uid邀请组队
function CommonCtrl.InviteJoinTeamByUid(uid)
	Network.send("C2s_team_chat_invite", {uid = uid})
end

--加载粒子特效
--@path  完整路径
--@target 参照对象，从他身上获取深度关系
--@isForward 是否显示在参照对象的前面
--@parent 如果有则加载在下面，没有则加载到target下
function CommonCtrl.LoadParticleEffect(path, target, isForward, parent)
	local prefab = Util.LoadPrefab(path)
	if not prefab then return nil end
	local effect = newObject(prefab)
	local script = effect:GetComponent(typeof(UIParticles))
	script.parentWidget = target.transform:GetComponent('UIWidget')
	script.IsForward = isForward
	if parent then
		effect.transform:SetParent(parent.transform)
	else
		effect.transform:SetParent(target.transform)
	end
	effect.transform.localScale = Vector3.one
	effect.transform.localPosition = Vector3.zero
	return script
end

function CommonCtrl.SetParticleScale(trans, scale)
	if not trans then return end

	local psArr = trans:GetComponentsInChildren(typeof(UnityEngine.ParticleSystem));
	for i = 0, psArr.Length -1 do
		psArr[i].gameObject.transform.localScale = scale
	end
end

-------------------------------------------------------------
--玩家使用键盘移动
function CommonCtrl.OnKeyboardMoved()
	--如果正在采集，则中断
	if CtrlManager.PanelIsPopuped("Caiji") then
		CtrlManager.GetCtrl(CtrlNames.Caiji).CaijiSuccess(false)
	end

	--如果在攻城战摧毁界面时
	local BPGCZLogic = require("Logic/BPGCZLogic")
	if BPGCZLogic.IsInBPGCZ and CtrlManager.GetCtrl(CtrlNames.Main).BPGCZBarCtrl.goDestroyCv.activeSelf then
		CtrlManager.GetCtrl(CtrlNames.Main).BPGCZBarCtrl.OnDestroyCancelBtnClick()
	end
end
-------------------------------------------------------------
--键盘技能按键响应
function CommonCtrl.SkillByKeyboard(keyIndex)

	print("--------------", keyIndex)
	if true then return end

	if keyIndex == 32 then		--空格
		mainCtrl.SkillBtnCtrl.JumpSkillBtnClick()
	elseif keyIndex == 256 then
		mainCtrl.SkillBtnCtrl.NormalAttackBtnClick()
	elseif keyIndex > 256 and keyIndex < 260 then
		local FUNCOPENDDATA = FUNCOPENDDATA
		if not FUNCOPENDDATA.JudgeFuncOpenState(keyIndex-207) then
			return
		end

		mainCtrl.SkillBtnCtrl.SkillBtnClick(keyIndex-257)
	elseif keyIndex == 266 then
		mainCtrl.SkillBtnCtrl.BigSkillBtnClick()
	end
end

function CommonCtrl.GetTitleInfo(title)
	local ChengHaoLogic = require('Logic/ChengHaoLogic')
	local titleSpr = ChengHaoLogic.GetTitleSpr(title)
	local title_effect = ChengHaoLogic.GetTitleEffect(title)
	return titleSpr, title_effect
end

function CommonCtrl.GetPostNameByPost(post)
	local BangHuiLogic = require("Logic/BangHuiLogic")
	return BangHuiLogic.GetPostNameByPost(post)
end
-------------------------------------------------------------
function CommonCtrl.DealWithSensitiveWord()
	local DealSensitiveWord = require("language/DealSensitiveWord")
	DealSensitiveWord.AddAndDeleteSensitiveWord()
end

-------------------------------------------------------------
--fps弹出设置
function CommonCtrl.ShowFpsSettingTip(type)
	SettingFpsPanel.show(type)
end

-------------------------------------------------------------

return CommonCtrl