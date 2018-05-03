local ChatOperateCtrl = require('Controller/chat/ChatOperateCtrl')
local ChatData = require('Model/ChatData')
local LanguageTip = LANGUAGE_TIP
ChatVoiceMsgBox = {}

local BaseWidth = 88
local MaxWidth = 500

local channelSprNames = {
	[6] = 'ui_chat_icon_system',
	[3] = 'ui_chat_icon_duiwu',
	[1] = 'ui_chat_icon_world',
	[2] = 'ui_chat_icon_banghui',
	[5] = 'ui_chat_icon_fujin',
	[4] = 'ui_chat_icon_miliao',
	[7] = 'ui_chat_icon_laba',
}

function ChatVoiceMsgBox:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ChatVoiceMsgBox:OnCreate(go)
	self.gameObject = go
	self.transform = go.transform
	self.behaviour = go.transform:GetComponent('LuaComponent')
	self.msgBgSpr = go.transform:Find('msgbg'):GetComponent('UISprite')
	self.headSpr = go.transform:Find('headSpr'):GetComponent('UISprite')
	self.channelSpr = go.transform:Find('table/channelSprite'):GetComponent('UISprite')
	self.vipGo = go.transform:Find('table/vip').gameObject
	self.nameLabel = go.transform:Find('table/nameLabel'):GetComponent('UILabel')
	self.voiceTimeLabel = go.transform:Find('voicelength'):GetComponent('UILabel')
	self.redtip = go.transform:Find('redtip').gameObject
	self.gradeLabel = go.transform:Find('gradeLabel'):GetComponent('UILabel')
	self.behaviour:AddClick(self.headSpr.gameObject, function() 
		ChatOperateCtrl.ShowOperatePanel(self.msg.chatName, self.msg.chatUid, self.msg.grade, false)
	end)
	self.behaviour:AddClick(self.msgBgSpr.gameObject, function(go)
		CHATLOGIC.StopAutoPlayVoice()
		CHATLOGIC.ClearVoiceRedTip(self.msg.voiceSn)
		self.redtip:SetActive(false)
		ChatVoiceService:YuYinClick(go, self.voiceTime)
	end)
end

function ChatVoiceMsgBox:AddMsg(channel, msg)
	if msg.voiceSn == '' then return end
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
	self.headSpr.spriteName = tostring(msg.head)
	self.nameLabel.text = msg.chatName
	self.nameLabel:UpdateNGUIText()
	self.nameLabel.width = math.ceil(NGUIText.CalculatePrintedSize(msg.chatName, MaxWidth).x)+2
	self.voiceTime = msg.voiceTime
	self.voiceTimeLabel.text = tostring(msg.voiceTime)..'\"'
	local width = self.msgBgSpr.width + msg.voiceTime * 10
	if MaxWidth - width < 36 then
		width = MaxWidth
	end
	width = math.max(BaseWidth, width)
	self.msgBgSpr.width = width
	local collider = self.msgBgSpr.gameObject:AddComponent(typeof(UnityEngine.BoxCollider))
	collider.size = Vector3.New(width, self.msgBgSpr.height)
	collider.center = Vector3.New(width/2*(msg.key == 1 and -1 or 1), -self.msgBgSpr.height/2)
	self.gradeLabel.text = tostring(msg.grade)
	--创建一个GameObject来保存sn
	local snGo = GameObject(msg.voiceSn)
	snGo.transform:SetParent(self.msgBgSpr.gameObject.transform)
	snGo.transform.localScale = Vector3.one
	snGo.transform.localPosition = Vector3.zero
	if channel ~= ChatData.ALL_CHANNEL and ChatData.GetChannelVoiceAutoPlay(channel) and msg.key ~= 1 then
		CHATLOGIC.AddAutoPlayVoice(self)
	end
	if msg.key ~= 1 then
		CHATLOGIC.voiceCtrlTbl[msg.voiceSn]  = CHATLOGIC.voiceCtrlTbl[msg.voiceSn] or {}
		table.insert(CHATLOGIC.voiceCtrlTbl[msg.voiceSn], self)
	end
end

function ChatVoiceMsgBox:AutoPlayVoice()
	self.redtip:SetActive(false)
	ChatVoiceService:YuYinClick(self.msgBgSpr.gameObject, self.voiceTime)
end