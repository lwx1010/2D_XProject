local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local mabs=math.abs
local mceil=math.ceil
local mfloor = math.floor
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local tinsert = table.insert
local tconcat = table.concat
local ssplit = string.split
local BATTLE_CAMP_1 = BATTLE_CAMP_1
local BATTLE_CAMP_2 = BATTLE_CAMP_2
local lua_time_sec = lua_time_sec
local askill_cool_cnt = math.floor(SKILL_ALL_COOLTIME / lua_time_sec)
local IS_RET_MOVE = IS_RET_MOVE
local MAP_MOVE_BC_CNT = MAP_MOVE_BC_CNT
local laoi = laoi
local MOVETYPE_SPRINT	= MOVETYPE_SPRINT		--冲刺
local MOVETYPE_FLY1		= MOVETYPE_FLY1			--跳跃1
local MOVETYPE_FLY2		= MOVETYPE_FLY2			--跳跃2
local MOVETYPE_FLY3		= MOVETYPE_FLY3			--位移同步
local MOVETYPE_FLYF		= MOVETYPE_FLYF			--跳跃结束
local MOVETYPE_PJUMP	= MOVETYPE_PJUMP		--剧情跳
local MAX_FIYTIMENO	= MAX_FIYTIMENO
local EVIL_COOLTIME = EVIL_COOLTIME
local SKILL_TIME_ACCU = 1						--技能冷却时间精确度

local SKILL_MTYPE_NORMAL = SKILL_MTYPE_NORMAL			--普通
local SKILL_MTYPE_MAGIC = SKILL_MTYPE_MAGIC				--技能
local SKILL_MTYPE_HETIJI = SKILL_MTYPE_HETIJI			--合体技

local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能

local DIE_STATE_GHOST = DIE_STATE_GHOST
local MAP_DIE_TYPE = MAP_DIE_TYPE

local SINGLE_TARGET_TYPE = SINGLE_TARGET_TYPE

local pbc_send_msg = pbc_send_msg
local IsKuaFuServer = cfgData and cfgData.IsKuaFuServer
local IS_DELAY_RETMOVE = IS_DELAY_RETMOVE

local DEL_ACCU_TYPE = 1

clsUser = BASECHAR.clsBaseChar:Inherit({__ClassType = "USER"})

function clsUser:__init__(vfd, x, y, z, syncData, ociData, mapLayer)
	assert(IsVfdType(vfd))
	self.teamUserIds = {}
	self:SetVfd(vfd)
	if IsKuaFuServer then
		self:SetMainServerName(ociData.MainServerName)
	end
	self.__filterlist = IS_MAP_FILTER and {[assert(ociData["Id"], "not ID in ociData")]=1} or nil	--过滤玩家列表
	Super(clsUser).__init__(self, x, y, z, PLAYER_TYPE, syncData, ociData, mapLayer)
	self:SetTmp("CanGetBossScore", ociData.CanGetBossScore)
	if not ociData["Sp"] then
		self:SetSp(0)
	end
	if not self:IsDie() then
		--放到最后,因为buff需要添加到ui界面的
		local initBuff = self:GetTmp("InitBuff")
		if initBuff then
			for _buffId, _buffInfo in pairs(initBuff) do
				if _buffInfo.notFight then
					FIGHT_EVENT.AddNotFightBuff(self, _buffInfo)
				else
					FIGHT_EVENT.AddBuff(self, self, _buffInfo)
				end
			end 
		end
		
		if not self:GetTmp("NotInvisibleBuff") then
			--添加隐身buff
			FIGHT_EVENT.AddBuff(self, self, {
				id = 10000,
				time = 10,
			})
		end
	end
	
--	FIGHT_EVENT.ProcessPassMessage(PASS_BATTLE, self, nil, nil)				--触发被动技能
	
	local t1HpMax = self:GetHpMax() or 0
	local t2HpMax = self:GetFightValue(mFIGHT_HpMax) or 0
	if t1HpMax ~= t2HpMax then
		self:AddHp(t2HpMax - t1HpMax, 0)
	end
	
	lretmap.useradd(self:GetVfd(), MAP_ID, self:GetMapLayer(), self:GetX(), self:GetY(), self:GetZ())
end

function clsUser:GetMainServerName()
	return self.__MainServerName
end

function clsUser:SetMainServerName(mainServerName)
	self.__MainServerName = mainServerName
end

function clsUser:CanMove()
	local isOk = Super(clsUser).CanMove(self)
	if isOk then return isOk end
	
	if DIE_STATE_GHOST == MAP_DIE_TYPE then
		return true
	end
end

function clsUser:GetClubPost()
	return self:GetTmp("ClubPost")
end
function clsUser:SetClubPost(clubPost, notSync)
	local oClubPost = self:GetClubPost()
	self:SetTmp("ClubPost", clubPost)
	if oClubPost ~= clubPost then
		local syncData = self:GetSyncData()
		if syncData then
			syncData.clubpost = clubPost
		end
		if not notSync then
			self:SyncNearByPlayer("S2c_aoi_sync_string", {
				fid = self:GetFId(),
				key = "clubpost",
				value = clubPost or "",
			})
		end
	end
end

function clsUser:GetClubName()
	return self:GetTmp("ClubName")
end
function clsUser:SetClubName(clubName, notSync)
	local oClubName = self:GetClubName()
	self:SetTmp("ClubName", clubName)
	if oClubName ~= clubName then
		local syncData = self:GetSyncData()
		if syncData then
			syncData.clubname = clubName
		end
		if not notSync then
			self:SyncNearByPlayer("S2c_aoi_sync_string", {
				fid = self:GetFId(),
				key = "clubname",
				value = clubName or "",
			})
		end
	end
end

-------------------------------------begin 组队玩家显示-------------------------------------
function clsUser:GetShowTeamPlayer()
	return self:GetTmp("ShowTeamP")
end
function clsUser:SetShowTeamPlayer(show)
	self:SetTmp("ShowTeamP", show)
end
function clsUser:GetTeamPosById(id)
	for _idx, _id in ipairs(self.teamUserIds) do
		if id == _id then
			return _idx
		end
	end
end
-------------------------------------end 组队玩家显示-------------------------------------

-------------------------------------begin 3v3 -------------------------------------
function clsUser:GetK3v3State()
	return self:GetTmp("K3v3State")
end
function clsUser:SetK3v3State(state)
	self:SetTmp("K3v3State", state)
end

function clsUser:GetK3v3Skill()
	return self:GetTmp("K3v3Skill")
end

function clsUser:SetK3v3Skill(skillId)
	if skillId == 0 then 
		self:SetTmp("K3v3Skill", nil )
	else
		self:SetTmp("K3v3Skill", skillId*10+1)
		pbc_send_msg(self:GetVfd(), "S2c_aoi_skill_add", {
			id = self:GetId(),
			uiskill ={{
				skill_id = skillId*10+1,
				skill_type = 2,
				skill_lv = 1,}},
		})
		self:AddMartial(skillId, 1 )	
	end
end

-------------------------------------end 3v3-------------------------------------

function clsUser:GetEvilTime(evilTime)
	return self:GetTmp("EvilTime") or os.time()
end
function clsUser:SetEvilTime(evilTime)
	self:SetTmp("EvilTime", evilTime)
end
function clsUser:GetEvilValue()
	return self:GetTmp("EvilValue") or 0
end
function clsUser:SetEvilValue(evilValue)
	self:SetTmp("EvilValue", evilValue)
end

function clsUser:SetGrade(grade, notSync)
	Super(clsUser).SetGrade(self, grade)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.grade = grade or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "grade",
			value = grade or 0,
		})
	end
end

function clsUser:GetDaZuo()
	return self:GetTmp("DaZuo")
end
function clsUser:SetDaZuo(daZuo, notSync)
	local oDaZuo = self:GetDaZuo()
	if oDaZuo == daZuo then return end

	self:SetTmp("DaZuo", daZuo)
	if daZuo == DAZUO_STATE then
		if self:IsShapeshift() then			--如果是在变身中就不能设置打坐，在SetTmp后是因为CheckBreakDaZuo需要是打坐状态中
			self:CheckBreakDaZuo()
			return
		end
	end
	
	local syncData = self:GetSyncData()
	if syncData then
		syncData.dazuo = daZuo or 0
	end
	if daZuo == DAZUO_STATE then
		FIGHT_EVENT.AddDaZuo(self)
	else
		FIGHT_EVENT.DelDaZuo(self)
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "dazuo",
			value = daZuo or 0,
		})
	end
end
function clsUser:CheckBreakDaZuo()
	if self:GetDaZuo() == DAZUO_STATE then
		self:SetDaZuo(0)
		--同步给主逻辑,客户端的
		if IsServer() then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_DAZUO,
				state = 0,
				aexp = 0,
			}))	
		else
			lretmap.other({
				type = RETMAP_DAZUO,
				dazuo = {
					state = 0,
					aexp = 0,
				},
			})		
		end	
		
	end
end

--是否在跳跃中
function clsUser:CheckIsFly()
	local flydata = self:GetFlyData()
	if flydata and flydata.mtype then		--跳跃中
		return true
	end
end


--双修
function clsUser:GetDoubleXiulian()
	return self:GetTmp("DoubleXiulian")
end
function clsUser:SetDoubleXiulian(doubleXiulian, notSync)
	local oDoubleXiulian = self:GetDoubleXiulian()
	if oDoubleXiulian == doubleXiulian then return end
	self:SetTmp("DoubleXiulian", doubleXiulian)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.double_xiulian = doubleXiulian or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "double_xiulian",
			value = doubleXiulian or 0,
		})
	end
end
function clsUser:CheckBreakDoubleXiulian()
	if self:GetDoubleXiulian() == DOUBLE_XIULIAN_STATE then
		self:SetDoubleXiulian(0)		--中断双修
		self:CheckBreakDaZuo()
		--同步给主逻辑,客户端的
		if IsServer() then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_DOUBLE_XIULIAN,
			}))	
		else
			lretmap.other({
				type = RETMAP_DOUBLE_XIULIAN,
			})		
		end	
	end
end
--双修特效
function clsUser:GetDoubleXiulianEffect()
	return self:GetTmp("DoubleXiulianEffect")
end
function clsUser:SetDoubleXiulianEffect(doubleXiulianEffect, notSync)
	self:SetTmp("DoubleXiulianEffect", doubleXiulianEffect)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.dx_effect = doubleXiulianEffect or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "dx_effect",
			value = doubleXiulianEffect or 0,
		})
	end
end
--双修朝向
function clsUser:GetDoubleXiulianDir360()
	return self:GetTmp("DoubleXiulianDir360")
end
function clsUser:SetDoubleXiulianDir360(Dir360, notSync)
	self:SetTmp("DoubleXiulianDir360", Dir360)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.dx_dir360 = Dir360 or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "dx_dir360",
			value = Dir360 or 0,
		})
	end
end


--养成之灵琴外形
function clsUser:GetLingqinModel()
	return self:GetTmp("LingqinModel")
end
function clsUser:SetLingqinModel(lingqinModel, notSync)
	self:SetTmp("LingqinModel", lingqinModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.lingqin_model = lingqinModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "lingqin_model",
			value = lingqinModel or 0,
		})
	end
end
--养成之灵翼外形
function clsUser:GetLingyiModel()
	return self:GetTmp("LingyiModel")
end
function clsUser:SetLingyiModel(lingyiModel, notSync)
	self:SetTmp("LingyiModel", lingyiModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.lingyi_model = lingyiModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "lingyi_model",
			value = lingyiModel or 0,
		})
	end
end
--养成之灵骑外形
function clsUser:GetThugHorseModel()
	return self:GetTmp("ThugHorseModel")
end
function clsUser:SetThugHorseModel(thugHorseModel, notSync)
	self:SetTmp("ThugHorseModel", thugHorseModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.partnerhorse_model = thugHorseModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "partnerhorse_model",
			value = thugHorseModel or 0,
		})
	end
end
--养成之宠物外形
function clsUser:GetPetModel()
	return self:GetTmp("PetModel")
end
function clsUser:SetPetModel(petModel, notSync)
	self:SetTmp("PetModel", petModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.pet_model = petModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "pet_model",
			value = petModel or 0,
		})
	end
end
--养成之神剑外形
function clsUser:GetShenjianModel()
	return self:GetTmp("ShenjianModel")
end
function clsUser:SetShenjianModel(shenjianModel, notSync)
	self:SetTmp("ShenjianModel", shenjianModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.shenjian_model = shenjianModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "shenjian_model",
			value = shenjianModel or 0,
		})
	end
end
--养成之神翼外形
function clsUser:GetShenyiModel()
	return self:GetTmp("ShenyiModel")
end
function clsUser:SetShenyiModel(shenyiModel, notSync)
	self:SetTmp("ShenyiModel", shenyiModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.shenyi_model = shenyiModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "shenyi_model",
			value = shenyiModel or 0,
		})
	end
end
--养成之坐骑外形
function clsUser:GetMountModel()
	return self:GetTmp("MountModel")
end
function clsUser:SetMountModel(mountModel, notSync)
	self:SetTmp("MountModel", mountModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.mount_model = mountModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "mount_model",
			value = mountModel or 0,
		})
	end
end
--上下马状态
function clsUser:GetUpMountModel()
	return self:GetTmp("UpMountModel")
end
function clsUser:SetUpMountModel(upMountModel, notSync)
	self:SetTmp("UpMountModel", upMountModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.up_mount = upMountModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "up_mount",
			value = upMountModel or 0,
		})
	end
end
--伙伴上下马状态
--显示/隐藏灵宝外形
function clsUser:GetUpHorseModel()
	return self:GetTmp("UpHorseModel")
end
function clsUser:SetUpHorseModel(upHorseModel, notSync)
	self:SetTmp("UpHorseModel", upHorseModel)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.up_horse = upHorseModel or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "up_horse",
			value = upHorseModel or 0,
		})
	end
end
--武器外形编号
function clsUser:GetActivateWeapon()
	return self:GetTmp("ActivateWeapon")
end
function clsUser:SetActivateWeapon(activateWeapon, notSync)
	self:SetTmp("ActivateWeapon", activateWeapon)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.weapon = activateWeapon or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "weapon",
			value = activateWeapon or 0,
		})
	end
end
--时装外形编号
function clsUser:GetFashion()
	return self:GetTmp("Fashion")
end
function clsUser:SetFashion(fashion, notSync)
	self:SetTmp("Fashion", fashion)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.fashion = fashion or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "fashion",
			value = fashion or 0,
		})
	end
end

--是否运镖
function clsUser:GetIsYunBiao()
	return self:GetTmp("IsYunBiao")
end
function clsUser:SetIsYunBiao(IsYunBiao, notSync)
	self:SetTmp("IsYunBiao", IsYunBiao)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.isyunbiao = IsYunBiao or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "isyunbiao",
			value = IsYunBiao or 0,
		})
	end
end

--VIP
function clsUser:GetVip()
	return self:GetTmp("Vip")
end
function clsUser:SetVip(vip, notSync)
	self:SetTmp("Vip", vip)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.vip = vip or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "vip",
			value = vip or 0,
		})
	end
end

--显示/隐藏神翼外形
function clsUser:GetShenyiModelState()
	return self:GetTmp("ShenyiModelState")
end
function clsUser:SetShenyiModelState(ShenyiModelState, notSync)
	self:SetTmp("ShenyiModelState", ShenyiModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.shenyi_model_state = ShenyiModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "shenyi_model_state",
			value = ShenyiModelState or 0,
		})
	end
end
--显示/隐藏神剑外形
function clsUser:GetShenjianModelState()
	return self:GetTmp("ShenjianModelState")
end
function clsUser:SetShenjianModelState(ShenjianModelState, notSync)
	self:SetTmp("ShenjianModelState", ShenjianModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.shenjian_model_state = ShenjianModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "shenjian_model_state",
			value = ShenjianModelState or 0,
		})
	end
end
--显示/隐藏灵琴外形
function clsUser:GetLingqinModelState()
	return self:GetTmp("LingqinModelState")
end
function clsUser:SetLingqinModelState(LingqinModelState, notSync)
	self:SetTmp("LingqinModelState", LingqinModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.lingqin_model_state = LingqinModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "lingqin_model_state",
			value = LingqinModelState or 0,
		})
	end
end
--显示/隐藏灵翼外形
function clsUser:GetLingyiModelState()
	return self:GetTmp("LingyiModelState")
end
function clsUser:SetLingyiModelState(LingyiModelState, notSync)
	self:SetTmp("LingyiModelState", LingyiModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.lingyi_model_state = LingyiModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "lingyi_model_state",
			value = LingyiModelState or 0,
		})
	end
end
--显示/隐藏宠物外形
function clsUser:GetPetModelState()
	return self:GetTmp("PetModelState")
end
function clsUser:SetPetModelState(PetModelState, notSync)
	self:SetTmp("PetModelState", PetModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.pet_model_state = PetModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "pet_model_state",
			value = PetModelState or 0,
		})
	end
end
--显示/隐藏伙伴外形
function clsUser:GetThugModelState()
	return self:GetTmp("ThugModelState")
end
function clsUser:SetThugModelState(ThugModelState, notSync)
	self:SetTmp("ThugModelState", ThugModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.partner_model_state = ThugModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "partner_model_state",
			value = ThugModelState or 0,
		})
	end
end
--显示/隐藏灵器外形
function clsUser:GetLingqiModelState()
	return self:GetTmp("LingqiModelState")
end
function clsUser:SetLingqiModelState(LingqiModelState, notSync)
	self:SetTmp("LingqiModelState", LingqiModelState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.magic_model_state = LingqiModelState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "magic_model_state",
			value = LingqiModelState or 0,
		})
	end
end

--称号
function clsUser:GetTitle()
	return self:GetTmp("Title")
end
function clsUser:SetTitle(Title, notSync)
	self:SetTmp("Title", Title)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.title = Title or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "title",
			value = Title or 0,
		})
	end
end

--伴侣
function clsUser:GetMateName()
	return self:GetTmp("MateName")
end
function clsUser:SetMateName(MateName, notSync)
	self:SetTmp("MateName", MateName)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.matename = MateName or ""
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_string", {
			fid = self:GetFId(),
			key = "matename",
			value = MateName or "",
		})
	end
end
function clsUser:GetEnemyName()
	return self:GetTmp("EnemyName")
end
function clsUser:SetEnemyName(EnemyName, notSync)
	self:SetTmp("EnemyName", EnemyName)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.enemyname = EnemyName or ""
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_string", {
			fid = self:GetFId(),
			key = "enemyname",
			value = EnemyName or "",
		})
	end
end
function clsUser:GetEnemyList()
	return self:GetTmp("EnemyList")
end
function clsUser:SetEnemyList(EnemyList)
	self:SetTmp("EnemyList", EnemyList)	
end

--显示/隐藏婚礼时模型
function clsUser:GetWeddingShapeState()
	return self:GetTmp("WeddingShapeState")
end
function clsUser:SetWeddingShapeState(WeddingShapeState, notSync)
	self:SetTmp("WeddingShapeState", WeddingShapeState)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.weddingshapestate = WeddingShapeState or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "weddingshapestate",
			value = WeddingShapeState or 0,
		})
	end
end

--多杀称号
function clsUser:GetMultiKill()
	return self:GetTmp("MultiKill")
end
function clsUser:SetMultiKill(MultiKill, notSync)
	self:SetTmp("MultiKill", Multikill)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.multikill = MultiKill or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "multikill",
			value = MultiKill or 0,
		})
	end
end

--头像
function clsUser:GetPhoto()
	return self:GetTmp("Photo")
end
function clsUser:SetPhoto(Photo, notSync)
	self:SetTmp("Photo", Photo)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.photo = Photo or 0
	end
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "photo",
			value = Photo or 0,
		})
	end
end

-------------------------------------begin pk模式-----------------------------------------
local PKINFO_TBL = {
	[1] = "",
	[2] = "",
	[3] = "",
	[4] = "",
	[5] = "",
	[6] = "",
}

local function GetPkInfoStr(UserObj)
	PKINFO_TBL[1] = "pkmode=" .. UserObj:GetPkMode() .. ";"
	local teamId = UserObj:GetTeamId() or 0
	PKINFO_TBL[2] = teamId ~= 0 and ("team_id=" .. teamId .. ";") or ""
	local clubId = UserObj:GetClubId() or ""
	PKINFO_TBL[3] = clubId ~= "" and ("club_id=" .. clubId .. ";") or ""
	local evilState = UserObj:GetEvilState() or EVILSTATE_WHITE
	PKINFO_TBL[4] = "evil_state=" .. evilState .. ";"
	local serverId = UserObj:GetServerId() or ""
	PKINFO_TBL[5] = serverId ~= 0 and ("server_id=" .. serverId .. ";") or ""
	local hostileClub = UserObj:GetHostileClub() or ""
	PKINFO_TBL[6] = hostileClub ~= "" and ("hostileclub=" .. hostileClub .. ";") or ""
	return tconcat(PKINFO_TBL)
end

function clsUser:ResetPkInfo(notSync)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.pkinfo = GetPkInfoStr(self)
	end
	if not notSync then	
		self:SyncNearByPlayer("S2c_aoi_sync_string", {
			fid = self:GetFId(),
			key = "pkinfo",
			value = syncData.pkinfo or "",
		})
	end
end

function clsUser:SetHostileClub(hostileClub, notSync)
	self:SetTmp("HostileClub", hostileClub)
	laoi.obj_sethostileclub(self:GetEngineObj(), hostileClub or "")
	self:ResetPkInfo(notSync)
end
function clsUser:GetHostileClub()
	return self:GetTmp("HostileClub")
end
function clsUser:SetPkMode(pkMode, notSync)
	self:SetTmp("PkMode", pkMode)
	laoi.obj_setpkmode(self:GetEngineObj(), pkMode or PKMODE_PEACE)
	self:ResetPkInfo(notSync)
end
function clsUser:GetPkMode()
	return self:GetTmp("PkMode") or PKMODE_PEACE
end
function clsUser:SetTeamId(teamId, notSync)
	if teamId == 0 then
		teamId = nil
	end
	self:SetTmp("TeamId", teamId)
	laoi.obj_setteamid(self:GetEngineObj(), teamId or 0)
	self:ResetPkInfo(notSync)
end
function clsUser:GetTeamId()
	return self:GetTmp("TeamId")
end
function clsUser:SetClubId(clubId, notSync)
	self:SetTmp("ClubId", clubId)
	laoi.obj_setclubid(self:GetEngineObj(), clubId or "")
	self:ResetPkInfo(notSync)
end
function clsUser:GetClubId()
	return self:GetTmp("ClubId")
end
function clsUser:SetEvilState(eState, notSync)
	local oEvilState = self:GetEvilState()
	if oEvilState ~= eState then
		if eState == EVILSTATE_YELLOW then
			FIGHT_EVENT.AddEvilYellowData(self)
		else
			FIGHT_EVENT.DelEvilYellowData(self)
		end
		
		if eState == EVILSTATE_RED then
			--添加红buff
			if IsServer() then
				FIGHT_EVENT.AddBuff(self, self, {
					id = EVIL_BUFFID,
					time = 100000000,
				})
			else
				FIGHT_EVENT.AddBuff(self, self, {
					id = EVIL_BUFFID_CLIENT,
					time = 100000000,
				})				
			end
		elseif oEvilState == EVILSTATE_RED then
			if IsServer() then
				FIGHT_EVENT.DelBuff(self, EVIL_BUFFID)	
			else
				FIGHT_EVENT.DelBuff(self, EVIL_BUFFID_CLIENT)	
			end
		end
		
		self:SetTmp("EvilState", eState)
		laoi.obj_setevilstate(self:GetEngineObj(), eState or EVILSTATE_WHITE)
		self:ResetPkInfo(notSync)
	end
end
function clsUser:GetEvilState()
	return self:GetTmp("EvilState") or EVILSTATE_WHITE
end
function clsUser:SetServerId(serverId, notSync)
	self:SetTmp("ServerId", serverId)
	laoi.obj_setserverid(self:GetEngineObj(), serverId or 0)
	self:ResetPkInfo(notSync)
end
function clsUser:GetServerId()
	return self:GetTmp("ServerId")
end
-------------------------------------end pk模式-------------------------------------------
function clsUser:ClearFlyData()
	if not self.__flydata then
		self.__flydata = {
			stno = 0,
			cetno = 0,		--客户端结束时间
			etno = 0,
			sx = nil,
			sy = nil,
			sz = nil,
			mtype = nil,	
		}		
	else
		self.__flydata.stno = 0
		self.__flydata.cetno = 0
		self.__flydata.etno = 0
		self.__flydata.sx = nil
		self.__flydata.sy = nil
		self.__flydata.sz = nil
		self.__flydata.mtype = nil
	end
end
function clsUser:GetFlyData()
	if not self.__flydata then
		self:ClearFlyData()
	end
	return self.__flydata
end
function clsUser:SetFlyData(fdata)
	self.__flydata = fdata
end

function clsUser:SetSkillAct(skillIdList)
	self:SetTmp("SkillAct", skillIdList)
end
function clsUser:GetSkillAct()
	return self:GetTmp("SkillAct")
end

function clsUser:SetASkillCoolCnt(cnt)
	self.__acoolc = cnt
end
function clsUser:GetASkillCoolCnt()
	return self.__acoolc or GetNowTimeNo()
end

function clsUser:GetFlyLastTime()
	return self:GetTmp("FlyLastTime")
end
function clsUser:SetFlyLastTime(t)
	self:SetTmp("FlyLastTime", t)
end
function clsUser:GetFlyDodge()
	return self:GetTmp("FlyDodge") or 0
end
function clsUser:SetFlyDodge(flyDodge, notSend)
	local oDodge = self:GetFlyDodge()
	self:SetTmp("FlyDodge", flyDodge)
	local nTime = GetNowTimeNo()
	if not notSend then
		--同步给主逻辑,客户端的
		if IsServer() then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_FLYDODGE,
				flyDodge = flyDodge,
			}))		
		else
			lretmap.other({
				type = RETMAP_FLYDODGE,
				fdodge = {
					flydodge = flyDodge,
				},
			})		
		end	
	end	
	local oFlyTime = self:GetFlyLastTime()
	local cooltime = FLYDODGE_COOLTIME
	if oFlyTime then
		cooltime = FLYDODGE_COOLTIME - (nTime - oFlyTime)
		if cooltime <= 0 then
			cooltime = FLYDODGE_COOLTIME
		end
	else
		self:SetFlyLastTime(nTime)
	end
	pbc_send_msg(self:GetVfd(), "S2c_aoi_flydodge", {flydodge = flyDodge, cooltime = cooltime})
	local flyinfo = self:GetFlyDodgeInfo()
	if flyDodge >= MAX_FLYDODGE then
		flyinfo.isadd = false
		self:SetFlyLastTime(nil)
		FIGHT_EVENT.DelFlyDodgeCoolTime(self)
	else
		flyinfo.isadd = true
		flyinfo.etime = nTime + cooltime 
		FIGHT_EVENT.SetFlyDodgeCoolTime(self, flyinfo.etime)
	end
end
function clsUser:SubFlyDodge(sValue)
	local flyDodge = self:GetFlyDodge()
	if flyDodge <= 0 then return end
	self:SetFlyDodge(flyDodge - 1)
end
function clsUser:AddFlyDodge()
	local flyDodge = self:GetFlyDodge()
	if flyDodge >= MAX_FLYDODGE then return end
	self:SetFlyDodge(flyDodge + 1)
	self:SetFlyLastTime(GetNowTimeNo())
end

function clsUser:GetFlyDodgeInfo()
	local info = self:SetTmp("FlyDodgeInfo")
	if not info then
		info = {
			etime = GetNowTimeNo() + FLYDODGE_COOLTIME,
			isadd = false,
		}
		self:SetTmp("FlyDodgeInfo", info)
	end
	return info
end
function clsUser:SetFlyDodgeInfo(info)
	self:SetTmp("FlyDodgeInfo", info)
end

function clsUser:GetCantAttackList()
	return self:GetTmp("CantAttackList")
end

function clsUser:SetCantAttackList(cantAttackList)
	self:SetTmp("CantAttackList", cantAttackList)
end

function clsUser:GetSeeNpcNoList()
	return self:GetTmp("SeeNpcNoList") or {}
end

table.subkey = function (tbl1, tbl2)
	local ret = {}
	for _key, _ in pairs(tbl1) do
		if not tbl2[_key] then
			ret[_key] = true
		end
	end
	return ret
end

function clsUser:SetSeeNpcNoList(npcList)
	local oNpcList = self:GetSeeNpcNoList()
	self:SetTmp("SeeNpcNoList", npcList)
	local nearTbl = self:GetNearByNpc()
	if nearTbl then
		local addList = table.subkey(npcList, oNpcList)
		local delList = table.subkey(oNpcList, npcList)
		for _, pCharId in pairs(nearTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				local charNo = pCharObj:GetCharNo()
				if addList[charNo] and pCharObj:GetCanSee(self) then
					--添加npc
					pbc_send_msg(self:GetVfd(), "S2c_aoi_addnpc", {
						nmsg = {
							fid = pCharObj:GetFId(),
							map_no = MAP_NO,
							map_id = MAP_ID,
							rid = pCharObj:GetId(),
							x = pCharObj:GetX(),
							z = pCharObj:GetY(),
							y = pCharObj:GetZ(),
							hp = pCharObj:GetHp(),
							hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
							char_no = pCharObj:GetCharNo(),
						},
						sync = pCharObj:GetSyncData()
					})	
				elseif delList[charNo] and not pCharObj:GetCanSee(self) then
					--删除npc
					pbc_send_msg(self:GetVfd(), "S2c_aoi_leave", {
						map_no = MAP_NO,
						map_id = MAP_ID,
						id = pCharObj:GetId(),
					})	
				end
			end
		end
	end
end

function clsUser:SetNowSkillId(skillId)
	local allSkill = self:GetAllSkill()
	local skillData = allSkill[skillId]
	if not skillData then return end
	if skillData.Type == SKILL_TYPE_INITIATIVE then
		local mtype = skillData.Mtype
--		if skillData.FrontSkill then		--分段技能
--			local frontSkillId = self:GetFrontSkillId()
--			if frontSkillId ~= skillData.FrontSkill then
--				return
--			end
--		end
		local isSubSp = skillData.IsSubSp
		if isSubSp == COST_SP_TYPE then
			local sp = self:GetSp() or 0
			if sp < SP_MAX then return end
		end
		local timeNoCnt = skillData.CD
		local eTimeNo = skillData.CDEndTimeNo
		local nTimeNo = GetNowTimeNo()
		if nTimeNo >= eTimeNo - SKILL_TIME_ACCU then
			skillData.CDEndTimeNo = nTimeNo + timeNoCnt
			self.__nskillid = skillId
			return true, timeNoCnt * 1000 * lua_time_sec, mtype
		end
	end
end

function clsUser:GetComp()
	return Super(clsUser).GetComp(self) or BATTLE_CAMP_1
end

function clsUser:SetComp(comp, notSync)
	Super(clsUser).SetComp(self, comp, notSync)
	local partnerObj = self:GetOnePartner()
	if partnerObj then
		partnerObj:SetComp(comp, notSync)
	end
	local magicObj = self:GetOneMagic()
	if magicObj then
		magicObj:SetComp(comp, notSync)
	end
end

function clsUser:SetSpeed(speed, notSync)
	Super(clsUser).SetSpeed(self, speed)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.speed = self:GetFightValue(mFIGHT_Speed)
	end
--	self:SetSyncData(syncData, notSync)
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "speed",
			value = syncData.speed,
		})
	end
end

function clsUser:SetScore(score, notSync)
	Super(clsUser).SetScore(self, score)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.score = score
	end	
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_int", {
			fid = self:GetFId(),
			key = "score",
			value = score,
		})
	end
end

function clsUser:GetDir360()
	return self:GetTmp("Dir360") or 0
end
function clsUser:SetDir360(dir360)			--修改的时候不用广播同步dir360
	self:SetTmp("Dir360", dir360)
	local syncData = self:GetSyncData()
	if syncData then
		syncData.dir360 = dir360
	end
end

function clsUser:AddFixedShieldBuffSync(sHp, sHpMax, sBuffId)
	if IsKuaFuServer then		-- 当前只有跨服是这么做
		local syncData = Super(clsUser).GetSyncData(self)
		syncData.shield_hp = sHp
		syncData.shield_hpmax = sHpMax
		syncData.shield_buffid = sBuffId
		Super(clsUser).SetSyncData(self, syncData, true)
		
		if sHp and sHpMax and sBuffId then
			self:SyncNearByPlayer("S2c_aoi_playershield", {
				fid = self:GetFId(),
				shield_hp = sHp,
				shield_hpmax = sHpMax,
				shield_buffid = sBuffId,
			})
		end
	end
end

function clsUser:GetSyncData()
	local syncData = Super(clsUser).GetSyncData(self) or {}
	local sHp, sHpMax, sBuffId = self:FixedShieldBuffSyncHp()
	syncData.shield_hp = sHp
	syncData.shield_hpmax = sHpMax
	syncData.shield_buffid = sBuffId
	return syncData
end

function clsUser:SetSyncData(value, notSync)
	local oSyncData = self:GetSyncData()
	if value then		
		if value.name then			--玩家需要更名
			self:SetName(value.name)
		end
		if not value.speed then
			value.speed = self:GetFightValue(mFIGHT_Speed)
		end
		if not value.dir360 then
			value.dir360 = self:GetDir360()
		end
		if not value.pkinfo then
			value.pkinfo = GetPkInfoStr(self)
		end
		if not value.partner and oSyncData then			--伙伴的不变,变了在伙伴那处理
			value.partner = oSyncData.partner
		end
		if not value.magic and oSyncData then			--法器的不变,变了在伙伴那处理
			value.magic = oSyncData.magic
		end
		local sHp, sHpMax, sBuffId = self:FixedShieldBuffSyncHp()
		value.shield_hp = sHp
		value.shield_hpmax = sHpMax
		value.shield_buffid = sBuffId
	end
	Super(clsUser).SetSyncData(self, value, notSync)
end

local _FRE_SAVE_SP = SP_MAX / 8
function clsUser:SetSp(sp)
	local oSp = self:GetSp() or 0
	Super(clsUser).SetSp(self, sp)
	if oSp ~= sp or sp == 0 then
		pbc_send_msg(self:GetVfd(), "S2c_aoi_sp", {sp=sp})		--同步sp
		
		local oC = mfloor(oSp / _FRE_SAVE_SP)
		if mfloor(sp / _FRE_SAVE_SP) ~= oC then
			if IsServer() then
				lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_SP,
					sp = sp,
				}))	
			end
		end
	end
end

function clsUser:SetVfd(vfd)
	self.__vfd = vfd
end

function clsUser:GetVfd()
	return self.__vfd
end

function clsUser:SetHpMax(hpMax, notSync)
	Super(clsUser).SetHpMax(self, hpMax)
	if not notSync then
		local playerTbl = self:GetNearByPlayers()
		local vfds = {}
		if playerTbl then
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
		end
		tinsert(vfds, self:GetVfd())
		pbc_send_msg(vfds, "S2c_aoi_ui_hp", {
			fid = self:GetFId(),
			hp = self:GetHp(),
			hp_max = self:GetFightValue(mFIGHT_HpMax),
		})
	end
end

function clsUser:SetHp(hp, attId, notSync, stype, isNotRetHp)
	local ohp = self:GetHp()
	Super(clsUser).SetHp(self, hp, attId, notSync, stype, isNotRetHp)
	hp = self:GetHp()
	if not isNotRetHp and (hp <= 0 or ohp ~= hp) then
		--同步给主逻辑,客户端的
		if IsServer() then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_RETHP,
				hp = hp,
			}))		
		else
--			lretmap.other({
--				type = RETMAP_RETHP,
--				php = {
--					hp = hp,
--					chp = -hp,
--				},
--			})		
		end	
	end
	if hp <= 0 then
		FIGHT_EVENT.DelDaZuo(self)
		local flydata = self:GetFlyData()
		if flydata.mtype then		--跳跃中
			self:SendFlyError()
		end
		
		if IsClient() then
			local attObj = CHAR_MGR.GetCharById(attId)
			local attName = ""
			local score = 0
			if attObj then
				attName = attObj:GetName()
				score = attObj:GetScore() or 0
			end
			
			if CHECKEND.CanReliveFuben() then			--需要复活
				lretmap.other({
					type = RETMAP_RELIVE,
					relive = {
						attname = attName,
						score = score,
					}
				})	
			end
		end
	end
	local sId = self:GetId()
	if attId and attId ~= 0 and sId ~= attId then
		if ohp - hp > 0 then
			local attObj = CHAR_MGR.GetCharById(attId)
			if attObj and (attObj:IsMagic() or attObj:IsPartner()) then
				attObj = attObj:GetOwner()
				attId = attObj:GetId()
			end
			if IsServer() then
				if attObj and attObj:IsPlayer() then
					if EVILCAL then
						local nTimeNo = GetNowTimeNo() 
						local eState = self:GetEvilState()
						if eState == EVILSTATE_WHITE then
							--attObj 攻击了白名单,并且如果他不是红名则设置一下为黄名单,并且设置黄名单时间
							if attObj:GetEvilState() ~= EVILSTATE_RED then
								attObj:SetEvilState(EVILSTATE_YELLOW)
								attObj:SetEvilYellowTime(nTimeNo)
							end
						end
						if hp <= 0 and eState == EVILSTATE_WHITE then
							lretmap.other(attId, MAP_ID, attObj:GetMapLayer(), lserialize.lua_seri_str({
								type = RETMAP_PKMODE_KILL,
								killId = sId,
							}))			
						end	
					else 
						local nTimeNo = GetNowTimeNo() 
						local eState = self:GetEvilState()
						if eState == EVILSTATE_WHITE then
							--attObj 攻击了白名单,并且如果他不是红名则设置一下为黄名单,并且设置黄名单时间
							if attObj:GetEvilState() ~= EVILSTATE_RED then
								if FLAG and SHOW_YELLOW then
									local hostileClub = self:GetHostileClub()
									local tarHostileClub = attObj:GetHostileClub()
									if (hostileClub and hostileClub ~= "" and hostileClub == attObj:GetClubId()) or 
										(tarHostileClub and tarHostileClub ~= "" and tarHostileClub == self:GetClubId()) then
											attObj:SetEvilState(EVILSTATE_YELLOW)
											attObj:SetEvilYellowTime(nTimeNo)
									end
								end
							end
						end
						if hp <= 0 and eState == EVILSTATE_WHITE then
							lretmap.other(attId, MAP_ID, attObj:GetMapLayer(), lserialize.lua_seri_str({
								type = RETMAP_CLUBDIDUI,
								killId = sId,
							}))			
						end	
					end
				end
			end
		end
	end
	
	pbc_send_msg(self:GetVfd(), "S2c_aoi_ui_hp", {fid=self:GetFId(),hp=self:GetHp(),hp_max=self:GetFightValue(mFIGHT_HpMax)})
end

function clsUser:GetEvilYellowTime()
	return self.__tmp.EvilYellowTime
end

function clsUser:SetEvilYellowTime(t)
	self.__tmp.EvilYellowTime = t
end

function clsUser:GetPartnerAp()
	local pObjs = self:GetAllPartner()
	for _, _pObj in pairs(pObjs) do
		return _pObj:GetPartnerAp()
	end	
end
function clsUser:SetPartnerAp(partnerAp)
	local pObjs = self:GetAllPartner()
	for _, _pObj in pairs(pObjs) do
		_pObj:SetPartnerAp(partnerAp)
	end
end

function clsUser:GetPartnerHurt()
	local pObjs = self:GetAllPartner()
	for _, _pObj in pairs(pObjs) do
		return _pObj:GetPartnerHurt()
	end	
end
function clsUser:SetPartnerHurt(partnerHurt)
	local pObjs = self:GetAllPartner()
	for _, _pObj in pairs(pObjs) do
		_pObj:SetPartnerHurt(partnerHurt)
	end
end
function clsUser:SetPartnerExtraHurt(partnerExtraHurt)
	local pObjs = self:GetAllPartner()
	for _, _pObj in pairs(pObjs) do
		_pObj:SetPartnerExtraHurt(partnerExtraHurt)
	end
end

function clsUser:GetMagicAp()
	local mObjs = self:GetAllMagic()
	for _, _mObj in pairs(mObjs) do
		_mObj:GetMagicAp()
	end
end
function clsUser:SetMagicAp(magicAp)
	local mObjs = self:GetAllMagic()
	for _, _mObj in pairs(mObjs) do
		_mObj:SetMagicAp(magicAp)
	end
end

function clsUser:GetMagicHurt()
	local mObjs = self:GetAllMagic()
	for _, _mObj in pairs(mObjs) do
		_mObj:GetMagicHurt()
	end
end
function clsUser:SetMagicHurt(magicHurt)
	local mObjs = self:GetAllMagic()
	for _, _mObj in pairs(mObjs) do
		_mObj:SetMagicHurt(magicHurt)
	end
end

function clsUser:SetPartnerReHurt(partnerReHurt)
	self.__tmp.PartnerReHurt = partnerReHurt
end
function clsUser:GetPartnerReHurt()
	return self.__tmp.PartnerReHurt or 0
end

function clsUser:SetThugModel(ThugModel, isNotSync)
	self.__tmp.ThugModel = ThugModel
	local partnerObj = self:GetOnePartner()
	if partnerObj then
		partnerObj:SetShape(ThugModel, isNotSync)
	else
		if not self.IsInInit then		--激活
			local martialTable = nil
			if self.GetPartnerMartial then
				martialTable = self:GetPartnerMartial()
			else
				martialTable = self:GetTmp("PartnerMartial")
			end
			PARTNER.clsPartner:New({MartialTable = martialTable}, self:GetId(), self:GetMapLayer())
		end
	end
end

function clsUser:SetLingqiModel(LingqiModel, isNotSync)
	self.__tmp.LingqiModel = LingqiModel
	local magicObj = self:GetOneMagic()
	if magicObj then
		magicObj:SetShape(LingqiModel, isNotSync)
	else
		if not self.IsInInit then		--激活
			local martialTable = nil
			if self.GetMagicMartial then
				martialTable = self:GetMagicMartial()
			else
				martialTable = self:GetTmp("MagicMartial")
			end
			MAGIC.clsMagic:New({MartialTable = martialTable}, self:GetId(), self:GetMapLayer())
		end
	end
end

function clsUser:GetAllPartner()
	if not self.__pTbl then
		self.__pTbl = {}
	end
	return self.__pTbl
end
function clsUser:AddPartner(PartnerObj)
	local pTbl = self:GetAllPartner()
	tinsert(pTbl, PartnerObj)
	
	PartnerObj:SetShape(self.__tmp.ThugModel)
end
function clsUser:GetAllMagic()
	if not self.__mTbl then
		self.__mTbl = {}
	end
	return self.__mTbl
end
function clsUser:AddMagic(MagicObj)
	local mTbl = self:GetAllMagic()
	tinsert(mTbl, MagicObj)
	
	MagicObj:SetShape(self.__tmp.LingqiModel)
end
function clsUser:GetOnePartner()
	return self:GetAllPartner()[1]
end
function clsUser:GetOneMagic()
	return self:GetAllMagic()[1]
end

function clsUser:PartnerDestroy()
	local allPartner = self:GetAllPartner()
	for _, _PartnerObj in pairs(allPartner) do
		_PartnerObj:Destroy()
	end
	self.__pTbl = {}
end
function clsUser:MagicDestroy()
	local allMagic = self:GetAllMagic()
	for _, _MagicObj in pairs(allMagic) do
		_MagicObj:Destroy()
	end
	self.__mTbl = {}
end

function clsUser:GetDiePartnerCnt()
	return 0
end

function clsUser:IsPlayer()
	return true
end

function clsUser:AddMap()			--添加进入地图后的处理,必须重载,不然报错(进入地图的时候可以npc,user,partner重合站一起,不然处理比较麻烦)
	local isOk, playerTbl, partnerTbl, npcTbl, itemTbl = laoi.map_addobj(self:GetMapObj(), self:GetEngineObj())
	assert(isOk, "user add map error: " .. self:GetName() .. " x,y" .. self:GetX()..","..self:GetY())
	local selfvfd = self:GetVfd()
	local selfId = self:GetId()
	if isOk then
		self:MoveChangeMapPos(CHANGE_MAPPOS_ADD)
		pbc_send_msg(selfvfd, "S2c_aoi_addself", {
			nmsg = {
				fid = self:GetFId(),
				map_no = MAP_NO,
				map_id = MAP_ID,
				rid = self:GetId(),
				x = self:GetX(),
				z = self:GetY(),
				y = self:GetZ(),
				hp = self:GetHp(),
				hpmax = self:GetFightValue(mFIGHT_HpMax),
				char_no = self:GetCharNo(),
			},
			sync = self:GetSyncData(),
		})
		--_RUNTIME(self:GetName(),sys.dump(self:GetSyncData()))
	end
	if playerTbl then
		if MAP_NO == 1089 then
			_RUNTIME_ERROR("___wulin 1:", os.time(), self:GetName(), self:GetId())
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					_RUNTIME_ERROR("___wulin 2:", os.time(), self:GetName(), pCharObj:GetName(), pCharId)
				end
			end
		end
		local vfds = {}
		for _, pCharId in pairs(playerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				tinsert(vfds, pCharObj:GetVfd())
				pbc_send_msg(selfvfd, "S2c_aoi_addplayer", {
					nmsg = {
						fid = pCharObj:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = pCharObj:GetId(),
						x = pCharObj:GetX(),
						z = pCharObj:GetY(),
						y = pCharObj:GetZ(),
						hp = pCharObj:GetHp(),
						hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
						char_no = pCharObj:GetCharNo(),
					},
					sync = pCharObj:GetSyncData(),
					
				})
			end
		end
		if #vfds > 0 then
			pbc_send_msg(vfds, "S2c_aoi_addplayer", {
				nmsg = {
					fid = self:GetFId(),
					map_no = MAP_NO,
					map_id = MAP_ID,
					rid = self:GetId(),
					x = self:GetX(),
					z = self:GetY(),
					y = self:GetZ(),
					hp = self:GetHp(),
					hpmax = self:GetFightValue(mFIGHT_HpMax),
					char_no = self:GetCharNo(),
				},
				sync = self:GetSyncData(),
				
			})	
		end					
	end
	if partnerTbl then
		for _, pCharId in pairs(partnerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				pbc_send_msg(selfvfd, "S2c_aoi_addpartner", {
					nmsg = {
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = pCharObj:GetId(),
						x = pCharObj:GetX(),
						z = pCharObj:GetY(),
						y = pCharObj:GetZ(),
						hp = pCharObj:GetHp(),
						hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
						char_no = pCharObj:GetCharNo(),
					},
					sync = pCharObj:GetSyncData(),
					
				})							
			end
		end		
	end
	if npcTbl then
		for _, pCharId in pairs(npcTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj and pCharObj:GetCanSee(self) then
				pbc_send_msg(selfvfd, "S2c_aoi_addnpc", {
					nmsg = {
						fid = pCharObj:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = pCharObj:GetId(),
						x = pCharObj:GetX(),
						z = pCharObj:GetY(),
						y = pCharObj:GetZ(),
						hp = pCharObj:GetHp(),
						hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
						char_no = pCharObj:GetCharNo(),
					},
					sync = pCharObj:GetSyncData(),
					
				})
				pCharObj:CheckNpcToAttack(self)
			end
		end		
	end
	if itemTbl then
--		for _, pCharId in pairs(itemTbl) do
--			local pCharObj = CHAR_MGR.GetCharById(pCharId)
--			if pCharObj then
--				pbc_send_msg(selfvfd, "S2c_aoi_add", {										--测试，暂时没有物品
--					rid = pCharObj:GetId(),
--					type = ITEM_TYPE,
--					x = pCharObj:GetX(),
--					y = pCharObj:GetY(),
--				})				
--			end
--		end		
	end
end

function clsUser:SendMove(ox, oy, oz, retnum, isJump, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, speed, mtype, syncmsg)
	self:CheckBreakDaZuo()
	self:CheckBreakDoubleXiulian()
	local selfvfd = self:GetVfd()
	local selfId = self:GetId()
	local nx, ny, nz = self:GetX(), self:GetY(), self:GetZ()
	if retnum == 0 then
		local vfds = {}
		if isJump then
			tinsert(vfds, selfvfd)	--添加告诉自己跳转了
		end
		if apTbl then
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())		
				end
			end
		end
		if #vfds > 0 then
			if isJump then
				pbc_send_msg(vfds, "S2c_aoi_jump", {	
					map_no = MAP_NO,
					map_id = MAP_ID,										
					rid = selfId,
					x = nx,
					z = ny,
					y = nz,
					
				})
			elseif mtype == MOVETYPE_FLYF then
				pbc_send_msg(vfds, "S2c_aoi_player_fly_finished", syncmsg)
			elseif mtype == MOVETYPE_FLY1 or mtype == MOVETYPE_FLY2 or mtype == MOVETYPE_FLY3 then
				pbc_send_msg(vfds, "S2c_aoi_player_fly", syncmsg)
			elseif mtype == MOVETYPE_PJUMP then
				pbc_send_msg(vfds, "S2c_aoi_plot_jump", syncmsg)
			elseif mtype == MOVETYPE_SPRINT then
				pbc_send_msg(vfds, "S2c_aoi_sprint", syncmsg)
			else
				pbc_send_msg(vfds, "S2c_aoi_move", {							
					fid = self:GetFId(),
					x = nx,
					z = ny,
					y = nz,
					speed = speed,
					type = mtype,
				})
			end
		end
	elseif retnum == 1 then
		if apTbl then
			--移动的用户的坐标告诉别人进入他们的视野使用旧的pos的(即先把player的旧坐标发给addplayer,然后最后move一次发送新坐标【里面也包含了addplayer】)
			local vfds = {}
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
					pbc_send_msg(selfvfd, "S2c_aoi_addplayer", {
						nmsg = {
							fid = pCharObj:GetFId(),
							map_no = MAP_NO,
							map_id = MAP_ID,
							rid = pCharObj:GetId(),
							x = pCharObj:GetX(),
							z = pCharObj:GetY(),
							y = pCharObj:GetZ(),
							hp = pCharObj:GetHp(),
							hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
							char_no = pCharObj:GetCharNo(),
						},
						sync = pCharObj:GetSyncData(),
						
					})		
				end
			end
			--把自己移动前的坐标告诉别人
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_addplayer", {
					nmsg = {
						fid = self:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = selfId,
						x = nx,
						z = ny,
						y = nz,
						hp = self:GetHp(),
						hpmax = self:GetFightValue(mFIGHT_HpMax),
						char_no = self:GetCharNo(),
					},
					sync = self:GetSyncData(),
					
				})		
			end			
		end
		if apaTbl then
			for _, pCharId in pairs(apaTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then	
					pbc_send_msg(selfvfd, "S2c_aoi_addpartner", {
						nmsg = {
							map_no = MAP_NO,
							map_id = MAP_ID,
							rid = pCharObj:GetId(),
							x = pCharObj:GetX(),
							z = pCharObj:GetY(),
							y = pCharObj:GetZ(),
							hp = pCharObj:GetHp(),
							hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
							char_no = pCharObj:GetCharNo(),
						},
						sync = pCharObj:GetSyncData(),
						
					})			
				end
			end		
		end
		if anTbl then
			for _, pCharId in pairs(anTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and pCharObj:GetCanSee(self) then
					pbc_send_msg(selfvfd, "S2c_aoi_addnpc", {
						nmsg = {
							fid = pCharObj:GetFId(),
							map_no = MAP_NO,
							map_id = MAP_ID,
							rid = pCharObj:GetId(),
							x = pCharObj:GetX(),
							z = pCharObj:GetY(),
							y = pCharObj:GetZ(),
							hp = pCharObj:GetHp(),
							hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
							char_no = pCharObj:GetCharNo(),
						},
						sync = pCharObj:GetSyncData(),
						
					})		
				end
			end	
		end
--		if aiTbl then
--			for _, pCharId in pairs(aiTbl) do
--				local pCharObj = CHAR_MGR.GetCharById(pCharId)
--				if pCharObj then
--					pbc_send_msg(selfvfd, "S2c_aoi_add", {										--测试，暂时没有物品
--						rid = pCharObj:GetId(),
--						type = ITEM_TYPE,
--						x = pCharObj:GetX(),
--						y = pCharObj:GetY(),
--					})				
--				end
--			end		
--		end		

		if isJump then
			pbc_send_msg(selfvfd, "S2c_aoi_jump", {	
				map_no = MAP_NO,
				map_id = MAP_ID,										
				rid = selfId,
				x = nx,
				z = ny,
				y = nz,
				
			})
		else
--			pbc_send_msg(selfvfd, "S2c_aoi_move", {
--				fid = self:GetFId(),
--				x = nx,
--				z = ny,
--				y = nz,
--				speed = speed,
--				type = mtype,
--			})	
		end		
		
		if dpTbl then
			local vfds = {}
			for _, pCharId in pairs(dpTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
					pbc_send_msg(selfvfd, "S2c_aoi_leave", {
						map_no = MAP_NO,
						map_id = MAP_ID,
						id = pCharObj:GetId(),
						
					})				
				end
			end
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_leave", {
					map_no = MAP_NO,
					map_id = MAP_ID,
					id = self:GetId(),
					
				})		
			end				
		end		
		if dpaTbl then
			for _, pCharId in pairs(dpaTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					pbc_send_msg(selfvfd, "S2c_aoi_leave", {
						map_no = MAP_NO,
						map_id = MAP_ID,
						id = pCharObj:GetId(),
						
					})				
				end
			end		
		end
		if dnTbl then
			for _, pCharId in pairs(dnTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and pCharObj:GetCanSee(self) then
					pbc_send_msg(selfvfd, "S2c_aoi_leave", {
						map_no = MAP_NO,
						map_id = MAP_ID,
						id = pCharObj:GetId(),
						
					})				
				end
			end		
		end
--		if diTbl then
--			for _, pCharId in pairs(diTbl) do
--				local pCharObj = CHAR_MGR.GetCharById(pCharId)
--				if pCharObj then
--					pbc_send_msg(selfvfd, "S2c_aoi_leave", {
--						map_no = MAP_NO,
--						map_id = MAP_ID,
--						id = pCharObj:GetId(),
--					})				
--				end
--			end		
--		end	
		
		
		if mpTbl then
			local vfds = {}
			local mCnt = 0
			for _, pCharId in pairs(mpTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
					mCnt = mCnt + 1
					if MAP_MOVE_BC_CNT and MAP_MOVE_BC_CNT <= mCnt then
						break
					end
				end
			end
			if #vfds > 0 then
				if isJump then
					pbc_send_msg(vfds, "S2c_aoi_jump", {	
						map_no = MAP_NO,
						map_id = MAP_ID,										
						rid = selfId,
						x = nx,
						z = ny,
						y = nz,
						
					})
				elseif mtype == MOVETYPE_FLYF then
					pbc_send_msg(vfds, "S2c_aoi_player_fly_finished", syncmsg)
				elseif mtype == MOVETYPE_FLY1 or mtype == MOVETYPE_FLY2 or mtype == MOVETYPE_FLY3 then
					pbc_send_msg(vfds, "S2c_aoi_player_fly", syncmsg)
				elseif mtype == MOVETYPE_PJUMP then
					pbc_send_msg(vfds, "S2c_aoi_plot_jump", syncmsg)
				elseif mtype == MOVETYPE_SPRINT then
					pbc_send_msg(vfds, "S2c_aoi_sprint", syncmsg)
				else
					pbc_send_msg(vfds, "S2c_aoi_move", {
						fid = self:GetFId(),
						x = nx, 
						z = ny,
						y = nz,
						speed = speed,
						type = mtype,
					})
				end
			end				
		end
	end
	
	local nearTbl = self:GetNearByNpc()
	if nearTbl then
		for _, pCharId in pairs(nearTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj and pCharObj:GetCanSee(self) then
				pCharObj:CheckNpcToAttack(self)	
			end
		end
	end
end

function clsUser:Move(speed, dir)	--添加移动后的处理,必须重载,不然报错(需要判断从开始到结束的点是否有npc,user,partner或者墙)

end

function clsUser:MoveErrorSend()
	pbc_send_msg(self:GetVfd(), "S2c_aoi_move_error", {
		ox = self:GetX(), 
		oz = self:GetY(),
		oy = self:GetZ(),
	})	
end

--只能是测试一格
function clsUser:CanMoveToOneGrid(ox, oy, nx, ny)
	if not self:CanMove() then return end	
	local disX = mabs(ox - nx)
	local disY = mabs(oy - ny)
	if disX == 0 and disY == 0 then return end		--只能一格
	if disX >= 2 or disY >= 2 then return end		--只能一格
	
	if self:IsBlockPoint(nx, ny) then return end	
	if not BASECHAR.CanMoveBySlope(ox, oy, nx, ny) then return end			--有坡度
	return true
end

--返回true/nil
function clsUser:MoveTo(nx, ny, nz, speed, notSlope, mtype)	--添加移动后的处理,必须重载,不然报错(只需要判断nx,ny点是否有npc,user,partner或者墙)
	if not self:CanMove() then 
		--返回移动错误给客户端	
		self:MoveErrorSend()
		return 
	end
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then return end
	end
	local ox, oy, oz = self:GetX(), self:GetY(), self:GetZ()
	if not IsClient() then
		if self:IsBlockPoint(nx, ny, nz) then
			--移动是客户端发过来的,不在引擎中
			--返回移动错误给客户端
			self:MoveErrorSend()
			return
		end
	end
	
	if nx ~= ox or ny ~= oy then
		local speed = self:GetFightValue(mFIGHT_Speed)
		local nTimeNo = GetNowTimeNo()
		local lTimeNo = self:GetTmp("LastMoveTime") or 0
		
		local disXTime = mabs(ox - nx) / speed * 10
		local disYTime = mabs(oy - ny) / speed * 10
		
		if nTimeNo == lTimeNo then
			if mabs(ox - nx) >= 6 or mabs(oy - ny) >= 6 then
				self:MoveErrorSend()
				return
			end
		else
			local dT = (nTimeNo - lTimeNo) * 6
			if dT < disXTime or dT < disYTime then
				self:MoveErrorSend()
				return
			end
		end
		
		self:SetTmp("LastMoveTime", nTimeNo)
	end
	
	local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(self:GetMapObj(), self:GetEngineObj(), nx, ny)
	if not retnum then
		self:MoveErrorSend()
		error("clsUser:MoveTo error user:" .. self:GetName())
	elseif retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		self:SetX(nx)
		self:SetY(ny)
		self:SetZ(nz)
		self:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		if mtype ~= MOVETYPE_SPRINT then
			if self:IsMirrorPlayer() then
				speed = self:GetFightValue(mFIGHT_Speed)
				local tLen = mabs(ox - nx) ^ 2 + mabs(oy - ny) ^ 2
				local func = DISTANCE_ADJUST_SPEED[tLen]
				if func then
					speed = func(speed)
				end
			end
			self:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, speed, mtype)
		else
			if self:IsMirrorPlayer() then
				speed = 0
			end
			self:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, speed)
		end
		if not IS_DELAY_RETMOVE then
			if IS_RET_MOVE then
				lretmap.usermove(self:GetVfd(), MAP_ID, self:GetMapLayer(), nx, ny, nz)
			end
		end
		
		if self:IsMirrorPlayer() then			--镜像
			local dir = BASECHAR.GetDirByPos(ox, oy, nx, ny)
			if dir then self:SetDir(dir) end
		end
		
		return true
	else
		if ox ~= nx or oy ~= ny then			--不同位置才设置回退
			self:MoveErrorSend()
		end
	end
	
--	_RUNTIME("clsUser:MoveTo 2:", retnum, 
--		sys.dump(apTbl), sys.dump(anTbl), sys.dump(aiTbl),
--		sys.dump(dpTbl), sys.dump(dnTbl), sys.dump(diTbl),
--		sys.dump(mpTbl), "oldxy:["..ox..","..oy.."]", "newxy["..nx..","..ny.."]"
--		)	
end

--返回true/nil
function clsUser:JumpTo(nx, ny, nz, cbidx, notCheckMove)		--添加跳转函数(只需要判断nx,ny点是否有npc,user,partner或者墙)
	if self:CheckIsFly() then
		return
	end

	if not notCheckMove then 
		if not self:CanMove() then 
			error("JumpTo CanMove error")
		end
	end
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then return end
	end
	local ox, oy, oz = self:GetX(), self:GetY(), self:GetZ()
	
	if self:IsBlockPoint(nx, ny, nz) then
		error("the block[" .. nx .. "," .. ny .. "," .. nz .. "]")
	end
	
	local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(self:GetMapObj(), self:GetEngineObj(), nx, ny)
	assert(retnum, "clsUser:JumpTo error user:" .. self:GetName())
	if retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		self:SetTmp("LastMoveTime", GetNowTimeNo())
		self:SetX(nx)
		self:SetY(ny)
		self:SetZ(nz)
		self:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		self:SendMove(ox, oy, oz, retnum, true, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl)
		if cbidx then
			lretmap.userjump(self:GetVfd(), MAP_ID, self:GetMapLayer(), nx, ny, nz, lserialize.lua_seri_str({
				cbidx=cbidx,
			}))
		end
		return true
	else
		if ox ~= nx or oy ~= ny then			--不同位置
			error(string.format("user:%s JumpTo retnum:%s error, ox:%s, oy:%s, nx:%s, ny:%s", self:GetName(), retnum, ox, oy, nx, ny))
		end
	end
	
--	_RUNTIME("clsUser:JumpTo 2:", retnum, 
--		sys.dump(apTbl), sys.dump(anTbl), sys.dump(aiTbl),
--		sys.dump(dpTbl), sys.dump(dnTbl), sys.dump(diTbl),
--		sys.dump(mpTbl), "oldxy:["..ox..","..oy.."]", "newxy["..nx..","..ny.."]"
--		)	
end

--返回true/nil, 与上面的JumpTo一样的, 只是用来查bug, 返回值不同, 而又不确定上面的方法是否已经用在其他地方
function clsUser:UserJumpTo(nx, ny, nz, cbidx, notCheckMove)		--添加跳转函数(只需要判断nx,ny点是否有npc,user,partner或者墙)
	if self:CheckIsFly() then
		return -1	--在跳跃中
	end

	if not notCheckMove then 
		if not self:CanMove() then 
--			error("JumpTo CanMove error")
			return -2	--有buff 不能拉
		end
	end
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then
			return -3	-- z轴错误
		end
	end
	local ox, oy, oz = self:GetX(), self:GetY(), self:GetZ()
	
	if self:IsBlockPoint(nx, ny, nz) then
--		error("the block[" .. nx .. "," .. ny .. "," .. nz .. "]")
		return -4		-- 处于阻挡点
	end
	
	local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(self:GetMapObj(), self:GetEngineObj(), nx, ny)
--	assert(retnum, "clsUser:JumpTo error user:" .. self:GetName())
	if not retnum then
		return -5		-- 引擎错误
	end
	
	if retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		self:SetTmp("LastMoveTime", GetNowTimeNo())
		self:SetX(nx)
		self:SetY(ny)
		self:SetZ(nz)
		self:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		self:SendMove(ox, oy, oz, retnum, true, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl)
		if cbidx then
			lretmap.userjump(self:GetVfd(), MAP_ID, self:GetMapLayer(), nx, ny, nz, lserialize.lua_seri_str({
				cbidx=cbidx,
			}))
		end
		return 1
--		return true
	else
		if ox ~= nx or oy ~= ny then			--不同位置
--			error(string.format("user:%s JumpTo retnum:%s error, ox:%s, oy:%s, nx:%s, ny:%s", self:GetName(), retnum, ox, oy, nx, ny))
			return -6
		end
	end
	
--	_RUNTIME("clsUser:JumpTo 2:", retnum, 
--		sys.dump(apTbl), sys.dump(anTbl), sys.dump(aiTbl),
--		sys.dump(dpTbl), sys.dump(dnTbl), sys.dump(diTbl),
--		sys.dump(mpTbl), "oldxy:["..ox..","..oy.."]", "newxy["..nx..","..ny.."]"
--		)	
end


function clsUser:LeaveMap(notRetMap)			--添加离开地图后的处理,必须重载,不然报错
	local isOk, playerTbl = laoi.map_removeobj(self:GetMapObj(), self:GetEngineObj())	
	assert(isOk, "clsUser:LeaveMap error: " .. self:GetName())
	local selfId = self:GetId()
	
	pbc_send_msg(self:GetVfd(), "S2c_aoi_leave", {
		map_no = MAP_NO,
		map_id = MAP_ID,
		id = self:GetId(),
	})	
	
	FIGHT_EVENT.DelDaZuo(self)
	
	if playerTbl then
		local vfds = {}
		for _, pCharId in pairs(playerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				tinsert(vfds, pCharObj:GetVfd())
			end
		end
		if #vfds > 0 then
			pbc_send_msg(vfds, "S2c_aoi_leave", {
				map_no = MAP_NO,
				map_id = MAP_ID,
				id = self:GetId(),
				
			})		
		end		
	end
	
	if not notRetMap then
		local taskBuffNo = self:GetTaskBuffNo()
		
		local extendTbl = {
			Sp = self:GetSp(),
			taskBuffNo = taskBuffNo,
			setHp = self:GetFightValue("OldHp"),
		}
		local extendData = lserialize.lua_seri_str(extendTbl)
		if self:IsMirrorPlayer() then
			lretmap.static_npcleave(self:GetId(), MAP_ID, self:GetMapLayer())
		else
			lretmap.userleave(self:GetVfd(), MAP_ID, self:GetMapLayer(), self:GetId(), extendData)
		end
	end

	self:MoveChangeMapPos(CHANGE_MAPPOS_LEAVE)
	--需要发送给自己表示离开此地图
	self:PartnerDestroy()				--删除同伴
	self:MagicDestroy()					--删除法器
end

function clsUser:SendFlyError()
	pbc_send_msg(self:GetVfd(), "S2c_aoi_player_fly_error", {
		pos = {
			self:GetX(),self:GetY(),
		},
		y = self:GetZ(),
	})
--	_RUNTIME("_________:", self:GetName(), debug.traceback())
end

function clsUser:SetAidSkillId(skillId)
	local allSkill = self:GetAllSkill()
	local skillData = allSkill[skillId]
	if not skillData then return end
	if skillData.Type == SKILL_TYPE_INITIATIVE then
		local mtype = skillData.Mtype
		local timeNoCnt = skillData.CD
		local eTimeNo = skillData.CDEndTimeNo
		local nTimeNo = GetNowTimeNo()
		if nTimeNo >= eTimeNo then
			skillData.CDEndTimeNo = nTimeNo + timeNoCnt
			return true, timeNoCnt * 1000 * lua_time_sec, mtype
		end
	end
end

--------------------------------------------------------------------协议处理------------------------------------------------------------------
function AoiSkillAct(UserObj, protoMsg)
	UserObj:CheckBreakDaZuo()
	UserObj:CheckBreakDoubleXiulian()
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE10)
	SendMainClientInState(UserObj)
	local skillId = protoMsg.skill_id
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then return end
	--非华山论剑不能使用技能,杀戮战场技能相同
	if skillId==40000301 and (MAP_NO~=1086 and MAP_NO~=1159) then
		return
	end
	
	if not UserObj:CanUserSkillByBuff(skillId) then return end
	local tarCharObj = CHAR_MGR.GetCharById(protoMsg.tar_id)
	if skillData.TargetType==SINGLE_TARGET_TYPE then
		if not tarCharObj then return end
		if tarCharObj:GetMapLayer() ~= UserObj:GetMapLayer() then return end
		if tarCharObj:IsDie() then return end
	end
	if tarCharObj then
		if tarCharObj:IsDie() then
			pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skillerror", {
				id = protoMsg.tar_id,
				is_exist = 3,
				pos = POS_EMPTY,
			})
			return
		end
	end
	
	if UserObj:IsDie() then return end
	local nTimeNo = GetNowTimeNo()
	local ncoolc = UserObj:GetASkillCoolCnt()
	local uSkillData = UserObj:GetOneSkillData(skillId)
	if not uSkillData then return end
	if uSkillData.AidSkill then
		local isOk, ctime, mtype = UserObj:SetAidSkillId(skillId)
		if isOk then
			if ctime > 0 then
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skill_ctime", {fid = UserObj:GetFId(), skill_id = skillId, ctime = ctime})
			end
			local ret = FIGHT.UseSkillAct(UserObj, tarCharObj, skillId, protoMsg.tx, protoMsg.ty, protoMsg.axyz, protoMsg.timestamp)
			if ret then
				local skillIdList = UserObj:GetSkillAct() or {}
				skillIdList[skillId] = true
				UserObj:SetSkillAct(skillIdList)
			end
		end
	else
		if uSkillData.SkillTime ~= 0 then
			if ncoolc > nTimeNo + SKILL_TIME_ACCU then return end			--还没到技能时间
		end
		local isOk, ctime, mtype = UserObj:SetNowSkillId(skillId)
		if isOk then
			if ctime > 0 then
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skill_ctime", {fid = UserObj:GetFId(), skill_id = skillId, ctime = ctime})
			end
			local ret = FIGHT.UseSkillAct(UserObj, tarCharObj, skillId, protoMsg.tx, protoMsg.ty, protoMsg.axyz, protoMsg.timestamp)
			if ret then
				local skillIdList = UserObj:GetSkillAct() or {}
				skillIdList[skillId] = true
				UserObj:SetSkillAct(skillIdList)
				if uSkillData.SkillTime ~= 0 then
					UserObj:SetASkillCoolCnt(nTimeNo + (uSkillData.SkillTime or askill_cool_cnt))
				end
			end
			UserObj:ClearNowSkillId()
		end
	end
end	

local POS_EMPTY = {}
function AoiSkillHit(UserObj, protoMsg)
	UserObj:CheckBreakDaZuo()
	UserObj:CheckBreakDoubleXiulian()
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE10)
	SendMainClientInState(UserObj)
	local skillId = protoMsg.skill_id
	local skillIdList = UserObj:GetSkillAct()
	if skillIdList and skillIdList[skillId] then
		skillIdList[skillId] = nil
		UserObj:SetSkillAct(skillIdList)
		if UserObj:IsDie() then return end
		local skillData = SKILL_DATA.GetMartialSkill(skillId)
		if not skillData then return end
		local tarCharObj = CHAR_MGR.GetCharById(protoMsg.tar_id)
		if skillData.TargetType==SINGLE_TARGET_TYPE then
			if not tarCharObj then return end
			if tarCharObj:GetMapLayer() ~= UserObj:GetMapLayer() then return end
			if tarCharObj:IsDie() then return end
		end
		if tarCharObj then
			if tarCharObj:IsDie() then
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skillerror", {
					id = protoMsg.tar_id,
					is_exist = 3,
					pos = POS_EMPTY,
				})
				return
			end
		end
		local ret = FIGHT.UseSkillHit(UserObj, tarCharObj, skillId, protoMsg.tx, protoMsg.ty, protoMsg.axyz, protoMsg.timestamp)
--		if not ret then
--			if not tarCharObj then
--				_RUNTIME_ERROR("not hit 1:", MAP_NO, skillId, ret, protoMsg.tx, protoMsg.ty, UserObj:GetX(), UserObj:GetY())
--			else
--				_RUNTIME_ERROR("not hit 2:", MAP_NO, skillId, ret, tarCharObj:GetX(), tarCharObj:GetY(), UserObj:GetX(), UserObj:GetY())
--			end
--		end
		if not ret and not tarCharObj then
			if protoMsg.tar_id ~= "0" then
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_skillerror", {
					id = protoMsg.tar_id,
					is_exist = 2,
					pos = POS_EMPTY,
				})
			end
		end
		if ret and tarCharObj then
			if not UserObj:IsShapeshift() then					--没变身的才攻击
				local allPartner = UserObj:GetAllPartner()
				for _, _PartnerObj in pairs(allPartner) do
					if not tarCharObj:IsDie() then
						TryCall(_PartnerObj.SkillCheckOnce, _PartnerObj, tarCharObj)
					end
				end
				local allMagic = UserObj:GetAllMagic()
				for _, _MagicObj in pairs(allMagic) do
					if not tarCharObj:IsDie() then
						TryCall(_MagicObj.SkillCheckOnce, _MagicObj, tarCharObj)
					end
				end
			end
		end
		if ret then
			local allSkill = UserObj:GetAllSkill()
			local oData = allSkill[skillId]
			if not oData then return end
			
			--判断一下是否主动技能,排除怒气技能
			if oData.Type == SKILL_TYPE_INITIATIVE and oData.Mtype ~= SKILL_MTYPE_HETIJI then
				local uGrade = UserObj:GetGrade() or 1
				--判断是否需要升级,武学等级与技能等级
				if uGrade > oData.Lv then
					if IsServer() then
						lretmap.other(UserObj:GetId(), MAP_ID, UserObj:GetMapLayer(), lserialize.lua_seri_str({
							type = RETMAP_MARTIALEXP,
							martialId = skillData.MartialId,
						}))		
					else
						lretmap.other({
							type = RETMAP_MARTIALEXP,
							sexp = {
								martial_id = skillData.MartialId or 0,
							},
						})		
					end			
				end
			end
		end
	else
--		_RUNTIME_ERROR("act hit not same skillid:", MAP_NO, UserObj:GetName(), skillId, sys.dump(skillIdList))
	end
end

function clsUser:GetMapClientInState()
	return self:GetTmp("MapClientInState")
end

function clsUser:SetMapClientInState(state)
	self:SetTmp("MapClientInState", state)
end

function SendMainClientInState(UserObj)
	if IsServer() and not UserObj:GetMapClientInState() then
		UserObj:SetMapClientInState(true)
		lretmap.other(UserObj:GetId(), MAP_ID, UserObj:GetMapLayer(), lserialize.lua_seri_str({
			type = RETMAP_CLIENT_INSTATE,
		}))	
	end
end

MOVE_SYNC_D = {}
setmetatable(MOVE_SYNC_D, {__mode = "v"}) --弱表

function SyncPlayerPosition()
	if IS_DELAY_RETMOVE then
		local data = {}
		local isOk = false
		for _id, _UserObj in pairs(MOVE_SYNC_D) do
			MOVE_SYNC_D[_id] = nil
			data[_id] = {
				x = _UserObj:GetX(),
				y = _UserObj:GetY(),
				z = _UserObj:GetZ(),
				mapLayer = _UserObj:GetMapLayer(),
			}
			isOk = true
		end
		
		if isOk then
			lretmap.other("0", MAP_ID, 1, lserialize.lua_seri_str({
				type = RETMAP_PLAYER_POS,
				data = data,
				mapId = MAP_ID,
			}))	
		end
	end
end

function UserMove(UserObj, protoMsg)
	UserObj:CheckBreakDaZuo()
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	SendMainClientInState(UserObj)
	local flydata = UserObj:GetFlyData() 
	if flydata.mtype then return end			--跳跃中不移动
	local ret = UserObj:MoveTo(protoMsg.pos[1], protoMsg.pos[2], nil, protoMsg.speed, nil, protoMsg.type)
	if ret then
		if IS_DELAY_RETMOVE then
			if IS_RET_MOVE then
				MOVE_SYNC_D[UserObj:GetId()] = UserObj
			end
		end
		UserObj:SetDir360(protoMsg.dir360)
		UserObj:CheckBreakDoubleXiulian()		--移动则取消双修
	end
end

function AoiPlayerFly(UserObj, protoMsg)					--todo 飞行次数,添加开始位置与当前位置判断，超过4就弹回
	UserObj:CheckBreakDaZuo()
	UserObj:CheckBreakDoubleXiulian()
	if UserObj:IsShapeshift() then 
		UserObj:SendFlyError()
		return
	end
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	SendMainClientInState(UserObj)
	if (UserObj:GetIsYunBiao() or 0)>0 then return end
	local flydata = UserObj:GetFlyData()
	local mtype = protoMsg.type
	local tOut = protoMsg.time
	tOut = mceil(tOut / lua_time_sec)
	if tOut < 0 then
		UserObj:SendFlyError()
		return
	end
	if not UserObj:CanMove() then 
		UserObj:SendFlyError()
		return 
	end
	
--	_RUNTIME(UserObj:GetName(), protoMsg.pos[1], protoMsg.pos[2], UserObj:GetX(), UserObj:GetY())
	if mtype == MOVETYPE_FLY1 then
		if tOut > 0 then
			if flydata.mtype then
				UserObj:SendFlyError()
				return 
			end
			
			local flyDodge = UserObj:GetFlyDodge()
			if flyDodge <= 0 then
				UserObj:SendFlyError()
				return 	
			else
				UserObj:SubFlyDodge(1)		
			end
			
			local nTimeNo = GetNowTimeNo()
			flydata.mtype = MOVETYPE_FLY1
			flydata.stno = nTimeNo
			flydata.cetno = nTimeNo + tOut
			flydata.etno = nTimeNo + MAX_FIYTIMENO
			flydata.sx = protoMsg.pos[1]
			flydata.sy = protoMsg.pos[2]
			flydata.sz = protoMsg.y
			FIGHT_EVENT.AddFlyPlayer(UserObj, flydata.etno)
		else
			if not flydata.mtype then
				UserObj:SendFlyError()
				return 
			end			
		end
	elseif mtype == MOVETYPE_FLY2 then
		if tOut > 0 then
			if flydata.mtype ~= MOVETYPE_FLY1 then
				UserObj:SendFlyError()
				return
			end
			flydata.mtype = MOVETYPE_FLY2
			flydata.cetno = flydata.cetno + tOut
		else
			if not flydata.mtype then
				UserObj:SendFlyError()
				return 
			end	
		end
	elseif mtype == MOVETYPE_FLY3 then					--位移同步
		if not flydata.mtype then
			UserObj:SendFlyError()
			return 
		end			
	else
		UserObj:SendFlyError()
		return
	end

	local nx, ny = protoMsg.pos[1], protoMsg.pos[2]
	local ox, oy, oz = UserObj:GetX(), UserObj:GetY(), UserObj:GetZ()
	if UserObj:IsBlockPoint(nx, ny) or (ox == nx and oy == ny) then
		local playerTbl = UserObj:GetNearByPlayers()		--都可能有位移变化
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			pbc_send_msg(vfds, "S2c_aoi_player_fly", {
				pos = protoMsg.pos,
				y = protoMsg.y,
				fid = UserObj:GetFId(),
				type = mtype,
				dir360 = protoMsg.dir360,
				vertSpeed = protoMsg.vertSpeed,
				horizSpeed = protoMsg.horizSpeed,
				horizAccSpeed = protoMsg.horizAccSpeed,
				horizEndSpeed = protoMsg.horizEndSpeed,
				syncpos = protoMsg.syncpos,
				timer = protoMsg.timer,
			})
		end		
	else
		local nz = lmapdata.getz(MAP_NO, nx, ny)
		assert(nz)
		local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(UserObj:GetMapObj(), UserObj:GetEngineObj(), nx, ny)	
		if not retnum then
			UserObj:SendFlyError()
			return
		elseif retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
			UserObj:SetX(nx)
			UserObj:SetY(ny)
			UserObj:SetZ(nz)
			UserObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
			UserObj:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, nil, mtype, {
				pos = protoMsg.pos,
				y = protoMsg.y,
				fid = UserObj:GetFId(),
				type = mtype,
				dir360 = protoMsg.dir360,
				vertSpeed = protoMsg.vertSpeed,
				horizSpeed = protoMsg.horizSpeed,
				horizAccSpeed = protoMsg.horizAccSpeed,
				horizEndSpeed = protoMsg.horizEndSpeed,
				syncpos = protoMsg.syncpos,
				timer = protoMsg.timer,				
			})
			if IS_RET_MOVE then
				lretmap.usermove(UserObj:GetVfd(), MAP_ID, UserObj:GetMapLayer(), nx, ny, nz)
			end
		else
			if ox ~= nx or oy ~= ny then			--不同位置才设置回退
				UserObj:SendFlyError()
				return
			end
		end
	end
	
	UserObj:SetDir360(protoMsg.dir360)
end

function AoiSprint(UserObj, protoMsg)
	UserObj:CheckBreakDaZuo()
	UserObj:CheckBreakDoubleXiulian()
	if UserObj:IsShapeshift() then return end
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	SendMainClientInState(UserObj)
	local nx, ny = protoMsg.pos[1], protoMsg.pos[2]
	local ox, oy, oz = UserObj:GetX(), UserObj:GetY(), UserObj:GetZ()
	
	if not UserObj:CanMove() then 
		UserObj:MoveErrorSend()
	end
	local nz = nil
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then return end
	end
	if UserObj:IsBlockPoint(nx, ny, nz) then
		error("the block[" .. nx .. "," .. ny .. "," .. nz .. "]")
	end
	
	local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(UserObj:GetMapObj(), UserObj:GetEngineObj(), nx, ny)
	if not retnum then
		UserObj:MoveErrorSend()
		return
	elseif retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		UserObj:SetX(nx)
		UserObj:SetY(ny)
		UserObj:SetZ(nz)
		UserObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		UserObj:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, nil, MOVETYPE_SPRINT, {
			fid = UserObj:GetFId(),
			fpos = protoMsg.fpos,
			y = protoMsg.y,
		})
		if IS_RET_MOVE then
			lretmap.usermove(UserObj:GetVfd(), MAP_ID, UserObj:GetMapLayer(), nx, ny, nz)
		end
		UserObj:SetDir360(protoMsg.dir360)
	else
		if ox ~= nx or oy ~= ny then			--不同位置才设置回退
			UserObj:MoveErrorSend()
			return
		end
	end
end

function AoiPlayerFlyFinished(UserObj, protoMsg)
	UserObj:CheckBreakDaZuo()
	UserObj:CheckBreakDoubleXiulian()
	FIGHT_EVENT.DelBuffType(UserObj, UserObj, BUFF_TYPE8)
	SendMainClientInState(UserObj)
	local flydata = UserObj:GetFlyData()
--	assert(flydata.mtype)
	if not flydata.mtype then return end
	if UserObj:IsBlockPoint(protoMsg.pos[1], protoMsg.pos[2]) then 
		UserObj:SendFlyError()
		return 
	end
	FIGHT_EVENT.DelFlyPlayer(UserObj, flydata.etno)	
	UserObj:ClearFlyData()
	
	local nx, ny = protoMsg.pos[1], protoMsg.pos[2]
	local ox, oy, oz = UserObj:GetX(), UserObj:GetY(), UserObj:GetZ()
	if ox == protoMsg.pos[1] and oy == protoMsg.pos[2] then
		local playerTbl = UserObj:GetNearByPlayers()
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			pbc_send_msg(vfds, "S2c_aoi_player_fly_finished", {
				fid = UserObj:GetFId(),
				pos = protoMsg.pos,
				syncpos = protoMsg.syncpos,
				timer = protoMsg.timer
			})
		end			
	else
--		UserObj:MoveTo(protoMsg.pos[1], protoMsg.pos[2], nil, nil, nil, MOVETYPE_FLYF)
		local nz = lmapdata.getz(MAP_NO, nx, ny)
		assert(nz)
		local retnum, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl = laoi.map_moveobj(UserObj:GetMapObj(), UserObj:GetEngineObj(), nx, ny)
		if not retnum then
			UserObj:SendFlyError()
			return
		elseif retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
			UserObj:SetX(nx)
			UserObj:SetY(ny)
			UserObj:SetZ(nz)
			UserObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
			UserObj:SendMove(ox, oy, oz, retnum, false, apTbl, apaTbl, anTbl, aiTbl, dpTbl, dpaTbl, dnTbl, diTbl, mpTbl, nil, MOVETYPE_FLYF, {
				fid = UserObj:GetFId(),
				pos = protoMsg.pos,
				syncpos = protoMsg.syncpos,
				timer = protoMsg.timer,
			})
			if IS_RET_MOVE then
				lretmap.usermove(UserObj:GetVfd(), MAP_ID, UserObj:GetMapLayer(), nx, ny, nz)
			end
		else
			if ox ~= nx or oy ~= ny then			--不同位置才设置回退
				UserObj:SendFlyError()
				return
			end
		end
	end
end

local BUFF_MOD = Import("setting/skill/buff_data.lua")
local BUFF_PROPDATA = BUFF_MOD.GetAllBuffData()
local WARRIOR_BUFFPROP_NAME = WARRIOR_BUFFPROP_NAME

local EVILCOOLTIME_DATA = nil
local MAX_EVILCOOL_VALUE = nil
local MAX_EVILCOOL_TIME = nil
if IsServer() then
	EVILCOOLTIME_DATA = Import("setting/evil/evil_data.lua").GetEvilCoolTimeData()
	MAX_EVILCOOL_VALUE = #EVILCOOLTIME_DATA
	MAX_EVILCOOL_TIME = EVILCOOLTIME_DATA[MAX_EVILCOOL_VALUE]
end

local function GetBuffProtoMsg(UserObj, tips, ttips, buffInfo)
	for _buffId, _buffData in pairs(buffInfo) do
		local _data = BUFF_PROPDATA[_buffId]
		if _data and _data.BuffPhotoId > 0 and _data.ShowInBuff == 1 then
			if _data.BuffType ~= BUFF_TYPE8 then
				if _buffId == EVIL_BUFFID or _buffId == EVIL_BUFFID_CLIENT then			--红名
					local tmp = {
						photo = _data.BuffPhotoId,
						name = _data.BuffHName,
						key = {},
						value = {},
						svalue = {},
					}
					if _buffId == EVIL_BUFFID then 
						local nTime = os.time()
						local evilTime = UserObj:GetEvilTime()
						local evilValue = UserObj:GetEvilValue()
						
						local lTime = 0
						
						for i = EVIL_REDVALUE + 1, evilValue do
							lTime = lTime + (EVILCOOLTIME_DATA[i] or MAX_EVILCOOL_TIME)
						end
						lTime = lTime - (nTime - evilTime)
						if lTime <= 0 then
							lTime = 0
						end
						
						local mTime = mceil(lTime / 60)
						local hour = mfloor(mTime / 60)
						local min = mTime % 60
						
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, hour, min
						))
					else
						tinsert(tmp.svalue, _data.BuffInfo)
					end
					tinsert(tips, tmp)
				elseif _buffData.notFight then
					if _buffData.buffType == NOTFIGHT_BUFF_TYPE_HP then
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, _buffData.hp
						))
						tinsert(tips, tmp)
					elseif _buffData.buffType == NOTFIGHT_BUFF_TYPE_EXP1_5 or _buffData.buffType == NOTFIGHT_BUFF_TYPE_EXP2_0 or 
							_buffData.buffType == NOTFIGHT_BUFF_TYPE_EXP4_0 then
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						local restTime = _buffData.ctime + _buffData.stime - os.time()
						if restTime < 0 then restTime = 0 end
						local h = mfloor(restTime / 3600)
						local m = mfloor((restTime - h * 3600) / 60)
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, h, m
						))
						tinsert(tips, tmp)
					elseif _buffData.buffType == NOTFIGHT_BUFF_TYPE_WORLDEXP then
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						
						local expRate = _buffData.expRate*100
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, _buffData.worldGrade, expRate, expRate 
						))
						tinsert(tips, tmp)
					elseif _buffData.buffType == NOTFIGHT_BUFF_TYPE_YEGUAIREWARD then
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						
						local remainTime = _buffData.remainTime
						if remainTime < 0 then remainTime = 0 end
						local h = mfloor(remainTime / 3600)
						local m = mfloor((remainTime - h * 3600) / 60)
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, h, m 
						))
						tinsert(tips, tmp)
					elseif _buffData.buffType == NOTFIGHT_BUFF_TYPE_MERGEACT then
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						
						local remainTime = _buffData.remainTime
						if remainTime < 0 then remainTime = 0 end
						local h = mfloor(remainTime / 3600)
						local m = mfloor((remainTime - h * 3600) / 60)
						tinsert(tmp.svalue, string.format(
							_data.BuffInfo, h, m 
						))
						tinsert(tips, tmp)
					else
						assert(_data.BuffInfo)
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						tinsert(tmp.svalue, _data.BuffInfo)
						tinsert(tips, tmp)					
					end
				else
					if _data.BuffMergeId then
						if not ttips[_data.BuffMergeId] then
							ttips[_data.BuffMergeId] = {
								photo = _data.BuffPhotoId,
								name = _data.BuffHName,
								key = {},
								value = {},
								svalue = {},
							}
						end
						if _buffData.buffInfo then
							tinsert(ttips[_data.BuffMergeId].svalue, _buffData.buffInfo)
						else
							for _key, _value in pairs(_buffData) do
								if WARRIOR_BUFFPROP_NAME[_key] then
									ttips[_data.BuffMergeId].key[_key] = true
									ttips[_data.BuffMergeId].value[_key] = (ttips[_data.BuffMergeId].value[_key] or 0) + _value
								end
							end
						end
					else
						local tmp = {
							photo = _data.BuffPhotoId,
							name = _data.BuffHName,
							key = {},
							value = {},
							svalue = {},
						}
						if _buffData.buffInfo then
							tinsert(tmp.svalue, _buffData.buffInfo)
						else
							for _key, _value in pairs(_buffData) do
								if WARRIOR_BUFFPROP_NAME[_key] then
									tinsert(tmp.key, _key)
									tinsert(tmp.value, _value)					
								end
							end
						end
						tinsert(tips, tmp)
					end
				end
			end
		end
	end
end

function AoiBuffTips(UserObj, protoMsg)
	local rid = protoMsg.rid
	local CharObj = CHAR_MGR.GetCharById(rid)
	if not CharObj then return end
	local buffInfo = CharObj:GetBuffInfo()
	local ttips = {}
	local tips = {}
	local sendMsg = {
		fid = CharObj:GetFId(),
		tips = tips,
	}
	
	if buffInfo then
		GetBuffProtoMsg(CharObj, tips, ttips, buffInfo)
	end
	if CharObj:IsPlayer() then
		local onePartnerObj = CharObj:GetOnePartner()
		if onePartnerObj then
			local pbuffInfo = onePartnerObj:GetBuffInfo()
			if pbuffInfo then
				GetBuffProtoMsg(CharObj, tips, ttips, pbuffInfo)
			end
		end
		local oneMagicObj = CharObj:GetOneMagic()
		if oneMagicObj then
			local mbuffInfo = oneMagicObj:GetBuffInfo()
			if mbuffInfo then
				GetBuffProtoMsg(CharObj, tips, ttips, mbuffInfo)
			end
		end
	end

	if next(ttips) ~= nil then
		for _, _data in pairs(ttips) do
			local tmp = {
				photo = _data.photo,
				name = _data.name,
				key = {},
				value = {},
				svalue = _data.svalue,
			}
			for _key, _ in pairs(_data.key) do
				tinsert(tmp.key, _key)
				tinsert(tmp.value, _data.value[_key])					
			end
			tinsert(tips, tmp)			
		end
	end
	if #tips > 0 then
		pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_bufftips", sendMsg)
	end
end

function OnAoiSearchNpc(UserObj, protoMsg)
	local npcNo = protoMsg.npc_no
	
	local nearTbl = UserObj:GetNearByNpc()
	if nearTbl then
		for _, pCharId in pairs(nearTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj and pCharObj:GetCharNo() == npcNo and not pCharObj:IsDie() and pCharObj:GetCanSee(UserObj) then
				--添加npc
				pbc_send_msg(UserObj:GetVfd(), "S2c_aoi_addnpc", {
					nmsg = {
						fid = pCharObj:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = pCharObj:GetId(),
						x = pCharObj:GetX(),
						z = pCharObj:GetY(),
						y = pCharObj:GetZ(),
						hp = pCharObj:GetHp(),
						hpmax = pCharObj:GetFightValue(mFIGHT_HpMax),
						char_no = pCharObj:GetCharNo(),
					},
					sync = pCharObj:GetSyncData()
				})	
			end
		end
	end
end

function OnAoiShapeshiftDel(UserObj, protoMsg)
	local buffId = protoMsg.buffid
	local buffInfo = UserObj:GetBuffInfo()
	if buffInfo and buffInfo[buffId] then
		FIGHT_EVENT.DelBuff(UserObj, buffId)
		local sHp, sHpMax, sBuffId = UserObj:FixedShieldBuffSyncHp()
		if sBuffId then
			FIGHT_EVENT.DelBuff(UserObj, sBuffId)
		end
	end
end

function __init__()
	func_call.C2s_aoi_move = UserMove
	func_call.C2s_aoi_skill_act = AoiSkillAct
	func_call.C2s_aoi_skill_hit = AoiSkillHit
	func_call.C2s_aoi_player_fly = AoiPlayerFly
	func_call.C2s_aoi_player_fly_finished = AoiPlayerFlyFinished
	func_call.C2s_aoi_bufftips = AoiBuffTips
	func_call.C2s_aoi_sprint = AoiSprint
	func_call.C2s_aoi_searchnpc = OnAoiSearchNpc
	func_call.C2s_aoi_shapeshift_del = OnAoiShapeshiftDel
end