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

clsSecretBossNpc = NPC.clsNpc:Inherit({__ClassType = "NPC"})

local MAX_ATKLIST_NUM = 1		--最大归属数量
local EXPIRE_ATKLIST_TIME = 10	

_BossIdMap = {}

function clsSecretBossNpc:__init__(x, y, z, syncData, ociData, mapLayer, minPos, maxPos)
	Super(clsSecretBossNpc).__init__(self, x, y, z, syncData, ociData, mapLayer)
	self.minPos = minPos
	self.maxPos = maxPos
	self.HitTime = os.time()
	_BossIdMap[self:GetId()] = self
end

function clsSecretBossNpc:Destroy(notRetMap)
	Super(clsSecretBossNpc).Destroy(self, notRetMap)
	_BossIdMap[self:GetId()] = nil
end

function HeartBeatCheck()
	for _,_bossObj in pairs(_BossIdMap) do
		_bossObj:HeartBeatCheck()
	end
end

function clsSecretBossNpc:SendAtkList(AtkList, Vfd)
	AtkList = AtkList or self:GetTmp("AtkList")
	if AtkList then
		local RetProtoMsg = {}
		local cur_time = os.time()
		if #AtkList>0 then
			for _,_data in pairs({AtkList[1]}) do
				RetProtoMsg.id = _data.id
				RetProtoMsg.boss_no = self:GetCharNo()
				RetProtoMsg.remain_time = _data.time-cur_time
			end
		end
		pbc_send_msg(Vfd or CHAR_MGR.GetAllPlayerVfds(self:GetMapLayer()), "S2c_secretboss_belong", RetProtoMsg)
	end
end

function clsSecretBossNpc:AddHitHp(hp, attId)
	Super(clsSecretBossNpc).AddHitHp(self, hp, attId)
	self.HitTime = os.time()
end

function clsSecretBossNpc:HeartBeatCheck()
	local cur_hp = self:GetHp() or 0
	if cur_hp<=0 then return end
	
	if (os.time()-self.HitTime)>=20 then
		--20秒没人攻击，加满血
		self:SetHp(self:GetHpMax())
	end
	
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
end

function clsSecretBossNpc:SetHp(hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	local pre_hp = self:GetHp() or 0
	Super(clsSecretBossNpc).SetHp(self, hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
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
	
	if cur_hp==0 then
		if #AtkList>0 then
			self:SendAtkList(AtkList)
			local team_uids = {}
			local mapLayer = self:GetMapLayer()
			local Char1stObj = CHAR_MGR.GetCharById(AtkList[1].id)
			local alluid_map = {}
			if Char1stObj then
				local team_id_1st = Char1stObj:GetTeamId() or -1
				for _,_playerObj in pairs(CHAR_MGR.GetAllPlayerObj(mapLayer)) do
					local player_uid = _playerObj:GetId()
					alluid_map[player_uid] = 1
					if team_id_1st==_playerObj:GetTeamId() then
						table.insert(team_uids, player_uid)			--id代替uid
					end
				end
			end
			
			local map_vfds = {}
			for _,_playerObj in pairs(CHAR_MGR.GetAllPlayerObj(mapLayer)) do
				table.insert(map_vfds, _playerObj:GetVfd())
			end
			
			lretmap.other(self:GetId(), MAP_ID, mapLayer, lserialize.lua_seri_str({
				type = RETMAP_SECRETBOSS_REWARD,
				enemy_no = self:GetCharNo(),
				char_id = AtkList[1].id,
				team_uids = table.random_values(team_uids, #team_uids),
				map_vfds = map_vfds,
			}))
			
			self:SetTmp("AtkList", nil)
			self:SetTmp("AtkMap", nil)
		end
	end
end

function OnBelongList(UserObj, protoMsg)
	local SecretBossObj = CHAR_MGR.GetCharById(protoMsg.boss_id)
	if SecretBossObj then
		SecretBossObj:SendAtkList(nil, UserObj:GetVfd())
	end
end

function __init__()
	func_call.C2s_secretboss_belonglist = OnBelongList
end
