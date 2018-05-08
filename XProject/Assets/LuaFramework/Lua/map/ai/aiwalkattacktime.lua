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

local GetNowTimeNo = GetNowTimeNo

local TmpWalkAttackTime = {
	__ClassType = "<walkaat>",
}

local WALK_ACTIVETIME = 50
local ATTACK_ACTIVETIME = 100

------------------【周围逛】,见到玩家则【跟踪并且击打】AI类(判断是否有阻塞的，有则搜周围最近的玩家打)---------------------------
clsAIWalkAttackTime = AI_BASE.clsAIBase:Inherit(TmpWalkAttackTime)
function clsAIWalkAttackTime:__init__(charObj, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIWalkAttackTime).__init__(self, charObj)
	
	if fradius > radius then
		_RUNTIME_ERROR("fradius is > radius:", fradius, radius)
		fradius = radius
	end
	local wTime = wTime or AI_WALK_TIME
	local aTime = aTime or AI_ATTACK_TIME
	
	self.walk_activetime = WALK_ACTIVETIME * (mrandom(100, 120) / 100)
	self.attack_activetime = ATTACK_ACTIVETIME * (mrandom(100, 120) / 100)
	
	local nx, ny = charObj:GetX(), charObj:GetY()
	local walkToPosAI = AI_WALKTOPOS.clsAIWalkToPos:New(charObj, nil, radius, 1, wTime, aTime, nx, ny, walkGrid)
	local walkToCharacterAndAttack = AI_WALKTOCHARANDATTACK.clsAIWalkToCharacterAndAttack:New(charObj, nil, radius, fradius, wTime, aTime, walkGrid)
	self.aAI[1] = walkToPosAI
	self.aAI[2] = walkToCharacterAndAttack
	
	self.radius = radius
	self.fradius = fradius
	self.wTime = wTime
	self.aTime = aTime
	self.walkGrid = walkGrid or 1
	
	self.activeStartTime = GetNowTimeNo()
end

function clsAIWalkAttackTime:GetTime()
	return self.retTime or Super(clsAIWalkAttackTime).GetTime(self)
end

function clsAIWalkAttackTime:Run()
	self.retTime = nil
	--如果是有提示技能的
	local sCharObj = self.charObj
	if sCharObj:IsNpc() then
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
			local ret = Super(clsAIWalkAttackTime).Run(self)
			if self.urgentAI then
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
	else
		if self.urgentAI then
			local ret = Super(clsAIWalkAttackTime).Run(self)
			if self.urgentAI then
				return ret
			end
		end		
	end

	if self.activeAINo == 1 then
		local activeStartTime = self.activeStartTime
		local nTimeNo = GetNowTimeNo()
		local activeTime = nTimeNo - activeStartTime
		if activeTime >= self.walk_activetime then
			self.activeStartTime = nTimeNo
			self.activeAINo = 2
			
			local tarObj = sCharObj:SearchOCompCharObj(self.radius)
			if tarObj then
				local tarId = tarObj:GetId()
				
				local x, y = tarObj:GetX(), tarObj:GetY()
				local sx, sy = mabs(x - sCharObj:GetX()), mabs(y - sCharObj:GetY())
				local radius = sx > sy and sx or sy
				self:SetRadius(radius)	
				
				self.aAI[2]:SetTarCharId(tarId)
				self:SetActiveAI(2)
				
				return AI_CONTINUE	
			end
		end
		
		local ret = Super(clsAIWalkAttackTime).Run(self)
		if ret ~= AI_CONTINUE then
			--随机获取下一个位置	
			local aiRange = sCharObj:GetAIRange()
			local nx, ny = sCharObj:GetX(), sCharObj:GetY()
			local rx, ry = aiRange.x, aiRange.y
			
			local tarX, tarY = nil, nil
			for i = 1, 10 do
				local tx = mrandom(aiRange.minx, aiRange.maxx)
				local ty = mrandom(aiRange.miny, aiRange.maxy)
				local nz = lmapdata.getz(MAP_NO, tx, ty)
				if nz then
					tarX, tarY = tx, ty
					break
				end
			end
			if not tarX then
				tarX, tarY = rx, ry
			end
			self.aAI[1]:SetDX(tarX)
			self.aAI[1]:SetDY(tarY)
		end
		
		return AI_CONTINUE
		
	elseif self.activeAINo == 2 then
		local activeStartTime = self.activeStartTime
		local nTimeNo = GetNowTimeNo()
		local activeTime = nTimeNo - activeStartTime
		if activeTime >= self.attack_activetime then
			self.activeStartTime = nTimeNo
			self.activeAINo = 1
			
			--随机获取下一个位置	
			local aiRange = sCharObj:GetAIRange()
			local nx, ny = sCharObj:GetX(), sCharObj:GetY()
			local rx, ry = aiRange.x, aiRange.y
			
			local tarX, tarY = nil, nil
			for i = 1, 10 do
				local tx = mrandom(aiRange.minx, aiRange.maxx)
				local ty = mrandom(aiRange.miny, aiRange.maxy)
				local nz = lmapdata.getz(MAP_NO, tx, ty)
				if nz then
					tarX, tarY = tx, ty
					break
				end
			end
			if not tarX then
				tarX, tarY = rx, ry
			end
			self.aAI[1]:SetDX(tarX)
			self.aAI[1]:SetDY(tarY)
			
			return AI_CONTINUE	
		end
		
		
		local ret = Super(clsAIWalkAttackTime).Run(self)
		
		if ret ~= AI_CONTINUE then
			--搜索下一个人物
			local tarObj = sCharObj:SearchOCompCharObj(self.radius)
			if tarObj then
				local tarId = tarObj:GetId()
				
				local x, y = tarObj:GetX(), tarObj:GetY()
				local sx, sy = mabs(x - sCharObj:GetX()), mabs(y - sCharObj:GetY())
				local radius = sx > sy and sx or sy
				self:SetRadius(radius)	
				
				self.aAI[2]:SetTarCharId(tarId)
				self:SetActiveAI(2)
			end
		end
		
		return AI_CONTINUE		
	end

end

--事件的改变，例如被攻击
function clsAIWalkAttackTime:OnEvent(eventTbl)
end

function clsAIWalkAttackTime:SetRadius(radius)
	local sCharObj = self.charObj
	local aiRange = sCharObj:GetAIRange()
	if radius > aiRange.range then
		radius = aiRange.range
	end
	Super(clsAIWalkAttackTime).SetRadius(self, radius)
end