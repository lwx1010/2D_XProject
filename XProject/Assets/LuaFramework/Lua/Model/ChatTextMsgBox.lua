ChatTextMsgBox = {}

local MaxWidth = 660

local offsetX = 2
local offsetY = 18

local channelSprNames = {
	[6] = 'ui_chat_icon_system',
	[3] = 'ui_chat_icon_team',
	[1] = 'ui_chat_icon_world',
	[2] = 'ui_chat_icon_banghui',
	[8] = 'ui_chat_icon_zhenying',
}

function ChatTextMsgBox:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ChatTextMsgBox:OnCreate(go)
	self.gameOject = go
	self.transform = go.transform
	self.behaviour = go.transform:GetComponent('LuaComponent')
	self.boxWidget = go.transform:GetComponent('UIWidget')
	self.channelSpr = go.transform:Find('channelSpr'):GetComponent('UISprite')
	self.msgLabel = go.transform:Find('msgLabel'):GetComponent('UILabel')
	self.behaviour:AddClick(self.msgLabel.gameObject, function()
		--送花
		local i, j = string.find(self.msg.chatMsg, "url=flower")
		if i and j then
			local SendFlowerCtrl = require('Controller/SendFlowerCtrl')
			SendFlowerCtrl.ShowHide(true, nil, '')
			return
		end

		--打开界面
		i, j = string.find(self.msg.chatMsg, "url=uiid=")
		if i and j then
			local k = string.find(self.msg.chatMsg, "]", j+1)
			local uiid = tonumber(string.sub(self.msg.chatMsg, j+1, k-1))
			if not uiid then return end
			local UiId = require("Logic/UiId")
			UiId.OpenUi(uiid , true)
			return	
		end
		--打开对应VIP等级特权界面
		i , j = string.find(self.msg.chatMsg , "url=vip=")
		if i and j then
			local k = string.find(self.msg.chatMsg, ']' , j+2)
			local viplevel = tonumber(string.sub(self.msg.chatMsg , j+2 , k-2))
			CtrlManager.GetCtrl("VipUiCtrl").OpenUi(2 , viplevel)
			return
		end
		if MarriageLogic.MarriageCheck() then return end
		--寻路
		i, j = string.find(self.msg.chatMsg, "url=npc=%(")
		if i and j then
			if roleMgr.mainRole and roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
				roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
			end
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local posStr = string.sub(self.msg.chatMsg, j+1, k-1)
			local posArr = string.split(posStr, ",")
			worldPathfinding:BeginWorldPathfinding(posArr[1], posArr[3], posArr[4], posArr[5], 2.0, function()
				Network.send('C2s_menu_clicknpc', {npc_no = posArr[2]})
			end, false)
			return
		end
		--[[url=applyclub=(%s)]加入帮派[/url]]
		i,j = string.find(self.msg.chatMsg, "url=applyclub=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local clubId = string.sub(self.msg.chatMsg, j+1, k-1)
			local BangHuiLogic = require "Logic/BangHuiLogic"
			BangHuiLogic.ApplyClub(clubId)
			return
		end
		--首领%s已刷新，赶紧前去击杀吧！[ff0000][[url=yewaiboss=(%s)]立即前往[/url]][-]
		i,j = string.find(self.msg.chatMsg, "url=yewaiboss=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local bossId = string.sub(self.msg.chatMsg, j+1, k-1)
			local FieldBossXls = require "xlsdata/FieldBoss/FieldBossXls"
			local bossXls = FieldBossXls[tonumber(bossId)]
			if HERO.Grade >= bossXls.Level then
				BossMainUIPanel.SetWorldBossData(1, tonumber(bossId))
				BossMainUIPanel.show()
				local FunctionTipUiCtrl = require "Controller/maintip/FunctionTipUiCtrl"
				FunctionTipUiCtrl.ClosePanel(bossId)
				return
			else
				CtrlManager.PopUpNotifyText(string.format(LANGUAGE_TIP.JumpGateTip, bossXls.Level))
			end
		end
		--婚宴
		i,j = string.find(self.msg.chatMsg, "url=marriage")
		if i and j then
			if not MarriageLogic.ISBGMMarriage then CtrlManager.PopUpNotifyText("婚礼已结束") return end
			if roleMgr.mainRole and roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
				roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
			end
			local chatCtrl = require("Controller.chat.ChatCtrl")
    		chatCtrl.ShowHide(false)
    		Network.send('C2s_marriage_wedding_go', {place_holder = 1})
    		MarriageLogic.ISONTRIGGER = false
			return
		end
		--帮派强盗
		i,j = string.find(self.msg.chatMsg, "url=thieves")
		if i and j then
			local BangHuiLogic = require("Logic/BangHuiLogic")
			BangHuiLogic.SendToEnterSite()
			return
		end
		--[[url=baotutask]宝图任务立即前往[/url]]
		i, j = string.find(self.msg.chatMsg, "url=baotutask=")
		if i and j then
			if roleMgr.mainRole and roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
				roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
			end
			local k = string.find(self.msg.chatMsg, ']' , j+1)
			local str = string.sub(self.msg.chatMsg, j+1, k-1)
			local tbl = string.split(str, '=')
			if roleMgr.curSceneNo == 1014 and tonumber(tbl[1]) == 1 then
				CtrlManager.PopUpNotifyText(LANGUAGE_TIP[string.format("treasureTaskTip%s", tbl[1])])
			elseif tbl[2] and tbl[3] then
				local x = tonumber(tbl[2]) == 0 and 69 or tonumber(tbl[2])
				local z = tonumber(tbl[3]) == 0 and 61 or tonumber(tbl[3])
				worldPathfinding:BeginWorldPathfinding(1014, x, 0, z, 0.1, nil, false)
			end
			return
		end
		i,j = string.find(self.msg.chatMsg, "url=rain")
		if i and j then
			require "View/XinChunAct/XinChunActPanel"
			XinChunActPanel.GotoRedBagRain()
		end

	end)

end

function ChatTextMsgBox:AddMsg(channel, msg)
	self.msg = msg
	self.channelSpr.spriteName = channelSprNames[msg.channel]
	self.msgLabel.text = string.format("[435B74FF]%s[-]", msg.chatMsg)
	self.msgLabel:UpdateNGUIText()
	local width = math.ceil(NGUIText.CalculatePrintedSize(msg.chatMsg, MaxWidth).x)
	if MaxWidth - width < 15 then
		width = MaxWidth
	end
	self.msgLabel.width = width + offsetX
	self.msgLabel.height = NGUIText.CalculatePrintedSize(msg.chatMsg, MaxWidth).y
	--self.boxWidget.height = NGUIText.CalculatePrintedSize(msg.chatMsg, MaxWidth).y + offsetY
	local i, j = string.find(msg.chatMsg, "url=")
	if i and j then 
		local collider = self.msgLabel.gameObject:AddComponent(typeof(UnityEngine.BoxCollider))
		collider.size = Vector3.New(width, self.msgLabel.height, 0)
		collider.center = Vector3.New(width/2, -self.msgLabel.height/2, 0)
	end
end
