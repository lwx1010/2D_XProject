OkTipPanel = {};
local this = OkTipPanel;
local OkTipCtrl = require("Controller/OkTipCtrl")
local gameObject;
local transform;

--由LuaBehaviour自动调用
function OkTipPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	
	this.okBtn = transform:Find('okBtn').gameObject
	this.closeBtn = transform:Find('closeBtn').gameObject
	this.contentLabel = transform:Find('contentLabel').gameObject
	this.titleLabel = transform:Find('titleLabel').gameObject
	this.mask = transform:Find('mask').gameObject
	this.isCreated = true
end

--由LuaBehaviour自动调用
function OkTipPanel.Start()

end

--由LuaBehaviour自动调用
function OkTipPanel.OnInit()
	LuaUIHelper.addUIDepth(gameObject , OkTipPanel)
end

--由LuaBehaviour自动调用
function OkTipPanel.OnClose()
	LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

--单击事件--
function OkTipPanel.OnDestroy()
	this.isCreated = false
end