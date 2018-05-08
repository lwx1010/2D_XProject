local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local mrandom=math.random
local debug = debug
local debugt = debug.traceback
local tinsert = table.insert
local lserialize = lserialize

--Data是序列化的数据(字符串)
local function UnSerialize(data)
	return assert(loadstring("return "..data))()
end

------------------------------------------------------------处理协议-------------------------------------------------------
function lua_ClientProto(vfd, proto_id, proto_data)
	local proto_name = GET_PROTO_NAME(proto_id)
	if not proto_name then return end
	
	local protoMsg = nil
	local function TryParseData(proto_data)
		protoMsg = PBC.decode(proto_name, proto_data)
		return true		
	end 
	local UserObj = CHAR_MGR.GetUserByVfd(vfd)
	if not UserObj then return end
	local ni, err = xpcall(function () return TryParseData(proto_data) end, debugt)
	if not ni then
		_RUNTIME_ERROR("lua_ClientProto Error1",proto_id, proto_name, err)
		return 
	end
	if not protoMsg then 
		_RUNTIME_ERROR("lua_ClientProto Error2", proto_id, proto_name)
		return 
	end
	if func_call[proto_name] then
		func_call[proto_name](UserObj, protoMsg)
	end
end

function lua_ClientProto2(proto_name, proto_data)
	local UserObj = CHAR_MGR.GetRandomUserObj(1)
	if not UserObj then return end
	if func_call[proto_name] then
		func_call[proto_name](UserObj, proto_data)
	end
end

------------------------------------------------------------更新文件-------------------------------------------------------
function lua_UpdateFile(updateFile)
    if not string.endswith(updateFile, ".lua") then
    	updateFile = updateFile..".lua"
    end	
	if string.sub(updateFile,1,1) == "/" then 
		updateFile = string.sub(updateFile,2,-1)
	end
	if posix.stat(updateFile) then
		local ret, err = Update(updateFile)
		_RUNTIME_ERROR("update file:", updateFile, ret, err)
	end
end

-----------------------------------------------------------删除该地图-------------------------------------------------------
local function GetLeavePosStr(tmpToMapInfo)
	local oneData = tmpToMapInfo[mrandom(#tmpToMapInfo)]
	local range = (oneData.range or 10) * 2
	local sr = range
	for i = 1, 20 do
		local ax = mrandom(range) - sr
		local ay = mrandom(range) - sr
		local z = lmapdata.getz(MAP_NO, oneData.toX + ax, oneData.toY + ay)
		if z then
			return {
				toMapId = oneData.toMapId,
				toX = oneData.toX + ax,
				toY = oneData.toY + ay,
				toZ = z,
			}
		end
	end
	return lserialize.lua_seri_str(oneData)
end

function lua_DeleteState(extDataStr)
	local tmpToMapInfo = UnSerialize(extDataStr) or {}
	local maxToMapNo = #tmpToMapInfo

	local allCharObj = CHAR_MGR.GetAllLayerCharObj()
	for _id, _CharObj in pairs(allCharObj) do					--目前只需要返回给玩家,静态npc,物品给主逻辑删除对象
		if _CharObj:IsPlayer() then
			TryCall(_CharObj.LeaveMap, _CharObj, true)
	--		_CharObj:LeaveMap(true)		--发给客户端离开地图
			if maxToMapNo == 0 then
				if _CharObj:IsMirrorPlayer() then
					lretmap.static_npcleave(_CharObj:GetId(), MAP_ID, _CharObj:GetMapLayer())
				else
					lretmap.userdmleave(_CharObj:GetVfd(), MAP_ID, _CharObj:GetMapLayer(), "nil")
				end
			else
				if _CharObj:IsMirrorPlayer() then
					lretmap.static_npcleave(_CharObj:GetId(), MAP_ID, _CharObj:GetMapLayer())
				else
					lretmap.userdmleave(_CharObj:GetVfd(), MAP_ID, _CharObj:GetMapLayer(), GetLeavePosStr(tmpToMapInfo))	
				end
			end		
		elseif _CharObj:IsStaticNpc() then
			lretmap.static_npcdmleave(_CharObj:GetId(), MAP_ID, _CharObj:GetMapLayer())
		elseif _CharObj:IsItem() then
			lretmap.itemdmleave(_CharObj:GetId(), MAP_ID, _CharObj:GetMapLayer())
		elseif _CharObj:IsNpc() then
			lretmap.static_npcdmleave(_CharObj:GetId(), MAP_ID, _CharObj:GetMapLayer())
		end
	end 
end

------------------------------------------------------------添加同伴,战斗npc-----------------------------------------------
local function CreateMartialTable(Martials, MartialTable)
	local MartialTable = MartialTable or {}
	for _,MartialData in pairs(Martials) do
		local Martial = SKILL_DATA.GetMartial(MartialData[1])
		if Martial then
			table.insert(MartialTable, {Lv=MartialData[2],MartialId=MartialData[1]})
		else
			_RUNTIME_ERROR("no this Martial:", MartialData[1])
		end
	end
	return MartialTable
end

function lua_AddNpcByTable(extData, syncNo , hpRate)
	local npcData = extData.npcData
	local syncData = extData.syncData	
	local x, y = npcData.x, npcData.y
	if not BASECHAR.IsBlockPoint(x, y) then
		local z = lmapdata.getz(MAP_NO, x, y)
		npcData.z = z
		local layer = npcData.MapLayer
		local npcObj = NPC.clsNpc:New(x, y, z, syncData, npcData, layer, hpRate)
		AddCharObjToAITbl(npcObj)
		if npcObj and syncNo then
			AddSyncHp(npcObj, syncNo)
		end
		return npcObj
	else
		error(string.format("pos error!! mapNo:%s, x:%s, y:%s", MAP_NO, x, y))
	end
end

function lua_AddNpcInClient(npcNo, x, y, bossHpMax, hpRate)
	local allNpcData = NPC_BATTLE_DATA.GetAllNpcData()
	local oneData = allNpcData[npcNo]
	if not oneData then
		error(string.format("__not npc_no:%s in map_no:%s__", npcNo, MAP_NO))
	end
	local id = CHAR_MGR.NewId()
	local npcData = {
		x=x,y=y,z=lmapdata.getz(MAP_NO, x, y),Id=id,
		AIRadius=oneData.AIRadius or NORMAL_NPC_RADIUS,
		CharNo=oneData.NpcNo or 0,
		AITrackRange=oneData.AITrackRange,
		BossType=oneData.BossType or 0,
		MartialTable=CreateMartialTable(oneData.Martials),
		Score=oneData.Score or 0,		
		AtkTime=oneData.AtkTime or AI_ATTACK_TIME,
		IsActiveAttack=oneData.IsActiveAttack or 0,
		CanPush=oneData.CanPush or CAN_PUSH,
		WalkGrid=oneData.WalkGrid or 1,
		PatrolRange=oneData.PatrolRange or NPC_PATROLRANGE,
		KReward=oneData.KReward,
		AReward=oneData.AReward,
		ExpReward=oneData.ExpReward,
		MapLayer = 1,
		Grade = oneData.Grade or 1,
		LimitHpHurt=oneData.LimitHpHurt,
		IsHurtFinish=oneData.IsHurtFinish,
		InitBuff=oneData.InitBuff,
		
		AIPatrol=oneData.AIPatrol or 0,
		LimitDropHp=oneData.LimitDropHp,
		SpNotHurt=oneData.SpNotHurt,
		
		FubenTriggerType=oneData.FubenTriggerType,
		FubenTalkNo=oneData.FubenTalkNo,
	}
	
	if IsClient() then
		local boxNpcObj = CHECKEND.HasBoxNpc(1)
		if boxNpcObj then
			npcData.HitDelNpcId = boxNpcObj:GetId()
		end
	end
	
	local WorldGradeAttr = oneData.WorldGradeAttr and oneData.WorldGradeAttr[_WORLD_GRADE] or {}
	
	for _, _varName in pairs(WARRIOR_DATA_NAME) do
		if _varName ~= "Id" then 
			if _varName == mFIGHT_Hp or _varName == mFIGHT_HpMax then
				if bossHpMax then
					npcData[_varName] = bossHpMax
				else
					npcData[_varName] = WorldGradeAttr[mFIGHT_HpMax] or oneData[mFIGHT_HpMax]
				end
			else
				npcData[_varName] = WorldGradeAttr[_varName] or oneData[_varName]
			end
		end
	end
	local propRate = CHECKEND.GetNpcCreatePropRate()
	if propRate ~= 0 then
		npcData[mFIGHT_Ap] = npcData[mFIGHT_Ap] * (1 + propRate / 100)
		npcData[mFIGHT_Dp] = npcData[mFIGHT_Dp] * (1 + propRate / 100)
		
		npcData[mFIGHT_HpMax] = npcData[mFIGHT_HpMax] * (1 + propRate / 100)
		npcData[mFIGHT_Hp] = npcData[mFIGHT_Hp] * (1 + propRate / 100)
	end
	local dir = oneData.Dir or math.random(8)

	local extData = {
		npcData = npcData,
		syncData = {
			rid = id,
			name=oneData.Name, 
			grade=oneData.Grade or 1,
			shape=oneData.Shape,
			dir=dir,
		},
	}
	npcData.Dir = dir
	return lua_AddNpcByTable(extData, nil , hpRate)
end

function lua_AddNpc(data)
	data.npcData.npcData.MainNpc = true
	local npcObj = lua_AddNpcByTable(data.npcData, data.syncNo)
	if npcObj and data.addBuffData then
		for _, _buffData in pairs(data.addBuffData) do
			FIGHT_EVENT.AddBuff(npcObj, npcObj, _buffData)
		end
	end
	return true
end

function lua_AddYewaiBossNpc(data)
	data.npcData.npcData.MainNpc = true
	local extData = data.npcData
	local syncNo = data.syncNo
	local npcData = extData.npcData
	local syncData = extData.syncData	
	local x, y = npcData.x, npcData.y
	if not BASECHAR.IsBlockPoint(x, y) then
		local z = lmapdata.getz(MAP_NO, x, y)
		local layer = npcData.MapLayer
		local npcObj = YEWAIBOSSNPC.clsYewaiBossNpc:New(x, y, z, syncData, npcData, layer, data.minPos, data.maxPos, data.bossScore)
		AddCharObjToAITbl(npcObj)
		
		if npcObj and syncNo then
			AddSyncHp(npcObj, syncNo)
		end
		return npcObj
	else
		error(string.format("pos error!! mapNo:%s, x:%s, y:%s", MAP_NO, x, y))
	end
	return true
end

function lua_AddSecretBossNpc(data)
	data.npcData.npcData.MainNpc = true
	local extData = data.npcData
	local syncNo = data.syncNo
	local npcData = extData.npcData
	local syncData = extData.syncData	
	local x, y = npcData.x, npcData.y
	if not BASECHAR.IsBlockPoint(x, y) then
		local z = lmapdata.getz(MAP_NO, x, y)
		local layer = npcData.MapLayer
		local npcObj = SECRETBOSSNPC.clsSecretBossNpc:New(x, y, z, syncData, npcData, layer, data.minPos, data.maxPos)
		AddCharObjToAITbl(npcObj)
		
		if npcObj and syncNo then
			AddSyncHp(npcObj, syncNo)
		end
		return npcObj
	else
		error(string.format("pos error!! mapNo:%s, x:%s, y:%s", MAP_NO, x, y))
	end
	return true
end

function lua_MoveNpc(data)
	local npcObj = CHAR_MGR.GetCharById(data.npc_id)
	if npcObj then
		return npcObj:JumpTo(data.x, data.y, data.z)
	end
	return false
end

function lua_MoveUserTo(data)
	local UserObj = CHAR_MGR.GetCharById(data.id)
	if not UserObj then return end
	
	local ret = UserObj:UserJumpTo(data.x, data.y, data.z)
	if ret and ret > 0 then
		return true
	else
		_RUNTIME_ERROR("engine jumpto error:", UserObj:GetName(), ret)
		return false
	end
end

function lua_SeeNpc(data)
	local npcObj = CHAR_MGR.GetCharById(data.npc_id)
	if npcObj then
		return npcObj:SetCanSee(data.see_type)
	end
	return false
end

function lua_CheckIsFly(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if UserObj then
		return UserObj:CheckIsFly()
	end
	return false
end

------------------------------------------------------------玩家函数-------------------------------------------------------
function lua_AddUserToMapByTbl(vfd, extData)	--报错，让引擎层返回错误
	if CHAR_MGR.HasUserObjByVfd(vfd) then 
		error("has vfd in map")
	end
	
	local userData = extData.userData
	local syncData = extData.syncData
	local layer = userData.MapLayer
	
	local userObj = USER.clsUser:New(vfd, userData.x, userData.y, userData.z, syncData, userData, layer)
	
	if userObj:IsDie() then				--创建就是死亡的发送
		lretmap.die(userObj:GetId(), MAP_ID, userObj:GetMapLayer(), "")
	else
		if userData.ThugModel then
			local partnerObj = PARTNER.clsPartner:New({
				MartialTable = userData.PartnerMartial,
				LingqiModel = userData.LingqiModel,
				ThugModel = userData.ThugModel,
			}, userObj:GetId(), layer)
		end
		if userData.LingqiModel then
			local magicObj = MAGIC.clsMagic:New({MartialTable = userData.MagicMartial}, userObj:GetId(), layer)
		end
		
		--华山论剑添加技能
		if MAP_NO == 1086 then
			pbc_send_msg(userObj:GetVfd(), "S2c_aoi_skill_add", {
				id = userObj:GetId(),
				uiskill ={{
					skill_id = 40000301,
					skill_type = 2,
					skill_lv = 1,}},
			})
			userObj:AddMartial(4000030, 1, userData.IsJoinHuaShan)
		end
		--杀戮战场添加技能
		if MAP_NO == 1159 then
			pbc_send_msg(userObj:GetVfd(), "S2c_aoi_skill_add", {
				id = userObj:GetId(),
				uiskill ={{
					skill_id = 40000301,
					skill_type = 2,
					skill_lv = 1,}},
			})
			userObj:AddMartial(4000030, 1)
		end
	end
end

function lua_AddUserToMap(vfd, extDataStr)	--报错，让引擎层返回错误
	if CHAR_MGR.HasUserObjByVfd(vfd) then 
		error("has vfd in map")
	end
	
--	_RUNTIME("lua_AddUserToMap extDataStr len:", string.len(extDataStr))
	local extData = UnSerialize(extDataStr)
	lua_AddUserToMapByTbl(vfd, extData)
end

function lua_AddUserMirrorToMap(vfd, extData)
	if CHAR_MGR.HasUserObjByVfd(vfd) then 
		error("has vfd in map")
	end
	
	local userData = extData.userData
	local syncData = extData.syncData
	local layer = userData.MapLayer
	local userObj = USER_MIRROR.clsUserMirror:New(vfd, userData.x, userData.y, userData.z, syncData, userData, layer)
	if userObj:GetMirrorType() == USER_MIRROR_WULIN then
	else
		AddCharObjToAITbl(userObj)
	end
	if userObj:IsDie() then				--创建就是死亡的发送
		lretmap.die(userObj:GetId(), MAP_ID, userObj:GetMapLayer(), "")
	else
		if userData.ThugModel then
			local partnerObj = PARTNER.clsPartner:New({
				MartialTable = userData.PartnerMartial,
				LingqiModel = userData.LingqiModel,
				ThugModel = userData.ThugModel,
			}, userObj:GetId(), layer)
		end
		if userData.LingqiModel then
			local magicObj = MAGIC.clsMagic:New({MartialTable = userData.MagicMartial}, userObj:GetId(), layer)
		end
	end
end

function lua_AddMirrorByNpcNoInClient( UserObj , vfd , npcNo, x, y , ociData )
	local allNpcData = NPC_BATTLE_DATA.GetAllNpcData()
	local oneData = allNpcData[npcNo]
	if not oneData then
		error(string.format("__not npc_no:%s in map_no:%s__", npcNo, MAP_NO))
	end
	local id = CHAR_MGR.NewId()
	local z = lmapdata.getz(MAP_NO, x, y)
	local npcData = {
		x=x,y=y,z=z ,Id=id,
		AIRadius=oneData.AIRadius or NORMAL_NPC_RADIUS,
		CharNo=oneData.NpcNo or 0,
		AITrackRange=oneData.AITrackRange,
		BossType=oneData.BossType or 0,
		MartialTable=CreateMartialTable(oneData.Martials),
		Score=oneData.Score or 0,		
		AtkTime=oneData.AtkTime or AI_ATTACK_TIME,
		IsActiveAttack=oneData.IsActiveAttack or 0,
		CanPush=oneData.CanPush or CAN_PUSH,
		WalkGrid=oneData.WalkGrid or 1,
		PatrolRange=oneData.PatrolRange or NPC_PATROLRANGE,
		KReward=oneData.KReward,
		AReward=oneData.AReward,
		ExpReward=oneData.ExpReward,
		MapLayer = 1,
		Grade = oneData.Grade or 1,
		LimitHpHurt=oneData.LimitHpHurt,
		IsHurtFinish=oneData.IsHurtFinish,
		InitBuff=oneData.InitBuff,
		
		AIPatrol=oneData.AIPatrol or 0,
		LimitDropHp=oneData.LimitDropHp,
		SpNotHurt=oneData.SpNotHurt,
		
		FubenTriggerType=oneData.FubenTriggerType,
		FubenTalkNo=oneData.FubenTalkNo,
		Photo = oneData.Head,
		Shape = oneData.Shape,
		Fashion = oneData.Shape,
	}
	for _, _varName in pairs(WARRIOR_DATA_NAME) do
		if _varName ~= "Id" then
			npcData[_varName] = oneData[_varName]			
		end
	end
	npcData[mFIGHT_Hp] = oneData[mFIGHT_HpMax]	

	local propRate = CHECKEND.GetNpcCreatePropRate()
	if propRate ~= 0 then
		npcData[mFIGHT_Ap] = npcData[mFIGHT_Ap] * (1 + propRate / 100)
		npcData[mFIGHT_Dp] = npcData[mFIGHT_Dp] * (1 + propRate / 100)
		
		npcData[mFIGHT_HpMax] = npcData[mFIGHT_HpMax] * (1 + propRate / 100)
		npcData[mFIGHT_Hp] = npcData[mFIGHT_Hp] * (1 + propRate / 100)
	end
	local dir = oneData.Dir or math.random(8)
	npcData.Dir = dir	
	npcData.NotInvisibleBuff= true 	
	npcData.MirrorType = mirrorType or USER_MIRROR_NORMAL

	if ociData then 
		for _k,_v in pairs(ociData) do
			npcData[_k] = _v
		end
	end
	local	syncData = {
			rid = id ,
			name = npcData.Name, 
			grade = npcData.Grade or 1,
			shape = oneData.Shape,
			teamids = "",
			adname = "",
			sex =  1,
			score = npcData.Score or 0,
		}
	if oneData.ShapeExtend then
		for k,v in string.gmatch(oneData.ShapeExtend,"(%w+)=(%d+)" ) do 
			syncData[k] = tonumber(v)
		end
	end
	local _,userObj = TryCall( USER_MIRROR.clsUserMirror.New , USER_MIRROR.clsUserMirror , vfd, x, y, z, syncData, npcData, 1 )	
	if not userObj then 
	else
		AddCharObjToAITbl(userObj)
	end
	return userObj	
end
	


function lua_UserJump(vfd, extDataStr)
	local userObj = CHAR_MGR.GetUserByVfd(vfd)
	if userObj then
		local extData = UnSerialize(extDataStr)
		assert(extData.x and extData.y and extData.z, "not speed and dir")
		userObj:JumpTo(extData.x, extData.y, extData.z, extData.cbidx)
	else
		error("not user when lua_UserJump")
	end
end

function lua_DelUserToMap(vfd)
	local userObj = CHAR_MGR.GetUserByVfd(vfd)
	assert(userObj, "no user has vfd:" .. vfd)
	userObj:Destroy()
end

local IS_RET_MOVE = IS_RET_MOVE
local function NormalRelive(userObj, data)
	local hp = userObj:GetHp()
	if hp > 0 then return end
	
	local x, y, z = data.x, data.y, data.z
	local hpRate = data.hpRate or 100
	local hpMax = userObj:GetHpMax() / 100 * hpRate
	userObj:AddDieHp(hpMax, 0)	--复活后移到原点,jumpto的时候会搜那里的npc
	
	if x and y then
		if IsServer() then
			if not z then
				z = lmapdata.getz(MAP_NO, x, y)
				if not z then
					if ENTER_POS and ENTER_POS[1] and ENTER_POS[2] and ENTER_POS[3] then
						x, y, z = ENTER_POS[1], ENTER_POS[2], ENTER_POS[3]
					else
						error(string.format("mapNo:%s %s %s %s:", MAP_NO, x, y, z or "nil"))
					end
				end
			end
			if userObj:IsBlockPoint(x, y, z) then
				if ENTER_POS and ENTER_POS[1] and ENTER_POS[2] and ENTER_POS[3] then
					x, y, z = ENTER_POS[1], ENTER_POS[2], ENTER_POS[3]
				else
					error(string.format("mapNo:%s %s %s %s:", MAP_NO, x, y, z or "nil"))
				end
			end
		else
			if not z then
				z = lmapdata.getz(MAP_NO, x, y)
				if not z then
					error(string.format("mapNo:%s %s %s %s:", MAP_NO, x, y, z or "nil"))
				end
			end
		end

		local ok = userObj:JumpTo(x, y, z, nil, true)	
		if ok and IS_RET_MOVE then
			lretmap.usermove(userObj:GetVfd(), MAP_ID, userObj:GetMapLayer(), x, y, z)
		end
	else
		--如果是原地复活则刷新npc追踪
		local nearTbl = userObj:GetNearByNpc()
		if nearTbl then
			for _, pCharId in pairs(nearTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					pCharObj:CheckNpcToAttack(userObj)	
				end
			end
		end
	end		
	
	local initBuff = data.initBuff
	if initBuff then
		for _buffId, _buffInfo in pairs(initBuff) do
			FIGHT_EVENT.AddBuff(userObj, userObj, _buffInfo)
		end 
	end
	
	pbc_send_msg(userObj:GetVfd(), "S2c_aoi_relive_after", {
		place_holder = 1,
	})
end

local function UpGradeResetHp(userObj, data)
	local hp = userObj:GetHp()
	if hp <= 0 then return end			--死亡了不处理
	local hpMax = userObj:GetHpMax()
	userObj:AddDieHp(hpMax, 0)
end

local function ResetSp(userObj, data)
	userObj:SetSp(data)
end

local ChangeOtherDataFunc = {
	NormalRelive = NormalRelive,
	UpGradeResetHp = UpGradeResetHp,
	ResetSp = ResetSp,
}

function lua_ClientRelive(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	if not UserObj then return end

	NormalRelive(UserObj, data.reliveData)
	return true	
end

function lua_ChangeUserData(vfd, extDataStr)
	local userObj = CHAR_MGR.GetUserByVfd(vfd)
	if userObj then
		local extData = UnSerialize(extDataStr)
		if type(extData.syncData) == mTABLETYPE then
			userObj:SetSyncData(extData.syncData)
		end
		if type(extData.userData) == mTABLETYPE then
			userObj:SetPairs(extData.userData)
		end
		if extData.userOtherData then
			for _funcName, _data in pairs(extData.userOtherData) do
				if ChangeOtherDataFunc[_funcName] then
					ChangeOtherDataFunc[_funcName](userObj, _data)
				end
			end			
		end
	end	
end

function lua_ClientChangeData(key, value)
	if key == "Hp" then return end

	local UserObj = CHAR_MGR.GetRandomUserObj(1)
	if not UserObj then return end

	if key == "MartialTable" then
		local data = UnSerialize(value)
		UserObj["Set" .. key](UserObj, data)
	elseif key == "PartnerMartial" then
		local data = UnSerialize(value)
		local onePartner = UserObj:GetOnePartner()
		if onePartner then
			onePartner["Set" .. key](onePartner, data)
		end
	elseif key == "MagicMartial" then
--		local data = UnSerialize(value)
--		local oneMagic = UserObj:GetOneMagic()
--		if oneMagic then
--			oneMagic["Set" .. key](oneMagic, data)
--		end	
	elseif key == "ChooseMagicMartial" then
		local data = UnSerialize(value)
		if UserObj.GetMagicMartial then
			UserObj:SetMagicMartial(data)
		else
			UserObj:SetTmp("MagicMartial", data)
		end
		
		local oneMagic = UserObj:GetOneMagic()
		if oneMagic then
			oneMagic:SetMartialTable(data)
		end	
	else
		if UserObj["Set" .. key] then
			UserObj["Set" .. key](UserObj, value)
		end
	end
end

------------------------------------------------------------静态npc函数(没有ai的npc,只站着的)-------------------------------------------------------
function lua_AddStaticNpcToMap(mlCharId, extDataStr)	--报错，让引擎层返回错误
	if CHAR_MGR.GetCharById(mlCharId) then 
		error("has mlCharId in map")
	end
	
	local extData = UnSerialize(extDataStr)
	
	local npcData = extData.npcData
	local syncData = extData.syncData
	
	local layer = npcData.MapLayer
	npcData.MainNpc = true
	local npcObj = STATICNPC.clsStaticNpc:New(mlCharId, npcData.x, npcData.y, npcData.z, syncData, npcData, layer)
end

function lua_DelStaticNpcToMap(mlCharId)
	local npcObj = CHAR_MGR.GetCharById(mlCharId)
	assert(npcObj, "no staticNpcObj has mlCharId:" .. mlCharId)
	npcObj:Destroy()
end

function lua_ChangeStaticNpcData(mlCharId, extDataStr)
	local npcObj = CHAR_MGR.GetCharById(mlCharId)
	if npcObj then
		local extData = UnSerialize(extDataStr)
		if type(extData.syncData) == mTABLETYPE then
			npcObj:SetSyncData(extData.syncData)
		end
		if type(extData.npcData) == mTABLETYPE then
			npcObj:SetPairs(extData.npcData)
		end
	end	
end



------------------------------------------------------------item函数---------------------------------------------------------------
function lua_AddItemToMap(mlCharId, extDataStr)	--报错，让引擎层返回错误
	if CHAR_MGR.GetCharById(mlCharId) then 
		error("has mlCharId in map")
	end
	
	local extData = UnSerialize(extDataStr)
	
	local itemData = extData.itemData
	local syncData = extData.syncData
	
	local layer = itemData.MapLayer
	local itemObj = ITEM.clsItem:New(mlCharId, itemData.x, itemData.y, itemData.z, syncData, itemData, layer)
end

function lua_DelItemToMap(mlCharId)
	local itemObj = CHAR_MGR.GetCharById(mlCharId)
	assert(itemObj, "no itemObj has mlCharId:" .. mlCharId)
	itemObj:Destroy()
end

function lua_ChangeItemData(mlCharId, extDataStr)
	local itemObj = CHAR_MGR.GetCharById(mlCharId)
	if itemObj then
		local extData = UnSerialize(extDataStr)
		if type(extData.syncData) == mTABLETYPE then
			itemObj:SetSyncData(extData.syncData)
		end
	end	
end

-------------------------------------------------------其他函数------------------------------------------------------
function lua_AddLayer(data)
	local mapLayer = data.mapLayer
	assert(mapLayer, "not mapLayer")
	if MAP_LAYER_DATA[mapLayer] then
		error(string.format("has mapLayer, mapNo:%s, mapLayer:%s", MAP_NO, mapLayer))
	end
	local MAP_OBJ = laoi.map_new(MAP_MAX_X, MAP_MAX_Y, ONE_GRID_X, ONE_GRID_Y)
	assert(MAP_OBJ, "not MAP_OBJ")
	MAP_LAYER_DATA[mapLayer] = MAP_OBJ
	MAP_LAYER_CHAR[mapLayer] = {}
	return true
end

--如果还有该层的对象就无法删除
function lua_DelLayer(data)
	local mapLayer = data.mapLayer
	assert(mapLayer, "not mapLayer")
	local leaveToMapData = data.leaveToMapData or "nil"
	if not MAP_LAYER_DATA[mapLayer] then
		error(string.format("not mapLayer, mapNo:%s, mapLayer:%s", MAP_NO, mapLayer))
	end	
	local allCharObj = CHAR_MGR.GetAllCharObj(mapLayer)
--	if not table.empty(allCharObj) then
--		error(string.format("has obj can not del Layer, mapNo:%s, mapLayer:%s", MAP_NO, mapLayer))
--	end

	local tmpToMapInfo = UnSerialize(leaveToMapData) or {}
	local maxToMapNo = #tmpToMapInfo

	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsPlayer() then
			TryCall(_CharObj.Destroy, _CharObj, true)
			if maxToMapNo == 0 then
				lretmap.userdmleave(_CharObj:GetVfd(), MAP_ID, _CharObj:GetMapLayer(), "nil")
			else
				lretmap.userdmleave(_CharObj:GetVfd(), MAP_ID, _CharObj:GetMapLayer(), GetLeavePosStr(tmpToMapInfo))
			end
		elseif _CharObj:IsStaticNpc() or _CharObj:IsItem() or _CharObj:IsNpc() then
			TryCall(_CharObj.Destroy, _CharObj)
		end
	end
	MAP_LAYER_DATA[mapLayer] = nil
	MAP_LAYER_CHAR[mapLayer] = nil
	return true
end

function lua_SubNpcHp(data)
	local ids = data.ids
	local subHp = data.subHp
	
	for _, _id in pairs(ids) do
		local NpcObj = CHAR_MGR.GetCharById(_id)
		if NpcObj then
			NpcObj:SubHp(subHp, 0)
		end
	end
	return true
end

function lua_SubDieHp(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)
	if charObj then
		charObj:SetTmp("LastHitSkillId", 40000301)
		local hpMax = (charObj:GetFightValue(mFIGHT_HpMax) or 0) + 1
		charObj:SubHp(hpMax, id)
	end
end

function lua_DelNpcList(data)
	for _, _charId in pairs(data) do
		local _CharObj = CHAR_MGR.GetCharById(_charId)
		if _CharObj and not _CharObj:IsDestroy() then
			_CharObj:Destroy()
		end
	end
	return true
end

function lua_DelMirror(data)
	local id = data.id
	local _CharObj = CHAR_MGR.GetCharById(id)
	if _CharObj and not _CharObj:IsDestroy() then
		_CharObj:Destroy()
	end
	return true
end

function lua_AddMirror(data)
	local vfd = data.vfd
	lua_AddUserMirrorToMap(vfd, data.data)
	return true
end

function lua_ClearOneSkillCD(id, skillId)
	local charObj = CHAR_MGR.GetCharById(id)
	if charObj then
		charObj:ClearOneSkillCD(skillId)
	end
end

---------------------------同步某集合的血 start---------------------------
SYNC_HP_DATA = {}
function SyncAllHp(CharObj, notSync, stype)
	local syncNo = CharObj:GetSyncHpNo()
	if not syncNo then return end
	if not SYNC_HP_DATA[syncNo] then return end
	
	local sid = CharObj:GetId()
	local nHp = CharObj:GetHp()
	for _id, _ in pairs(SYNC_HP_DATA[syncNo]) do
		if sid ~= _id then
			local _CharObj = CHAR_MGR.GetCharById(_id)
			if _CharObj then
				--第三个参数notSync需要设置为true，因为同步血量是没在攻击上扣的,所以要发给客户端
				_CharObj:SetHp(nHp, 0, nil, stype, true, true)
			end
		end
	end
end

function SyncAllBuff(CharObj, buffMsg)
	local syncNo = CharObj:GetSyncHpNo()
	if not syncNo then return end
	if not SYNC_HP_DATA[syncNo] then return end
	
	local sid = CharObj:GetId()
	for _id, _ in pairs(SYNC_HP_DATA[syncNo]) do
		if sid ~= _id then
			local _CharObj = CHAR_MGR.GetCharById(_id)
			if _CharObj then
				FIGHT_EVENT.AddBuff(_CharObj, _CharObj, buffMsg, nil, true)
			end
		end
	end
end

function SyncAllFixedShield(CharObj, fixedHp)
	local syncNo = CharObj:GetSyncHpNo()
	if not syncNo then return end
	if not SYNC_HP_DATA[syncNo] then return end
	
	local sid = CharObj:GetId()
	for _id, _ in pairs(SYNC_HP_DATA[syncNo]) do
		if sid ~= _id then
			local _CharObj = CHAR_MGR.GetCharById(_id)
			if _CharObj then
				local shieldHp, fixHurtHp = _CharObj:FixedShieldBuff(_CharObj, nil, fixedHp)
				if shieldHp <= 0 then
					local sHp = (_CharObj:FixedShieldBuffSyncHp() or 0)
					_CharObj:SyncNearByPlayer("S2c_aoi_hp", {
						att_fid = 0,
						fid = _CharObj:GetFId(),
						hp = fixHurtHp,
						nhp = sHp or 0,
						hp_max = _CharObj:GetFightValue(mFIGHT_HpMax) or 0,
						type = HURT_TYPE_11,
						isdie = _CharObj:IsDie() and 1 or 0,
						die_x = _CharObj:IsDie() and _CharObj:GetX() or nil,
						die_y = _CharObj:IsDie() and _CharObj:GetY() or nil,
					})
				end
			end
		end
	end	
end

function AddSyncHp(CharObj, syncNo)
	assert(not CharObj:GetSyncHpNo(), "has syncHpNo")
	if not SYNC_HP_DATA[syncNo] then
		SYNC_HP_DATA[syncNo] = {}
	end
	CharObj:SetSyncHpNo(syncNo)
	SYNC_HP_DATA[syncNo][CharObj:GetId()] = true
	local sid = CharObj:GetId()
	for _id, _ in pairs(SYNC_HP_DATA[syncNo]) do
		if sid ~= _id then
			local _CharObj = CHAR_MGR.GetCharById(_id)
			if _CharObj then
				CharObj:SetHp(_CharObj:GetHp(), 0, nil, nil, true, true)		--改变一下npc的血
				
				local syncBuffList = _CharObj:GetSyncBuffData()
				for _buffId, _syncEffect in pairs(syncBuffList) do
					FIGHT_EVENT.AddBuff(CharObj, CharObj, {
						id = _buffId,
						time = 10000,		--同步的时候会设置到同时了的
					}, nil, true, _syncEffect)
				end
				break
			end
		end
	end	
end

function DelSyncHp(CharObj)
	local syncNo = CharObj:GetSyncHpNo()
	if not syncNo then return end
	if not SYNC_HP_DATA[syncNo] then return end
	SYNC_HP_DATA[syncNo][CharObj:GetId()] = nil
end

function lua_SyncHp(data)
	local id = data.id
	local syncNo = data.syncNo
	local CharObj = CHAR_MGR.GetCharById(id)
	AddSyncHp(CharObj, syncNo)
	return true
end

function lua_GetHitInfo(data)
	local retHitInfo = {}
	for _, _data in pairs(data) do
		local id = _data.id
		tinsert(retHitInfo, {
			name = _data.name,
			grade = _data.grade,
			shape = _data.shape,
			hurthp = CHAR_MGR.GetHurtOtherHp(id) or 0,
			bearhp = CHAR_MGR.GetBearHurtHp(id) or 0,
			rehp = CHAR_MGR.GetReHp(id) or 0,
		})
	end
	return true, lserialize.lua_seri_str(retHitInfo)
end

function lua_GetHitInfo2(data)
	local retHitInfo = {}
	for _, _data in pairs(data) do
		local id = _data.id
		tinsert(retHitInfo, {
			name = _data.name,
			grade = _data.grade,
			photo = _data.photo,
			step = _data.step,
			hurthp = CHAR_MGR.GetHurtOtherHp(id) or 0,
			bearhp = CHAR_MGR.GetBearHurtHp(id) or 0,
			rehp = CHAR_MGR.GetReHp(id) or 0,
		})
	end
	return true, lserialize.lua_seri_str(retHitInfo)	
end

function lua_SyncPlotJump(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	
	local sType = data.protoMsg.type
	data.protoMsg.rid = nil
	data.protoMsg.fid = UserObj:GetFId()
	local nx, ny = data.protoMsg.pos[1], data.protoMsg.pos[2]
	local ox, oy, oz = UserObj:GetX(), UserObj:GetY(), UserObj:GetZ()
	if sType == 1 or (nx == ox and ny == oy) then
		local playerTbl = UserObj:GetNearByPlayers()
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			pbc_send_msg(vfds, "S2c_aoi_plot_jump", data.protoMsg)
		end			
	else
		local nz = lmapdata.getz(MAP_NO, nx, ny)
		assert(nz)
		local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(UserObj:GetMapObj(), UserObj:GetEngineObj(), nx, ny)
		if not retnum then
			error("lua_SyncPlotJump move error")
			return
		elseif retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
			UserObj:SetX(nx)
			UserObj:SetY(ny)
			UserObj:SetZ(nz)
			UserObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
			UserObj:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, nil, MOVETYPE_PJUMP, data.protoMsg)
			if IS_RET_MOVE then
				lretmap.usermove(UserObj:GetVfd(), MAP_ID, UserObj:GetMapLayer(), nx, ny, nz)
			end
		else
			if ox ~= nx or oy ~= ny then			--不同位置
				error("lua_SyncPlotJump move error")
			end
		end
	end
end

function lua_ChangeMartialLevel(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	UserObj:ChangeMartialLevel(data.martialId, data.lv)
	return true
end

function lua_ChangeMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	UserObj:ChangeMartial(data.oriM, data.toM)
	return true
end

function lua_DelMultMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local martialList = data.martialList
	for _martialId, _ in pairs(martialList) do
		UserObj:DelMartial(_martialId)
	end
	return true	
end

function lua_AddMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	UserObj:AddMartial(data.martialId, data.lv, data.cd)
	return true	
end

function lua_AddAidMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local martialId = data.martialId
	local lv = data.lv or 1
	UserObj:AddMartial(martialId, lv, data.cd, true)	
	
	
	local uiskill = {}												--技能面板需要
	local martialData = SKILL_DATA.GetMartial(martialId)
	if martialData and martialData.SkillList then
		for _, _skillId in ipairs(martialData.SkillList) do
			local skillData = SKILL_DATA.GetMartialSkill(_skillId)
			if skillData then
				tinsert(uiskill, {
					skill_id = _skillId,
					skill_type = martialData.Mtype,
					skill_lv = lv,
				})					
			end
		end
	end
	pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skill_add", {
		id = UserObj:GetId(),
		uiskill = uiskill,
	})
end

function lua_ChangePartnerMartialLevel(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local onePartner = UserObj:GetOnePartner()
	if onePartner then
		onePartner:ChangeMartialLevel(data.martialId, data.lv)
		return true	
	end
end

function lua_AddPartnerMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local onePartner = UserObj:GetOnePartner()
	if onePartner then
		onePartner:AddMartial(data.martialId, data.lv, data.cd)
		return true	
	end	
end

function lua_ChangeMagicMartialLevel(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local oneMagic = UserObj:GetOneMagic()
	if oneMagic then
		oneMagic:ChangeMartialLevel(data.martialId, data.lv)
		return true	
	end
end

function lua_AddMagicMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	local oneMagic = UserObj:GetOneMagic()
	if oneMagic then
		oneMagic:AddMartial(data.martialId, data.lv, data.cd)
		return true	
	end	
end

function lua_ChangeMagicMartial(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	
	if UserObj.GetMagicMartial then
		UserObj:SetMagicMartial(data.changeMartial)
	else
		UserObj:SetTmp("MagicMartial", data.changeMartial)
	end
	
	local oneMagic = UserObj:GetOneMagic()
	if oneMagic then
--		oneMagic:ChangeMartial(data.oriM, data.toM)
		oneMagic:SetMartialTable(data.changeMartial)
		return true	
	end		
end

function lua_AddNotFightBuff(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	if not UserObj then return end
	if data.addBuff then
		FIGHT_EVENT.AddNotFightBuff(UserObj, data.addBuff)
	elseif data.delBuff then
		FIGHT_EVENT.DelNotFightBuff(UserObj, data.delBuff)
	end
	return true	
end

function lua_AddFightBuff(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)	
	if not charObj then return end
	if data.addBuff then
		FIGHT_EVENT.AddBuff(charObj, charObj, data.addBuff)
	end
	return true		
end

function lua_TerritoryAddBuff(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)	
	if not charObj then return end
	
	--已经有变身buff了
	if charObj:IsShapeshift() then return end
	if data.addBuff then
		local isOk = FIGHT_EVENT.AddBuff(charObj, charObj, data.addBuff)
		if isOk then
			--添加其他buff
			FIGHT_EVENT.AddBuff(charObj, charObj, {
				id = BUFF_IMM_DEBUFF2,
				time = 1000000,
			})
			FIGHT_EVENT.AddBuff(charObj, charObj, {
				id = BUFF_IMM_CONTROL2,
				time = 1000000,
			})
		end
		return isOk
	end
end

function lua_SiegewarAddBuff(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	if not UserObj then return end	
	if not UserObj:IsPlayer() then return end
	
	--已经有变身buff了
	if UserObj:IsShapeshift() then return end
	local npcId = data.npcId
	local npcObj = CHAR_MGR.GetCharById(npcId)	
	if not npcObj then return end	
	if not npcObj:IsNpc() then return end
	if npcObj:IsDie() then return end
	
	if data.addBuff then
		local tbl = {}
		local isOk = true
		for _, _buffData in ipairs(data.addBuff) do
			local ok = FIGHT_EVENT.AddBuff(UserObj, UserObj, _buffData)
			if ok then
				tbl[_buffData.id] = true
			else
				for _id, _ in pairs(tbl) do
					FIGHT_EVENT.DelBuff(UserObj, _id)
				end
				isOk = false
				break
			end
		end
		if isOk then
			if not npcObj:IsDestroy() then
				npcObj:Destroy()
			end
		end
		return isOk
	end
end

function lua_SiegewarJump(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	if not UserObj then return end	
	if not UserObj:IsPlayer() then return end
	
	return UserObj:JumpTo(data.x, data.y)	
end

function lua_AddFightBuffAndDel(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)	
	if not charObj then return end	
	if data.delBuffNo then
		FIGHT_EVENT.DelBuff(charObj, data.delBuffNo)
	end
	if data.addBuff then
		FIGHT_EVENT.AddBuff(charObj, charObj, data.addBuff)
	end
	return true	
end

function lua_SyncNearByPlayer(data)
	local UserObj = CHAR_MGR.GetCharById(data.player_id)
	if UserObj then
		UserObj:SyncNearByPlayer(data.proto_name, data.proto_data)
	end
	return true
end

function lua_DelFightBuff(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)	
	if not charObj then return end	
	if data.delBuffNo then
		FIGHT_EVENT.DelBuff(charObj, data.delBuffNo.id)
	end
	return true	
end

function lua_DelFightBuffType(data)
	local id = data.id
	local charObj = CHAR_MGR.GetCharById(id)	
	if not charObj then return end	
	if data.delBuffType then
		FIGHT_EVENT.DelBuffType(charObj, charObj, data.delBuffType)
	end
	return true	
	
end

function lua_MirrorStartAi(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	if not UserObj then return end	
	
	AddCharObjToAITbl(UserObj, 40)			--执行ai
end

function lua_SetWordGrade(data)
	_WORLD_GRADE = data.worldgrade
end

function lua_TestFunc(data)
	_RUNTIME("~~~~~~~~in lua_TestFunc 1")
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	
	FIGHT_EVENT.AddBuff(UserObj, UserObj, {
		id = data.buffId,
		time = 1000000,
	}, 10000011)
end

function lua_TestFunc1(data)
	local npcNo = data.npcNo
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	if not UserObj then return end
	
	
	local npcObj = lua_AddNpcInClient(npcNo, UserObj:GetX(), UserObj:GetY())
	FIGHT_EVENT.AddBuff(npcObj, npcObj, {
		id = 40000351,
		time = 3,
	})
end

---------------------------同步某集合的血 end---------------------------

function lua_GetAllUserList(data)
	local uList = CHAR_MGR.GetAllLayerCharObj()
	local ret = {}
	for _id, _CharObj in pairs(uList) do
		if _CharObj:IsPlayer() then
			tinsert(ret, _id)
		end
	end
	return true, lserialize.lua_seri_str(ret)
end

---------------------------同步玩家场景区域发言 start---------------------------
function lua_AreaChat(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)
	
	--有发言的玩家在黑名单的 玩家列表
	local blackVfds = data.blackVfds
	
	local playerVfdTbl = CHAR_MGR.GetAllPlayerVfds(UserObj:GetMapLayer())
	if playerVfdTbl then
		local vfds = {}
		local self_vfd = UserObj:GetVfd()
--		if data.isSendSelf then
--			tinsert(vfds, self_vfd)
--		end
		for _, pVfd in pairs(playerVfdTbl) do
			if not blackVfds[self_vfd] then
				tinsert(vfds, pVfd)
			end
		end
		pbc_send_msg(vfds, "S2c_aoi_chat_public", data.protoMsg)
	end	
	return true
end
---------------------------同步玩家场景区域发言 end---------------------------

---------------------------玩家场景广播 start---------------------------------
function lua_BroadcastReward(data)
	local id = data.id
	local UserObj = CHAR_MGR.GetCharById(id)	
	local playerVfdTbl = CHAR_MGR.GetAllMapPlayerVfds()
	if playerVfdTbl then
		local vfds = {}
		local self_vfd = UserObj:GetVfd()
		for _, pVfd in pairs(playerVfdTbl) do
			tinsert(vfds, pVfd)
		end
		pbc_send_msg(vfds, "S2c_aoi_chat_system", data.protoMsg)
	end	
end
---------------------------玩家场景广播 end-----------------------------------
local OtherFunc = {
	broadcastReward = lua_BroadcastReward,
	addNpc = lua_AddNpc,
	addYewaiBossNpc = lua_AddYewaiBossNpc,
	addSecretBossNpc = lua_AddSecretBossNpc,
	moveNpc = lua_MoveNpc,
	seeNpc = lua_SeeNpc,
	subNpcHp = lua_SubNpcHp,
	subDieHp = lua_SubDieHp,
	delNpcList = lua_DelNpcList,
	addLayer = lua_AddLayer,
	delLayer = lua_DelLayer,
	addMirror = lua_AddMirror,
	delMirror = lua_DelMirror,
	syncHp = lua_SyncHp,							--某个集合需要同步学
	getHitInfo = lua_GetHitInfo,
	getHitInfo2 = lua_GetHitInfo2,
	syncPlotJump = lua_SyncPlotJump,
	delMultMartial = lua_DelMultMartial,
	changeMartial = lua_ChangeMartial,				--只能是神器
	changeMartialLevel = lua_ChangeMartialLevel,
	addMartial = lua_AddMartial,
	changePartnerMartialLevel = lua_ChangePartnerMartialLevel,
	addPartnerMartial = lua_AddPartnerMartial,
	changeMagicMartialLevel = lua_ChangeMagicMartialLevel,
	addMagicMartial = lua_AddMagicMartial,
	changeMagicMartial = lua_ChangeMagicMartial,
	notFightBuff = lua_AddNotFightBuff,
	addFightBuff = lua_AddFightBuff,
	delFightBuff = lua_DelFightBuff,
	delFightBuffType = lua_DelFightBuffType,
	testFunc = lua_TestFunc,
	testFunc1 = lua_TestFunc1,
	syncAreaChat = lua_AreaChat,					--玩家区域发言
	getAllUserList = lua_GetAllUserList,
	addFightBuffAndDel = lua_AddFightBuffAndDel,
	syncNearByPlayer = lua_SyncNearByPlayer,		--广播九宫格协议
	mirrorStartAi = lua_MirrorStartAi,
	checkIsFly = lua_CheckIsFly,					--检测跳跃
	territoryAddBuff = lua_TerritoryAddBuff,		--帮派
	moveUserTo = lua_MoveUserTo,					--皇城双修拉人
	clientRelive = lua_ClientRelive,				--副本复活
	siegewarAddBuff = lua_SiegewarAddBuff,			--跨服攻城战
	siegewarJump = lua_SiegewarJump, 				--跨服攻城战跳跃
	setWorldGrade = lua_SetWordGrade,				--设置世界等级
}

function lua_Other(extDataStr)						--注意:只有返回true才表明调用成功,所以要主逻辑处理回调函数成功必须返回true
	local extData = UnSerialize(extDataStr)
	for _funcName, _data in pairs(extData) do
		if OtherFunc[_funcName] then
			return OtherFunc[_funcName](_data)
		end
	end
end

function lua_Clear()
	local allCharObj = CHAR_MGR.GetAllLayerCharObj()
	for _id, _CharObj in pairs(allCharObj) do
		TryCall(_CharObj.Destroy, _CharObj)
		if not _CharObj:IsDestroy() then
			_CharObj:SetHp(0, 0)
		end
	end
	
	lua_TimerClear()
	FIGHT_EVENT.lua_EventClear()
	CHECKEND.lua_CheckEndClear()
	CHAR_MGR.lua_ClearHurtHp()
	lua_PreloadClear()
	if IsClient() then
		lua_ExportClear()
	else
		NPC_AUTOCREATE.lua_AutoClear()
	end
end

------------------------------------------------------------------客户端start----------------------------------------------------------------------
function PlotFindNpc(findId)
	return CHECKEND.PlotFindNpc(findId)
end

function lua_SampleBattle(protoMsg, npc_proprate, player_protprate)
	local npcInfo = protoMsg.npcinfo
	local groupDelayTime = protoMsg.groupdtime
	local groupData = {}
	local groupDTimeList = {}
	local cnt = #npcInfo
	local groupDelayTimeCnt = #groupDelayTime
	for i = 1, cnt  do
		local _data = npcInfo[i]
		local group = _data.group
		if not groupData[group] then
			groupData[group] = {}
		end
		tinsert(groupData[group], _data)
	end
	
	for i = 1, groupDelayTimeCnt do
		groupDTimeList[i] = groupDelayTime[i]
		if groupDTimeList[i] < 0 then
			groupDTimeList[i] = 0
		end
	end
	
	local extData = {
		nGroupNo = 0,
		groupData = groupData,
		groupDTimeList = groupDTimeList,
		npc_proprate = npc_proprate or 0,
		player_protprate = player_protprate or 0,
	}
	CHECKEND.SetCheckEnd(BATTLE_TYPE_SAMPLE, extData)	
end

function lua_FubenPgBattle(protoMsg, npc_proprate, player_protprate)
	local npcInfo = protoMsg.npcinfo
	local groupDelayTime = protoMsg.groupdtime
	local groupData = {}
	local groupDTimeList = {}
	local cnt = #npcInfo
	local groupDelayTimeCnt = #groupDelayTime
	for i = 1, cnt  do
		local _data = npcInfo[i]
		local group = _data.group
		if not groupData[group] then
			groupData[group] = {}
		end
		tinsert(groupData[group], _data)
	end
	
	for i = 1, groupDelayTimeCnt do
		groupDTimeList[i] = groupDelayTime[i]
		if groupDTimeList[i] < 0 then
			groupDTimeList[i] = 0
		end
	end
	
	local extData = {
		nGroupNo = 0,
		groupData = groupData,
		groupDTimeList = groupDTimeList,
		npc_proprate = npc_proprate or 0,
		player_protprate = player_protprate or 0,
	}

	CHECKEND.SetCheckEnd(BATTLE_TYPE_FUBEN_PG, extData)	
end

function lua_FubenPtBattle(protoMsg, npc_proprate, player_protprate)
	if protoMsg.isfirst and (protoMsg.gateno==102005 or protoMsg.gateno==102001) then
		local CharObj = CHAR_MGR.GetRandomUserObj(1)
		if CharObj then
			CharObj:SetSp(SP_MAX)
		end
	end
	
	local npcInfo = protoMsg.npcinfo
	local groupDelayTime = protoMsg.groupdtime
	local groupData = {}
	local groupDTimeList = {}
	local cnt = #npcInfo
	local groupDelayTimeCnt = #groupDelayTime
	for i = 1, cnt  do
		local _data = npcInfo[i]
		local group = _data.group
		if not groupData[group] then
			groupData[group] = {}
		end
		tinsert(groupData[group], _data)
	end
	
	for i = 1, groupDelayTimeCnt do
		groupDTimeList[i] = groupDelayTime[i]
		if groupDTimeList[i] < 0 then
			groupDTimeList[i] = 0
		end
	end
	
	local extData = {
		nGroupNo = 0,
		groupData = groupData,
		groupDTimeList = groupDTimeList,
		isnext = protoMsg.isnext,
		layer = protoMsg.layer,
		npc_proprate = npc_proprate or 0,
		player_protprate = player_protprate or 0,
	}

	CHECKEND.SetCheckEnd(BATTLE_TYPE_FUBEN_PT, extData)	
end

function lua_FubenNextLayer(protoMsg)
	lua_FubenPtBattle(protoMsg)
	TryCall(CHECKEND.CheckEnd)
end

function lua_FubenTfBattle(protoMsg, npc_proprate, player_protprate)
	local allCharObj = CHAR_MGR.GetAllLayerCharObj()
	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsNpc() then
			TryCall(_CharObj.Destroy, _CharObj)
			if not _CharObj:IsDestroy() then
				_CharObj:SetHp(0, 0)
			end
		end
	end

	local npcInfo = protoMsg.npcinfo
	local groupDelayTime = protoMsg.groupdtime
	local groupData = {}
	local groupDTimeList = {}
	local cnt = #npcInfo
	local groupDelayTimeCnt = #groupDelayTime
	for i = 1, cnt  do
		local _data = npcInfo[i]
		local group = _data.group
		if not groupData[group] then
			groupData[group] = {}
		end
		tinsert(groupData[group], _data)
	end
	
	for i = 1, groupDelayTimeCnt do
		groupDTimeList[i] = groupDelayTime[i]
		if groupDTimeList[i] < 0 then
			groupDTimeList[i] = 0
		end
	end
	
	local extData = {
		nGroupNo = 0,
		groupData = groupData,
		groupDTimeList = groupDTimeList,
		isnext = protoMsg.isnext,
		layer = protoMsg.layer,
		npc_proprate = npc_proprate or 0,
		player_protprate = player_protprate or 0,
		ftype = protoMsg.ftype,
		waveno = protoMsg.waveno,
	}

	CHECKEND.SetCheckEnd(BATTLE_TYPE_FUBEN_TF, extData)	
end


function lua_FubenBsBattle(protoMsg, npc_proprate, player_protprate)
	local allCharObj = CHAR_MGR.GetAllLayerCharObj()
	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsNpc() then
			TryCall(_CharObj.Destroy, _CharObj)
			if not _CharObj:IsDestroy() then
				_CharObj:SetHp(0, 0)
			end
		end
	end

	local extData = {
		npcInfo = protoMsg.npcinfo,
		taskid = protoMsg.taskid,
		ftype = protoMsg.ftype,
		npc_proprate = npc_proprate or 0,
		player_protprate = player_protprate or 0,
		enterPos = protoMsg.groupdtime,
	}

	CHECKEND.SetCheckEnd(BATTLE_TYPE_FUBEN_BS, extData)	
end


function lua_FubenBsStep( step ) -- 1组队  2结束
	CHECKEND.FubenBsStep( step ) 
end 

function lua_FubenBsTeam()
	lua_FubenBsStep( 1 )
end 

function lua_FubenNextLayer_TaFang(protoMsg)
	lua_FubenTfBattle(protoMsg)
	TryCall(CHECKEND.CheckEnd)
end

------------------------------------------------------------------客户端end------------------------------------------------------------------------