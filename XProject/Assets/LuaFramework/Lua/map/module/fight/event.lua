local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local os=os
local type=type
local mfloor=math.floor
local mceil=math.ceil
local tinsert=table.insert
local tremove=table.remove
local unpack=unpack
local FIGHT_EFF_NAME=FIGHT_EFF_NAME
local FIGHT_STILLEFF_NAME=FIGHT_STILLEFF_NAME
local lua_time_sec=lua_time_sec
local CAN_PUSH=CAN_PUSH
local IsKuaFuServer = cfgData and cfgData.IsKuaFuServer

local mTABLETYPE=mTABLETYPE
local mNUMBERTYPE = mNUMBERTYPE
local pbc_send_msg = pbc_send_msg

local LOGIC_METH_NO_BUFF		= "no_buff"
local LOGIC_METH_HAS_BUFF 		= "has_buff"
local LOGIC_METH_COMPARE_PROP 	= "compare_prop"
local LOGIC_METH_NO_LOGIC 		= "no_logic"
local LOGIC_METH_TARGET_TYPE	= "target_type"
local LOGIC_METH_HAS_BUFFSUBTYPE = "has_buffsubtype"

local EFFECT_OWNER_SKILL_TARGET 	= "skill_target"
local EFFECT_OWNER_SELF_TARGET 		= "self_target"

local EFFECT_TARTYPE_ALL		= "tar_all"
local EFFECT_TARTYPE_NPC		= "tar_npc"
local EFFECT_TARTYPE_PLAYER		= "tar_player"

local EFFECT_METH_ADD_BUFF = "Buff"

local SKILL_MOD= Import("setting/skill/skill_data.lua")
local SKILL_PROPDATA = SKILL_MOD.GetAllSkillPropData()
local PROP_LOGICDATA = SKILL_MOD.GetAllPropLogicData()
local BUFF_MOD = Import("setting/skill/buff_data.lua")
local BUFF_PROPDATA = BUFF_MOD.GetAllBuffData()
local ARTIFACT_DATA = ARTIFACTDATA.GetArtifactMartialData()

function TryCall(Func, ...)
--	local num_args = select("#", ...)
--	local arg = {}
--	for i = 1, num_args do
--		arg[i] = select(i, ...)
--	end
--	arg['n'] = num_args

	if IsClient() then
		local flag, err = pcall(Func, ...)
		if not flag then
			_RUNTIME_ERROR(err, debug.traceback())
		end
		return flag, err
	else
		local tbl = {...}
		local flag, err = xpcall(function () return Func(unpack(tbl)) end , debug.traceback)
		if not flag then
			_RUNTIME_ERROR(err)
		end
		return flag, err
	end
end

--添加生命
local function OnEffHp(AttObj, target, value)
	if target:IsDie() then return end
	
	if value > 0 then
		local reHpRate = AttObj:GetFightValue(mFIGHT_ReHpRate) or 0
		local reHpValue = AttObj:GetFightValue(mFIGHT_ReHpValue) or 0
		
		local tarReHpRate = target:GetFightValue(mFIGHT_SubReHpRate) or 0
		local tarHpValue = target:GetFightValue(mFIGHT_SubReHpValue) or 0
		value = value + reHpValue + tarHpValue
		if value <= 0 then return end
--		if reHpRate > 0 then
			value = value * (1 + (reHpRate + tarReHpRate) / 10000)
--		end
	end
	
	value = mfloor(value)
	target:SetTmp(FIGHT_SHOW_HP, true)
	target:AddHp(value, AttObj:GetId(), nil, HURT_TYPE_00)
end

--添加buff属性
local function OnAddBuffProp(AttObj, target, value)
	if target:IsDie() then return end
	
	if not value.buffNo then
		_RUNTIME_ERROR("OnAddBuffProp not buffNo")
		return
	end
	
	local buffData = target:GetBuffDataById(value.buffNo)
	if not buffData then return end
	for _key, _value in pairs(value) do
		if _key ~= "buffNo" then
			local tValue = buffData[_key]
			if not tValue then
				buffData[_key] = _value
			else
				if type(_value) == "number" then
					buffData[_key] = tValue + _value
					
					if _key == mFIGHT_Shield then
						local shieldHp = target:GetSaveShieldHp() or 0
						shieldHp = shieldHp + _value
						target:SetSaveShieldHp(shieldHp)
					end
				end
			end
		end
	end
end

local function OnAddNpcProp(AttObj, TarObj, value, propData)
	AddNpcProp(AttObj, TarObj, value, propData)
end

local function OnEffAddBuff(AttObj, TarObj, value, skillId)
	AddBuff(AttObj, TarObj, value, skillId)
end

local function OnEffDelBuffType(AttObj, TarObj, value, skillId)
	DelBuffType(AttObj, TarObj, value)
end 

ExtraSkill = {}

function GetPlayerSp()
	local UserObj = CHAR_MGR.GetRandomUserObj(1)
	if UserObj then
		return UserObj:GetSp() or 0
	end
	return 0
end

function AddExtraSkill(AttObj, TarObj, skillId, attSkillId)
	local nTimeNo = GetNowTimeNo()
	local nextTimeNo = nTimeNo + 1
	local data = ExtraSkill[nextTimeNo] or {} 
	
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not FIGHT.CheckTarget(AttObj, TarObj, skillData.AttKind, true) then
		local tmpTarObj = FIGHT.GetMainTarget(AttObj, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
		if tmpTarObj then
			TarObj = tmpTarObj
		end
	end
	
	local tmp = {
		att_id = AttObj:GetId(),
		tar_id = TarObj:GetId(),
		mx = TarObj:GetX(),
		my = TarObj:GetY(),
		skillId = skillId,
	}
	AttObj:SetTmpSkillLv(skillId, AttObj:GetMartialLevelBySkillId(attSkillId))
	tinsert(data, tmp)
	ExtraSkill[nextTimeNo] = data	
end

function CheckExtraSkill(noTimeNo)
	local extraSkillData = ExtraSkill[noTimeNo]
	if not extraSkillData then return end
	ExtraSkill[noTimeNo] = nil
	for _, _data in pairs(extraSkillData) do
		local AttObj = CHAR_MGR.GetCharById(_data.att_id)
		local TarObj = CHAR_MGR.GetCharById(_data.tar_id)
		local skillData = SKILL_DATA.GetMartialSkill(_data.skillId)
		if skillData.TargetType ~= SINGLE_TARGET_TYPE or TarObj then
			if AttObj and not AttObj:IsDie() then
				TryCall(FIGHT.UseSkillAct, AttObj, TarObj, _data.skillId, _data.mx, _data.my)
			end			
		end 
	end
end

local function OnEffExtraSkill(AttObj, TarObj, skillId, attSkillId)
	if not TarObj then return end
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then return end
	if skillId == attSkillId then return end			--防止循环
	
	if not FIGHT.CheckTarget(AttObj, TarObj, skillData.AttKind) then
		TarObj = FIGHT.GetMainTarget(AttObj, skillData.AttKind, skillData.AttKind2, skillData.AttRange)
		if not TarObj then return end
	end
	AddExtraSkill(AttObj, TarObj, skillId, attSkillId)	
end

local EffectFucMap = {
	[mFIGHT_Hp] = OnEffHp,
	[mFIGHT_AddBuffProp] = OnAddBuffProp,
}
local EffectFucMap2 = {
	[EFFECT_METH_ADD_BUFF] = OnEffAddBuff,
	[mFIGHT_DelBuffType] = OnEffDelBuffType,
}
local EffectFucMap3 = {
	[mFIGHT_ExtraSkill] = OnEffExtraSkill,
}
local EffectFucMap4 = {
	[mFIGHT_AddNpcProp] = OnAddNpcProp,
}

---------------------------------------------------逻辑判断 start-----------------------------------------------------
local function on_has_buff(TarObj, buffType, logicData)
	local buffData = TarObj:GetBuffInfo()
	if not buffData then return	end	--没有buff
		
	if buffType == 1 then
		for _, _bId in pairs(logicData.VarList or {}) do
			if buffData[_bId] then	--有这个buff
				return true
			end
		end
	else	--某一类buff
		local checkType = logicData.VarName3
		if not checkType then return end
		for _bId, _bInfo in pairs(buffData) do
			if _bInfo.buffType == checkType then
				return true
			end
		end
	end
end 

local function on_logic_no_buff(logicData, AttObj, TarObj)
	local filterObj = nil
	local filterType = logicData.VarName1
	local buffType = logicData.VarName2 
	if filterType ~= 1 and filterType ~= 2 and filterType ~= 3 then return end
	if buffType ~= 1 and buffType ~= 2 then return end	
	if filterType == 1 then
		filterObj = AttObj
	else
		if not TarObj then return end
		filterObj = TarObj
		if filterType == 3 then
			if not TarObj:IsPlayer() then
				return
			end
		end
	end
	
	return not on_has_buff(filterObj, buffType, logicData)	
end

local function on_logic_has_buff(logicData, AttObj, TarObj)
	local filterObj = nil
	local filterType = logicData.VarName1
	local buffType = logicData.VarName2 
	if filterType ~= 1 and filterType ~= 2 then return end
	if buffType ~= 1 and buffType ~= 2 then return end	
	if filterType == 1 then
		filterObj = AttObj
	else
		if not TarObj then return end
		filterObj = TarObj
	end

	return on_has_buff(filterObj, buffType, logicData)
end 

local panduan = {
	[1] = function(value1,value2) if value1 > value2 then return true end end,
	[2] = function(value1,value2) if value1 < value2 then return true end end,
	[3] = function(value1,value2) if value1 >= value2 then return true end end,
	[4] = function(value1,value2) if value1 <= value2 then return true end end,
	[5] = function(value1,value2) if value1 == value2 then return true end end,
}
local function on_logic_compare_prop(logicData, AttObj, TarObj)
	local compType = logicData.VarName1
	if compType ~= 1 and compType ~= 2 and compType ~= 3 and compType ~= 4 and compType ~= 5 then return end
	if not logicData.VarName3 then return end
	local panduanFunc = panduan[logicData.VarName3]
	if not panduanFunc then return end
	if type(logicData.VarName2) ~= mSTRINGTYPE then return end
	local checkNum1, checkNum2 = nil, nil
	
	if compType == 1 then
		checkNum2 = logicData.VarName4
		if type(checkNum2) ~= mNUMBERTYPE then return end
				
		checkNum1 = AttObj:GetFightValue(logicData.VarName2)
		if type(checkNum1) ~= mNUMBERTYPE then return end
		return panduanFunc(checkNum1, checkNum2)
	elseif compType == 2 then
		if not TarObj then return end
		checkNum2 = logicData.VarName4
		if type(checkNum2) ~= mNUMBERTYPE then return end
		
		checkNum1 = TarObj:GetFightValue(logicData.VarName2)
		if type(checkNum1) ~= mNUMBERTYPE then return end
		return panduanFunc(checkNum1, checkNum2)	
	elseif compType == 3 then
		if not TarObj then return end
		checkNum1 = AttObj:GetFightValue(logicData.VarName2)
		if type(checkNum1) ~= mNUMBERTYPE then return end
		checkNum2 = TarObj:GetFightValue(logicData.VarName2)
		if type(checkNum2) ~= mNUMBERTYPE then return end	
		return panduanFunc(checkNum1, checkNum2)
	elseif compType == 4 then
		checkNum2 = logicData.VarName4
		if type(checkNum2) ~= mNUMBERTYPE then return end
		checkNum1 = AttObj:GetFightValue(logicData.VarName2)
		if type(checkNum1) ~= mNUMBERTYPE then return end
		checkNum2 = checkNum1 * checkNum2 / 100
		if logicData.VarName2 == mFIGHT_HpMax then
			checkNum1 = AttObj:GetFightValue(mFIGHT_Hp)
		end
		return panduanFunc(checkNum1, checkNum2)
	else
		if not TarObj then return end
		checkNum2 = logicData.VarName4
		if type(checkNum2) ~= mNUMBERTYPE then return end	
		
		checkNum1 = TarObj:GetFightValue(logicData.VarName2)
		if type(checkNum1) ~= mNUMBERTYPE then return end	
		checkNum2 = checkNum1 * checkNum2 / 100
		if logicData.VarName2 == mFIGHT_HpMax then
			checkNum1 = TarObj:GetFightValue(mFIGHT_Hp)
		end
		return panduanFunc(checkNum1, checkNum2)
	end
end

local function on_logic_no_logic(logicData, AttObj, ...)
	return true
end

local function on_logic_target_type(logicData, AttObj, TarObj)
	local filterType = logicData.VarName1
	if not TarObj then return end
	if filterType == 1 then
		return TarObj:IsPlayer()
	elseif filterType == 2 then
		return TarObj:IsNpc()
	elseif filterType == 3 then
		return not TarObj:IsNpc()
	end
	return 
end 

local function on_logic_has_buffsubtype(logicData, AttObj, TarObj)
	local filterObj = nil
	local filterType = logicData.VarName1
	local buffSubType = logicData.VarName2 
	if filterType ~= 1 and filterType ~= 2 then return end
	if filterType == 1 then
		filterObj = AttObj
	else
		if not TarObj then return end
		filterObj = TarObj
	end
	if not buffSubType then return end
	local buffData = filterObj:GetBuffInfo()
	if not buffData then return	end	-- 没有buff
	for _bId, _bInfo in pairs(buffData) do
		if _bInfo.buffSubType == buffSubType then
			return true
		end
	end
end

--逻辑方法表
local LogicFuncMap = {
	[LOGIC_METH_NO_BUFF]			= on_logic_no_buff,			--#判断某人身没buff
	[LOGIC_METH_HAS_BUFF] 			= on_logic_has_buff,		--#判断某人身是否有buff
	[LOGIC_METH_COMPARE_PROP] 		= on_logic_compare_prop,	--#比较某个属性的大小
	[LOGIC_METH_NO_LOGIC] 			= on_logic_no_logic,		--#没有逻辑
	[LOGIC_METH_TARGET_TYPE]		= on_logic_target_type,		--#敌方对象类型
	[LOGIC_METH_HAS_BUFFSUBTYPE] 	= on_logic_has_buffsubtype,	--#是否有某子类型buff
}
---------------------------------------------------逻辑判断 end-----------------------------------------------------

function DoEffect(propData, AttObj, TarObj, ...)
	local func = propData.EffectTrueFunc
	if not func then return end
	local effectOwnerStr = propData.EffectOwner
	local skillId = propData.ID
	
	local effectObj = nil
	if effectOwnerStr == EFFECT_OWNER_SKILL_TARGET then
		effectObj = TarObj
	elseif effectOwnerStr == EFFECT_OWNER_SELF_TARGET then
		effectObj = AttObj
	end
	if not effectObj then return end

	local resTbl = func(skillId, AttObj, effectObj)
	for _key, _value in pairs(resTbl) do 
		if EffectFucMap[_key]  then 
			EffectFucMap[_key](AttObj, effectObj, _value, ...)
		elseif EffectFucMap2[_key] then
			EffectFucMap2[_key](AttObj, effectObj, _value, skillId, ...)
		else --其它战斗属性
			local isOk = true
			if _key == mFIGHT_MovePush or _key == mFIGHT_MovePull then
				if effectObj:IsNpc() and effectObj:GetCanPush() ~= CAN_PUSH then
					isOk = nil
				elseif effectObj:IsPlayer() then
					isOk = nil
				end
			end
			if isOk then
				AddFightEff(AttObj, effectObj, _key, _value, propData, ...)
			end
		end 
	end
end 

function DoPassEffect(propData, AttObj, TarObj, attSkillId, passSkillId, ...)
	local func = propData.EffectTrueFunc
	if not func then return end
	local effectOwnerStr = propData.PassEffectOwner
	local skillId = propData.ID
	local effectObjs = {}
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then return end
	if skillData.PassUseTarType and attSkillId then
		skillData = SKILL_DATA.GetMartialSkill(attSkillId)
		if not skillData then return end
	end
	if skillData.OverUseTarType and passSkillId then
		skillData = SKILL_DATA.GetMartialSkill(passSkillId)
		if not skillData then return end
	end
	local tarType = skillData.TargetType
	local attKind = skillData.AttKind
	local attKind2 = skillData.AttKind2
	local attRange = skillData.AttRange
	if tarType == SINGLE_TARGET_TYPE then		--单体攻击
		if not FIGHT.CheckTarget(AttObj, TarObj, attKind, true) then
			local tmpObj = FIGHT.GetMainTarget(AttObj, attKind, attKind2, attRange)
			if tmpObj then
				tinsert(effectObjs, tmpObj)
			end
		else
			tinsert(effectObjs, TarObj)
		end
	else
		local areaCenter = skillData.AttAreaCenter
		local area = skillData.AttArea
		if not area then return end
		local martialLevel = AttObj:GetMartialLevelBySkillId(skillData.ID) or 1
		area = area(martialLevel)
		if not FIGHT.CheckTarget(AttObj, TarObj, attKind, true) then			--选取主的TarObj
			TarObj = FIGHT.GetMainTarget(AttObj, attKind, attKind2, attRange)
		end
--		if not TarObj then return end			--所有人的那个不用判断
		for _, _oneAreaInfo in ipairs(area) do
			local shape = _oneAreaInfo.shape
			if shape ~= SKILL_SHAPE_ALL then
				if not TarObj then return end
			end
			if shape == SKILL_SHAPE_CIRCLE then
				local r = _oneAreaInfo.r
				if not r then return end
				local tr = r > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or r
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = AttObj:GetX(), AttObj:GetY()	
				else						--敌人为中心
					rx, ry = TarObj:GetX(), TarObj:GetY()
				end
				local dtr = tr ^ 2
				for i = rx - tr, rx + tr do
					for j = ry - tr, ry + tr do
						if not BASECHAR.IsBlockPoint(i, j) then
							if (i - rx) ^ 2 + (j - ry) ^ 2 <= dtr then
								local tCharTbl = FIGHT.GetXYBattleCharObjs(AttObj:GetMapLayer(), i, j)
								if tCharTbl then
									for _id, _CharObj in pairs(tCharTbl) do
										if FIGHT.CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
											tinsert(effectObjs, _CharObj)
										end
									end
								end
							end
						end
					end
				end
			elseif shape == SKILL_SHAPE_SECTOR then
--				{shape=3,r=10,degreeleft=135,degreeright=45}
				local r = _oneAreaInfo.r
				local dLeft = _oneAreaInfo.degreeleft
				local dRight = _oneAreaInfo.degreeright
				if not dLeft or not dRight or not r then return end
				local tr = r > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or r
				
				local ax, ay = AttObj:GetX(), AttObj:GetY()	
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = ax, ay
				else						--敌人为中心
					rx, ry = TarObj:GetX(), TarObj:GetY()
				end
				local dx, dy = TarObj:GetX(), TarObj:GetY()
				if dx == ax and dy == ay then
					dx, dy = ax + 1, ay + 1
				end
				local func = FIGHT.GetSectorFunc(dLeft, dRight, ax, ay, dx, dy, rx, ry, tr)
				local dtr = tr ^ 2
				for i = rx - tr, rx + tr do
					for j = ry - tr, ry + tr do
						if not BASECHAR.IsBlockPoint(i, j) then
							if func(i, j) then
								local tCharTbl = FIGHT.GetXYBattleCharObjs(AttObj:GetMapLayer(), i, j)
								if tCharTbl then
									for _id, _CharObj in pairs(tCharTbl) do
										if FIGHT.CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
											tinsert(effectObjs, _CharObj)
										end
									end
								end
							end
						end
					end
				end
			elseif shape == SKILL_SHAPE_LINE then
				local len = _oneAreaInfo.len
				if not len then return end
				local len = len > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or len
				local accuracy = _oneAreaInfo.accuracy or 0.2
				local dlen = len ^ 2
				local ax, ay = AttObj:GetX(), AttObj:GetY()	
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = ax, ay
				else						--敌人为中心
					rx, ry = TarObj:GetX(), TarObj:GetY()
				end
				local dx, dy = TarObj:GetX(), TarObj:GetY()
				if dx == ax and dy == ay then
					dx, dy = ax + 1, ay + 1
				end
				
				--rx=x1 dx=x2 ry=y1 dy=y2
				local PosList = {}
				local IsInFunc
				if rx==dx then		--x轴
					PosList.minx = rx-accuracy
					PosList.maxx = rx+accuracy
					PosList.miny = ry>dy and dy or ry
					PosList.maxy = PosList.miny+len
				elseif ry==dy then	--y轴
					PosList.miny = ry-accuracy
					PosList.maxy = ry+accuracy
					PosList.minx = rx>dx and dx or rx
					PosList.maxx = PosList.minx+len
				else
					PosList, IsInFunc = FIGHT.GetIsInFunc(rx, ry, dx, dy, accuracy, len)
				end
				
				--_RUNTIME("PosList:", sys.dump(PosList))
				for _x=math.floor(PosList.minx),math.ceil(PosList.maxx) do
					for _y=math.floor(PosList.miny),math.ceil(PosList.maxy) do
						if not BASECHAR.IsBlockPoint(_x, _y) then
							local tCharTbl = FIGHT.GetXYBattleCharObjs(AttObj:GetMapLayer(), _x, _y)
							if tCharTbl and (not IsInFunc or IsInFunc(_x, _y)) then
								for _id, _CharObj in pairs(tCharTbl) do
									if FIGHT.CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
										tinsert(effectObjs, _CharObj)
									end
								end
							end
						end	
					end
				end
			elseif shape == SKILL_SHAPE_ALL then
				if attKind == 2 then			--敌方的所有
					local allCharObj = CHAR_MGR.GetAllCharObj(AttObj:GetMapLayer())
					for _, _CharObj in pairs(allCharObj) do
						if AttObj:IsHitTarget(_CharObj) then
							tinsert(effectObjs, _CharObj)
						end
					end
				else
					if AttObj:IsPlayer() then
						if FIGHT.CheckTarget(AttObj, AttObj, attKind) then		--测试是否需要攻击
							tinsert(effectObjs, AttObj)
						end							
					elseif AttObj:IsNpc() then
						local allCharObj = CHAR_MGR.GetAllCharObj(AttObj:GetMapLayer())
						for _, _CharObj in pairs(allCharObj) do
							if AttObj:GetComp() == _CharObj:GetComp() then
								tinsert(effectObjs, _CharObj)
							end
						end						
					end
				end
			end
		end
	end
	if #effectObjs > 0 then
		local hasDeal = false
		for _, _effectObj in pairs(effectObjs) do
			local isTarOk = false
			local tarType = propData.EffectTarType
			if not tarType or tarType == EFFECT_TARTYPE_ALL then
				isTarOk = true
			elseif tarType == EFFECT_TARTYPE_NPC then
				if _effectObj:IsNpc() then
					isTarOk = true
				end
			elseif tarType == EFFECT_TARTYPE_PLAYER then
				if _effectObj:IsPlayer() then
					isTarOk = true
				end					
			end
			if isTarOk then
				hasDeal = true
				local resTbl = func(skillId, AttObj, _effectObj)
				for _key, _value in pairs(resTbl) do 
					if EffectFucMap[_key] then 
						EffectFucMap[_key](AttObj, _effectObj, _value, ...)
					elseif EffectFucMap2[_key] then
						EffectFucMap2[_key](AttObj, _effectObj, _value, skillId, ...)
					elseif EffectFucMap3[_key] then	--额外技能
						EffectFucMap3[_key](_effectObj, TarObj, _value, attSkillId, ...)
					elseif EffectFucMap4[_key] then
						EffectFucMap4[_key](AttObj, _effectObj, _value, propData, ...)
					else --其它战斗属性
						local isOk = true
						if _key == mFIGHT_MovePush or _key == mFIGHT_MovePull then
							if _effectObj:IsNpc() and effectObj:GetCanPush() ~= CAN_PUSH then
								isOk = nil
							elseif _effectObj:IsPlayer() then
								isOk = nil
							end
						end
						if isOk then
							AddFightEff(AttObj, _effectObj, _key, _value, propData, ...)
						end
					end 
				end
			end
		end
		if hasDeal then
			local attOneSkill = AttObj:GetOneSkillData(skillId)
			if attOneSkill then
				local nTimeNo = GetNowTimeNo()
				if attOneSkill.PassTimeNo ~= nTimeNo then
					attOneSkill.PassTimeNo = nTimeNo
					if attOneSkill.MartialId and ARTIFACT_DATA[attOneSkill.MartialId] then
						pbc_send_msg(AttObj:GetVfd(), "S2c_aoi_skill_ctime", {fid = AttObj:GetFId(), skill_id = skillId, ctime = attOneSkill.CD * 1000 * lua_time_sec})
					end
				end
			end
		end
	end
end

PassMsgProcess = {}
function AddPassMsg(propData, AttObj, TarObj, attSkillId, ...)
	local num_args = select("#", ...)
	local arg = {}
	for i = 1, num_args do
		arg[i] = select(i, ...)
	end
	arg['n'] = num_args

	local tmp = {
		att_id = AttObj:GetId(),
		tar_id = TarObj and TarObj:GetId() or nil,
		propData = propData,
		attSkillId = attSkillId,
		arg = arg,
	}
	local nTimeNo = GetNowTimeNo()
	local nextTimeNo = nTimeNo + 1
	local data = PassMsgProcess[nextTimeNo] or {} 
	tinsert(data, tmp)
	PassMsgProcess[nextTimeNo] = data
end

function CheckPassMsg(noTimeNo)
	local msgData = PassMsgProcess[noTimeNo]
	if not msgData then return end
	
	PassMsgProcess[noTimeNo] = nil
	for _, _data in pairs(msgData) do
		local AttObj = CHAR_MGR.GetCharById(_data.att_id)
		if AttObj and not AttObj:IsDie() then
			local TarObj = CHAR_MGR.GetCharById(_data.tar_id)
			TryCall(DoPassEffect, _data.propData, AttObj, TarObj, _data.attSkillId, nil, unpack(_data.arg))
		end
	end
end

--处理被动技能消息
--第三个如果有必须是TarObj,不然就弄nil
--注意:如果是PASS_BATTLE则延迟下次执行,因为可能没创建完成队伍的信息
function ProcessPassMessage(msgId, AttObj, TarObj, attSkillId, ...)
	CheckClearStillFightEff(AttObj, TarObj)
	if TarObj then
		if type(TarObj) ~= mTABLETYPE then
			error("not TarObj in DoEffect, skillId:" .. propData.ID)
		end
		if not TarObj.IsBaseChar or not TarObj.IsBaseChar() then
			error("not TarObj in DoEffect, skillId:" .. propData.ID)
		end
	end
	local passSkillData = AttObj:GetPassSkillByWhen(msgId)
	if not passSkillData then return end
	for _skillId, _ in pairs(passSkillData) do
		local attOneSkill = AttObj:GetOneSkillData(_skillId)
		if attOneSkill then
			local nowTimeNo = GetNowTimeNo()
			local passTimeNo = attOneSkill.PassTimeNo
			if not passTimeNo or passTimeNo == nowTimeNo or (passTimeNo + attOneSkill.CD <= nowTimeNo) then
				local propData = SKILL_PROPDATA[_skillId]
				if not propData then return end 
				for _, _data in ipairs(propData) do
					if _data.When == msgId and _data.RateFunc then
--						local rateNum = _data.RateFunc or 10000
						local rateNum = nil
						if msgId == PASS_ADDBUFF then
							rateNum = _data.RateFunc(attSkillId or _skillId, AttObj)
						else
							rateNum = _data.RateFunc(_skillId, AttObj)
						end
						local rand = math.random(10000)
						if rand <= rateNum then
							local logicNo = _data.LogicNo			--判断逻辑
							local logicOk = true
							if logicNo then
								logicNo = logicNo(_skillId, AttObj)
							end
							if logicNo and PROP_LOGICDATA[logicNo] then	
								local logicMethName = PROP_LOGICDATA[logicNo].LogicMeth
								if LogicFuncMap[logicMethName] then
									logicOk = LogicFuncMap[logicMethName](PROP_LOGICDATA[logicNo], AttObj, TarObj)
								end
							end
							if logicOk then
								if msgId == PASS_BATTLE then
									AddPassMsg(_data, AttObj, TarObj, attSkillId, ...)
								else
									DoPassEffect(_data, AttObj, TarObj, attSkillId, nil, ...)
								end
								ProcessOverMessage(AttObj, TarObj, attSkillId, _skillId, ...)
							end
						end 			
					end
				end		
			end
		end
	end
end

function ProcessOverMessage(AttObj, TarObj, attSkillId, passSkillId, ...)
	local passSkillData = AttObj:GetPassSkillByWhen(OVER_PASSHIT)
	if not passSkillData then return end
	for _skillId, _ in pairs(passSkillData) do
		local attOneSkill = AttObj:GetOneSkillData(_skillId)
		if attOneSkill then
			local nowTimeNo = GetNowTimeNo()
			local passTimeNo = attOneSkill.PassTimeNo
			if not passTimeNo or passTimeNo == nowTimeNo or (passTimeNo + attOneSkill.CD <= nowTimeNo) then
				local propData = SKILL_PROPDATA[_skillId]
				if not propData then return end 
				for _, _data in pairs(propData) do
					if _data.When == OVER_PASSHIT and _data.RateFunc then
						local rateNum = _data.RateFunc(passSkillId, AttObj)	-- 这里传passSkill为了检测是否概率上中了
						local rand = math.random(10000)
						if rand <= rateNum then
							local logicNo = _data.LogicNo			--判断逻辑
							local logicOk = true
							if logicNo then
								logicNo = logicNo(_skillId, AttObj)
							end
							if logicNo and PROP_LOGICDATA[logicNo] then	
								local logicMethName = PROP_LOGICDATA[logicNo].LogicMeth
								if LogicFuncMap[logicMethName] then
									logicOk = LogicFuncMap[logicMethName](PROP_LOGICDATA[logicNo], AttObj, TarObj)
								end
							end
							if logicOk then
								DoPassEffect(_data, AttObj, TarObj, attSkillId, passSkillId, ...)
							end
						end 			
					end
				end		
			end
		end
	end
end

--处理消息
function ProcessMessage(msgId, AttObj, TarObj, skillId, ...)
	CheckClearStillFightEff(AttObj, TarObj)
	if not skillId then return end 
	local propData = SKILL_PROPDATA[skillId]
	if not propData then return end 
	for _, _data in pairs(propData) do
		if _data.When == msgId and _data.RateFunc then
--			local rateNum = _data.RateFunc or 10000
			local rateNum = _data.RateFunc(skillId, AttObj)
			local rand = math.random(10000)
			if rand <= rateNum then				
				local logicNo = _data.LogicNo			--判断逻辑
				local logicOk = true
				if logicNo then
					logicNo = logicNo(skillId, AttObj)
				end
				
				if logicNo and PROP_LOGICDATA[logicNo] then	
					local logicMethName = PROP_LOGICDATA[logicNo].LogicMeth
					if LogicFuncMap[logicMethName] then
						logicOk = LogicFuncMap[logicMethName](PROP_LOGICDATA[logicNo], AttObj, TarObj)
					end
				end
					
				if logicOk then								
					DoEffect(_data, AttObj, TarObj, ...)
				end
			end 			
		end
	end 
end

function AddNpcProp(AttObj, TarObj, info, propData)
	if not info or not info.NpcNo then return end
	if TarObj and TarObj:IsNpc() and TarObj:GetCharNo() == info.NpcNo then
		for _k, _v in pairs(info) do
			if _k ~= "NpcNo" then
				AddFightEff(AttObj, TarObj, _k, _v, propData)
			end
		end
	end
end

function AddBuffChecker(AttObj, TarObj, buffData, buffInfo, skillLv)
	if TarObj.GetBossType and TarObj:GetBossType() == BOSS_TYPE_WORLD then
		if AttObj and TarObj then
			if AttObj:GetId() ~= TarObj:GetId() then
				return
			end
		end
	end
	if TarObj.GetBossType and TarObj:GetBossType() == BOSS_TYPE_CLUBBOSS then
		if AttObj and TarObj then
			if AttObj:GetId() ~= TarObj:GetId() then
				return
			end
		end
	end
	if buffData.BuffType == BUFF_TYPE1 then			
		if TarObj:IsIMMCONTROL() or TarObj:IsInvincible() then				--免疫控制技能
			return
		end
	end
	if buffData.BuffType == BUFF_TYPE2 then
		if TarObj:IsIMMDEBUFF() or TarObj:IsInvincible() then				--免疫debuff技能
			return
		end
	end
	if buffData.BuffSubType == BUFF_SUBTYPE1 then
		if TarObj:HasBuffType(BUFF_TYPE11) then								--免疫眩晕类buff
			-- 发送Miss提示		
			local protoMsg = {
				att_fid = AttObj:GetFId() or 0,
				fid = TarObj:GetFId(),
				hp = 0,
				nhp = TarObj:GetHp() or 0,
				hp_max = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
				type = HURT_TYPE_04,
				isdie = TarObj:IsDie() and 1 or nil,
			}
			TarObj:SyncNearByPlayer("S2c_aoi_hp", protoMsg)
			
			return
		end
	end
	if buffInfo and buffInfo[buffData.BuffNo] and buffInfo[buffData.BuffNo].buffCheckLv then
		if not buffData.BuffOverType or buffData.BuffOverType == BUFFOVER_TYPE_BIGGERE then
			if skillLv < buffInfo[buffData.BuffNo].buffCheckLv then			--比之前的buff等级低
				return
			end
		elseif buffData.BuffOverType == BUFFOVER_TYPE_NOT then
			return
		elseif buffData.BuffOverType == BUFFOVER_TYPE_BIGGER then
			if skillLv <= buffInfo[buffData.BuffNo].buffCheckLv then		--比之前的buff等级低或者等于
				return
			end
		end
	end
	local propName = buffData.ReBuffPropName
	if propName and propName ~= "" then
		local reRate = TarObj:GetFightValue(propName) or 0
		if reRate > 0 then
			local rNum = math.random(10000)
			if reRate >= rNum then
				return
			end
		end
	end
	
	return not TarObj:IsDie()
end 

--存储所有buff的人
BuffCharTbl = {}
--setmetatable(BuffCharTbl, {__mode = "v"})

function DelNotFightBuff(TarObj, buffNo)
	DelBuff(TarObj, buffNo)
end

function AddNotFightBuff(TarObj, buffMsg)			--可以累加的
	local buffId = buffMsg.id
	local buffType = buffMsg.buffType
	assert(buffType and buffId)
	if buffType == NOTFIGHT_BUFF_TYPE_HP then
		assert(buffMsg.hp)
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			buffInfo[buffId].hp = buffMsg.hp
		else
			local saveData = {
				notFight = true,
				buffType = buffType,
				hp = buffMsg.hp,
				tno = 1,
			}
			buffInfo[buffId] = saveData
			TarObj:SetBuffInfo(buffInfo)
			
			local cbuffTbl = {}
			for _id, _ in pairs(buffInfo) do
				tinsert(cbuffTbl, _id)
			end
			TarObj:SetBuff(cbuffTbl)			--要广播
			
			local charId = TarObj:GetId()
			if not BuffCharTbl[charId] then
				BuffCharTbl[charId] = true
			end
		end
	elseif buffType == NOTFIGHT_BUFF_TYPE_EXP1_5 or buffType == NOTFIGHT_BUFF_TYPE_EXP2_0 or 
			buffType == NOTFIGHT_BUFF_TYPE_EXP4_0 then
		assert(buffMsg.ctime and buffMsg.stime and buffMsg.rate)
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			buffInfo[buffId].ctime = buffMsg.ctime
		else
			local saveData = {
				notFight = true,
				buffType = buffType,
				ctime = buffMsg.ctime,
				stime = buffMsg.stime,
				rate = buffMsg.rate,
			}
			buffInfo[buffId] = saveData
			TarObj:SetBuffInfo(buffInfo)
			
			local cbuffTbl = {}
			for _id, _ in pairs(buffInfo) do
				tinsert(cbuffTbl, _id)
			end
			TarObj:SetBuff(cbuffTbl)			--要广播
			
			local charId = TarObj:GetId()
			if not BuffCharTbl[charId] then
				BuffCharTbl[charId] = true
			end
		end	
	elseif buffType == NOTFIGHT_BUFF_TYPE_NCLUBFLAG or buffType == NOTFIGHT_BUFF_TYPE_CCLUBFLAG or buffType == NOTFIGHT_BUFF_TYPE_VIP then
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			DelBuff(TarObj, buffId)
		end
		local saveData = {
			notFight = true,
			buffType = buffType,
		}
		buffInfo[buffId] = saveData
		TarObj:SetBuffInfo(buffInfo)
		
		local cbuffTbl = {}
		for _id, _ in pairs(buffInfo) do
			tinsert(cbuffTbl, _id)
		end
		TarObj:SetBuff(cbuffTbl)			--要广播
		
		local charId = TarObj:GetId()
		if not BuffCharTbl[charId] then
			BuffCharTbl[charId] = true
		end
	elseif buffType == NOTFIGHT_BUFF_TYPE_WORLDEXP then
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			DelBuff(TarObj, buffId)
		end
		local saveData = {
			notFight = true,
			buffType = buffType,
			worldGrade = buffMsg.worldGrade,
			expRate = buffMsg.expRate,
		}
		buffInfo[buffId] = saveData
		TarObj:SetBuffInfo(buffInfo)
		
		local cbuffTbl = {}
		for _id, _ in pairs(buffInfo) do
			tinsert(cbuffTbl, _id)
		end
		TarObj:SetBuff(cbuffTbl)			--要广播
		
		local charId = TarObj:GetId()
		if not BuffCharTbl[charId] then
			BuffCharTbl[charId] = true
		end
	elseif buffType == NOTFIGHT_BUFF_TYPE_YEGUAIREWARD then
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			DelBuff(TarObj, buffId)
		end
		local saveData = {
			notFight = true,
			buffType = buffType,
			remainTime = buffMsg.remainTime,
		}
		buffInfo[buffId] = saveData
		TarObj:SetBuffInfo(buffInfo)
		
		local cbuffTbl = {}
		for _id, _ in pairs(buffInfo) do
			tinsert(cbuffTbl, _id)
		end
		TarObj:SetBuff(cbuffTbl)			--要广播
		
		local charId = TarObj:GetId()
		if not BuffCharTbl[charId] then
			BuffCharTbl[charId] = true
		end
	elseif buffType == NOTFIGHT_BUFF_TYPE_MERGEACT then
		local buffInfo = TarObj:GetBuffInfo()
		if not buffInfo then
			buffInfo = {}
		end
		if buffInfo[buffId] then
			DelBuff(TarObj, buffId)
		end
		local saveData = {
			notFight = true,
			buffType = buffType,
			remainTime = buffMsg.remainTime,
		}
		buffInfo[buffId] = saveData
		TarObj:SetBuffInfo(buffInfo)
		
		local cbuffTbl = {}
		for _id, _ in pairs(buffInfo) do
			tinsert(cbuffTbl, _id)
		end
		TarObj:SetBuff(cbuffTbl)			--要广播
		
		local charId = TarObj:GetId()
		if not BuffCharTbl[charId] then
			BuffCharTbl[charId] = true
		end
	end
end

function AddBuff(AttObj, TarObj, buffMsg, skillId, notSyncBuff, syncEffect)
	local skillLv = skillId and AttObj:GetMartialLevelBySkillId(skillId) or 1
	if not AttObj or not TarObj then return end
	if not (buffMsg.id and buffMsg.time) then return end
	local buffId = buffMsg.id
	local buffData = BUFF_PROPDATA[buffId]
	if not buffData then return end
	local buffInfo = TarObj:GetBuffInfo()
	if not AddBuffChecker(AttObj, TarObj, buffData, buffInfo, skillLv) then return end 
	local nTimeNo = GetNowTimeNo()
	local effTimeNo = math.floor(buffMsg.time / lua_time_sec)
	local saveData = {
		endTime = nTimeNo + effTimeNo,
		buffType = buffData.BuffType,
		buffSubType = buffData.BuffSubType,
		buffCheckLv = skillLv,
		buffInfo = buffData.BuffInfo,
	}
	local effect = nil
	if buffData.EffectFunc then
		effect = buffData.EffectFunc(skillId, AttObj, TarObj)
	end
	
	if not buffInfo then
		buffInfo = {}
	end	
	
	if effect and effect.ctime == 1 and buffInfo[buffId] then
		buffInfo[buffId].endTime = nTimeNo + effTimeNo
	else
		local isSyncChar = TarObj:GetSyncHpNo()
		local isSyncBuff = false
		if syncEffect then
			effect = syncEffect
			for _k, _v in pairs(effect) do
				saveData[_k] = _v
			end
		elseif effect then
			for _k, _v in pairs(effect) do
				local _value = _v
				if _k == mFIGHT_Hp then
					if _value > 0 then
						local reHpRate = AttObj:GetFightValue(mFIGHT_ReHpRate) or 0
						local reHpValue = AttObj:GetFightValue(mFIGHT_ReHpValue) or 0
						
						local tarReHpRate = TarObj:GetFightValue(mFIGHT_SubReHpRate) or 0
						local tarHpValue = TarObj:GetFightValue(mFIGHT_SubReHpValue) or 0
						_value = _value + reHpValue + tarHpValue
						if _value <= 0 then 
							_value = 0 
						end
--						if reHpRate > 0 then
							_value = _value * (1 + (reHpRate + tarReHpRate) / 10000)
--						end
					end
					saveData["HpTime"] = 0
				elseif isSyncChar and _k == mFIGHT_FixedShield then
					isSyncBuff = true
				end
				saveData[_k] = _value
			end
		end
		saveData.attId = AttObj:GetId()			--防止分线同步buff的时候attId出错
		if buffMsg.exPropData then
			for _k, _v in pairs(buffMsg.exPropData) do
				saveData[_k] = _v
			end
		end
		
		local msgObj = nil
		if TarObj:IsPlayer() then
			msgObj = TarObj
		elseif TarObj:IsPartner() or TarObj:IsMagic() then
			msgObj = TarObj:GetOwner()
		end
	
		if buffInfo[buffId] then
			DelBuff(TarObj, buffId, nil, true)
		end
		
		buffInfo[buffId] = saveData
		TarObj:SetBuffInfo(buffInfo)
		if TarObj:IsNpc() and saveData[mFIGHT_FixedShield] and saveData[mFIGHT_FixedShieldHpMax] then
			TarObj:AddFixedShieldBuffSync(saveData[mFIGHT_FixedShield], saveData[mFIGHT_FixedShieldHpMax], buffId)
		end
		if IsKuaFuServer and TarObj:IsPlayer() and saveData[mFIGHT_FixedShield] and saveData[mFIGHT_FixedShieldHpMax] then
			TarObj:AddFixedShieldBuffSync(saveData[mFIGHT_FixedShield], saveData[mFIGHT_FixedShieldHpMax], buffId)
		end
	
		if msgObj then
			msgObj:SyncNearByPlayer("S2c_aoi_ui_buff", {fid=TarObj:GetFId(), buff_id=buffId, type=2})
		end
		local cbuffTbl = {}
		for _id, _ in pairs(buffInfo) do
			tinsert(cbuffTbl, _id)
		end
		TarObj:SetBuff(cbuffTbl)			--要广播
		
		if skillId then
			TarObj:SetTmp(FIGHT_SHOW_ADDBUFF, true)
		end
		
		if buffData.IsShowLeftTime then
			TarObj:AddBuffTime(buffId, GetNowTimeNo(), effTimeNo)
		end
		
		local charId = TarObj:GetId()
		if not BuffCharTbl[charId] then
			BuffCharTbl[charId] = true
		end
		if saveData[mFIGHT_Speed] then
			if TarObj:IsPlayer() then
				TarObj:SetSpeed(TarObj:GetSpeed())	--修改speed会广播GetFightValue(mFIGHT_Speed)
			end
		end
	
		--如果中的buff是变羊并且是(镜像玩家或者npc则添加ai)
		if buffData.BuffType == BUFF_TYPE6 then
			if TarObj:IsMirrorPlayer() or TarObj:IsNpc() then
				local aiObj = TarObj:GetAI()
				if aiObj then
					if not aiObj.urgentAI then
						local sTime = 4
						aiObj.urgentAI = AI_WALKCSHEEP.clsAIWalkCSheep:New(TarObj, nil, TarObj:GetAIRadius(), 1, sTime, 1, TarObj:GetX(), TarObj:GetY())
					end
				end
			end
		end
		
		--如果是变换buff
		if buffData.BuffType == BUFF_TYPE9 then	
			if TarObj:IsPlayer() then
				TarObj:CheckBreakDaZuo()
			end
			if saveData.IsSiegeWar then
				local addTbl = {}
				for _varName, _varValue in pairs(saveData) do
					local pName = string.match(_varName, "Reset(%a*)")
					if pName then
						if _varName ~= mFIGHT_ResetHpMax and _varName ~= mFIGHT_OldExchangeMartial then
--							saveData["OldReset" .. pName] = TarObj["Get" .. pName](TarObj)
--							TarObj["Set" .. pName](TarObj, _varValue)
							local oValue = TarObj["Get" .. pName](TarObj) or 0
							local diffValue = _varValue - oValue
							addTbl[pName] = diffValue
						end
					end
				end
				for _pName, _value in pairs(addTbl) do
					saveData[_pName] = _value
				end
			else
				saveData.startTimeNo = GetNowTimeNo()
			end
			--修正最大血量
			if saveData[mFIGHT_ResetHpMax] then
				saveData[mFIGHT_OldResetHpMax] = TarObj:GetHpMax()
				saveData.OldHp = TarObj:GetHp()
				TarObj:SetHpMax(saveData[mFIGHT_ResetHpMax])
				local nHpMax = TarObj:GetFightValue(mFIGHT_HpMax)
				if saveData.setHp then
					TarObj:SetHp(saveData.setHp, TarObj:GetId(), nil, HURT_TYPE_13)
				else
					TarObj:SetHp(nHpMax, TarObj:GetId(), nil, HURT_TYPE_13)
				end
			end
			
			--保存就的martial, skill. 然后重新SetMartial
			if saveData[mFIGHT_ExchangeMartial] then
				saveData[mFIGHT_OldExchangeMartial] = TarObj:GetMartialTable()
				TarObj:ClearMartialTable()
				TarObj:SetMartialTable(saveData[mFIGHT_ExchangeMartial])
			end
		end
	end
	
	if saveData[mFIGHT_Shield] then
		TarObj:SetSaveShieldHp(saveData[mFIGHT_Shield])
	end
	if not notSyncBuff then
		SyncAllBuff(TarObj, buffMsg)
	end

	return true
end

function DelBuff(CharObj, buffId, notIsDie, isReplace)
	if not CharObj then return end
	local buffInfo = CharObj:GetBuffInfo()
	if not buffInfo then return end
	local oneInfo = buffInfo[buffId]
	if not oneInfo then return end
	local oBuffInfo = buffInfo[buffId]
	buffInfo[buffId] = nil
	CharObj:SetBuffInfo(buffInfo)
	local cbuffTbl = {}
	for _id, _ in pairs(buffInfo) do
		tinsert(cbuffTbl, _id)
	end	
	CharObj:SetBuff(cbuffTbl)			--要广播
	
	local msgObj = nil
	if CharObj:IsPlayer() then
		msgObj = CharObj
		if oBuffInfo[mFIGHT_Speed] then
			if CharObj:IsPlayer() then
				CharObj:SetSpeed(CharObj:GetSpeed())	--修改speed会广播GetFightValue(mFIGHT_Speed)
			end
		end
	elseif CharObj:IsPartner() or CharObj:IsMagic() then
		msgObj = CharObj:GetOwner()
	end
	
	if msgObj then
		msgObj:SyncNearByPlayer("S2c_aoi_ui_buff", {fid=CharObj:GetFId(), buff_id=buffId, type=1})
	end
	if #cbuffTbl == 0 then
		local charId = CharObj:GetId()
		if BuffCharTbl[charId] then
			BuffCharTbl[charId] = nil
		end
	end
	
	--如果删除的是变羊,要把ai去掉,需要判断ai是否变羊的再去掉
	if oneInfo.buffType == BUFF_TYPE6 then
		local aiObj = CharObj:GetAI()
		if aiObj and aiObj.urgentAI and aiObj.urgentAI.__ClassType == "<csheepai>" then		--变羊的ai
			aiObj.urgentAI = nil
		end
	end
	if not notIsDie and oneInfo[mFIGHT_Shield] then										
		FIGHT_EVENT.ProcessPassMessage(PASS_DAPPSHIELD, CharObj)
		FIGHT_EVENT.ClearFightEff(CharObj, PASS_DAPPSHIELD)		
	end
	if CharObj:IsNpc() and CharObj:GetBossType() == BOSS_TYPE_WORLD and CharObj:GetMapLayer() == 1 then
		lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_WORLDBOSS_DELBUFF,
			bossNo = CharObj:GetCharNo(),
		}))
	end
	
	if CharObj:IsNpc() and CharObj:GetBossType() == BOSS_TYPE_JIERI and CharObj:GetMapLayer() == 1 then
		lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_JIERIBOSS_DELBUFF,
			bossNo = CharObj:GetCharNo(),
			buffId = buffId,
		}))
	end
	
	--帮派boss 消除buff
	if CharObj:IsNpc() and CharObj:GetBossType() == BOSS_TYPE_CLUBBOSS and CharObj:GetMapLayer() == 1 then
		lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_CLUBBOSS_DELBUFF,
			bossNo = CharObj:GetCharNo(),
		}))
	end
	
	--变身buff去除了
	if oneInfo.buffType == BUFF_TYPE9 then
		local lastAttack = CharObj:GetLastAttack()
		
		if oneInfo.IsSiegeWar then
			if CharObj:IsPlayer() then
				local sHp, sHpMax, sBuffId = CharObj:FixedShieldBuffSyncHp()
				lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_SHIFTDEL_SIEGE,
					attId = lastAttack.attId,
					hp = CharObj:GetHp(),
					exNpcId = oneInfo.exNpcId,
					x = CharObj:GetX(),
					y = CharObj:GetY(),
					sHp = sHp or 0,				-- 护盾剩余血量
					mapId = MAP_ID,
					clubId = CharObj:GetClubId(),
				}))	
				
				CharObj:SyncNearByPlayer("S2c_aoi_playershield", {
					fid = CharObj:GetFId(),
					shield_hp = 0,
					shield_hpmax = sHpMax,
					shield_buffid = sBuffId,
				})
			end		
		else
			if CharObj:GetHp() <= 0 then
				if lastAttack.attId then
					lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
						type = RETMAP_SHIFTDEL_TER,
						attId = lastAttack.attId,
					}))
				end
			else
				local startTimeNo = oneInfo.startTimeNo
				local lastAttack = CharObj:GetLastAttack()
				if lastAttack.attId and lastAttack.attId ~= CharObj:GetId() and lastAttack.attTime and lastAttack.attTime >= startTimeNo then
					if GetNowTimeNo() - lastAttack.attTime <= 50 then	--5秒内
						lretmap.other(CharObj:GetId(), MAP_ID, CharObj:GetMapLayer(), lserialize.lua_seri_str({
							type = RETMAP_SHIFTDEL_TER,
							attId = lastAttack.attId,
						}))
					end
				end		
			end
		end
		
--		for _varName, _varValue in pairs(oneInfo) do
--			local pName = string.match(_varName, "OldReset(%a*)")
--			if pName then
--				if _varName ~= mFIGHT_OldResetHpMax and _varName ~= mFIGHT_OldExchangeMartial then
--					CharObj["Set" .. pName](CharObj, _varValue)
--				end
--			end
--		end		

		--修正最大血量
		if oneInfo[mFIGHT_OldResetHpMax] then
			CharObj:SetHpMax(oneInfo[mFIGHT_OldResetHpMax])
			CharObj:SetHp(oneInfo.OldHp, CharObj:GetId(), nil, HURT_TYPE_13)
		end
		
		--保存就的martial, skill. 然后重新SetMartial
		if oneInfo[mFIGHT_OldExchangeMartial] then
			CharObj:ClearMartialTable()
			CharObj:SetMartialTable(oneInfo[mFIGHT_OldExchangeMartial])
		end
	end
	
	CharObj:DelBuffTime(buffId)
	
	if oneInfo.DelEvent then
		if oneInfo.DelEvent.skill and not notIsDie and not isReplace then		--出技能
			if CharObj:IsPlayer() then
				TryCall(FIGHT.UseSkillHit, CharObj, nil, oneInfo.DelEvent.skill, CharObj:GetX(), CharObj:GetY())
			elseif CharObj:IsNpc() then
				local aiObj = CharObj:GetAI()
				if aiObj then
					if not aiObj.npcExtSkillAI then
						aiObj.npcExtSkillAI = AI_AIATTACKEXTSKILL.clsAIAttackExtSkill:New(CharObj, oneInfo.DelEvent.skill)
					end
				end
			end
		end
	end
	
	return true
end

--local _BUFFTYPE_TBL = {
--	[BUFF_TYPE1] = true,
--	[BUFF_TYPE2] = true,
--	[BUFF_TYPE3] = true,
--	[BUFF_TYPE4] = true,
--}

function DelBuffType(AttObj, TarObj, buffType)
--	if not _BUFFTYPE_TBL[buffType] then return end
	local buffInfo = TarObj:GetBuffInfo()
	if buffInfo then
		local delTbl = {}
		for _buffId, _data in pairs(buffInfo) do
			if _data.buffType == buffType then
				tinsert(delTbl, _buffId)
			end
		end
		for _, _buffId in pairs(delTbl) do
			DelBuff(TarObj, _buffId)
		end
	end		
end

function DelAllBuff(CharObj)
	local buffInfo = CharObj:GetBuffInfo()
	if buffInfo then
		local delTbl = {}
		for _buffId, _data in pairs(buffInfo) do
			tinsert(delTbl, _buffId)
		end
		for _, _buffId in pairs(delTbl) do
			DelBuff(CharObj, _buffId, true)
		end
		CharObj:SetBuff(nil)
	end	
end

function DelAllFightBuff(CharObj)
	local buffInfo = CharObj:GetBuffInfo()
	if buffInfo then
		local delTbl = {}
		for _buffId, _data in pairs(buffInfo) do
			if not _data.notFight then
				if _buffId ~= EVIL_BUFFID and _buffId ~= EVIL_BUFFID_CLIENT then		--红名buff不删
					tinsert(delTbl, _buffId)
				end
			end
		end
		for _, _buffId in pairs(delTbl) do
			DelBuff(CharObj, _buffId, true)
		end
	end	
end

function AddFightEff(AttObj, CharObj, key, value, propData)
	if CharObj:IsDie() then return end
	if key == mFIGHT_STILLPROPHP then
		local effData = CharObj:GetTmp(FIGHT_STILLEFF_NAME) or {}
		for _k, _v in pairs(value) do
			if _k ~= "true_type" and _k ~= "comp_type" and _k ~= "DelHpMaxRate" and _k ~= "DelHpVaule" then
				if #effData < 1 then
					tinsert(effData, value)
					CharObj:SetTmp(FIGHT_STILLEFF_NAME, effData)
					if _k == "Buff" then
						AddBuff(AttObj, CharObj, _v, propData.ID)
					end
				else
					local isOk = true
					for _, _tv in pairs(effData) do
						if _tv[_k] and _k ~= "Buff" then				--同属性不能叠加
							isOk = false
							break
						end
					end
					if isOk then
						tinsert(effData, value)
						CharObj:SetTmp(FIGHT_STILLEFF_NAME, effData)
						if _k == "Buff" then
							AddBuff(AttObj, CharObj, _v, propData.ID)
						end
					end
				end
				break
			end
		end
	else
		local effData = CharObj:GetTmp(FIGHT_EFF_NAME) or {}
		local when = propData.When
		local oneData = effData[when]
		if not oneData then
			oneData = {}
		end
		if oneData[key] then
			if type(value) == mNUMBERTYPE then
				oneData[key] = oneData[key] + value
			else
				oneData[key] = value
			end
		end
		oneData[key] = value
		effData[when] = oneData
		CharObj:SetTmp(FIGHT_EFF_NAME, effData)
		
		if key == mFIGHT_Speed then
			if CharObj:IsPlayer() then
				CharObj:SetSpeed(CharObj:GetSpeed())	--修改speed会广播GetFightValue(mFIGHT_Speed)
			end
		end		
	end
end

function CheckClearStillFightEff(CharObj, TarObj)
	if not CharObj then return end
	local stillEffData = CharObj:GetTmp(FIGHT_STILLEFF_NAME)
	if stillEffData then
		for _idx, _oneData in pairs(stillEffData) do
			local sType = _oneData.true_type
			local cType = _oneData.comp_type
			local delHpValue = _oneData.DelHpVaule
			local delHpMaxRate = _oneData.DelHpMaxRate
			if sType and cType then
				local panduanFunc = panduan[sType]
				if panduanFunc then
					if cType == 1 then
						if delHpValue then
							local hp = CharObj:GetHp() or 0
							if panduanFunc(hp, delHpValue) then
								stillEffData[_idx] = nil
								if _oneData.Buff and _oneData.Buff.id then
									DelBuff(CharObj, _oneData.Buff.id)
								end
							end
						end
					elseif cType == 2 then
						if delHpValue and TarObj then
							local hp = TarObj:GetHp() or 0
							if panduanFunc(hp, delHpValue) then
								stillEffData[_idx] = nil
								if _oneData.Buff and _oneData.Buff.id then
									DelBuff(CharObj, _oneData.Buff.id)
								end
							end
						end						
					elseif cType == 3 then
						if TarObj then
							local aHp = AttObj:GetHp() or 0
							local hp = TarObj:GetHp() or 0
							if panduanFunc(aHp, hp) then
								stillEffData[_idx] = nil
								if _oneData.Buff and _oneData.Buff.id then
									DelBuff(CharObj, _oneData.Buff.id)
								end
							end
						end							
					elseif cType == 4 then
						if delHpMaxRate then
							local hp = CharObj:GetHp() or 0
							local hpMax = CharObj:GetFightValue(mFIGHT_HpMax) or 0
							
							if panduanFunc(hp, hpMax * delHpMaxRate / 100) then
								stillEffData[_idx] = nil
								if _oneData.Buff and _oneData.Buff.id then
									DelBuff(CharObj, _oneData.Buff.id)
								end
							end			
						end			
					elseif cType == 5 then
						if delHpMaxRate and TarObj then
							local hp = TarObj:GetHp() or 0
							local hpMax = TarObj:GetFightValue(mFIGHT_HpMax) or 0
							
							if panduanFunc(hp, hpMax * delHpMaxRate / 100) then
								stillEffData[_idx] = nil
								if _oneData.Buff and _oneData.Buff.id then
									DelBuff(CharObj, _oneData.Buff.id)
								end
							end
						end
					end
				end
			end
		end
	end
end

function ClearFightEff(CharObj, when)
	if not CharObj then return end
	if not when then
		CharObj:SetTmp(FIGHT_EFF_NAME, nil)
		return
	end
	local effData = CharObj:GetTmp(FIGHT_EFF_NAME) or {}
	local oneData = effData[when]
	effData[when] = nil
	CharObj:SetTmp(FIGHT_EFF_NAME, effData)
	if CharObj:IsPlayer() and oneData and oneData[mFIGHT_Speed] then
		CharObj:SetSpeed(CharObj:GetSpeed())	--修改speed会广播GetFightValue(mFIGHT_Speed)
	end
end

function GetFightEff(CharObj, key)
	local value = nil

	local effData = CharObj:GetTmp(FIGHT_EFF_NAME)
	if effData then
		for _when, _oneData in pairs(effData) do
			local v = _oneData[key]
			if v then
				if type(v) == mNUMBERTYPE then
					value = (value or 0) + v
				else
					return v
				end
			end
		end
	end
	
	local stillEffData = CharObj:GetTmp(FIGHT_STILLEFF_NAME)
	if stillEffData then
		for _, _oneData in pairs(stillEffData) do
			local v = _oneData[key]
			if v then
				if type(v) == mNUMBERTYPE then
					value = (value or 0) + v
				else
					return v
				end
			end			
		end
	end
	
	return value
end

--{
--	[nextTimeNo] = {
--		[1] = {
--			att_id = ,
--			tar_id = ,
--			skillId = ,
--			mxCenter = ,
--			myCenter = ,
--			axyz = ,
--		}
--	}
--}
StartSkill = {}
START_SKILL_RID = 0
function GetStartSkillRid()
	START_SKILL_RID = START_SKILL_RID + 1
	return START_SKILL_RID
end

function OnAoiSkillCalInfo(UserObj, protoMsg)
	local s_rid = protoMsg.s_rid
	local checktime = protoMsg.checktime
	
	local contData = StartSkill[checktime]
	if not contData then return end	
	local rmIdx = nil
	for _idx, _data in pairs(contData) do
		if _data.s_rid == s_rid then
			rmIdx = _idx
		end
	end
	if rmIdx then
		local _data = contData[rmIdx]
		tremove(contData, rmIdx)
		
		local AttObj = CHAR_MGR.GetCharById(_data.att_id)
		local TarObj = nil
		local canTarHit = true
		if _data.tar_id then
			TarObj = CHAR_MGR.GetCharById(_data.tar_id)
			if not TarObj or TarObj:IsDie() then
				canTarHit = false
			end
		end
		if canTarHit and AttObj and not AttObj:IsDie() and _data.skillId then
			TryCall(FIGHT.UseSkillHit, AttObj, TarObj, _data.skillId, _data.mxCenter, _data.myCenter, _data.axyz, protoMsg.timestamp or _data.timestamp)
		end
	end
end

function AddStartSkill(AttObj, TarObj, skillId, mxCenter, myCenter, effTimeNo, s_rid, axyz, timestamp)
	local tmp = {
		att_id = AttObj:GetId(),
		tar_id = TarObj and TarObj:GetId() or nil,
		skillId = skillId,
		mxCenter = mxCenter,
		myCenter = myCenter,
		s_rid = s_rid,
		axyz = axyz,
		timestamp = timestamp,
	}
	
	local nTimeNo = GetNowTimeNo()
	local nextTimeNo = nTimeNo + effTimeNo
	local data = StartSkill[nextTimeNo] or {} 
	tinsert(data, tmp)
	StartSkill[nextTimeNo] = data
end

function CheckStartSkill(noTimeNo)
	local contData = StartSkill[noTimeNo]
	if not contData then return end
	
	StartSkill[noTimeNo] = nil
	for _, _data in pairs(contData) do
		local AttObj = CHAR_MGR.GetCharById(_data.att_id)
		local TarObj = nil
		local canTarHit = true
		if _data.tar_id then
			TarObj = CHAR_MGR.GetCharById(_data.tar_id)
			if not TarObj or TarObj:IsDie() then
				canTarHit = false
			end
		end
		if not canTarHit then
			if AttObj and (AttObj:IsPartner() or AttObj:IsMagic()) then			--清空技能cd时间
				AttObj:ClearAllSkillCD()
			end
		end
		
		if canTarHit and AttObj and not AttObj:IsDie() and _data.skillId then
			TryCall(FIGHT.UseSkillHit, AttObj, TarObj, _data.skillId, _data.mxCenter, _data.myCenter, _data.axyz, _data.timestamp)
		end
	end
end

local function GetNotFightBuffAddHpMax(CharObj)
	local grade = CharObj:GetGrade() or 1
	if grade < 1 then grade = 1 end
	return (grade - 1) * 40 + 140
end

local FIVE_SEC_BUFFNO = {
	[40000421] = true,
	[40000431] = true,
	[40000441] = true,
	[40000451] = true,
}

function CheckBuff(nTimeNo)
	for _id, _ in pairs(BuffCharTbl) do
		local _CharObj = CHAR_MGR.GetCharById(_id)
		if _CharObj then		--有可能不一样,复活了的同伴
			local buffInfo = _CharObj:GetBuffInfo()
			if buffInfo then
				local delTbl = {}
				for _buffId, _data in pairs(buffInfo) do
					if _data.notFight then		--非战斗buff
						if _data.buffType == NOTFIGHT_BUFF_TYPE_HP then
							if _data.hp <= 0 then
								tinsert(delTbl, _buffId)
							else
								if not _CharObj:IsDie() and _data["tno"] % 10 == 0 then			--5秒一次加血
									local hp = _CharObj:GetHp() or 0
									local hpMax = _CharObj:GetFightValue(mFIGHT_HpMax) or 1
									if hp < hpMax then
										if not _CharObj:IsShapeshift() then			--变身不加
											local addHp = hpMax - hp
											local maxHp = GetNotFightBuffAddHpMax(_CharObj)
											addHp = addHp >= maxHp and maxHp or addHp
											
											if addHp >= _data.hp then
												addHp = _data.hp
												_data.hp = 0
												_CharObj:AddHp(addHp, _id)		--广播的
												tinsert(delTbl, _buffId)
											else
												_data.hp = _data.hp - addHp
												_CharObj:AddHp(addHp, _id)			--广播的
											end
											
											if IsServer() then
												lretmap.other(_id, MAP_ID, _CharObj:GetMapLayer(), lserialize.lua_seri_str({
													type = RETMAP_NOTFIGHT_BUFFHP,
													subhp = addHp,
												}))		
											else
												lretmap.other({
													type = RETMAP_NOTFIGHT_BUFFHP,
													buffhp = {
														subhp = addHp,
													},
												})		
											end	
										end
									end
								end
								_data["tno"] = _data["tno"] + 1
							end
						elseif _data.buffType == NOTFIGHT_BUFF_TYPE_EXP1_5 or _data.buffType == NOTFIGHT_BUFF_TYPE_EXP2_0 or 
								_data.buffType == NOTFIGHT_BUFF_TYPE_EXP4_0 then
							if os.time() >= _data.ctime + _data.stime then
								tinsert(delTbl, _buffId)
							end
						end
					else
						if _data.endTime < nTimeNo then
							tinsert(delTbl, _buffId)
						elseif _data[mFIGHT_Hp] then	
							if FIVE_SEC_BUFFNO[_buffId] then
								if _data["HpTime"] % 10 == 0 then
									_CharObj:AddHp(_data[mFIGHT_Hp], _data.attId, nil, HURT_TYPE_09)			--广播的
								end								
							else
								if _data["HpTime"] % 2 == 0 then
									_CharObj:AddHp(_data[mFIGHT_Hp], _data.attId, nil, HURT_TYPE_09)			--广播的
								end
							end
							_data["HpTime"] = _data["HpTime"] + 1
						elseif _data[mFIGHT_RoundAddBuff] then
							local buffData = _data[mFIGHT_RoundAddBuff]	
							if buffData.r and buffData.buffNo and buffData.addBuffTime and buffData.attKind then
								local attKind = buffData.attKind
								local tr = buffData.r
								local dtr = tr ^ 2
								local rx, ry = _CharObj:GetX(), _CharObj:GetY()
								local buffMsg = {id = buffData.buffNo, time = buffData.addBuffTime}
								
								for i = rx - tr, rx + tr do
									for j = ry - tr, ry + tr do
										if not BASECHAR.IsBlockPoint(i, j) then
											if (i - rx) ^ 2 + (j - ry) ^ 2 <= dtr then
												local tCharTbl = FIGHT.GetXYBattleCharObjs(_CharObj:GetMapLayer(), i, j)
												if tCharTbl then
													for _id, _tCharObj in pairs(tCharTbl) do
														if FIGHT.CheckTarget(_CharObj, _tCharObj, attKind) then		--测试是否需要加buff
															AddBuff(_CharObj, _tCharObj, buffMsg)
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
				for _, _buffId in pairs(delTbl) do
					DelBuff(_CharObj, _buffId)
				end
			end
		end
	end 
end

FlyData = {}
function AddFlyPlayer(UserObj, eTimeNo)
	local id = UserObj:GetId()
	local edata = FlyData[eTimeNo] or {}
	edata[id] = true
	FlyData[eTimeNo] = edata
end
function DelFlyPlayer(UserObj, eTimeNo)
	local edata = FlyData[eTimeNo]
	if edata then
		edata[UserObj:GetId()] = nil
	end
end
function CheckFlyPlayer(nTimeNo)
	local ndata = FlyData[nTimeNo]
	if ndata then
		for _id, _ in pairs(ndata) do
			local UserObj = CHAR_MGR.GetCharById(_id)
			if UserObj then
				UserObj:ClearFlyData()
			end
		end
		FlyData[nTimeNo] = nil
	end
end

FlyDodge = {}
function SetFlyDodgeCoolTime(UserObj, eTimeNo)
	local id = UserObj:GetId()
	FlyDodge[id] = eTimeNo
end
function DelFlyDodgeCoolTime(UserObj)
	FlyDodge[UserObj:GetId()] = nil
end
function CheckFlyDodgeCoolTime(nTimeNo)
	for _id, _eTimeNo in pairs(FlyDodge) do
		if nTimeNo >= _eTimeNo then
			local UserObj = CHAR_MGR.GetCharById(_id)
			if UserObj then
				UserObj:AddFlyDodge()
			end			
		end
	end
end

EvilYellowData = {}
function AddEvilYellowData(UserObj)
	local id = UserObj:GetId()
	EvilYellowData[id] = true
end

function DelEvilYellowData(UserObj)
	local id = UserObj:GetId()
	EvilYellowData[id] = nil	
end

function CheckEvilYellow(nTimeNo)
	for _id, _ in pairs(EvilYellowData) do
		local UserObj = CHAR_MGR.GetCharById(_id)
		if UserObj then
			local eState = UserObj:GetEvilState()
			if eState == EVILSTATE_YELLOW then
				local t = UserObj:GetEvilYellowTime()
				if not t or (t + EVIL_COOLTIME < nTimeNo) then		--时间都了设置成白名单
					UserObj:SetEvilState(EVILSTATE_WHITE)
				end
			else
				EvilYellowData[_id] = nil	
			end
		else
			EvilYellowData[_id] = nil	
		end
	end
end

DaZuoData = {}
function AddDaZuo(UserObj)
	if UserObj:GetTmp("DaZuoTimeNo") then return end
	local nTimeNo = GetNowTimeNo()
	local nextTimeNo = nTimeNo + DAZUO_REWARDTIME_NO
	UserObj:SetTmp("DaZuoTimeNo", nextTimeNo)
	local t = DaZuoData[nextTimeNo] or {}
	t[UserObj:GetId()] = true
	DaZuoData[nextTimeNo] = t
end

function DelDaZuo(UserObj)
	local nextTimeNo = UserObj:GetTmp("DaZuoTimeNo")
	if not nextTimeNo then return end
	UserObj:SetTmp("DaZuoTimeNo", nil)
	local t = DaZuoData[nextTimeNo]
	if not t then return end
	t[UserObj:GetId()] = nil
end

function CheckDaZuo(nTimeNo)
	local ndata = DaZuoData[nTimeNo]
	local nextTimeNo = nTimeNo + DAZUO_REWARDTIME_NO
	if ndata then
		for _id, _ in pairs(ndata) do
			local UserObj = CHAR_MGR.GetCharById(_id)
			if UserObj then
				local state = UserObj:GetDaZuo()
				if state == DAZUO_STATE then
					UserObj:SetTmp("DaZuoTimeNo", nextTimeNo)
					local nextTbl = DaZuoData[nextTimeNo]
					if not nextTbl then
						nextTbl = {}
						DaZuoData[nextTimeNo] = nextTbl
					end
					nextTbl[UserObj:GetId()] = true
					
					--发送经验
					if IsServer() then
						lretmap.other(UserObj:GetId(), MAP_ID, UserObj:GetMapLayer(), lserialize.lua_seri_str({
							type = RETMAP_DAZUO,
							state = 1,
							aexp = 1,
						}))	
					else
						lretmap.other({
							type = RETMAP_DAZUO,
							dazuo = {
								state = 1,
								aexp = 1,
							},
						})	
					end
					--加血
					local hpMax = UserObj:GetFightValue(mFIGHT_HpMax)
					local addHp = mfloor(hpMax * 0.05)
					UserObj:AddHp(addHp, UserObj:GetId(), nil, HURT_TYPE_10)
				else
					UserObj:SetTmp("DaZuoTimeNo", nil)
				end
			end
		end
		DaZuoData[nTimeNo] = nil
	end	
end

function lua_EventClear()
	START_SKILL_RID = 0
	StartSkill = {}
	BuffCharTbl = {}
	PassMsgProcess = {}
	ExtraSkill = {}
	FlyData = {}
	FlyDodge = {}
	DaZuoData = {}
	EvilYellowData = {}
end

function lua_PataClear()
	StartSkill = {}
	ExtraSkill = {}
end

function __init__()
	func_call.C2s_aoi_skill_calinfo = OnAoiSkillCalInfo
--	print("___", MAP_ID, MAP_NO, collectgarbage("count"), os.time())
end