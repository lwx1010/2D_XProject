local roleMgr = roleMgr
local HERO = HERO
local Time = Time
local Network = Network
local LANGUAGE_TIP = LANGUAGE_TIP

local HeroSkillMgr = {}
local this = HeroSkillMgr

-------------------------------技能基础--------------------------------------------------
local activeMartials 		--主动武学
local passiveMartials 		--被动武学

local changeMartials = nil		--变身后的武学列表

local NORMAL_ATTACK_INTERVAL = 0.5	--技能公共cd
local lastAttackSkill			--上次释放技能时间

local curMartial				--当前武学
local nextMartial				--下一个武学

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
--协议相关

function HeroSkillMgr.SendToLockEnemy()

end

function HeroSkillMgr.SendSkill(index)
	local skillId = skillTbl[index]


end


return HeroSkillMgr