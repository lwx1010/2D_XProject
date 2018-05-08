local roleMgr = roleMgr
local HEROSKILLMGR = HEROSKILLMGR
local HERO = HERO
local FightLogic = FightLogic
local Time = Time
local allSkillXls = require "xlsdata/Skill/SkillXls"
local allMartialXls = require "xlsdata/Skill/MartialXls"
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

--设置技能目标相关
function FightMgr.SetTargets( skill, targets, targetId, skillType, targetPos, htype, needChangeDir)
	if targets ~= nil then
		-- print("------------------------11111111111111111----------------------")
		--skill.hurts = targets
		if #targets > 0 then
			if roleMgr:GetSceneEntityById(targets[1].tar_id, 0) ~= nil then
				skill.targetGo = roleMgr:GetSceneEntityById(targets[1].tar_id, 0)

				if skillType == SKILL_SINGLE then
					skill.targetPos = skill.targetGo.transform.position

					if (not htype or htype == 1 or htype == 0) and needChangeDir then
						skill.startGo.gameObject.transform:LookAt(Vector3.New(skill.targetGo.transform.position.x, skill.startGo.transform.position.y, skill.targetGo.transform.position.z))
					end
				end
			end
		end
	elseif roleMgr:GetSceneEntityById(targetId, 0) ~= nil then
		-- print("------------------------22222222222222222----------------------")

		skill.targetGo = roleMgr:GetSceneEntityById(targetId, 0)
		if  skillType == SKILL_SINGLE then
			skill.targetPos = skill.targetGo.gameObject.transform.position
			if (not htype or htype == 1 or htype == 0) and needChangeDir then
				-- print("------------------aaaaa---------", Time.time, skill.targetGo.transform.position.x, skill.startGo.gameObject.transform.position.y, skill.targetGo.transform.position.z )
				skill.startGo.gameObject.transform:LookAt(Vector3.New(skill.targetGo.transform.position.x, skill.startGo.gameObject.transform.position.y, skill.targetGo.transform.position.z))
			end
		elseif targetPos == Vector3.New(0,0,0) then
			targetPos = skill.targetGo.transform.position
		end
	end

	if skillType == SKILL_AOE then
		-- print("------------------------33333333333333333----------------------")

		-- print("------------------SetTargets---------", Time.time, targetPos.x, targetPos.y, targetPos.z )
		skill.targetPos = targetPos
		if targetPos ~= Vector3.New(0,0,0) and (not htype or htype == 1 or htype == 0) and needChangeDir then
			skill.startGo.gameObject.transform:LookAt(Vector3.New(targetPos.x, skill.startGo.gameObject.transform.position.y, targetPos.z))
		end
	end
end

--释放技能前处理
--添加技能动作状态
--可以移动的技能需要添加技能移动状态
--不可以移动的技能需要停止移动，面向技能方向
function FightMgr.DoPreSkill(attacker, isSelf, dir, skillXls)
	if not attacker then return end

	if isSelf then
		roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.SkillActing)
		if skillXls.CanMove == 1 then
			roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.SkillMoving)
		end
	end

	attacker.move:StopToDir(dir)
end

--广播协议(玩家或npc技能协议按队列进行处理)
-- @param: isDequeue 是否是队列出栈消息
function FightMgr.StartSkill(pb, isSelf, isDequeue)
	if not allSkillXls[pb.skill_id] then return end

	local attacker
	if isSelf then
		attacker = roleMgr.mainRole
	else
		attacker = roleMgr:GetSceneEntityById(pb.att_id)
	end

	this.DoPreSkill(attacker, isSelf, pb.dir, allSkillXls[pb.skill_id])
	
	if attacker then
		attacker.skillCtrl:UseSkillByTabel(allSkillXls[pb.skill_id],  (pb.tx and (not pb.mtar_id or #pb.mtar_id == 0)) and SceneHelper.GetNearestPos(pb.tx, pb.tz) or Vector3.zero, true)
	end
end

--广播技能击中协议(玩家或npc技能协议按队列进行处理)
-- @param: isDequeue 是否是队列出栈消息
function FightMgr.SkillHit( pb, isDequeue )
	if not allSkillXls[pb.skill_id] then return end

	--如果calcnt 字段为0和nil， 且没有前摇的时候，服务端会把act协议优化掉，收到这样的hit协议时，需要播放动作
	if ( not pb.calcnt or pb.calcnt == 0) and (not allSkillXls[pb.skill_id].BeforeHurtTime or allSkillXls[pb.skill_id].BeforeHurtTime <= 0) then
		this.StartSkill(pb)
	end

	local attacker = roleMgr:GetSceneEntityById(pb.att_id)
end

--动作结束需要重置主角状态
function FightMgr.DoAfterSkill(attacker)
	if not attacker then return end

	if attacker.entityType == EntityType.EntityType_Self then
		roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.SkillActing)
		if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.SkillMoving) then
			roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.SkillMoving)
		end
	end
end

--获取字体字符串
local function GetFontString(attackertype, targettype, skilltype, hp)
	--print("131313 ", attackertype, targettype, skilltype, hp)
	if skilltype == 11 then
		return "Atlas/font/NumFont_Weapon", "%" .. math.abs(hp)
	end

	if attackertype == 1 and hp < 0 then
		if targettype == 1 then
			return "Atlas/font/NumFont_Hurt", math.abs(hp)
		elseif skilltype == 1 then	--宠物/标识，&暴击
			return "Atlas/font/NumFont_Pet", "/&" .. math.abs(hp)
		else
			return "Atlas/font/NumFont_Pet", "/" .. math.abs(hp)
		end
	elseif attackertype == 2 and hp < 0 then
		if targettype == 1 then
			return "Atlas/font/NumFont_Hurt", math.abs(hp)
		else
			return "Atlas/font/NumFont_Weapon", "&" .. math.abs(hp)
		end
	end

	if hp > 0 then
		return "Atlas/font/NumFont_Add", "+" .. math.abs(hp)
	end

	if skilltype == 1 then
		if targettype == 1 then
			return "Atlas/font/NumFont_Hurt", math.abs(hp)
		else
			return "Atlas/font/NumFont_Double", "&" .. math.abs(hp)
		end
	end

	if skilltype == 0 then
		if targettype == 1 then
			return "Atlas/font/NumFont_Hurt", math.abs(hp)
		else
			return "Atlas/font/NumFont_Monster", math.abs(hp)
		end
	end

	if skilltype == 9 then
		return "Atlas/font/NumFont_Monster", "&" .. math.abs(hp)
	end
end

--伤害显示
function FightMgr.ShowHurt( attacker, objType, tar_id, hp, skilltype, nhp, hpmax, skillId, isdie)
	--UI自己血量更新
	if HERO.Id == tar_id then
		local MainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)

		if roleMgr.mainRole and skilltype ~= 11 then
			MainCtrl.UpdatePlayerBarHp(roleMgr.mainRole.hp > nhp and nhp or roleMgr.mainRole.hp, hpmax)
		end
	end

	if attacker and (HERO.Id == attacker.id) then
		--UI目标血量更新
		--HEROSKILLMGR.UpdateTargetBlood(tar_id, nhp, hpmax)

		--设置为目标
		if nhp > 0 then
			HEROSKILLMGR.SetCurTarget(tar_id)
		end
	-- elseif objType > 0 and attacker and (attacker.ownerId == HERO.Id) then
	-- 	--UI目标血量更新
	-- 	HEROSKILLMGR.UpdateTargetBlood(tar_id, nhp, hpmax)
	end

	--0:普通伤害(加血),1:暴击伤害,2:破格伤害,3:暴击破格伤害,4:miss,5:无敌,6.非攻击技能加减血,7:加buff,8：跳闪，9：buff加减血, 10,打坐， 11护盾 12隐身无敌
	if skilltype == 7 or skilltype == 2 or skilltype == 3 or skilltype == 6 or skilltype == 12 or skilltype == 13 then
		return
	end

	--skilltype = 4
	local target = roleMgr:GetSceneEntityById(tar_id, 0)
	if target == nil then return end

	local pos = FightLogic:GetTargetHurtPos(target) or target.gameObject.transform.position
	local targettype = Util.GetEntityType(target)
	--这里存在攻击者的时候要设置血量位置偏移，暂时不加
	local dir = 0

	if attacker and tar_id ~= HERO.Id then
		dir = Util.WorldToUI(target.gameObject.transform.position).x - Util.WorldToUI(attacker.gameObject.transform.position).x
	end

	--弹出显示
	if skillId > 0 then
		if allSkillXls[skillId] == nil then
			logWarn("333找不到技能" .. skillId)
			this.ShowHpText(skilltype, objType, targettype, pos, hp, dir)
			return
		end

		if #allSkillXls[skillId].DivideShow == 0 or #allSkillXls[skillId].DivideShow ~= #allSkillXls[skillId].DivideDelay then
			this.ShowHpText(skilltype, objType, targettype, pos, hp, dir)
			return
		end

		for i = 1, #allSkillXls[skillId].DivideShow do
			local co
			co = coroutine.start(function()
				coroutine.wait(allSkillXls[skillId].DivideDelay[i])
				target = roleMgr:GetSceneEntityById(tar_id, 0)
				if isdie or not target then
					this.ShowHpText(skilltype, objType, targettype, pos, math.floor(hp*allSkillXls[skillId].DivideShow[i]/100), dir)
				else
					--print("-----------", target)
					pos = FightLogic:GetTargetHurtPos(target) or target.gameObject.transform.position
					this.ShowHpText(skilltype, objType, targettype, pos, math.floor(hp*allSkillXls[skillId].DivideShow[i]/100), dir)
				end
				coroutine.stop(co)
			end)
		end
	else
		this.ShowHpText(skilltype, objType, targettype, pos, hp, dir)
	end

end

function FightMgr.ShowHpText(skilltype, attackertype, targettype, pos, hp, dir)
	local MainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	local parent = MainCtrl.GetHpWidget()
	if parent == nil then
		return
	end

	local hpGo = GameObject.New("hptest")
	if skilltype == 4 or skilltype == 5 then
		--显示闪避或者无敌
		local spr = hpGo:AddComponent(typeof(UISprite))
		Util.SetUISprite(spr, "Atlas/main/main", "ui_word_shanbi")
	elseif skilltype == 8 then
		--显示跳闪
		local spr = hpGo:AddComponent(typeof(UISprite))
		Util.SetUISprite(spr, "Atlas/main/main", "ui_word_shantiao")
	else
		--显示伤害
		local lab = hpGo:AddComponent(typeof(UILabel))
		local fontstr, text = GetFontString(attackertype, targettype, skilltype, hp)
		Util.SetUILabelFont(lab, fontstr, 0, 2)
		lab.text = text
	end

	hpGo.layer = 5

	hpGo.transform:SetParent(panelMgr.hpRoot)
	local factor = targettype == 1 and 0.8 or 1		--主角自己受到的伤害显示缩放为0.5
	hpGo.transform.localScale = Vector3.New(1,1,1)*factor
	hpGo.transform.position = Util.WorldToUI(pos)

	local mHight = 100
	local moveTime = 0.5
	local moveHeight = 50

	local vecList = {}
	table.insert(vecList, Vector3.New(1.5, 1.5, 1.5)*factor)
	table.insert(vecList, Vector3.New(1, 1, 1)*factor)

	local mTimes = {}
	table.insert(mTimes, 0.5)
	table.insert(mTimes, 0.3)

	--print("------------", dir)
	local dirWidth
	if attackertype == 1 then
		dirWidth = dir == 0 and 0 or 210*dir/math.abs(dir)
	elseif attackertype == 2 then
		dirWidth = dir == 0 and 0 or 210*dir/math.abs(dir)
	else
		dirWidth = dir == 0 and 0 or 230*dir/math.abs(dir)
	end

	this.DoTweenHp(hpGo, vecList, mTimes, mHight*(0.5+ math.random()), dirWidth, moveTime, moveHeight)
end

local function OnAnimationEnd( go )
	local widget = go.transform:GetComponent("UIWidget")
	DOTween.To(DG.Tweening.Core.DOGetter_float(function()return widget.alpha end),
		DG.Tweening.Core.DOSetter_float(function(x) widget.alpha = x end),
		0, 0.5):OnComplete(function() destroy(go) end)
end

local function OnMoveEnd( go, moveTime, moveHeight)
	go.transform:DOLocalMoveY(go.transform.localPosition.y + moveHeight, moveTime, false
		):SetEase(DG.Tweening.Ease.Linear
		):OnComplete(function() OnAnimationEnd(go) end)
end

local function OnScaleEnd(scaleObj, vecList, mTimes)
	local len = #vecList
	if scaleObj.nowScaleNo < len then
		scaleObj.nowScaleNo = scaleObj.nowScaleNo + 1
		scaleObj.go.transform:DOScale(vecList[scaleObj.nowScaleNo], mTimes[scaleObj.nowScaleNo]
			):SetEase(DG.Tweening.Ease.OutCubic
			):OnComplete(function() OnScaleEnd(scaleObj, vecList, mTimes) end)
	end
end

--弹血
function FightMgr.DoTweenHp(hpGo, vecList, mTimes, mHight, dirWidth, moveTime, moveHeight)
	if not hpGo then return end

	local totalTime = 0
	for k, v in pairs(mTimes) do
		totalTime = totalTime + v
	end

	hpGo.transform:DOLocalMove(Vector3.New(
		hpGo.transform.localPosition.x + dirWidth,
		hpGo.transform.localPosition.y + mHight, 0),
		totalTime, false
	):SetEase(DG.Tweening.Ease.OutBack):OnComplete(function() OnMoveEnd(hpGo, moveTime, moveHeight) end)

	if not vecList or #vecList ~= #mTimes then return end

	local scaleObj = {}
	scaleObj.go = hpGo
	scaleObj.nowScaleNo = 1
	hpGo.transform:DOScale(vecList[1], mTimes[1]
		):SetEase(DG.Tweening.Ease.OutCubic
		):OnComplete(function() OnScaleEnd(scaleObj, vecList, mTimes) end)
end

--判断对象是否可用攻击(自己以及目标的攻击模式，阵营，队伍等等)
function FightMgr.ChargeTargetCanAttack()
	return true
end

function FightMgr.ClearHpPopUpCourtine(  )

end

--------------------------------------------------------------------------------------------------
--冒经验显示
local expList = {}
local lastPopTime = 0
local POP_EXP_TIME = 0.3

function FightMgr.AddShowExp(exp)
	if (Time.time - lastPopTime) > POP_EXP_TIME then
		this.ShowExpText(exp)
		lastPopTime = Time.time
	elseif #expList > 0 then
		table.insert(expList, exp)
	else
		table.insert(expList, exp)
		this.CoroutineShowExp()
	end
end

function FightMgr.CoroutineShowExp()
	if #expList == 0 then return end

	coroutine.start(function()
			coroutine.wait(POP_EXP_TIME - (Time.time - lastPopTime))
			this.ShowExpText(table.remove(expList, 1))
			lastPopTime = Time.time
			this.CoroutineShowExp()
		end)
end

function FightMgr.ShowExpText(exp)
	local MainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	local parent = MainCtrl.GetHpWidget()
	if parent == nil then
		return
	end

	if not roleMgr.mainRole then return end

	local expGo = GameObject.New("expGo")
	local lab = expGo:AddComponent(typeof(UILabel))
	Util.SetUILabelFont(lab, "Atlas/font/NumFont_Add", 0, 2)
	lab.text = "+ &" .. exp

	expGo.layer = 5

	expGo.transform:SetParent(parent.transform)
	expGo.transform.localScale = Vector3.New(1,1,1)
	expGo.transform.position = Util.WorldToUI(FightLogic:GetTargetHeadPos(roleMgr.mainRole))

	local mHight = 110
	local moveTime = 0.2
	local moveHeight = 30

	local vecList = {}
	table.insert(vecList, Vector3.New(1.1, 1.1, 1.1))
	table.insert(vecList, Vector3.New(1, 1, 1))

	local mTimes = {}
	table.insert(mTimes, 0.3)
	table.insert(mTimes, 0.1)

	this.DoTweenHp(expGo, vecList, mTimes, mHight, 0, moveTime, moveHeight)
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