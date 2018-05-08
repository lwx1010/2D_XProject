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

local TmpShouWeiPlayerFirstChar = {
	__ClassType = "<swplayerf>",
}

clsAIShouWeiPlayerFirst = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:Inherit(TmpShouWeiPlayerFirstChar)
function clsAIShouWeiPlayerFirst:__init__(charObj, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIShouWeiPlayerFirst).__init__(self, charObj, radius, fradius, wTime, aTime, walkGrid)
end

function clsAIShouWeiPlayerFirst:Run()			--重载一下
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
			--判断一下是否有玩家在附近, 有则删除紧急处理的
			local tarObj = sCharObj:SearchOCompCharObj(self.radius, nil, nil, PLAYER_TYPE)
			if tarObj then
				self.urgentAI:Destroy()
				self.urgentAI = nil
			else
				--判断正常的搜索范围是否有npc, 有则删除紧急处理
				tarObj = sCharObj:SearchOCompCharObj(self.radius, nil, nil, NPC_TYPE)
				if tarObj then
					self.urgentAI:Destroy()
					self.urgentAI = nil
				else
					local ret = Super(AI_WALKAROUNDATTACK.clsAIWalkAroundAttack).Run(self)
					if self.urgentAI then
						return ret
					end
				end
			end
		end
		
		if self.npcExtSkillAI then
			local ret = Super(AI_WALKAROUNDATTACK.clsAIWalkAroundAttack).Run(self)
			if self.npcExtSkillAI then
				return ret
			end
		end		
	else
		if self.urgentAI then
			local ret = Super(AI_WALKAROUNDATTACK.clsAIWalkAroundAttack).Run(self)
			if self.urgentAI then
				return ret
			end
		end		
	end
	
	--当是周围逛状态的时候，搜索一下周围看是否有玩家
	if self.activeAINo == 1 then
		--判断周围是否有玩家, 有则追踪玩家
		local tarObj = sCharObj:SearchOCompCharObj(self.radius, nil, nil, PLAYER_TYPE)
		if tarObj then
			local tarId = tarObj:GetId()
			
			local x, y = tarObj:GetX(), tarObj:GetY()
			local sx, sy = mabs(x - sCharObj:GetX()), mabs(y - sCharObj:GetY())
			local radius = sx > sy and sx or sy
			self:SetRadius(radius)	
			
			self.aAI[2]:SetTarCharId(tarId)
			return self:OnReturn(AI_NEXT)	
		end
		
		if not tarObj then
			--判断正常的搜索范围是否有npc, 有则删除紧急处理
			tarObj = sCharObj:SearchOCompCharObj(self.radius, nil, nil, NPC_TYPE)
			if tarObj then
				local tarId = tarObj:GetId()
				
				local x, y = tarObj:GetX(), tarObj:GetY()
				local sx, sy = mabs(x - sCharObj:GetX()), mabs(y - sCharObj:GetY())
				local radius = sx > sy and sx or sy
				self:SetRadius(radius)	
				
				self.aAI[2]:SetTarCharId(tarId)
				return self:OnReturn(AI_NEXT)	
			end
		end
		
		--周围没玩家, 搜索全景地图是否有敌对的npc, 有则设置紧急处理
		if not tarObj then
			tarObj = sCharObj:SearchOCompCharObj_ByAllChar(NPC_TYPE)
			if tarObj then
				self.aAI[2]:SetTarCharId(nil)
				local x, y = tarObj:GetX(), tarObj:GetY()
				self.urgentAI = AI_WALKTOPOS.clsAIWalkToPos:New(self.charObj, nil, self.radius, 1, self.wTime, self.aTime, x, y, self.walkGrid)
				return AI_CONTINUE
			end
		end
	elseif self.activeAINo == 2 then
		--如果当前的对象是npc, 则搜索附近是否有玩家, 有则追踪玩家
		local tarId = self.aAI[2]:GetTarCharId()
		local tarObj = CHAR_MGR.GetCharById(tarId)
		if tarObj and tarObj:IsNpc() then
			tarObj = sCharObj:SearchOCompCharObj(self.radius, nil, nil, PLAYER_TYPE)
			if tarObj then
				local tarId = tarObj:GetId()
				
				local x, y = tarObj:GetX(), tarObj:GetY()
				local sx, sy = mabs(x - sCharObj:GetX()), mabs(y - sCharObj:GetY())
				local radius = sx > sy and sx or sy
				self:SetRadius(radius)	
				
				self.aAI[2]:SetTarCharId(tarId)
			end
		end
	end
	local ret = Super(AI_WALKAROUNDATTACK.clsAIWalkAroundAttack).Run(self)
	if ret == AI_EXCEPTION then
		--加速下次判断可以搜索人
		self.retTime = 10
	end
	return ret
end