ChatTextMsgBox2 = {}
local ChatData = require('Model/ChatData')
local MaxWidth = 504

local offsetX = 2
local offsetY = 0
local faceNameLen = 5				--表情命名格式'#e001'
local spaceChar = ','
local vipSpace = '[ffffff00],,,,[-]'
local faceWH = 32
local spaceCount = 5

local LanguageTip = LANGUAGE_TIP

local channelSprNames = {
	[6] = 'ui_chat_icon_system',
	[3] = 'ui_chat_icon_duiwu',
	[1] = 'ui_chat_icon_world',
	[2] = 'ui_chat_icon_banghui',
	[5] = 'ui_chat_icon_fujin',
	[4] = 'ui_chat_icon_miliao',
	[7] = 'ui_chat_icon_laba',
	[8] = 'ui_chat_icon_zhenying',
	[66] = 'ui_liaotian_dati_bq', --问题的标签
}

local channelColors = {
	[1] = 'ebd6bb',
	[2] = '51d14f',
	[3] = '3dbdff',
	[4] = 'eb7cf5',
	[5] = 'b9e6e6',
	[6] = 'ebd6bb',
	[7] = 'ffd926',
	[8] = '3dbdff',
	[66] = 'ebd6bb',
}

function ChatTextMsgBox2:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ChatTextMsgBox2:OnCreate(go)
	self.gameObject = go
	self.transform = go.transform
	self.boxWidget = go.transform:GetComponent('UIWidget')
	self.channelSpr = go.transform:Find('channelSpr'):GetComponent('UISprite')
	self.msgLabel = go.transform:Find('msgLabel'):GetComponent('UILabel')
	self.vipGo = go.transform:Find('vip').gameObject
end

function ChatTextMsgBox2:AddMsg(msg)
	self.msg = msg
	local channel = msg.channel
	if msg.sendType == ChatData.SEND_TYPE_BIG then
		channel = 7
	end
	self.channelSpr.spriteName = channelSprNames[channel]
	local str = msg.chatMsg
	if msg.chatName ~= '' then
		str = string.format('[ffd926]%s[-]:%s', msg.chatName, msg.chatMsg)
	end
	if msg.vip > 0 then
		str = string.format("%s%s", vipSpace, str)
		self.vipGo:SetActive(true)
	end
	self.msgLabel.text = str
	str = self:ReplaceFace(str)
	self.msgLabel.text = str
	self.msgLabel:UpdateNGUIText()
	str = string.format('[%s]%s[-]', channelColors[channel], str)
	local width = math.ceil(NGUIText.CalculatePrintedSize(str, MaxWidth).x)
	if MaxWidth - width < faceWH then
		width = MaxWidth
	end
	self.msgLabel.width = width + offsetX
	self.msgLabel.height = NGUIText.CalculatePrintedSize(str, width).y + offsetY
	self.msgLabel.text = str
	self:DrawFace()
end

--判断是不是遇到表情
local function isFaceLabel(text, index)
	local str = string.subString(text, index, index)
	local str2 = string.subString(text, index+1, index+1)
	local str3 = string.subString(text, index+2, index+4)
	return str == '#' and index + 4 <= string.utf8len(text) and str2 == 'e' and tonumber(str3)
end

--替换表情符号
function ChatTextMsgBox2:ReplaceFace(str)
	if not str or str == '' then return '' end
	self.msgLabel:UpdateNGUIText()
	self.msgLabel.spacingY = 12
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
				if textV.x > MaxWidth - faceWH/2 then  --行尾不足换行
					lineW = 0
					lastStartIndex = i
					ePos.posY = -(rowCount+1) * faceWH - (rowCount+1) * 8 + 2
				else                                   --行尾足够这个表情不换行，文本换行
					lastStartIndex = i + faceNameLen     
					lineW = textV.x
					ePos.posY = -rowCount * faceWH - rowCount * 8 + 2
				end
				rowCount = rowCount + 1
			else
				lineW = textV.x
				ePos.posY = -rowCount * faceWH - rowCount * 8 + 2
			end
			ePos.posX = lineW
			ePos.index = i
			table.insert(self.eList, ePos)

			local spaceTemp = ''
			for j=1,spaceCount do
				spaceTemp = spaceTemp .. spaceChar
			end
			local str1 = string.subString(str, 1, i-1)
			local str2 = string.subString(str, i+faceNameLen)
			str = string.format('%s%s%s', str1, spaceTemp, str2)
			-- len = string.utf8len(str)
			-- i = i + 13 + spaceCount - 1
		else
			if i > lastStartIndex then
				local curWidth = NGUIText.CalculatePrintedSize(string.subString(str, lastStartIndex, i), MaxWidth + faceWH).x
				if curWidth >= MaxWidth then
					lastStartIndex = i
					rowCount = rowCount + 1
				end
			end
		end
	end
	return self:HideReplaceChars(str)
end

function ChatTextMsgBox2:HideReplaceChars(str)
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
function ChatTextMsgBox2:DrawFace()
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
		spr.pivot = UIWidget.Pivot.TopLeft
		go.transform.localPosition = Vector3.New(ePos.posX, ePos.posY, 0)
		local sprAni = go:AddComponent(typeof(UISpriteAnimation))
		sprAni.namePrefix = ePos.faceID
		sprAni.framesPerSecond = 5
	end
end

function ChatTextMsgBox2:UpdateQuestionStatus()
	if not CHATLOGIC.chatQuestion or self.msg.channel ~= 66 then return end
	if CHATLOGIC.chatQuestion.state == 2 then
		self.msg.chatMsg = string.format('%s[f0361d](%s)[-]', self.msg.chatMsg, LanguageTip.chatQaTip1)
	elseif CHATLOGIC.chatQuestion.state == 3 then
		self.msg.chatMsg = string.format('%s[f0361d](%s)[-]', self.msg.chatMsg, LanguageTip.chatQaTip2)
	end
	self:AddMsg(self.msg)
end
