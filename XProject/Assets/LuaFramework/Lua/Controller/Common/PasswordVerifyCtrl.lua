--[===[
Author: 柯明余
Time:   2017-03-14
Note:   密码验证通用窗口Ctrl层
]===]
local PasswordVerifyCtrl = {}
local this = PasswordVerifyCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil 
local callback = nil

function PasswordVerifyCtrl.New()
    return this
end

function PasswordVerifyCtrl.SetData(func)
    callback = func
end

function PasswordVerifyCtrl.Awake(closeMainCamera)
    createPanel("Common/PasswordVerify", this.OnCreate, closeMainCamera)
end

function PasswordVerifyCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    
    this.AddClickEvent()
end

function PasswordVerifyCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, PasswordVerifyPanel)
    PasswordVerifyPanel.inputField.value = ""
    PasswordVerifyPanel.inputField.defaultText = LANGUAGE_TIP.CommonKey0003
end

function PasswordVerifyCtrl.OnVerifyClick()
    if callback ~= nil then
        callback(PasswordVerifyPanel.inputLbl.text)
    end
    
    CtrlManager.ClosePanel("PasswordVerify")
end

function PasswordVerifyCtrl.OnReturnClick()
    CtrlManager.ClosePanel("PasswordVerify")
end

function PasswordVerifyCtrl.OnCloseClick()
    CtrlManager.ClosePanel("PasswordVerify")
end

function PasswordVerifyCtrl.OnClose()
    PasswordVerifyPanel.inputLbl.text = ""
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function PasswordVerifyCtrl.AddClickEvent()
    luaBehaviour:AddClick(PasswordVerifyPanel.verifyBtn, this.OnVerifyClick)
    luaBehaviour:AddClick(PasswordVerifyPanel.returnBtn, this.OnReturnClick)
    luaBehaviour:AddClick(PasswordVerifyPanel.closeBtn, this.OnCloseClick)
end

function PasswordVerifyCtrl.OnDestroy()
    callback = nil
end

return this