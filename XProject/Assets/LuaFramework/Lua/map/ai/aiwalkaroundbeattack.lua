local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local tinsert = table.insert
local mceil = math.ceil
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

local TmpWalkAroundBeAttackChar = {
	__ClassType = "<walkaba>",
}

------------------【周围逛】,被攻击则【跟踪并且击打】AI类-----------------------------
clsAIWalkAroundBeAttack = AI_BASE.clsAIBase:Inherit(TmpWalkAroundBeAttackChar)
function clsAIWalkAroundBeAttack:__init__(charObj, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIWalkAroundBeAttack).__init__(self, charObj)
	
	if fradius > radius then
		_RUNTIME_ERROR("fradius is > radius:", fradius, radius)
		fradius = radius
	end
	local wTime = wTime or AI_WALK_TIME
	local aTime = aTime or AI_ATTACK_TIME
	
	local walkAroundAI = AI_WALKAROUND.clsAIWalkAround:New(charObj, wTime, aTime, walkGrid)
	local walkToCharacterAndAttack = AI_WALKTOCHARANDATTACK.clsAIWalkToCharacterAndAttack:New(charObj, nil, radius, fradius, wTime, aTime, walkGrid)
	self.aAI[1] = walkAroundAI
	self.aAI[2] = walkToCharacterAndAttack
	
	self.aNextNo[1] = 2
	self.aNextNo[2] = 1
	
	self.aExceptionNo[2] = 1
	
	self.radius = radius
	self.fradius = fradius
	self.wTime = wTime
	self.aTime = aTime
	self.walkGrid = walkGrid or 1
end

function clsAIWalkAroundBeAttack:GetTime()
	return self.retTime or Super(clsAIWalkAroundBeAttack).GetTime(self)
end

function clsAIWalkAroundBeAttack:Run()
	self.retTime = nil
	--如果是有提示技能的
	local sCharObj = self.charObj
	local skillId, tipsTime = sCharObj:IsNowTipsSkillId()
	if skillId then
		local allSkill = sCharObj:GetAllSkill()
		local skillData = allSkill[skillId]
		if not skillData then 
			return AI_CONTINUE
		end
		FIGHT.UseSkillAct(sCharObj, nil, skillId, skillData.x, skillData.y)
		skillData.x = nil
		skillData.y = nil
		local nTimeNo = GetNowTimeNo()
		local timeNoCnt = skillData.CD
		skillData.CDEndTimeNo = nTimeNo + timeNoCnt
		
		self.retTime = skillData.SkillTime
		
		sCharObj:ClearNowSkillId()
		FIGHT.DelSkillTips(sCharObj, skillId)
		return AI_CONTINUE
	end
	
	if self.urgentAI then
		local ret = Super(clsAIWalkAroundBeAttack).Run(self)
		if self.urgentAI then
			return ret
		end
	end
	
	if self.npcExtSkillAI then
		local ret = Super(clsAIWalkAroundBeAttack).Run(self)
		if self.npcExtSkillAI then
			return ret
		end
	end	
	
	--如果超出范围,返回出生点,清除tar id.
	local aiRange = sCharObj:GetAIRange()
	local nx, ny = sCharObj:GetX(), sCharObj:GetY()
	if nx < aiRange.minx or nx > aiRange.maxx or ny < aiRange.miny or ny > aiRange.maxy then
		self.aAI[2]:SetTarCharId(nil)
		self.urgentAI = AI_WALKTOPOS.clsAIWalkToPos:New(self.charObj, nil, self.radius, 1, self.wTime, self.aTime, aiRange.x, aiRange.y, self.walkGrid)
		
		return AI_CONTINUE
	end
	
	return Super(clsAIWalkAroundBeAttack).Run(self)
end

local AllNpcData = NPC_BATTLE_DATA.GetAllNpcData()
--事件的改变，例如被攻击
function clsAIWalkAroundBeAttack:OnEvent(eventTbl)
	local resetAiTime = false
	if self.activeAINo == 1 and eventTbl.eventType == EVENT_BEATTACK and not self.urgentAI then				--如果是在urgentAi的状态下则return
		if eventTbl.eventAttackCharId then
			self.aAI[2]:SetTarCharId(eventTbl.eventAttackCharId)
			
			local x, y = eventTbl.attX, eventTbl.attY
			assert(x and y, "not clsAIWalkAroundBeAttack:OnEvent attX, attY")
			local sx, sy = mabs(x - self.charObj:GetX()), mabs(y - self.charObj:GetY())
			local radius = sx > sy and sx or sy
			self:SetRadius(radius)		
			
			if IsClient() then
				if eventTbl.eventType == EVENT_BEATTACK then
					local oneData = AllNpcData[self.charObj:GetCharNo()]
					if oneData then
						self:SetRadius(oneData.AITrackRange)
					end
				end
			end	
				
			resetAiTime = true
		else
			return AI_CONTINUE
		end
	end
	Super(clsAIWalkAroundBeAttack).OnEvent(self, eventTbl)
	if resetAiTime and self.activeAINo == 2 then
		--如果是被攻击导致追踪,需要把ai时间设置下
		DelCharObjToAITbl(self.charObj)
		AddCharObjToAITbl(self.charObj, 10)
	end
end

function clsAIWalkAroundBeAttack:SetRadius(radius)
	local sCharObj = self.charObj
	local aiRange = sCharObj:GetAIRange()
	if radius > aiRange.range then
		radius = aiRange.range
	end
	Super(clsAIWalkAroundBeAttack).SetRadius(self, radius)
end