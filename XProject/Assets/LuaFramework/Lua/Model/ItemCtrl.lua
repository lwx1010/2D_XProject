
ItemCtrl = {}

local itemparent = nil
local itemXls = require("xlsdata/Item/ItemXls")

require("Model/ItemInfo")

function ItemCtrl:New(id)
	local o = {}
	setmetatable(o, self)
	self.__index = self
	o:Awake(id)
	return o
end

--生成object
function ItemCtrl:Awake(id)
	self.GetItenParent()
	self.gameObject = Riverlake.ObjectPool.instance:PushToPool("Prefab/Gui/bag/BagItem", 50, itemparent, 0,0,0)	  --格子对象
	self.gameObject.name = "item" .. id
	self.transform = self.gameObject.transform
	self.ItemCell = self.transform:GetComponent("ItemCell")
	self.ItemCell.mParentTrans = BagUiPanel.EndlessScroller.transform
	self.ItemCell.itemId = id
	self:InitPanel()
end

function ItemCtrl:InitPanel()
	self.bg = self.transform:Find('bg'):GetComponent('UISprite')
	self.iconspr = self.transform:Find("iconspr"):GetComponent("UISprite")
	-- self.icom = self.transform:Find('icom'):GetComponent('UITexture')
	self.number = self.transform:Find('number'):GetComponent('UILabel')
	self.lock = self.transform:Find('lock'):GetComponent('UISprite')
	self.timelimiticon = self.transform:Find('timelimiticon'):GetComponent('UISprite')
	self.steplab = self.transform:Find('steplab'):GetComponent('UILabel')
	self.showgrade = self.transform:Find('showgrade'):GetComponent('UILabel')
	self.equipPos = self.transform:Find('equipPos'):GetComponent('UISprite')
	self.iconlab = self.transform:Find('iconstr'):GetComponent('UILabel')
	self.nicklab = self.transform:Find('nicklab'):GetComponent('UILabel')
	self.addspr = self.transform:Find("addspr"):GetComponent('UISprite')
end 

--获取当前生成在那个父物体里面
function ItemCtrl.GetItenParent()
	if CtrlManager.GetCtrl("BagUiCtrl").alltabIsShow then 
	     itemparent = BagUiPanel.allItemtab.transform
	elseif CtrlManager.GetCtrl("BagUiCtrl").materialtabIsShow then
	     itemparent =BagUiPanel.materilItemtab.transform
    elseif CtrlManager.GetCtrl("BagUiCtrl").equiptabIsShow then
    	itemparent = BagUiPanel.equipItemtab.transform
    else
    	itemparent = BagUiPanel.imgItemtab.transform
    end
    return itemparent
end

--设置全部标签信息     信息表 
function ItemCtrl:SetAllItmeInfo(itemList,indexs)
 	self:InitItem()
 	for k,v in pairs(itemList) do
		if k == indexs  then  --是否在这个格子如果是的话就设置信息，不是的话就是空格子 and v.Info.EquipPos == 0
			self:InitInfoItem(v)
           	return
		else
			self:InitItem()
		end
	end 
end  

--设施材料和图鉴、装备
function ItemCtrl:SetItmeInfo(itemList,indexs)
	--log(TableToString(itemList))
 	self:InitItem()
 	for k,v in pairs(itemList) do
		if k == indexs  then  --是否在这个格子如果是的话就设置信息，不是的话就是空格子
			self:InitInfoItem(v)
           	return
		else
			self:InitItem()
		end
	end 
end  

function ItemCtrl:InitInfoItem(v)
	self.nicklab.text = ""
	if itemXls[v.Info.ItemNo].Kind == 1 or itemXls[v.Info.ItemNo].Kind == 3  or itemXls[v.Info.ItemNo].Kind == 4 then
		self.steplab.gameObject:SetActive(false)
		self.showgrade.gameObject:SetActive(false)
	elseif itemXls[v.Info.ItemNo].Kind == 2 and itemXls[v.Info.ItemNo].SubKind > 10 then  --装备类型
		self.steplab.gameObject:SetActive(true)
		self.showgrade.gameObject:SetActive(true)
		self.steplab.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and "" or  itemXls[v.Info.ItemNo].Step.."#"
		self.showgrade.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and "" or "+"..itemXls[v.Info.ItemNo].ShowGrade
		self.nicklab.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and ToolHelper.GetGodNickName(itemXls[v.Info.ItemNo].Step) or  ""
	end
	if self.number == nil then
		self:InitPanel()
	end
	if v.Info.Amount == 1 then
		self.number.text = nil
	else
	   	self.number.text = tostring(v.Info.Amount)
	end
	local ItemLogic = require('Logic/ItemLogic')
	ItemLogic.GetItemSprite(v.Info.ItemNo, self.iconspr)
    --self.icom.mainTexture = ItemLogic.GetItemTexture(v.Info.ItemNo)
   	ItemLogic.SetAddspr(v.Info.ItemNo , self.addspr)
    self:SetRare(itemXls[v.Info.ItemNo].Rare)
    if v.Info.IsBind == 1 then 
        self.lock.transform.gameObject:SetActive(true)
    else
        self.lock.transform.gameObject:SetActive(false)
    end

    if  v.Info.ExpireTime ~= nil and v.Info.ExpireTime ~= 0 then 
        if v.Info.ExpireTime == -1 then 
        	self.timelimiticon.gameObject:SetActive(true)
           	self.timelimiticon.spriteName = "ui_overtime_tag"
        else
        	self.timelimiticon.gameObject:SetActive(true)
           	self.timelimiticon.spriteName = "ui_timelimit_tag"
        end
    else
        self.timelimiticon.gameObject:SetActive(false)
    end

    if v.Info.EquipPos > 0 then 
    	self.equipPos.spriteName = "ui_icon_beibao_yizhuangbei"
    else
    	self.equipPos.spriteName = ""
    end

    self.iconlab.text = itemXls[v.Info.ItemNo].IconStr or ""
end

--设置稀有度的背景图
function ItemCtrl:SetRare(num)
	if num == 1 then 
		self.bg.spriteName = "ui_box_green"
	elseif num == 2 then
		self.bg.spriteName = "ui_box_blue"
    elseif num == 3 then
    	self.bg.spriteName = "ui_box_purple"
    elseif num == 4 then
    	self.bg.spriteName = "ui_box_orange"
    else
    	self.bg.spriteName = "ui_box_golden"
    end
    return self.bg.spriteName
end

--默认初始值
function ItemCtrl:InitItem()
	self.nicklab.text = ""
	self.steplab.gameObject:SetActive(false)
	self.showgrade.gameObject:SetActive(false)
	self.timelimiticon.gameObject:SetActive(false)
	self.bg.spriteName = "ui_bag_emptybox"
	--self.icom.mainTexture = nil
	self.iconspr.spriteName = ""
	self.number.text = nil
	self.lock.transform.gameObject:SetActive(false)
	self.equipPos.spriteName = ""
	self.iconlab.text = ""
	self.addspr.gameObject:SetActive(false)
end


function ItemCtrl.OnDestroy()
	UpdateBeat:Remove(this.UpdateTableItems, this)
end

function ItemCtrl:RemoveSelf()
	log("============ Remove item ============")
end

