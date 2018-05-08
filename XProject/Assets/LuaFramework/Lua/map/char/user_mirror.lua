local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local mceil=math.ceil
local tinsert = table.insert
local ssplit = string.split
local lua_time_sec = lua_time_sec

clsUserMirror = USER.clsUser:Inherit({__ClassType = "USER"})
function clsUserMirror:__init__(vfd, x, y, z, syncData, ociData, mapLayer)
	Super(clsUserMirror).__init__(self, vfd, x, y, z, syncData, ociData, mapLayer)
	
	local range = ociData.AITrackRange or NORMAL_NPC_RADIUS
	local rTbl = {
		minx = x - range,
		maxx = x + range,
		miny = y - range,
		maxy = y + range,
		x = x,
		y = y,
		range = range,
	}
	self:SetAIRange(rTbl)
	
	self:SetSpeed(SPEED_NORMAL)
	self:AdjustSpeed()		--调整速度
	local sTime = SPEED_TIME[self:GetSpeed()]
	assert(sTime, string.format("name:%s not sTime:%s", self:GetName(), self:GetSpeed()))
	
	if self:GetMirrorType() == USER_MIRROR_WULIN then
		local aiObj = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:New(self, range * 5, 1, sTime, 5, 2)
		self:SetAI(aiObj)
	elseif self:GetMirrorType() == USER_MIRROR_WALKONLY then
		local aiObj = AI_WALKAROUND.clsAIWalkAround:New(self, 10, 10, 2)
		aiObj.wTime = 10 + math.random(5)
		self:SetAI(aiObj)		
	else
		local aiObj = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:New(self, range, 1, sTime, 5, 2)		--atkTime 为1好放普通技能
		self:SetAI(aiObj)
	end
end

function clsUserMirror:GetAIRange()
	return self.__airange
end
function clsUserMirror:SetAIRange(range)
	self.__airange = range
end

function clsUserMirror:IsMirrorPlayer()
	return true
end

function clsUserMirror:SearchOCompCharObj(radius, isCircle, notCheck9)
	if self:GetMirrorType() == USER_MIRROR_WULIN then
		return Super(clsUserMirror).SearchOCompCharObj(self, radius, isCircle, true)
	else
		return Super(clsUserMirror).SearchOCompCharObj(self, radius, isCircle, notCheck9)
	end
end

function clsUserMirror:GetMirrorType()
	return self:GetTmp("MirrorType")
end

function clsUserMirror:SetMirrorType(mirrorType)
	self:SetTmp("MirrorType", mirrorType)
end

function clsUserMirror:GetNowSkillId()
	local allSkill = self:GetAllSkill()
	
	local nTimeNo = GetNowTimeNo()
	
	--优先选择分段技能
	local frontSkillId = self:GetFrontSkillId()
	if frontSkillId then
		for _skillId, _data in pairs(allSkill) do		
			if _data.Type == SKILL_TYPE_INITIATIVE then
				if _data.FrontSkill == frontSkillId then
					local timeNoCnt = _data.CD
					local eTimeNo = _data.CDEndTimeNo
					if nTimeNo >= eTimeNo then
						_data.CDEndTimeNo = nTimeNo + timeNoCnt					--之后真正释放的时候再重新设置,因为这个可能是预先提示的
						return _skillId, _data.SkillTime
					end
				end
			end
		end
	end
	--技能优先
	for _skillId, _data in pairs(allSkill) do
		if _data.Type == SKILL_TYPE_INITIATIVE and not _data.FrontSkill then
			if _data.Mtype == SKILL_MTYPE_MAGIC then
				local timeNoCnt = _data.CD
				local eTimeNo = _data.CDEndTimeNo
				local cCoolTime = _data.CreateCoolTime
				if not cCoolTime or nTimeNo > cCoolTime then
					if nTimeNo >= eTimeNo then
						_data.CDEndTimeNo = nTimeNo + timeNoCnt					--之后真正释放的时候再重新设置,因为这个可能是预先提示的
						return _skillId, _data.SkillTime
					end
				end
			end
		end
	end
	--普通攻击
	for _skillId, _data in pairs(allSkill) do
		if _data.Type == SKILL_TYPE_INITIATIVE and not _data.FrontSkill then
			if _data.Mtype == SKILL_MTYPE_NORMAL then
				return _skillId, _data.SkillTime
			end
		end
	end	
end

function clsUserMirror:SetRetHpFunc( func )
	self.__RetHpFunc = func
end

function clsUserMirror:SetCanRelive( canRelive )
	self.__canRelive = canRelive
end

function clsUserMirror:GetCanRelive()
	return self.__canRelive
end

function clsUserMirror:SetRelivePos(x,y,z)
	self._reliveData = {
		x = x,
		y = y,
		z = z,
	}
end

function clsUserMirror:SetHp(hp, attId, notSync, stype, isNotRetHp)
	local ohp = self:GetHp()
	Super(clsUserMirror).SetHp(self, hp, attId, notSync, stype, isNotRetHp)
	local hp = self:GetHp()
	if  self.__RetHpFunc  then 
		self.__RetHpFunc(self, hp , ohp , attId)
	end 
	-- if hp <= 0 then 
	-- 	if self.__canRelive then
	-- 		lua_ClientRelive( {
	-- 			id = self:GetId() ,
	-- 			reliveData = self._reliveData or {}
	-- 		})
	-- 	end
	-- end
end 