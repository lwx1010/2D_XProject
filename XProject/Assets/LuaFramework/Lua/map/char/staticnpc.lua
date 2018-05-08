local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local tinsert = table.insert

clsStaticNpc = NPC.clsNpc:Inherit({__ClassType = "NPC"})

function clsStaticNpc:__init__(mlCharId, x, y, z, syncData, ociData, mapLayer)
	assert(mlCharId, "not mlCharId in clsStaticNpc")
	Super(clsStaticNpc).__init__(self, x, y, z, syncData, ociData, mapLayer)
--	lretmap.static_npcadd(self:GetId(), MAP_ID, self:GetMapLayer(), self:GetX(), self:GetY(), self:GetZ())	
	
	self:SetComp(0)			--…Ë÷√µΩ“˝«Ê
end

function clsStaticNpc:IsStaticNpc()
	return true
end

function clsStaticNpc:CanMove()
	return true
end