local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local mceil = math.ceil
local tinsert = table.insert
local c_nBlockCnt 		= c_nBlockCnt
local c_nBlockMaxCnt 	= c_nBlockMaxCnt
local c_nHaltCnt 		= c_nHaltCnt					--追随者在被追随者旁边的停留步数c_nHaltCnt后移动一下
local c_aPoint = MOVE_DIR
--AI,EVENT状态--
local AI_CONTINUE 		= AI_CONTINUE
local AI_NEXT 			= AI_NEXT
local AI_EXCEPTION 		= AI_EXCEPTION
local AI_CANCELTARGET	= AI_CANCELTARGET
local AI_NULL 			= AI_NULL

local EVENT_BEATTACK	= EVENT_BEATTACK					--被攻击
local EVENT_TOATTACK	= EVENT_TOATTACK					--指使追随者(例如宠物)攻击

local function gmax(a, b)
	return a > b and a or b
end

------------------击打某物AI类（如果途中被人打，有几率换击打人）-----------------------------
clsAIAttack = AI_BASE.clsAIBase:Inherit()
function clsAIAttack:__init__(charObj, tarCharId, fradius, aTime)
	Super(clsAIAttack).__init__(self, charObj)
	
	self.tarCharId = tarCharId
	self.fradius = fradius
	
	self.aTime = aTime
	self.retTime = aTime
end

function clsAIAttack:GetTime()
	return self.retTime
end

function clsAIAttack:ResetRetTime()
	self.retTime = self.aTime
end

function clsAIAttack:SetRetTime(time)
	self.retTime = time
end

function clsAIAttack:Run()
	self:ResetRetTime()
--	assert(self.tarCharId, "not tarCharId in clsAIAttack:Run!:"..self.charObj:GetName())
	local tarCharObj = self.charObj:FindCharObjByRId(self.tarCharId)					--获取跟踪人
	if not tarCharObj then
		return AI_EXCEPTION
	end
	if tarCharObj:IsDie() then			--获取跟踪人的血量
		return AI_EXCEPTION
	end
	
	local sCharObj = self.charObj
	
	local ox = sCharObj:GetX()			
	local oy = sCharObj:GetY()				
	
	local dx = tarCharObj:GetX()		
	local dy = tarCharObj:GetY()	
	
	local disX = mabs(ox - dx)
	local disY = mabs(oy - dy)
	--如果现在跟目标点一样则返回AI_NEXT
	if disX == 0 and disY == 0 then
		return AI_NEXT
	end

	local fradius = self.fradius
	local sBRadius = sCharObj:GetBattleRadius()	
	local tRadius = gmax(fradius, sBRadius)
	local dTRadius = tRadius ^ 2
	
	--改成取敌我双方,还有fradius的最大范围
	--如果现在目标不在+-[0,1]内则返回AI_NEXT继续追击
--	if disX > tRadius or disY > tRadius then
--		return AI_NEXT
--	end
--	if disX ~= 1 or disY ~= 1 then					--如果disX == 1 和 disY == 1,那么对于dTRadius为1的也是不成功,但是可以打到
		if disX ^ 2 + disY ^ 2 > dTRadius then
			return AI_NEXT
		end
--	end

	local minBRadius = sCharObj:GetMinBattleRadius()
	local dMinBRadius = minBRadius ^ 2
--	if disX ^ 2 + disY ^ 2 < minBRadius ^ 2 then				--只要不重叠就可以了
--		return AI_NEXT
--	end
	
	local selfId = sCharObj:GetId()
	if BASECHAR.AdjustHasObj(sCharObj:GetMapLayer(), ox, oy, selfId) then		--有obj
		if self.mbattlemCnt	< 2 then
			for i = 1, 8 do
				local nx = ox + c_aPoint[i][1]
				local ny = oy + c_aPoint[i][2]
				if not BASECHAR.AdjustHasObj(sCharObj:GetMapLayer(), nx, ny, selfId) and not sCharObj:IsBlockPoint(nx, ny) and sCharObj:CanMoveToOneGrid(ox, oy, nx, ny) then
					local tmpDis = (nx - dx) ^ 2 + (ny - dy) ^ 2
					if tmpDis >= dMinBRadius and tmpDis <= dTRadius then
						self.mbattlemCnt = self.mbattlemCnt + 1
						return AI_NEXT
					end
				end
			end
		end
	end
	
	--对着施法者需要转身
	local dir = BASECHAR.GetDirByPos(ox, oy, dx, dy)
	if dir then sCharObj:SetDir(dir) end
	
	if sCharObj:IsNpc() then
		local skillId, isTips, isWait, _tx, _ty = sCharObj:GetNowSkillId(tarCharObj)
		if isWait then
			--设置已经等待了
			sCharObj:ClearSkillIdWait(skillId)
			if _tx and _ty then
				FIGHT.AddSkillTips(sCharObj, nil, skillId, _tx, _ty)
			else
				FIGHT.AddSkillTips(sCharObj, tarCharObj, skillId)
			end
			
			if IsClient() then
				local triggerType = sCharObj:GetTmp("FubenTriggerType")
				if triggerType == FUBEN_TALK_TRIGGER_SKILLTIPS then
					--触发对话条件
					local talkNo = sCharObj:GetTmp("FubenTalkNo")
					if talkNo then
						ShowFightTalk(talkNo, sCharObj:GetCharNo())
					end
				end
			end
			
			return AI_CONTINUE
		end
		local skillData = sCharObj:GetOneSkillData(skillId)
		if skillData then
			if FIGHT.NpcCheckTarget(sCharObj, tarCharObj, skillData.AttKind, skillData.AttKind2) then
				FIGHT.UseSkillAct(sCharObj, tarCharObj, skillId)
			else				--攻击的人物对应不上
				local tarObj = FIGHT.GetMainTarget(sCharObj, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
				if tarObj then
					FIGHT.UseSkillAct(sCharObj, tarObj, skillId)
				end
			end
			local skillTime = skillData.SkillTime			--npc不需要无间隙出手
			if skillTime and skillTime >= self.aTime then	--技能时间太长了才设置技能的时间
				self:SetRetTime(skillTime)
			end
		end
	elseif sCharObj:IsMirrorPlayer() then		--镜像人
		local skillId, skillTime = sCharObj:GetNowSkillId()
		if skillId then
			local skillData = sCharObj:GetOneSkillData(skillId)
			if skillData then
				if FIGHT.CheckTarget(sCharObj, tarCharObj, skillData.AttKind) then
					FIGHT.UseSkillAct(sCharObj, tarCharObj, skillId)
				else				--攻击的人物对应不上
					local tarObj = FIGHT.GetMainTarget(sCharObj, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
					if tarObj then
						FIGHT.UseSkillAct(sCharObj, tarObj, skillId)
					end
				end
			end
		end
		if skillTime then
			self:SetRetTime(skillTime + 3)
		end
		sCharObj:ClearNowSkillId()		
	end
	return AI_CONTINUE
end

function clsAIAttack:SetTarCharId(tarCharId)
	self.mbattlemCnt = 0
	self.tarCharId = tarCharId
end