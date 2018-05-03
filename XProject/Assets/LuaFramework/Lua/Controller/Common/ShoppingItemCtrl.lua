    --[===[
Author: 柯明余
Time:   2017-03-07
Note:   购买物品详情弹窗Ctrl层
]===]
local ItemLogic = require("Logic/ItemLogic")
local ItemConst = require('Model/ItemConst')
local ItemXls = require("xlsdata/Item/ItemXls")
local ShoppingItemCtrl = {}
local this = ShoppingItemCtrl

local gameObject = nil
local transform = nil
local luaBehaviour = nil

local mItemNo = 0           --物品id
local mName = ""            --名称
local mRare = 1         --稀有度
local mPrice = 0            --单价
local mMaxCount = 0         --最大数量
local mMaxDescribe = ""       --购买最大数量的描述
local mBuyNum = 1           --购买数量
local mTotalPrice = 0       --总价
local mHaveMoney = 0        --拥有的价格
local mCurrencyType = 1     --货币类型
local mLeftFunc = nil       --右按钮方法回调
local mRightFunc = nil      --右按钮方法回调
local mNum = 0              --物品数量
-- local mPriceBossScore = 0
-- local mPriceYuanBao = 0
local mTotalPBS = 0
local mTotalPYB = 0
local mleftLab = ""

function ShoppingItemCtrl.New()
    return this
end

--[[
@rare 稀有
@priceList 物品单价（需要消耗多种货币时依次填入）
@currencyType 物品消耗货币（需要消耗多种货币时依次填入）
@maxDescribe 限购描述
@maxCount 可购买最大数量
@leftFunc 左按钮回调
@rightFunc 左按钮回调
@num 物品数量
@buyNum 当前购买数量
]]
function ShoppingItemCtrl.SetData(itemNo, name, rare, priceList, currencyType, maxDescribe, maxCount, leftFunc, rightFunc, num, buyNum,leftLab)
    if type(priceList) ~= "table" then
        priceList = {priceList}
    end

    mItemNo = itemNo
    mName = name
    mRare = rare

    local newToken = {}
    local newPriceList = {}
    if type(currencyType) == "table" then
        for k,v in ipairs(currencyType) do 
            if priceList[k] ~= 0 then
                table.insert(newToken,v)
                table.insert(newPriceList,priceList[k])
            end
        end
        if #newToken == 1 then
            newToken = newToken[1]
        end
    else
        newToken = currencyType
        newPriceList = priceList
    end
    mPrice = newPriceList
    -- mPriceBossScore = priceList.priceBossScore or 0 
    -- mPriceYuanBao = priceList.priceYuanBao or 0 
    mCurrencyType = newToken

    mMaxCount = maxCount
    mMaxDescribe = maxDescribe
    mLeftFunc = leftFunc
    mRightFunc = rightFunc
    mNum = num
    mBuyNum = buyNum and buyNum or 1
    mleftLab = leftLab
end

function ShoppingItemCtrl.Awake(closeWindow)
    createPanel("Shop/ShoppingItem", this.OnCreate, closeWindow)
end

function ShoppingItemCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    luaBehaviour = transform:GetComponent("LuaBehaviour")
    this.AddClickEvent()
    this.AddEvent()
end

function ShoppingItemCtrl.OnInit()
    LuaUIHelper.addUIDepth(gameObject, ShoppingItemPanel)
    this.InitPanel()
    if mBuyNum == 1 and mMaxDescribe ~= "" then --商品默认数量为玩家能买的最大值，不是数量限制的，显示1
        mBuyNum = mMaxCount == 0 and 1 or mMaxCount
    end
    this.RefreshData()
end

function ShoppingItemCtrl.InitPanel()
    ShoppingItemPanel.nameLbl.text = mName
    ItemLogic.GetItemSprite(mItemNo, ShoppingItemPanel.icon)
    ShoppingItemPanel.nicklab.text = ToolHelper.GetGodNickName(ItemXls[mItemNo].Step) or ""
    ShoppingItemPanel.nicklab.gameObject:SetActive(ItemXls[mItemNo].Kind==2)    
    ToolHelper.SetItemRare(mRare, ShoppingItemPanel.iconGo)
    ShoppingItemPanel.countLbl.text = mMaxDescribe

    if type(mCurrencyType) == "table" then
        ShoppingItemPanel.singlePrice1.gameObject:SetActive(false)
        ShoppingItemPanel.totalPrice1.gameObject:SetActive(false)
        ShoppingItemPanel.singlePrice2.gameObject:SetActive(true)
        ShoppingItemPanel.totalPrice2.gameObject:SetActive(true)

        ShoppingItemPanel.priceBSLbl.text = mPrice[1]
        ShoppingItemPanel.priceYBLbl.text = mPrice[2]
        ToolHelper.SetCurrencySprite(mCurrencyType[1], ShoppingItemPanel.singleIcon1)
        ToolHelper.SetCurrencySprite(mCurrencyType[2], ShoppingItemPanel.singleIcon2) 
        ToolHelper.SetCurrencySprite(mCurrencyType[1], ShoppingItemPanel.totalIcon1)
        ToolHelper.SetCurrencySprite(mCurrencyType[2], ShoppingItemPanel.totalIcon2) 

    else
        ShoppingItemPanel.singlePrice1.gameObject:SetActive(true)
        ShoppingItemPanel.totalPrice1.gameObject:SetActive(true)
        ShoppingItemPanel.singlePrice2.gameObject:SetActive(false)
        ShoppingItemPanel.totalPrice2.gameObject:SetActive(false)

        ShoppingItemPanel.priceLbl.text = mPrice[1]
        ToolHelper.SetCurrencySprite(mCurrencyType, ShoppingItemPanel.currencyIcon)
        ToolHelper.SetCurrencySprite(mCurrencyType, ShoppingItemPanel.currencyIcon2) 
        
    end

    ShoppingItemPanel.leftBtnLab.text = mleftLab

    ShoppingItemPanel.numlab.text = not mNum and "" or mNum
    ShoppingItemPanel.buyBtn:SetActive(mRightFunc == nil and true or false)
    ShoppingItemPanel.uiGrid.gameObject:SetActive(mRightFunc ~= nil and true or false)    
end

function ShoppingItemCtrl.GetHaveCost(ctype)
    if ctype == 1 then        
        return HERO.YuanBao
    elseif ctype == 2 then
        return HERO.BindYuanBao
    elseif ctype == 3 then
        return HERO.ShengWang
    elseif ctype == 4 then        
        return HERO.JJCHonor
    elseif ctype == 5 then
        return GongXun
    elseif ctype == 6 then
        return HERO.LiLian
    elseif ctype == 7 then
        return HERO.BossScore
    end
end

function ShoppingItemCtrl.RefreshData()
    ShoppingItemPanel.buyNumLbl.text = mBuyNum

    if type(mCurrencyType) == "table" then
        mTotalPBS = mPrice[1] * mBuyNum
        mTotalPYB = mPrice[2] * mBuyNum
        ShoppingItemPanel.totalPBSLbl.text = mTotalPBS
        ShoppingItemPanel.totalPYBLbl.text = mTotalPYB
        if mMaxCount == 0 then
            ShoppingItemPanel.totalPBSLbl.color = Color.New(166/255,1,118/255,1)
            ShoppingItemPanel.totalPYBLbl.color = Color.New(166/255,1,118/255,1)
            if ShoppingItemCtrl.GetHaveCost(mCurrencyType[1]) < mPrice[1] then
                ShoppingItemPanel.totalPBSLbl.color = Color.New(229/255,15/255,25/255,1)
            end
            if ShoppingItemCtrl.GetHaveCost(mCurrencyType[2]) < mPrice[2] then
                ShoppingItemPanel.totalPYBLbl.color = Color.New(229/255,15/255,25/255,1)
            end
        else
            ShoppingItemPanel.totalPBSLbl.color = Color.New(166/255,1,118/255,1)
            ShoppingItemPanel.totalPYBLbl.color = Color.New(166/255,1,118/255,1)
        end  
    else
        mTotalPrice = mPrice[1] * mBuyNum
        ShoppingItemPanel.totalPriceLbl.text = mTotalPrice
        if mMaxCount == 0 then
            ShoppingItemPanel.totalPriceLbl.color = Color.New(229/255,15/255,25/255,1)
        else
            ShoppingItemPanel.totalPriceLbl.color = Color.New(166/255,1,118/255,1)
        end  
    end
end

--减少
function ShoppingItemCtrl.OnDecreaseClick()
    if mBuyNum > 1 then
        mBuyNum = mBuyNum - 1
        this.RefreshData()
    end
end

--增加
function ShoppingItemCtrl.OnIncreaseClick()
    if mBuyNum < mMaxCount then
        mBuyNum = mBuyNum + 1
        this.RefreshData()
    end
end

--最大数量
function ShoppingItemCtrl.OnMaxClick()
    if mBuyNum == mMaxCount then
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.shopMaxCount)
    end
    mBuyNum = mMaxCount == 0 and 1 or mMaxCount
    this.RefreshData()
end

function ShoppingItemCtrl.OnBuyClick()
    if mLeftFunc ~= nil then
        mLeftFunc(mBuyNum)
    end
end

function ShoppingItemCtrl.OnLeftClick()
    if mLeftFunc ~= nil then
        mLeftFunc(mBuyNum)
    end
end

function ShoppingItemCtrl.OnRightClick()
    if mRightFunc ~= nil then
        mRightFunc(mBuyNum)
    end
end

function ShoppingItemCtrl.OnCloseClick()
    CtrlManager.ClosePanel("ShoppingItem")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function ShoppingItemCtrl.AddClickEvent()
    luaBehaviour:AddClick(ShoppingItemPanel.closeBtn, this.OnCloseClick)
    luaBehaviour:AddClick(ShoppingItemPanel.buyBtn, this.OnBuyClick)
    luaBehaviour:AddClick(ShoppingItemPanel.leftBtn, this.OnLeftClick)
    luaBehaviour:AddClick(ShoppingItemPanel.rightBtn, this.OnRightClick)
    luaBehaviour:AddClick(ShoppingItemPanel.decreaseBtn, this.OnDecreaseClick)
    luaBehaviour:AddClick(ShoppingItemPanel.increaseBtn, this.OnIncreaseClick)
    luaBehaviour:AddClick(ShoppingItemPanel.maxBtn, this.OnMaxClick)
end

function ShoppingItemCtrl.OnBuyCallback()
    CtrlManager.ClosePanel("ShoppingItem")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
end

function ShoppingItemCtrl.AddEvent()
     Event.AddListener(EventName.SHOP_ITEM_BUY_EVENT, this.OnBuyCallback)
end

function ShoppingItemCtrl.RemoveEvent()
    Event.RemoveListener(EventName.SHOP_ITEM_BUY_EVENT, this.OnBuyCallback)
end

function ShoppingItemCtrl.OnDestroy()
    mLeftFunc = nil
    this.RemoveEvent()
end

function ShoppingItemCtrl.BuySuccess()
    CtrlManager.ClosePanel("ShoppingItem")
    LuaUIHelper.removeUIDepth(gameObject)  --还原全局深度
    local xls = ItemXls[mItemNo]
    if not xls then return end
    CtrlManager.PopUpNotifyText(string.format(LANGUAGE_TIP.buySuccessTip, mBuyNum, ItemConst.RARE_COLORS[xls.Rare], xls.Name))
end

return this