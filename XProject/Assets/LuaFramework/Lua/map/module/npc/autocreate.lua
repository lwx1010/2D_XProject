local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local assert=assert
local error=error
local tinsert=table.insert
local tremove=table.remove
local mfloor=math.floor
local mceil=math.ceil
local MAP_NO=MAP_NO
local GetNowTimeNo=GetNowTimeNo

AUTO_CREATE = false
--[timeno] = {
--	[1] = {npcNo, x, y},
--}
LATER_CREATETBL = {}

function CheckNpcRefresh(nowTimeNo)
	local timeTbl = LATER_CREATETBL[nowTimeNo]
	if timeTbl then
		for _, _freshData in pairs(timeTbl) do
			local isOk, nNpcObj = TryCall(lua_AddNpcInClient, _freshData.npcno, _freshData.x, _freshData.y)
			if isOk and nNpcObj then
				nNpcObj:SetRefreshData(_freshData)
			end					
		end
		LATER_CREATETBL[nowTimeNo] = nil
	end
end

function AddNpcRefresh(npcObj)
	if not npcObj:IsDie() then return end
	if npcObj:IsStaticNpc() or not npcObj:IsNpc() then return end
	local freshData = npcObj:GetRefreshData()
	if not freshData then return end
	
	if freshData.fno <= 0 then
		local isOk, nNpcObj = TryCall(lua_AddNpcInClient, freshData.npcno, freshData.x, freshData.y)
		if isOk and nNpcObj then
			nNpcObj:SetRefreshData(freshData)
		end		
	else
		local nowTimeNo = GetNowTimeNo()
		local nTimeNo = nowTimeNo + freshData.fno
		local nextTbl = LATER_CREATETBL[nTimeNo] or {}
		tinsert(nextTbl, freshData)
		LATER_CREATETBL[nTimeNo] = nextTbl
	end
end

function AutoCreateNpc()
	if not AUTO_CREATE then
		local autoDataFile = string.format("setting/npc/auto_createnpc/%d.lua", MAP_NO)
		if not posix.stat(autoDataFile) then 
			return 
		else
--			print("loading autonpc file:", os.time(), autoDataFile)
		end
		local modFile = Import(autoDataFile)
		if not modFile then 
			_RUNTIME_ERROR("__AutoCreateNpc__ error import", autoDataFile)
			return 
		end
		AUTO_CREATE = true
		local autoData = modFile.GetAutoCreateData()
		for _npcNo, _timeData in pairs(autoData) do
			for _freshTime, _data in pairs(_timeData) do
				local freshTimeNo = mceil(_freshTime / (1000 * lua_time_sec))
				for _, _posData in pairs(_data) do
					local isOk, npcObj = TryCall(lua_AddNpcInClient, _npcNo, _posData[1], _posData[2])
					if isOk and npcObj then
						npcObj:SetRefreshData({
							npcno = _npcNo,
							fno = freshTimeNo,
							x = _posData[1],
							y = _posData[2],
						})
					end
				end
			end
		end
	end
end

function lua_AutoClear()
	LATER_CREATETBL = {}
end

function CheckAutoNpc()
	if CHECKEND.GetDieNpcCnt() <= 0 then
		AutoCreateNpc()
	end
end

function __init__()
	AutoCreateNpc()
end