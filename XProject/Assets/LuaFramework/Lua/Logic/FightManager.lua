local roleMgr = roleMgr
local HEROSKILLMGR = HEROSKILLMGR
local HERO = HERO
local FightLogic = FightLogic
local Time = Time
local AllSkillXls = require "xlsdata/Skill/SkillXls"
local AllMartialXls = require "xlsdata/Skill/MartialXls"
local MapDataXls = require("xlsdata/Map/MapDataXls")

local FightMgr = {}
local this = FightMgr

this.preSkillEffects = nil 	--技能预警
this.enableskillmsg = true

---------------------------------------------------------------------------
--自身死亡
this.isDie = false

function FightMgr.Init()
	log("FightMgr.Init----->>>")


	--初始化SkillManager
	HEROSKILLMGR.Init()

	return this
end

--释放技能前处理
--添加技能动作状态
--可以移动的技能需要添加技能移动状态
--不可以移动的技能需要停止移动，面向技能方向
function FightMgr.DoPreSkill(attacker, isSelf, dir, skillXls)
	if IsNil(attacker) then return end

	if isSelf then
		roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.SkillActing)
		if skillXls.CanMove == 1 then
			roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.SkillMoving)
		end
	end

	-- print("=================", debug.traceback())
	if attacker.move then
		attacker.move:StopToDir(dir)
	end
end

--广播协议(玩家或npc技能协议按队列进行处理)
-- @param: isDequeue 是否是队列出栈消息
function FightMgr.StartSkill(pb, isSelf, isDequeue)
	if not AllSkillXls[pb.skill_id] then return end

	local attacker
	if isSelf then
		HEROSKILLMGR.curActSkill = pb.skill_id
		attacker = roleMgr.mainRole
	else
		attacker = roleMgr:GetSceneEntityById(pb.att_id)
	end

	this.DoPreSkill(attacker, isSelf, pb.dir, AllSkillXls[pb.skill_id])
	
	if not IsNil(attacker) and attacker.skillCtrl ~= nil then
		attacker.skillCtrl:UseSkillByTabel(AllSkillXls[pb.skill_id]
			,  (pb.tx and (not pb.mtar_id or #pb.mtar_id == 0)) and SceneHelper.GetNearestPos(pb.tx, pb.tz) or Vector3.zero
			, isSelf and roleMgr.mainRole.transform.position or SceneHelper.GetNearestPos(pb.move_x, pb.move_z)
			, AllMartialXls[math.floor(pb.skill_id/10)].Mtype == 1 and false or true)
	end
end

local tbl = {showHp, showType, nhp, backPos, floatdown_buff}

--广播技能击中协议(玩家或npc技能协议按队列进行处理)
-- @param: isDequeue 是否是队列出栈消息
function FightMgr.SkillHit( pb, isDequeue )
	if not AllSkillXls[pb.skill_id] then return end

	--如果calcnt 字段为0和nil， 且没有前摇的时候，服务端会把act协议优化掉，收到这样的hit协议时，需要播放动作
	if ( not pb.calcnt or pb.calcnt == 0) and (not AllSkillXls[pb.skill_id].BeforeHurtTime or AllSkillXls[pb.skill_id].BeforeHurtTime <= 0) then
		this.StartSkill(pb)
	end

	local attacker = roleMgr:GetSceneEntityById(pb.att_id)

	--血量,受击处理
	local tbl 
	for k, v in pairs(pb.tar_chars) do
		local target = roleMgr:GetSceneEntityById(v.tar_id)
		if target then 
			tbl = {}
			tbl.showHp = 1					--是否显示伤害（只显示与自己相关的）
			tbl.hpType = v.type 			--0:普通伤害,1:其他(技能导致加buff,删buff等),2:暴击伤害,3:miss,4:无敌
			tbl.hpNum = v.show_hp			--冒血值
			tbl.backPosX = v.back_x			--被击退到的点
			tbl.backPosZ = v.back_z
			tbl.fdBuff = v.floatdown_buff	--浮空击倒buff
			tbl.divideTime = AllSkillXls[pb.skill_id].DivideDamageTime and AllSkillXls[pb.skill_id].DivideDamageTime[pb.calcnt ~= 0 and pb.calcnt+1 or 1] or nil 		--分段伤害时间
			tbl.divideNum = AllSkillXls[pb.skill_id].DivideDamageNum and AllSkillXls[pb.skill_id].DivideDamageNum[pb.calcnt ~= 0 and pb.calcnt+1 or 1] or nil 			--分段伤害数值
			tbl.SkillResPath = this.GetSkillTreePath(target)

			-- print("==============", pb.skill_id, pb.calcnt, TableToString(tbl))
			target.skillCtrl:ExecuteHit(tbl)

			if target.charData.hpstamp < v.hpstamp then
				target.charData.hp = v.nhp
				target.skillCtrl:UpdateHP(v.nhp, target.charData.hpmax)
				
				if v.nhp <= 0 then
					target:Dead()
				end
			end
		end
	end
end


--动作结束需要重置主角状态
function FightMgr.DoAfterSkill(attacker)
	if IsNil(attacker) then return end

	if attacker.entityType == EntityType.EntityType_Self then
		HEROSKILLMGR.curActSkill = 0

		roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.SkillActing)
		if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.SkillMoving) then
			roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.SkillMoving)
		end
	end
end

--闪
function FightMgr.SkillDodge(entity, x, z)
	if not entity then return end

	entity.skillCtrl:StopSkill(x, z)
end

--获取受击行为树路径
function FightMgr.GetSkillTreePath(target)
	if target.entityType == EntityType.EntityType_Monster then
		return string.format("monster/%s", target.charData.shape)
	else
		-- print("=====================", target.charData.skeletonNo, target.id)
		return string.format("player/%s", target.charData.skeletonNo)
	end
end

---------------------------------------------------------------------------------------------------
--设置客户端战斗信息
function FightMgr.SetFightInfo( type, fid, mno )
	--print("---------------FightMgr--------------------", type)
	this.fightType = type
	this.fightId = fid
	this.mno = mno

	--副本战斗默认是挂机
	local ClientFightMgr = require("Logic/ClientFightManager")
	if this.fightType ~= ClientFightMgr.BATTLE_TYPE.BATTLE_TYPE_FUBEN_BS then
		HEROSKILLMGR.EnterFuBenSceneSetGuaJi()
	end
end

--退出战斗场景
function FightMgr.ExitSceneDealWithFight()
	if this.preSkillEffects then
		this.preSkillEffects = nil
	end

	HEROSKILLMGR.ResetTargetDistance(HEROSKILLMGR.DEFAULT_DISTANCE)

	--客户端战斗场景退出时 销毁客户端战斗
	if this.JudgeIsClientFight() then
		this.fightType = 1
		this.fightId = nil
		this.mno = nil

		local ClientFightMgr = require("Logic/ClientFightManager")
 	    ClientFightMgr.ClearClientFight()

 	    local MainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
 	    MainCtrl.SetFuBenFightShow(false)

 	    --清理aider
 	    HEROSKILLMGR.ClearAiderMartial()

 	    --清楚副本数据
 	    FUBENLOGIC.SetCurFuBenInfo(nil, nil)
	end
end

function FightMgr.FubenFightIsOutTime()
	--print("---------FubenFightIsOutTime------", this.fightType)
	if this.JudgeIsClientFight() then
		--暂停客户端战斗
		local ClientFightMgr = require("Logic/ClientFightManager")
		ClientFightMgr.StopFight()

		--HEROSKILLMGR.StopFight(stop)

		--发送结束协议
		local cmd = {}
		cmd.fid = this.fightId
		cmd.iswin = 0
		cmd.extend = ""
		cmd.sp = FIGHT_EVENT.GetPlayerSp()
		local str = string.format("fid=%s,extend=%s,mno=%s,iswin=%s", cmd.fid, cmd.extend, this.mno, cmd.iswin)
		--print("-------122----",  cmd.fid, cmd.extend, FIGHTMGR.mno, cmd.iswin, str, cmd.sp)
		cmd.sign = string.sub(MD5.ComputeString(str), 9, 24)
		cmd.cvar = {}	
		Network.send("C2s_battle_fight_end", cmd)
	end
end

----------------------------------------------------------------------------------------------------------------
--技能预警

--圆形
function FightMgr.ShowCirclePreSkillTipEffect(tipId, tipInfo)
	if not this.preSkillEffects then
		this.preSkillEffects = {}
	end

	local pos = Util.Convert2RealPosition(tipInfo.x, tipInfo.y)
	if pos == Vector3.zero then return end

	local prefabGo = Util.LoadPrefab("Prefab/Other/CircleEffect")
	local tipGo = newObject(prefabGo)
	tipGo.transform.position = pos

	tipGo.transform.localScale = Vector3.New(tipInfo.r/2.02, 1, tipInfo.r/2.02)

	if not this.preSkillEffects[tipId] then
		this.preSkillEffects[tipId] = {}
	end

	table.insert(this.preSkillEffects[tipId], tipGo)
end

--线形
function FightMgr.ShowLinePreSkillTipEffect(tipId, tipInfo)
	if not this.preSkillEffects then
		this.preSkillEffects = {}
	end

	local apos = Util.Convert2RealPosition(tipInfo.ax, tipInfo.ay)
	local tpos = Util.Convert2RealPosition(tipInfo.tx, tipInfo.ty)
	if apos == Vector3.zero or tpos == Vector3.zero then return end

	for k, v in ipairs(tipInfo.degree) do
		local prefabGo = Util.LoadPrefab("Prefab/Other/LineEffect")
		local tipGo = newObject(prefabGo)

		if tipInfo.atype == 1 then
			tipGo.transform.position = apos
		else
			tipGo.transform.position = tpos
		end

		if tipInfo.ax == tipInfo.tx then
			if tipInfo.ay > tipInfo.ty then
				tipGo.transform.eulerAngles = Vector3.New(0, 90 - (v - 90) + 90, 0)
            else
                tipGo.transform.eulerAngles = Vector3.New(0, 270 - (v - 90) + 90, 0)
			end
		else
			local factor = math.atan((tipInfo.ty - tipInfo.ay)/(tipInfo.tx - tipInfo.ax))
			if tipInfo.ax < tipInfo.tx then
				tipGo.transform.eulerAngles = Vector3.New(0, - factor * 180 / math.pi - (v- 90) + 90, 0)
			else
				tipGo.transform.eulerAngles = Vector3.New(0, 180 - factor * 180 / math.pi - (v - 90) + 90, 0)
			end
		end

		tipGo.transform.localScale = Vector3.New(tipInfo.accu/3.32, 1, tipInfo.len/4.62)

		if not this.preSkillEffects[tipId] then
			this.preSkillEffects[tipId] = {}
		end

		table.insert(this.preSkillEffects[tipId], tipGo)
	end
end

--扇形
function FightMgr.ShowSectorPreSkillTipEffect(tipId, tipInfo)
	if not this.preSkillEffects then
		this.preSkillEffects = {}
	end

	local apos = Util.Convert2RealPosition(tipInfo.ax, tipInfo.ay)
	local tpos = Util.Convert2RealPosition(tipInfo.tx, tipInfo.ty)
	if apos == Vector3.zero or tpos == Vector3.zero then return end

	local prefabGo = Util.LoadPrefab("Prefab/Other/SectorEffect")
	local tipGo = newObject(prefabGo)

	if tipInfo.atype == 1 then
		tipGo.transform.position = apos
	else
		tipGo.transform.position = tpos
	end

	if tipInfo.ax == tipInfo.tx then
		if tipInfo.ay > tipInfo.ty then
			tipGo.transform.eulerAngles = Vector3.New(0, 180, 0)
        else
            tipGo.transform.eulerAngles = Vector3.New(0, 0, 0)
		end
	else
		local factor = math.atan((tipInfo.ty - tipInfo.ay)/(tipInfo.tx - tipInfo.ax))
		if tipInfo.ax < tipInfo.tx then
			tipGo.transform.eulerAngles = Vector3.New(0, - factor * 180 / math.pi + 90, 0)
		else
			tipGo.transform.eulerAngles = Vector3.New(0, 180 - factor * 180 / math.pi + 90, 0)
		end
	end

	tipGo.transform.localScale = Vector3.New(tipInfo.len/3.77, 1, tipInfo.len/3.77)

	if not this.preSkillEffects[tipId] then
		this.preSkillEffects[tipId] = {}
	end

	table.insert(this.preSkillEffects[tipId], tipGo)
end

--删除技能预警
function FightMgr.DeleteSkillTipEffect(tipId)
	if not this.preSkillEffects or not this.preSkillEffects[tipId] then
		return
	end

	for k, v in pairs(this.preSkillEffects[tipId]) do
		destroy(v)
	end

	this.preSkillEffects[tipId] = nil
end

--------------------------------------------------------------------------------------
function FightMgr.CreatePkModeTbl(pkmode)
	local tbl = {}
	local pkTbl = string.split(pkmode, ";")
	local splitStr
	for k, v in pairs(pkTbl) do
		if string.len(v) > 0 then
			splitStr = string.split(v, "=")
			tbl[splitStr[1]] = (splitStr[1] == "pkmode" or splitStr[1] == "evil_state") and tonumber(splitStr[2]) or splitStr[2]
		end
	end

	return tbl
end

function FightMgr.ChargetDiDuiClubStatus(pkmode)
	if not roleMgr.mainRole then return false end

	require "map/map_const"

	local myPkMode = this.CreatePkModeTbl(roleMgr.mainRole.pkInfo)
	-- print("-=--------11111111----------", myPkMode.pkmode, PKMODE_HOSTILECLUB, myPkMode.hostileclub)
	if myPkMode.pkmode == PKMODE_PEACE then
		return false
	end

	local otherPkMode = this.CreatePkModeTbl(pkmode)
	if myPkMode.pkmode == PKMODE_TEAM 
		and myPkMode.team_id and otherPkMode.team_id and myPkMode.team_id == otherPkMode.team_id then
		return false
	end

	-- print("-=--------22222222----------", otherPkMode.club_id or "nil",  myPkMode.hostileclub)
	if (otherPkMode.club_id and #otherPkMode.club_id > 0 and myPkMode.hostileclub and myPkMode.hostileclub == otherPkMode.club_id)
		or (myPkMode.club_id and #myPkMode.club_id > 0 and otherPkMode.hostileclub and otherPkMode.hostileclub == myPkMode.club_id) then
		return true
	end

	-- print("-=--------33333333----------")
	return false
end

--判断玩家目标状态	--0和平 1全体 2队友 3帮友 4善恶 5同服
function FightMgr.ChargeTargetStatus(pkmode)
	--怪物
	if not pkmode or #pkmode == 0 then return true end

	if not roleMgr.mainRole then return false end

	require "map/map_const"

	local myPkMode = this.CreatePkModeTbl(roleMgr.mainRole.pkInfo)
	--print("-------my----", TableToString(myPkMode))
	if myPkMode.pkmode == PKMODE_PEACE then
		return false
	elseif myPkMode.pkmode == PKMODE_WHOLE then
		return true
	end

	local otherPkMode = this.CreatePkModeTbl(pkmode)
	--print("-------other----", TableToString(myPkMode))
	if myPkMode.pkmode == PKMODE_TEAM then
		if myPkMode.team_id and otherPkMode.team_id and  myPkMode.team_id == otherPkMode.team_id and myPkMode.team_id ~= 0 then
			return false
		else
			return true
		end
	end

	if myPkMode.pkmode == PKMODE_CLUB then
		if myPkMode.club_id and otherPkMode.club_id and  myPkMode.club_id == otherPkMode.club_id and myPkMode.club_id ~= 0  then
			return false
		else
			return true
		end
	end

	if myPkMode.pkmode == PKMODE_EVIL then
		return otherPkMode.evil_state >= 2
	end

	if myPkMode.pkmode == PKMODE_SERVER then
		if myPkMode.server_id and otherPkMode.server_id and  myPkMode.server_id == otherPkMode.server_id then
			return false
		else
			return true
		end
	end
end

--判断玩家目标名字类型 0白色 1黄色 2红色
function FightMgr.ChargeTargetNameType(pkmode)
	if not roleMgr.mainRole then return 0 end

	require "map/map_const"
	local myPkMode = this.CreatePkModeTbl(pkmode)
	--print("-=----------", myPkMode.evil_state)

	--华山论剑中名字都为0色
	local HuaShanLunJianLogic = require("Logic/HuaShanLunJianLogic")
	return HuaShanLunJianLogic.IsInHSLJ and 0 or myPkMode.evil_state 
end

--攻击优先级判断：仇杀>阵营>敌对>安全区域(配置)>其他模式
--判断其他玩家血条颜色(是否可以攻击)
function FightMgr.ChangeTargetBloodColor(pkmode, x, y, z, entityType, comp,otherId)
    if entityType ~= EntityType.EntityType_Monster and entityType ~= EntityType.EntityType_Role then
    	return false
    end

    if not roleMgr.mainRole then
    	return false
    end

    if entityType == EntityType.EntityType_Role then
    	--判断阵营
    	-- print("-----------111--------", roleMgr.mainRole.comp, comp)
    	--  if roleMgr.mainRole.comp == comp then
    	--  	return false
    	--  end

    	--判断敌对pk模式
    	-- print("-----------2222222---------", this.ChargetDiDuiClubStatus(pkmode) )
    	

    	-- print("-----------33333---------", this.ChargetDiDuiClubStatus(pkmode) )

    	--判断安全区
	    local pos = Util.Convert2MapPosition(roleMgr.mainRole.transform.position.x
	            , roleMgr.mainRole.transform.position.z
	            , roleMgr.mainRole.transform.position.y)
	    local MAPLOGIC = MAPLOGIC
	    local mySafe = MAPLOGIC.ChargeIsInSafeArea(pos.x, pos.z)
	    if mySafe == 3 or mySafe == 1 then
	    	return false
	    end

	    pos = Util.Convert2MapPosition(x, z, y)
	    local targetSafe = MAPLOGIC.ChargeIsInSafeArea(pos.x, pos.z)
	    if targetSafe == 3 then
	    	return false
	    end

	    --判断是否在仇杀场景并且为仇人
	    local sceneNo = roleMgr.curSceneNo
	    local cfg = MapDataXls[sceneNo]
	    local myPkMode = this.CreatePkModeTbl(roleMgr.mainRole.pkInfo)

	    if myPkMode.pkmode ~= PKMODE_PEACE and otherId and (mySafe == 5 or mySafe == 6 or mySafe == 7) then
	        local FriendLogic = require('Logic/FriendLogic')
	        local enemyList = FriendLogic.GetBriefEnemy()
	        if enemyList[otherId] then
	        	return true
	        end
	    end

	    --不同阵营
	    if roleMgr.mainRole.comp ~= comp then
    		return true
    	end

	    -- print("-----------44444---------", this.ChargetDiDuiClubStatus(pkmode) )

	    --安全可仇杀不可敌对返回
	    if mySafe == 5 then
    		return false
    	end

	    -- print("-----------------", mySafe, targetSafe, roleMgr.mainRole.pkInfo, pkmode)
	    --敌对判断
	    if this.ChargetDiDuiClubStatus(pkmode) then
    		return true
    	end 

    	-- print("--------------------", mySafe, targetSafe)
    	--安全场景：敌对，仇杀返回
    	if mySafe < 3 or targetSafe < 3 or mySafe == 5 or mySafe == 6 then
    		return false
    	end

    	--判断阵营和攻击模式
	    if not this.ChargeTargetStatus(pkmode) then
	    	return false
	    end

    	return true
	elseif entityType == EntityType.EntityType_Monster then
		--判断阵营
	    if roleMgr.mainRole.comp == comp then
	    	return false
	    end

		return true
	end

    return false
end

--杀戮战场调用，只判断阵营
function FightMgr.JudgeTargetIsEnemy(comp)
    if not roleMgr.mainRole then
    	return false
    end

    if roleMgr.mainRole.comp == comp then
    	return false
    end

	return true
end

--------------------------------------------------------------------------------------
--获取是否场景技能特效显示
function FightMgr.GetSceneSkillShow()
	local mapData = require "xlsdata/Map/MapDataXls"
	local map = mapData[roleMgr.curSceneNo]
    if not map then
        logError("---GetSceneSkillShow-地图数据错误" .. roleMgr.curSceneNo)
        return false
    end

    return map.ShowAllSkillEffect and map.ShowAllSkillEffect == 1 or false
end

--获取是否场景玩家技能特效简化显示
function FightMgr.GetSceneSkillSimpleShow()
	-- local mapData = require "xlsdata/Map/MapDataXls"
	-- local map = mapData[roleMgr.curSceneNo]
 --    if not map then
 --        logError("---GetSceneSkillSimpleShow-地图数据错误" .. roleMgr.curSceneNo)
 --        return false
 --    end

    return true
end
--------------------------------------------------------------------------------------
--判断是否是客户端战斗
function FightMgr.JudgeIsClientFight()
	return this.fightType == 2 or this.fightType == 3 or this.fightType == 4 or this.fightType == 5
end

--------------------------------------------------------------------------------------
function FightMgr.NpcCreateClubListDeal(clublist)
	-- print("------------------------")
	if not roleMgr.mainRole or not HERO.ClubId then return false end

	for k, v in pairs(clublist) do
		if v == HERO.ClubId then
			return true
		end
	end

	return false
end


return FightMgr