ChatSettingPanel = {};
local this = ChatSettingPanel;

local gameObject;
local transform;

--由LuaBehaviour自动调用
function ChatSettingPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	
	this.systemCb = transform:Find('systemCheckbox').gameObject
	this.worldCb = transform:Find('worldCheckbox').gameObject
	this.nearCb = transform:Find('nearCheckbox').gameObject
	this.gangCb = transform:Find('gangCheckbox').gameObject
	this.teamCb = transform:Find('teamCheckbox').gameObject
	this.strangerCb = transform:Find('strangerCheckbox').gameObject
	this.voiceToTextCb = transform:Find('voiceToTextCheckbox').gameObject
	this.worldVoiceCb = transform:Find('worldVoiceCheckbox').gameObject
	this.nearVoiceCb = transform:Find('nearVoiceCheckbox').gameObject
	this.gangVoiceCb = transform:Find('gangVoiceCheckbox').gameObject
	this.teamVoiceCb = transform:Find('teamVoiceCheckbox').gameObject
	this.closeBtn = transform:Find('closeBtn').gameObject
	this.okBtn = transform:Find('okBtn').gameObject
	this.mask = transform:Find('mask').gameObject
end

--由LuaBehaviour自动调用
function ChatSettingPanel.Start()

end

function ChatSettingPanel.OnInit()
	LuaUIHelper.addUIDepth(gameObject , ChatSettingPanel)
end

function ChatSettingPanel.OnClose()
	LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

--单击事件--
function ChatSettingPanel.OnDestroy()
	CtrlManager.GetCtrl(CtrlNames.ChatSetting).OnDestroy()
end