local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local tinsert=table.insert
local tremove=table.remove
local SINGLE_TARGET_TYPE=SINGLE_TARGET_TYPE
local MULTI_TARGET_TYPE=MULTI_TARGET_TYPE
local SKILL_SHAPE_LINE=SKILL_SHAPE_LINE
local SKILL_SHAPE_CIRCLE=SKILL_SHAPE_CIRCLE
local SKILL_ATTAREA_MAX=SKILL_ATTAREA_MAX
local MAP_LAYER_CHAR=MAP_LAYER_CHAR
local mabs = math.abs
local mfloor = math.floor
local MOVE_DIR=MOVE_DIR

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

lua_time_sec = 0.1			--每+1相当于0.1秒
local SECOND_TIME_CNT = 10			--1秒多少个time_no
local SECOND_2_TIME_CNT	= SECOND_TIME_CNT * 2
local SECOND_10_TIME_CNT = SECOND_TIME_CNT * 10
local HALF_SECOND_TIME_CNT = 5		--0.5秒多少个time_no

local lua_nowtime_no = 0			--每+1相当于0.1秒
local AI_WALK_TIME = AI_WALK_TIME
local AI_DESTROY_TIME = AI_DESTROY_TIME
local AI_RELIVE_TIME = AI_DESTROY_TIME

function GetTimeSec()
	return lua_time_sec
end

function GetSecondTimeCnt()
	return SECOND_TIME_CNT
end

function lua_Timer()
	lua_nowtime_no = lua_nowtime_no + 1
	AITimer()
	TryCall(FIGHT_EVENT.CheckStartSkill, lua_nowtime_no)
	TryCall(FIGHT_EVENT.CheckPassMsg, lua_nowtime_no)
	TryCall(FIGHT_EVENT.CheckExtraSkill, lua_nowtime_no)
	if IsServer() then
		TryCall(NPC_AUTOCREATE.CheckNpcRefresh, lua_nowtime_no)
		
		if lua_nowtime_no % SECOND_10_TIME_CNT == 0 then
			TryCall(NPC_AUTOCREATE.CheckAutoNpc)
		end
	end
	TryCall(FIGHT_EVENT.CheckFlyPlayer, lua_nowtime_no)
	if lua_nowtime_no % HALF_SECOND_TIME_CNT == 0 then			--每秒检查buff
		TryCall(FIGHT_EVENT.CheckBuff, lua_nowtime_no)
	end
	if lua_nowtime_no == 1 or lua_nowtime_no % SECOND_TIME_CNT == 0 then		--每秒检查是否战斗结束
		TryCall(CHECKEND.CheckEnd)
		TryCall(FIGHT_EVENT.CheckFlyDodgeCoolTime, lua_nowtime_no)
	end
	TryCall(FIGHT_EVENT.CheckDaZuo, lua_nowtime_no)
	if lua_nowtime_no % SECOND_2_TIME_CNT == 0 then	
		TryCall(FIGHT_EVENT.CheckEvilYellow, lua_nowtime_no)
	end
	
	if IsServer() and lua_nowtime_no % SECOND_TIME_CNT == 0 then	
		TryCall(YEWAIBOSSNPC.HeartBeatCheck)
		TryCall(SECRETBOSSNPC.HeartBeatCheck)
		TryCall(USER.SyncPlayerPosition)
	end
end

function GetNowTimeNo()
	return lua_nowtime_no
end

local AITimerTbl = {}
local AIDestroyTbl = {}
local AIReliveTbl = {}
local WEAK_TBL = {__mode = "v"}

function lua_TimerClear()
	lua_nowtime_no = 0
	AITimerTbl = {}
	AIDestroyTbl = {}
	AIReliveTbl = {}
end

function DumpTimer(isDetail)
	print("_______________________AITimerTbl___________________________ 1")
	for _timeNo, _tbl in pairs(AITimerTbl) do
		if isDetail then
			print(_timeNo, table.size(_tbl))
		else
			print(_timeNo, table.size(_tbl))
		end
	end
	print("_______________________AITimerTbl___________________________ 2")
	print()
	print("_______________________AIDestroyTbl___________________________ 1")
	for _timeNo, _tbl in pairs(AIDestroyTbl) do
		if isDetail then
			print(_timeNo, table.size(_tbl))
		else
			print(_timeNo, table.size(_tbl))
		end
	end
	print("_______________________AIDestroyTbl___________________________ 2")
end

function GetAITimeNoTbl(timeNo)
	local tmpTbl = AITimerTbl[timeNo]
	if not tmpTbl then
		tmpTbl = {}
		AITimerTbl[timeNo] = tmpTbl
		setmetatable(tmpTbl, WEAK_TBL)
	end	
	return tmpTbl
end

function GetAIDestroyTbl(timeNo)
	local tmpTbl = AIDestroyTbl[timeNo]
	if not tmpTbl then
		tmpTbl = {}
		AIDestroyTbl[timeNo] = tmpTbl
		setmetatable(tmpTbl, WEAK_TBL)
	end	
	return tmpTbl	
end
function GetAIReliveTbl(timeNo)
	local tmpTbl = AIReliveTbl[timeNo]
	if not tmpTbl then
		tmpTbl = {}
		AIReliveTbl[timeNo] = tmpTbl
		setmetatable(tmpTbl, WEAK_TBL)
	end	
	return tmpTbl	
end

function AddCharObjToAITbl(CharObj, intervalNo)
	local AIObj = CharObj:GetAI()
	if AIObj then
		DelCharObjToAITbl(CharObj)
		local nT = AIObj:GetTime() or AI_WALK_TIME
		local nextT = GetNowTimeNo() + (intervalNo or nT)
		local tmpTbl = GetAITimeNoTbl(nextT)
		tmpTbl[CharObj:GetId()] = CharObj
		AIObj:SetNextTimeNo(nextT)
		return true
	end	
end

function DelCharObjToAITbl(CharObj)
	local AIObj = CharObj:GetAI()
	if AIObj then
		local nextT = AIObj:GetNextTimeNo()
		if nextT then
			local tmpTbl = GetAITimeNoTbl(nextT)
			tmpTbl[CharObj:GetId()] = nil
			return true
		end
	end	
end

function DestroyOneCharObjDelay(CharObj)
	local timeNo = GetNowTimeNo() + AI_DESTROY_TIME
	local tmpTbl = GetAIDestroyTbl(timeNo)
	tinsert(tmpTbl, CharObj)
end
function ReliveOneCharObjDelay(CharObj)
	local timeNo = GetNowTimeNo() + AI_RELIVE_TIME
	local tmpTbl = GetAIReliveTbl(timeNo)
	tinsert(tmpTbl, CharObj)
end

function AITimer()
	local nowTimeNo = GetNowTimeNo()
	local timeTbl = AITimerTbl[nowTimeNo]
	if timeTbl then
		for _, _CharObj in pairs(timeTbl) do
			if not _CharObj:IsDie() then
				local AIObj = _CharObj:GetAI()
				if AIObj then
					local actTimeNo = nil
					if _CharObj:IsNpc() then
						actTimeNo = _CharObj:GetActDelayTime()
					end
					if not actTimeNo or nowTimeNo >= actTimeNo then
						if not _CharObj:IsDizziness() then	--中了不能移动buff
							TryCall(AIObj.Run, AIObj)		--需要做保护,免得报错后面的ai都不动
						end
					end
					local nT = nil
					if _CharObj:IsNpc() then
						local _, tipsTime = _CharObj:IsNowTipsSkillId()
						if tipsTime then
							nT = mfloor(tipsTime / (1000 * lua_time_sec))
						else
							nT = AIObj:GetTime() or AI_WALK_TIME
						end
					else
						nT = AIObj:GetTime() or AI_WALK_TIME
					end
					
					local nextT = nowTimeNo + nT
					if actTimeNo and actTimeNo > nowTimeNo then
						nextT = actTimeNo
					end
					local tmpTbl = GetAITimeNoTbl(nextT)
					tmpTbl[_CharObj:GetId()] = _CharObj
					AIObj:SetNextTimeNo(nextT)
				end
			end
		end
		AITimerTbl[nowTimeNo] = nil
	end
	local destroyTbl = AIDestroyTbl[nowTimeNo]
	if destroyTbl then
		for _, _CharObj in pairs(destroyTbl) do
			if not _CharObj:IsDestroy() then
				_CharObj:Destroy()
			end
		end
		AIDestroyTbl[nowTimeNo] = nil		
	end

	local reliveTbl = AIReliveTbl[nowTimeNo]
	if reliveTbl then
		for _, _CharObj in pairs(reliveTbl) do
			if not _CharObj:IsDestroy() and _CharObj:IsDie() then
				lua_ClientRelive( {
					id = _CharObj:GetId() ,
					reliveData = _CharObj._reliveData or {}
				})
				AddCharObjToAITbl(_CharObj)
			end
		end
		AIReliveTbl[nowTimeNo] = nil		
	end

	
end