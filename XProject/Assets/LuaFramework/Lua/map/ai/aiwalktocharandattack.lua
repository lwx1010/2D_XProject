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

------------------【跟踪】并且【击打】某物AI类-----------------------------
clsAIWalkToCharacterAndAttack = AI_BASE.clsAIBase:Inherit()
function clsAIWalkToCharacterAndAttack:__init__(charObj, tarCharId, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIWalkToCharacterAndAttack).__init__(self, charObj)
	local walkToCharAI = AI_WALKTOCHAR.clsAIWalkToCharacter:New(charObj, tarCharId, radius, fradius, wTime, aTime, walkGrid)
	local attackAI = AI_ATTACK.clsAIAttack:New(charObj, tarCharId, fradius, aTime)

	self.tarCharId = tarCharId

	self.aAI[1] = walkToCharAI
	self.aAI[2] = attackAI
	
	self.aNextNo[1] = 2
	self.aNextNo[2] = 1
	
	self.wTime = wTime
	self.aTime = aTime
	self.walkGrid = walkGrid or 1
end

function clsAIWalkToCharacterAndAttack:GetTarCharId()
	return self.tarCharId
end

function clsAIWalkToCharacterAndAttack:SetTarCharId(tarCharId)
	self.tarCharId = tarCharId
	for _, oneAIObj in pairs(self.aAI) do
		oneAIObj:SetTarCharId(tarCharId)
	end
end

function clsAIWalkToCharacterAndAttack:SetActiveAIInit()
	self.aAI[1]:SetActiveAIInit()		--其他包含aAI的基本也要这样设置，因为第二次调用的时候无法SetActiveAIInit
	self.activeAINo = 1
end

--function clsAIWalkToCharacterAndAttack:OnEvent(eventTbl)
--	if eventTbl.eventType == EVENT_BEATTACK then		--被攻击了转到下个目标去
--		--修改追踪的人的id
--		if eventTbl.eventAttackCharId ~= self.tarCharId then
--			local randomNum = mrandom(8)
--			if randomNum <= 1 then		--1/8几率改变追踪人物
--				self:SetTarCharId(eventTbl.eventAttackCharId)
--			end
--		end
--	end
--	return AI_CONTINUE
--end