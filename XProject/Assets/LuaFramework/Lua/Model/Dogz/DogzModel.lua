-- @Author: Chenguili
-- @Date:   2018-05-15 16:04:33
-- @Last Modified time: 2018-05-07 16:04:38


local DogzModel = class("DogzModel")

function DogzModel:ctor()
   self:Init()
end

function DogzModel:Init()
	self.dogzDate = {}
end

function DogzModel:SetDogzData(arge)
	for _,v in pairs(arge) do
		self.dogzDate[v.id] = v
	end
end

function DogzModel:GetDogzData()
	return self.dogzDate
end

function DogzModel:GetProperty(id,star,lv,passage)
	-- body
end

local data = {
	id = 10001, 
	lv = {1,1,1}, --{星,阶，段}
	exp = 0, --经验
	state = 1, --状态：1装备 2未装备
	pskill = {1,1,1,0}, --被动技能等级{第一个技能等级，第二个技能等级，。。。}

	--
	fight = 100, --战力
	askill = 1, --主动技能等级
}

DogzModel.inst = DogzModel.new()

return DogzModel
