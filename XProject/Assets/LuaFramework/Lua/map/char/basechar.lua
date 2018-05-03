local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local tsize = table.size
local mrandom = math.random
local mabs=math.abs
local mfloor=math.floor
local mceil=math.ceil
local MAP_LAYER_CHAR = MAP_LAYER_CHAR
local MAP_MAX_X = MAP_MAX_X
local MAP_MAX_Y = MAP_MAX_Y
local MAP_LAYER_DATA = MAP_LAYER_DATA
local MAP_NO = MAP_NO
local MAP_DATA_OBJ = MAP_DATA_OBJ
local WeakMetaTbl = {__mode = "v"}
local tinsert = table.insert
local tremove = table.remove
local msqrt = math.sqrt
local c_aPoint = MOVE_DIR
local c_aAdjustPoint = MOVE_ADJUST
local NORMAL_SLOPE = NORMAL_SLOPE
local NORMAL_TBL = {}
local mNUMBERTYPE = mNUMBERTYPE
local mTABLETYPE = mTABLETYPE
local FIGHT_EFF_NAME=FIGHT_EFF_NAME
local HURT_TYPE_00=HURT_TYPE_00
local SP_MAX = SP_MAX
local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能
local BUFF_DIZZINESS 	= BUFF_DIZZINESS		--眩晕
local BUFF_BIND			= BUFF_BIND				--缠绕
local BUFF_FREEZE		= BUFF_FREEZE			--冰冻
local BUFF_PETRIFACTION	= BUFF_PETRIFACTION		--石化	
local DISTANCE_SORT = DISTANCE_SORT
local DISTANCE_SORT_CNT = #DISTANCE_SORT
local PRO_TANK = PRO_TANK
local MIN_GRID_XY = MIN_GRID_XY
local lua_time_sec = lua_time_sec
local IS_FIGHT_MAP = IS_FIGHT_MAP

local PASS_HP			= PASS_HP		--当自身hp低于，（注意:敌方的不判断）
local PASS_BATTLE		= PASS_BATTLE	--当进入战斗模式
local PASS_HIT			= PASS_HIT		--命中敌人时
local PASS_SHIT			= PASS_SHIT		--技能命中敌人时(前一个包含普通和合体技能)
local PASS_BEHIT		= PASS_BEHIT	--受击时
local PASS_DIE			= PASS_DIE		--死亡的时候
local PASS_RELIVE		= PASS_RELIVE	--添加复活时机

local lserialize = lserialize
local pbc_send_msg = pbc_send_msg
local SECURITY_AREAS = SECURITY_AREAS	--安全区域
local GetNowTimeNo = GetNowTimeNo
local GetSecondTimeCnt = GetSecondTimeCnt

local IsKuaFuServer = cfgData and cfgData.IsKuaFuServer

local TmpBaseChar = {
	__ClassType = "<basechar>",
}

local _NOT_SYNC_INIT = {
	HpMax = true,
	Comp = true,
	Speed = true,
	Score = true,
	FlyDodge = true,
	PkMode = true,
	TeamId = true,
	ClubId = true,
	EvilState = true,
	ServerId = true,
	ThugModel = true,
	LingqiModel = true,
	DaZuo = true,
	LingqinModel = true,
	LingyiModel = true,
	ThugHorseModel = true,
	PetModel = true,
	ShenjianModel = true,
	ShenyiModel = true,
	MountModel = true,
	UpMountModel = true,
	UpHorseModel = true,
	ActivateWeapon = true,
	Fashion = true,
	Grade = true,
	IsYunBiao = true,
	ClubName = true,
	ClubPost = true,	
	LingqiModelState = true,
	LingqinModelState = true,
	LingyiModelState = true,
	ThugModelState = true,
	ThugHorseModelState = true,
	PetModelState = true,
	ShenjianModelState = true,
	ShenyiModelState = true,
	Vip = true,
	DoubleXiulian = true,
	Title = true,
	DoubleXiulianEffect = true,
	DoubleXiulianDir360 = true,
	Photo = true,
	HostileClub = true,
	MateName = true,
	WeddingShapeState = true,
	MultiKill = true,
	EnemyName = true,
	Name = true,
}

clsBaseChar = clsObject:Inherit(TmpBaseChar)

function clsBaseChar:__init__(x, y, z, charType, syncData, ociData, mapLayer)
	assert(x and y and z, "must input x y z")
	assert(charType and syncData and ociData, "must input charType and syncData and ociData")
	assert(CHAR_TYPE_TBL[charType], "not type charType:" .. charType)
	assert(mapLayer and MAP_LAYER_DATA[mapLayer], "must input correct mapLayer")

	if ociData.InitBuff then
		if type(ociData.InitBuff) == mSTRINGTYPE then
			ociData.InitBuff = assert(loadstring("return "..ociData.InitBuff))()
		end
	end

	self:SetMapLayer(mapLayer)	
	Super(clsBaseChar).__init__(self)
	self.__tmp = {}
	self.__init_time = os.time() --对象初始化时间
--	self.__ID = CHAR_MGR.NewId() --对象ID									--需要在ociData中有ID
	self.__ID = assert(ociData["Id"], "not ID in ociData")
	self.__x = x
	self.__y = y
	self.__z = z
	self.__AI = nil
	self._fid = CHAR_MGR.NewFightId()
	self:SetSyncData(syncData, true)
	
	if IsKuaFuServer and self:IsPlayer() then
		CHAR_MGR.AddKuaFuCharVfd(self:GetVfd(), self)
	end
	
	if IsServer() then
		self.__engineobj = nil			--引擎的对象
		local tmpEngineobj = laoi.obj_new(self:GetId(), x, y, self.__hp or 0, charType)
		assert(tmpEngineobj, "new engine obj error")
		self.__engineobj = tmpEngineobj
	else
		self.__engineobj = self
	end
	
	self.IsInInit = true
	for _name, _value in pairs(ociData) do
		if self["Set".._name] then
			if _name == "Hp" then
				if IsServer() then
					self["Set".._name](self, _value, 0, true, nil, true, true)	--不广播
				else
					if self.__npc_hpRate then
						self["Set".._name]( self, (ociData["HpMax"] or _value)*self.__npc_hpRate, 0, true, nil, true, true)	--不广播
					else
						self["Set".._name](self, ociData["HpMax"] or _value, 0, true, nil, true, true)	--不广播
					end
				end
			elseif _NOT_SYNC_INIT[_name] then
				self["Set".._name](self, _value, true)				--不广播
			else
				self["Set".._name](self, _value)
			end
		else
			self:SetTmp(_name, _value)
		end
	end
	self.IsInInit = nil
	self:SetComp(self:GetComp(), true)			--设置到引擎
	--调用添加进MAP_LAYER_DATA对象后的处理
	self:AddMap()
	if not self:IsItem() and not self:IsStaticNpc() then
		self:SetHp(self:GetHp(), 0)									--广播血
	end
	
	CHAR_MGR.ClearHurtOtherHp(self.__ID)		--清除攻击伤害
	CHAR_MGR.ClearBearHurtHp(self.__ID)			--清除被攻击伤害
	CHAR_MGR.ClearReHp(self.__ID)				--清除回血
	
	CHAR_MGR.AddCharId(self.__ID, self) 		--有可能AddMap报错，所以最后才加入CHAR_MGR管理
end

--用metatable实现继承,这样子类继承自clsBaseChar时，可以通过元表功能实现继承
function clsBaseChar:Inherit(o)
	o = o or {}
	setmetatable(o, {__index = self})
	o.__SuperClass = self
	return o
end

function clsBaseChar:GetFId()
	return self._fid
end

function clsBaseChar:GetMapLayer()
	return self.__maplayer
end

function clsBaseChar:SetMapLayer(layer)
	self.__maplayer = layer
end

function clsBaseChar:GetMapObj()
	local layer = self:GetMapLayer()
	assert(layer, "char obj not layer:" .. self:GetName())
	return MAP_LAYER_DATA[layer]
end

function clsBaseChar:GetId()
	return self.__ID
end

function clsBaseChar:SetId(Id)
	 self.__ID = Id
	 return Id
end

function clsBaseChar:GetX()
	return self.__x
end
function clsBaseChar:SetX(x)
	self.__x = x
end
function clsBaseChar:GetY()
	return self.__y
end
function clsBaseChar:SetY(y)
	self.__y = y
end
function clsBaseChar:GetZ()
	return self.__z
end
function clsBaseChar:SetZ(z)
	self.__z = z
end
function clsBaseChar:GetGrade()
	return self.__grade or 1
end
function clsBaseChar:SetGrade(g)
	self.__grade = g
end
function clsBaseChar:GetSp()
	return self.__tmp.Sp or 0
end
function clsBaseChar:SetSp(Sp)
	self.__tmp.Sp = Sp
end
function clsBaseChar:GetXp()
	return self.__tmp.Xp or 0
end
function clsBaseChar:SetXp(Xp)
	self.__tmp.Xp = Xp
end
function clsBaseChar:GetAp()
	return self.__tmp.Ap
end
function clsBaseChar:SetAp(Ap)
	self.__tmp.Ap = Ap
end
function clsBaseChar:GetMa()
	return self.__tmp.Ma
end
function clsBaseChar:SetMa(Ma)
	self.__tmp.Ma = Ma
end
function clsBaseChar:GetDp()
	return self.__tmp.Dp
end
function clsBaseChar:SetDp(Dp)
	self.__tmp.Dp = Dp
end
function clsBaseChar:GetMr()
	return self.__tmp.Mr
end
function clsBaseChar:SetMr(Mr)
	self.__tmp.Mr = Mr
end
function clsBaseChar:GetSpeed()
	return self.__tmp.Speed
end
function clsBaseChar:SetSpeed(Speed)
	self.__tmp.Speed = Speed
end
function clsBaseChar:GetDouble()
	return self.__tmp.Double
end
function clsBaseChar:SetDouble(Double)
	self.__tmp.Double = Double
end
function clsBaseChar:GetTenacity()
	return self.__tmp.Tenacity
end
function clsBaseChar:SetTenacity(Tenacity)
	self.__tmp.Tenacity = Tenacity
end
function clsBaseChar:GetParry()
	return self.__tmp.Parry
end
function clsBaseChar:SetParry(Parry)
	self.__tmp.Parry = Parry
end
function clsBaseChar:GetReParry()
	return self.__tmp.ReParry
end
function clsBaseChar:SetReParry(ReParry)
	self.__tmp.ReParry = ReParry
end
function clsBaseChar:GetHitRate()
	return self.__tmp.HitRate
end
function clsBaseChar:SetHitRate(HitRate)
	self.__tmp.HitRate = HitRate
end
function clsBaseChar:GetDodge()
	return self.__tmp.Dodge
end
function clsBaseChar:SetDodge(Dodge)
	self.__tmp.Dodge = Dodge
end
function clsBaseChar:GetDoubleHurt()
	return self.__tmp.DoubleHurt
end
function clsBaseChar:SetDoubleHurt(DoubleHurt)
	self.__tmp.DoubleHurt = DoubleHurt
end
function clsBaseChar:GetReDoubleHurt()
	return self.__tmp.ReDoubleHurt
end
function clsBaseChar:SetReDoubleHurt(ReDoubleHurt)
	self.__tmp.ReDoubleHurt = ReDoubleHurt
end
function clsBaseChar:GetHurt()
	return self.__tmp.Hurt
end
function clsBaseChar:SetHurt(Hurt)
	self.__tmp.Hurt = Hurt
end
function clsBaseChar:GetReHurt()
	return self.__tmp.ReHurt
end
function clsBaseChar:SetReHurt(ReHurt)
	self.__tmp.ReHurt = ReHurt
end

function clsBaseChar:GetCharNo()
	return self.__char_no or 0
end
function clsBaseChar:SetCharNo(charNo)
	self.__char_no = charNo
end
function clsBaseChar:GetScore()
	return self.__tmp.Score or 1
end
function clsBaseChar:SetScore(score)
	self.__tmp.Score = score
end
function clsBaseChar:GetRetHp()
	return self.__tmp.RetHp
end
function clsBaseChar:SetRetHp(retHp)
	self.__tmp.RetHp = retHp
end
function clsBaseChar:GetRetDie()
	return self.__tmp.RetDie
end
function clsBaseChar:SetRetDie(retDie)
	self.__tmp.RetDie = retDie
end
function clsBaseChar:GetAtkTime()
	return self.__tmp.AtkTime or (AI_ATTACK_TIME * (1000 * lua_time_sec))
end
function clsBaseChar:SetAtkTime(atkTIme)
	self.__tmp.AtkTime = atkTIme
end
function clsBaseChar:GetAIRadius()
	return self.__tmp.AIRadius
end
function clsBaseChar:SetAIRadius(radius)
	if IsServer() then
		if radius > MIN_GRID_XY then			--因为需要引擎预判
			radius = MIN_GRID_XY
		end
	end
	self.__tmp.AIRadius = radius
end
function clsBaseChar:GetSyncHpNo()
	return self.__tmp.SyncHpNo
end
function clsBaseChar:SetSyncHpNo(syncNo)
	self.__tmp.SyncHpNo = syncNo
end
function clsBaseChar:GetLastAttack()
	local lastAttack = self.__tmp.LastAttack
	if not lastAttack then
		lastAttack = {}
		self.__tmp.LastAttack = lastAttack
	end
	return lastAttack
end
function clsBaseChar:SetLastAttack(attId)
	local lastAttack = self:GetLastAttack()
	lastAttack.attId = attId
	lastAttack.attTime = GetNowTimeNo()
end

function clsBaseChar:GetLastHurt()
	return self:GetTmp("LastHurt") or 1
end
function clsBaseChar:SetLastHurt(hurt)
	self:SetTmp("LastHurt", hurt)
end

function clsBaseChar:GetHpstamp()
	local hpstamp = self._hpstamp or 0
	hpstamp = hpstamp + 1
	self._hpstamp = hpstamp
	return hpstamp
end

function clsBaseChar:GetHp()
	return self.__hp or 0
end
function clsBaseChar:SetHp(hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	if not stype then
		stype = HURT_TYPE_00
	end
	local thp = self.__hp or 0
	if thp == hp then return end					--相同的血量返回
	
	if thp > 0 and hp <= 0 then
		--如果有变身buff的处理
		local buffId, shapeshift = self:GetShapeshift()
		if buffId and shapeshift then
			self.__hp = hp
			local AttObj = CHAR_MGR.GetCharById(attId)
			if AttObj and (AttObj:IsPartner() or AttObj:IsMagic()) then
				attId = AttObj:GetOwnerId()
			end
			if AttObj then
				self:SetLastAttack(attId)
			end
			if FIGHT_EVENT.DelBuff(self, buffId) then
				return
			end
		end
		
		local canDoPassDie = true
		local lastSkillId = self:GetTmp("LastHitSkillId")
		if lastSkillId then
			-- 华山论剑, 自爆不执行不死
			if lastSkillId == 40000301 or lastSkillId == 40000381 then
				canDoPassDie = false
			end
		end
		local notDie = nil
		if canDoPassDie then
			local tarObj = attId and CHAR_MGR.GetCharById(attId)	
			FIGHT_EVENT.ProcessPassMessage(PASS_DIE, self, tarObj, nil)				--触发被动技能
			notDie = self:GetFightValue(mFIGHT_NotDie)
			FIGHT_EVENT.ClearFightEff(self, PASS_DIE)
		end
		
		if notDie then
			notDie = mceil(notDie)
			hp = notDie
			if thp == hp then return end
		else
			if self:IsNpc() then
				FIGHT_EVENT.DelAllBuff(self)
				DestroyOneCharObjDelay(self)
				if self:IsNpc() then
					CHECKEND.SetLastDieNpcPos(self:GetX(), self:GetY())
				end
			elseif self:IsPlayer() then			--虚拟的
				if self:IsMirrorPlayer() then
					FIGHT_EVENT.DelAllBuff(self)
					if self:GetCanRelive() then
						ReliveOneCharObjDelay(self)
					else
						DestroyOneCharObjDelay(self)
					end
				else
					FIGHT_EVENT.DelAllFightBuff(self)
				end
			end
		end
	end
	
	self.__hp = hp	
	if thp > 0 and hp <= 0 then
		if self:IsNpc() then
			CHECKEND.AddDieNpcCnt(self)
		end
	end	
	
	if self:GetEngineObj() then
		local AttObj = CHAR_MGR.GetCharById(attId)
		if not notSync then
			local nHp = self:GetHp() or 0
			local sHpstamp = nil
			if stype == HURT_TYPE_11 then
				nHp = (self:FixedShieldBuffSyncHp() or 0)
				sHpstamp = self:GetHpstamp()
			end
			local protoMsg = {
				att_fid = AttObj and AttObj:GetFId() or 0,
				fid = self:GetFId(),
				hp = hp - thp,
				nhp = nHp,
				hp_max = self:GetFightValue(mFIGHT_HpMax) or 0,
				type = stype,
				isdie = self:IsDie() and 1 or 0,
				htype = AttObj and AttObj:GetHitType() or nil,
				die_x = self:IsDie() and self:GetX() or nil,
				die_y = self:IsDie() and self:GetY() or nil,
				shpstamp = sHpstamp,
			}
			local playerTbl = self:GetNearByPlayers()
			if playerTbl then
				local vfds = {}
				for _, pCharId in pairs(playerTbl) do
					local pCharObj = CHAR_MGR.GetCharById(pCharId)
					if pCharObj then
						tinsert(vfds, pCharObj:GetVfd())
					end
				end
				if self:IsPlayer() then
					tinsert(vfds, self:GetVfd())		--把自己添加进去
					pbc_send_msg(vfds, "S2c_aoi_hp", protoMsg)
				elseif self:IsNpc() then
					if #vfds > 0 then
						pbc_send_msg(vfds, "S2c_aoi_hp", protoMsg)	
					end
				end	
			else
				if self:IsPlayer() then		--把自己添加进去
					pbc_send_msg(self:GetVfd(), "S2c_aoi_hp", protoMsg)	
				end
			end			
		end
		
		local oAttId = attId
		if AttObj and (AttObj:IsPartner() or AttObj:IsMagic()) then
			attId = AttObj:GetOwnerId()
		end
		
		if AttObj then
			self:SetLastAttack(attId)
		end
		
		laoi.obj_sethp(self:GetEngineObj(), hp)		--一定要调用不然会影响获取附近的玩家(hp <= 0 的不会获取)
		if self:GetRetHp() and not isNotRetHp then
			lretmap.hp(self:GetId(), MAP_ID, self:GetMapLayer(), hp, lserialize.lua_seri_str({
				addHp = hp - thp, 
				attId = attId or 0,
				oAttId = oAttId,
			}))
		end
		if (self:GetRetDie() or self:IsPlayer()) and ((hp <= 0 and thp > 0) or (hp <= 0 and self.IsInInit))then
			if AttObj then
				lretmap.die(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
					attname = AttObj:GetName(),
					attid = attId,
					oAttId = oAttId,
					score = AttObj:GetScore(),
				}))
			else
				lretmap.die(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
					attname = "",
					attid = attId,
					oAttId = attId,
					score = 0,
				}))
			end
		end
	end
	
	if not isNotSyncHp then
		SyncAllHp(self, notSync, stype)			--同步血量集
		
		if not self.IsInInit then
			if thp <= 0 and hp > 0 then			--复活
				FIGHT_EVENT.ProcessPassMessage(PASS_RELIVE, self, nil, nil)	--触发被动技能
				FIGHT_EVENT.ClearFightEff(self, PASS_RELIVE)
			end
		end
	end
	
	if not self.IsInInit then
		FIGHT_EVENT.ProcessPassMessage(PASS_HP, self, nil, nil)				--触发被动技能
		FIGHT_EVENT.ClearFightEff(self, PASS_HP)
		
		FIGHT_EVENT.CheckClearStillFightEff(self)
	end
end

function clsBaseChar:SubHp(subHp, attId, notSync, stype)--subHp为正数
	subHp = mfloor(subHp)
	if subHp > 0 then			
		if self:IsInvincible() then	--无敌中不扣血
			return
		end
		self:SetLastHurt(subHp)
		local shieldHp, fixedHp = self:FixedShieldBuff(subHp, CHAR_MGR.GetCharById(attId))
		if shieldHp <= 0 then
			subHp = 0
			stype = HURT_TYPE_11
			SyncAllFixedShield(self, fixedHp)
		else
			shieldHp = self:ShieldBuff(subHp)
		end
		
		if shieldHp > 0 then
			subHp = mfloor(subHp - shieldHp)
			if subHp < 0 then
				subHp = 0
			end
		end
		if subHp == 0 then				--护盾吸收了
			if not notSync then
				local AttObj = CHAR_MGR.GetCharById(attId)
				local nHp = self:GetHp() or 0
				local sHpstamp = nil
				if stype == HURT_TYPE_11 then
					nHp = (self:FixedShieldBuffSyncHp() or 0)
					sHpstamp = self:GetHpstamp()
				end
				local protoMsg = {
					att_fid = AttObj and AttObj:GetFId() or 0,
					fid = self:GetFId(),
					hp = stype == HURT_TYPE_11 and fixedHp or 0,
					nhp = nHp,
					hp_max = self:GetFightValue(mFIGHT_HpMax) or 0,
					type = stype,
					isdie = self:IsDie() and 1 or 0,
					htype = AttObj and AttObj:GetHitType() or nil,
					die_x = self:IsDie() and self:GetX() or nil,
					die_y = self:IsDie() and self:GetY() or nil,
					shpstamp = sHpstamp,
				}
				local playerTbl = self:GetNearByPlayers()
				if playerTbl then
					local vfds = {}
					for _, pCharId in pairs(playerTbl) do
						local pCharObj = CHAR_MGR.GetCharById(pCharId)
						if pCharObj then
							tinsert(vfds, pCharObj:GetVfd())
						end
					end
					if self:IsPlayer() then
						tinsert(vfds, self:GetVfd())		--把自己添加进去
						pbc_send_msg(vfds, "S2c_aoi_hp", protoMsg)
					elseif self:IsNpc() then
						if #vfds > 0 then
							pbc_send_msg(vfds, "S2c_aoi_hp", protoMsg)	
						end
					end	
				else
					if self:IsPlayer() then		--把自己添加进去
						pbc_send_msg(self:GetVfd(), "S2c_aoi_hp", protoMsg)	
					end
				end			
			end
		end
	end
	local oHp = self.__hp or 0
	local hp = self.__hp or 0
	hp = hp - subHp
	if hp < 0 then hp = 0 end
	local maxHp = self:GetFightValue(mFIGHT_HpMax)
	if hp > maxHp then hp = maxHp end
	self:SetHp(hp, attId, notSync, stype)
	
	if subHp > 0 then									--添加伤害记录
		if hp <= 0 then
			subHp = oHp
		end
		CHAR_MGR.AddHurtOtherHp(attId, subHp)
		CHAR_MGR.AddBearHurtHp(self:GetId(), subHp)
	elseif subHp < 0 then								--加血
		subHp = -subHp
		if hp >= maxHp then
			subHp = maxHp - (self.__hp or 0)
		end
		CHAR_MGR.AddReHp(attId, subHp)
	end
end
function clsBaseChar:AddHp(addHp, attId, notSync, stype)		--addHp为整数
	if self:IsDie() then return end
	self:SubHp(-addHp, attId, notSync, stype)
end

function clsBaseChar:AddDieHp(addHp, attId, notSync, stype)
	self:SubHp(-addHp, attId, notSync, stype)
end

function clsBaseChar:SubSp(subSp)
	local sp = self:GetSp() or 0
	sp = sp - subSp
	if sp < 0 then sp = 0 end
	if sp > SP_MAX then sp = SP_MAX end
	self:SetSp(sp)
end
function clsBaseChar:AddSp(addSp)
	self:SubSp(-addSp)
end

function clsBaseChar:GetSaveShieldHp()
	return self._saveShieldHp or 0
end
function clsBaseChar:SetSaveShieldHp(shieldHp)
	self._saveShieldHp = shieldHp
end

function clsBaseChar:GetHpMax()
	return self.__hpMax or 0
end
function clsBaseChar:SetHpMax(hpMax)
	self.__hpMax = hpMax
end
function clsBaseChar:GetName()
	return self.__Name or "nothing"
end
function clsBaseChar:SetName(name)
	self.__Name = name
end
function clsBaseChar:GetAI()
	return self.__AI
end
function clsBaseChar:SetAI(ai)
	self.__AI = ai
end
function clsBaseChar:GetDir()
	return self.__dir or 1
end
function clsBaseChar:SetDir(dir)
	self.__dir = dir
end
function clsBaseChar:GetBattleRadius()
	return self.__bradius or 1
end
function clsBaseChar:SetBattleRadius(radius)
	self.__bradius = radius
end
function clsBaseChar:GetMinBattleRadius()
	return self.__mbradius or 1
end
function clsBaseChar:SetMinBattleRadius(radius)
	self.__mbradius = radius
end
function clsBaseChar:GetBuffInfo()
	return self.__buffinfo
end
function clsBaseChar:SetBuffInfo(buffInfo)
	self.__buffinfo = buffInfo
end
function clsBaseChar:GetBuffDataById(buffId)
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	return buffInfo[buffId]
end
function clsBaseChar:GetTaskBuffNo()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	for _id, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE10 then
			return _id
		end
	end
end
function clsBaseChar:HasBuffType(buffType)
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	for _id, _data in pairs(buffInfo) do
		if _data.buffType == buffType then
			return true
		end
	end	
end
--是否中了眩晕之类的buff
function clsBaseChar:IsDizziness()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
--	if buffInfo[BUFF_DIZZINESS] or buffInfo[BUFF_BIND] or buffInfo[BUFF_FREEZE] or buffInfo[BUFF_PETRIFACTION] then 
--		return true 
--	end
	--直接判断控制类型
	for _id, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE1 then
			return true
		end
	end
end
--是否用了变身BUFF
function clsBaseChar:IsShapeshift()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end

	for _id, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE9 then
			return true
		end
	end	
end
--获取变身BUFF
function clsBaseChar:GetShapeshift()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end

	for _id, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE9 then
			return _id, _data
		end
	end		
end
--是否中了变羊
function clsBaseChar:IsChangeSheep()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end

	for _id, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE6 then
			return true
		end
	end
end
function clsBaseChar:CanUserSkillByBuff(skillId)
	if self:IsDizziness() or self:IsChangeSheep() then 		--有控制类
		local skillTbl = self:GetFightValue(mFIGHT_ControlCUseSkill)
		if type(skillTbl) == mTABLETYPE then
			for _, _skillId in pairs(skillTbl) do
				if skillId == _skillId then
					return true
				end
			end
		end
		return false
	end
	return true
end
--是否隐身
function clsBaseChar:IsInvisible()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	for _, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE8 then
			return true
		end
	end	
end
--是否无敌中
function clsBaseChar:IsInvincible()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
--	if buffInfo[BUFF_INVINCIBLE] then 
--		return true 
--	end
	for _, _data in pairs(buffInfo) do
		if _data.buffType == BUFF_TYPE7 or _data.buffType == BUFF_TYPE8 then
			return true
		end
	end
end
--是否有免疫控制类
function clsBaseChar:IsIMMCONTROL()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	if buffInfo[BUFF_IMM_CONTROL] or buffInfo[BUFF_IMM_CONTROL2] then 
		return true 
	end	
end
--是否有免疫debuff类
function clsBaseChar:IsIMMDEBUFF()
	local buffInfo = self:GetBuffInfo()
	if not buffInfo then return end
	if buffInfo[BUFF_IMM_DEBUFF] or buffInfo[BUFF_IMM_DEBUFF2] then 
		return true 
	end	
end
--护盾 (subValue 为正数)
function clsBaseChar:ShieldBuff(subValue)
	local nowSubValue = 0
	local buffInfo = self:GetBuffInfo() or {}
	local delBuffTbl = nil
	for _id, _data in pairs(buffInfo) do
		if _data[mFIGHT_Shield] and _data[mFIGHT_Shield] >= 0 then
			if _data[mFIGHT_Shield] > subValue then
				nowSubValue = nowSubValue + subValue
				_data[mFIGHT_Shield] = _data[mFIGHT_Shield] - subValue
				break
			else
				nowSubValue = nowSubValue + _data[mFIGHT_Shield]
				subValue = subValue - _data[mFIGHT_Shield]
				_data[mFIGHT_Shield] = 0
				
				delBuffTbl = delBuffTbl or {}
				tinsert(delBuffTbl, _id)
			end
			if subValue <= 0 then
				break
			end
		end
	end
	if delBuffTbl then
		for _, _buffId in pairs(delBuffTbl) do
			FIGHT_EVENT.DelBuff(self, _buffId)
		end
	end
	return nowSubValue
end

function clsBaseChar:FixedShieldBuffSyncHp()
	local buffInfo = self:GetBuffInfo()
	if buffInfo then		--是数值
		for _id, _data in pairs(buffInfo) do
			if _data[mFIGHT_FixedShieldHpMax] then
				return _data[mFIGHT_FixedShield], _data[mFIGHT_FixedShieldHpMax], _id
			end
		end
	end
end
--固定扣血型护盾
function clsBaseChar:FixedShieldBuff(subValue, AttObj, syncFixedHp)
	local nowSubValue = 0
	local buffInfo = self:GetBuffInfo() or {}
	local delBuffTbl = nil
	
	
	local SubShieldHp = self:GetFightValue(mFIGHT_FixedShieldSubHurt) or 0
	if SubShieldHp <= 0 then return subValue end
	local AddShieldHp = 0
	if AttObj then
		AddShieldHp = AttObj:GetFightValue(mFIGHT_FixedShieldAddHurt) or 0
	end
	
	for _id, _data in pairs(buffInfo) do
		if _data[mFIGHT_FixedShield] and _data[mFIGHT_FixedShield] > 0 then
			if syncFixedHp then
				_data[mFIGHT_FixedShield] = _data[mFIGHT_FixedShield] - syncFixedHp
			else
				_data[mFIGHT_FixedShield] = _data[mFIGHT_FixedShield] - SubShieldHp - AddShieldHp
				syncFixedHp = SubShieldHp + AddShieldHp
			end
			if _data[mFIGHT_FixedShield] <= 0 then
				delBuffTbl = delBuffTbl or {}
				tinsert(delBuffTbl, _id)				
			end
			break
		end
	end
	
	if IsKuaFuServer and self:IsNpc() and self:GetRetHp() then
		local mapLayer = self:GetMapLayer()
		if mapLayer == 1 then
			local sHp, sHpMax, sBuffId = self:FixedShieldBuffSyncHp()
			lretmap.other(self:GetId(), MAP_ID, mapLayer, lserialize.lua_seri_str({		--发送护盾buff创建
				type = RETMAP_SHIELDBUFF,
				iType = "change",
				sHp = sHp,
				sHpMax = sHpMax,
			}))
		end
	end
	
	if delBuffTbl then
		for _, _buffId in pairs(delBuffTbl) do
			FIGHT_EVENT.DelBuff(self, _buffId)
		end
	end
	return nowSubValue, syncFixedHp
end
--获取分线需要同步的buff
function clsBaseChar:GetSyncBuffData()
	local ret = {}
	local buffInfo = self:GetBuffInfo() or {}
	for _id, _data in pairs(buffInfo) do
		if _data[mFIGHT_FixedShield] and _data[mFIGHT_FixedShield] > 0 then
			ret[_id] = _data
		end
	end
	return ret
end
--调整时间
function clsBaseChar:AdjustSpeed()
	local speed = self:GetSpeed()
	if speed <= SPEED_SLOW then
		speed = SPEED_SLOW
	elseif speed <= SPEED_NORMAL then
		speed = SPEED_NORMAL
	else
		speed = SPEED_FAST
	end
	self:SetSpeed(speed)
end

function clsBaseChar:GetComp()
	return self.__comp
end
function clsBaseChar:SetComp(comp, notSync)
	self.__comp = comp
	laoi.obj_setcomp(self:GetEngineObj(), comp)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.comp = comp
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "comp",
			value = comp,
		})
	end
end
function clsBaseChar:GetBuff()
	return self.__buff or NORMAL_TBL
end
function clsBaseChar:SetBuff(buff, notSync)
	if not buff then
		buff = NORMAL_TBL
	end
	self.__buff = buff
	local syncData = self:GetSyncData()
	if syncData then
		syncData.buff = buff
	end
--	self:SetSyncData(syncData, notSync)
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_repeatint", {
			fid = self:GetFId(),
			key = "buff",
			value = buff,
		})
	end
end
function clsBaseChar:GetBuffTime()
	return self.__bufft
end
function clsBaseChar:SetBuffTime(buffTime)
	self.__bufft = buffTime
end
function clsBaseChar:AddBuffTime(buffId, sFrame, cFrame)
	local buffTime = self:GetBuffTime()
	if not buffTime then
		buffTime = {}
		self:SetBuffTime(buffTime)
	end
	buffTime[buffId] = {
		buffid = buffId,
		start_frame = sFrame,
		con_frame = cFrame,
	}
	
	--广播一下
	self:SyncNearByPlayer("S2c_aoi_buff_time", self:GetBuffTimeProto())
end
function clsBaseChar:DelBuffTime(buffId)
	local buffTime = self:GetBuffTime()
	if not buffTime then return end
	if buffTime[buffId] then
		buffTime[buffId] = nil
		if tsize(buffTime) <= 0 then
			self:SetBuffTime(nil)
		end
	end
	--删除不用广播，因为会消除了
end

local SECOND_TIME_CNT = GetSecondTimeCnt()
function clsBaseChar:GetBuffTimeProto()
	local buffTime = self:GetBuffTime()
	if not buffTime then return end
	
	local binfo = {}
	local protoMsg = {
		fid = self:GetFId(),
		s_frame = GetNowTimeNo(),
		sec_frame = SECOND_TIME_CNT,
		binfo = binfo,
	}
	for _, _data in pairs(buffTime) do
		tinsert(binfo, _data)
	end
	return protoMsg
end

function clsBaseChar:GetSyncData()
	local syncData = self.__sync_data
	if syncData then
		if self:GetBuffTime() then
			syncData.btime = self:GetBuffTimeProto()
		end
	end
	return syncData
end
function clsBaseChar:SetSyncData(Value, notSync)
	if not Value.comp then
		Value.comp = self:GetComp()
	end
	if not Value.buff then
		Value.buff = self:GetBuff()
	end
	local oSyncData = self:GetSyncData() or {}
	table.merge(oSyncData, Value)
	self.__sync_data = oSyncData
	if not notSync then
		local playerTbl = self:GetNearByPlayers()
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if self:IsPlayer() then
				tinsert(vfds, self:GetVfd())		--把自己添加进去
				pbc_send_msg(vfds, "S2c_aoi_syncplayer", oSyncData)	
			elseif self:IsNpc() then
				if #vfds > 0 then
					pbc_send_msg(vfds, "S2c_aoi_syncnpc", oSyncData)	
				end
			end	
		else
			if self:IsPlayer() then		--把自己添加进去
				pbc_send_msg(self:GetVfd(), "S2c_aoi_syncplayer", oSyncData)	
			end
		end
	end
end

function clsBaseChar:GetProfession()
	return self.__profess
end
function clsBaseChar:SetProfession(profession)
	self.__profess = profession
end

function clsBaseChar:GetEngineObj()		--不能保存在别的地方,只能获取
	return self.__engineobj
end

function clsBaseChar:AddMap()			--添加进入地图后的处理
	assert("must override AddMap")
end

function clsBaseChar:Move(speed, dir)	--添加移动后的处理
	assert("must override Move")
end

function clsBaseChar:MoveTo(nx, ny)		--添加移动后的处理
	assert("must override MoveTo")
end

function clsBaseChar:JumpTo(nx, ny)		--添加跳转函数
	assert("must override JumpTo")
end

function clsBaseChar:LeaveMap()			--添加离开地图后的处理
	assert("must override LeaveMap")
end

function clsBaseChar:CanMove()
	if self:GetHp() > 0 and (not self:IsDizziness()) then	--有血才能移动并且不在控制buff中
		return true
	end
end

function clsBaseChar:HasObjNotItemInPos(x, y)				--是否有对象在该位置(除去item对象)
	return false
end

function clsBaseChar:MoveChangeMapPos(changeType, ox, oy)	--当玩家移动之后必须调用调整的
	local isAdd, isDel = nil, nil
	if changeType == CHANGE_MAPPOS_ADD then
		isAdd = true
	elseif changeType == CHANGE_MAPPOS_MOVE and ox and oy then
		isDel = true
		isAdd = true
	elseif changeType == CHANGE_MAPPOS_LEAVE then
		ox, oy = self:GetX(), self:GetY()
		isDel = true
	else
		return
	end

	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end

	if isDel then
		local oXCharTbl = mapChar[ox]
		if oXCharTbl then
			local oXYCharTbl = oXCharTbl[oy]
			if oXYCharTbl then
				local id = self:GetId()
				if oXYCharTbl[id] then
					oXYCharTbl[id] = nil
				end
				if table.empty(oXYCharTbl) then			--清除tbl,免得地图大部分都走过后都创建了table
					oXCharTbl[oy] = nil
				end
			end
		end
	end

	if isAdd then
--		_RUNTIME("########clsBaseChar:MoveChangeMapPos add")
		local nx, ny = self:GetX(), self:GetY()
		local xCharTbl = mapChar[nx]
		if not xCharTbl then
			xCharTbl = {}
			mapChar[nx] = xCharTbl
		end
		local yCharTbl = xCharTbl[ny]
		if not yCharTbl then
			yCharTbl = setmetatable({}, WeakMetaTbl)
			xCharTbl[ny] = yCharTbl
		end
		yCharTbl[self:GetId()] = self
	end
	
--	_RUNTIME("########clsBaseChar:MoveChangeMapPos:")
--	local num = 0
--	for k, v in pairs(mapChar) do
--		for _k, _v in pairs(v) do
--			for _, obj in pairs(_v) do
--				print("     ["..k..",".._k.."]:", obj:GetId())
--				num = num + 1
--			end
--		end
--	end
--	_RUNTIME("~~~~~~~~~~~~~~~clsBaseChar:MoveChangeMapPos:", num)
end

--销毁一个对象
function clsBaseChar:Destroy(notRetMap)
	if not self:IsDestroy() then
		self:LeaveMap(notRetMap)					--有可能引擎报错，所以在前面就报错，然后返回给主逻辑退出不成功
		FIGHT_EVENT.DelAllBuff(self)
		DelSyncHp(self)
	
		Super(clsBaseChar).Destroy(self)
		local Id = self:GetId()
		CHAR_MGR.RemoveCharId(Id)
		self.__engineobj = nil
		if self.__AI then
			self.__AI:Destroy()
		end
		self.__AI = nil
		
		CHAR_MGR.ClearHurtOtherHp(self:GetId())			--清除攻击伤害
		CHAR_MGR.ClearBearHurtHp(self:GetId())			--清除被攻击伤害
		CHAR_MGR.ClearReHp(self:GetId())				--清除回血
	end
end

function clsBaseChar:IsDestroy()
	return self.__engineobj == nil
end

function clsBaseChar:IsDie()
	return self:GetHp() <= 0
end

function clsBaseChar:SetTmp(key, value)
	self.__tmp[key] = value
	return value
end
function clsBaseChar:GetTmp(key, def)
	return self.__tmp[key] or def
end

function clsBaseChar:SetPairs(varsTable)
	for k, v in pairs(varsTable) do
		self['Set'..k](self, v)
	end
end

function clsBaseChar:SetTmpSkillLv(skillId, lv)
	if not self.__tmp.TmpSkillLv then
		self.__tmp.TmpSkillLv = {}
	end
	self.__tmp.TmpSkillLv[skillId] = lv
end
function clsBaseChar:GetTmpSkillLv(skillId)
	return self.__tmp.TmpSkillLv and self.__tmp.TmpSkillLv[skillId]
end

function clsBaseChar:GetMartialLevelBySkillId(skillId)
	local allSkill = self:GetAllSkill()
	local oneData = allSkill[skillId]
	if oneData then
		return oneData.Lv or 1
	else
		return self:GetTmpSkillLv(skillId) or 1
	end
end

function clsBaseChar:GetMartialLevel(MartialId)
	local MartialTable = self.__tmp.MartialTable or {}
	for _,_martial in pairs(MartialTable) do
		if _martial.MartialId == MartialId then
			return _martial.Lv
		end
	end
end

function clsBaseChar:IsSkillNormal(skillId)
	local oneData = self:GetOneSkillData(skillId)
	if not oneData then return end
	return oneData.Mtype == SKILL_MTYPE_NORMAL
end
function clsBaseChar:ClearOneSkillCD(skillId)
	local allSkill = self:GetAllSkill()
	local UserObj = nil
	if self:IsPlayer() then
		UserObj = self
	end
	if not allSkill[skillId] then return end
	allSkill[skillId].CDEndTimeNo = 0
	if allSkill[skillId].Mtype == SKILL_MTYPE_MAGIC then
		if UserObj then
			pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skill_ctime", {fid = self:GetFId(), skill_id = skillId, ctime = 0})
		end
	end	
end
function clsBaseChar:ClearAllSkillCD()
	local allSkill = self:GetAllSkill()
	local UserObj = nil
	if self:IsPlayer() then
		UserObj = self
	end
	for _skillId, _data in pairs(allSkill) do
		_data.CDEndTimeNo = 0
		if _data.Mtype == SKILL_MTYPE_MAGIC then
			if UserObj then
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skill_ctime", {fid = self:GetFId(), skill_id = _skillId, ctime = 0})
			end
		end
	end
end

function clsBaseChar:AddMartial(martialId, lv, isCd, isAidMartial)
	local martialTbl = self:GetMartialTable()
	if self:HasMartial(martialId) then return end
	tinsert(martialTbl, {
		MartialId = martialId,
		Lv = lv,
	})
	
	local SKILL_PROPDATA = SKILL_DATA.GetAllSkillPropData()
	local battleRadius = self:GetBattleRadius() or NORMAL_NPC_RADIUS
	local allSkill = self:GetAllSkill() or {}
	local martialData = SKILL_DATA.GetMartial(martialId)
	if martialData and martialData.SkillList then
		local nTimeNo = GetNowTimeNo()
		for _, _skillId in ipairs(martialData.SkillList) do
			local skillData = SKILL_DATA.GetMartialSkill(_skillId)
			if skillData then
				if lv > 0 then
					if allSkill[_skillId] then
						allSkill[_skillId].Lv = lv
						if martialData.Type == SKILL_TYPE_PASSIVE then
							local propData = SKILL_PROPDATA[_skillId]
							if propData then 
								for _, _onePropData in pairs(propData) do
									self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
								end 
							end
						end
					else
						local AttRange = skillData.AttRange or 2.9
						AttRange = math.ceil(AttRange)
						allSkill[_skillId] = {
							Lv = lv,
							IsSubSp = skillData.IsSubSp,
							MartialId = skillData.MartialId,
							BeHitMax = skillData.BeHitMax,
							Type = martialData.Type,			--1:主动,2:被动
							Mtype = martialData.Mtype,			--1:普通攻击,2:技能,3:合体技
							AttKind = skillData.AttKind,
							AttKind2 = skillData.AttKind2,
							AttRange = AttRange,
							CD = mfloor(skillData.CD / (1000 * GetTimeSec())),
							CDEndTimeNo = nTimeNo,
							SkillTime = skillData.SkillTime and mceil(skillData.SkillTime / (1000 * GetTimeSec())) or AI_ATTACK_TIME,
							FrontSkill = skillData.FrontSkill,
							CreateCoolTime = skillData.CreateCoolTime and mfloor(skillData.CreateCoolTime / (1000 * GetTimeSec()) + nTimeNo),
							AidSkill = isAidMartial,
							
							--提示攻击信息
							Tips = skillData.BeforeTips,
							IsWait = nil,
							x = nil,
							y = nil,
							TipsId = nil,
							PassTimeNo = nil,
						}
						if isCd then
							allSkill[_skillId].CDEndTimeNo = allSkill[_skillId].CDEndTimeNo + allSkill[_skillId].CD
							if self:IsPlayer() then
								local ctime = allSkill[_skillId].CD * 1000 * lua_time_sec
								pbc_send_msg(self:GetVfd(), "S2c_aoi_skill_ctime", {fid = self:GetFId(), skill_id = _skillId, ctime = ctime})
							end
						end
						
						if martialData.Type == SKILL_TYPE_INITIATIVE then
							--去最小的攻击距离
							if skillData.AttRange < battleRadius then
								battleRadius = skillData.AttRange
							end
						elseif martialData.Type == SKILL_TYPE_PASSIVE then
							local propData = SKILL_PROPDATA[_skillId]
							if propData then 
								for _, _onePropData in pairs(propData) do
									self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
								end 
							end
						end
					end
				end
			end
		end
	end
	self:SetAllSkill(allSkill)
	if not table.empty(allSkill) then
		if battleRadius < 2.9 then
			battleRadius = 2.9
		end
		self:SetBattleRadius(battleRadius)
	else
		self:SetBattleRadius(2.9)
	end
end

function clsBaseChar:DelMartial(martialId)					--只能被动技能
	if not self:HasMartial(martialId) then return end
	local oriMData = SKILL_DATA.GetMartial(martialId)
	assert(oriMData.Type == SKILL_TYPE_PASSIVE)
	local martialTbl = self:GetMartialTable()
	local removeIdx = nil
	for _idx, _data in pairs(martialTbl) do
		if _data.MartialId == martialId then
			removeIdx = _idx
			break
		end
	end
	if removeIdx then
		tremove(martialTbl, removeIdx)
	end
	local SKILL_PROPDATA = SKILL_DATA.GetAllSkillPropData()
	if oriMData and oriMData.SkillList then
		for _, _skillId in ipairs(oriMData.SkillList) do
			local propData = SKILL_PROPDATA[_skillId]
			if propData then 
				for _, _onePropData in pairs(propData) do
					self:SetPassSkillByWhen(_skillId, _onePropData.When, nil)
				end 
			end		
		end	
	end	
end

function clsBaseChar:ChangeMartialLevel(martialId, lv)		--武学等级变换
	local martialTbl = self:GetMartialTable()
	if not self:HasMartial(martialId) then return end
	local martialData = SKILL_DATA.GetMartial(martialId)
	tinsert(martialTbl, {
		MartialId = martialId,
		Lv = lv,
	})
	if martialData and martialData.SkillList then
		local allSkill = self:GetAllSkill() or {}
		for _, _skillId in ipairs(martialData.SkillList) do
			if allSkill[_skillId] then
				allSkill[_skillId].Lv = lv	
			end
		end
	end
end
function clsBaseChar:ChangeMartial(oriM, toM)				--转换武学
	local oriMData
	if oriM then
		if not self:HasMartial(oriM.MartialId) then return end
		oriMData = SKILL_DATA.GetMartial(oriM.MartialId)
		assert(oriMData.Type == SKILL_TYPE_PASSIVE)
	end
	local allSkill = self:GetAllSkill() or {}
	local toMData = SKILL_DATA.GetMartial(toM.MartialId)
	assert(toMData.Type == SKILL_TYPE_PASSIVE)		--只能被动技能
	local SKILL_PROPDATA = SKILL_DATA.GetAllSkillPropData()
	
	if oriMData and oriMData.SkillList then
		for _, _skillId in ipairs(oriMData.SkillList) do
			local propData = SKILL_PROPDATA[_skillId]
			if propData then 
				for _, _onePropData in pairs(propData) do
					self:SetPassSkillByWhen(_skillId, _onePropData.When, nil)
				end 
			end		
		end	
	end
	
	if not self:HasMartial(toM.MartialId) then
		local martialTbl = self:GetMartialTable()
		tinsert(martialTbl, {
			MartialId = toM.MartialId,
			Lv = toM.Lv,
		})
	end
	
	local nTimeNo = GetNowTimeNo()
	if toMData and toMData.SkillList then
		for _, _skillId in ipairs(toMData.SkillList) do
			local skillData = SKILL_DATA.GetMartialSkill(_skillId)
			if skillData then
				if toM.Lv > 0 then
					if allSkill[_skillId] then
						local oneSkillData = allSkill[_skillId]
						oneSkillData.Lv = toM.Lv
						
						if toMData.Type == SKILL_TYPE_PASSIVE then
							local propData = SKILL_PROPDATA[_skillId]
							if propData then 
								for _, _onePropData in pairs(propData) do
									self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
								end 
							end
						end
						if oneSkillData.PassTimeNo then
							local cTime = oneSkillData.PassTimeNo + oneSkillData.CD - nTimeNo
							if cTime > 0 then
								pbc_send_msg(self:GetVfd(), "S2c_aoi_skill_ctime", {
									fid = self:GetFId(), 
									skill_id = _skillId, 
									ctime = cTime * 1000 * lua_time_sec,
								})
							end
						end
					else
						local AttRange = skillData.AttRange or 2.9
						AttRange = math.ceil(AttRange)
						allSkill[_skillId] = {
							Lv = toM.Lv,
							IsSubSp = skillData.IsSubSp,
							MartialId = skillData.MartialId,
							BeHitMax = skillData.BeHitMax,
							Type = toMData.Type,			--1:主动,2:被动
							Mtype = toMData.Mtype,			--1:普通攻击,2:技能,3:合体技
							AttKind = skillData.AttKind,
							AttKind2 = skillData.AttKind2,
							AttRange = AttRange,
							CD = mfloor(skillData.CD / (1000 * GetTimeSec())),
							CDEndTimeNo = nTimeNo,
							SkillTime = skillData.SkillTime and mceil(skillData.SkillTime / (1000 * GetTimeSec())) or AI_ATTACK_TIME,
							FrontSkill = skillData.FrontSkill,
							CreateCoolTime = skillData.CreateCoolTime and mfloor(skillData.CreateCoolTime / (1000 * GetTimeSec()) + nTimeNo),
							
							--提示攻击信息
							Tips = skillData.BeforeTips,
							IsWait = nil,
							x = nil,
							y = nil,
							TipsId = nil,
							PassTimeNo = nil,
						}
						local propData = SKILL_PROPDATA[_skillId]
						if propData then 
							for _, _onePropData in pairs(propData) do
								self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
							end 
						end
					end
				end
			end
		end			
	end
end

function clsBaseChar:HasMartial(martialId)
	local martialTbl = self:GetMartialTable()
	for _, _data in pairs(martialTbl) do
		if _data.MartialId == martialId then
			return true
		end
	end
end
function clsBaseChar:GetMartialTable()
	return self.__tmp.MartialTable or {}
end
function clsBaseChar:ClearMartialTable()
	self.__tmp.MartialTable = {}
	self:SetAllSkill({})
	self:ClearPassSkill()
end
function clsBaseChar:SetMartialTable(MartialTable)
	self.__tmp.MartialTable = MartialTable
	local nTimeNo = GetNowTimeNo()
	
	local SKILL_PROPDATA = SKILL_DATA.GetAllSkillPropData()
	local battleRadius = NORMAL_NPC_RADIUS
	local allSkill = self:GetAllSkill() or {}
	local oAllSkill = nil
	if self:IsMagic() then
		oAllSkill = self:GetOldAllSkill()
	end
	local changeSkillTbl = {}
	for _, _data in pairs(MartialTable) do
		local martialId = _data.MartialId
		local martialData = SKILL_DATA.GetMartial(martialId)
		if martialData and martialData.SkillList then
			for _, _skillId in ipairs(martialData.SkillList) do
				local skillData = SKILL_DATA.GetMartialSkill(_skillId)
				if skillData then
					if _data.Lv > 0 then
						changeSkillTbl[_skillId] = true
						
						local CDEndTimeNo = nil
						if _data.OriCd then
							local cTime = mfloor(_data.OriCd / (1000 * GetTimeSec()))
							if cTime > 0 then
								pbc_send_msg(self:GetVfd(), "S2c_aoi_skill_ctime", {
									fid = self:GetFId(), 
									skill_id = _skillId, 
									ctime = cTime * 1000 * lua_time_sec,
								})
								CDEndTimeNo = cTime + nTimeNo
							end
						end
						
						if allSkill[_skillId] then
							allSkill[_skillId].Lv = _data.Lv
							if martialData.Type == SKILL_TYPE_PASSIVE then
								local propData = SKILL_PROPDATA[_skillId]
								if propData then 
									for _, _onePropData in pairs(propData) do
										self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
									end 
								end
							end
						else
							local AttRange = skillData.AttRange or 2.9
							AttRange = math.ceil(AttRange)
							
							if oAllSkill and oAllSkill[_skillId] then
								allSkill[_skillId] = oAllSkill[_skillId]
								allSkill[_skillId].Lv = _data.Lv
							else
								allSkill[_skillId] = {
									Lv = _data.Lv,
									IsSubSp = skillData.IsSubSp,
									MartialId = skillData.MartialId,
									BeHitMax = skillData.BeHitMax,
									Type = martialData.Type,			--1:主动,2:被动
									Mtype = martialData.Mtype,			--1:普通攻击,2:技能,3:合体技
									AttKind = skillData.AttKind,
									AttKind2 = skillData.AttKind2,
									AttRange = AttRange,
									CD = mfloor(skillData.CD / (1000 * GetTimeSec())),
									CDEndTimeNo = CDEndTimeNo or nTimeNo,
									SkillTime = skillData.SkillTime and mceil(skillData.SkillTime / (1000 * GetTimeSec())) or AI_ATTACK_TIME,
									FrontSkill = skillData.FrontSkill,
									CreateCoolTime = skillData.CreateCoolTime and mfloor(skillData.CreateCoolTime / (1000 * GetTimeSec()) + nTimeNo),
									
									--提示攻击信息
									Tips = skillData.BeforeTips,
									IsWait = nil,
									x = nil,
									y = nil,
									TipsId = nil,
									PassTimeNo = nil,
								}
							end
							
							if martialData.Type == SKILL_TYPE_INITIATIVE then
								--去最小的攻击距离
								if skillData.AttRange < battleRadius then
									battleRadius = skillData.AttRange
								end
							elseif martialData.Type == SKILL_TYPE_PASSIVE then
								local propData = SKILL_PROPDATA[_skillId]
								if propData then 
									for _, _onePropData in pairs(propData) do
										self:SetPassSkillByWhen(_skillId, _onePropData.When, true)
									end 
								end
							end
						end
					end
				end
			end
		end
	end
	
	for _skillId, _data in pairs(allSkill) do				--少了的技能要删除,并且被动技能触发机制也要删除
		if not changeSkillTbl[_skillId] then
			if _data.Type ~= SKILL_TYPE_PASSIV then
				if self:IsMagic() then
					local oAllSkill = self:GetOldAllSkill()
					oAllSkill[_skillId] = allSkill[_skillId]
					allSkill[_skillId] = nil	
				else
					if IsServer() then						--注意:客户端的不删,因为助战伙伴技能不存服务端的
						allSkill[_skillId] = nil				
					end
				end
			else
				local propData = SKILL_PROPDATA[_skillId]
				if propData then 
					for _, _onePropData in pairs(propData) do
						self:SetPassSkillByWhen(_skillId, _onePropData.When, nil)
					end 
				end
			end			
		end
	end
--	_RUNTIME("__________:", self:GetName(), self:IsPlayer(), sys.dump(allSkill))
	self:SetAllSkill(allSkill)
	if not table.empty(allSkill) then
		if battleRadius < 2.9 then
			battleRadius = 2.9
		end
		self:SetBattleRadius(battleRadius)
	else
		self:SetBattleRadius(2.9)
	end
	self:SetMinBattleRadius(2)
end
function clsBaseChar:GetPassSkillByWhen(when)
	local passSkillData = self.__tmp.AllPassSkill
	return passSkillData and passSkillData[when]
end
function clsBaseChar:ClearPassSkill()
	self.__tmp.AllPassSkill = nil
end
function clsBaseChar:SetPassSkillByWhen(skillId, when, isOk)
	local passSkillData = self.__tmp.AllPassSkill
	if not passSkillData then
		passSkillData = {}
		self.__tmp.AllPassSkill = passSkillData
	end
	passSkillData[when] = passSkillData[when] or {}
	passSkillData[when][skillId] = isOk
end
function clsBaseChar:GetAllSkill()
	return self.__tmp.AllSkill or {}
end
function clsBaseChar:SetAllSkill(allSkill)
	self.__tmp.AllSkill = allSkill
end
function clsBaseChar:SetFrontSkillId(skillId)
	self.__tmp.FrontSkill = skillId
end
function clsBaseChar:GetFrontSkillId()
	return self.__tmp.FrontSkill
end
function clsBaseChar:GetNowSkillId()												--如果没有就返回普通攻击
	if self.__nskillid then return self.__nskillid end
	local allSkill = self:GetAllSkill()
	for _skillId, _data in pairs(allSkill) do
		if _data.Mtype == SKILL_MTYPE_NORMAL and _data.Type == SKILL_TYPE_INITIATIVE then
			return _skillId
		end
	end
end
function clsBaseChar:SetNowSkillId(skillId)
	error("not Inherit")
end
function clsBaseChar:ClearNowSkillId()
	self.__nskillid = nil
end
function clsBaseChar:GetOneSkillData(skillId)
	local allSkill = self:GetAllSkill()
	return allSkill[skillId]
end

function clsBaseChar:GetAllSkillId()
	return table.keys(self:GetAllSkill() or {})
end

--获取战斗值
function clsBaseChar:GetFightValue(key)
	local value = nil
	if self["Get"..key] then
		value = self["Get"..key](self)
	end	
	local effValue = FIGHT_EVENT.GetFightEff(self, key)
	
	if not value then
		if effValue then
			if type(effValue) == mNUMBERTYPE then
				value = effValue
			else
				return effValue
			end
		end
		
		local buffInfo = self:GetBuffInfo() or {}
		if value then		--是数值
			for _id, _data in pairs(buffInfo) do
				value = value + (_data[key] or 0)
			end
			return value
		else
			if key == mFIGHT_ControlCUseSkill then
				local retTbl = {}
				for _id, _data in pairs(buffInfo) do
					if _data[key] then
						tinsert(retTbl, _data[key])
					end
				end					
				return retTbl
			else
				for _id, _data in pairs(buffInfo) do
					if _data[key] then
						if type(value) == mNUMBERTYPE then
							value = value + _data[key]
						else
							if type(_data[key]) == mNUMBERTYPE then
								value = _data[key]
							else
								return _data[key]
							end
						end
					end
				end	
				if value then
					return value
				end
			end		
		end
	end
	
	if type(value) == mNUMBERTYPE and key ~= mFIGHT_Hp then
		local buffValue = 0
		local buffInfo = self:GetBuffInfo() or {}
		for _id, _data in pairs(buffInfo) do
			buffValue = buffValue + (_data[key] or 0)
		end
		return value + (effValue or 0) + buffValue
	end
	
	return value
end

---------------------------------------------ai使用的函数 start-----------------------------------------------
function clsBaseChar:FindCharObjByRId(charId)	--整张地图搜索玩家坐标，(没有则返回nil)
	if not charId then return end
	local charObj = CHAR_MGR.GetCharById(charId)
	if not charObj then return end
	if self:GetMapLayer() ~= charObj:GetMapLayer() then return end		--要判断同一层
	return charObj
end

function clsBaseChar:FindCharObjPosByRId(charId)	--整张地图搜索玩家坐标，(没有则返回nil,nil)
	if not charId then return end
	local charObj = CHAR_MGR.GetCharById(charId)
	if not charObj then return end
	--判断是否同一层
	if self:GetMapLayer() ~= charObj:GetMapLayer() then return end
	return charObj:GetX(), charObj:GetY()
end

--获取周围的人
function clsBaseChar:GetNearByPlayers()
	local isOk, playerTbl = laoi.map_region9player(self:GetMapObj(), self:GetEngineObj())
	if isOk and playerTbl then
		return playerTbl
	end
end

--获取周围npc
function clsBaseChar:GetNearByNpc()
	local isOk, npcTbl = laoi.map_region9npc(self:GetMapObj(), self:GetEngineObj())
	if isOk and npcTbl then
		return npcTbl
	end	
end

--搜索周围附近的玩家(除去excludeCharId对象)
function clsBaseChar:SearchNearbyPlayer(radius, excludeCharId)
	if not radius then return end
	local isOk, playerId = laoi.map_nearbyplayer(self:GetMapObj(), self:GetEngineObj(), radius, excludeCharId or 0)
	if isOk and playerId then
		return playerId
	end
end

--搜索周围附近的同伴(除去excludeCharId对象)
function clsBaseChar:SearchNearbyPartner(radius, excludeCharId)
	if not radius then return end
	local isOk, partnerId = laoi.map_nearbypartner(self:GetMapObj(), self:GetEngineObj(), radius, excludeCharId or 0)
	if isOk and partnerId then
		return partnerId
	end
end

function clsBaseChar:CanAttackState(tarObj)
	local attObj = nil
	if self:IsPlayer() then
		attObj = self
	elseif self:IsMagic() or self:IsPartner() then
		attObj = self:GetOwner() 
	end
	if attObj then
		local cantAttackList = attObj:GetCantAttackList()
		if cantAttackList and cantAttackList[tarObj:GetId()] then		--不能攻击
			return
		end
	end
	return true
end

local CHECK_HOSTILECLUB_MODE = {
	[PKMODE_HOSTILECLUB] = true,
	[PKMODE_WHOLE] = true,
	[PKMODE_TEAM] = true,
	[PKMODE_CLUB] = true,
}

function clsBaseChar:SecurityAreaCheckAttack(tarObj)		--如果2者都是玩家才需要判断,tarObj是被攻击者.坐标点应该去被攻击者的来判断
	if IS_FIGHT_MAP then			--战斗场景的再判断一下是否有安全区域
		if not SECURITY_AREAS then return true end
		local isANpc = self:IsNpc()
		local isTNpc = tarObj:IsNpc()
		if isANpc or isTNpc then return true end
		
		if self:IsPlayer() then									--同队的可以打，因为是有益的
			if tarObj:IsPartner() or tarObj:IsMagic() then
				if tarObj:GetOwner() == self then
					return true
				end
			end
		elseif self:IsPartner() or self:IsMagic() then
			if tarObj:IsPlayer() then
				if self:GetOwner() == tarObj then
					return true
				end
			end
		end
		
		local inSecurity = nil
		local x, y = tarObj:GetX(), tarObj:GetY()
		local sx, sy = self:GetX(), self:GetY()
		for _i, _area in pairs(SECURITY_AREAS) do
			local pos = _area.pos
			local r = _area.r
			local absx = mabs(x - pos[1])
			local absy = mabs(y - pos[2])
			if absx <= r and absy <= r then 
				if absx ^ 2 + absy ^ 2 <= r ^ 2 then
					inSecurity = true
					break
				end
			end
			
			local sabsx = mabs(sx - pos[1])
			local sabsy = mabs(sy - pos[2])
			if sabsx <= r and sabsy <= r then 
				if sabsx ^ 2 + sabsy ^ 2 <= r ^ 2 then
					inSecurity = true
					break
				end
			end
		end
		return not inSecurity
	else
		local isANpc = self:IsNpc()
		local isTNpc = tarObj:IsNpc()
		if isANpc or isTNpc then return true end
		
		if self:IsPlayer() and tarObj:IsPlayer() then	
			local sMode = self:GetPkMode()
			local isDibang,isEnemy -- 是否敌帮，是否仇人

			if FLAG and CHECK_HOSTILECLUB_MODE[sMode] then			--同阵营的，并且是敌对帮派，安全区都能打
				local hostileClub = self:GetHostileClub()
				local tarHostileClub = tarObj:GetHostileClub()
				if (hostileClub and hostileClub ~= "" and hostileClub == tarObj:GetClubId()) or 
					(tarHostileClub and tarHostileClub ~= "" and tarHostileClub == self:GetClubId()) then
						isDibang = true 
				end
			end 

			local enemyList = self:GetEnemyList() or {}
			if PK_ENEMY and sMode ~= PKMODE_PEACE and enemyList[tarObj:GetId()] then				
				isEnemy = true 				
			end	
			if isDibang or isEnemy then
				if SECURITY_AREAS then
					local sx, sy = self:GetX(), self:GetY()
					local inSecurity = nil
					local x, y = tarObj:GetX(), tarObj:GetY()
					for _i, _area in pairs(SECURITY_AREAS) do
						local pos = _area.pos
						local r = _area.r
						local absx = mabs(x - pos[1])
						local absy = mabs(y - pos[2])
						if absx <= r and absy <= r then 
							if absx ^ 2 + absy ^ 2 <= r ^ 2 then
								inSecurity = true
								break
							end
						end
						
						local sabsx = mabs(sx - pos[1])
						local sabsy = mabs(sy - pos[2])
						if sabsx <= r and sabsy <= r then 
							if sabsx ^ 2 + sabsy ^ 2 <= r ^ 2 then
								inSecurity = true
								break
							end
						end
					end
					return not inSecurity
				else
					return true
				end
			end
		end

		if self:IsPlayer() then									--同队的可以打，因为是有益的
			if tarObj:IsPartner() or tarObj:IsMagic() then
				if tarObj:GetOwner() == self then
					return true
				end
			end
		elseif self:IsPartner() or self:IsMagic() then
			if tarObj:IsPlayer() then
				if self:GetOwner() == tarObj then
					return true
				end
			end
		end		
	end
end
local function IsInSecurityPos( x, y)
	local inSecurity = nil
	if SECURITY_AREAS then
		for _i, _area in pairs(SECURITY_AREAS) do
			local pos = _area.pos
			local r = _area.r
			local absx = mabs(x - pos[1])
			local absy = mabs(y - pos[2])
			if absx <= r and absy <= r then 
				if absx ^ 2 + absy ^ 2 <= r ^ 2 then
					inSecurity = true
					break
				end
			end
		end
	end
	return inSecurity
end
function clsBaseChar:IsHitTarget(tarObj)
	if tarObj:IsPlayer() then
		if self:IsNpc() and self:GetBossType() == BOSS_TYPE_KSIEGEWAR then
			local compClubList = self:GetCompClubList()
			if compClubList then
				local tClubId = tarObj:GetClubId()
				if tClubId and compClubList[tClubId] then	--同队不能打
					return
				end
			end
		end
		
		local pkmode 
		if self:IsPlayer() then
			pkmode = self:GetPkMode()			
			local enemyList = self:GetEnemyList() or {}
			if  PK_ENEMY and pkmode ~= PKMODE_PEACE and enemyList[tarObj:GetId()] then
				--if not IsInSecurityPos( tarObj:GetX(), tarObj:GetY())  then
					return true 
				--end
			end
		end	
		if pkmode == PKMODE_HOSTILECLUB and not FLAG then 		
			return 
		end
		return laoi.obj_ishittarget(self:GetEngineObj(), tarObj:GetEngineObj())
	elseif tarObj:IsNpc() then
		if self:IsNpc() and self:GetBossType() == BOSS_TYPE_KSIEGEWAR then
			return
		elseif self:IsPlayer() and tarObj:GetBossType() == BOSS_TYPE_KSIEGEWAR then
			local compClubList = tarObj:GetCompClubList()
			if compClubList then
				local clubId = self:GetClubId()
				if clubId and compClubList[clubId] then	--同队不能打
					return
				end
			end
		end
		return laoi.obj_ishittarget(self:GetEngineObj(), tarObj:GetEngineObj())
	end
end

local function _CheckIsPlayer(charObj)
	return charObj:IsPlayer()
end

local function _CheckIsNpc(charObj)
	return charObj:IsNpc()
end

local CHECK_CHARTYPE = {
	[PLAYER_TYPE] = _CheckIsPlayer,
	[NPC_TYPE] = _CheckIsNpc,
}

function clsBaseChar:CheckCharType(charType)
	local charCheckFunc = charType and CHECK_CHARTYPE[charType]
	if charCheckFunc then
		if charCheckFunc(self) then
			return true
		else
			return 
		end
	end
	return true
end

function clsBaseChar:SearchOCompCharObj_ByAllChar(charType)
	local charCheckFunc = charType and CHECK_CHARTYPE[charType]
	local mapLayer = self:GetMapLayer()
	local aCharObj = CHAR_MGR.GetAllCharObj(mapLayer)
	for _, _CharObj in pairs(aCharObj) do
		if self:IsHitTarget(_CharObj) then
			if charCheckFunc then
				if charCheckFunc(_CharObj) then
					return _CharObj
				end
			else
				return _CharObj
			end
		end
	end
end

function clsBaseChar:SearchOCompCharObj(radius, isCircle, notCheck9, charType)
	if not radius then return end
	radius = mceil(radius)
	local dRadius = radius ^ 2
	
	if IsServer() then			--玩家不同阵营需要判断攻击模式和是否组队
		if not notCheck9 then
			if not laoi.map_hasocomp9(self:GetMapObj(), self:GetEngineObj(), radius) then		--预判周围没有其他人
				return
			end
		end
	end
	
	local charCheckFunc = charType and CHECK_CHARTYPE[charType]
	
	local x, y = self:GetX(), self:GetY()
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	
	local disSortTbl = DISTANCE_SORT[radius] or DISTANCE_SORT[DISTANCE_SORT_CNT]
	for i = #disSortTbl, 1, -1 do
		local tXCharTbl = mapChar[disSortTbl[i].x + x]
		if tXCharTbl then
			local tXYCharTbl = tXCharTbl[disSortTbl[i].y + y]
			if tXYCharTbl then
				if not isCircle or (dRadius >= disSortTbl[i].x ^ 2 + disSortTbl[i].y ^ 2) then
					for _id, _CharObj in pairs(tXYCharTbl) do
						if self:IsHitTarget(_CharObj) then
							if charCheckFunc then
								if charCheckFunc(_CharObj) then
									return _CharObj
								end
							else
								return _CharObj
							end
						end
					end
				end
			end
		end			
	end
	
	if DISTANCE_SORT_CNT < radius then
		for nRadius = DISTANCE_SORT_CNT + 1, radius do
			for i = -nRadius, nRadius do
				local tx = x + i
				if i == -nRadius or i == nRadius then 
					for j = -nRadius, nRadius do
						local ty = y + j
						local tXCharTbl = mapChar[tx]
						if tXCharTbl then
							local tXYCharTbl = tXCharTbl[ty]
							if tXYCharTbl then
								if not isCircle or (dRadius >= i ^ 2 + j ^ 2) then
									for _id, _CharObj in pairs(tXYCharTbl) do
										if self:IsHitTarget(_CharObj) then
											if charCheckFunc then
												if charCheckFunc(_CharObj) then
													return _CharObj
												end
											else
												return _CharObj
											end
										end
									end
								end
							end
						end
					end	
				else		
					local tXCharTbl = mapChar[tx]
					if tXCharTbl then
						for ty = y - nRadius, y + nRadius, 2 * nRadius do
							local tXYCharTbl = tXCharTbl[ty]
							if tXYCharTbl then
								if not isCircle or (dRadius >= (tx - x) ^ 2 + (ty - y) ^ 2) then
									for _id, _CharObj in pairs(tXYCharTbl) do
										if self:IsHitTarget(_CharObj) then
											if charCheckFunc then
												if charCheckFunc(_CharObj) then
													return _CharObj
												end
											else
												return _CharObj
											end
										end
									end
								end
							end
						end
					end
				end
			end			
		end
	end
end

function clsBaseChar:SearchSelfCompCharObj(radius)
	if not radius then return end
	radius = mceil(radius)
	
	if IsServer() then						--玩家不同阵营需要判断攻击模式和是否组队,但是这里是包含的,所以可以不用and self:IsPlayer()
		if not laoi.map_hasscomp9(self:GetMapObj(), self:GetEngineObj(), radius) then		--预判周围没有其他人
			return
		end
	end	
	
	local comp = self:GetComp()
	local x, y = self:GetX(), self:GetY()
	
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	
	local disSortTbl = DISTANCE_SORT[radius] or DISTANCE_SORT[DISTANCE_SORT_CNT]
	for i = #disSortTbl, 1, -1 do
		local tXCharTbl = mapChar[disSortTbl[i].x + x]
		if tXCharTbl then
			local tXYCharTbl = tXCharTbl[disSortTbl[i].y + y]
			if tXYCharTbl then
				for _id, _CharObj in pairs(tXYCharTbl) do
					if _CharObj ~= self and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
						return _CharObj
					end
				end
			end
		end			
	end
	
	if DISTANCE_SORT_CNT < radius then
		for nRadius = 1, radius do
			for i = -nRadius, nRadius do
				local tx = x + i
				if i == -nRadius or i == nRadius then 
					for j = -nRadius, nRadius do
						local ty = y + j
						local tXCharTbl = mapChar[tx]
						if tXCharTbl then
							local tXYCharTbl = tXCharTbl[ty]
							if tXYCharTbl then
								for _id, _CharObj in pairs(tXYCharTbl) do
									if _CharObj ~= self and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
										return _CharObj
									end
								end
							end
						end
					end	
				else		
					local tXCharTbl = mapChar[tx]
					if tXCharTbl then
						for ty = y - nRadius, y + nRadius, 2 * nRadius do
							local tXYCharTbl = tXCharTbl[ty]
							if tXYCharTbl then
								for _id, _CharObj in pairs(tXYCharTbl) do
									if _CharObj ~= self and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
										return _CharObj
									end
								end
							end
						end
					end
				end
			end			
		end	
	end
end

function clsBaseChar:GetMinRateHpSelfCompCharObj(radius, excludeCharId)
	if not radius then return end
	radius = mceil(radius)
	local minNum = 1
	local minObj = nil
	local comp = self:GetComp()
	local x, y = self:GetX(), self:GetY()
	
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	for i = -radius, radius do
		local tx = x + i
		for j = -radius, radius do
			local ty = y + j
			local tXCharTbl = mapChar[tx]
			if tXCharTbl then
				local tXYCharTbl = tXCharTbl[ty]
				if tXYCharTbl then
					for _id, _CharObj in pairs(tXYCharTbl) do
						if _CharObj:GetId() ~= excludeCharId and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
							local prop = (_CharObj["Get" .. mFIGHT_Hp](_CharObj) or 0) / (_CharObj["Get" .. mFIGHT_HpMax](_CharObj) or 1)
							if prop <= minNum then
								minNum = prop
								minObj = _CharObj
							end
						end
					end
				end
			end
		end		
	end		
	return minObj
end

function clsBaseChar:GetMaxRateHpSelfCompCharObj(radius, excludeCharId)
	if not radius then return end
	radius = mceil(radius)
	local maxNum = -100000000
	local maxObj = nil
	local comp = self:GetComp()
	local x, y = self:GetX(), self:GetY()
	
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	
	for i = -radius, radius do
		local tx = x + i
		for j = -radius, radius do
			local ty = y + j
			local tXCharTbl = mapChar[tx]
			if tXCharTbl then
				local tXYCharTbl = tXCharTbl[ty]
				if tXYCharTbl then
					for _id, _CharObj in pairs(tXYCharTbl) do
						if _CharObj:GetId() ~= excludeCharId and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
							local prop = (_CharObj["Get" .. mFIGHT_Hp](_CharObj) or 0) / (_CharObj["Get" .. mFIGHT_HpMax](_CharObj) or 1)
							if prop > maxNum then
								maxNum = prop
								maxObj = _CharObj
							end
						end
					end
				end
			end
		end		
	end		
	return maxObj
end

function clsBaseChar:GetMinPropSelfCompCharObj(radius, propName, excludeCharId)
	if not radius then return end
	radius = mceil(radius)
	local minNum = 100000000
	local minObj = nil
	local comp = self:GetComp()
	local x, y = self:GetX(), self:GetY()
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	
	for i = -radius, radius do
		local tx = x + i
		for j = -radius, radius do
			local ty = y + j
			local tXCharTbl = mapChar[tx]
			if tXCharTbl then
				local tXYCharTbl = tXCharTbl[ty]
				if tXYCharTbl then
					for _id, _CharObj in pairs(tXYCharTbl) do
						if _CharObj:GetId() ~= excludeCharId and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
							local prop = _CharObj["Get"..propName](_CharObj) or 0
							if prop < minNum then
								minNum = prop
								minObj = _CharObj
							end
						end
					end
				end
			end
		end		
	end		
	return minObj
end

function clsBaseChar:GetMaxPropSelfCompCharObj(radius, propName, excludeCharId)
	if not radius then return end
	radius = mceil(radius)
	local maxNum = -100000000
	local maxObj = nil
	local comp = self:GetComp()
	local x, y = self:GetX(), self:GetY()
	
	local mapLayer = self:GetMapLayer()
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	
	for i = -radius, radius do
		local tx = x + i
		for j = -radius, radius do
			local ty = y + j
			local tXCharTbl = mapChar[tx]
			if tXCharTbl then
				local tXYCharTbl = tXCharTbl[ty]
				if tXYCharTbl then
					for _id, _CharObj in pairs(tXYCharTbl) do
						if _CharObj:GetId() ~= excludeCharId and _CharObj:GetComp() == comp and not _CharObj:IsDie() then
							local prop = _CharObj["Get"..propName](_CharObj) or 0
							if prop > maxNum then
								maxNum = prop
								maxObj = _CharObj
							end
						end
					end
				end
			end
		end		
	end		
	return maxObj
end

--调整坐标,选择最接近ox,oy的dx+-[0,1],dy+-[0,1]的没阻塞位置
--如果现在的点跟dX,dY一样，																	
--		1.如果周围是npc,或者墙,或者玩家使得无法走动,那么返回0, nil, nil
--		2.如果周围能+-[0,1]能走动的则返回0, dX+-[0,1], dY+-[0,1]
--如果现在的点跟dX,dY不一样，
--		1.如果周围是npc,或者墙,或者玩家使得无法走动,那么返回1, nil, nil, 然后尽量往dX,dY前走(不包括平走),没则随机
--		2.如果周围能+-[0,1]能走动的则返回1, dX+-[0,1], dY+-[0,1], 
function clsBaseChar:AdjustPointNoItem(ox, oy, dx, dy)
	local dx, dy = dx or self:GetX(), dy or self:GetY()
	if dx == ox and dy == oy then
		for i = 1, 8 do
			local tx, ty = dx + c_aPoint[i][1], dy + c_aPoint[i][2]
			if not self:IsBlockPoint(tx, ty) and not self:HasObjNotItemInPos(tx, ty) then
				return 0, tx, ty
			end
		end
		return 0, nil, nil
	else
		local subX, subY = dx - ox, dy - oy
		if subX > 1 then
			subX = 1
		elseif subX < -1 then
			subX = -1
		end
		if subY > 1 then
			subY = 1
		elseif subY < -1 then
			subY = -1
		end
		local adjustTbl = c_aAdjustPoint[subX][subY]
		for _, _oneAdjustTbl in ipairs(adjustTbl or {}) do
			local tx, ty = dx + _oneAdjustTbl[1], dy + _oneAdjustTbl[2]
			if not self:IsBlockPoint(tx, ty) and not self:HasObjNotItemInPos(tx, ty) then
				return 1, tx, ty
			end			
		end
		return 1, nil, nil 
	end
end

function AdjustHasObj(mapLayer, x, y, excludeCharId)				--是否有对象在该位置(除去item对象)
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	local xCharTbl = mapChar[x]
	if xCharTbl then
		local xYCharTbl = xCharTbl[y]
		if xYCharTbl then	
			for _, charObj in pairs(xYCharTbl) do
				if not charObj:IsItem() and charObj:GetId() ~= excludeCharId then
					return true
				end
			end	
		end
	end
end

function clsBaseChar:AdjustPointNoItemClient(dMinBRadius, ddistance, ox, oy, dx, dy)
	local dx, dy = dx or self:GetX(), dy or self:GetY()
	if dx == ox and dy == oy then
		for i = 1, 16 do	--循环后还没移动代表无法移动					
			local randDir = nil
			if i <= 8 then
				randDir = mrandom(8)
			else
				randDir = i - 8
			end	
			local tx, ty = dx + c_aPoint[randDir][1], dy + c_aPoint[randDir][2]
			if not self:IsBlockPoint(tx, ty) and not self:HasObjNotItemInPos(tx, ty) then
				return 0, tx, ty
			end
		end
--		for i = 1, 8 do
--			local tx, ty = dx + c_aPoint[i][1], dy + c_aPoint[i][2]
--			if not self:IsBlockPoint(tx, ty) and not self:HasObjNotItemInPos(tx, ty) then
--				return 0, tx, ty
--			end
--		end
		return 0, nil, nil
	else
		local subX, subY = dx - ox, dy - oy
		if subX > 1 then
			subX = 1
		elseif subX < -1 then
			subX = -1
		end
		if subY > 1 then
			subY = 1
		elseif subY < -1 then
			subY = -1
		end
		local adjustTbl = c_aAdjustPoint[subX][subY]
		local adjustX, adjustY
		for _, _oneAdjustTbl in ipairs(adjustTbl or {}) do
			local tx, ty = dx + _oneAdjustTbl[1], dy + _oneAdjustTbl[2]
			if not (tx == ox and ty == oy) and not self:IsBlockPoint(tx, ty) then
				while true do
					local tmptx, tmpty = tx + _oneAdjustTbl[1], ty + _oneAdjustTbl[2]
					if not (tmpty == ox and tmpty == oy) and not self:IsBlockPoint(tmptx, tmpty) then
						if (tmptx - dx) ^ 2 + (tmpty - dy) ^ 2 > ddistance then
							if AdjustHasObj(self:GetMapLayer(), tx, ty)	then		--尽量不重叠
								adjustX, adjustY = tx, ty
								break
							end
							return 1, tx, ty
						end
						tx, ty = tmptx, tmpty
					else
						if (tx - dx) ^ 2 + (ty - dy) ^ 2 < dMinBRadius then		--小于最小值,去另外一边
							break
						end
						return 1, tx, ty
					end
				end
			end			
		end
		return 1, adjustX, adjustY
	end
end

function IsBlockPoint(x, y)				--是否是墙
	if x > MAP_MAX_X or y > MAP_MAX_Y or x <= 0 or y <= 0 then return true end					--如果从0开始就修改这里
	return not lmapdata.getz(MAP_NO, x, y)
end

function clsBaseChar:IsBlockPoint(x, y)
	return IsBlockPoint(x, y)
end

function CanMoveBySlope(ox, oy, nx, ny)
	local oz = lmapdata.getz(MAP_NO, ox, oy)
	if not oz then return end
	local nz = lmapdata.getz(MAP_NO, nx, ny)
	if not nz then return end
	return mabs(oz - nz) <= NORMAL_SLOPE
end

function GetDirByPos(ox, oy, dx, dy)
	local disX, disY = dx - ox, dy - oy
	if disX == 0 and disY == 0 then return end
	
	if disX == 0 then
		if disY > 0 then
			return 5
		else
			return 1
		end
	elseif disX > 0 then
		if disY == 0 then
			return 3
		elseif disY > 0 then   
			if disX == disY then
				return 4
			elseif disX > disY then
				local tmp11 = (disX - disY) / (disX + disY)
				local tmp10 = disY / disX
				if tmp11 <= tmp10 then
					return 4
				else
					return 3
				end
			else
				local tmp01 = disX / disY
				local tmp11 = (disY - disX) / (disX + disY)
				if tmp01 <= tmp11 then
					return 5
				else
					return 4
				end				
			end
		else
			disY = -disY
			if disX == disY then
				return 2
			elseif disX > disY then
				local tmp1_1 = (disX - disY) / (disX + disY)
				local tmp10 = disY / disX
				if tmp1_1 <= tmp10 then
					return 2
				else
					return 3
				end
			else
				local tmp0_1 = disX / disY
				local tmp1_1 = (disY - disX) / (disX + disY)
				if tmp0_1 <= tmp1_1 then
					return 1
				else
					return 2
				end				
			end			
		end
	else
		disX = -disX
		if disY == 0 then
			return 7
		elseif disY > 0 then   
			if disX == disY then
				return 6
			elseif disX > disY then
				local tmp_11 = (disX - disY) / (disX + disY)
				local tmp_10 = disY / disX
				if tmp_11 <= tmp_10 then
					return 6
				else
					return 7
				end
			else
				local tmp01 = disX / disY
				local tmp_11 = (disY - disX) / (disX + disY)
				if tmp01 <= tmp_11 then
					return 5
				else
					return 6
				end				
			end
		else
			disY = -disY
			if disX == disY then
				return 8, 7, 1
			elseif disX > disY then
				local tmp_1_1 = (disX - disY) / (disX + disY)
				local tmp_10 = disY / disX
				if tmp_1_1 <= tmp_10 then
					return 8
				else
					return 7
				end
			else
				local tmp0_1 = disX / disY
				local tmp_1_1 = (disY - disX) / (disX + disY)
				if tmp0_1 <= tmp_1_1 then
					return 1
				else
					return 8
				end				
			end			
		end		
	end
end

--校验技能坐标
function clsBaseChar:GetSkillPosCheck()
	return self:GetTmp("SkillPosCheck")
end
function clsBaseChar:SetSkillPosCheck(skillId, x, y)
	--设置空,因为怕用了旧的
	self:SetTmp("SkillPosCheck", nil)
	
	local gx, gy = lmapdata.realtogrid(MAP_NO, x, y)
	if gx and gy then
		self:SetTmp("SkillPosCheck", {
			skillId = skillId,
			x = x,
			y = y,
			gx = gx,
			gy = gy,
		})	
	end
end

function clsBaseChar:GetHitType()
	return nil
end

function clsBaseChar:SyncNearByPlayer(protoName, protoTbl)
	local playerTbl = self:GetNearByPlayers()
	if playerTbl then
		local vfds = {}
		for _, pCharId in pairs(playerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				tinsert(vfds, pCharObj:GetVfd())
			end
		end
		if self:IsPlayer() then
			tinsert(vfds, self:GetVfd())		--把自己添加进去
			pbc_send_msg(vfds, protoName, protoTbl)	
		elseif self:IsNpc() then
			if #vfds > 0 then
				pbc_send_msg(vfds, protoName, protoTbl)	
			end
		end	
	else
		if self:IsPlayer() then		--把自己添加进去
			pbc_send_msg(self:GetVfd(), protoName, protoTbl)
		end
	end
end

function clsBaseChar:GetNearByPlayerVfds()
	local playerTbl = self:GetNearByPlayers()
	local vfds = {}
	if self:IsPlayer() then
		tinsert(vfds, self:GetVfd())		--把自己添加进去
	end	
	
	if playerTbl then
		for _, pCharId in pairs(playerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				tinsert(vfds, pCharObj:GetVfd())
			end
		end
	end
	return vfds
end

function clsBaseChar:IsSkillSameComp(tarObj)
	if self:IsPlayer() then
		if tarObj:IsMagic() or tarObj:IsPartner() then
			return self == tarObj:GetOwner()
		elseif tarObj:IsPlayer() then
			local sClubId = self:GetClubId()
			return (sClubId and sClubId ~= "" and sClubId == tarObj:GetClubId())
		end
	elseif self:IsMagic() or self:IsPartner() then
		if tarObj:IsMagic() or tarObj:IsPartner() then
			return self:GetOwner() == tarObj:GetOwner()
		elseif tarObj:IsPlayer() then
			return self:GetOwner() == tarObj
		end	
	elseif self:IsNpc() then
		if tarObj:IsNpc() then
			return self:GetComp() == tarObj:GetComp()
		elseif tarObj:IsPlayer() then
			return self:GetComp() == tarObj:GetComp()
		end	
	end
end

---------------------------------------------ai使用的函数 end-----------------------------------------------

---Char是什么...
function clsBaseChar:IsBaseChar()
	return true
end
function clsBaseChar:IsPlayer()
	return false
end
function clsBaseChar:IsNpc()
	return false
end
function clsBaseChar:IsStaticNpc()
	return false
end
function clsBaseChar:IsItem()
	return false
end
function clsBaseChar:IsPartner()
	return false
end
function clsBaseChar:IsMagic()
	return false
end
function clsBaseChar:IsMirrorPlayer()
	return false
end

function __init__()
	
end

