--[===[
Author: 柯明余
Time:   2017-03-15
Note:   选择文本通用窗口Ctrl层
]===]
local SelectTextCtrl = {}
local this = SelectTextCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil

local mTitle = ""                --标题
local mDescribe = ""             --描述文本
local mFontSize = 30             --描述文本字体大小
local mLeftBtnText = ""          --左按钮文本
local mRightBtnText = ""         --右按钮文本
local mLeftBtnFunc = nil         --左按钮回调
local mRightBtnFunc = nil        --右按钮回调
local mCloseBtnFunc = nil        --关闭按钮回调
--local mAlignment = NGUIText.Alignment.Center

this.isEnabled = false

function SelectTextCtrl.New()
    return this
end

function SelectTextCtrl.SetData(title, describe, fontSize, leftBtnText, rightBtnText, leftBtnFunc, rightBtnFunc, closeBtnFunc, alignment)
    mTitle = title
    mDescribe = describe
    mFontSize = fontSize
    mLeftBtnText = leftBtnText
    mRightBtnText = rightBtnText
    mLeftBtnFunc = leftBtnFunc
    mRightBtnFunc = rightBtnFunc
    mCloseBtnFunc = closeBtnFunc
    --mAlignment = alignment
end

function SelectTextCtrl.Awake(closeMainCamera)
    createPanel("Common/SelectText", this.OnCreate, closeMainCamera)
end

function SelectTextCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    this.AddClickEvent()
    this.AddEvent()
end

function SelectTextCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, SelectTextPanel)
    soundMgr:PlayUISound("panel-open")
    --SelectTextPanel.describeLbl.alignment = mAlignment == nil and NGUIText.Alignment.Center or mAlignment
    SelectTextPanel.titleLbl.text = mTitle
    SelectTextPanel.describeLbl.text = mDescribe
    SelectTextPanel.describeLbl.fontSize = mFontSize
    SelectTextPanel.leftBtnLbl.text = mLeftBtnText
    SelectTextPanel.rightBtnLbl.text = mRightBtnText
    this.isEnabled = true
end

function SelectTextCtrl.OnCloseClick(go)
    CtrlManager.ClosePanel("SelectText")
    if mCloseBtnFunc then
        mCloseBtnFunc()
    end
end

function SelectTextCtrl.OnLeftClick()
    if mLeftBtnFunc ~= nil then
        mLeftBtnFunc()
    end
end 

function SelectTextCtrl.OnRightClick()
    if mRightBtnFunc ~= nil then
        mRightBtnFunc()
    end
end

function SelectTextCtrl.AddClickEvent()
    luaBehaviour:AddClick(SelectTextPanel.closeBtn, this.OnCloseClick)
    luaBehaviour:AddClick(SelectTextPanel.leftBtn, this.OnLeftClick)
    luaBehaviour:AddClick(SelectTextPanel.rightBtn, this.OnRightClick)
end

function SelectTextCtrl.AddEvent()
    Event.AddListener(EventName.CLOSE_WINDOW_EVENT, this.OnCloseEvent)
end

function SelectTextCtrl.RemoveEvent()
    Event.RemoveListener(EventName.CLOSE_WINDOW_EVENT, this.OnCloseEvent)    
end

function SelectTextCtrl.OnClose()
    this.isEnabled = false
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function SelectTextCtrl.OnDestroy()
    mLeftBtnFunc = nil
    mRightBtnFunc = nil
    this.RemoveEvent()
end

function SelectTextCtrl.OnCloseEvent()
    CtrlManager.ClosePanel("SelectText")
end

return this