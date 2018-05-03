local ChatData = require('Model/ChatData')
local hero = HERO
require('Model/ChatMsgBox')
require('Model/ChatTextMsgBox')
require('Model/ChatVoiceMsgBox')
ChatScrollView = {}


function ChatScrollView:New(o)
	o = o or {}
	setmetatable(o, ChatScrollView)
	self.__index = ChatScrollView
	return o
end

function ChatScrollView:OnCreate(go)
	self.gameOject = go
	self.transform = go.transform

	self.listTrans = go.transform:Find('list')
	self.listTable = go.transform:Find('list'):GetComponent('UITable')
	self.listTable.onReposition = handler(self, self.onReposition)
	self.timeStampCount = 0
end

function ChatScrollView:OnEnable()
	if self.listTable then
		self.listTable.repositionNow = true
	end
end

function ChatScrollView:AddTextWithBg(channel, msg)
	local prefabName = ''
	if msg.chatUid == '' then
		prefabName = 'Prefab/Gui/chat/TextMsgBox'
	elseif msg.chatUid == hero.Uid then
		if msg.voiceSn == '' then
			prefabName = 'Prefab/Gui/chat/RightMsgBox'
		else
			prefabName = 'Prefab/Gui/chat/RightVoiceMsgBox'
		end
		msg.key = 1
	else
		if msg.voiceSn == '' then
			prefabName = 'Prefab/Gui/chat/LeftMsgBox'
		else
			prefabName = 'Prefab/Gui/chat/LeftVoiceMsgBox'
		end
		msg.key = 2
	end
	if self.listTrans.childCount - self.timeStampCount >= ChatData.MSG_MAX_COUNT[channel] then
		local child = self.listTrans:GetChild(0).gameObject
		if child.name == "TimeStamp" then
			destroy(child)
			self.timeStampCount = self.timeStampCount - 1
		end
		destroy(self.listTrans:GetChild(0).gameObject)
	end
	if ChatData.NeedToRecordTime(msg) then --打下时间刻度
		local timeGo = newObject(Util.LoadPrefab("Prefab/Gui/chat/TimeItem"))
		timeGo.name = "TimeStamp"
		timeGo.transform:SetParent(self.listTrans)
		timeGo.transform.localScale = Vector3.one
		timeGo.transform.localPosition = Vector3.zero
		local timeLabel = timeGo.transform:Find('Label'):GetComponent("UILabel")
		timeLabel.text = os.date("%H:%M", msg.chatTime)
		self.timeStampCount = self.timeStampCount + 1
	end
	local msgGo = newObject(Util.LoadPrefab(prefabName))
	msgGo.transform:SetParent(self.listTrans)
	msgGo.transform.localScale = Vector3.one
	msgGo.transform.localPosition = Vector3.zero
	self.listTable.repositionNow = true
	local ctrl
	if msg.chatUid == '' then
		ctrl = ChatTextMsgBox:New()
	elseif msg.voiceSn == '' then
		ctrl = ChatMsgBox:New()
	else
		ctrl = ChatVoiceMsgBox:New()
	end
	LuaComponent.Add(msgGo, ctrl)
	ctrl:AddMsg(channel, msg)
	self.channel = channel
	self.key = msg.key
	self.listTable.repositionNow = true
end

--查找本地的该对象的密聊消息，显示出来
function ChatScrollView:ShowPrivateChatMsgList(uid)
	self.timeStampCount = 0
	local childCount = self.listTrans.childCount
	for i=childCount, 1, -1 do
		destroy(self.listTrans:GetChild(i-1).gameObject)
	end
	self.listTable.repositionNow = true
	local list = ChatData.privateMsgList[uid]
	if list then
		local count = #list
		for i=1, count do
			self:AddTextWithBg(ChatData.PLAYER_CHANNEL, list[i])
		end
	end
end

function ChatScrollView:onReposition()
	CHATLOGIC.UpdateMsgScrollList(self.channel, self.key == 1)
end

function ChatScrollView:Clear()
	-- local childCount = self.listTrans.childCount
	-- for i=childCount, 1, -1 do
	-- 	destroy(self.listTrans:GetChild(i-1).gameObject)
	-- end
	-- self.listTable.repositionNow = true
end