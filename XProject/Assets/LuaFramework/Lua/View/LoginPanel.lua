local transform;
local gameObject;

LoginPanel = {};
local this = LoginPanel;

--启动事件--
function LoginPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	log("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function LoginPanel.InitPanel()
	this.btnEnter = transform:Find("enterbtn").gameObject;
	this.accountInput = transform:Find("accountinput")
	this.passwordInput = transform:Find("passwordinput")
	this.serverListBtn = transform:Find("serverlist")
	this.serverList = transform:Find("serverlist/Label")
	this.showMsgGo = transform:Find("showmsg")
	this.version = transform:Find("version")
	this.btnNotices = transform:Find("noticesbtn").gameObject
	--this.bgTexture = transform:Find("BgTexture").gameObject
	this.logoGo = transform:Find("Logo").gameObject
	this.msgLabel = this.showMsgGo:GetComponent('Text')
	--this.loginCollider = transform:Find("LoginCollider").gameObject
end

function LoginPanel.Start()
	local LoginCtrl = CtrlManager.GetCtrl("LoginCtrl")
	LoginCtrl.Start()
end

--单击事件--
function LoginPanel.OnDestroy()
	local login = CtrlManager.GetCtrl(CtrlNames.Login)
	if login ~= nil then
		login.StopCheckTimeout()
		login.SetEnterMsg("")
	end
	if panelMgr then
		panelMgr:ClosePanel("Login")
	end
	this.msgLabel = nil
	COMMONCTRL.RemoveQuanQuan()
end