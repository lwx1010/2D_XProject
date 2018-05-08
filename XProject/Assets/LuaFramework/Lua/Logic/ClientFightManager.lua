--local MainCtrl = require "Controller/MainCtrl"
local FIGHTMGR = FIGHTMGR
local HERO = HERO
local Util = Util
local GAMECONST = GAMECONST
local FUBENLOGIC = FUBENLOGIC
-- local lua_Other = lua_Other

local ClientFightManager = {}
local this = ClientFightManager

--计时器间隔
local needDelay = true
local clientDelay = 1
local lastTimer = 0
local TIME_INTERVAL = 0.1

--local recieveTime
local curBattlePb

--爬塔相关参数
local curFType 			--副本类型
local isNext 			--0表示没有下一层,如果是0打完则需要发C2s_battle_fight_end,1打完则获取下一波怪
--local curLayer			--当前正在打的层数
local curGateNo			--当前爬塔的关卡no

--战斗类型
this.BATTLE_TYPE = 
{
	BATTLE_TYPE_SAMPLE	= 1,		--普通类型
	BATTLE_TYPE_FUBEN_PG = 2,		--通关副本
	BATTLE_TYPE_FUBEN_PT = 3,		--爬塔副本
	BATTLE_TYPE_FUBEN_TF = 4,		--塔防副本
	BATTLE_TYPE_FUBEN_BS = 5		--BOSS引导
}

local WARRIOR_DATA_NAME_CLIENT = {						--战斗属性变量
	"Comp",
	"AutoFight",
	"Dir360",
	"PkMode",
	"TeamId",
	"ClubId",
	"EvilState",
	"EvilTime",
	"EvilValue",
	"ServerId",
	"Grade",
	"SeeNpcNoList",
	"CantAttackList",
	"Score",
	"LingqiModel",
	"ThugModel",
	"DaZuo",
	"Fashion",
--	"LingqiModel",		--养成之灵器外形
	"LingqinModel",		--养成之灵琴外形
	"LingyiModel",		--养成之灵翼外形
--	"ThugModel",		--养成之伙伴外形
	"ThugHorseModel",	--养成之灵骑外形
	"PetModel",			--养成之宠物外形
	"ShenjianModel",	--养成之神剑外形
	"ShenyiModel",		--养成之神翼外形
--	"JingmaiModel",		--养成之经脉外形
	"MountModel",		--坐骑外形
	"UpMountModel",		--上下马状态
	"UpHorseModel",		--伙伴上下马状态
	"ActivateWeapon",	--武器外形编号
	
	"LingqiModelState",		--显示/隐藏灵器外形
	"LingqinModelState",	--显示/隐藏灵琴外形
	"LingyiModelState",		--显示/隐藏灵翼外形
	"ThugModelState",		--显示/隐藏伙伴外形
	"ThugHorseModelState",	--显示/隐藏灵骑外形
	"PetModelState",		--显示/隐藏宠物外形
	"ShenjianModelState",	--显示/隐藏神剑外形
	"ShenyiModelState",		--显示/隐藏神翼外形
	
	"IsYunBiao",		--是否护送
	
	"ClubName",
	"ClubPost",
	
	"Vip",
	
	"DoubleXiulian",		--双修
	"DoubleXiulianEffect",	--双修特效
	
	"Title",				--称号
	"Photo",				--头像

	"Id",
	"Grade",
	"Name",
	"Ap",
	"Dp",
	"Ma",
	"Mr",
	"Hp",
	"HpMax",
	"Speed",
	
	"HitRate",
	"Dodge",
	
	"Double",
	"Tenacity",
	"Parry",
	"ReParry",
	
	"DoubleHurt",
	"ReDoubleHurt",
	
	"Hurt",
	"ReHurt",
	"AbsHurt",
	
	"AtkTime",

	"PartnerAp"	,	--伙伴攻击
	"PartnerHurt",	--伙伴伤害
	"PartnerReHurt",	--伙伴伤害减免
	"ReSlow",		--减速免疫
	"ReDizzy",		--眩晕免疫
	"ReBleed",		--流血免疫
	"PartnerExtraHurt",	--伙伴额外伤害

	"MagicAp",
	"MagicHurt",
}

--------------------------------------------------------------------------------
function IsVfdType(vfd)
	--return type(vfd) == type(1)
	return type(vfd) == type("")
end

--服务端lua调用
function pbc_send_msg(vfd, protoName, protoData)
	--print("========pbc_send_msg=========", protoName, protoData)
	local typeVfd = type(vfd)
	if typeVfd == "string" then
		if vfd ~= "1" then return end
	elseif typeVfd =="table" then
		local isOk = false
		for _, _vfd in pairs(vfd) do
			if _vfd == "1" then
				isOk = true
				break
			end
		end
		if not isOk then return end
	end
	
	if Network[protoName] then
		Network[protoName](protoData)
	else
		print("不存在的协议处理", protoName)
	end
end

function GetMapYByXAndZ(x, y)
	--print("--GetMapYByXAndZ---", x, y)
	local pos = Util.Convert2RealPosition(x, y)
	-- print("---------111---------", pos.x, pos.y, pos.z)
	-- if pos.y == 0 then
	-- 	print(debug.traceback())
	-- end
	if pos.y == 0 and pos.x == 0 and pos.z == 0 then 
		return 
	end

	return pos.y
end

function GetMapPosByRealPos(x, y)
	local pos = Util.Convert2MapPosition(x, GetMapYByXAndZ(x, y), y)
	return pos.x, pos.z
end

function GetRealPosByMapPos(gx ,gy)
	local pos = Util.Convert2RealPosition(gx ,gy)
	return pos.x, pos.z, pos.y
end

function SendToClientFightEnd(iswin, diecnt, sp, extend)
	--print("---------FightEnd--------", iswin, diecnt, FIGHTMGR.fightType)
	local GuideLogic = require("Logic/GuideLogic")
	if iswin == 1 and FIGHTMGR.fightType == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_PT and isNext == 1
		and not GuideLogic.IsGuideStopFuBen(curGateNo) then
		--客户端爬塔一波战斗结束，获取下一波战斗数据

		--print("----------1111-------")
		local cmd = {}
		cmd.fid = FIGHTMGR.fightId
		cmd.ftype = curFType
		cmd.gateno = curGateNo
		cmd.extend = extend
		local str = string.format("fid=%s,extend=%s,mno=%s,ftype=%s,gateno=%s", cmd.fid, cmd.extend, FIGHTMGR.mno, cmd.ftype, cmd.gateno)
		cmd.sign = string.sub(MD5.ComputeString(str), 9, 24)
		cmd.cvar = CHECKEND.GetFightEndCheckVar() or {}
		Network.send("C2s_battle_pt_next", cmd)
	elseif iswin == 1 and FIGHTMGR.fightType == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_TF and isNext == 1
		and not GuideLogic.IsGuideStopFuBen(curGateNo) then
		--客户端爬塔一波战斗结束，获取下一波战斗数据

		--print("----------1111-------")
		local cmd = {}
		cmd.fid = FIGHTMGR.fightId
		cmd.ftype = curFType
		cmd.gateno = curGateNo
		cmd.extend = extend
		local str = string.format("fid=%s,extend=%s,mno=%s,ftype=%s,gateno=%s", cmd.fid, cmd.extend, FIGHTMGR.mno, cmd.ftype, cmd.gateno)
		cmd.sign = string.sub(MD5.ComputeString(str), 9, 24)
		cmd.cvar = CHECKEND.GetFightEndCheckVar() or {}
		Network.send("C2s_battle_pt_next", cmd)

	else
		--客户端战斗结束
		--验证
		local cmd = {}
		cmd.fid = FIGHTMGR.fightId
		cmd.iswin = iswin
		cmd.extend = extend
		cmd.sp = FIGHT_EVENT.GetPlayerSp()
		local str = string.format("fid=%s,extend=%s,mno=%s,iswin=%s", cmd.fid, cmd.extend, FIGHTMGR.mno, cmd.iswin)
		--print("-----------",  cmd.fid, cmd.extend, FIGHTMGR.mno, cmd.iswin, str, cmd.sp)
		cmd.sign = string.sub(MD5.ComputeString(str), 9, 24)
		cmd.cvar = CHECKEND.GetFightEndCheckVar() or {}
		Network.send("C2s_battle_fight_end", cmd)

		--客户端暂停
		MainCtrl.TimeBarStop(true)

		--boss引导战斗，停止引导
		local BossGuideLogic = require("Logic/BossGuideLogic")
		if BossGuideLogic.IsInBossGuideScene then
			BossGuideLogic.guideOver = true
			BossGuideLogic.ClearGuide()
		end
	end
end

function ClientSendPbToLua(pbname, pb)
	lua_ClientProto2(pbname, pb)
	--print("=====ClientSendPbToLua=========", pbname, TableToString(pb))
end

function FuBenMonsterDie(npcNo)
	MainCtrl.UpdateFinishCondition(npcNo)
end

--md5码转换
function ConvertToMD5String(str)
	return MD5.ComputeString(AttrStr)
end

function ServerToFuBenServer(data)
	--客户端战斗
	lua_Other(data)
end

--灵源副本信息
function SetLingYuanFbGateInfo(layer, wave, wavemax)
	-- print("=-=====SetLingYuanFbGateInfo========", layer, wave, wavemax)

	if (wave - 1) > 0 and (wave - 1) <= wavemax then
		local cmd = {}
		cmd.ftype = GAMECONST.FuBenFType.TYPE_LINGYUAN
		cmd.nlayer = layer
		cmd.waveno = wave - 1
		-- print("===========sendtoserver=============", layer, wave-1)
		Network.send("C2s_fuben_tf_wave", cmd)
	end

	FUBENLOGIC.LingYuanBarUpdate(layer, wave, wavemax)
end

function SetLingYuanFbWaveTime(time, nextwave, maxwave)
	-- print("---------------SetLingYuanFbWaveTime-----------------", time, nextwave, maxwave)
	FUBENLOGIC.LingYuanBarUpdateTime(time, nextwave, maxwave)
end

function SetLingYuanFbMonsterCnt(name, diecnt, maxcnt)
	-- print("=----------SetLingYuanFbMonsterCnt-------------------------------", name, diecnt, maxcnt)
	FUBENLOGIC.LingYuanBarUpdateMonsterInfo(name, diecnt, maxcnt)
end

function SetLingYuanFbTimeTip()
	FUBENLOGIC.SetLingYuanFbNoneTimeTip()
end
---------------------------------------------------------------------------------------
--战斗触发对话
function ShowFightTalk(talkNo, npcNo)
	--print("--------ShowFightTalk-----------", talkNo, npcNo)
	CtrlManager.GetCtrl(CtrlNames.Main).ShowFightTalk(talkNo, npcNo)
end
---------------------------------------------------------------------------------------
--野外boss引导
--归属变更
function BossGuideSetGuiShu(show, id)
	-- print("========BossGuideSetGuiShu========", show, id)
	local BossGuideLogic = require("Logic/BossGuideLogic")
	BossGuideLogic.SetGuiShu(show, id)
end

--触发队伍相关引导
-- function BossGuideSetTeam()
-- 	-- print("========BossGuideSetTeam========", TableToString(data))
-- 	local BossGuideLogic = require("Logic/BossGuideLogic")
-- 	BossGuideLogic.BossGuideSetTeam(lua_FubenBsTeam)
-- end
---------------------------------------------------------------------------------------

--------------------------------------------------------------------------------

local UnLoad = true
local function Enter(...)
	local func, err = loadfile("map/client/preload.lua")
	--print(...)
	func(UnLoad, ...)
	UnLoad = false
end

--初始化
function ClientFightManager.FightLuaInit(pb)
	if not UnLoad then
		lua_Clear()
	end

	Enter(pb.map_no, pb.map_id, pb.maxx, pb.maxy)

	this.fightType = pb.type
	this.fid = pb.fid
	this.mapno = pb.mno
	this.enterDir = pb.dir

	local buffstr = ""
	for k,v in pairs( pb.initbuff_list) do
		if v.id == HERO.Id then
			buffstr = v.initbuff
			break
		end
	end

	local tbl = this.CreateHeroFightTable(pb.x, pb.y, pb.z, buffstr)

	--创建主角
	lua_AddUserToMapByTbl("1", tbl)

	curBattlePb = pb
	--recieveTime = TimeManager.GetRealTimeSinceStartUp()

	Event.AddListener(EventType.Hero_Attrs, this.UpdateHeroAttrs)
end

function ClientFightManager.UpdateServerFlyDodge(value)
	if curFType and curFType > 0 then
		lua_ClientChangeData("FlyDodge", value)
	end
end

--玩家属性更新
function ClientFightManager.UpdateHeroAttrs(attrs)
	for k,v in pairs(attrs) do
		if isInTable(WARRIOR_DATA_NAME_CLIENT, k) or k == "MartialTable"
			or k == "ChooseMagicMartial" then
			lua_ClientChangeData(k, v)
		end
	end
end

function ClientFightManager.UpdatePlayerDir()
	if curBattlePb and roleMgr.mainRole then
		roleMgr.mainRole:ChangeDir(this.enterDir)
	end
end

function ClientFightManager.DelayToCreateFuBenFight()
	if curBattlePb  then 	
		--设置战斗类型
		FIGHTMGR.SetFightInfo(curBattlePb.type, curBattlePb.fid, curBattlePb.mno)

		--创建怪物
		local time = 0
		-- if roleMgr.mainRole then
		-- 	roleMgr.mainRole:ChangeDir(this.enterDir)
		-- end
		--print("======type=======", curBattlePb.type)
		if curBattlePb.type == this.BATTLE_TYPE.BATTLE_TYPE_SAMPLE then
			--设置主界面信息
			MainCtrl.SetFuBenFightShow(true, true)

			time = curBattlePb.sampleinfo.btime
			lua_SampleBattle(curBattlePb.sampleinfo, curBattlePb.npc_proprate, curBattlePb.player_protprate)
		elseif curBattlePb.type == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_PG then
			--设置主界面信息
			MainCtrl.SetFuBenFightShow(true, true)

			time = curBattlePb.fpginfo.btime
			lua_FubenPgBattle(curBattlePb.fpginfo, curBattlePb.npc_proprate, curBattlePb.player_protprate)
			MainCtrl.SetTongGuanFubenBarInfo(curBattlePb.fpginfo.ftype, curBattlePb.fpginfo.gateno, curBattlePb.fpginfo.isfirst)

			curFType = curBattlePb.fpginfo.ftype
			--保存当前的副本类型
			FUBENLOGIC.SetCurFuBenInfo(curBattlePb.fpginfo.ftype, curBattlePb.fpginfo.gateno)

			--任务编号1000304（坐骑进阶）退出时点击任务链接
			local TaskLogic = require("Logic/TaskLogic")
			if TaskLogic.trunkTaskInfo and TaskLogic.trunkTaskInfo.taskNo == 1000304 then
				CtrlManager.GetCtrl(CtrlNames.FuBenJieSuanUi).cancelOpenUi = true
			end

		elseif curBattlePb.type == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_PT then
			--设置主界面信息
			MainCtrl.SetFuBenFightShow(true, true)

			time = curBattlePb.fptinfo.btime

			lua_FubenPtBattle(curBattlePb.fptinfo, curBattlePb.npc_proprate, curBattlePb.player_protprate)

			--设置当前层
			this.curExtend = curBattlePb.extend
			FUBENLOGIC.UpdatePataFuBenLayer(curBattlePb.fptinfo.ftype, curBattlePb.fptinfo.layer, -1)
			
			local GuideLogic = require("Logic/GuideLogic")
			GuideLogic.GuildNeedStop(curBattlePb.fptinfo.ftype)

			MainCtrl.SetTongGuanFubenBarInfo(curBattlePb.fptinfo.ftype, curBattlePb.fptinfo.gateno, 0)

			--保存当前的副本类型
			FUBENLOGIC.SetCurFuBenInfo(curBattlePb.fptinfo.ftype, curBattlePb.fptinfo.gateno)

			curFType = curBattlePb.fptinfo.ftype
			isNext = curBattlePb.fptinfo.isnext
			curGateNo = curBattlePb.fptinfo.gateno

			--处理助战
			--print("=========", curBattlePb.fptinfo.aidpskill and curBattlePb.fptinfo.aidpskill.martialid or "no aider")
			if curBattlePb.fptinfo.aidpskill.martialid and curBattlePb.fptinfo.aidpskill.martialid > 0 then
				--print("---------", curBattlePb.fptinfo.aidpskill.martialid)
				local data = {
					id = HERO.Id,
					martialId = curBattlePb.fptinfo.aidpskill.martialid,
					lv = 1,
				}


				lua_AddAidMartial(data)

				--创建助战小弟
				this.CreateAider(curBattlePb.fptinfo.gateno, curBattlePb.fptinfo.aidpskill.partnerno)

				this.SetHeroSkillWarning(curBattlePb.fptinfo.aidpskill.partnerno)
			end

			--武林试炼特殊处理
			if curBattlePb.fptinfo.ftype == GAMECONST.FuBenFType.TYPE_WULIN then
				this.SetSkillWarning(curBattlePb.fptinfo.gateno)
			end

			--经验副本引导特殊处理
			if curFType == GAMECONST.FuBenFType.TYPE_JINGYAN and curGateNo == 102001 then
				local GuideLogic = require("Logic/GuideLogic")
				GuideLogic.DealNuQiSkillGuide()
			end
		elseif curBattlePb.type == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_TF then
			--设置主界面信息
			FUBENLOGIC.LingYuanEnter()

			--塔防
			time = curBattlePb.ftfinfor.btime

			lua_FubenNextLayer_TaFang(curBattlePb.ftfinfor)
			
			--设置当前层
			this.curExtend = curBattlePb.extend
			FUBENLOGIC.UpdatePataFuBenLayer(curBattlePb.ftfinfor.ftype, curBattlePb.ftfinfor.layer, -1)
			-- print("================enterfb==============", curBattlePb.ftfinfor.waveno)
			FUBENLOGIC.LingYuanBarEnterRewardWave(curBattlePb.ftfinfor.waveno)

			-- MainCtrl.SetTongGuanFubenBarInfo(curBattlePb.ftfinfor.ftype, curBattlePb.ftfinfor.gateno, 0)

			--保存当前的副本类型
			FUBENLOGIC.SetCurFuBenInfo(curBattlePb.ftfinfor.ftype, curBattlePb.ftfinfor.gateno)

			curFType = curBattlePb.ftfinfor.ftype
			isNext = curBattlePb.ftfinfor.isnext
			curGateNo = curBattlePb.ftfinfor.gateno
		elseif curBattlePb.type == this.BATTLE_TYPE.BATTLE_TYPE_FUBEN_BS then
			local BossGuideLogic = require("Logic/BossGuideLogic")
			BossGuideLogic.bossNo = curBattlePb.fbsinfor.npcinfo[1].npcno
			time = curBattlePb.fbsinfor.btime
			lua_FubenBsBattle(curBattlePb.fbsinfor, curBattlePb.npc_proprate, curBattlePb.player_protprate)

			BossGuideLogic.ShowGuideBossBarUI()
			CtrlManager.GetCtrl(CtrlNames.FuBenJieSuanUi).cancelOpenUi = true
		end

		MainCtrl.SetFightLeftTime(time, TimeManager.GetRealTimeSinceStartUp() + 2)

		--创建计时器
		this.CreateTimer()

		curBattlePb = nil
	end
end

--创建助战
function ClientFightManager.CreateAider(gateno, partnerno)
	local FUBENLOGIC = FUBENLOGIC
	local wuLinXls = FUBENLOGIC.GetWuLinFbXls(gateno)
	if not wuLinXls then
		log("错误的武林试炼信息"..gateno)
		return
	end

	local aiderXls = FUBENLOGIC.GetAiderInfo(partnerno)
	if not aiderXls then
		log("错误的助战伙伴信息".. partnerno)
		return
	end

	roleMgr:AddAider(aiderXls.PartnerShape, wuLinXls.AiderBornPoint, wuLinXls.AiderDir or 2)
end

--设置技能警报（boss喊话）
function ClientFightManager.SetSkillWarning(gateno)
	local FUBENLOGIC = FUBENLOGIC
	local xlsInfo = FUBENLOGIC.GetFuBenXlsInfo(GAMECONST.FuBenFType.TYPE_WULIN, gateno)
	if not xlsInfo then
		log("错误的武林试炼信息" .. gateno)
		return
	end

	if xlsInfo.SkillWarning then
		MainCtrl.SetSkillWarning(xlsInfo.SkillWarning)
	end
end

--设置助战技能提示
function ClientFightManager.SetHeroSkillWarning(partnerno)
	local FUBENLOGIC = FUBENLOGIC
	local xlsInfo = FUBENLOGIC.GetAiderInfo(partnerno)
	if not xlsInfo then
		log("错误的武林试炼助战伙伴信息" .. partnerno)
		return
	end

	if xlsInfo.SkillTip then
		MainCtrl.SetHeroSkillWarning(xlsInfo.SkillTip)
	end
end

--爬塔战斗处理
function ClientFightManager.CreatePaTaFuBenFight(ptinfo)	
	lua_FubenNextLayer(ptinfo)

	--设置当前层
	--print("---------", ptinfo.ftype, ptinfo.layer)
	FUBENLOGIC.UpdatePataFuBenLayer(ptinfo.ftype, ptinfo.layer, -1)
	
	this.curExtend = ptinfo.extend
	MainCtrl.SetTongGuanFubenBarInfo(ptinfo.ftype, ptinfo.gateno, 0)

	--保存当前的副本类型
	FUBENLOGIC.SetCurFuBenInfo(ptinfo.ftype, ptinfo.gateno)

	curFType = ptinfo.ftype
	isNext = ptinfo.isnext
	curGateNo = ptinfo.gateno

	--经验副本引导特殊处理
	if curFType == GAMECONST.FuBenFType.TYPE_JINGYAN and curGateNo == 102005 then
		local GuideLogic = require("Logic/GuideLogic")
		GuideLogic.DealNuQiSkillGuide()
	end
end

--塔防战斗处理
function ClientFightManager.CreateTaFangFuBenFight(tfinfo)	
	lua_FubenNextLayer_TaFang(tfinfo)

	--设置当前层
	--print("---------", ptinfo.ftype, ptinfo.layer)
	FUBENLOGIC.UpdatePataFuBenLayer(tfinfo.ftype, tfinfo.layer, -1)
	this.curExtend = tfinfo.extend
	-- MainCtrl.SetTongGuanFubenBarInfo(tfinfo.ftype, tfinfo.gateno, 0)

	--保存当前的副本类型
	FUBENLOGIC.SetCurFuBenInfo(tfinfo.ftype, tfinfo.gateno)

	--重置水晶血量
	FUBENLOGIC.LingYuanBarHpReset()

	curFType = tfinfo.ftype
	isNext = tfinfo.isnext
	curGateNo = tfinfo.gateno
end

local function UnSerialize(data)
	return assert(loadstring("return "..data))()
end

local function GetPlayerSyncData()
	return {		--aoi.proto的同步协议
		rid = HERO.Id or 0,
		name = HERO.Name or "", 
		grade = HERO.Grade or 1,
		shape = HERO.Shape or 0,
		teamids = HERO.teamids or "",
		weapon = HERO.ActivateWeapon or 0,
--		setno = UserObj:GetSetNo(),
--		fashion = UserObj:GetFashionNo(),
		adname = "",
		sex = HERO.Sex or 1,

		-- this.MountModel = nil		--坐骑外形
		-- this.LingqiModel = nil		--养成之灵器外形
		-- this.LingqinModel = nil		--养成之灵琴外形
		-- this.LingyiModel = nil		--养成之灵翼外形
		-- this.ThugModel = nil		--养成之伙伴外形
		-- this.ThugHorseModel = nil	--养成之灵骑外形
		-- this.PetModel = nil			--养成之宠物外形
		-- this.ShenjianModel = nil	--养成之神剑外形
		-- this.ShenyiModel = nil		--养成之神翼外形
		-- this.JingmaiModel = nil		--养成之经脉外形
		-- this.ActivateWeapon = nil	--激活的武器外形

		mount_model = HERO.MountModel, 
		lingqi_model = HERO.LingqiModel,
		lingqin_model = HERO.LingqinModel,
		lingyi_model = HERO.LingyiModel,
		thug_model = HERO.ThugModel,
		partnerhorse_model = HERO.ThugHorseModel,
		pet_model = HERO.PetModel,
		shenjian_model = HERO.ShenjianModel,
		shenyi_model = HERO.ShenyiModel,
		jingmai_model = HERO.JingmaiModel,
	}
end

--创建主角属性字符串
function ClientFightManager.CreateHeroFightTable(x, y, z, InitBuff)
	local HEROSKILLMGR = HEROSKILLMGR
	local heroData = {
		x = x, y= y, z= z,
		AIRadius=HERO.AIRadius,
		InitBuff=UnSerialize(InitBuff or "") or {},
		Profession=HERO.Profession,
		MartialTable=UnSerialize(HERO.MartialTable or "") or {},
		Score=HERO.Score or 0,
		MapLayer = 1,
		FlyDodge = HERO.FlyDodge or 0,
		PartnerMartial=UnSerialize(HERO.PartnerMartial or "") or {},
		MagicMartial=UnSerialize(HERO.ChooseMagicMartial or "") or {},
		ThugModel = HERO.ThugModel,
		LingqiModel = HERO.LingqiModel,
		PetModel = HERO.PetModel,
		Sp = HEROSKILLMGR.GetCurSp(),
		ClubPost = HERO.ClubPost or "",
		ClubId = HERO.ClubId or 0,
		ClubName = HERO.ClubName or "",
		Vip = HERO.Vip or 0,
		Title = HERO.Title or 0,
		MateName = HERO.MateName or "",
	}

	--print(" ====MartialTable== ", heroData.Sp or "")

	for _, Attr in pairs(WARRIOR_DATA_NAME_CLIENT) do
		heroData[Attr] = HERO[Attr]
	end

	local ret = {
		userData = heroData,
		syncData = GetPlayerSyncData(),
	}

	--print("---------", TableToString(HERO.MartialTable))
	-- print("=========", HERO.Hp)
	return ret
end	

--获取主角属性md5
function ClientFightManager.GetHeroFightStringMD5()
	local AttrStr = ""
	for _, Attr in pairs(WARRIOR_DATA_NAME) do
		local AttrVal =  HERO[Attr]
		if AttrVal and AttrVal ~= 0 and Attr ~= "Hp" then
			AttrStr = AttrStr..Attr.."="..AttrVal
		end
	end

	-- print("-----------", AttrStr)
	return string.sub(MD5.ComputeString(AttrStr), 9, 24)
end


--创建计时器
function ClientFightManager.CreateTimer()
	UpdateBeat:Add(this.Update, this)
end

local tipCnt = 0

function ClientFightManager.Update()
	if needDelay and (Time.time - lastTimer) < clientDelay then 
		return 
	end

	if needDelay then
		lastTimer = Time.time 
		needDelay = false
	end
	if this.stopFight then return end
	if (Time.time - lastTimer >= TIME_INTERVAL) then
		tipCnt = tipCnt + 1
		lua_Timer()
		lastTimer = Time.time - (Time.time - lastTimer - TIME_INTERVAL)
	end
end

--暂停战斗
function ClientFightManager.StopFight()
	this.stopFight = true
end

function ClientFightManager.GetCurFuBenType()
	return curFType
end

--删除场景，战斗结束清理
function ClientFightManager.ClearClientFight()
	UpdateBeat:Remove(this.Update, this)
	Event.RemoveListener(EventType.Hero_Attrs, this.UpdateHeroAttrs)
	--lua_Clear()

	curFType = 0
	needDelay = true
	this.stopFight = false

	local GuideLogic = require("Logic/GuideLogic")
	GuideLogic.ClearGuideStop()
end

return ClientFightManager