local RollTextCtrl = {} --require('Controller/RollTextCtrl')
local LabaRollTextCtrl = {} --require('Controller/LabaRollTextCtrl')
local Network = Network
local hero = HERO

Network["S2c_chat_initvoice"] = function(pb)
    User_Config.soundIp = pb.ip;
    User_Config.soundPort = pb.port;
    User_Config.soundToken = pb.token;
end

Network["S2c_chat_system"] = function(pb)
	log(TableToString(pb))
	--1系统广播（跑马灯）, 2提示信息 3消息框，需要确认消失 4系统信息 5公告信息 6特殊系统消息(处理类似夫妻上下线提示需要在主界面聊天框显示的信息)
	if pb.type == 1 then
		CHATLOGIC.ReceiveSystemMsg(pb)
		RollTextCtrl.ShowRollText(pb.msg, false)
	elseif pb.type == 2 then 
		CtrlManager.PopUpNotifyText(pb.msg)
		--CHATLOGIC.ReceiveSystemMsg(pb)
	elseif pb.type == 4 then
		CHATLOGIC.ReceiveSystemMsg(pb)
	elseif pb.type == 6 then
		CHATLOGIC.ReceiveSystemMsg(pb, true)
	end
end

--公共消息
Network["S2c_chat_public"] = function(pb)
	--大喇叭
	if pb.send_type == 1 then
		LabaRollTextCtrl.ShowRollText(pb.chat_name, pb.chat_msg, false)
	end
	CHATLOGIC.ReceivePublicMsg(pb)
end

--收到广播的密聊消息
Network["S2c_chat_private"] = function(pb)
	CHATLOGIC.ReceivePrivateMsg(pb)
end

--密聊列表
Network["S2c_chat_private_list"] = function(pb)
	local PrivateChatListCtrl = require('Controller/PrivateChatListCtrl')
	PrivateChatListCtrl.ShowTargetList(pb.private_user_list)
end

Network["S2c_chat_answer_question"] = function(pb)
	CHATLOGIC.RecieveChatQuestion(pb)
end

Network["S2c_aoi_chat_system"] = function(pb)
	--1系统广播（跑马灯）, 2提示信息 3消息框，需要确认消失 4系统信息 5公告信息 6特殊系统消息(处理类似夫妻上下线提示需要在主界面聊天框显示的信息)
	if pb.type == 1 then
		CHATLOGIC.ReceiveSystemMsg(pb)
		RollTextCtrl.ShowRollText(pb.msg, false)
	elseif pb.type == 2 then 
		CtrlManager.PopUpNotifyText(pb.msg)
		--CHATLOGIC.ReceiveSystemMsg(pb)
	elseif pb.type == 4 then
		CHATLOGIC.ReceiveSystemMsg(pb)
	elseif pb.type == 6 then
		CHATLOGIC.ReceiveSystemMsg(pb, true)
	end
end
