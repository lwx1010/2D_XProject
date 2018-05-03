local hero = HERO
local ChatData = {}
local this = ChatData

ChatData.ALL_CHANNEL = 0		--综合
ChatData.WORLD_CHANNEL = 1		--世界
ChatData.GANG_CHANNEL = 2		--帮派
ChatData.TEAM_CHANNEL = 3		--队伍
ChatData.PRIVATE_CHANNEL = 7	--密聊
ChatData.NEAR_CHANNEL = 5		--附近
ChatData.SYSTEM_CHANNEL = 6		--系统
ChatData.PLAYER_CHANNEL = 4		--私聊玩家频道
ChatData.CAMP_CHANNEL = 8       --阵营

ChatData.MAX_MSG_COUNT = 20
local TIME_INTERVAL = 120   --需要打下时间刻度的间隔值

--ChatData.soundIp = nil
--ChatData.soundPort = nil
--ChatData.soundToken = nil

ChatData.allMsgList = {}
ChatData.worldMsgList = {}
ChatData.gangMsgList = {}
ChatData.nearMsgList = {}
ChatData.systemMsgList = {}
ChatData.privateMsgList = {}
ChatData.teamMsgList = {}
ChatData.voiceMsgList = {}
ChatData.privateRedTip = {}
ChatData.campMsgList = {}

ChatData.historyMsgList = {}

ChatData.Settings = {
	['recieveSystemChannel'] = true,
	['recieveWorldChannel'] = true,
	['recieveNearChannel'] = true,
	['recieveGangChannel'] = true,
	['recieveTeamChannel'] = true,
	['recieveStrangerChannel'] = true,
	['autoVoiceToText'] = false,
	['autoPlayWorldVoice'] = false,
	['autoPlayNearVoice'] = false,
	['autoPlayGangVoice'] = false,
	['autoPlayTeamVoice'] = false,
}

ChatData.Channels = {
	['zongheTab'] = 0,
	['worldTab'] = 1,
	['gangTab'] = 2,
	['teamTab'] = 3,
	['privateTab'] = 7,
	['nearTab'] = 5,
	['systemTab'] = 6,
	['playerNameTab'] = 4,
    ['campTab'] = 8,
}

ChatData.MSG_SEND_TIME_INTERVAL = {
    [1] = 10,
    [2] = 2,
    [3] = 3,
    [4] = 3,
    [5] = 5,
    [8] = 5,
}

ChatData.MSG_MAX_COUNT = {
    [0] = 50,
    [1] = 50,
    [2] = 50,
    [3] = 50,
    [4] = 50,
    [5] = 50,
    [6] = 50,
    [8] = 50,
}

ChatData.SEND_TYPE_NORMAL = 0
ChatData.SEND_TYPE_BIG = 1
ChatData.SEND_TYPE_SMALL = 2

function ChatData.AddMsg(channel, sendtype, uid, name, content, sex, photo, vip, sn, time, grade, clubId, teamId, chatTime, hostId)
    local msgList = this.GetMsgList(channel);
    if not msgList then return end
    local msg = {}
    msg.channel = channel
    msg.sendType = sendtype or 0
    msg.chatName = name or ''
    msg.chatUid = uid or ''
    msg.chatSex = sex or 0
    msg.chatMsg = content or ''
    msg.voiceSn = sn or ''
    msg.grade = grade or 0
    msg.clubId = clubId or 0
    msg.chatTime = os.time()
    msg.teamId = teamId or 0
    msg.voiceTime = time or 0
    msg.head = photo
    msg.vip = vip
    msg.hasPlay = true
    msg.hostId = hostId or 0

    table.insert(msgList, msg)

    if #msgList > this.MSG_MAX_COUNT[channel] then
        local firstMsg = msgList[1];
        if firstMsg.voiceSn ~= '' then
            this.RemoveYuYinData(firstMsg.voiceSn);
        end
        table.remove(msgList, 1)
	end   
    return msg
end

--私聊信息--根据对象ID存储
function ChatData.AddPrivateMsg(pb)
	local targetId = pb.src_uid == hero.Uid and pb.dst_uid or pb.src_uid
    if not this.privateMsgList[targetId] then 
    	this.privateMsgList[targetId] = {}
        this.privateRedTip[targetId] = pb.src_uid ~= HERO.Uid and 1 or 0
    end
    local msgList = this.privateMsgList[targetId]  
    local msg = {}
    msg.channel = ChatData.PLAYER_CHANNEL
    msg.chatName = pb.src_name
    msg.chatUid = pb.src_uid
    msg.targetId = targetId
    msg.chatSex = pb.sex
    msg.chatMsg = pb.chat_msg
    msg.grade = pb.grade
    msg.chatTime = pb.chat_time
    msg.voiceSn = pb.voice_sn or ''
    msg.voiceTime = pb.voice_time or 0
    msg.head = pb.photo
    msg.vip = pb.vip
    msg.hasPlay = true
    msg.hostId = pb.host_id or 0

    table.insert(msgList, msg)

    if #msgList > this.MSG_MAX_COUNT[ChatData.PLAYER_CHANNEL] then
        local firstMsg = msgList[1];
        if firstMsg.voiceSn ~= '' then
            this.RemoveYuYinData(firstMsg.voiceSn);
        end
        table.remove(msgList, 1)
	end  
    if pb.src_uid ~= HERO.Uid then
        if CHATLOGIC.curShowChannel == ChatData.PLAYER_CHANNEL then
            if CHATLOGIC.curPrivateTarget and CHATLOGIC.curPrivateTarget.Uid ~= pb.src_uid then
                this.privateRedTip[targetId] = 1
            end
        else
            this.privateRedTip[targetId] = 1 
        end
        if CHATLOGIC.curShowChannel == ChatData.PRIVATE_CHANNEL then
            local PrivateChatListCtrl = require('Controller/PrivateChatListCtrl')
            PrivateChatListCtrl.UpdateRedTips()
        end
    end

    return msg
end

function ChatData.HasNewPrivateMsg()
    for i, v in pairs(ChatData.privateRedTip) do
        if v == 1 then return true end
    end
    return false
end

function ChatData.GetMsgList(channel)
	if channel == 0 then
		return ChatData.allMsgList
	elseif channel == 1 then
		return ChatData.worldMsgList
	elseif channel == 2 then
		return ChatData.gangMsgList
	elseif channel == 3 then
		return ChatData.teamMsgList
	elseif channel == 4 then
		return ChatData.privateMsgList
	elseif channel == 5 then
		return ChatData.nearMsgList
	elseif channel == 6 then
		return ChatData.systemMsgList
    elseif channel == 8 then
        return ChatData.campMsgList
	end
end

function ChatData.GetChannelVoiceAutoPlay(channel)
    if channel == 0 then
        return false
    elseif channel == 1 then
        return ChatData.Settings.autoPlayWorldVoice
    elseif channel == 2 then
        return ChatData.Settings.autoPlayGangVoice
    elseif channel == 3 then
        return ChatData.Settings.autoPlayTeamVoice
    elseif channel == 4 then
        return false
    elseif channel == 5 then
        return ChatData.Settings.autoPlayNearVoice
    elseif channel == 6 then
        return false
    elseif channel == 8 then
        return false
    end
end

function ChatData.RemoveYuYinData(sn)
	-- body
    ChatVoiceService:RemoveYuYinData(sn)
end

function ChatData.NeedToRecordTime(msg)
    local msgList = this.GetMsgList(msg.channel)
    if not msgList then return true end
    if msg.channel == this.PLAYER_CHANNEL then
        msgList = msgList[msg.targetId]
        if not msgList then return true end
    end
    local count = #msgList
    for i=1, count do
        if msgList[i].chatTime == msg.chatTime then
            if not msgList[i-1] or msg.chatTime - msgList[i-1].chatTime >= TIME_INTERVAL then
                return true 
            else
                return false
            end
        end
    end
    return true
end

return ChatData