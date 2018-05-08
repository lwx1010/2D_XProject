local Network = Network
local COMMONCTRL = COMMONCTRL

Network["S2c_login_player_list"] = function(pb)
	log("服务器返回角色列表S2c_login_player_list")
	networkMgr:StartHeartBeat()
	local login = CtrlManager.GetCtrl(CtrlNames.Login)
	if login ~= nil then
		login.SetEnterMsg(LANGUAGE_TIP.EnterServer)
	end

	--log(TableToString(pb))
	if next(pb.list_info) == nil then
		--Util.LoadScene("CreateRoleScene")
		-- Network.isNewRole = true
		-- sceneMgr:LoadScene(CreateRoleStage.new())

		local cmd = {}
		local RandomName = require "language/RandomName"
		local curSex = math.random() > 0.5
		cmd.name = RandomName.create(curSex)
		cmd.head_id = 0
		cmd.profession = 1
		cmd.sex = curSex and 1 or 2

		Network.send("C2s_login_player_add", cmd)
	else
		local cmd = {}
		cmd.id = pb.list_info[1].id
		cmd.scene_name = {}
		local list = gameMgr:GetSceneFileList()
		if list.Length == 0 then
			cmd.isall = 1
		else
			cmd.isall = 0
			for i = 0, list.Length - 1 do
				table.insert(cmd.scene_name, list[i])
			end
		end
		Network.send("C2s_login_player_enter", cmd)

		--启动FightManager
		FIGHTMGR.Init()
		--启动主界面
		-- SkillUiPanel.show()
	end
end

Network["S2c_login_check_time"] = function(pb)
	log("收到S2c_login_check_time")
	log(TableToString(pb))
	TimeManager.instance:SetServerTime(pb.sec, pb.usec)
end

Network["S2c_login_playername"] = function(pb)
	log("S2c_login_playername")
	local CreateRoleCtrl = require "Controller/CreateRoleCtrl"
	CreateRoleCtrl.SetNameExist(pb.exist == 1)
end

Network["S2c_login_status"] = function(pb)

end

Network["S2c_login_error"] = function(pb)
	COMMONCTRL.RemoveQuanQuan()

	if panelMgr:IsPanelVisible("LoginPanel") then
		local wnd = MessageBox.DisplayMessageBox(LANGUAGE_TIP.LoginError..pb.err_desc
			, 0, null, null)

		local login = CtrlManager.GetCtrl(CtrlNames.Login)
		if login ~= nil then
			login.SetEnterMsg('')
			login.inConnect = false
		end
	elseif panelMgr:IsPanelVisible("CreateRolePanel") then
		CtrlManager.PopUpNotifyText(pb.err_desc, nil, nil, nil, nil, CreateRolePanel.notifyPanel)
	end

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