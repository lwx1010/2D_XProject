Item = {}

require("Model/ItemInfo")
require("Model/ItemCtrl")

function Item:New(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function Item:GetItemCount()
	return self.Info.Amount
end

function Item:CreateItemInfo(item)
	if not self.Info then
		self.Info = ItemInfo:New()
		self.Info:OnCreate(item)
	end
end

function Item:UpdateItemInfo(baseInfo)
	if not self.Info then
		logError("update base info error: no item info exists")
		return
	end
	self.Info:UpdateItemInfo(baseInfo)
end

function Item:CreateItemCtrl(id)
	-- body
	if not this.Info then
		logError("Item has no info to create control")
		return
	end
	if this.Ctrl ~= nil then
		logError("Item already created control "..this.Info.Id)
		return
	end
	this.Ctrl = ItemCtrl.New(id)
end
