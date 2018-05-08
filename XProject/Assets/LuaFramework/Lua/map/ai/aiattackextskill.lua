local assert = assert
local pairs = pairs
local math = math
local mfloor = math.floor
local AI_CONTINUE = AI_CONTINUE
local lua_time_sec = lua_time_sec

clsAIAttackExtSkill = AI_BASE.clsAIBase:Inherit()
function clsAIAttackExtSkill:__init__(charObj, skillId, aTime)
	Super(clsAIAttackExtSkill).__init__(self, charObj)
	
	local aTime = aTime or AI_ATTACK_TIME
	self.aTime = aTime
	self.retTime = aTime
	self.skillId = skillId
	
	self.tipsId = nil
end

function clsAIAttackExtSkill:GetTime()
	return self.retTime
end

function clsAIAttackExtSkill:ResetRetTime()
	self.retTime = self.aTime
end

function clsAIAttackExtSkill:SetRetTime(time)
	self.retTime = time
end

function clsAIAttackExtSkill:Destroy()			--如果在预警死亡则删除预警
	if self.tipsId then
		FIGHT.DelSkillTipsByTipsId(self.charObj, self.tipsId)
	end
end

function clsAIAttackExtSkill:Run()
	self:ResetRetTime()
	local sCharObj = self.charObj
	if sCharObj:IsDie() then return AI_EXCEPTION end
	
	local skillId = self.skillId
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if skillData then
		if self.tipsId then
			TryCall(FIGHT.UseSkillAct, sCharObj, nil, skillId, sCharObj:GetX(), sCharObj:GetY())
			FIGHT.DelSkillTipsByTipsId(sCharObj, self.tipsId)
			return AI_EXCEPTION
		else
			local tipsTime = skillData.BeforeTips
			local x, y = sCharObj:GetX(), sCharObj:GetY()
			if not tipsTime then
				--直接出技能
				TryCall(FIGHT.UseSkillAct, sCharObj, nil, skillId, x, y)
			else
				tipsTime = mfloor(tipsTime / (1000 * lua_time_sec))
				self:SetRetTime(tipsTime)
				self.tipsId = FIGHT.AddSkillTips(sCharObj, nil, skillId, x, y)
				return AI_CONTINUE
			end
		end
	end
	
	return AI_EXCEPTION
end