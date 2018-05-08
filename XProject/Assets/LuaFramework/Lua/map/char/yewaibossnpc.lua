local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local os=os
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO

local pbc_send_msg = pbc_send_msg

clsYewaiBossNpc = NPC.clsNpc:Inherit({__ClassType = "NPC"})

local MAX_ATKLIST_NUM = 10		--最大归属数量
local EXPIRE_ATKLIST_TIME = 10	
local MAX_SCORE_LIST = 10		--伤害排行最大数量

_YewaiBossId = 0
_BossHpMax = 1
_ScoreList = {}
_BossScoreReward = {}
_HitHpTbl = {}

function clsYewaiBossNpc:__init__(x, y, z, syncData, ociData, mapLayer, minPos, maxPos, bossScore)
	Super(clsYewaiBossNpc).__init__(self, x, y, z, syncData, ociData, mapLayer)
	self.minPos = minPos
	self.maxPos = maxPos
	self.rewardIdx = 0
	self.needSortHurt = false
	_YewaiBossId = self:GetId()
	_ScoreList = {}
	_HitHpTbl = {}
	_BossScoreReward = bossScore
	_BossHpMax = self:GetHpMax()
end

function clsYewaiBossNpc:Destroy(notRetMap)
	Super(clsYewaiBossNpc).Destroy(self, notRetMap)
	_YewaiBossId = 0
end

function clsYewaiBossNpc:AddHitHp(hp, attId)
	Super(clsYewaiBossNpc).AddHitHp(self, hp, attId)
	self.needSortHurt = true
end

function HeartBeatCheck()
	if _YewaiBossId~=0 then
		local YewaiBossObj = CHAR_MGR.GetCharById(_YewaiBossId)
		if YewaiBossObj then
			YewaiBossObj:HeartBeatCheck()
		end
	end
end

function clsYewaiBossNpc:SendAtkList(AtkList, Vfd)
	AtkList = AtkList or self:GetTmp("AtkList")
	if AtkList then
		local RetProtoMsg = {}
		RetProtoMsg.list = {}
		RetProtoMsg.boss_id = _YewaiBossId
		local cur_time = os.time()
		if #AtkList>0 then
			for _,_data in pairs({AtkList[1]}) do
				local info = {}
				info.id = _data.id
				info.remain_time = _data.time-cur_time
				table.insert(RetProtoMsg.list, info)
			end
		end
		pbc_send_msg(Vfd or CHAR_MGR.GetAllPlayerVfds(self:GetMapLayer()), "S2c_yewaiboss_belonglist", RetProtoMsg)
	end
end

function clsYewaiBossNpc:SortHurt()
	local hp_max = self:GetHpMax()
	local score_list = {}
	_HitHpTbl = self:GetHitHpTbl() or {}
	for _uid,_hurt in pairs(_HitHpTbl) do
		table.insert(score_list, {score=_hurt,uid=_uid})
	end
	
	function _sortfunc(ua, ub)
		return ua.score > ub.score
	end
	
	table.sort(score_list, _sortfunc)
	local size = #score_list
	size = size<=MAX_SCORE_LIST and size or MAX_SCORE_LIST
	local score_list_size = {}
	for i=1,size do
		score_list[i].score = score_list[i].score*100/hp_max
		table.insert(score_list_size, score_list[i])
	end
	_ScoreList = score_list_size
end

function clsYewaiBossNpc:HeartBeatCheck()
	local cur_hp = self:GetHp() or 0
	if cur_hp<=0 then return end
	
	local AtkList = self:GetTmp("AtkList") or {}
	local AtkMap = self:GetTmp("AtkMap") or {}
	
	local size = #AtkList
	if size>0 then
		local cur_time = os.time()
		for i=size, 1, -1 do
			local data = AtkList[i]
			local is_remove = false
			if cur_time>=data.time then
				is_remove = true
			else
				local CharObj = CHAR_MGR.GetCharById(data.id)
				if not CharObj then
					is_remove = true
				else
					local x = CharObj:GetX()
					local y = CharObj:GetY()
					is_remove = x<self.minPos[1] or x>self.maxPos[1] or y<self.minPos[2] or y>self.maxPos[2]
				end
			end
			
			if is_remove then
				table.remove(AtkList, i)
				if i==1 then	--头名变化才显示
					self:SetTmp("IsBelongChange", 1)
				end
			end
		end
		
		if self:GetTmp("IsBelongChange") then
			local newAtkMap = {}
			for idx,data in pairs(AtkList) do
				newAtkMap[data.id] = idx
			end
			self:SetTmp("AtkMap", newAtkMap)
			
			self:SetTmp("IsBelongChange", nil)
			self:SendAtkList(AtkList)
		end
	end
	
	local ResetTimes = (self:GetTmp("ResetTimes") or 0) + 1
	self:SetTmp("ResetTimes",ResetTimes)
	if ResetTimes%5==0 then
		ResetTimes=0
		self.needSortHurt = false
		self:SortHurt()
	end
end

function SendHurtList(UserObj)
	local protoMsg = {list={},can_score=UserObj:GetTmp("CanGetBossScore") or 0}
	for _idx,_data in pairs(_ScoreList) do
		local list_info = {}
		list_info.uid = _data.uid
		list_info.name = CHAR_MGR.GetUserName(_data.uid) or ""
		list_info.hurt = _data.score*100
		list_info.score = _BossScoreReward[_idx] or 0
		table.insert(protoMsg.list, list_info)
	end
	protoMsg.self_hurt = (_HitHpTbl[UserObj:GetId()] or 0)*100/_BossHpMax*100
	protoMsg.join_score = _BossScoreReward[#_BossScoreReward]
	pbc_send_msg(UserObj:GetVfd(),"S2c_yewaiboss_scorelist",protoMsg)
end

function clsYewaiBossNpc:SetHp(hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	local pre_hp = self:GetHp() or 0
	Super(clsYewaiBossNpc).SetHp(self, hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	local AtkList = self:GetTmp("AtkList")
	if not AtkList then
		AtkList = {}
		self:SetTmp("AtkList", AtkList)
	end
	local AtkMap = self:GetTmp("AtkMap")
	if not AtkMap then
		AtkMap = {}
		self:SetTmp("AtkMap", AtkMap)
	end
	
	local cur_hp = self:GetHp() or 0
	cur_hp = cur_hp<0 and 0 or cur_hp
	
	local CharObj = CHAR_MGR.GetCharById(attId)
	if CharObj and CharObj:IsPlayer() then
		local idx = AtkMap[attId]
		if idx then
			local data = AtkList[idx]
			if data then
				data.time  = os.time()+EXPIRE_ATKLIST_TIME
			end
		else
			local size = #AtkList
			if size<MAX_ATKLIST_NUM then
				table.insert(AtkList, {id=attId,name=CharObj:GetName(),time=os.time()+EXPIRE_ATKLIST_TIME})
				local now_idx = size+1
				AtkMap[attId] = now_idx
				if now_idx==1 and cur_hp>0 then		--头名变化才显示
					self:SetTmp("IsBelongChange", 1)
				end
			end
		end
	end
	
	local hp_max = self:GetHpMax()
	local hp_rate = cur_hp/hp_max
	local pre_rate = pre_hp/hp_max
	if pre_rate>0.3 and hp_rate<=0.3 then
		lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_SYS_TIPS,
			send_type = SYS_ROLL,
			content = string.format(_T("yewaiboss12"), _CR(self:GetName()), _CR("30%"), self:GetCharNo()),
		}))
	elseif pre_rate>0.7 and hp_rate<=0.7 then
		lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_SYS_TIPS,
			send_type = SYS_ROLL,
			content = string.format(_T("yewaiboss11"), _CR(self:GetName()), _CR("70%"), self:GetCharNo()),
		}))
	end
	
	if cur_hp==0 then
		if #AtkList>0 then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_SYS_TIPS,
				send_type = SYS_ROLL,
				content = string.format(_T("yewaiboss6"), _CB(AtkList[1].name), _CDEG(SCENE_NAME), _CR(self:GetName())),
			}))
			
			self:SendAtkList(AtkList)
			local team_uids = {}
			local club_uids = {}
			local mapLayer = self:GetMapLayer()
			local Char1stObj = CHAR_MGR.GetCharById(AtkList[1].id)
			local alluid_map = {}
			if Char1stObj then
				local club_id_1st = Char1stObj:GetClubId() or -1
				local team_id_1st = Char1stObj:GetTeamId() or -1
				for _,_playerObj in pairs(CHAR_MGR.GetAllPlayerObj(mapLayer)) do
					local player_uid = _playerObj:GetId()
					alluid_map[player_uid] = 1
					if team_id_1st==_playerObj:GetTeamId() then
						table.insert(team_uids, player_uid)			--id代替uid
					elseif club_id_1st==_playerObj:GetClubId() then
						table.insert(club_uids, player_uid)
					end
				end
			end
			
			local hit_uids = {}
			for _uid,_hurt in pairs(self:GetHitHpTbl() or {}) do
				if alluid_map[_uid] then
					table.insert(hit_uids, _uid)
				end
			end
			
			self:SortHurt()
			for _idx,_data in pairs(_ScoreList) do
				_data.inmap = alluid_map[_data.uid] and 1 or 0
				local char_obj = CHAR_MGR.GetCharById(_data.uid)
				if char_obj then
					local can_score = char_obj:GetTmp("CanGetBossScore") or 0
					can_score = can_score-_BossScoreReward[_idx]
					can_score = can_score<0 and 0 or can_score
					char_obj:SetTmp("CanGetBossScore", can_score)
				end
			end
			
			lretmap.other(self:GetId(), MAP_ID, mapLayer, lserialize.lua_seri_str({
				type = RETMAP_YEWAIBOSS_REWARD,
				enemy_no = self:GetCharNo(),
				reward_idx = 1,
				char_id = AtkList[1].id,
				team_uids = table.random_values(team_uids, #team_uids),
				club_uids = club_uids,
				join_uids = hit_uids,
				score_list = _ScoreList,
			}))
			
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_YEWAIBOSS_BELONG,
				user_id = AtkList[1].id,
				enemy_no = self:GetCharNo(),
			}))
			
			self:SetTmp("AtkList", nil)
			self:SetTmp("AtkMap", nil)
		end
	end
end

function OnBelongList(UserObj, protoMsg)
	if _YewaiBossId~=0 then
		local YewaiBossObj = CHAR_MGR.GetCharById(_YewaiBossId)
		if YewaiBossObj then
			YewaiBossObj:SendAtkList(nil, UserObj:GetVfd())
		end
	else
		local RetProtoMsg = {}
		RetProtoMsg.list = {}
		RetProtoMsg.boss_id = "0"
		pbc_send_msg(UserObj:GetVfd(), "S2c_yewaiboss_belonglist", RetProtoMsg)
	end
end

function OnScoreList(UserObj)
	SendHurtList(UserObj)
end

function __init__()
	func_call.C2s_yewaiboss_belonglist = OnBelongList
	func_call.C2s_yewaiboss_scorelist = OnScoreList
end
