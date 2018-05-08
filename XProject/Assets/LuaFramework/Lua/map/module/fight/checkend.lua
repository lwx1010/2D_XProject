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
local mabs=math.abs
local tinsert=table.insert

local WAVE_TMP = "wave_t"

--{
--	battleType = ,
--	extend = {},
--}
CheckData = {}

--{
--	
--}
PlotData = {}

DieNpcCnt = 0
NPC_CreatePropRate = 0

DieNpcPos = {
	[1] = 0,
	[2] = 0,
}

function GetDieNpcCnt( )
	return DieNpcCnt
end

function AddDieNpcCnt(npcObj)
	DieNpcCnt = DieNpcCnt + 1
	local charNo = npcObj:GetCharNo()
	if IsClient() then
		FuBenMonsterDie(charNo)
		
		if CheckData.battleType == BATTLE_TYPE_FUBEN_TF then
			local extend = CheckData.extend
			local leftNpcInfo = extend.leftNpcInfo
			
			local tmp_t = npcObj:GetTmp(WAVE_TMP)
			if not tmp_t then return end
			
			local oGroupCnt = 0
			for i = 1, #extend.groupData do
				if leftNpcInfo[i].dCnt >= leftNpcInfo[i].mCnt then
					oGroupCnt = i
				else
					break
				end
			end
			
			leftNpcInfo[tmp_t].dCnt = leftNpcInfo[tmp_t].dCnt + 1
			
			local nGroupCnt = 0
			for i = 1, #extend.groupData do
				if leftNpcInfo[i].dCnt >= leftNpcInfo[i].mCnt then
					nGroupCnt = i
				else
					break
				end
			end
			
			if tmp_t == oGroupCnt + 1 then
				SetLingYuanFbMonsterCnt(leftNpcInfo[tmp_t].name, leftNpcInfo[tmp_t].dCnt, leftNpcInfo[tmp_t].mCnt)
			end
			
			if nGroupCnt ~= oGroupCnt then
				if leftNpcInfo[nGroupCnt].dCnt >= leftNpcInfo[nGroupCnt].mCnt then
					SetLingYuanFbGateInfo(extend.layer, nGroupCnt + 1, #extend.groupData)
					if nGroupCnt + 1 <= #extend.groupData then
						SetLingYuanFbMonsterCnt(leftNpcInfo[nGroupCnt + 1].name, leftNpcInfo[nGroupCnt + 1].dCnt, leftNpcInfo[nGroupCnt + 1].mCnt)
					end
				end
			end
		end
	end
end

function SetLastDieNpcPos(x, y)
	DieNpcPos[1] = x
	DieNpcPos[2] = y
end

function GetLastDieNpcPos()
	local posList = ArrayList()
	posList:Add(DieNpcPos[1])
	posList:Add(DieNpcPos[2])
	return posList
end

function PlotFindNpc(findId)
	if PlotData[1] then
		local npcObj = CHAR_MGR.GetCharById(findId)
		if npcObj and npcObj:IsNpc() and npcObj:GetBossType() ~= BOSS_TYPE_BOX then
			local plotInfo = PlotData[1]
			PlotData[1] = nil
			PlayPlot(plotInfo)
			return true
		end
	end
end

function GetNpcCreatePropRate()
	return NPC_CreatePropRate
end

function SetNpcCreatePropRate(rate)
	NPC_CreatePropRate = rate
end

function ChangePlayerProp(rate)
	if rate == 0 then return end
	local UserObj = CHAR_MGR.GetRandomUserObj(1)
	if not UserObj then return end
	
	local ap = UserObj:GetAp() * (rate / 100)
	local dp = UserObj:GetDp() * (rate / 100)
	local nHpMax = UserObj:GetHpMax()
	local hpMax = nHpMax * (rate / 100)
	
	FIGHT_EVENT.AddFightEff(UserObj, UserObj, mFIGHT_Ap, ap, {When = CMD_FUBEN_S})
	FIGHT_EVENT.AddFightEff(UserObj, UserObj, mFIGHT_Dp, dp, {When = CMD_FUBEN_S})
	FIGHT_EVENT.AddFightEff(UserObj, UserObj, mFIGHT_HpMax, hpMax, {When = CMD_FUBEN_S})
	
	UserObj:SetHp(hpMax + nHpMax, UserObj:GetId())
end

function GetFightEndExtend()
	if not CheckData.battleType then return "" end
	if CheckData.battleType == BATTLE_TYPE_FUBEN_BS then
		return string.format("{taskid=%s}", CheckData.extend.taskid)
	end
--	if CheckData.battleType == BATTLE_TYPE_SIEGE then
--		return string.format("{hurtHp=%s}", DieNpcCnt)
	return ""
end

function GetFightEndCheckVar()
	local UserObj = CHAR_MGR.GetRandomUserObj(1)
	if not UserObj then return end
	
	local checkProto = {}
	for _varName, _ in pairs(FUBEN_CHECK_VAR) do
		local value = UserObj["Get" .. _varName](UserObj) or 0
		tinsert(checkProto, {
			key = _varName,
			value = value,
		})
	end	
	return checkProto
end

function SetCheckEnd(battleType, extend)
	if CheckData.battleType and (CheckData.battleType ~= BATTLE_TYPE_FUBEN_PT and CheckData.battleType ~= BATTLE_TYPE_FUBEN_TF) then
		error("has CheckData.battleType" .. CheckData.battleType)
	end
	CheckData.battleType = battleType
	CheckData.extend = extend
	
	if CheckData.battleType == BATTLE_TYPE_FUBEN_TF then
		extend.isFirstCreateNpc = true
		-- 记录怪物数量
		local leftNpcInfo = {}
		extend.leftNpcInfo = leftNpcInfo
		
		for _g, _data in pairs(extend.groupData) do
			leftNpcInfo[_g] = {
				dCnt = 0,
				mCnt = 0,
			}
			for _, _nData in pairs(_data) do
				local npcNo = _nData.npcno
				local allNpcData = NPC_BATTLE_DATA.GetAllNpcData()
				local oneData = allNpcData[npcNo]
				if not oneData then
					error(string.format("__not npc_no:%s in map_no:%s__", npcNo, MAP_NO))
				end
				
				if (oneData.BossType or 0) ~= BOSS_TYPE_SHUIJING then
					leftNpcInfo[_g].mCnt = leftNpcInfo[_g].mCnt + 1
					leftNpcInfo[_g].name = oneData.Name or ""
					leftNpcInfo[_g].npcNo = npcNo
				end
			end
		end	
		
		SetLingYuanFbGateInfo(extend.layer, 1, #extend.groupData)
		SetLingYuanFbMonsterCnt(leftNpcInfo[1].name, leftNpcInfo[1].dCnt, leftNpcInfo[1].mCnt)
	end
	SetNpcCreatePropRate(extend.npc_proprate)
	ChangePlayerProp(extend.player_protprate)
end

function SetTaFangEnd()
	if CheckData.battleType == BATTLE_TYPE_FUBEN_TF then
		CheckData.isTaFangEnd = true
	end
end

function CanReliveFuben()
	if CheckData.battleType == BATTLE_TYPE_FUBEN_BS then -- boss引导
		return true 
	end
	if CheckData.battleType == BATTLE_TYPE_FUBEN_TF then
		return CheckData.extend.ftype == FUBEN_TAFANG_SHOUWEI
	end
end

local function HasNpcWithoutBox(mapLayer)
	local allCharObj = CHAR_MGR.GetAllCharObj(mapLayer) 
	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsNpc() and not _CharObj:IsDie() and _CharObj:GetBossType() ~= BOSS_TYPE_BOX then
			return _CharObj
		end
	end
end

local function HasNpcWithoutShuiJing(mapLayer)
	local allCharObj = CHAR_MGR.GetAllCharObj(mapLayer) 
	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsNpc() and not _CharObj:IsDie() and _CharObj:GetBossType() ~= BOSS_TYPE_SHUIJING then
			return true
		end
	end	
end

function HasBoxNpc(mapLayer)
	local allCharObj = CHAR_MGR.GetAllCharObj(mapLayer) 
	for _id, _CharObj in pairs(allCharObj) do
		if _CharObj:IsNpc() and not _CharObj:IsDie() and _CharObj:GetBossType() == BOSS_TYPE_BOX then
			return _CharObj
		end
	end		
end

function CheckEnd()
	if not CheckData.battleType then return end
	
	local battleType = CheckData.battleType
	if battleType == BATTLE_TYPE_SAMPLE or battleType == BATTLE_TYPE_FUBEN_PG then
		local extend = CheckData.extend
		local nGroupNo = extend.nGroupNo
		local groupData = extend.groupData
		local groupDTimeList = extend.groupDTimeList
		
		local UserObj = CHAR_MGR.GetRandomUserObj(1)
		if not UserObj then return end
		if UserObj:IsDie() then							--玩家死亡判断为输
			local extendStr = GetFightEndExtend()
			lua_CheckEndClear()
			local dieCnt = UserObj:GetDiePartnerCnt()
			SendToClientFightEnd(0, dieCnt, UserObj:GetSp() or 0, extendStr)	
			return		
		end
		
		if #groupDTimeList > 0 then
			local hasNoCreateNpc = false
			local tmpNGroupNo = 1
			local notCreateGroupNo = nil
			while true do
				if groupDTimeList[tmpNGroupNo] == 0 then
					--刷新怪物
					for _, _data in pairs(groupData[tmpNGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y, extend.bossHp)
						end
					end						
					
					tmpNGroupNo = tmpNGroupNo + 1
					if not groupDTimeList[tmpNGroupNo] then
						break
					end
				elseif groupDTimeList[tmpNGroupNo] and groupDTimeList[tmpNGroupNo] > 0 then
					groupDTimeList[tmpNGroupNo] = groupDTimeList[tmpNGroupNo] - 1
					notCreateGroupNo = tmpNGroupNo
					break
				else
					break
				end
			end
			if not hasNoCreateNpc and not HasNpcWithoutBox(1) then		
				--如果没有怪物待刷并且当前没有怪物则刷新下一波
				if notCreateGroupNo then 
					groupDTimeList[notCreateGroupNo] = 0
					for _, _data in pairs(groupData[notCreateGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y, extend.bossHp)
						end
					end	
				end				
			end
			
			if not hasNoCreateNpc and not HasNpcWithoutBox(1) then	--没有待刷怪物并且没怪物判断为胜利
				local UserObj = CHAR_MGR.GetRandomUserObj(1)
				if UserObj then
					local extendStr = GetFightEndExtend()
					lua_CheckEndClear()
					local dieCnt = UserObj:GetDiePartnerCnt()
					SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, extendStr)
				end
				return
			end
		else
			if nGroupNo > 0 then
				local hasNoCreateNpc = false
				--刷新怪物
				for _, _data in pairs(groupData[nGroupNo]) do
					if _data.dtime > 0 then
						hasNoCreateNpc = true
						_data.dtime = _data.dtime - 1
					elseif _data.dtime == 0 then
						hasNoCreateNpc = true
						_data.dtime = -1
						TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y, extend.bossHp)
					end
				end	
				if hasNoCreateNpc then
					return
				end					
			end
			if not HasNpcWithoutBox(1) then
				nGroupNo = nGroupNo + 1
				extend.nGroupNo = nGroupNo
				if not groupData[nGroupNo] then				--没有怪物判断为胜利
					local extendStr = GetFightEndExtend()
					lua_CheckEndClear()
					local UserObj = CHAR_MGR.GetRandomUserObj(1)
					if UserObj then
						local dieCnt = UserObj:GetDiePartnerCnt()
						SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, extendStr)
					end
					return
				else
					--刷新怪物
					for _, _data in pairs(groupData[nGroupNo]) do
						if _data.dtime > 0 then
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y, extend.bossHp)
						end
					end				
				end
			end	
		end
	elseif battleType == BATTLE_TYPE_FUBEN_PT then						--爬塔类型
		local extend = CheckData.extend
		local nGroupNo = extend.nGroupNo
		local groupData = extend.groupData
		local groupDTimeList = extend.groupDTimeList
		local isNextLayer = assert(extend.isnext)
		
		local UserObj = CHAR_MGR.GetRandomUserObj(1)
		if not UserObj then return end
		if UserObj:IsDie() then							--玩家死亡判断为输
			lua_CheckEndClear()
			local dieCnt = UserObj:GetDiePartnerCnt()
			SendToClientFightEnd(0, dieCnt, UserObj:GetSp() or 0, "")	
			return		
		end

		if #groupDTimeList > 0 then
			local hasNoCreateNpc = false
			local tmpNGroupNo = 1
			local notCreateGroupNo = nil
			while true do
				if groupDTimeList[tmpNGroupNo] == 0 then
					--刷新怪物
					for _, _data in pairs(groupData[tmpNGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
						end
					end						
					
					tmpNGroupNo = tmpNGroupNo + 1
					if not groupDTimeList[tmpNGroupNo] then
						break
					end
				elseif groupDTimeList[tmpNGroupNo] and groupDTimeList[tmpNGroupNo] > 0 then
					groupDTimeList[tmpNGroupNo] = groupDTimeList[tmpNGroupNo] - 1
					notCreateGroupNo = tmpNGroupNo
					break
				else
					break
				end
			end
			
			if not hasNoCreateNpc and not HasNpcWithoutBox(1) then		
				--如果没有怪物待刷并且当前没有怪物则刷新下一波
				if notCreateGroupNo then 
					groupDTimeList[notCreateGroupNo] = 0
					for _, _data in pairs(groupData[notCreateGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
						end
					end	
				end				
			end
			
			if not hasNoCreateNpc and not HasNpcWithoutBox(1) then	--没有待刷怪物并且没怪物判断为胜利
				local UserObj = CHAR_MGR.GetRandomUserObj(1)
				if UserObj then
					if isnext ~= FUBEN_PT_CANNEXT_LAYER then		--没有下一层了,清除一下信息
						lua_CheckEndClear()
					end
					local dieCnt = UserObj:GetDiePartnerCnt()
					SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, "")
				end
				return
			end
		else
			if nGroupNo > 0 then
				local hasNoCreateNpc = false
				--刷新怪物
				for _, _data in pairs(groupData[nGroupNo]) do
					if _data.dtime > 0 then
						hasNoCreateNpc = true
						_data.dtime = _data.dtime - 1
					elseif _data.dtime == 0 then
						hasNoCreateNpc = true
						_data.dtime = -1
						TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
					end
				end	
				if hasNoCreateNpc then
					return
				end					
			end
			
			if not HasNpcWithoutBox(1) then
				nGroupNo = nGroupNo + 1
				extend.nGroupNo = nGroupNo
				if not groupData[nGroupNo] then					--没有怪物判断为胜利
					if isnext ~= FUBEN_PT_CANNEXT_LAYER then	--没有下一层了,清除一下信息
						lua_CheckEndClear()
					end
					local UserObj = CHAR_MGR.GetRandomUserObj(1)
					if UserObj then
						local dieCnt = UserObj:GetDiePartnerCnt()
						SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, "")
					end
					return
				else
					--刷新怪物
					for _, _data in pairs(groupData[nGroupNo]) do
						if _data.dtime > 0 then
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							_data.dtime = -1
							TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
						end
					end				
				end
			end	
		end
	elseif battleType == BATTLE_TYPE_FUBEN_TF then						--塔防类型	
		local extend = CheckData.extend
		local nGroupNo = extend.nGroupNo
		local groupData = extend.groupData
		local groupDTimeList = extend.groupDTimeList
		local isNextLayer = assert(extend.isnext)
		
		local UserObj = CHAR_MGR.GetRandomUserObj(1)
		if not UserObj then return end
		
		if CheckData.isTaFangEnd then
			lua_CheckEndClear()
			local dieCnt = UserObj:GetDiePartnerCnt()
			SendToClientFightEnd(0, dieCnt, UserObj:GetSp() or 0, "")	
			return
		end
		
		if #groupDTimeList > 0 then
			local hasNoCreateNpc = false
			local tmpNGroupNo = 1
			local notCreateGroupNo = nil
			
			if extend.isFirstCreateNpc then
				extend.isFirstCreateNpc = nil
				if groupDTimeList[1] > 0 then
					SetLingYuanFbWaveTime(groupDTimeList[1], 1, #extend.groupData)
				else 
					if groupDTimeList[2] > 0 then
						SetLingYuanFbWaveTime(groupDTimeList[2], 2, #extend.groupData)
					end
				end
			end
			
			while true do
				if groupDTimeList[tmpNGroupNo] == 0 then			
					--刷新怪物
					for _, _data in pairs(groupData[tmpNGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							local isOk, npcObj = TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
							if isOk and npcObj then
								npcObj:SetTmp(WAVE_TMP, tmpNGroupNo)
								AddCharObjToAITbl(npcObj, 1)
							end
						end
					end						
					
					tmpNGroupNo = tmpNGroupNo + 1
					if not groupDTimeList[tmpNGroupNo] then
						break
					end
				elseif groupDTimeList[tmpNGroupNo] and groupDTimeList[tmpNGroupNo] > 0 then
					groupDTimeList[tmpNGroupNo] = groupDTimeList[tmpNGroupNo] - 1
					if groupDTimeList[tmpNGroupNo] == 0 then
						if groupDTimeList[tmpNGroupNo + 1] then
							SetLingYuanFbWaveTime(groupDTimeList[tmpNGroupNo + 1], tmpNGroupNo + 1, #extend.groupData)
						else
							SetLingYuanFbTimeTip()
						end
					end
					notCreateGroupNo = tmpNGroupNo
					break
				else
					break
				end
			end
			
			if not hasNoCreateNpc and not HasNpcWithoutShuiJing(1) then		
				--如果没有怪物待刷并且当前没有怪物则刷新下一波
				if notCreateGroupNo then 
					groupDTimeList[notCreateGroupNo] = 0
					if groupDTimeList[notCreateGroupNo + 1] then
						SetLingYuanFbWaveTime(groupDTimeList[notCreateGroupNo + 1], notCreateGroupNo + 1, #extend.groupData)
					end
					for _, _data in pairs(groupData[notCreateGroupNo]) do
						if _data.dtime > 0 then
							hasNoCreateNpc = true
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							hasNoCreateNpc = true
							_data.dtime = -1
							local isOk, npcObj = TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
							if isOk and npcObj then
								npcObj:SetTmp(WAVE_TMP, notCreateGroupNo)
								AddCharObjToAITbl(npcObj, 1)
							end
						end
					end	
				end				
			end
			
			if not hasNoCreateNpc and not HasNpcWithoutShuiJing(1) then	--没有待刷怪物并且没怪物判断为胜利
				local UserObj = CHAR_MGR.GetRandomUserObj(1)
				if UserObj then
					if isnext ~= FUBEN_PT_CANNEXT_LAYER then		--没有下一层了,清除一下信息
						lua_CheckEndClear()
					end
					local dieCnt = UserObj:GetDiePartnerCnt()
					SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, "")
				end
				return
			end
		else
			if nGroupNo > 0 then
				local hasNoCreateNpc = false
				--刷新怪物
				for _, _data in pairs(groupData[nGroupNo]) do
					if _data.dtime > 0 then
						hasNoCreateNpc = true
						_data.dtime = _data.dtime - 1
					elseif _data.dtime == 0 then
						hasNoCreateNpc = true
						_data.dtime = -1
						local isOk, npcObj = TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
						if isOk and npcObj then
							npcObj:SetTmp(WAVE_TMP, nGroupNo)
							AddCharObjToAITbl(npcObj, 1)
						end
					end
				end	
				if hasNoCreateNpc then
					return
				end					
			end
			
			if not HasNpcWithoutShuiJing(1) then
				nGroupNo = nGroupNo + 1
				extend.nGroupNo = nGroupNo
				if not groupData[nGroupNo] then					--没有怪物判断为胜利
					if isnext ~= FUBEN_PT_CANNEXT_LAYER then	--没有下一层了,清除一下信息
						lua_CheckEndClear()
					end
					local UserObj = CHAR_MGR.GetRandomUserObj(1)
					if UserObj then
						local dieCnt = UserObj:GetDiePartnerCnt()
						SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, "")
					end
					return
				else
					--刷新怪物
					for _, _data in pairs(groupData[nGroupNo]) do
						if _data.dtime > 0 then
							_data.dtime = _data.dtime - 1
						elseif _data.dtime == 0 then
							_data.dtime = -1
							local isOk, npcObj = TryCall(lua_AddNpcInClient, _data.npcno, _data.x, _data.y)
							if isOk and npcObj then
								npcObj:SetTmp(WAVE_TMP, nGroupNo)
								AddCharObjToAITbl(npcObj, 1)
							end
						end
					end				
				end
			end	
		end
	elseif battleType ==  BATTLE_TYPE_FUBEN_BS then
		local extend = CheckData.extend
		local npcInfo = extend.npcInfo		
		
		local UserObj = CHAR_MGR.GetRandomUserObj(1)
		if not UserObj then return end
		
		if not extend.initBoss then 			
			UserObj:SetComp(0)			
			local bossData = npcInfo[1]
			if bossData then
				local _,bossObj = TryCall(lua_AddNpcInClient, bossData.npcno, bossData.x, bossData.y , nil , 0.5 )
				if bossObj then
					extend.bossId = bossObj:GetId()
				end	
			end	
			extend.initBoss =  true				
			local _,goodObj1 = TryCall( lua_AddMirrorByNpcNoInClient , UserObj ,"10000001" , npcInfo[2].npcno, npcInfo[2].x, npcInfo[2].y , {Comp = 0,TeamId = 1, PkMode = 0})	
			local _,goodObj2 = TryCall( lua_AddMirrorByNpcNoInClient , UserObj ,"10000002" , npcInfo[3].npcno, npcInfo[3].x, npcInfo[3].y , {Comp = 0,TeamId = 1, PkMode = 0})
			local _,badObj   = TryCall( lua_AddMirrorByNpcNoInClient , UserObj ,"10000003" , npcInfo[4].npcno, npcInfo[4].x, npcInfo[4].y , {Comp = 0,TeamId = 2, PkMode = 0})			
				
			local function RetHpFunc(CharObj, hp , oldHp , attId )
				if hp <= 0 and UserObj:GetId() ~= extend.bossOwner then 					
					extend.bossOwner = UserObj:GetId()
					if BossGuideSetGuiShu then	
						BossGuideSetGuiShu( false, CharObj:GetId())
						BossGuideSetGuiShu( true, UserObj:GetId())
					end
				end
			end		
			--goodObj1:SetRetHpFunc(RetHpFunc)
			--goodObj2:SetRetHpFunc(RetHpFunc)
			badObj:SetRetHpFunc(RetHpFunc)	

			goodObj1:SetCanRelive(true)
			goodObj2:SetCanRelive(true)
			badObj:SetCanRelive(true)	

			if extend.enterPos and extend.enterPos[1] and extend.enterPos[2] and extend.enterPos[3] then 
				goodObj1:SetRelivePos(extend.enterPos[1],extend.enterPos[2],extend.enterPos[3])
				goodObj2:SetRelivePos(extend.enterPos[1],extend.enterPos[2],extend.enterPos[3])
				badObj:SetRelivePos(extend.enterPos[1],extend.enterPos[2],extend.enterPos[3])
			end
			
			extend.bossOwner =  badObj:GetId()		
			if BossGuideSetGuiShu then
				BossGuideSetGuiShu( true, badObj:GetId() )
			end
		else
			local npc = HasNpcWithoutBox(1)
			if not npc then
				--没有怪物判断为胜利
				local extendStr = GetFightEndExtend()
				lua_CheckEndClear()
				local dieCnt = UserObj:GetDiePartnerCnt()
				SendToClientFightEnd(1, dieCnt, UserObj:GetSp() or 0, extendStr)				
				return	
			elseif not extend.hpTipsOn50 then 
				-- if npc:GetHp() < npc:GetHpMax()/2 then 
				-- 	extend.hpTipsOn50 = true
				-- 	local _joinTeamCbFunc = function()
				-- 		UserObj:SetTeamId(1)
				-- 		--pbc_send_msg( UserObj:GetVfd()  ,"S2c_hero_info_int",{info={{key="PkMode",value=PKMODE_TEAM}}})	
				-- 	end 
				-- 	if BossGuideSetTeam then
				-- 		BossGuideSetTeam( { hp = 50  , joinTeamCbFunc  = _joinTeamCbFunc } )
				-- 	end
				-- end
			end	
		end
	end
end

function FubenBsStep( step ) 
	if step == 1 then
		local UserObj = CHAR_MGR.GetRandomUserObj(1)
		if not UserObj then return end
		UserObj:SetTeamId(1)
	elseif step == 2 then 
		local npcObj = HasNpcWithoutBox(1)
		if not npcObj then return end
		FIGHT_EVENT.AddBuff(npcObj, npcObj, { id = 1001 , time = 3600} )
	end
end 

function lua_CheckEndClear()
	CheckData = {}
	PlotData = {}
	DieNpcPos = {
		[1] = 0,
		[2] = 0,
	}
	DieNpcCnt = 0
	NPC_CreatePropRate = 0
end