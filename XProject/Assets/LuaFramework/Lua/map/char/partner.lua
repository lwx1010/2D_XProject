local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local mceil=math.ceil
local mabs=math.abs
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local tinsert = table.insert
local c_aPoint = MOVE_DIR
local lua_time_sec = lua_time_sec
local BATTLE_CAMP_1 = BATTLE_CAMP_1
local BATTLE_CAMP_2 = BATTLE_CAMP_2
local SP_MAX = SP_MAX
local DISTANCE_ADJUST_SPEED = DISTANCE_ADJUST_SPEED
local PARTNER_HITTYPE = PARTNER_HITTYPE

local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能

local pbc_send_msg = pbc_send_msg

clsPartner = BASECHAR.clsBaseChar:Inherit({__ClassType = "PARTENR"})			

function clsPartner:__init__(ociData, ownerId, mapLayer)
	assert(ownerId, "partner not ownerId")
	assert(mapLayer, "partner not mapLayer")
	local ownerObj = CHAR_MGR.GetCharById(ownerId)
	if not ownerObj or ownerObj:IsDie() then return end

	self._fid = CHAR_MGR.NewFightId()
	self:SetMapLayer(mapLayer)
	self:SetOwnerId(ownerId)
	self:SetComp(ownerObj:GetComp())
	
	self.__tmp = {}
	self.__init_time = os.time() --对象初始化时间
	local id = CHAR_MGR.NewId()
	self.__ID = id
	
	ownerObj:AddPartner(self)
	self.IsInInit = true
	
	self:SetName(ownerObj:GetName())
	for _name, _value in pairs(ociData) do
		if self["Set".._name] then
			self["Set".._name](self, _value)
		else
			self:SetTmp(_name, _value)
		end
	end
	self:AddMartial(1000012, 1)
	self.IsInInit = nil
	
	CHAR_MGR.AddCharId(self.__ID, self) 		--有可能AddMap报错，所以最后才加入CHAR_MGR管理
	
	--放到最后,因为buff需要添加到ui界面的
	local initBuff = self:GetTmp("InitBuff")
	if initBuff then
		for _buffId, _buffInfo in pairs(initBuff) do
			if _buffInfo.notFight then
				FIGHT_EVENT.AddNotFightBuff(self, _buffInfo)
			else
				FIGHT_EVENT.AddBuff(self, self, _buffInfo)
			end
		end 
	end
	
--	FIGHT_EVENT.ProcessPassMessage(PASS_BATTLE, self, nil, nil)				--触发被动技能
	
	local t1HpMax = self:GetHpMax() or 0
	local t2HpMax = self:GetFightValue(mFIGHT_HpMax) or 0
	if t1HpMax ~= t2HpMax then
		self:AddHp(t2HpMax - t1HpMax, 0)
	end
	
	self:RefreshSyncDataStr()
end

----------------------------------------------start 重载战斗信息--------------------------------------------
function clsPartner:RefreshSyncDataStr(isNotSync)
	self.__syncdstr = string.format("%s;%d;%d", self:GetId(), self:GetShape(), self:GetFId())
	local ownerObj = self:GetOwner()
	local syncData = ownerObj:GetSyncData()
	syncData.partner = self.__syncdstr
	ownerObj:SetSyncData(syncData, isNotSync)
end

function clsPartner:GetHp()
	return 1
end
function clsPartner:SetHp(Hp)
--	error("can not SetHp")
end
function clsPartner:GetHpMax()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetHpMax() or 1
	end
	return 1	
end
function clsPartner:AddSp(addSp)
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:SubSp(-addSp)
	end
end
function clsPartner:GetAp()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetAp() or 1
	end
	return 1
end
function clsPartner:SetAp(Ap)
--	error("can not SetAp")
end
function clsPartner:GetMa()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetMa() or 1
	end
	return 1
end
function clsPartner:SetMa(Ma)
--	error("can not SetMa")
end
function clsPartner:GetDp()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDp() or 1
	end
	return 1
end
function clsPartner:SetDp(Dp)
--	error("can not SetDp")
end
function clsPartner:GetMr()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetMr() or 1
	end
	return 1
end
function clsPartner:SetMr(Mr)
--	error("can not SetMr")
end
function clsPartner:GetSpeed()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetSpeed() or 1
	end
	return 1
end
function clsPartner:SetSpeed(Speed)
--	error("can not SetSpeed")
end
function clsPartner:GetDouble()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDouble() or 1
	end
	return 1
end
function clsPartner:SetDouble(Double)
--	error("can not SetDouble")
end
function clsPartner:GetTenacity()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetTenacity() or 1
	end
	return 1
end
function clsPartner:SetTenacity(Tenacity)
--	error("can not SetTenacity")
end
function clsPartner:GetParry()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetParry() or 1
	end
	return 1
end
function clsPartner:SetParry(Parry)
--	error("can not SetParry")
end
function clsPartner:GetReParry()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetReParry() or 1
	end
	return 1
end
function clsPartner:SetReParry(ReParry)
--	error("can not SetReParry")
end
function clsPartner:GetHitRate()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetHitRate() or 1
	end
	return 1
end
function clsPartner:SetHitRate(HitRate)
--	error("can not SetHitRate")
end
function clsPartner:GetDodge()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDodge() or 1
	end
	return 1
end
function clsPartner:SetDodge(Dodge)
--	error("can not SetDodge")
end
function clsPartner:GetDoubleHurt()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDoubleHurt() or 1
	end
	return 1
end
function clsPartner:SetDoubleHurt(DoubleHurt)
--	error("can not SetDoubleHurt")
end
function clsPartner:GetReDoubleHurt()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetReDoubleHurt() or 1
	end
	return 1
end
function clsPartner:SetReDoubleHurt(ReDoubleHurt)
--	error("can not SetReDoubleHurt")
end
----------------------------------------------end 重载战斗信息--------------------------------------------------

function clsPartner:IsPartner()
	return true
end
function clsPartner:GetComp()
	return self.__comp
end
function clsPartner:SetComp(comp)
	self.__comp = comp
end
function clsPartner:GetBuff()
	return self.__buff or {}
end
function clsPartner:SetBuff(buff)
	self.__buff = buff
end
function clsPartner:GetShape()
	return self.__shape or 0
end
function clsPartner:SetShape(shape, isNotSync)
	self.__shape = shape
	self:RefreshSyncDataStr(isNotSync)
end

function clsPartner:GetOwnerId()
	return self.__ownerId
end
function clsPartner:SetOwnerId(ownerId)
	self.__ownerId = ownerId
end
function clsPartner:GetOwner()
	local ownerObj = CHAR_MGR.GetCharById(self:GetOwnerId())
	if not ownerObj then return end
	return ownerObj
end
function clsPartner:IsHitTarget(tarObj)	
	if tarObj:IsPlayer() then
		local ownerObj = self:GetOwner()
		if ownerObj then
			local pkmode 
			if ownerObj:GetPkMode() == PKMODE_HOSTILECLUB then 
				pkmode = ownerObj:GetPkMode()
			end
			if pkmode == PKMODE_HOSTILECLUB and not FLAG then 		
				return 
			end
			return ownerObj:IsHitTarget(tarObj)
		end
	else
		return self:GetComp() ~= tarObj:GetComp()
	end
end
function clsPartner:GetEngineObj()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetEngineObj()
	end
end
function clsPartner:GetX()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetX()
	end
end
function clsPartner:GetY()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetY()
	end
end
function clsPartner:GetZ()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetZ()
	end
end

function clsPartner:GetHitType()
	return PARTNER_HITTYPE
end

function clsPartner:SetPartnerAp(partnerAp)
	self.__tmp.PartnerAp = partnerAp
end
function clsPartner:GetPartnerAp()
	return self.__tmp.PartnerAp or 0
end

function clsPartner:SetPartnerHurt(partnerHurt)
	self.__tmp.PartnerHurt = partnerHurt
end
function clsPartner:GetPartnerHurt()
	return self.__tmp.PartnerHurt or 0
end

function clsPartner:SetPartnerExtraHurt(partnerExtraHurt)
	self.__tmp.PartnerExtraHurt = partnerExtraHurt
end
function clsPartner:GetPartnerExtraHurt()
	return self.__tmp.PartnerExtraHurt or 0
end

function clsPartner:SkillCheckOnce(tarCharObj)
	if not self:IsHitTarget(tarCharObj) then return end
	local allSkill = self:GetAllSkill()
	for skillId, skillData in pairs(allSkill) do
		if skillData.Type == SKILL_TYPE_INITIATIVE then
			if skillData.Mtype == SKILL_MTYPE_MAGIC then			--技能攻击
				local timeNoCnt = skillData.CD
				local eTimeNo = skillData.CDEndTimeNo
				local nTimeNo = GetNowTimeNo()
				if nTimeNo >= eTimeNo then
					skillData.CDEndTimeNo = nTimeNo + timeNoCnt
					self.__nskillid = skillId
					--执行技能
					if FIGHT.CheckTarget(self, tarCharObj, skillData.AttKind) then
						FIGHT.UseSkillAct(self, tarCharObj, skillId)
					else				--攻击的人物对应不上
						local tarObj = FIGHT.GetMainTarget(self, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
						if tarObj then
							FIGHT.UseSkillAct(self, tarObj, skillId)
						end
					end
					return 
				end
			end
		end
	end
	
	for skillId, skillData in pairs(allSkill) do
		if skillData.Type == SKILL_TYPE_INITIATIVE then
			if skillData.Mtype == SKILL_MTYPE_NORMAL then			--普通攻击
				local timeNoCnt = skillData.CD
				local eTimeNo = skillData.CDEndTimeNo
				local nTimeNo = GetNowTimeNo()
				if nTimeNo >= eTimeNo then
					skillData.CDEndTimeNo = nTimeNo + timeNoCnt
					self.__nskillid = skillId
					--执行技能
					if FIGHT.CheckTarget(self, tarCharObj, skillData.AttKind) then
						FIGHT.UseSkillAct(self, tarCharObj, skillId)
					else				--攻击的人物对应不上
						local tarObj = FIGHT.GetMainTarget(self, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
						if tarObj then
							FIGHT.UseSkillAct(self, tarObj, skillId)
						end
					end
					return 
				end
			end
		end
	end
end

--销毁一个对象
function clsPartner:Destroy()
	FIGHT_EVENT.DelAllBuff(self)
	local Id = self:GetId()
	CHAR_MGR.RemoveCharId(Id)
end