ChatItemCtrl = {}
local itemXls = require("xlsdata/Item/ItemXls")
function ChatItemCtrl:New(o)
	o = o or {}
	setmetatable(o, ChatItemCtrl)
	self.__index = ChatItemCtrl
	return o
end

function ChatItemCtrl:OnCreate(go)
	self.gameObject = go
	self.bg = go.transform:Find('bg'):GetComponent('UISprite')
	self.iconspr = go.transform:Find("iconspr"):GetComponent("UISprite")
	self.number = go.transform:Find('number'):GetComponent('UILabel')
	self.lock = go.transform:Find('lock'):GetComponent('UISprite')
	self.timelimiticon = go.transform:Find('timelimiticon'):GetComponent('UISprite')
	self.steplab = go.transform:Find('steplab'):GetComponent('UILabel')
	self.showgrade = go.transform:Find('showgrade'):GetComponent('UILabel')
	self.equipPos = go.transform:Find('equipPos'):GetComponent('UISprite')
	self.iconlab = go.transform:Find('iconstr'):GetComponent('UILabel')
	self.nicklab = go.transform:Find('nicklab'):GetComponent('UILabel')
	self.behaviour = go.transform:GetComponent('LuaComponent')
	self.selectedSpr = go.transform:Find("selectedspr")
	self.behaviour:AddClick(self.gameObject, function()
		if self.isJishou then
			JiaoyihangSellPanel.SetSelectedItem(self) 
		else
			CHATLOGIC.AddItemMsg(self.item) 
		end
	end)
end

function ChatItemCtrl:SetItemInfo(v, isJishou)
	self.isJishou = isJishou
	self.item = v
	if not v then
		self:InitItem()
		return
	end
	if itemXls[v.Info.ItemNo].Kind == 1 or itemXls[v.Info.ItemNo].Kind == 3  or itemXls[v.Info.ItemNo].Kind == 4 then
		self.steplab.gameObject:SetActive(false)
		self.showgrade.gameObject:SetActive(false)
		self.nicklab.text = ""
	elseif itemXls[v.Info.ItemNo].Kind == 2 and itemXls[v.Info.ItemNo].SubKind > 10 then  --装备类型
		self.steplab.gameObject:SetActive(true)
		self.showgrade.gameObject:SetActive(true)
		self.steplab.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and "" or  itemXls[v.Info.ItemNo].Step.."#"
		self.showgrade.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and "" or "+"..itemXls[v.Info.ItemNo].ShowGrade
		self.nicklab.text = (itemXls[v.Info.ItemNo].SubKind >= 51 and itemXls[v.Info.ItemNo].SubKind <= 58 ) and ToolHelper.GetGodNickName(itemXls[v.Info.ItemNo].Step) or  ""
	end
	if v.Info.Amount == 1 then
		self.number.text = nil
	else
	   	self.number.text = tostring(v.Info.Amount)
	end
	local ItemLogic = require('Logic/ItemLogic')
	ItemLogic.GetItemSprite(v.Info.ItemNo, self.iconspr)
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
function ChatItemCtrl:SetRare(num)
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
function ChatItemCtrl:InitItem()
	self.steplab.gameObject:SetActive(false)
	self.showgrade.gameObject:SetActive(false)
	self.timelimiticon.gameObject:SetActive(false)
	self.bg.spriteName = "ui_bag_emptybox"
	self.iconspr.spriteName = ""
	self.number.text = nil
	self.lock.transform.gameObject:SetActive(false)
	self.equipPos.spriteName = ""
	self.iconlab.text = ""
	self.nicklab.text = ""
end

function ChatItemCtrl:SetSelected(value)
	if self.selectedSpr then
		self.selectedSpr.gameObject:SetActive(value)
	end
end