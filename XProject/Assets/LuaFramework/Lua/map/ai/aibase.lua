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

local AI_WALK_TIME 		= AI_WALK_TIME
local AI_ATTACK_TIME 	= AI_ATTACK_TIME

local TmpBaseChar = {
	__ClassType = "<baseai>",
}

clsAIBase = clsObject:Inherit(TmpBaseChar)
function clsAIBase:__init__(charObj)
	assert(charObj, "ai error no char!")
	self.charObj = charObj			
	self.aAI = {}				
	self.aNextNo = {}			--下个ai编号
	self.aExceptionNo = {}		--下个执行ai编号

	self.activeAINo = 1
	self.urgentAI = nil			--紧急的AI
	self.npcExtSkillAI = nil	--npc特殊技能ai
end

function clsAIBase:GetActiveAINo()
	return self.activeAINo or 1
end

function clsAIBase:Destroy()
	if self.urgentAI then
		self.urgentAI:Destroy()
	end
	if self.npcExtSkillAI then
		self.npcExtSkillAI:Destroy()
	end
	for _, oneAIObj in pairs(self.aAI) do
		oneAIObj:Destroy()
	end
	self.charObj = nil
end

function clsAIBase:SetActiveAIInit()
	--nothing
end

function clsAIBase:GetActiveAI()
	if self.activeAINo >= 1 and self.activeAINo <= #self.aAI then
		return self.aAI[self.activeAINo]
	end
	return nil
end

function clsAIBase:SetActiveAI(n)
	if n >= 1 and n <= #self.aAI then
		self.activeAINo = n
		self:GetActiveAI():SetActiveAIInit()
		self:GetActiveAI():Run()					--到这个设置就代表上次的失败了，然后执行这个next的.不能返回新的状态,如果使用新的状态可能会导致循环
		return true
	end
	return false
end

function clsAIBase:OnReturn(rt)
	if rt == AI_CONTINUE then
		return AI_CONTINUE
	elseif rt == AI_NEXT then
		local nextAiNo = self.aNextNo[self.activeAINo]		--根据跳转表来处理next
		if nextAiNo and self:SetActiveAI(nextAiNo) then
			return AI_CONTINUE
		end
		return AI_NEXT
	elseif rt == AI_EXCEPTION then
		local nextAiNo = self.aExceptionNo[self.activeAINo]		--根据跳转表来处理exception
		if nextAiNo and self:SetActiveAIWithoutRun(nextAiNo) then
			return AI_CONTINUE
		end
		return AI_EXCEPTION	
	elseif rt == AI_CANCELTARGET then
		return AI_EXCEPTION
	end
	assert(false, "error ai OnReturn!")
	return AI_CONTINUE
end

function clsAIBase:Run()
	if self.urgentAI then
		local rt = self.urgentAI:Run()				--处理紧急AI
		if rt == AI_EXCEPTION then					--如果紧急动作返回的不是继续进行紧急动作
			self.urgentAI:Destroy()
			self.urgentAI = nil
		end
		return AI_CONTINUE
	end
	
	if self.npcExtSkillAI then
		local rt = self.npcExtSkillAI:Run()				--处理紧急AI
		if rt == AI_EXCEPTION then					--如果紧急动作返回的不是继续进行紧急动作
			self.npcExtSkillAI = nil
		end
		return AI_CONTINUE		
	end
	
	local activeAI = self:GetActiveAI()
	if not activeAI then 
		return AI_NEXT 
	end
	
	local rt = activeAI:Run()
	return self:OnReturn(rt)
end

function clsAIBase:GetNextTimeNo()
	return self.__timeno
end

function clsAIBase:SetNextTimeNo(timeNo)
	self.__timeno = timeNo
end

function clsAIBase:GetTime()
	if self.urgentAI then
		local t = self.urgentAI:GetTime() or AI_WALK_TIME
		return t
	end
	if self.npcExtSkillAI then
		local t = self.npcExtSkillAI:GetTime() or AI_WALK_TIME
		return t
	end
	
	local activeAI = self:GetActiveAI()
	if not activeAI then 
		return AI_WALK_TIME 
	end
	
	local t = activeAI:GetTime() or AI_WALK_TIME
	return t
end

function clsAIBase:SetActiveAIWithoutRun(n)
	if n >= 1 and n <= #self.aAI then
		self.activeAINo = n
		self:GetActiveAI():SetActiveAIInit()
		return true
	end
	return false	
end

function clsAIBase:OnEventReturn(rt)
	if rt == AI_CONTINUE then
		return AI_CONTINUE
	elseif rt == AI_NEXT then
		local nextAiNo = self.aNextNo[self.activeAINo]		--根据跳转表来处理next
		if nextAiNo and self:SetActiveAIWithoutRun(nextAiNo) then
			return AI_CONTINUE
		end
		return AI_NEXT
	elseif rt == AI_EXCEPTION then
		local nextAiNo = self.aExceptionNo[self.activeAINo]		--根据跳转表来处理exception
		if nextAiNo and self:SetActiveAIWithoutRun(nextAiNo) then
			return AI_CONTINUE
		end
		return AI_EXCEPTION	
	elseif rt == AI_CANCELTARGET then
		return AI_EXCEPTION
	end
	assert(false, "error ai OnEventReturn!")
	return AI_CONTINUE
end

--事件的改变，例如被攻击
function clsAIBase:OnEvent(eventTbl)
	if self.urgentAI then
		self.urgentAI:OnEvent(eventTbl)
		return AI_CONTINUE
	end
	if self.npcExtSkillAI then
		self.npcExtSkillAI:OnEvent(eventTbl)
		return AI_CONTINUE
	end
	if #self.aAI == 0 then
		return AI_CONTINUE
	end
	local activeAI = self:GetActiveAI()
	if activeAI then
		local rt = activeAI:OnEvent(eventTbl)
		return self:OnEventReturn(rt)
	else
		return AI_CONTINUE
	end
end

--修改npc的视野大小
function clsAIBase:SetRadius(radius)
	if self.radius and self.radius < radius then
		self.radius = radius
	end
	if self.urgentAI then
		self.urgentAI:SetRadius(radius)
	end
	if self.npcExtSkillAI then
		self.npcExtSkillAI:SetRadius(radius)
	end
	for _, oneAIObj in pairs(self.aAI) do
		oneAIObj:SetRadius(radius)
	end
end