--[===[
Author: 柯明余
Time:   2017-03-17
Note:   战斗准备提示窗口Ctrl层
]===]
local FightReadyTipsCtrl = {}
local this = FightReadyTipsCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil 
local mTime = 3
local startTime = 0
local mState = 0            --1、准备战斗；2、结束战斗

function FightReadyTipsCtrl.New()
    return this
end

function FightReadyTipsCtrl.SetData(time, state)
    mTime = time
    mState = state
end

function FightReadyTipsCtrl.Awake(closeMainCamera)
    createPanel("Common/FightReadyTips", this.OnCreate, closeMainCamera)
end

function FightReadyTipsCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    UpdateBeat:Add(this.Update, this)
end

function FightReadyTipsCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, FightReadyTipsPanel)
    startTime = Time.time
    if mState == 1 then
        FightReadyTipsPanel.contentSpr.spriteName = "ui_drfbtip_zdts_zi01"
    elseif mState == 2 then
        FightReadyTipsPanel.contentSpr.spriteName = "ui_wfdb_lkysz"
    end
    FightReadyTipsPanel.contentSpr:MakePixelPerfect()
end

function FightReadyTipsCtrl.Update()
    if gameObject == nil then
        return
    end
    local deltaTime = math.floor(mTime + startTime + 1 - Time.time)
    if deltaTime >= 0 then
        FightReadyTipsPanel.timeLbl.text = deltaTime
    elseif gameObject.activeSelf then
        gameObject:SetActive(false)
    end
end

function FightReadyTipsCtrl.OnDestroy()
    UpdateBeat:Remove(this.Update, this)
    gameObject = nil
    transform = nil
end

return this