local assert = assert
local pairs = pairs
local mrandom = math.random
local mceil = math.ceil
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

------------------【追随】,在途中被击打(不包括追随者)或者玩家击打某物则【跟踪并且击打】AI类----------
clsAIFollowAttack = AI_BASE.clsAIBase:Inherit()
function clsAIFollowAttack:__init__(charObj, followCharId, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIFollowAttack).__init__(self, charObj)
	
	self.followCharId = followCharId
	
	if fradius > radius then
		_RUNTIME_ERROR("fradius is > radius:", fradius, radius)
		fradius = radius
	end
	local wTime = wTime or AI_WALK_TIME
	local aTime = aTime or AI_ATTACK_TIME
	
	local followAI = AI_FOLLOW.clsAIFollow:New(charObj, followCharId, radius, 1, wTime, aTime)
	local walkToCharacterAndAttack = AI_WALKTOCHARANDATTACK.clsAIWalkToCharacterAndAttack:New(charObj, nil, radius, fradius, wTime, aTime, walkGrid)
	self.aAI[1] = followAI
	self.aAI[2] = walkToCharacterAndAttack
	
	self.aNextNo[1] = 2
	self.aNextNo[2] = 1
	
	self.aExceptionNo[2] = 1

	self.wTime = wTime
	self.aTime = aTime
	self.radius = radius
	
	self.nextTime = nil
	self.walkGrid = walkGrid or 1
end

function clsAIFollowAttack:GetTime()
	if self.urgentAI then
		return Super(clsAIFollowAttack).GetTime(self)
	end
	
	return self.aTime or Super(clsAIFollowAttack).GetTime(self)
end

function clsAIFollowAttack:Run()
	if not self.urgentAI then
		local sCharObj = self.charObj
		local followCharId = self.followCharId
				
		--判断是否玩家死亡就同伴死亡
		local tarCharObj = sCharObj:FindCharObjByRId(followCharId) 			--整张地图搜索玩家坐标，(没有则返回nil)
		if not tarCharObj then
			return AI_CANCELTARGET
		end
		if tarCharObj:IsDie() then
			return AI_CANCELTARGET
		end
		
		sCharObj:TryAutoSkill()	
		local skillId = sCharObj:GetNowSkillId()
		if skillId then
			local skillData = sCharObj:GetOneSkillData(skillId)
			if skillData then
				if FIGHT.CheckGoodAttKind(skillData.AttKind) then		--对自己有好处的
					local tarObj = FIGHT.GetMainTarget(sCharObj, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
					if tarObj then
						if FIGHT.UseSkillAct(sCharObj, tarObj, skillId) then
							sCharObj:ClearNowSkillId()
							return AI_CONTINUE
						end
					end
				end
			end
		end
		
		if self.activeAINo == 1 then
			local tarObj = sCharObj:SearchOCompCharObj(self.radius)
			if tarObj then							--主动攻击
				self.aAI[2]:SetTarCharId(tarObj:GetId())
				return self:OnReturn(AI_NEXT)	
			end
		end
	end
	
	local ret = Super(clsAIFollowAttack).Run(self)
	
	return ret
end

--设置同伴攻击谁
function clsAIFollowAttack:SetAttTarId(tarId)
end

--设置同伴追随自己(参数为true就是follow)
function clsAIFollowAttack:SetFollowMe(isFollow)
	if isFollow then
		if self.urgentAI then return end
		self.urgentAI = AI_FOLLOW.clsAIFollow:New(self.charObj, self.followCharId, self.radius, 1, self.wTime, self.aTime, self.walkGrid)
	else
		self.urgentAI = nil
	end
end

--事件的改变，例如被攻击
function clsAIFollowAttack:OnEvent(eventTbl)	--如果是被追随的人打击就不处理
--	if self.activeAINo == 1 then
--		if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK then
--			if eventTbl.eventAttackCharId ~= self.followCharId then
--				self.aAI[2]:SetTarCharId(eventTbl.eventAttackCharId)
--			else	--如果是被追随者攻击自己则返回
--				return AI_CONTINUE
--			end
--		end
--		return AI_CONTINUE
--	elseif self.activeAINo == 2 then
--		if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK then
--			if eventTbl.eventAttackCharId ~= self.followCharId then
--				--设置一下，因为clsAIWalkToCharacterAndAttack只处理这个
--				eventTbl.eventType = EVENT_BEATTACK		
--			else	--如果是被追随者攻击自己则返回
--				return AI_CONTINUE
--			end
--		end		
--	end
--	if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK then
--		local x, y = eventTbl.attX, eventTbl.attY
--		assert(x and y, "not clsAIFollowAttack:OnEvent attX, attY")
--		local sx, sy = mabs(x - self.charObj:GetX()), mabs(y - self.charObj:GetY())
--		local radius = sx > sy and sx or sy
--		self:SetRadius(radius)	
--	end
--	return Super(clsAIFollowAttack).OnEvent(self, eventTbl)
end