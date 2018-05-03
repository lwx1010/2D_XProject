local TipLogic = require('Logic/TipLogic')
TipPanel = {};
local this = TipPanel;

local gameObject;
local transform;

--由LuaBehaviour自动调用
function TipPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	
	this.okBtn = transform:Find('okBtn').gameObject
	this.noBtn = transform:Find('noBtn').gameObject
	this.closeBtn = transform:Find('closeBtn').gameObject
	this.titleLabel = transform:Find("titleLabel")
	this.contentLabel = transform:Find('contentLabel').gameObject
	this.mask = transform:Find('mask').gameObject
end

--由LuaBehaviour自动调用
function TipPanel.Start()

end

--由LuaBehaviour自动调用
function TipPanel.OnInit()
	LuaUIHelper.addUIDepth(gameObject , TipPanel)
end

--由LuaBehaviour自动调用
function TipPanel.OnClose()
	LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

--单击事件--
function TipPanel.OnDestroy()
	TipLogic.RemoveNormalTip()
	TipLogic.isCarent = false
end