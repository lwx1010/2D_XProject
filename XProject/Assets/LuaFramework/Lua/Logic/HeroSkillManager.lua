local roleMgr = roleMgr
local HERO = HERO
local Time = Time
local Network = Network
local LANGUAGE_TIP = LANGUAGE_TIP
-- local FIGHTMGR = FIGHTMGR

local AllMartialXls = require("xlsdata/Skill/MartialXls")
local AllSkillXls = require("xlsdata/Skill/SkillXls")

local HeroSkillMgr = {}
local this = HeroSkillMgr

-------------------------------技能基础--------------------------------------------------
local activeMartials 		--主动武学
local changeMartials = nil		--变身后的武学列表

--[[martialtbl = {
	MartialId = xxx,
	Skills = {},
	MartialType = 1,
	Type = 1,
	Combo = 1,					--连招第几招
	SkillShow = 1,				--显示在第几个技能上
	
	--IsAider = 1,				--助战技能
	--IconNo = xxx,				--图标
	--SkillShowTip = "xxx", 	--释放技能提示
}]]--


local NORMAL_ATTACK_INTERVAL = 0.5	--技能公共cd
local lastAttackSkill			--上次释放技能时间

local curMartial				--当前武学
local nextMartial				--下一个武学

local curTarget 				--当前目标

-------------------------------战斗状态--------------------------------------------------
local curTargetMode 			--手动操作的选怪模式
local fightStatus				--挂机状态

-------------------------------选怪相关--------------------------------------------------
--当前指定目标编号
local curEnemyNo = 0			--当前指定怪物编号
--已有的切换目标列表
local switchTargetTbl = {}
--手动切换目标范围
local SWITCH_DISTANCE = 8

--发送切换目标事件
this.ChangeTargetEvent = false

-------------------------------锁敌相关--------------------------------------------------
--用于小弟的攻击目标（战斗状态中；选中目标一定时间后，发送锁敌协议，宠物去攻击）
local lastSelectTime 				--选中目标的时间
local LOCK_TARGET_INTERVAL = 1


function HeroSkillMgr.Init()
	log("HeroSkillMgr.Init----->>>")
	
	UpdateBeat:Add(this.Update, this)
	return this
end

function HeroSkillMgr.Update()

end

--------------------------------------------------------------------
--------------------------------------------------------------------
--------------------------------------------------------------------
--20180426 新项目战斗
local skillTbl = {}



--------------------------------------------------------------------
--技能释放判断
function HeroSkillMgr.JudgeSkillCd(skillId)
	
end

--技能释放距离
function HeroSkillMgr.JudgeSkillDisctance(skill)
	
end

--------------------------------------------------------------------
--挂机逻辑
function HeroSkillMgr.SelectNextTarget()
	local target 


	return target
end


--获取附近的怪物
function HeroSkillMgr.GetNearMonsterTarget(stype)
	local mainRole = roleMgr.mainRole
	local nextTarget 
	local distance
	local len = roleMgr.monsters.Count
	local monster
	for i = 0, len-1 do
		monster = roleMgr.monsters[i]
		if not IsNil(monster) and monster.hp > 0 
			and monster.canHeroAttack
			and (this.curEnemyNo == 0 or (this.curEnemyNo > 0 and monster.char_id == this.curEnemyNo)) then
			if IsNil(nextTarget) then
				if Vector3.Distance(mainRole.gameObject.transform.position, monster.gameObject.transform.position) <= TARGET_DISTANCE then
					nextTarget = monster
					distance = Vector3.Distance(mainRole.gameObject.transform.position, nextTarget.gameObject.transform.position)
				end
			else
				if distance > Vector3.Distance(mainRole.gameObject.transform.position, monster.gameObject.transform.position) then
					nextTarget = monster
					distance = Vector3.Distance(mainRole.gameObject.transform.position, monster.gameObject.transform.position)
				end
			end
		end
	end

	return nextTarget
end

--获取最近的可攻击的玩家
function HeroSkillMgr.GetNearestPlayerTarget()
	local mainRole = roleMgr.mainRole
	local nextTarget 
	local distance
	local len = roleMgr.roles.Count
	local role
	for i = 0, len-1 do
		role = roleMgr.roles[i]
		-- print("-=---------------", role.pkInfo, roleMgr.mainRole.pkInfo)
		if not IsNil(role) and role.hp > 0 and role.id ~= HERO.Id and role.entityType ~= EntityType.EntityType_Robot
		 and role.canHeroAttack
		 and role.buffs.lastStatusEff / 1000 % 10 == 0 then
			if nextTarget == nil then
				if TARGET_DISTANCE >= Vector3.Distance(mainRole.gameObject.transform.position, role.gameObject.transform.position) then 
					nextTarget = role
					distance = Vector3.Distance(mainRole.gameObject.transform.position, role.gameObject.transform.position)
				end
			else
				if distance > Vector3.Distance(mainRole.gameObject.transform.position, role.gameObject.transform.position) then
					nextTarget = role
					distance = Vector3.Distance(mainRole.gameObject.transform.position, role.gameObject.transform.position)
				end
			end
		end
	end

	return nextTarget
end

--------------------------------------------------------------------
--玩家技能设置
local function SetSkills(selectMartialTbl)
	local index = 1
	for k,v in pairs (selectMartialTbl) do
		local martialId = math.floor(v.skill_id/10)
		if martialXls[martialId] then
			local martial 
			for i = 1, #activeMartials do
				if activeMartials[i].martialId == martialId then
					martial = activeMartials[i]
					break
				end
			end

			if martial == nil then
				martial = {}
				martial.martialId = martialId
				martial.skills = {}
				table.insert(martial.skills, v.skill_id)

				martial.martialType = martialXls[martialId].Mtype
				martial.Type = martialXls[martialId].Type
				martial.cdTime = 0
				martial.receiveCd = 0
				martial.ShowIndex = index
				index = index + 1

				table.insert(martialTbl, martial)
			else
				table.insert(martial.skills, v.skill_id)
			end
		end
	end
end

--设置玩家技能
function HeroSkillMgr.SetPlayerMartial(pb)
	
end

--添加玩家技能
function HeroSkillMgr.AddPlayerMartial(pb)

end

--设置技能cd
function HeroSkillMgr.SetMartialCd(pb)
	local martialId = math.floor(pb.skill_id/10)
	for k, v in pairs (activeMartials) do
		if v.martialId == martialId then
			v.cdTime = pb.cooltime
			v.receiveCd = TimeManager.GetRealTimeSinceStartUp()
		end
	end

	SkillUiPanel.UpdateSkillCd(pb)
end

--更换技能显示设置
function HeroSkillMgr.UpdateSkillShow(pb)

end


--------------------------------------------------------------------
local normalSkillCombo = 1
local selectMartialTbl = {10000011, 10000021, 10000031, 10000041, 10000051, 10000052, 10000061, 10000071, 10000081}

--技能按钮点击	//1普通攻击 2-后面的技能
function HeroSkillMgr.SetHeroNextMartial(index, dir)
	if not activeMartials then 
		local skillId = this.GetSkillIdByBtnIndex(index)

		if dir then
			this.SendSkill(skillId, math.deg(math.atan2(dir.z, dir.x)))
		else
			this.SendSkill(skillId, -roleMgr.mainRole.move.selTrans.eulerAngles.y + 90)
		end

		-- this.SendSkill(skillId, -roleMgr.mainRole.move.selTrans.eulerAngles.y + 90)
		return 
	end

	local martial 
	for k, v in pairs(activeMartials) do
		if v.SkillShow == index then
			martial = v
			break
		end
	end

	if not martial then
		error("===============位置的技能" .. index)
		return
	end


end

--点击了技能
function HeroSkillMgr.SkillBtnClick()
	if roleMgr.mainRole == nil or roleMgr.mainRole.hp <= 0 or (Time.time -lastAttackTime) < normalAttackGap then
		return 
	end

	if FightLogic.GetCanCastSkill() == false then return end

	AutoSetSkill()

	if not this.ClickSkillArangeDeal() then 
		return 
	end

	SendHeroSkill()	
end

this.skillAreaType = {
	SkillAreaType.OuterCircle, 
	SkillAreaType.OuterCircle_InnerCube, 
	SkillAreaType.OuterCircle_InnerCircle, 
	SkillAreaType.OuterCircle_InnerSector, 
}

function HeroSkillMgr.GetSkillShowType(skillId)
	if not AllSkillXls[skillId] or not AllSkillXls[skillId].AttArea then return end

	return this.skillAreaType[AllSkillXls[skillId].AttArea.shape + 1], AllSkillXls[skillId].AttArea.shape == 3 and 120 or 0
end

function HeroSkillMgr.JudgeSkillIsInCd(skillId)
	if not activeMartials then
		error("=============技能列表为空")
		return
	end

	local martialId = math.floor(skillId/10)
	for k, v in pairs(activeMartials) do
		-- if v.
	end
end

--获取按钮对应的技能
function HeroSkillMgr.GetSkillIdByBtnIndex(index)
	local skillId
	if index == 1 then
		skillId = 10000000+normalSkillCombo
		normalSkillCombo = normalSkillCombo + 1
		normalSkillCombo = normalSkillCombo > 4 and 1 or normalSkillCombo
	else
		skillId = selectMartialTbl[index -1]
	end

	return skillId
end

--------------------------------------------------------------------
--协议相关

--发送锁敌，用于小弟攻击目标
function HeroSkillMgr.SendToLockEnemy(id)
	Network.send("C2s_aoi_skilllock", {tar_id = id})
end

--发送技能
function HeroSkillMgr.SendSkill(skillId, dir, targetId, targetPos)
	print("====SendSkill=============11111111111==============")
	local skillXls = AllSkillXls[skillId]
	if not skillXls then return end

	print("====SendSkill=============22222222222==============")
	local cmd = {}

	cmd.skill_id = skillId
	cmd.dir = dir --math.deg(math.atan2(dir.z, dir.x))
	if targetId then
		cmd.mtar_id = targetId
		cmd.tx = targetPos.x
		cmd.tz = targetPos.z
	elseif targetPos then
		cmd.tx = targetPos.x
		cmd.tz = targetPos.z
	end

	if skillXls.MoveLength and skillXls.MoveLength > 0 then

		local moveEnd = roleMgr.mainRole.move.selTrans.position + Vector3.New(math.cos(dir*math.pi/180), 0, math.sin(dir*math.pi/180))*skillXls.MoveLength
		local movePos = SceneHelper.Linecast(roleMgr.mainRole.move.selTrans.position, moveEnd)

		cmd.move_dir = dir
		if movePos == Vector3.zero then
			cmd.move_x = moveEnd.x 
			cmd.move_z = moveEnd.z
		else
			cmd.move_x = movePos.x 
			cmd.move_z = movePos.z
		end
	end

	--发送协议给服务端
	Network.send("C2s_aoi_skillact_bycharpos", cmd)
	-- 客户端先行表现
	FIGHTMGR.StartSkill(cmd, true)
	print("====SendSkill=============3333333333333==============", TableToString(cmd))
end

--闪  dir 方向 pos最终点的坐标
function HeroSkillMgr.SendDodge(dir, pos)
	
end


--------------------------------------------------------------------

return HeroSkillMgr