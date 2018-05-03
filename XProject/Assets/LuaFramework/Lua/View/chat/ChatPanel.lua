ChatPanel = {};
local this = ChatPanel;

local gameObject;
local transform;

--由LuaBehaviour自动调用
function ChatPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	
	this.chatListPanel = transform:Find('chatPanel').gameObject
	this.mailListPanel = transform:Find('mailPanel').gameObject

	this.mailBtn = transform:Find('mailBtn').gameObject
	this.mask = transform:Find('mask').gameObject
	this.hideBtn = transform:Find('hideBtn').gameObject

	this.zongheTab = transform:Find('zongheTab').gameObject
	this.systemTab = transform:Find('systemTab').gameObject
	this.worldTab = transform:Find('worldTab').gameObject
	this.nearTab = transform:Find('nearTab').gameObject
	this.gangTab = transform:Find('gangTab').gameObject
	this.teamTab = transform:Find('teamTab').gameObject
	this.campTab = transform:Find("campTab").gameObject
	this.privateTab = transform:Find('privateTab').gameObject
	this.playerNameTab = transform:Find('playerNameTab').gameObject
	this.playerNameTabLabel = transform:Find('playerNameTab/Label').gameObject
	this.pNameHighlightLabel = transform:Find('playerNameTab/highlight/Label').gameObject
	this.tip = transform:Find('mailBtn/tip').gameObject
	this.oncreate = true
end

function ChatPanel.OnInit()
	
end

--由LuaBehaviour自动调用
function ChatPanel.Start()

end

function ChatPanel.OnChangeSceneHide()
	CtrlManager.GetCtrl(CtrlNames.Chat).OnChangeSceneHide()
end

--单击事件--
function ChatPanel.OnDestroy()
	CtrlManager.GetCtrl(CtrlNames.Chat).Clear()
	this.oncreate = nil
end