--[===[
Author: 柯明余
Time:   2017-03-17
Note:   战斗结束界面Ctrl层
]===]
local Hero = require "Logic/Hero"
local RewardLogic = require "Logic/RewardLogic"
require("Model/RewardItemCtrl")
local FightingEndingCtrl = {}
local this = FightingEndingCtrl

local gameObject = nil
local transform = nil 
local luaBehaviour = nil

local awardItemList = {}
local mState = 1
local mTimer = 10
local startTime = nil

local charFunc = nil 
local strengthFunc = nil
local weaponFunc = nil
local continueFunc = nil
local exitFunc = nil 
local mData = {}
local mName = ""
local mTips = ""

function FightingEndingCtrl.New()
    return this
end

function FightingEndingCtrl.SetFailureData(charCallback, strengthCallback, weaponCallback, exitCallback)
    mState = 1
    charFunc = charCallback
    strengthFunc = charCallback
    weaponFunc = weaponCallback
    exitFunc = exitCallback
end

--@data数据格式table.insert(data, {itemId=0, rare=1, count=0}) 
function FightingEndingCtrl.SetVictoryData(name, data, tips, continueCallback, exitCallback)
    mState = 2
    mName = name
    mData = data
    mTips = tips
    continueFunc = continueCallback
    exitFunc = exitCallback
end

function FightingEndingCtrl.Awake(closeMainCamera)
    createPanel("Common/FightingEnding", this.OnCreate, closeMainCamera)
end

function FightingEndingCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")

    this.AddClickEvent()
    this.AddEvent()
    
    for i = 1, 11 do
        local rewardItem = RewardItemCtrl:New()
        rewardItem:OnCreate(FightingEndingPanel.uiTable.transform, string.format("RewardItem_%s", i), luaBehaviour, 3)
        table.insert(awardItemList, rewardItem)
    end

    UpdateBeat:Add(this.Update, this)
end

function FightingEndingCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, FightingEndingPanel)
    mTimer = 10
    startTime = Time.time
    this.SetPanelStyle(mState)

    --音效
    soundMgr:StopBGM()
    soundMgr:PlayUISound(mState == 2 and "mission-complete" or "mission-failed")
end

function FightingEndingCtrl.Update()
    if not startTime then
        return
    end
    local deltaTime = math.floor(mTimer + startTime - Time.time)
    if deltaTime >= 0 then
        if mState == 1 then
            FightingEndingPanel.failureTimeLbl.text = string.format( LANGUAGE_TIP.CommonKey0001, deltaTime ) 
        elseif mState == 2 then
            FightingEndingPanel.victoryTimeLbl.text = mTips..string.format( LANGUAGE_TIP.CommonKey0002, deltaTime ) 
        end
    end
    if deltaTime == 0 then
        startTime = nil
        if mState == 1 then
            if exitFunc ~= nil then
                exitFunc()
            end
        elseif mState == 2 then
            if continueFunc ~= nil then
                continueFunc()
            end
        end
    end
end

--设置面板样式
function FightingEndingCtrl.SetPanelStyle(state)
    if state == 1 then
        FightingEndingPanel.failurePanelGo:SetActive(true)
        FightingEndingPanel.victoryPanelGo:SetActive(false)
    elseif state == 2 then
        FightingEndingPanel.nameLbl.text = mName--string.format( "会员%s级·%s", Hero.Vip, Hero.Name )
        FightingEndingPanel.failurePanelGo:SetActive(false)
        FightingEndingPanel.victoryPanelGo:SetActive(true)
        this.RefreshAwardItem()
    end
end

function FightingEndingCtrl.RefreshAwardItem()
    local RewardLogic = require("Logic/RewardLogic")
	RewardLogic.DealRealRewards(awardItemList, mData)
    FightingEndingPanel.uiTable.repositionNow = true
end

function FightingEndingCtrl.OnAwardItemClick(item)
    local ItemLogic = require "Logic/ItemLogic"
    ItemLogic.LoadBagMaterialTipUiPanel(item.itemId, false)
end

--角色升级按钮事件
function FightingEndingCtrl.OnCharBtnClick()
    if charFunc ~= nil then
        charFunc()
    end
end

--装备锻造按钮事件
function FightingEndingCtrl.OnStrengthBtnClick()
    if strengthFunc ~= nil then
        strengthFunc()
    end
end

--神器注灵按钮事件
function FightingEndingCtrl.OnWeaponClick()
    if weaponFunc ~= nil then
        weaponFunc()
    end
end

--继续按钮事件
function FightingEndingCtrl.OnContinueClick()
    if continueFunc ~= nil then
        continueFunc()
    end
end

--退出按钮事件
function FightingEndingCtrl.OnExitClick()
    if exitFunc ~= nil then
        exitFunc()
    end
end

function FightingEndingCtrl.OnClose()
    startTime = nil
    FightingEndingPanel.failurePanelGo:SetActive(false)
    FightingEndingPanel.victoryPanelGo:SetActive(false)
    FightingEndingPanel.delayshowGo:SetActive(false)
end

function FightingEndingCtrl.OnCloseWindowEvent()
    CtrlManager.ClosePanel("FightingEnding")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function FightingEndingCtrl.AddClickEvent()
    Event.AddListener(EventName.CLOSE_WINDOW_EVENT, this.OnCloseWindowEvent)
end

function FightingEndingCtrl.AddEvent()
    luaBehaviour:AddClick(FightingEndingPanel.charBtn, this.OnCharBtnClick)
	luaBehaviour:AddClick(FightingEndingPanel.strengthBtn, this.OnStrengthBtnClick)
	luaBehaviour:AddClick(FightingEndingPanel.weaponBtn, this.OnWeaponClick)
    luaBehaviour:AddClick(FightingEndingPanel.continueBtn, this.OnContinueClick)
    luaBehaviour:AddClick(FightingEndingPanel.exitBtn, this.OnExitClick)   
end

function FightingEndingCtrl.RemoveEvent()
    Event.RemoveListener(EventName.CLOSE_WINDOW_EVENT, this.OnCloseWindowEvent)
end

function FightingEndingCtrl.OnDestroy()
    this.RemoveEvent()
    charFunc = nil 
    strengthFunc = nil
    weaponFunc = nil
    continueFunc = nil
    exitFunc = nil 
    startTime = nil
    awardItemList = {}
    UpdateBeat:Remove(this.Update, this)
end

return this