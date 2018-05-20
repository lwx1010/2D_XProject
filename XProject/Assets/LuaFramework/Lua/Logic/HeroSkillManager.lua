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

this.curActSkill = 0 			--当前正在播放的技能

--[[martialtbl = {
	MartialId = xxx,
	Skills = {},
	MartialType = 1,			--1普通攻击 2技能 3大招
	Type = 1,					--主动被动
	Combo = 1,					--连招第几招(当前可用或者正在使用,对应当前显示技能图标)
	ComboMax = 3,				--最大连招数
	SkillShow = 1,				--显示在第几个技能上
	isOver = true,				--当前武学技能是否都释放完了(只在自动战斗时有用)
	cdTime = 0,
	receiveCd = 0,
	SkillPriority = 0,			--武学优先级

	--IsAider = 1,				--助战技能
	--IconNo = xxx,				--图标
	--SkillShowTip = "xxx", 	--释放技能提示
}]]--

local NORMAL_ATTACK_INTERVAL = 0.5	--技能公共cd
local lastAttackTime = 0			--上次释放技能时间

local curMartial				--当前武学
local nextMartial				--下一个武学
local nextDir					--下一招技能方向

local DODGE_DISTANCE = 5		--闪的距离

local hasSend = false			--标识客户端已经发送了技能，但是没有返回

-------------------------------连招处理--------------------------------------------------

local COMBO_SKILL_BREAK_INTERVAL = 5		--连招中断间隔
local COMBO_NORMAL_BREAK_INTERVAL = 2		--普通攻击连招中断间隔
local lastSendComBoSkill = 0 				--上次发送连招时间
local curTime = 0							--当前时间

-------------------------------战斗状态--------------------------------------------------
local curTargetMode 			--手动操作的选怪模式
local fightStatus = 0				--挂机状态		0 非战斗  1点怪 2挂机
this.curTarget = nil 			--当前目标

local TARGET_DISTANCE = 15

local AUTO_FIGHT_BREAK_INTERVAL = 5			--点击中断挂机
local AUTO_FIGHT_OFFSET = 1					--挂机状态下攻击距离减2

--挂机参数
local isGuaJiOn = false
local lastOutGuaJiTime = 0		--挂机状态下手操退出挂机

local guaJiMartials = {}		--挂机武学列表

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

-------------------------------闪避相关--------------------------------------------------
this.DODGE_CHARGE_TIME = 12				--闪避充能时间
this.ONE_DODGE_VALUE = 5000				--一个闪避值对应5000
this.DODGE_MAX_COUNT = 2				--闪的最大数量
this.receiveDodgeTime = 0			
this.receiveDodgeValue = 0
this.curDodge = 0						--当前闪值

-----------------------------------------------------------------------------------------

function HeroSkillMgr.Init()
	log("HeroSkillMgr.Init----->>>")
	
	UpdateBeat:Add(this.Update, this)

	this.TestInitPlayerMartial()
	return this
end

function HeroSkillMgr.Update()
	if not roleMgr.mainRole or roleMgr.mainRole.charData.hp <= 0  then return end

	curTime = TimeManager.GetRealTimeSinceStartUp()

	--挂机处理
	this.OnHandCtrlEnterGuaJi()

	--连招中断处理(手动切换技能，或者时间到了)
	this.UpdateDealComboSkill(curTime)

	--是否服务端已经回了技能
	-- print("=======Update=000=======", hasSend)
	if hasSend then 
		return 
	end
	-- print("=======Update=111=======", hasSend)

	--公共cd
	if curTime - lastAttackTime < NORMAL_ATTACK_INTERVAL then
		return
	end
	-- print("=======Update==222======", hasSend)

	--当前状态是否可以放技能
	if not roleMgr.mainRole:CanSkill() then
		return
	end
	-- print("=======Update===3333=====", hasSend)

	--是否有下一个技能（自动战斗情况下，自动筛选技能）
	if fightStatus ~= 0 then
		if roleMgr.mainRole.move.isAutoRunning then
			return false
		end

		if not nextMartial then
			this.AutoSetMartial()
		end

		if IsNil(this.curTarget) then
			this.curTarget = this.SelectNextTarget()
		end
		-- print("=====Update===444====", nextMartial, this.curTarget)
		if IsNil(this.curTarget) then return end
	end

	if not nextMartial then return end

	--释放技能
	this.DoSkill(fightStatus ~= 0)
end

--------------------------------连招中断处理------------------------------------

function HeroSkillMgr.ComboSkillBreakDeal(time)
	curMartial.isOver = true
	curMartial.receiveCd = time 			--连招中断后
	curMartial.Combo = 1

	lastSendComBoSkill = 0
	SkillUiPanel.UpdateShowMartial(curMartial)
end

function HeroSkillMgr.UpdateDealComboSkill(time)
	--没有技能，或者普通攻击，或者非连击
	if not curMartial or lastSendComBoSkill <= 0 then 
		return 
	end
	
	--自动中断普攻（时间到）
	if curMartial.Mtype == 1 and (time - lastSendComBoSkill > COMBO_NORMAL_BREAK_INTERVAL) then
		this.ComboSkillBreakDeal(time)
		return
	end

	--自动中断技能（时间到）
	if curMartial.Mtype ~= 1 and (time - lastSendComBoSkill > COMBO_SKILL_BREAK_INTERVAL) then
		this.ComboSkillBreakDeal(time)
		return
	end

	--切换技能中断技能
	if nextMartial and curMartial ~= nextMartial then
		this.ComboSkillBreakDeal(time)
	end
end

--------------------------------释放技能处理------------------------------------

function HeroSkillMgr.DoSkill(auto)
	curMartial = nextMartial
	nextMartial = nil
	local skillId = curMartial.skills[curMartial.Combo]

	if auto then
		if this.DealSkillAttRange(skillId) then
			local dir = this.curTarget.transform.position - roleMgr.mainRole.move.selTrans.position
			this.SendSkill(skillId, math.deg(math.atan2(dir.z, dir.x)), this.curTarget.id, this.curTarget.transform.position)
		end
		return
	else
		if nextDir then
			this.SendSkill(skillId, math.deg(math.atan2(nextDir.z, nextDir.x)), nil, nil)
		else
			this.SendSkill(skillId, -roleMgr.mainRole.move.selTrans.eulerAngles.y + 90)
		end

		-- print("==========================", skillId)
		nextDir = nil
	end
end

--自动技能时添加距离判断(距离不够就跑过去，距离够了就直接打)
function HeroSkillMgr.DealSkillAttRange(skillId)
	if curMartial == nil then 
		return false 
	end

	if curMartial.AttAreaCenter == 1 then
		return true
	end

	local dis = Vector3.Distance(this.curTarget.gameObject.transform.position, roleMgr.mainRole.gameObject.transform.position)
	if AllSkillXls[skillId].AttRange - AUTO_FIGHT_OFFSET > dis then
		return true
	end

	-- print("==================DealSkillAttRange====")
	if roleMgr.mainRole.move.isAutoRunning then
		return false
	end

	roleMgr.mainRole.move:WalkTo(this.curTarget.gameObject.transform.position, AllSkillXls[skillId].AttRange - AUTO_FIGHT_OFFSET)
	return false
end

-----------------------------上一个技能是否返回---------------------------------------
function HeroSkillMgr.HasSendSkill()
	return hasSend
end

--------------------------------------------------------------------
--------------------------------------------------------------------
--------------------------------------------------------------------
--20180426 新项目战斗
local skillTbl = {}



--------------------------------------------------------------------
function HeroSkillMgr.AutoSetMartial()
	--连招继续
	if curMartial and this.JudgeMartialCdOk(curMartial) and curMartial.Combo <= curMartial.ComboMax then
		nextMartial = curMartial
		return 
	end

	-- 否则自动选择技能
	local tmp
	for k, v in pairs(activeMartials) do
		-- print("============", v, this.JudgeMartialCdOk(v), v.SkillPriority)
		if (v.SkillShow == 1 or v.SkillShow == 10 or ((v.SkillShow - SkillUiPanel.testSkillAdd) > 1 and (v.SkillShow - SkillUiPanel.testSkillAdd) < 6))
			and  this.JudgeMartialCdOk(v) then
			-- print("=====================", v.SkillShow, v.SkillPriority)
			if not tmp or (tmp.SkillPriority < v.SkillPriority) then
				tmp = v
			end
		end
	end

	nextMartial = tmp
end

--技能释放判断
function HeroSkillMgr.JudgeMartialCdOk(martial)
	if martial.cdTime <= 0 then return true end

	local time = TimeManager.GetRealTimeSinceStartUp()
	-- print("============", time, martial.receiveCd, martial.cdTime)
	if (time - martial.receiveCd) > martial.cdTime then
		return true
	end

	if not martial.isOver then
		return true
	end

	return false
end

--技能释放距离
function HeroSkillMgr.JudgeSkillDisctance(skill)
	
end

--------------------------------------------------------------------
--挂机逻辑
function HeroSkillMgr.SelectNextTarget()
	if roleMgr.mainRole == nil then return end

	local nextTarget
	nextTarget = this.GetNearMonsterTarget()
	if IsNil(nextTarget) then
		nextTarget = this.GetNearestPlayerTarget()
	end
	return nextTarget
end


--获取附近的怪物
function HeroSkillMgr.GetNearMonsterTarget()
	local mainRole = roleMgr.mainRole
	local nextTarget 
	local distance
	local len = roleMgr.monsters.Count
	local monster
	for i = 0, len-1 do
		monster = roleMgr.monsters[i]
		if not IsNil(monster) and monster.charData.hp > 0 
			and monster.canHeroAttack then
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
		if not IsNil(role) and role.charData.hp > 0 and role.id ~= HERO.Id 
		 and role.canHeroAttack then
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
local function SetSkills(selectMartialTbl, martialTbl)
	local index = 1
	for i = 1, #selectMartialTbl do
		local martialId = math.floor(selectMartialTbl[i]/10)
		if AllMartialXls[martialId] then
			local martial 
			for k = 1, #activeMartials do
				if activeMartials[k].martialId == martialId then
					martial = activeMartials[k]
					break
				end
			end

			if martial == nil then
				martial = {}
				martial.martialId = martialId
				martial.skills = {}
				table.insert(martial.skills, selectMartialTbl[i])

				martial.martialType = AllMartialXls[martialId].Mtype
				martial.Type = AllMartialXls[martialId].Type
				martial.cdTime = 0
				martial.receiveCd = 0
				martial.SkillShow = index
				martial.isOver = true
				martial.SkillPriority = AllMartialXls[martialId].SkillPriority
				index = index + 1

				table.insert(martialTbl, martial)
			else
				table.insert(martial.skills, selectMartialTbl[i])
			end
		end
	end

	--连招处理
	for k, v in pairs(martialTbl) do
		v.Combo = 1
		v.ComboMax = #v.skills
	end
end

--设置玩家技能
function HeroSkillMgr.SetPlayerMartial(pb)
	SetSkills(pb.skills, activeMartials)
end

--添加玩家技能
function HeroSkillMgr.AddPlayerMartial(pb)

end

--设置技能cd以及连招图片
function HeroSkillMgr.SetMartialCd(pb)
	-- print("=============SetMartialCd==========", pb.skill_id)
	if hasSend then
		hasSend = false
	end

	local martialId = math.floor(pb.skill_id/10)
	for k, v in pairs (activeMartials) do
		if v.martialId == martialId then
			v.cdTime = pb.cooltime/1000
			v.receiveCd = TimeManager.GetRealTimeSinceStartUp()
			--连招判断
			v.Combo = v.Combo + 1
			if v.Combo > v.ComboMax then
				v.Combo = 1
				SkillUiPanel.UpdateShowMartial(v)
				v.isOver = true

				if v.martialId == curMartial.martialId then
					curMartial = nil
				end
			else
				v.isOver = false
			end
			-- print("================", v.Combo, v.ComboMax)
			break
		end
	end
end

--更换技能显示设置
function HeroSkillMgr.UpdateSkillShow(pb)

end


--------------------------------------------------------------------
local normalSkillCombo = 1

-- local selectMartialTbl = {10000011, 10000021, 10000031, 10000041, 10000051, 10000061, 10000071, 10000081}

--技能按钮点击	//1普通攻击 2-后面的技能
function HeroSkillMgr.SetHeroNextMartial(index, dir)
	--手动处理打断挂机
	this.OnHandCtrlBreakGuaJi()

	--设置下一招
	local martial = this.GetMartialByBtnIndex(index)

	if not martial then
		error("===============位置的技能" .. index)
		return
	end

	nextMartial = martial
	nextDir = dir
end

--点击了技能
function HeroSkillMgr.SkillBtnClick()
	if roleMgr.mainRole == nil or roleMgr.mainRole.hp <= 0 or (Time.time -lastAttackTime) < normalAttackGap then
		return 
	end

	if FightLogic.GetCanCastSkill() == false then return end

	AutoSetMartial()

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
	if not AllSkillXls[skillId] or not AllSkillXls[skillId].AttAreaShow then return end

	if AllSkillXls[skillId].AttAreaCenter == 1 then
		if AllSkillXls[skillId].AttAreaShow.shape == 2 then
			return SkillAreaType.OuterCircle, 0, AllSkillXls[skillId].AttAreaShow.r, 1, 1
		else
			return this.skillAreaType[AllSkillXls[skillId].AttAreaShow.shape + 1]
				, 120--AllSkillXls[skillId].AttAreaShow.shape == 3 and AllSkillXls[skillId].AttAreaShow.angle or 0
				, AllSkillXls[skillId].AttRange
				, AllSkillXls[skillId].AttAreaShow.r or AllSkillXls[skillId].AttAreaShow.length
				, AllSkillXls[skillId].AttAreaShow.r or AllSkillXls[skillId].AttAreaShow.width
		end
	else
		return this.skillAreaType[AllSkillXls[skillId].AttAreaShow.shape + 1]
			, 120--AllSkillXls[skillId].AttAreaShow.shape == 3 and AllSkillXls[skillId].AttAreaShow.angle or 0
			, AllSkillXls[skillId].AttRange
			, AllSkillXls[skillId].AttAreaShow.r or AllSkillXls[skillId].AttAreaShow.length
			, AllSkillXls[skillId].AttAreaShow.r or AllSkillXls[skillId].AttAreaShow.width
	end
end

function HeroSkillMgr.JudgeCanCastSkill(skillId)
	if not roleMgr.mainRole then 
		print("=====没有主角")
		return false 
	end

	if not roleMgr.mainRole.roleState:CanSkill() then
		print("=====主角当前状态不能释放技能")
		return false 
	end
	
	return true
end

--获取按钮对应的技能
function HeroSkillMgr.GetMartialByBtnIndex(index)
	if not activeMartials then 
		return 
	end

	if index == 0 then return activeMartials[10] end
	if index == 1 then return activeMartials[1] end

	local martial 
	for k, v in pairs(activeMartials) do
		if v.SkillShow == index then
			martial = v
			break
		end
	end

	return martial
end

--------------------------------------------------------------------
--协议相关

--发送锁敌，用于小弟攻击目标
function HeroSkillMgr.SendToLockEnemy(id)
	Network.send("C2s_aoi_skilllock", {tar_id = id})
end

--发送技能
function HeroSkillMgr.SendSkill(skillId, dir, targetId, targetPos)
	-- print("====SendSkill=============11111111111==============", skillId)
	local skillXls = AllSkillXls[skillId]
	if not skillXls then return end

	-- print("====SendSkill=============22222222222==============")
	local cmd = {}

	cmd.skill_id = skillId
	cmd.dir = dir --math.deg(math.atan2(dir.z, dir.x))
	if targetPos then
		cmd.mtar_id = targetId
		cmd.tx = targetPos.x
		cmd.tz = targetPos.z
	elseif targetId then
		cmd.mtar_id = targetId
		cmd.tx = targetPos.x
		cmd.tz = targetPos.z
	elseif AllSkillXls[skillId].AttAreaCenter == 2 then
		cmd.tx = roleMgr.mainRole.move.selTrans.position.x
		cmd.tz = roleMgr.mainRole.move.selTrans.position.z
	end

	if skillXls.MoveLength and skillXls.MoveLength ~= 0 then

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
		-- print("----------------------", cmd.move_x, cmd.move_z)
	end

	--发送协议给服务端
	Network.send("C2s_aoi_skillact_bycharpos", cmd)
	-- 客户端先行表现
	FIGHTMGR.StartSkill(cmd, true)

	hasSend = true
	-- print("====SendSkill=============3333333333333==============", TableToString(cmd))
	--公共cd
	lastAttackTime = TimeManager.GetRealTimeSinceStartUp()
	if curMartial.ComboMax > 1 then
		lastSendComBoSkill = lastAttackTime
	end

	--普攻处理
	if AllMartialXls[math.floor(skillId/10)].Mtype == 1 then
		normalSkillCombo = normalSkillCombo + 1
		normalSkillCombo = normalSkillCombo > 4 and 1 or normalSkillCombo
	end
end

--我闪
function HeroSkillMgr.SkillDodge()
	--当前技能不能打断
	if AllSkillXls[this.curActSkill] and (not AllSkillXls[this.curActSkill].CanBreak or AllSkillXls[this.curActSkill].CanBreak == 0) then return end

	local cmd = {}
	cmd.dir = -roleMgr.mainRole.move.selTrans.eulerAngles.y + 90
	local moveEnd = roleMgr.mainRole.move.selTrans.position + Vector3.New(math.cos(cmd.dir*math.pi/180), 0, math.sin(cmd.dir*math.pi/180))*DODGE_DISTANCE
	local movePos = SceneHelper.Linecast(roleMgr.mainRole.move.selTrans.position, moveEnd)
	if movePos == Vector3.zero then
		cmd.x = moveEnd.x 
		cmd.z = moveEnd.z
	else
		cmd.x = movePos.x 
		cmd.z = movePos.z
	end
	Network.send("C2s_aoi_dodge", cmd)

	--客户端先行
	FIGHTMGR.SkillDodge(roleMgr.mainRole, cmd.x, cmd.z)

	-- hasSend = true
end

--------------------------------技能错误相关------------------------------------
function HeroSkillMgr.SkillErrorDeal()
	if hasSend then
		hasSend = false
	end

	this.curActSkill = 0
	if curMartial then
		curMartial.isOver = true
		curMartial.Combo = 1
		--连招errorcd问题
		curMartial.receiveCd = TimeManager.GetRealTimeSinceStartUp() 			--连招中断后
	end
end

--------------------------------------------------------------------
function HeroSkillMgr.GetCurMartials()
	return activeMartials
end

--------------------------------------------------------------------
--获取技能图标
function HeroSkillMgr.GetSkillIcon(skillId)
	if not AllSkillXls[skillId] or not AllSkillXls[skillId].SkillIcon then return end

	-- print("==========GetSkillIcon=========", )
	return resMgr.LoadSpriteFromAtlasBundle(AllSkillXls[skillId].SkillIcon, "Atlas/main/main-0") 
end

--------------------------------------------------------------------
--挂机处理
function HeroSkillMgr.OnGuaJiBtnClick()
	isGuaJiOn = not isGuaJiOn
	this.SetFightStatus(isGuaJiOn and 2 or 0)

	SkillUiPanel.SetGuaJiBtnState(isGuaJiOn)
end

function HeroSkillMgr.SetFightStatus(status)
	if fightStatus ~= 2 and status == 2 then
		MainCtrl.PopUpNotifyText("开始挂机！")
	elseif fightStatus == 2 and status ~= 2 then
		MainCtrl.PopUpNotifyText("退出挂机！")
	end

	fightStatus = status
end

--手动操作打断挂机
function HeroSkillMgr.OnHandCtrlBreakGuaJi()
	-- print("==========OnHandCtrlBreakGuaJi===================", isGuaJiOn)
	if not isGuaJiOn then return end

	lastOutGuaJiTime = TimeManager.GetRealTimeSinceStartUp()
	if fightStatus == 2 then
		fightStatus = 0
		MainCtrl.PopUpNotifyText("退出挂机！")
		
		this.CancelTarget()
	end
end

--时间到继续挂机
function HeroSkillMgr.OnHandCtrlEnterGuaJi()
	if isGuaJiOn and fightStatus == 0 and (curTime - lastOutGuaJiTime) > AUTO_FIGHT_BREAK_INTERVAL then
		this.SetFightStatus(2)
	end
end


function HeroSkillMgr.CancelTarget()
	this.curTarget = nil
	nextMartial = nil
end

--------------------------------------------------------------------
--设置玩家闪
function HeroSkillMgr.SetHeroDodge(dodge)
	this.receiveDodgeValue = dodge
	this.receiveDodgeTime = TimeManager.GetRealTimeSinceStartUp()
	this.curDodge = math.floor(this.receiveDodgeValue/this.ONE_DODGE_VALUE)

	-- SkillUiPanel.UpdateDodgeShow(this.receiveDodgeValue, this.receiveDodgeTime)
end

--------------------------------------------------------------------
--测试
function HeroSkillMgr.TestInitPlayerMartial()
	local cmd = {}
	cmd.skills = {10000001, 10000002, 10000003, 10000004, 10000011, 10000021, 10000031, 10000041, 10000051, 10000061, 10000071, 10000081, 10300001}
	activeMartials = {}
	this.SetPlayerMartial(cmd)
end
--------------------------------------------------------------------

return HeroSkillMgr