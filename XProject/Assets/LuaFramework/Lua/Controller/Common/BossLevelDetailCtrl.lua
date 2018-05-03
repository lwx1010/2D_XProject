--[===[
Author: 柯明余
Time:   2017-03-02
Note:   Boss关卡详情Ctrl层（通用弹窗）
]===]
local ItemLogic = require("Logic/ItemLogic")
local BossLevelDetailCtrl = {}
local this = BossLevelDetailCtrl

local gameObject
local transform
local luaBehaviour

--模版数据
local templateData = {name="", title1="", describe="", title2="", itemList1={}, title3="", itemList2={}}
local itemList ={{itemId="", count=0, rare=1}, {itemId="", count=0, rare=1}}        ---视需求而定

local myData = {}
local items1 = {}
local items2 = {}

function BossLevelDetailCtrl.New()
    return this
end

function BossLevelDetailCtrl.GetTemplateData()
    return {name="", title1="",describe="",title2="",itemList1={},title3="",itemList2={}}    
end

function BossLevelDetailCtrl.SetData(data)
    myData = data
end

function BossLevelDetailCtrl.Awake(closeMainCamera)
    createPanel("Common/BossLevelDetail", this.OnCreate, closeMainCamera)
end

function BossLevelDetailCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    luaBehaviour:AddClick(BossLevelDetailPanel.closeBtn, this.OnCloseClick)
    items1 = this.CreateItems(BossLevelDetailPanel.uiGrid1.transform)
    items2 = this.CreateItems(BossLevelDetailPanel.uiGrid2.transform)
end

function BossLevelDetailCtrl.CreateItems(parent)
    local list = {}
    for i=1,4 do
        local go = newObject(BossLevelDetailPanel.itemGo)
        go.transform:SetParent(parent)
        go.transform.localPosition = Vector3.zero
        go.transform.localScale = Vector3.one
        go.name = parent.name..i
        local item = AwardItem:New(go)
        list[i] = item
        luaBehaviour:AddClick(go, function()
            this.OnItemClick(item)
        end)
    end
    return list
end

function BossLevelDetailCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, BossLevelDetailPanel)
    this.Refresh()
end

function BossLevelDetailCtrl.Refresh()
    BossLevelDetailPanel.nameLbl.text = myData.name
    BossLevelDetailPanel.titleLbl1.text = myData.title1
    BossLevelDetailPanel.describeLbl1.text = myData.describe
    BossLevelDetailPanel.titleLbl2.text = myData.title2
    BossLevelDetailPanel.titleLbl3.text = myData.title3
    
    this.SetItemList(items1, myData.itemList1, BossLevelDetailPanel.uiGrid1)
    this.SetItemList(items2, myData.itemList2, BossLevelDetailPanel.uiGrid2)
end

function BossLevelDetailCtrl.SetItemList(items, dataList, grid)
    for i=1,4 do
        local item = dataList[i]
        if item ~= nil then
            items[i]:SetData(item.itemId, item.count)
            items[i]:Refresh()
            items[i].gameObject:SetActive(true)
        else
            items[i].gameObject:SetActive(false)
        end
    end
    grid.repositionNow = true
end

function BossLevelDetailCtrl.OnItemClick(item)
    ItemLogic.LoadBagMaterialTipUiPanel(item.itemId, false)
end

function BossLevelDetailCtrl.OnCloseClick(go)
    CtrlManager.ClosePanel("BossLevelDetail")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function BossLevelDetailCtrl.OnDestroy()

end

return this