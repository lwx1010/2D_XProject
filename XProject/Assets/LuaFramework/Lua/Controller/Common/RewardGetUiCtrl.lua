local LANGUAGE_TIP = LANGUAGE_TIP
require("Model/RewardItemCtrl")

local RewardGetUiCtrl = {}
local this = RewardGetUiCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil

local receiveTime = 0
local leftTime = 0
local confirmFunc

local needUpdate 

function RewardGetUiCtrl.New()
    return this
end

function RewardGetUiCtrl.Awake(closeMainCamera)
    createPanel("Common/RewardGetUi", this.OnCreate, closeMainCamera, 0, 0, 0, false, false)
end

function RewardGetUiCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform

    local panel = transform:GetComponent('UIPanel')
    panel.startingRenderQueue = CtrlManager.panelDepth + CtrlManager.startRenderQ
    panel.depth =  CtrlManager.panelDepth

    luaBehaviour = transform:GetComponent("LuaBehaviour")
    luaBehaviour:AddClick(RewardGetUiPanel.closeBtn, this.OnRewardBtnClick)
    luaBehaviour:AddClick(RewardGetUiPanel.rewardBtn, this.OnRewardBtnClick)

    this.titleLab = RewardGetUiPanel.titleLab:GetComponent("UILabel")
    this.timeLab = RewardGetUiPanel.timeLab:GetComponent("UILabel")

    this.rewardGrid = RewardGetUiPanel.rewardGrid:GetComponent("UIGrid")

    this.rewards = {}
    for i = 1, 3 do
        local reward = RewardItemCtrl:New()
        reward:OnCreate(this.rewardGrid.transform, string.format("RewardItem_%s", i)
            , luaBehaviour, 2)
        table.insert(this.rewards, reward)
    end

    UpdateBeat:Add(this.Update, this)

    if this.CreateCallBack then
        this.CreateCallBack()
        this.CreateCallBack = nil
    end
end

function RewardGetUiCtrl.Update()
    if not needUpdate then
        return
    end

    if (receiveTime + leftTime) >= Time.time then
        this.timeLab.text = string.format("%s%s", math.floor(receiveTime + leftTime - Time.time), LANGUAGE_TIP.RewardGetTip)
    else
        this.OnRewardBtnClick()
        this.timeLab.text = ""
        needUpdate = false
    end
end

function RewardGetUiCtrl.OnRewardBtnClick()
    if confirmFunc then 
        confirmFunc()
    end
end

function RewardGetUiCtrl.OnCloseClick()
    CtrlManager.ClosePanel("RewardGetUi")
end
----------------------------------------------------------------------
--外部调用
function RewardGetUiCtrl.ShowUIInfo(title, rewardStr, time, func, isReceive)
    if isReceive then
        receiveTime = Time.time
    end
    if not RewardGetUiPanel.hasCreated then
        this.CreateCallBack = function()
            this.ShowUIInfo(title, rewardStr, time, func, false)
        end
        return
    end

    leftTime = time
    this.titleLab.text = title
    confirmFunc = func
    needUpdate = true

    --处理奖励
    local RewardLogic = require("Logic/RewardLogic")
    RewardLogic.ParseRewardStr(this.rewards, rewardStr)
    this.rewardGrid.repositionNow = true
end

function RewardGetUiCtrl.GetRewardDeal()
    --奖励飘过去

    CtrlManager.ClosePanel("RewardGetUi")
end

----------------------------------------------------------------------

function RewardGetUiCtrl.OnDestroy()
    UpdateBeat:Remove(this.Update, this)
end

return this