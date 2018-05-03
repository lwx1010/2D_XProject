--[===[
Author: 柯明余
Time:   2017-03-27
Note:   获取途径通用窗体View层
]===]

local GetWayCtrl = {}
local this = GetWayCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil

local itemList = {}
local mDataList = {}

function GetWayCtrl.New()
    return this
end
--@data  table.insert( data, {name="", sceneId=1} )
function GetWayCtrl.SetData(dataList)
    mDataList = dataList
end

function GetWayCtrl.Awake(closeMainCamera)
    createPanel("Common/GetWay", this.OnCreate, closeMainCamera)
end

function GetWayCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    luaBehaviour:AddClick(GetWayPanel.closeBtn, this.OnCloseClick)
    luaBehaviour:AddClick(GetWayPanel.background, this.OnCloseClick)
    
    this.CreateItem()
end

function GetWayCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, GetWayPanel)
    for i=1,4 do
        local item = mDataList[i]
        if item ~= nil then
            itemList[i]:SetData(item.name, item.sceneId)
            itemList[i]:Refresh()
            itemList[i].gameObject:SetActive(true)
        else
            itemList[i].gameObject:SetActive(false)
        end
    end
    GetWayPanel.uiGrid.repositionNow = true
end

function GetWayCtrl.CreateItem()
    for i=1,4 do
        local go = newObject(GetWayPanel.itemGo)
        go.transform:SetParent(GetWayPanel.uiGrid.transform)
        go.transform.localPosition = Vector3.zero
        go.transform.localScale = Vector3.one
        go.name = "WayItem"..i
        local item = GetWayPanel.WayItem:New(go)
        itemList[i] = item
        luaBehaviour:AddClick(go, function()
            local UiId = require "Logic/UiId"
            panelMgr:CloseAllPopedPanels()
            UiId.OpenUi(item.sceneId)
        end)
    end
    
end

function GetWayCtrl.OnCloseClick()
    CtrlManager.ClosePanel("GetWay")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function GetWayCtrl.OnDestroy()

end

return this