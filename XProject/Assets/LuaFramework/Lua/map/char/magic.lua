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
local MAGIC_HITTYPE = MAGIC_HITTYPE

local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能

local pbc_send_msg = pbc_send_msg

clsMagic = BASECHAR.clsBaseChar:Inherit({__ClassType = "MAGIC"})			

function clsMagic:__init__(ociData, ownerId, mapLayer)
	assert(ownerId, "magic not ownerId")
	assert(mapLayer, "magic not mapLayer")
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
	
	ownerObj:AddMagic(self)
	self.IsInInit = true
	
	self:SetName(ownerObj:GetName())
	for _name, _value in pairs(ociData) do
		if self["Set".._name] then
			self["Set".._name](self, _value)
		else
			self:SetTmp(_name, _value)
		end
	end
--	self:AddMartial(1000013, 1)
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
function clsMagic:RefreshSyncDataStr(isNotSync)
	self.__syncdstr = string.format("%s;%d;%d", self:GetId(), self:GetShape(), self:GetFId())
	local ownerObj = self:GetOwner()
	local syncData = ownerObj:GetSyncData()
	syncData.magic = self.__syncdstr
	ownerObj:SetSyncData(syncData, isNotSync)
end

function clsMagic:GetHp()
	return 1
end
function clsMagic:SetHp(Hp)
--	error("can not SetHp")
end
function clsMagic:GetHpMax()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetHpMax() or 1
	end
	return 1	
end
function clsMagic:AddSp(addSp)
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:SubSp(-addSp)
	end
end
function clsMagic:GetAp()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetAp() or 1
	end
	return 1
end
function clsMagic:SetAp(Ap)
--	error("can not SetAp")
end
function clsMagic:GetMa()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetMa() or 1
	end
	return 1
end
function clsMagic:SetMa(Ma)
--	error("can not SetMa")
end
function clsMagic:GetDp()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDp() or 1
	end
	return 1
end
function clsMagic:SetDp(Dp)
--	error("can not SetDp")
end
function clsMagic:GetMr()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetMr() or 1
	end
	return 1
end
function clsMagic:SetMr(Mr)
--	error("can not SetMr")
end
function clsMagic:GetSpeed()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetSpeed() or 1
	end
	return 1
end
function clsMagic:SetSpeed(Speed)
--	error("can not SetSpeed")
end
function clsMagic:GetDouble()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDouble() or 1
	end
	return 1
end
function clsMagic:SetDouble(Double)
--	error("can not SetDouble")
end
function clsMagic:GetTenacity()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetTenacity() or 1
	end
	return 1
end
function clsMagic:SetTenacity(Tenacity)
--	error("can not SetTenacity")
end
function clsMagic:GetParry()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetParry() or 1
	end
	return 1
end
function clsMagic:SetParry(Parry)
--	error("can not SetParry")
end
function clsMagic:GetReParry()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetReParry() or 1
	end
	return 1
end
function clsMagic:SetReParry(ReParry)
--	error("can not SetReParry")
end
function clsMagic:GetHitRate()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetHitRate() or 1
	end
	return 1
end
function clsMagic:SetHitRate(HitRate)
--	error("can not SetHitRate")
end
function clsMagic:GetDodge()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDodge() or 1
	end
	return 1
end
function clsMagic:SetDodge(Dodge)
--	error("can not SetDodge")
end
function clsMagic:GetDoubleHurt()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetDoubleHurt() or 1
	end
	return 1
end
function clsMagic:SetDoubleHurt(DoubleHurt)
--	error("can not SetDoubleHurt")
end
function clsMagic:GetReDoubleHurt()
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetReDoubleHurt() or 1
	end
	return 1
end
function clsMagic:SetReDoubleHurt(ReDoubleHurt)
--	error("can not SetReDoubleHurt")
end
----------------------------------------------end 重载战斗信息--------------------------------------------------

function clsMagic:IsMagic()
	return true
end
function clsMagic:GetComp()
	return self.__comp
end
function clsMagic:SetComp(comp)
	self.__comp = comp
end
function clsMagic:GetBuff()
	return self.__buff or {}
end
function clsMagic:SetBuff(buff)
	self.__buff = buff
end
function clsMagic:GetShape()
	return self.__shape or 0
end
function clsMagic:SetShape(shape, isNotSync)
	self.__shape = shape
	self:RefreshSyncDataStr(isNotSync)
end

function clsMagic:GetOwnerId()
	return self.__ownerId
end
function clsMagic:SetOwnerId(ownerId)
	self.__ownerId = ownerId
end
function clsMagic:GetOwner()
	local ownerObj = CHAR_MGR.GetCharById(self:GetOwnerId())
	if not ownerObj then return end
	return ownerObj
end
function clsMagic:IsHitTarget(tarObj)	
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
function clsMagic:GetEngineObj()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetEngineObj()
	end
end
function clsMagic:GetX()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetX()
	end
end
function clsMagic:GetY()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetY()
	end
end
function clsMagic:GetZ()				--返回主角
	local ownerObj = self:GetOwner()
	if ownerObj then
		return ownerObj:GetZ()
	end
end

function clsMagic:GetHitType()
	return MAGIC_HITTYPE
end

function clsMagic:SetMagicAp(magicAp)
	self.__tmp.MagicAp = magicAp
end
function clsMagic:GetMagicAp()
	return self.__tmp.MagicAp or 0
end

function clsMagic:SetMagicHurt(magicHurt)
	self.__tmp.MagicHurt = magicHurt
end
function clsMagic:GetMagicHurt()
	return self.__tmp.MagicHurt or 0
end

function clsMagic:SkillCheckOnce(tarCharObj)
	if not self:IsHitTarget(tarCharObj) then return end
	local allSkill = self:GetAllSkill()
	for skillId, skillData in pairs(allSkill) do
		if skillData.Type == SKILL_TYPE_INITIATIVE then
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

function clsMagic:GetOldAllSkill()
	local oAllSkill = self:GetTmp("OldAllSkill")
	if not oAllSkill then
		oAllSkill = {}
		self:SetTmp("OldAllSkill", oAllSkill)
	end
	return oAllSkill
end
function clsMagic:SetOldAllSkill(skillTbl)
	self:SetTmp("OldAllSkill", skillTbl)
end

--销毁一个对象
function clsMagic:Destroy()
	FIGHT_EVENT.DelAllBuff(self)
	local Id = self:GetId()
	CHAR_MGR.RemoveCharId(Id)
end