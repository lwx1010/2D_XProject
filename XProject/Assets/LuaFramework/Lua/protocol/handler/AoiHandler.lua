local HEROSKILLMGR = HEROSKILLMGR
local Network = Network
local HERO = HERO
local FIGHTMGR = FIGHTMGR
local MapData = MapData

local function CreateAoiSyncplayer(pb)
	local player = Aoi_syncplayer.New()
	player.shape = pb.shape
	player.name = pb.name
	player.speed = pb.speed
	player.dir = pb.dir
	player.camp = pb.camp
	player.buffs = pb.buffs
	player.hpmax = pb.hpmax
	player.clubid = pb.clubid or ""
	player.teamid = pb.teamid or ""
	player.extend = pb.extend or ""
	return player
end

local function CreateAoiSyncnpc(pb)
	local npc = Aoi_syncnpc.New()
	npc.npcno = pb.npcno
	npc.shape = pb.shape
	npc.name = pb.name
	npc.speed = pb.speed
	npc.dir = pb.dir
	npc.camp = pb.camp
	npc.buffs = pb.buffs
	npc.hpmax = pb.hpmax
	npc.isstatic = pb.isstatic
	npc.extend = pb.extend or ""
	-- print("================", npc.shape, npc.npcno )
    return npc
end

local function CreateAoiSyncPartner(pb)
	local partner = Aoi_syncpartner.New()
	partner.partnerno = pb.partnerno
	partner.ownerid = pb.ownerid
	partner.shape = pb.shape
	partner.name = pb.name
	partner.speed = pb.speed
	partner.dir = pb.dir
	partner.camp = pb.camp
	partner.buffs = pb.buffs
	partner.hpmax = pb.hpmax
	partner.extend = pb.extend or ""
	return partner
end

Network["S2c_aoi_sync_int"] = function(pb)
	local entity = roleMgr:GetSceneEntityById(pb.id)
	if entity ~= nil then
		ENTITYCREATE.RefreshEntityData(entity, pb.key, pb.value)
	end
end

Network["S2c_aoi_sync_float"] = function(pb)
	local entity = roleMgr:GetSceneEntityById(pb.id)
	if entity ~= nil then
		ENTITYCREATE.RefreshEntityData(entity, pb.key, pb.value)
	end
end

Network["S2c_aoi_sync_string"] = function(pb)
	local entity = roleMgr:GetSceneEntityById(pb.id)
	if entity ~= nil then
		ENTITYCREATE.RefreshEntityData(entity, pb.key, pb.value)
	end
end

Network["S2c_aoi_addnpc"] = function(pb)
	local npc = S2c_aoi_addnpc.New()
	npc.id = pb.id
	npc.x = pb.x
	npc.z = pb.z
	npc.move_dir = pb.move_dir
	npc.hp = pb.hp
	npc.curTime = TimeManager.GetRealTimeSinceStartUp()
	npc.sync = CreateAoiSyncnpc(pb.sync)

	roleMgr:AddEntityToScene(npc)
end

Network["S2c_aoi_addplayer"] = function(pb)
	-- print("===============S2c_aoi_addplayer=====", pb.id)
	local player = S2c_aoi_addplayer.New()
	player.id = pb.id
	player.x = pb.x
	player.z = pb.z
	player.move_dir = pb.move_dir
	player.hp = pb.hp
	player.curTime = TimeManager.GetRealTimeSinceStartUp()
	player.sync = CreateAoiSyncplayer(pb.sync)
	roleMgr:AddEntityToScene(player)
end

Network["S2c_aoi_addpartner"] = function(pb)
	-- local player = S2c_aoi_addpartner.New()
	-- player.id = pb.id
	-- player.x = pb.x
	-- player.z = pb.z
	-- player.move_dir = pb.move_dir
	-- player.hp = pb.hp
	-- player.curTime = TimeManager.GetRealTimeSinceStartUp()
	-- player.sync = CreateAoiSyncplayer(pb.sync)
	-- roleMgr.entityCreate:AddPlayer(player)
end

local function TestMove(pb)
	local player = S2c_aoi_addplayer.New()
	player.id = pb.id .. "_1"
	player.x = pb.x
	player.z = pb.z
	player.move_dir = 0
	player.hp = pb.hp
	player.curTime = TimeManager.GetRealTimeSinceStartUp()
	player.sync = CreateAoiSyncplayer(pb.sync)
	roleMgr:AddEntityToScene(player)
end

Network["S2c_aoi_addself"] = function(pb)
	local mainRole = S2c_aoi_addself.New()
	mainRole.map_no = pb.map_no
	mainRole.map_id = pb.map_id
	mainRole.id = pb.id
	mainRole.x = pb.x
	mainRole.z = pb.z
	mainRole.hp = pb.hp
	mainRole.curTime = TimeManager.GetRealTimeSinceStartUp()
	mainRole.sync = CreateAoiSyncplayer(pb.sync)
	-- print("===========S2c_aoi_addself====================", pb.map_id, pb.map_no, mainRole.sync.shape)

	roleMgr:AddEntityToScene(mainRole)

	-- TestMove(pb)
end

-----------------------------------------------------
--移动
Network["S2c_aoi_move_start"] = function(pb)
	-- print("===========S2c_aoi_move_start====================")
	if pb.id == HERO.Id then return end

	local obj = roleMgr:GetSceneEntityById(pb.id)
	if obj and obj.move then
		obj.move:MoveStart(pb.x, pb.z, pb.dir, TimeManager.GetRealTimeSinceStartUp())
	end

	---当obj还没有创建时
end

Network["S2c_aoi_move_stop"] = function(pb)
	-- print("===========S2c_aoi_move_stop====================")
	local obj = roleMgr:GetSceneEntityById(pb.id)
	if obj and obj.move then
		obj.move:MoveStop(pb.x, pb.z, pb.dir, TimeManager.GetRealTimeSinceStartUp(), 0.5)
	end

	---当obj还没有创建时

end

Network["S2c_aoi_move_to"] = function(pb)
	--print("===========S2c_aoi_move_to====================")
	local obj = roleMgr:GetSceneEntityById(pb.id)
	if obj and obj.move then
		obj.move:MoveStop(pb.x, pb.z, pb.dir, TimeManager.GetRealTimeSinceStartUp(), 0.1)
	end

	---当obj还没有创建时

end

Network["S2c_aoi_move_update"] = function(pb)
	if IsNil(roleMgr.mainRole) then return end

	if roleMgr.mainRole.move then
		roleMgr.mainRole.move:SendAoiMoveUpdate()
	end
end

--- 删除NPC
Network["S2c_aoi_leave"] = function(pb)
	-- print("========S2c_aoi_leave=========", pb.id)
	roleMgr:DeleteObjectsById(pb.id)
end

Network["S2c_aoi_move_error"] = function(pb)
	if IsNil(roleMgr.mainRole) then return end

	if roleMgr.mainRole.move then
		roleMgr.mainRole.move:MoveError(pb.ox, pb.oz)
	end
end

Network["S2c_aoi_jump"] = function(pb)
	local obj = roleMgr:GetSceneEntityById(pb.id)
	if obj and obj.move then
		obj.move:MoveStop(pb.x, pb.z, pb.dir, TimeManager.GetRealTimeSinceStartUp(), 0.1)
		return
	end

	---当obj还没有创建时

end

--伙伴跟随
Network["S2c_aoi_partner_follow"] = function(pb)

end
-----------------------------------------------------

Network["S2c_aoi_addbuff"] = function(pb)

end

Network["S2c_aoi_delbuff"] = function(pb)

end

------------------------------------------------------------------
--过场景完成
Network["S2c_aoi_createmap"] = function (pb)
	MapData.curMapNo = pb.map_no
	MapData.curMapId = pb.map_id

	Game.LoadSceneViaPreloading(pb.map_no)
end

------------------------------------------------------------------
Network["S2c_aoi_sync_hp"] = function(pb)
	-- print("============S2c_aoi_sync_hp========================")

	local obj = roleMgr:GetSceneEntityById(pb.id)
	if obj then
		if obj.charData.hpstamp < pb.hpstamp then
			obj.skillCtrl:UpdateHP(pb.nhp, obj.charData.hpmax)

			if pb.nhp <= 0 then
				obj.charData.hp = pb.nhp
				obj:Dead()
			elseif obj.charData.hp <= 0 and pb.nhp > 0 then
				obj.charData.hp = pb.nhp
				obj:Relive()
			end
		end
	end
end


------------------------------------------------------------------
--技能相关
Network["S2c_aoi_skillhit_bycharpos"] = function(pb)
	-- print("============S2c_aoi_skillhit_bycharpos========================", TableToString(pb))
	FIGHTMGR.SkillHit(pb)

end

Network["S2c_aoi_skillact_bycharpos"] = function(pb)
	-- print("============S2c_aoi_skillact_bycharpos========================")
	-- if pb.att_id == HERO.Id then 
	-- 	return
	-- end

	FIGHTMGR.StartSkill(pb)
end

Network["S2c_aoi_skillcd"] = function(pb)
	-- print("============S2c_aoi_skillcd========================")
	HEROSKILLMGR.SetMartialCd(pb)
end

Network["S2c_aoi_skillstop"] = function(pb)
	-- print("============S2c_aoi_skillstop========================")
	local obj = roleMgr:GetSceneEntityById(pb.att_id)
	if not IsNil(obj) then
		obj:BreakSkill(pb.skill_id)
	end
end

Network["S2c_aoi_skillerror"] = function(pb)
	error("================技能错误：" .. pb.erro_no)
	HEROSKILLMGR.SkillErrorDeal()
end

-------------------------------------------------------------------------------
--闪
Network["S2c_aoi_dodge"] = function(pb)
	-- print("============S2c_aoi_dodge========================")
	local obj = roleMgr:GetSceneEntityById(pb.id)
	FIGHTMGR.SkillDodge()
end

Network["S2c_aoi_dodge_rate"] = function(pb)
	print("============S2c_aoi_dodge_rate========================", pb.dodge_rate)
	HEROSKILLMGR.SetHeroDodge(pb.dodge_rate)
end

Network["S2c_aoi_dodge_error"] = function(pb)
	error("================闪错误：" .. pb.type)
	-- if IsNil(roleMgr.mainRole) then return end

	-- if roleMgr.mainRole.move then
	-- 	roleMgr.mainRole.move:MoveError(pb.ox, pb.oz)
	-- end
end