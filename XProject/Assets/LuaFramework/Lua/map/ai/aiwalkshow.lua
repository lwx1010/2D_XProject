local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local tinsert = table.insert
local c_nBlockCnt 		= c_nBlockCnt
local c_nBlockMaxCnt 	= c_nBlockMaxCnt
local c_nHaltCnt 		= c_nHaltCnt				--追随者在被追随者旁边的停留步数c_nHaltCnt后移动一下
local c_aPoint = MOVE_DIR
--AI,EVENT状态--
local AI_CONTINUE 		= AI_CONTINUE
local AI_NEXT 			= AI_NEXT
local AI_EXCEPTION 		= AI_EXCEPTION
local AI_CANCELTARGET	= AI_CANCELTARGET
local AI_NULL 			= AI_NULL

local EVENT_BEATTACK	= EVENT_BEATTACK					--被攻击
local EVENT_TOATTACK	= EVENT_TOATTACK					--指使追随者(例如宠物)攻击

local NORMAL_NPC_RADIUS = NORMAL_NPC_RADIUS

------------------去某位置(跟WalkToCharacter不一样)------------------------
clsAIWalkShow = AI_WALKTOCHAR.clsAIWalkToCharacter:Inherit()
function clsAIWalkShow:__init__(charObj, tarCharId, radius, fradius, wTime, walkGrid, posList)
	Super(clsAIWalkShow).__init__(self, charObj, tarCharId, radius, fradius, wTime, 1, walkGrid)
	
	self.tarCharId = tarCharId
	self.radius = 100000
	
	assert(#posList >= 1)
	self.wTime = wTime
	self.posList = posList
	self.posCnt = 1
	
	self:SetDX(posList[1][1])
	self:SetDY(posList[1][2])
	
	self.walkGrid = walkGrid or 1
end
function clsAIWalkShow:IsWalkToPos()
	return true
end

function clsAIWalkShow:SetDX(dx)
	self.dx = dx
end
function clsAIWalkShow:SetDY(dy)
	self.dy = dy
end

function clsAIWalkShow:GetDX()
	return self.dx
end
function clsAIWalkShow:GetDY()
	return self.dy
end

function clsAIWalkShow:Run(isDouble)
	if not self.posList[self.posCnt] then return AI_EXCEPTION end
	local rt = Super(clsAIWalkShow).Run(self)
	if rt == AI_EXCEPTION then
		local nPos = self.posList[self.posCnt]
		if self.charObj:GetX() == nPos[1] and self.charObj:GetY() == nPos[2] then
			self.posCnt = self.posCnt + 1
			if not self.posList[self.posCnt] then
				lretmap.other(self.charObj:GetId(), MAP_ID, self.charObj:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_WALKSHOW,
				}))
				return
			else
				self:SetDX(self.posList[self.posCnt][1])
				self:SetDY(self.posList[self.posCnt][2])
				if not isDouble then
					return self:Run(true)
				end
			end
		end
	end
	return rt
end