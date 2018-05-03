ItemInfo = {}

local itemXls = require("xlsdata/Item/ItemXls")
local ItemConst= require('Model/ItemConst')

function ItemInfo:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function ItemInfo:OnCreate(item)
	if not item then return end
	self.Id = item.Id
	self.ItemNo = item.ItemNo
	self.EquipPos = item.EquipPos
	self.Score = item.Score
	self.Grid = item.Grid
	self.Amount = item.Amount
	self.IsBind = item.IsBind
	self.StrengGrade = item.StrengGrade
	self.ExpireTime = item.ExpireTime
	self.TreasurePos = item.TreasurePos
	local xls = itemXls[item.ItemNo]
	if not xls then
		logError("no item exist in itemXls: "..item.ItemNo)
		return
	end
	self.Rare = xls.Rare
	self.Kind = xls.Kind
	self.ItemName = xls.Name
	self.SubKind = xls.SubKind
	if xls.Kind == ItemConst.ITEM_TYPE_EQUIP then
		self.Step = xls.Step
		self.ShowGrade = xls.ShowGrade
		self.FitEquip = xls.FitEquip
		self.Score = xls.BaseScore
	end
end

function ItemInfo:UpdateItemInfo(baseInfo)
	if not self[baseInfo.key] then
		log(baseInfo.key.." is not initilized")
	end
	if baseInfo.type == "string" then
		self[baseInfo.key] = baseInfo.value_str
	elseif baseInfo.type == "number" then
		self[baseInfo.key] = baseInfo.value_int
	elseif baseInfo.type == "table" then
		self[baseInfo.key] = baseInfo.value_array
	end
end

function ItemInfo.RemoveSelf()
	--log("============ Remove item ============")
end