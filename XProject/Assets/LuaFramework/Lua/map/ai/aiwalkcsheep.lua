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
clsAIWalkCSheep = AI_WALKTOCHAR.clsAIWalkToCharacter:Inherit({__ClassType = "<csheepai>"})
function clsAIWalkCSheep:__init__(charObj, tarCharId, radius, fradius, wTime, aTime, sx, sy)
	Super(clsAIWalkCSheep).__init__(self, charObj, tarCharId, radius, fradius, wTime, aTime, sx, sy)
	
	self.tarCharId = tarCharId
	self.radius = radius			--跟踪半径，超过就不跟了
	self.fradius = fradius			--距离跟踪物体多少就停止跟踪
	
	self.wTime = wTime * 5
	self.aTime = aTime
	
	self.sx = sx
	self.sy = sy
end

function clsAIWalkCSheep:IsWalkCSheep()
	return true
end

function clsAIWalkCSheep:GetDX()
	return self.dx
end
function clsAIWalkCSheep:GetDY()
	return self.dy
end

function clsAIWalkCSheep:Run()
	--寻找下个目标点
	local ox, oy = self.charObj:GetX(), self.charObj:GetY()
	
	for i = 1, 16 do	--循环后还没移动代表无法移动					
		local randDir = nil
		if i <= 8 then
			randDir = mrandom(8)
		else
			randDir = i - 8
		end
		local nx = ox + c_aPoint[randDir][1]
		local ny = oy + c_aPoint[randDir][2]
		if mabs(nx - self.sx) <= SHEEP_RANGE and mabs(ny - self.sy) <= SHEEP_RANGE then		--在范围内
			if self.charObj:CanMoveToOneGrid(ox, oy, nx, ny) then
				self.dx = nx
				self.dy = ny
				break
			end
		end
	end
	
	return Super(clsAIWalkCSheep).Run(self)
end

