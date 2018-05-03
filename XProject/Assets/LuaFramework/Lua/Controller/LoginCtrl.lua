local COMMONCTRL = COMMONCTRL

local LoginCtrl = {}
local this = LoginCtrl

local login
local transform
local gameObject

local accInput
local popupList
local versionText
local isInit = false
local loginCollider
--构建函数--
function LoginCtrl.New()
	log("LoginCtrl.New--->>")
	return this
end

function LoginCtrl.Awake()
	isInit = false

	networkMgr.isKickOut = false
	networkMgr.inGame = false
	Util.AutoAdjustCameraRect(Camera.main)
	createPanel('Prefab/Gui/Login/LoginPanel', 1, this.OnCreate)
end

function LoginCtrl.Start()
	if User_Config.internal_sdk == 1 then
		SDKInterface.Instance.OnLoginSuc = this.OnLoginSuc;
		SDKInterface.Instance.OnLogout = this.OnLogout
		if CenterServerManager.Instance.UserID == "" then
			LoginPanel.loginCollider:SetActive(true)
			SDKInterface.Instance:Login()
		end
	end
end

--启动事件--
function LoginCtrl.OnCreate(obj)
	gameObject = obj
	transform = gameObject.transform

	--local panel = gameObject:GetComponent('UIPanel')
	--panel.depth = 2	--设置纵深--

	accInput = LoginPanel.accountInput:GetComponent('InputField')
	popupList = LoginPanel.serverList:GetComponent('Text')
	versionText = LoginPanel.version:GetComponent('Text')

	login = gameObject:GetComponent(typeof(LuaBehaviour))
	login:AddClick(LoginPanel.btnEnter, this.OnClick)
	login:AddClick(LoginPanel.btnNotices , this._onClickNotices)
	login:AddClick(LoginPanel.serverListBtn.gameObject , this._onClickServers)
	--login:AddClick(LoginPanel.loginCollider, this.OnLoginClick)

	local logoTex = LoginPanel.logoGo:GetComponent("Image")
	if User_Config.logoName and User_Config.logoName ~= "" then
        logoTex.sprite = resMgr.LoadSpriteAssets("Logo/" .. User_Config.logoName .. ".png")
        logoTex:SetNativeSize()
        --LoginPanel.logoGo.transform.localScale = Vector3.New(0.67, 0.67, 0.67)
    end
	--[[if Util.IsDebugBuild() == false then
		LoginPanel.passwordInput.gameObject:SetActive(true)
		LoginPanel.serverList.gameObject:SetActive(false)
	end]]

	versionText.text = string.format("%s：%s", LANGUAGE_TIP.Version, gameMgr.localVersion:ToString())

	this.SetDefaultAccount()
	this.SetServerList(User_Config.default_server)

	if User_Config.internal_sdk == 1 then
		LoginPanel.accountInput.gameObject:SetActive(false)
		LoginPanel.btnEnter.transform.localPosition = LoginPanel.serverList.transform.localPosition
		LoginPanel.serverList.transform.localPosition = LoginPanel.accountInput.transform.localPosition
	end

	this.SetEnterMsg("")
	isInit = true
	log("Start lua--->>"..gameObject.name)
end

function LoginCtrl.SetDefaultAccount()
	if User_Config.user_account == nil or User_Config.user_account == "" then
		accInput.text = Util.GetDeviceIdentifierString()
	else
		accInput.text = User_Config.user_account
	end
end

--- 设置当前的服务器列表
function LoginCtrl.SetServerList(serverNo)
	if not User_Config.serverList then return end

	local serverList = User_Config.serverList
	local lastServerList = User_Config.lastServerList
	local count = serverList.Count

	if count == 0 then return end

	if  User_Config.internal_sdk == 1 then
		if lastServerList.Count > 0 then
			local serverInfo0 = lastServerList:get_Item(0)
			this.setCurrentServer(serverInfo0)
		end
	else
		for i = 1, count do
			local serverInfo = serverList:get_Item(i - 1)
			if serverInfo.serverNo == serverNo then
				this.setCurrentServer(serverInfo)
				break
			end
		end
	end
	if not this.curServer then
		local serverInfo1 = serverList:get_Item(0)
		this.setCurrentServer(serverInfo1)
	end
end

--- 设置当前的服务器列表
--@param serverInfo
function LoginCtrl.setCurrentServer( serverInfo )
	popupList.text = serverInfo.serverName
	this.curServer = serverInfo
    User_Config.default_server = serverInfo.serverNo

	if User_Config.internal_sdk == 1 then
		local csm = CenterServerManager.Instance
		csm.Sid = serverInfo.serverNo
		csm.ServerName = serverInfo.serverName
		csm:StepLogRequest(3)
	end
end

function LoginCtrl.GetServerInfoByNo(server_no)
	local serverList = User_Config.serverList
	for i = 1, User_Config.serverList.Count do
		local serverInfo = serverList:get_Item(i - 1)
		if serverInfo.serverNo == server_no then
			return serverInfo
		end
	end
	return nil
end

--单击事件--
function LoginCtrl.OnClick(go)
	local defaultServerInfo = this.curServer
	if not defaultServerInfo or defaultServerInfo == nil then return end

    User_Config.SetLoginServerInfo(defaultServerInfo)
    User_Config.SetUserInfo(accInput.text, '')
    User_Config.Save()
	if accInput.text == nil or accInput.text == "" then
		return
	end
	if User_Config.internal_sdk == 1 then
		local csm = CenterServerManager.Instance
		if defaultServerInfo.status == 4 and not csm.IsWhite then  --非白名单用户
			CtrlManager.PopUpNotifyText(defaultServerInfo.tips)
			return
		end
	end
	AppConst.SocketPort = defaultServerInfo.serverPort;
    AppConst.SocketAddress = defaultServerInfo.serverIp;
    print("==============", defaultServerInfo.serverPort, defaultServerInfo.serverIp, defaultServerInfo.corpId, networkMgr)
    networkMgr:SendConnect();

	COMMONCTRL.CreateQuanQuan(transform.parent)
	--   if CtrlManager.GetCtrl("NetConnecting") == nil then
		-- local netConnectPrefab = Util.LoadPrefab("Prefab/Gui/NetConnecting")
		-- local netConnectGo = newObject(netConnectPrefab)
		-- netConnectGo.transform:SetParent(transform.parent)
		-- netConnectGo.transform.localScale = Vector3.New(1, 1, 1)
		-- CtrlManager.AddCtrl("NetConnecting", netConnectGo)
  	--   end
  	this.SetEnterMsg(LANGUAGE_TIP.ConnectingServer)
    this.WaitForConnect()

	print("进入游戏按钮~~~~~~~~~~~~~~~~~~~~~~")
end

function LoginCtrl.SetEnterMsg(msg)
	if LoginPanel.msgLabel ~= nil then
		LoginPanel.msgLabel.text = msg
	end
end

--点击公告入口
function LoginCtrl._onClickNotices(  )
	NoticesPanel.show()
	print("点击公告入口~~~~~~~~~~~~~~~~~")
end

--点击服务器选择界面入口
function LoginCtrl._onClickServers( )
	ServersPanel.show()
	print("选择服务器界面~~~~~~~~~~~~~~~~~~~~~~~~~~~")
end

function LoginCtrl.OnLoginClick()
	SDKInterface.Instance:Login()
end

function LoginCtrl.WaitForConnect()
	this.startTime = TimeManager.GetRealTimeSinceStartUp()
	this.inConnect = true
	UpdateBeat:Add(this.Update,this)
end

function LoginCtrl.Update()
	if this.inConnect and TimeManager.GetRealTimeSinceStartUp() - this.startTime > 20 then
		this.inConnect = false
		this.startTime = 0
		networkMgr:Close()
		MessageBox.DisplayMessageBox(LANGUAGE_TIP.ConnectServerError, 0, function()
			this.SetEnterMsg('')
			this.StopCheckTimeout()
		end, null)
	end
end

function LoginCtrl.StopCheckTimeout()
	COMMONCTRL.RemoveQuanQuan()
	UpdateBeat:Remove(this.Update,this)
end

function LoginCtrl.OnConnectCallback()
	-- print("-=-------------------------------------", User_Config.internal_sdk, this.curServer.hostId)
	--服务端时间检查
	Network.send("C2s_login_check_time", {place_holder=1})

	-- 发送登陆信息
    local account_info = {}

	if User_Config.internal_sdk == 1 then	--sdk
		local csm = CenterServerManager.Instance
		account_info.corp_id = csm.Pid == 0 and 1000 or csm.Pid
		account_info.login_time = csm.Time
		account_info.adult = 1
		account_info.acct = csm.AccName
		account_info.sign = csm.Token
		account_info.extdata = string.format("jihuo=1|serverid=%s|acct_id=%s|servername=%s|appid=%s", csm.Sid, csm.AccName, this.curServer.serverName, csm.AppID)
		csm:SetLastServer()
	else
		account_info.corp_id = this.curServer.corpId
		account_info.login_time = os.time()
		account_info.adult = 0
		account_info.acct = accInput.text
		account_info.sign = ""
		account_info.extdata = string.format("jihuo=1|serverid=%s|acct_id=%s|servername=%s", this.curServer.hostId, accInput.text, this.curServer.serverName)
	end
    Network.send("C2s_login_corp_account", account_info)
end

--登录成功回调
function LoginCtrl.OnLoginSuc(result)
	local csm = CenterServerManager.Instance
	if not result.isSuc then
		print("登录失败")
		LoginPanel.loginCollider:SetActive(true)
		return
	end
	if result.isSwitchAccount then
		print("切换帐号成功: ", result.token)
		csm.UserID = ""
		LoginPanel.loginCollider:SetActive(true)
	else
		print("登录成功: ", result.token)
		csm.UserID = result.userID
        csm.Token = result.token
        csm.AccName = result.userID
        csm.Mac = SDKInterface.Instance:GetMacAddr()
        local addr = SDKInterface.Instance:GetIpAddr()
        local temps = string.split(addr, '%')
        if #temps > 0 then
			csm.Ip = temps[1]
		else
			csm.Ip = addr
		end
		print("UserID: ", result.userID)
		csm:LoginRequest(function()
			LoginPanel.loginCollider:SetActive(false)
            TimeManager.instance:SetServerTime(csm.Time.."000", 0)			--单位毫秒
            csm:StepLogRequest(2)
			csm:ButtonControll()
			--显示公告
			NoticeModel.inst:getCenterServerNotices()
			if isInit then
				this.SetDefaultAccount()
				this.SetServerList(User_Config.default_server)
			end
		end)
	end
end

--登出回调
function LoginCtrl.OnLogout()
	print(">>>>>>>>>>>>>>>>>>>>>>>登出")
	CenterServerManager.Instance.UserID = ""
	LoginPanel.loginCollider:SetActive(true)
end

return LoginCtrl