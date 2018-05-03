ChatTextMsgBox3 = {}
local ChatData = require('Model/ChatData')
local MaxWidth = 504

local offsetX = 2
local offsetY = 0
local vipSpace = '[ffffff00],,,,[-]'
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

local channelColors = {
	[1] = 'ebd6bb',
	[2] = '51d14f',
	[3] = '3dbdff',
	[4] = 'eb7cf5',
	[5] = 'b9e6e6',
	[6] = 'f0361d',
	[7] = 'ffd926',
	[8] = '3dbdff',
}

function ChatTextMsgBox3:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ChatTextMsgBox3:OnCreate(go)
	self.gameOject = go
	self.transform = go.transform
	self.boxWidget = go.transform:GetComponent('UIWidget')
	self.channelSpr = go.transform:Find('channelSpr'):GetComponent('UISprite')
	self.msgLabel = go.transform:Find('msgLabel'):GetComponent('UILabel')
	self.voiceTimeLabel = go.transform:Find('voiceLength'):GetComponent('UILabel')
	self.vipGo = go.transform:Find('vip').gameObject
end

function ChatTextMsgBox3:AddMsg(msg)
	local channel = msg.channel
	if msg.sendType == ChatData.SEND_TYPE_BIG then
		channel = 7
	end
	self.channelSpr.spriteName = channelSprNames[channel]
	local str = msg.chatMsg
	if msg.chatName ~= '' then
		str = string.format('[ffd926]%s[-]:', msg.chatName)
	end
	if msg.vip > 0 then
		str = string.format("%s%s", vipSpace, str)
		self.vipGo:SetActive(true)
	end
	self.msgLabel.text = str
	self.msgLabel:UpdateNGUIText()
	str = string.format('[%s]%s[-]', channelColors[channel], str)
	local width = math.ceil(NGUIText.CalculatePrintedSize(str, MaxWidth).x)
	if MaxWidth - width < 36 then
		width = MaxWidth
	end
	self.msgLabel.width = width + offsetX
	self.msgLabel.height = NGUIText.CalculatePrintedSize(str, MaxWidth).y + offsetY
	self.msgLabel.text = str
	self.voiceTimeLabel.text = string.format('[%s]%s[-]', channelColors[channel], tostring(msg.voiceTime)..'\"')
	self.voiceTimeLabel:UpdateNGUIText()
end

