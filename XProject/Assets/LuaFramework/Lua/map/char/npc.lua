local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local mfloor=math.floor
local mceil=math.ceil
local mabs=math.abs
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local NORMAL_NPC_RADIUS = NORMAL_NPC_RADIUS
local lua_time_sec = lua_time_sec
local tinsert = table.insert
local BATTLE_CAMP_1 = BATTLE_CAMP_1
local BATTLE_CAMP_2 = BATTLE_CAMP_2
local DISTANCE_ADJUST_SPEED = DISTANCE_ADJUST_SPEED

local SKILL_MTYPE_NORMAL = SKILL_MTYPE_NORMAL			--普通
local SKILL_MTYPE_MAGIC = SKILL_MTYPE_MAGIC				--技能
local SKILL_MTYPE_HETIJI = SKILL_MTYPE_HETIJI			--合体技

local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能
local EVENT_TOATTACK = EVENT_TOATTACK
local RETMAP_TYPE_NPCDIE_REWARD = RETMAP_TYPE_NPCDIE_REWARD

local pbc_send_msg = pbc_send_msg

local IsKuaFuServer = cfgData and cfgData.IsKuaFuServer

clsNpc = BASECHAR.clsBaseChar:Inherit({__ClassType = "NPC"})

function clsNpc:__init__(x, y, z, syncData, ociData, mapLayer, hpRate)
	if ociData.BossType == BOSS_TYPE_CONVOY or ociData.BossType == BOSS_TYPE_SHUIJING then			--需要提前预判设置阵营
		ociData.Comp = BATTLE_CAMP_1
	end
	self.fsSkill = 0
	self.__npc_hpRate = hpRate
	Super(clsNpc).__init__(self, x, y, z, NPC_TYPE, syncData, ociData, mapLayer)
	
	local npcNo = self:GetCharNo()
	local allNpcBattleData = NPC_BATTLE_DATA.GetAllNpcData()
	if not self:IsStaticNpc() then
		if not allNpcBattleData[npcNo] then
			error(string.format("__not npc_no:%s in map_no:%s__", npcNo, MAP_NO))
		end
	end
	if allNpcBattleData and allNpcBattleData[npcNo] and allNpcBattleData[npcNo].FreshDelayTime then
		local delayTimeNo = mceil(allNpcBattleData[npcNo].FreshDelayTime / (1000 * lua_time_sec)) + GetNowTimeNo()
		self:SetActDelayTime(delayTimeNo)
	end
	
	local range = self:GetAITrackRange()
	local rTbl = {
		minx = x - range,
		maxx = x + range,
		miny = y - range,
		maxy = y + range,
		x = x,
		y = y,
		range = range,
	}
	self:SetAIRange(rTbl)
	
	local sTime = 2
	if allNpcBattleData and allNpcBattleData[npcNo] and allNpcBattleData[npcNo].WalkIntervalTime > 0 then			--npc有行走间隔
		local wInervalTime = allNpcBattleData[npcNo].WalkIntervalTime
		local sp = self:GetSpeed()
		local wTime = mceil(wInervalTime / (1000 * lua_time_sec))
		local intervalSp = mceil(wTime * 3.5)
		if sp >= intervalSp then				--行走速度大于间隔时间了
			self:AdjustSpeed()		--调整速度
			sTime = SPEED_TIME[self:GetSpeed()]
			assert(sTime, string.format("name:%s not sTime:%s", self:GetName(), self:GetSpeed()))	
		else
			sTime = wTime
		end
	else
		self:AdjustSpeed()		--调整速度
		sTime = SPEED_TIME[self:GetSpeed()]
		assert(sTime, string.format("name:%s not sTime:%s", self:GetName(), self:GetSpeed()))
	end
	
	if not self:IsStaticNpc() then
		local aiObj = nil
		local atkTime = mceil(self:GetAtkTime() / (1000 * lua_time_sec))
		if self:GetBossType() == BOSS_TYPE_BOX or self:GetBossType() == BOSS_TYPE_NPCBOX or self:GetBossType() == BOSS_TYPE_SHUIJING 
			or self:GetBossType() == BOSS_TYPE_CLUBTER_TT or self:GetBossType() == BOSS_TYPE_CLUBTER_BQ then
		elseif self:GetBossType() == BOSS_TYPE_CLUBWAR or self:GetBossType() == BOSS_TYPE_CLUBTER_JT or self:GetBossType() == BOSS_TYPE_KSIEGEWAR then
--			local dir = self:GetDir()
--			local toDirData = MOVE_DIR[dir] or MOVE_DIR[1]
--			aiObj = AI_WBOSSATTACK.clsAIWBossAttack:New(self, atkTime, self:GetX() + toDirData[1], self:GetY() + toDirData[2])
--			self:SetWorldBossSkillList()

			aiObj = AI_CWARBOSSATTACK.clsAICWarBossAttack:New(self, atkTime)
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_HUASHAN then
			aiObj = AI_WALKATTACKTIME.clsAIWalkAttackTime:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid())
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_WALKAROUND then
			aiObj = AI_WALKAROUND.clsAIWalkAround:New(self, sTime, atkTime, self:GetWalkGrid())
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_ONLYNPC then
			aiObj = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid(), NPC_TYPE)
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_SHOUWEI_PF then
			aiObj = AI_SHOUWEIPLAYERFIRST.clsAIShouWeiPlayerFirst:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid())
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_WALKSHOW then
			aiObj = AI_WALKSHOW.clsAIWalkShow:New(self, self:GetId(), self:GetAIRadius(), 1, sTime, self:GetWalkGrid(), self:GetTmp("WalkShowPos"))
			self:SetAI(aiObj)
		elseif self:GetBossType() == BOSS_TYPE_YEWAIBOSS then
			aiObj = AI_YEWAIBOSS.clsAIYewaiBoss:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid())
			self:SetAI(aiObj)
		else
			if self:GetIsActiveAttack() == ACTIVE_NPC then
				aiObj = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid())
				self:SetAI(aiObj)
				--主动攻击的,还需要判断周围是否有玩家,有玩家则直接设置追踪攻击   (EVENT_TOATTACK)
				local tarObj = self:SearchOCompCharObj(self:GetAIRadius())
				if tarObj then
					aiObj:OnEvent({
						eventType = EVENT_TOATTACK,
						eventAttackCharId = tarObj:GetId(),
						attX = tarObj:GetX(),
						attY = tarObj:GetY(),
					})	
				end
			else
				aiObj = AI_WALKAROUNDBEATTACK.clsAIWalkAroundBeAttack:New(self, self:GetAIRadius(), 1, sTime, atkTime, self:GetWalkGrid())
				self:SetAI(aiObj)
			end
		end		
	end

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
	
--	FIGHT_EVENT.ProcessPassMessage(PASS_BATTLE, self, nil, nil)				--触发被动技能
	
	local t1HpMax = self:GetHpMax() or 0
	local t2HpMax = self:GetFightValue(mFIGHT_HpMax) or 0
	if t1HpMax ~= t2HpMax then
		self:AddHp(t2HpMax - t1HpMax, 0)
	end
	if hpRate then
		local hp = t1HpMax*hpRate
		if hp < t1HpMax then
			self:SetHp(hp) 
		end
	end
	
	if ociData.MainNpc then
		lretmap.static_npcadd(self:GetId(), MAP_ID, self:GetMapLayer(), self:GetX(), self:GetY(), self:GetZ())
	end
end

function clsNpc:GetMainNpc()
	return self:GetTmp("MainNpc")
end

function clsNpc:SetMainNpc(MainNpc)
	self:SetTmp("MainNpc", MainNpc)
end

function clsNpc:GetCanSee(UserObj)
	local canSeeType = self:GetTmp("CanSee") or CANSEE_TYPE_CAN
	if canSeeType ~= CANSEE_TYPE_CANT then
		return true
	else
		local npcList = UserObj:GetSeeNpcNoList() or {}
		if npcList[self:GetCharNo()] then
			return true
		end
	end
end
function clsNpc:SetCanSee(canSee)
	canSee = canSee or CANSEE_TYPE_CAN
	local oCanSee = self:GetTmp("CanSee") or CANSEE_TYPE_CAN
	self:SetTmp("CanSee", canSee)
	if not self.IsInInit and oCanSee ~= canSee then
		local playerTbl = self:GetNearByPlayers()
		if playerTbl then
			if canSee == CANSEE_TYPE_CAN then		--变成可见
				local vfds = {}
				for _, pCharId in pairs(playerTbl) do
					local pCharObj = CHAR_MGR.GetCharById(pCharId)
					local npcList = pCharObj:GetSeeNpcNoList() or {}
					if pCharObj and not npcList[self:GetCharNo()] then		--原本不可见的
						tinsert(vfds, pCharObj:GetVfd())
					end
				end
				
				--添加npc
				pbc_send_msg(vfds, "S2c_aoi_addnpc", {
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
					sync = self:GetSyncData()
				})	
			else
				local vfds = {}
				for _, pCharId in pairs(playerTbl) do
					local pCharObj = CHAR_MGR.GetCharById(pCharId)
					local npcList = pCharObj:GetSeeNpcNoList() or {}
					if pCharObj and not npcList[self:GetCharNo()] then		--原本可见的
						tinsert(vfds, pCharObj:GetVfd())
					end
				end
				
				--删除npc
				pbc_send_msg(vfds, "S2c_aoi_leave", {
					map_no = MAP_NO,
					map_id = MAP_ID,
					id = self:GetId(),
				})	
			end
		end
	end
end

function clsNpc:GetLimitDropHp()
	return self:GetTmp("LimitDropHp")
end
function clsNpc:SetLimitDropHp(limitDropHp)
	self:SetTmp("LimitDropHp", limitDropHp)
end

function clsNpc:GetCompClubList()
	return self:GetTmp("CompClubList")
end
function clsNpc:SetCompClubList(list)
	self:SetTmp("CompClubList", list)
	self:SetCompClubListProto(list)
end
function clsNpc:SetCompClubListProto(list)
	local l = {}
	for _clubId, _ in pairs(list) do
		tinsert(l, _clubId)
	end
	self:SetTmp("CompClubListProto", l)
end
function clsNpc:GetCompClubListProto()
	return self:GetTmp("CompClubListProto")
end

--刷新属性设置
function clsNpc:SetRefreshData(fdata)
	self:SetTmp("RefreshData", fdata)
end
function clsNpc:GetRefreshData()
	return self:GetTmp("RefreshData")
end
function clsNpc:SetCanPush(push)
	self:SetTmp("CanPush", push)
end
function clsNpc:GetCanPush()
	return self:GetTmp("CanPush")
end
function clsNpc:GetWalkGrid()
	return self:GetTmp("WalkGrid") or 1
end
function clsNpc:SetWalkGrid(wGrid)
	if wGrid > 2 then wGrid = 2 end
	self:SetTmp("WalkGrid", wGrid)
end
function clsNpc:CheckNpcToAttack(tarObj)
	local aiObj = self:GetAI()
	if aiObj and aiObj.CheckNpcToAttack then
		aiObj:CheckNpcToAttack(tarObj)
	end
end
function clsNpc:GetIsActiveAttack()
	return self:GetTmp("IsActiveAttack")
end
function clsNpc:SetIsActiveAttack(isActiveAttack)
	self:SetTmp("IsActiveAttack", isActiveAttack)
end
function clsNpc:GetActDelayTime()
	return self:GetTmp("ActDelayTime")
end
function clsNpc:SetActDelayTime(time)
	self:SetTmp("ActDelayTime", time)
end

function clsNpc:GetAIPatrol()
	return self:GetTmp("AIPatrol") or 0
end
function clsNpc:SetAIPatrol(aiPatrol)
	self:SetTmp("AIPatrol", aiPatrol)
end

function clsNpc:GetBossType()
	return self.__bossType
end
function clsNpc:SetBossType(BossType)
	self.__bossType = BossType
end

function clsNpc:GetAITrackRange()
	return self.__trange or NORMAL_NPC_RADIUS
end
function clsNpc:SetAITrackRange(range)
	self.__trange = range
end
function clsNpc:GetAIRange()
	return self.__airange
end
function clsNpc:SetAIRange(range)
	self.__airange = range
end
function clsNpc:GetHitDelNpcId()
	return self.__tmp.HitDelNpcId
end
function clsNpc:SetHitDelNpcId(npcId)
	self.__tmp.HitDelNpcId = npcId
end

function clsNpc:GetPatrolRange()
	return self.__tmp.PatrolRange or NPC_PATROLRANGE
end
function clsNpc:SetPatrolRange(range)
	self.__tmp.PatrolRange = range
end

function clsNpc:GetKReward()
	return self.__tmp.KReward
end
function clsNpc:SetKReward(KReward)
	self.__tmp.KReward = KReward
end

function clsNpc:GetAReward()
	return self.__tmp.AReward
end
function clsNpc:SetAReward(AReward)
	self.__tmp.AReward = AReward
end

function clsNpc:GetExpReward()
	return self.__tmp.ExpReward
end
function clsNpc:SetExpReward(ExpReward)
	self.__tmp.ExpReward = ExpReward
end

function clsNpc:AddHitHp(hp, attId)
	if not self.__tmp.HitHpTbl then
		self.__tmp.HitHpTbl = {}
	end
	self.__tmp.HitHpTbl[attId] = (self.__tmp.HitHpTbl[attId] or 0) + hp
end
function clsNpc:GetHitHpTbl()
	return self.__tmp.HitHpTbl
end

function clsNpc:GetRealExpReward(attId)
	local expReward = self:GetExpReward()
	if not expReward then return 0 end
	local hitHpTbl = self:GetHitHpTbl()
	if not hitHpTbl or not hitHpTbl[attId] then return 0 end
	local rate = mfloor(hitHpTbl[attId] / self:GetHpMax() * 100 / 10) * 10
	if rate <= 40 then
		rate = 30
	elseif rate >= 100 then
		rate = 100
	end
	local realExp = mfloor(expReward * rate / 100)
	return realExp
end

--跑镖护送信息
function clsNpc:GetConvoyInfo()
	return self:GetTmp("ConvoyInfo")
end
function clsNpc:SetConvoyInfo(info)
	self:SetTmp("ConvoyInfo", info)
end

function clsNpc:GetShieldBuffHpList()
	local sHpList = self:GetTmp("ShieldBuffHpList")
	if not sHpList then
		sHpList = {}
		self:SetTmp("ShieldBuffHpList", sHpList)
	end
	return sHpList
end
function clsNpc:SetShieldBuffHpList(sHpList)
	self:SetTmp("ShieldBuffHpList", sHpList)
end

local NPT_TASK = {
--	[10100101] = true,
--	[10100102] = true,
}

function clsNpc:SetHp(hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	local ohp = self:GetHp()
	
	if hp < ohp then
		if IsClient() then
			local triggerType = self:GetTmp("FubenTriggerType")
			if triggerType == FUBEN_TALK_TRIGGER_HURT then
				self:SetTmp("FubenTriggerType", nil)
				--触发对话条件
				local talkNo = self:GetTmp("FubenTalkNo")
				if talkNo then
					ShowFightTalk(talkNo, self:GetCharNo())
				end
			end
		end
	end
	
	if ohp > 0 then
		local limitDropHp = self:GetLimitDropHp()
		if limitDropHp then
			for _, _dropHp in ipairs(limitDropHp) do
				local hpMax = self:GetFightValue(mFIGHT_HpMax) or 1
				local oRate = (ohp / hpMax) * 100
				local nRate = (hp / hpMax) * 100
				
				if oRate > _dropHp and nRate <= _dropHp then
					hp = mfloor(_dropHp / 100 * hpMax)
				end
			end
		end
	end
	
	Super(clsNpc).SetHp(self, hp, attId, notSync, stype, isNotRetHp, isNotSyncHp)
	hp = self:GetHp()			--有可能前后不一致，所以需要重新获取
	if ohp > 0 then
		--只有第一层
		if self:GetMapLayer() == 1 then
			--护盾添加值
			local shieldBuffList = self:GetTmp(SHIELDBUFF_DHP)
			if shieldBuffList then
				local sHpList = self:GetShieldBuffHpList()
				for _, _data in ipairs(shieldBuffList) do
					local _hpRate = _data.hpRate
					local _buffId = _data.buffId
					
					if _hpRate and _buffId then
						local nRate = (hp / self:GetFightValue(mFIGHT_HpMax)) * 100
						if _hpRate >= nRate and not sHpList[_hpRate] then
							sHpList[_hpRate] = true
							
							FIGHT_EVENT.AddBuff(self, self, {
								id = _buffId,
								time = 10000000,
							})
							
							break
						end
					end
				end
			end
		end
	end
	
	if IsClient() then			--删除旧的宝箱
		if ohp > hp then
			local delNpcId = self:GetHitDelNpcId()
			if delNpcId then
				local delNpcObj = CHAR_MGR.GetCharById(delNpcId)
				if delNpcObj then
					if not delNpcObj:IsDestroy() then
						delNpcObj:Destroy()
					end
				end
			end
		end
	end
	
	local AttObj = CHAR_MGR.GetCharById(attId)
	if AttObj then
		if AttObj:IsPartner() or AttObj:IsMagic() then
			AttObj = AttObj:GetOwner()
		end
	end

	if ohp > hp and not isNotSyncHp then
		local hithp = ohp - hp
		if attId and attId ~= 0 then
			if AttObj and AttObj:IsPlayer() then
				self:AddHitHp(hithp, AttObj:GetId())
			end
		end
	end
	
	if ohp > 0 and hp <= 0 then
		if IsServer() then
			NPC_AUTOCREATE.AddNpcRefresh(self)
		else
			if self:GetBossType() == BOSS_TYPE_SHUIJING then			--塔防挂掉
				CHECKEND.SetTaFangEnd()
			end
		end
		
		local kReward = self:GetKReward()
		local aReward = self:GetAReward()
		local expReward = self:GetExpReward() or 0
		local tAttId = attId
		if AttObj and AttObj:IsPlayer() then
			tAttId = AttObj:GetId()
		end
		
		if kReward or aReward or expReward then
			local isReward = false
			if kReward or aReward then
				isReward = true
				if AttObj then
					if IsServer() then
						lretmap.other(AttObj:GetId(), MAP_ID, AttObj:GetMapLayer(), lserialize.lua_seri_str({
							type = RETMAP_TYPE_NPCDIE_REWARD,
							kReward = kReward,
							aReward = aReward,
							expReward = self:GetRealExpReward(AttObj:GetId()),
							pos = {self:GetX(), self:GetY()},
							npcNo = self:GetCharNo(),
							grade = self:GetGrade() or 1,
						}))
					else
						lretmap.other({
							type = RETMAP_TYPE_NPCDIE_REWARD,
							kreward = {
								npc_no = self:GetCharNo() or 0,
								npc_pos = {self:GetX(), self:GetY()},
							},
						})
					end
				end
			end
			if expReward then
				local hitHpTbl = self:GetHitHpTbl() or {}	
				for _id, _hitHp in pairs(hitHpTbl) do
					local getExp = true
					if _id == tAttId and isReward then
						getExp = false
					end 
					if getExp then
						local AttObj = CHAR_MGR.GetCharById(_id)
						if AttObj then
							if IsServer() then
								lretmap.other(AttObj:GetId(), MAP_ID, AttObj:GetMapLayer(), lserialize.lua_seri_str({
									type = RETMAP_TYPE_NPCDIE_REWARD,
									expReward = self:GetRealExpReward(AttObj:GetId()),
									pos = {self:GetX(), self:GetY()},
									npcNo = self:GetCharNo(),
									grade = self:GetGrade() or 1,
								}))		
							else
								lretmap.other({
									type = RETMAP_TYPE_NPCDIE_REWARD,
									kreward = {
										npc_no = self:GetCharNo() or 0,
										npc_pos = {self:GetX(), self:GetY()},
									},
								})							
							end					
						end
					end
				end		
			end
		else
			if IsServer() and AttObj and AttObj:IsPlayer() then
				lretmap.other(AttObj:GetId(), MAP_ID, AttObj:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_TYPE_NPCDIE_REWARD,
					npcNo = self:GetCharNo(),
					grade = self:GetGrade() or 1,
				}))			
			end		
		end
		
		if IsServer() and AttObj and AttObj:IsPlayer() then
			lretmap.other(AttObj:GetId(), MAP_ID, AttObj:GetMapLayer(), lserialize.lua_seri_str({
				type = RETMAP_TYPE_NPCDIE_TASK,
				npcNo = self:GetCharNo(),
				atkList = self:GetHitHpTbl(),
			}))
		end	
		
		--把技能提示清空
		local allSkill = self:GetAllSkill()
		for _skillId, _data in pairs(allSkill) do
			if _data.TipsId then
				FIGHT.DelSkillTips(self, _skillId)
			end
		end
	else
		if self:GetTmp("IsHurtFinish") then
--		if NPT_TASK[self:GetCharNo()] then
			if IsServer() and AttObj and AttObj:IsPlayer() then
				lretmap.other(AttObj:GetId(), MAP_ID, AttObj:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_TYPE_NPCDIE_TASK,
					npcNo = self:GetCharNo(),
					atkList = self:GetHitHpTbl(),
				}))
			end	
		end
	end
end

function clsNpc:SetDir(dir)
	Super(clsNpc).SetDir(self, dir)
	local syncData = self:GetSyncData()
	syncData.dir = dir
end

function clsNpc:IsNowTipsSkillId()
	local nSkillId = self.__nskillid
	if nSkillId then
		local allSkill = self:GetAllSkill()
		local skillData = allSkill[nSkillId]
		return nSkillId, skillData.Tips
	end
end

function clsNpc:SetWorldBossSkillList()
	local wSkillList = {}
	local allSkill = self:GetAllSkill()
	for _skillId, _data in pairs(allSkill) do
		if _data.Type == SKILL_TYPE_INITIATIVE then
			tinsert(wSkillList, _skillId)
		end
	end
	table.sort(wSkillList)
	self.__wsList = wSkillList
end

function clsNpc:GetWorldBossSkillList()
	return self.__wsList or {}
end

function clsNpc:GetNowSkillId(tarCharObj)
--	if self:GetBossType() == BOSS_TYPE_CLUBWAR then
--		local wSkillList = self:GetWorldBossSkillList()
--		local nSkillNum = self:GetTmp("WBSkillNum") or 0
--		nSkillNum = nSkillNum + 1
--		if not wSkillList[nSkillNum] then
--			nSkillNum = 1
--		end
--		self:SetTmp("WBSkillNum", nSkillNum)
--		local nSkillId = wSkillList[nSkillNum]
--		if nSkillId then
--			local allSkill = self:GetAllSkill()
--			local skillData = allSkill[nSkillId]
--			if skillData.Tips then
--				self.__nskillid = nSkillId
--				return nSkillId, skillData.Tips
--			else
--				return nSkillId
--			end
--		end 
--	else
		local nSkillId = self.__nskillid
		local allSkill = self:GetAllSkill()
		if nSkillId then
			local skillData = allSkill[nSkillId]
			if skillData.Tips then
				return nSkillId, skillData.Tips, skillData.IsWait, skillData.x, skillData.y
			else
				return nSkillId
			end
		else
			local nTimeNo = GetNowTimeNo()
			
			--优先选择分段技能
			local frontSkillId = self:GetFrontSkillId()
			if frontSkillId then
				for _skillId, _data in pairs(allSkill) do		
					if _data.Type == SKILL_TYPE_INITIATIVE then
						if _data.FrontSkill == frontSkillId then
							local timeNoCnt = _data.CD
							local eTimeNo = _data.CDEndTimeNo
							if nTimeNo >= eTimeNo then
								_data.CDEndTimeNo = nTimeNo + timeNoCnt					--之后真正释放的时候再重新设置,因为这个可能是预先提示的
								self.fsSkill = self.fsSkill + 1
								return _skillId
							end
						end
					end
				end
			end
			
			if self.fsSkill >= 2 then
				--技能优先
				for _skillId, _data in pairs(allSkill) do
					if _data.Type == SKILL_TYPE_INITIATIVE and not _data.FrontSkill then
						if _data.Mtype == SKILL_MTYPE_MAGIC then
							local timeNoCnt = _data.CD
							local eTimeNo = _data.CDEndTimeNo
							local cCoolTime = _data.CreateCoolTime
							if not cCoolTime or nTimeNo > cCoolTime then
								if nTimeNo >= eTimeNo then
									_data.CDEndTimeNo = nTimeNo + timeNoCnt					--之后真正释放的时候再重新设置,因为这个可能是预先提示的
									if _data.Tips then		--需要提前提示
										self.__nskillid = _skillId		--只有提前提示才设置
										_data.IsWait = true
										_data.x = tarCharObj:GetX()
										_data.y = tarCharObj:GetY()
										if not FIGHT.NpcCheckTarget(self, tarCharObj, _data.AttKind, _data.AttKind2) then									
											local tarObj = FIGHT.GetMainTarget(self, _data.AttKind, _data.AttKind2, _data.AttRange)
											if tarObj then
												_data.x = tarObj:GetX()
												_data.y = tarObj:GetY()
											end
										end
									end
									return _skillId, _data.Tips, _data.IsWait, _data.x, _data.y
								end
							end
						end
					end
				end
			end
			--普通攻击
			for _skillId, _data in pairs(allSkill) do
				if _data.Type == SKILL_TYPE_INITIATIVE and not _data.FrontSkill then
					if _data.Mtype == SKILL_MTYPE_NORMAL then
						self.fsSkill = self.fsSkill + 1
						return _skillId
					end
				end
			end	
		end
--	end
end

--设置成已经等待了
function clsNpc:ClearSkillIdWait(skillId)
	local allSkill = self:GetAllSkill()
	local skillData = allSkill[skillId]
	if not skillData then return end
	skillData.IsWait = nil
end

function clsNpc:SetNowSkillId(skillId)
end

function clsNpc:GetComp()
	return Super(clsNpc).GetComp(self) or BATTLE_CAMP_2
end

function clsNpc:GetSpeed()
	return Super(clsNpc).GetSpeed(self) or SPEED_NORMAL
end

function clsNpc:IsNpc()
	return true
end

function clsNpc:AddFixedShieldBuffSync(sHp, sHpMax, sBuffId)
	local syncData = Super(clsNpc).GetSyncData(self)
	syncData.shield_hp = sHp
	syncData.shield_hpmax = sHpMax
	syncData.shield_buffid = sBuffId
	Super(clsNpc).SetSyncData(self, syncData)
	
	if IsKuaFuServer and self:GetRetHp() then
		local mapLayer = self:GetMapLayer()
		if mapLayer == 1 then
			lretmap.other(self:GetId(), MAP_ID, mapLayer, lserialize.lua_seri_str({		--发送护盾buff创建
				type = RETMAP_SHIELDBUFF,
				iType = "create",
				sHp = sHp,
				sHpMax = sHpMax,
			}))
		end
	end
end

function clsNpc:SetName(name, notSync)
	Super(clsNpc).SetName(self, name, notSync)
	if not notSync then
		self:SyncNearByPlayer("S2c_aoi_sync_string", {
			fid = self:GetFId(),
			key = "name",
			value = name,
		})
	end
end

local EMPTY_TBL = {}
function clsNpc:GetSyncData()
	local syncData = Super(clsNpc).GetSyncData(self) or {}
	local sHp, sHpMax, sBuffId = self:FixedShieldBuffSyncHp()
	syncData.shield_hp = sHp
	syncData.shield_hpmax = sHpMax
	syncData.shield_buffid = sBuffId
	syncData.clublist = self:GetCompClubListProto() or EMPTY_TBL
	return syncData
end

function clsNpc:SetSyncData(value)
	if self:IsStaticNpc() then
		value.canattk = 0
	else
		value.canattk = 1
	end
	if value then		
		if value.name then			--需要更名
			self:SetName(value.name)
		end
	end
	Super(clsNpc).SetSyncData(self, value)
end

function clsNpc:SetExtend(value)
	local syncData = self:GetSyncData()
	local oExtend = syncData.extend
	syncData.extend = value
	if oExtend ~= syncData.extend then
--		Super(clsNpc).SetSyncData(self, syncData)
		self:SyncNearByPlayer("S2c_aoi_sync_string", {
			fid = self:GetFId(),
			key = "extend",
			value = value or "",
		})
	end
end

function clsNpc:AddMap()			--添加进入地图后的处理,必须重载,不然报错(进入地图的时候可以npc,user,partner重合站一起,不然处理比较麻烦)
	local isOk, playerTbl = laoi.map_addobj(self:GetMapObj(), self:GetEngineObj())
	assert(isOk, "npc add map error: " .. self:GetName() .. " x,y" .. self:GetX()..","..self:GetY())
	if isOk then
		self:MoveChangeMapPos(CHANGE_MAPPOS_ADD)
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_addnpc", {
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
					sync = self:GetSyncData()
				})		
			end		
		end
	end
end

function clsNpc:SendPushPullPos(ox, oy, oz, retnum, apTbl, dpTbl, speed)
	local selfId = self:GetId()
	local selfFId = self:GetFId()
	local nx, ny, nz = self:GetX(), self:GetY(), self:GetZ()
	speed = speed or self:GetFightValue(mFIGHT_Speed)
	
	if retnum == 0 then
		local vfds = {}
		if apTbl then
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
					tinsert(vfds, pCharObj:GetVfd())		
				end
			end
		end
		if #vfds > 0 then
			pbc_send_msg(vfds, "S2c_aoi_move", {											
				fid = selfFId,
				x = nx,
				z = ny,
				y = nz,
				speed = speed,
			})
		end
	elseif retnum == 1 then
		if apTbl then
			--移动的npc的坐标告诉别人进入他们的视野使用旧的pos的(即先把npc的旧坐标发给addplayer,然后最后move一次发送新坐标【里面也包含了addplayer】)
			local vfds = {}
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
					tinsert(vfds, pCharObj:GetVfd())			
				end
			end
			--把自己移动前的坐标告诉别人
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_addnpc", {
					nmsg = {
						fid = self:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = selfId,
						x = ox,
						z = oy,
						y = oz,
						hp = self:GetHp(),
						hpmax = self:GetFightValue(mFIGHT_HpMax),
						char_no = self:GetCharNo(),
					},
					sync = self:GetSyncData(),
							
				})		
			end				
		end
		
		if dpTbl then
			local vfds = {}
			for _, pCharId in pairs(dpTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
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
	end
end

function clsNpc:SendMove(ox, oy, oz, retnum, isJump, apTbl, dpTbl, mpTbl, speed)
	local selfId = self:GetId()
	local selfFId = self:GetFId()
	local nx, ny, nz = self:GetX(), self:GetY(), self:GetZ()
	speed = speed or self:GetFightValue(mFIGHT_Speed)
	
	if retnum == 0 then
		local vfds = {}
		if apTbl then
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
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
			else
				pbc_send_msg(vfds, "S2c_aoi_move", {											
					fid = selfFId,
					x = nx,
					z = ny,
					y = nz,
					speed = speed,
				})
			end
		end
	elseif retnum == 1 then
		if apTbl then
			--移动的npc的坐标告诉别人进入他们的视野使用旧的pos的(即先把npc的旧坐标发给addplayer,然后最后move一次发送新坐标【里面也包含了addplayer】)
			local vfds = {}
			for _, pCharId in pairs(apTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
					tinsert(vfds, pCharObj:GetVfd())			
				end
			end
			--把自己移动前的坐标告诉别人
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_addnpc", {
					nmsg = {
						fid = self:GetFId(),
						map_no = MAP_NO,
						map_id = MAP_ID,
						rid = selfId,
						x = ox,
						z = oy,
						y = oz,
						hp = self:GetHp(),
						hpmax = self:GetFightValue(mFIGHT_HpMax),
						char_no = self:GetCharNo(),
					},
					sync = self:GetSyncData(),
					
				})		
			end				
		end
		
		if dpTbl then
			local vfds = {}
			for _, pCharId in pairs(dpTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
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
		
		if mpTbl then
			local vfds = {}
			for _, pCharId in pairs(mpTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj and self:GetCanSee(pCharObj) then
					tinsert(vfds, pCharObj:GetVfd())		
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
				else
					pbc_send_msg(vfds, "S2c_aoi_move", {
						fid = selfFId,
						x = nx, 
						z = ny,
						y = nz,
						speed = speed,
					})	
				end
			end			
		end
	end
end

function clsNpc:Move(speed, dir)	--添加移动后的处理,必须重载,不然报错(需要考虑ox,oy到nx,ny是否有npc,玩家,同伴,不然不能移动到那里,没有写完)
end

--只能是测试一格
function clsNpc:CanMoveToOneGrid(ox, oy, nx, ny)
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
function clsNpc:MoveTo(nx, ny, nz, speed, notSlope)	--添加移动后的处理,必须重载,不然报错(只需要考虑nx,ny是否有npc,玩家,同伴,不然不能移动到那里,没有写完)
	if not self:CanMove() then return end
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then return end
	end
	local ox, oy, oz = self:GetX(), self:GetY(), self:GetZ()
	if not notSlope then
		if not BASECHAR.CanMoveBySlope(ox, oy, nx, ny) then return end			--有坡度
	end
	if self:IsBlockPoint(nx, ny, nz) or self:HasObjNotItemInPos(nx, ny, nz) then
		return
	end
	local retnum, apTbl, dpTbl, mpTbl = laoi.map_moveobj(self:GetMapObj(), self:GetEngineObj(), nx, ny)
	assert(retnum, "clsNpc:MoveTo error npc:" .. self:GetName())
	
	if retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		self:SetX(nx)
		self:SetY(ny)
		self:SetZ(nz)
		self:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		
		speed = self:GetFightValue(mFIGHT_Speed)
--		if IsServer() then	
			local tLen = mabs(ox - nx) ^ 2 + mabs(oy - ny) ^ 2
			local func = DISTANCE_ADJUST_SPEED[tLen]
			if func then
				speed = func(speed)
			end
--		end
		
		self:SendMove(ox, oy, oz, retnum, false, apTbl, dpTbl, mpTbl, speed)
		
		if self:GetBossType() == BOSS_TYPE_WALKSHOW then
			lretmap.other(self:GetId(), MAP_ID, self:GetMapLayer(), lserialize.lua_seri_str({
					type = RETMAP_SYN_CAB_POS,
					x = nx,
					y = ny,
					z = nz,
				}))
		end
		
		local dir = BASECHAR.GetDirByPos(ox, oy, nx, ny)
		if dir then self:SetDir(dir) end
	end
	
--	_RUNTIME("clsNpc:MoveTo 2:", retnum, 
--		sys.dump(apTbl),
--		sys.dump(dpTbl),
--		sys.dump(mpTbl), "oldxy:["..ox..","..oy.."]", "newxy["..nx..","..ny.."]"
--		)
	if retnum >= 0 then
		return true
	end
end

--返回true/nil
function clsNpc:JumpTo(nx, ny, nz)	--添加移动后的处理,必须重载,不然报错(需要考虑nx,ny是否有npc,玩家,同伴,不然不能移动到那里,没有写完)
	if not self:CanMove() then return end
	if not nz then
		nz = lmapdata.getz(MAP_NO, nx, ny)
		if not nz then return end
	end
	local ox, oy, oz = self:GetX(), self:GetY(), self:GetZ()
	
	if self:IsBlockPoint(nx, ny, nz) or self:HasObjNotItemInPos(nx, ny, nz) then
		return
	end
	
	local retnum, apTbl, dpTbl, mpTbl = laoi.map_moveobj(self:GetMapObj(), self:GetEngineObj(), nx, ny)
	assert(retnum, "clsNpc:JumpTo error npc:" .. self:GetName())
	
	if retnum >= 0 then		--0移动后还在同一格子,1移动后不同格子
		self:SetX(nx)
		self:SetY(ny)
		self:SetZ(nz)
		self:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
		self:SendMove(ox, oy, oz, retnum, true, apTbl, dpTbl, mpTbl)
	end
	
--	_RUNTIME("clsNpc:JumpTo 2:", retnum, 
--		sys.dump(apTbl),
--		sys.dump(dpTbl),
--		sys.dump(mpTbl), "oldxy:["..ox..","..oy.."]", "newxy["..nx..","..ny.."]"
--		)
	if retnum >= 0 then
		return true
	end
end

function clsNpc:LeaveMap(notRetMap)			--添加离开地图后的处理,必须重载,不然报错
	local isOk, playerTbl = laoi.map_removeobj(self:GetMapObj(), self:GetEngineObj())
--	_RUNTIME("leave 1:", isOk, self:GetMapObj(), self:GetEngineObj())	
	assert(isOk, "clsNpc:LeaveMap error: " .. self:GetName())
	self:MoveChangeMapPos(CHANGE_MAPPOS_LEAVE)
	
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
	
	if not notRetMap and self:GetMainNpc() then
		lretmap.static_npcleave(self:GetId(), MAP_ID, self:GetMapLayer())
	end	
end