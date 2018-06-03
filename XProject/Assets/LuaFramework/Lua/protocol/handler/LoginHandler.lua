local Network = Network
local COMMONCTRL = COMMONCTRL

Network["S2c_login_playerlist"] = function(pb)
	log("服务器返回角色列表S2c_login_playerlist")
	--networkMgr:StartHeartBeat()
	local login = CtrlManager.GetCtrl(CtrlNames.Login)
	if login ~= nil then
		login.SetEnterMsg(LANGUAGE_TIP.EnterServer)
	end

	log(TableToString(pb))
	if next(pb.pinfos) == nil then
		print("没有角色，进入创角")
		--Util.LoadScene("CreateRoleScene")
		-- Network.isNewRole = true
		-- sceneMgr:LoadScene(CreateRoleStage.new())

		local cmd = {}
		local RandomName = require "language/RandomName"
		local curSex = math.random() > 0.5
		cmd.name = RandomName.create(curSex)
		cmd.role_no = curSex and 1 or 2

		Network.send("C2s_login_player_add", cmd)
	else
		print("已有角色，直接进入游戏")
		local cmd = {}
		for i = 1, #pb.pinfos do
			print("需要实现多角色登陆")
			print(TableToString(pb.pinfos[i]))
		end
		cmd.uid = pb.pinfos[1].uid
		Network.send("C2s_login_player_enter", cmd)

		--启动FightManager
		--FIGHTMGR.Init()
		--启动主界面
		-- SkillUiPanel.show()
	end
end

--[[Network["S2c_login_check_time"] = function(pb)
	log("收到S2c_login_check_time")
	log(TableToString(pb))
	TimeManager.instance:SetServerTime(pb.sec, pb.usec)
end]]

Network["S2c_login_playername"] = function(pb)
	log("S2c_login_playername")
	local CreateRoleCtrl = require "Controller/CreateRoleCtrl"
	CreateRoleCtrl.SetNameExist(pb.exist == 1)
end

Network["S2c_login_status"] = function(pb)

end

Network["S2c_login_error"] = function(pb)
	COMMONCTRL.RemoveQuanQuan()

	MessageBox.Show(LANGUAGE_TIP.LoginError..pb.err_desc)
	--CtrlManager.PopUpNotifyText(LANGUAGE_TIP.LoginError..pb.err_desc)
	--logError(pb.err_desc)
end

--创建角色回调
Network["S2c_login_addplayer"] = function(pb)
	local Hero = require("Logic/Hero")
	Hero.CreateTime = pb.create_time

	if User_Config.internal_sdk == 1 then
		Hero.SubmitExtraData(2)
		local csm = CenterServerManager.Instance
		csm:StepLogRequest(4)
		csm:CreateRoleInfo(HERO.Name, HERO.Grade or 1, HERO.Uid, HERO.Vip or 0)
	end
end

--角色重连
Network["S2c_login_player_recon"] = function(pb)
	print("角色重连成功")
	-- WangfuSnatchManager.isInWangfuSnatchScene = false
	CtrlManager.PopUpNotifyText(LANGUAGE_TIP.ReconnectSuccess)
	COMMONCTRL.RemoveQuanQuan()
	networkMgr:StartHeartBeat()
	gameMgr:RestartUpatePack()
	Event.Brocast(EventName.DISCONNECT_AND_RECONNECT)
end

--角色被踢
Network["S2c_login_kickout"] = function(pb)
	print("角色被踢下线, place_holder:"..pb.place_holder)
	if pb.place_holder == 2 then
		networkMgr:KickOut(tonumber(Protocal.AccConflict))
	else
		networkMgr:KickOut(tonumber(Protocal.KickOut))
	end
end

--重新分配服务器Gateid
Network["S2c_login_serverinfo"] = function(pb)
	print("S2c_login_serverinfo"..TableToString(pb))
	AppConst.GatePort = pb.port
	AppConst.GateAddress = pb.ip and pb.ip ~= "" or AppConst.SocketAddress
    AppConst.GateSecret = pb.secret
    print(AppConst.GateAddress)
    print(AppConst.GatePort)
    networkMgr:KickOut(Protocal.ConnectToGate)
end

--正式进入游戏
Network["S2c_login_c2gateserver"] = function (pb)
	networkMgr:StartHeartBeat()
	print("从这里开始游戏主逻辑")
end