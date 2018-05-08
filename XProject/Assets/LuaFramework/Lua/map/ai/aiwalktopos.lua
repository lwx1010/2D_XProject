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
clsAIWalkToPos = AI_WALKTOCHAR.clsAIWalkToCharacter:Inherit()
function clsAIWalkToPos:__init__(charObj, tarCharId, radius, fradius, wTime, aTime, dx, dy, walkGrid)
	Super(clsAIWalkToPos).__init__(self, charObj, tarCharId, radius, fradius, wTime, aTime, dx, dy, walkGrid)
	
	self.tarCharId = tarCharId
	self.radius = radius			--跟踪半径，超过就不跟了
	self.fradius = fradius			--距离跟踪物体多少就停止跟踪
	
	self.wTime = wTime
	self.aTime = aTime
	
	self.dx = dx
	self.dy = dy
	
--	if self.charObj:IsNpc() then
--		FIGHT_EVENT.AddBuff(self.charObj, self.charObj, {id=BUFF_INVINCIBLE,time=100})
--	end
	
	self.lastBlock = nil
	self.walkGrid = walkGrid or 1
end
function clsAIWalkToPos:IsWalkToPos()
	return true
end

function clsAIWalkToPos:SetDX(dx)
	self.dx = dx
end

function clsAIWalkToPos:SetDY(dy)
	self.dy = dy
end

function clsAIWalkToPos:GetDX()
	return self.dx
end
function clsAIWalkToPos:GetDY()
	return self.dy
end

function clsAIWalkToPos:Run()
	local rt = Super(clsAIWalkToPos).Run(self)
	if rt == AI_EXCEPTION then
		--去除无敌buff
--		if self.charObj:IsNpc() then
--			FIGHT_EVENT.DelBuff(self.charObj, BUFF_INVINCIBLE)
--		end
	elseif self.lastBlock and not self.isBlock then		--判断位置是否一直徘徊在那	
		--去除无敌buff
		if self.charObj:IsNpc() then
			FIGHT_EVENT.DelBuff(self.charObj, BUFF_INVINCIBLE)
		end		
		return AI_EXCEPTION
	end
	self.lastBlock = self.isBlock
	return rt
end