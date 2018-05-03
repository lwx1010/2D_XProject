local assert = assert
local pairs = pairs
local AI_CONTINUE 		= AI_CONTINUE

clsAIWBossAttack = AI_BASE.clsAIBase:Inherit()
function clsAIWBossAttack:__init__(charObj, aTime, x, y)
	Super(clsAIWBossAttack).__init__(self, charObj)
	
	local aTime = aTime or AI_ATTACK_TIME
	self.aTime = aTime
	self.retTime = aTime
	
	self.tx = x
	self.ty = y
end

function clsAIWBossAttack:GetTime()
	return self.retTime
end

function clsAIWBossAttack:ResetRetTime()
	self.retTime = self.aTime
end

function clsAIWBossAttack:SetRetTime(time)
	self.retTime = time
end

function clsAIWBossAttack:Run()
	self:ResetRetTime()
	local sCharObj = self.charObj
	if sCharObj:IsDie() then return end
	
	--如果是有提示技能的
	local skillId, tipsTime = sCharObj:IsNowTipsSkillId()
	if skillId then
		local allSkill = sCharObj:GetAllSkill()
		local skillData = allSkill[skillId]
		if not skillData then 
			return AI_CONTINUE
		end
		FIGHT.UseSkillAct(sCharObj, nil, skillId, self.tx, self.ty)
		skillData.x = nil
		skillData.y = nil
		sCharObj:ClearNowSkillId()
		FIGHT.DelSkillTips(sCharObj, skillId)
		
		local skillTime = skillData.SkillTime
		if skillTime then
			self:SetRetTime(skillTime + self.aTime)
		end
		
		return AI_CONTINUE
	else
		local skillId, tipsTime = sCharObj:GetNowSkillId()
		if tipsTime then
			FIGHT.AddSkillTips(self.charObj, nil, skillId, self.tx, self.ty)
			return AI_CONTINUE
		else
			local skillData = sCharObj:GetOneSkillData(skillId)
			if skillData then
				FIGHT.UseSkillAct(sCharObj, nil, skillId, self.tx, self.ty)

				local skillTime = skillData.SkillTime
				if skillTime then
					self:SetRetTime(skillTime + self.aTime)
				end
			end
			return AI_CONTINUE
		end
	end
end