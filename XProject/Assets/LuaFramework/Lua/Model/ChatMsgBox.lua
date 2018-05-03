local ChatOperateCtrl = require('Controller/chat/ChatOperateCtrl')
local ChatData = require('Model/ChatData')
local LanguageTip = LANGUAGE_TIP
ChatMsgBox = {}

local BaseWidth = 45
local MaxWidth = 500

local faceNameLen = 5				--表情命名格式'#e001'
local spaceChar = ','
local faceWH = 32
local spaceCount = 5
local face_offsetX = 12
local face_offsetY = 12
local bg_offsetX = 40
local bg_offsetY = 28

local channelSprNames = {
	[6] = 'ui_chat_icon_system',
	[3] = 'ui_chat_icon_duiwu',
	[1] = 'ui_chat_icon_world',
	[2] = 'ui_chat_icon_banghui',
	[5] = 'ui_chat_icon_fujin',
	[4] = 'ui_chat_icon_miliao',
	[7] = 'ui_chat_icon_laba',
	[8] = 'ui_chat_icon_zhenying',
}

function ChatMsgBox:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ChatMsgBox:OnCreate(go)
	self.gameOject = go
	self.transform = go.transform
	self.behaviour = self.transform:GetComponent('LuaComponent')
	self.msgBgSpr = self.transform:Find('msgbg'):GetComponent('UISprite')
	self.headSpr = self.transform:Find('headSpr'):GetComponent('UISprite')
	self.channelSpr = self.transform:Find('table/channelSprite'):GetComponent('UISprite')
	self.vipGo = self.transform:Find('table/vip').gameObject
	self.nameLabel = self.transform:Find('table/nameLabel'):GetComponent('UILabel')
	self.labaSpr = self.transform:Find('table/labaSpr').gameObject
	self.titleGrid = self.transform:Find('table'):GetComponent("UITable")
	self.msgLabel = self.transform:Find('msgLabel'):GetComponent('UILabel')
	self.gradeLabel = self.transform:Find('gradeLabel'):GetComponent('UILabel')
	self.copyGo = self.transform:Find("msgbg/copy").gameObject
	self.behaviour:AddSelect(self.copyGo, function(o, status)
		if not status then
			self.copyGo:SetActive(false)
		end
	end)
	self.behaviour:AddClick(self.copyGo, function()
		self.copyGo:SetActive(false)
		local str = string.gsub(self.msg.chatMsg, "%[.-%]", "")
		PlatformHelper.CopyToClipboard(str)
		CtrlManager.PopUpNotifyText(LANGUAGE_TIP.copyMsgTip)
	end)
	self.behaviour:AddClick(self.headSpr.gameObject, function() 
		if HERO.HostId ~= self.msg.hostId then
			CtrlManager.PopUpNotifyText(LANGUAGE_TIP.KuaFuChatTip)
			return
		end

		ChatOperateCtrl.ShowOperatePanel(self.msg.chatName, self.msg.chatUid, self.msg.grade, false)
	end)
	self.behaviour:AddClick(self.msgLabel.gameObject, function()
		local i, j = string.find(self.msg.chatMsg, "url=daoju=")
		if i and j then
			local k = string.find(self.msg.chatMsg, "]", j+1)
			local itemNo = string.sub(self.msg.chatMsg, j+1, k-1)
			ITEMLOGIC.LoadBagMaterialTipUiPanel(tonumber(itemNo), false)
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
		i, j = string.find(self.msg.chatMsg, "url=horseEquip=")
		if i and j then
			local k = string.find(self.msg.chatMsg, "]", j+1)
			local itemNo = string.sub(self.msg.chatMsg, j+1, k-1)
			CtrlManager.PopUpPanel('HorseEquipTipUi', false)
			local HorseEquipTipUiCtrl = require('Controller/HorseEquipTipUiCtrl')
			HorseEquipTipUiCtrl.SetEquipTipInfo(tonumber(itemNo))
			return
		end
		i, j = string.find(self.msg.chatMsg, "url=equip=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local idStr = string.sub(self.msg.chatMsg, j+1, k-1)
			local idArr = string.split(idStr, ",")
			Network.send('C2s_hero_equiptips', {uid = idArr[1], sid = idArr[2]})
			return
		end
		i, j = string.find(self.msg.chatMsg, "url=bazheEquip=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local idStr = string.sub(self.msg.chatMsg, j+1, k-1)
			local idArr = string.split(idStr, ",")
			GodSuitTipUiPanel.show(tonumber(idArr[1]), 0)
			return
		end
		i, j = string.find(self.msg.chatMsg, "url=pos=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local posStr = string.sub(self.msg.chatMsg, j+1, k-1)
			local posArr = string.split(posStr, ",")
			if tonumber(posArr[1]) == SecretBossLogic.SECRET_MAPID then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.secretBossTip_15) return end 
			if not MAPLOGIC.JudgeLevelCanJump(tonumber(posArr[1])) then return end
			if tonumber(posArr[5]) == roleMgr.sceneType then
				if roleMgr.sceneType ~= 1 and not ((roleMgr.sceneType == 3 or roleMgr.sceneType == 4 or roleMgr.sceneType == 9) and tonumber(posArr[6]) == roleMgr.curSceneId) then
					CtrlManager.PopUpNotifyText(LANGUAGE_TIP.cantGoto)
					return
				end
			elseif tonumber(posArr[5]) ~= 4 or roleMgr.sceneType ~= 1 then
				CtrlManager.PopUpNotifyText(LANGUAGE_TIP.cantGoto)
				return
			end
			if roleMgr.mainRole and roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
				roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
			end
			worldPathfinding:BeginWorldPathfinding(posArr[1], posArr[2], posArr[3], posArr[4], 0.1, nil, false)
			CtrlManager.GetCtrl(CtrlNames.Chat).ShowHide(false)
			CtrlManager.PopUpNotifyText(LanguageTip.zhengzaiqianwang)
			return
		end
		i, j = string.find(self.msg.chatMsg, "url=protect=%(")
		if i and j then
			if HERO.IsYunBiao > 0 then
				CtrlManager.PopUpNotifyText(LANGUAGE_TIP.husongProtectTip3)
				return
			end
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local uidStr = string.sub(self.msg.chatMsg, j+1, k-1)
			if uidStr == HERO.Uid then
				CtrlManager.PopUpNotifyText(LANGUAGE_TIP.husongProtectTip2)
				return
			end
			local HusongLogic = require('Logic/HusongLogic')
			HusongLogic.ParseSosMsg(uidStr)
			return
		end
		--[[url=roomteam=(%s,%s)]加入队伍[/url]]    --公告的这个,第一个是team_id,第二个是否有密码,1有密码,0没密码
		i, j = string.find(self.msg.chatMsg, "url=roomteam=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local str = string.sub(self.msg.chatMsg, j+1, k-1)
			local values = string.split(str, ',')
			local teamId = tonumber(values[1])
			local passward = tonumber(values[2])
			local MultiCopyCtrl = require "Controller/MultiCopy/MultiCopyCtrl"
			MultiCopyCtrl.ApplyRoomTeam(teamId, 1, passward)
			return
		end

		--3v3组队邀请组队
		i,j = string.find(self.msg.chatMsg, "url=k3v3team=%(")
		if i and j then
			local k = string.find(self.msg.chatMsg, ")", j+1)
			local teamId = string.sub(self.msg.chatMsg, j+1, k-1)
			ThreeUnitRacesHandler:ApplyJoinTeam(teamId)
			return
		end
		
		self.copyGo:SetActive(true)
		local factor = self.key == 1 and -1 or 1
		self.copyGo.transform.localPosition = Vector3.New(self.msgBgSpr.width*factor - 54, 70, 0)
		UICamera.selectedObject = self.copyGo
	end)
end

function ChatMsgBox:AddMsg(channel, msg)
	self.msg = msg
	self.key = msg.key
	if channel == ChatData.ALL_CHANNEL then
		self.channelSpr.gameObject:SetActive(true)
		self.channelSpr.spriteName = channelSprNames[msg.channel]
	else
		self.channelSpr.gameObject:SetActive(false)
	end
	if msg.vip > 0 then
		self.vipGo:SetActive(true)
	end
	if msg.sendType == 1 then -- 喇叭
		self.labaSpr:SetActive(channel ~= ChatData.ALL_CHANNEL)
		self.channelSpr.spriteName = channelSprNames[7]
		self.msgBgSpr.spriteName = msg.key == 1 and "ui_liaotian_duihualaba" or "ui_liaotian_duihualaba2"
	end
	self.headSpr.spriteName = tostring(msg.head)
	self.nameLabel.text = msg.chatName
	self.nameLabel:UpdateNGUIText()
	self.nameLabel.width = math.ceil(NGUIText.CalculatePrintedSize(msg.chatName, MaxWidth).x)+2
	self.titleGrid.repositionNow = true
	self.msgLabel.text = msg.chatMsg
	local str = self:ReplaceFace(msg.chatMsg)
	-- str = self:DrawText()
	self.msgLabel.text = str
	self.msgLabel:UpdateNGUIText()
	local width = math.ceil(NGUIText.CalculatePrintedSize(str, MaxWidth).x)
	local height = math.ceil(NGUIText.CalculatePrintedSize(str, MaxWidth).y)
	if MaxWidth - width < 36 then
		width = MaxWidth
	end
	if height < 32 then
		height = 32
	end
	width = math.max(BaseWidth, width)
	self.msgLabel.width = width
	self.msgLabel.height = height
	self.msgBgSpr.width = width + bg_offsetX
	self.msgBgSpr.height = height + bg_offsetY
	self.msgLabel.text = string.format("[435B74FF]%s[-]", str)
	self:DrawFace()
	local collider = self.msgLabel.gameObject:AddComponent(typeof(UnityEngine.BoxCollider))
	collider.size = Vector3.New(width, self.msgLabel.height)
	collider.center = Vector3.New(width/2*(msg.key == 1 and -1 or 1), -self.msgLabel.height/2)
	self.gradeLabel.text = tostring(msg.grade)
end

--判断是不是遇到表情
local function isFaceLabel(text, index)
	local str = string.subString(text, index, index)
	local str2 = string.subString(text, index+1, index+1)
	local str3 = string.subString(text, index+2, index+4)
	return str == '#' and index + 4 <= string.utf8len(text) and str2 == 'e' and tonumber(str3)
end

-- --替换表情符号
-- function ChatMsgBox:ReplaceFace(str)
-- 	if not str or str == '' then return '' end
-- 	self.msgLabel:UpdateNGUIText()
-- 	self.msgLabel.spacingY = 4
-- 	self.eList = {}
-- 	self.lineList = {}
-- 	self.expInLine = {}
-- 	local row = 0
-- 	local textWidth = 0
-- 	local lastStartIndex = 1
-- 	local curLine = ''
-- 	local len = string.len(str)
-- 	local i = 1
-- 	while i<= len do
-- 		if isFaceLabel(str, i) then
-- 			local fx = 0
-- 			local ePos = {}
-- 			ePos.faceID = string.sub(str, i+1, i+4)
-- 			--self.msgLabel.spacingY = math.abs(faceWH - self.msgLabel.fontSize)
-- 			local spaceTemp = ''
-- 			for j=1,spaceCount do
-- 				spaceTemp = spaceTemp .. spaceChar
-- 			end
-- 			local str1 = string.sub(str, 1, i-1)
-- 			local str2 = string.sub(str, i+faceNameLen)
-- 			str = string.format('%s%s%s', str1, spaceTemp, str2)
-- 			len = string.len(str)
-- 			textWidth = NGUIText.CalculatePrintedSize(string.sub(str, lastStartIndex, i-1), MaxWidth).x
-- 			if textWidth + faceWH > MaxWidth then
-- 				curLine = string.sub(str, lastStartIndex, i)
-- 				table.insert(self.lineList, curLine)
-- 				-- if (textWidth + faceWH/2 <= MaxWidth) or textWidth >= MaxWidth then

-- 				-- else
-- 				-- 	fx = textWidth
-- 				-- 	lastStartIndex = i+spaceCount
-- 				-- 	ePos.posX = fx
-- 				-- 	ePos.posY = row
-- 				-- 	row = row + 1
-- 				-- end
-- 				fx = 0
-- 				row = row + 1
-- 				lastStartIndex = i
-- 				ePos.posX = fx
-- 				ePos.posY = row
-- 			else
-- 				fx = textWidth
-- 				ePos.posX = fx
-- 				ePos.posY = row
-- 			end
-- 			table.insert(self.eList, ePos)
-- 			self.expInLine[row] = true
-- 		else
-- 			if i>= lastStartIndex then
-- 				local curWidth = NGUIText.CalculatePrintedSize(string.sub(str, lastStartIndex, i), MaxWidth).x
-- 				if curWidth > MaxWidth - 36 then
-- 					curLine = string.sub(str, lastStartIndex, i)
-- 					table.insert(self.lineList, curLine)
-- 					lastStartIndex = i +1
-- 					row = row + 1
-- 				end
-- 				if i==len then
-- 					curLine = string.sub(str, lastStartIndex, i)
-- 					table.insert(self.lineList, curLine)
-- 				end
-- 			end
-- 		end
-- 		i = i+1
-- 	end
-- 	return str
-- end

-- function ChatMsgBox:DrawText()
-- 	local text = ''
-- 	for i=1, #self.lineList do
-- 		if self.expInLine[i-1] then
-- 			if i == 1 then
-- 				text = string.format("%s\n%s", text, self.lineList[i])
-- 			else
-- 				text = string.format("%s\n\n%s", text, self.lineList[i])
-- 			end
-- 		else
-- 			if i == 1 then
-- 				text = string.format("%s%s", text, self.lineList[i])
-- 			else
-- 				text = string.format("%s\n%s", text, self.lineList[i])
-- 			end
-- 		end
-- 	end
-- 	self.msgLabel.text = text
-- 	return text
-- end

-- --画表情
-- function ChatMsgBox:DrawFace()
-- 	local count = #self.eList
-- 	local faceAtlas = Util.loadAtlas('Atlas/icon/othericon')
-- 	local textWidth = self.msgLabel.width
-- 	for i=1, count do
-- 		local ePos = self.eList[i]
-- 		local go = GameObject.New('e' .. i)
-- 		go.layer = 5
-- 		local spr = go:AddComponent(typeof(UISprite))
-- 		spr.atlas = faceAtlas
-- 		spr.spriteName = ePos.faceID .. '_01'
-- 		spr:MakePixelPerfect()
-- 		spr.depth = self.msgLabel.depth+1
-- 		go.transform:SetParent(self.msgLabel.gameObject.transform)
-- 		go.transform.localScale = Vector3.one
-- 		if self.key == 1 then-----1为自己说话right，2为其他人说话left
-- 			go.transform.localPosition = Vector3.New(faceWH/2 + ePos.posX - textWidth, self:CalculateEmotionY(ePos.posY), 0)
-- 		else
-- 			go.transform.localPosition = Vector3.New(faceWH/2 + ePos.posX, self:CalculateEmotionY(ePos.posY), 0)
-- 		end
-- 		local sprAni = go:AddComponent(typeof(UISpriteAnimation))
-- 		sprAni.namePrefix = ePos.faceID
-- 		sprAni.framesPerSecond = 5
-- 	end
-- end

-- function ChatMsgBox:CalculateEmotionY(row)
-- 	local height = faceWH/2
-- 	for i=0, row do
-- 		if self.expInLine[i] then
-- 			height = height - faceWH
-- 		else
-- 			height = height - faceWH/2
-- 		end
-- 	end
-- 	return height
-- end

--替换表情符号
function ChatMsgBox:ReplaceFace(str)
	if not str or str == '' then return '' end
	self.msgLabel:UpdateNGUIText()
	self.msgLabel.spacingY = 4
	self.eList = {}
	local lineW = 0
	local rowCount = 0
	local lastStartIndex = 1
	local len = string.utf8len(str)
	local i = 1
	for i=1, len do
		if isFaceLabel(str, i) then
			local ePos = {}
			ePos.faceID = string.subString(str, i+1, i+4)
			local textV = NGUIText.CalculatePrintedSize(string.subString(str, lastStartIndex, i-1), MaxWidth+faceWH)
			if textV.x > MaxWidth - faceWH then
				if textV.x > MaxWidth - faceWH/2 then
					lineW = 0
					lastStartIndex = i
					ePos.posY = -(rowCount+1) * faceWH + 2
				else
					lastStartIndex = i + faceNameLen
					lineW = textV.x
					ePos.posY = -rowCount * faceWH + 2
				end
				rowCount = rowCount + 1
			else
				lineW = textV.x
				ePos.posY = -rowCount * faceWH + 2
			end
			local spaceTemp = ''
			for j=1,spaceCount do
				spaceTemp = spaceTemp .. spaceChar
			end
			local str1 = string.subString(str, 1, i-1)
			local str2 = string.subString(str, i+faceNameLen)
			-- str = string.format('%s[ffffffff]%s[-]%s', str1, spaceTemp, str2)
			str = str1 .. spaceTemp .. str2
			-- len = string.utf8len(str)
			ePos.posX = lineW
			ePos.index = i
			table.insert(self.eList, ePos)
			-- i = i + 13 + faceNameLen - 1
		else
			if i> lastStartIndex then
				local curWidth = NGUIText.CalculatePrintedSize(string.subString(str, lastStartIndex, i), MaxWidth+faceWH).x
				if curWidth>= MaxWidth then
					lastStartIndex = i
					rowCount = rowCount + 1
				end
			end
		end
	end
	return self:HideReplaceChars(str)
end

function ChatMsgBox:HideReplaceChars(str)
	local count = #self.eList
	for i=1, count do
		local index = self.eList[i].index + (13+spaceCount - faceNameLen)*(i-1)
		local spaceTemp = ''
		for j=1,spaceCount do
			spaceTemp = spaceTemp .. spaceChar
		end
		local str1 = string.subString(str, 1, index-1)
		local str2 = string.subString(str, index+spaceCount)
		str = string.format('%s[ffffff00]%s[-]%s', str1, spaceTemp, str2)
	end
	return str
end

--画表情
function ChatMsgBox:DrawFace()
	if not self.eList then return end
	local count = #self.eList
	local faceAtlas = Util.loadAtlas('Atlas/icon/othericon')
	local textWidth = self.msgLabel.width
	for i=1, count do
		local ePos = self.eList[i]
		local go = GameObject.New('e' .. i)
		go.layer = 5
		local spr = go:AddComponent(typeof(UISprite))
		spr.atlas = faceAtlas
		spr.spriteName = ePos.faceID .. '_01'
		spr.width = faceWH
		spr.height = faceWH
		spr.depth = self.msgLabel.depth+1
		go.transform:SetParent(self.msgLabel.gameObject.transform)
		go.transform.localScale = Vector3.one
		if self.key == 1 then-----1为自己说话right，2为其他人说话left
			go.transform.localPosition = Vector3.New(faceWH/2 + ePos.posX - textWidth, ePos.posY-faceWH/2, 0)
		else
			go.transform.localPosition = Vector3.New(faceWH/2 + ePos.posX, ePos.posY-faceWH/2, 0)
		end
		-- spr.pivot = UIWidget.Pivot.TopLeft
		--go.transform.localPosition = Vector3.New(ePos.posX, ePos.posY, 0)
		local sprAni = go:AddComponent(typeof(UISpriteAnimation))
		sprAni.namePrefix = ePos.faceID
		sprAni.framesPerSecond = 5
	end
end
