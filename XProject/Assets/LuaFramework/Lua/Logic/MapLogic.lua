local GAMECONST = GAMECONST
local mapData = require "xlsdata/Map/MapDataXls"

local MapLogic = {}
local this = MapLogic

------------------------------------------------------------------
--飞鞋逻辑参数
this.FLY_SHOE_ITEMNO = 10105003

local FLY_SHOE_DISTANCE = 20
local Y_OFFSET = 6
local targetInfo
local cantFlyCallBack		--不能飞回调

local midPointWalk

function MapLogic.SendFlyShoe(callback)
	local HERO = HERO
	local mainRole = roleMgr.mainRole
	if HERO.midPointWalk then
		if not midPointWalk then return end

		local sameScene = roleMgr.curSceneNo == midPointWalk.sceneNo
		if sameScene then
			local realPos = Util.Convert2MapPosition(midPointWalk.pos.x, midPointWalk.pos.z, midPointWalk.pos.y)
			--print("=----1-----", mainRole.transform.position.y, realPos.y, FLY_SHOE_DISTANCE)
			if math.pow(mainRole.transform.position.x - realPos.x, 2) + math.pow(mainRole.transform.position.z - realPos.z, 2)
			 < math.pow(FLY_SHOE_DISTANCE, 2) then
				local LANGUAGE_TIP = LANGUAGE_TIP
				CtrlManager.PopUpNotifyText(LANGUAGE_TIP.FlyShoeTip)
				return
			end
		end

		if not this.JudgeLevelCanJump(midPointWalk.sceneNo) then
			return
		end

		local cmd = {}
		cmd.map_no = midPointWalk.sceneNo
		cmd.x = midPointWalk.pos.x
		cmd.z = midPointWalk.pos.z

		Network.send("C2s_hero_canflyshoe", cmd)

		cantFlyCallBack = function()
			targetInfo = midPointWalk
			HERO.SetMidPointWalk(false)
			midPointWalk = nil
			if callback then
				callback()
			end
		end
	else
		local sameScene = not worldPathfinding.targetInfo.toMapId or worldPathfinding.targetInfo.toMapId == 0
		--print("=-----2----", mainRole.transform.position.y, mainRole.move.finalDestination.y, FLY_SHOE_DISTANCE)
		if sameScene
			and math.pow(mainRole.transform.position.x - mainRole.move.finalDestination.x, 2) + math.pow(mainRole.transform.position.z - mainRole.move.finalDestination.z, 2)
			 < math.pow(FLY_SHOE_DISTANCE, 2)  then
			local LANGUAGE_TIP = LANGUAGE_TIP
			CtrlManager.PopUpNotifyText(LANGUAGE_TIP.FlyShoeTip)
			return
		end

		local cmd = {}
		cmd.map_no = sameScene and roleMgr.curSceneNo or worldPathfinding.targetInfo.toMapId 

		if not this.JudgeLevelCanJump(cmd.map_no) then
			return
		end

		targetInfo = {}
		targetInfo.sceneNo = cmd.map_no
		targetInfo.radius = sameScene and mainRole.move.endReachedDistance or worldPathfinding.targetInfo.radius
		targetInfo.callback = sameScene and mainRole.move.pathFinished or worldPathfinding.targetInfo.pathFinished
		targetInfo.pos = sameScene 
			and Util.Convert2MapPosition(mainRole.move.finalDestination.x
			, mainRole.move.finalDestination.z
			, mainRole.move.finalDestination.y)
			or worldPathfinding.targetInfo.destination
		cmd.x = targetInfo.pos.x
		cmd.z = targetInfo.pos.z
		
		--print("--------11111111-------", targetInfo.callback)
		-- print("-------SendFlyShoe---------", TableToString(cmd))
		Network.send("C2s_hero_canflyshoe", cmd)

		cantFlyCallBack = callback		
	end
end

---------------------------------------------------------------------
--起跳
function MapLogic.FlyUpAction()
	local mainRole = roleMgr.mainRole
	if not mainRole then 
		return 
	end

	--弹出coverd
	if not CtrlManager.PanelIsPopuped("CoverUi") then
		CtrlManager.PopUpPanel("CoverUi")
	end

	--跳跃动作，停止摄像机移动
	mainRole.roleState:AddState(GAMECONST.RoleState.FlyShoe)
	if mainRole.roleState:IsInState(GAMECONST.RoleState.AutoPathfinding) then
		mainRole.roleState:RemoveState(GAMECONST.RoleState.AutoPathfinding)
	end
	worldPathfinding:StopWorldPathfinding()
	mainRole.move:StopPath()
	mainRole.roleAction:ResetToIdleImmediate()
	mainRole.move:StopJump()

	mainRole.roleAction:SetRoleJump(1)
	--mainRole.move:Jump(10, 0, 0, 0, 0)
	mainRole.transform:DOMoveY(mainRole.transform.position.y + Y_OFFSET, 1, false
		):SetEase(DG.Tweening.Ease.OutSine)
end

function MapLogic.FlyDownAction(callback)
	local mainRole = roleMgr.mainRole
	if not mainRole then 
		return 
	end

	--弹出cover
	if not CtrlManager.PanelIsPopuped("CoverUi") then
		CtrlManager.PopUpPanel("CoverUi")
	end

	worldPathfinding:StopWorldPathfinding()
	mainRole.move:StopPath()
	mainRole.roleAction:ResetToIdleImmediate()
	mainRole.roleState:AddState(GAMECONST.RoleState.FlyShoe)
	
	if mainRole.hp == 0 then
		mainRole.roleAction:SetActionStatus(15);
	end
	--下落
	local originY = mainRole.transform.position.y
	mainRole.transform.position = Vector3.New(mainRole.transform.position.x
		, originY + Y_OFFSET
		, mainRole.transform.position.z)
	mainRole.roleAction:SetRoleJump(1)

	local co
	co = coroutine.start(function()
				coroutine.wait(0.3)
				roleMgr.mainRole.roleAction:SetRoleFall(3)

				roleMgr.mainRole.transform:DOMoveY(originY, 0.95, false
					):SetEase(DG.Tweening.Ease.OutSine
					):OnComplete(
						function()
							--print("--------------ResetToIdleImmediate----------")
							roleMgr.mainRole.roleAction:ResetToIdleImmediate()
						end)

				if callback then
					callback()
				end
				coroutine.stop(co)
			end)
end

---------------------------------------------------------------------

function MapLogic.StartFlyShow(canfly)
	if canfly ~= 1 then
		return
	end

	this.FlyUpAction()

	local cmd = {}
	cmd.map_no = targetInfo.sceneNo
	cmd.x = targetInfo.pos.x
	cmd.z = targetInfo.pos.z

	local co
	co = coroutine.start(function()
				coroutine.wait(1)
				Network.send("C2s_hero_flyshoe", cmd)
				coroutine.stop(co)
			end)
end

function MapLogic.EnterSceneEndFlyShow()
	--print("=---------------", targetInfo, roleMgr.mainRole)
	if not targetInfo then
		--this.ClearFlyShoeInfo()
		return 
	end

	this.FlyDownAction(function()
			local co2
			co2 = coroutine.start(function()
							coroutine.wait(1)
							this.FlyShowOverToWalk()
							coroutine.stop(co2)
						end)
		end)
end

function MapLogic.FlyShowOverToWalk()
	local mainRole = roleMgr.mainRole
	mainRole.roleState:RemoveState(GAMECONST.RoleState.FlyShoe)

	if targetInfo and targetInfo.callback then
		worldPathfinding:DoCallBack(targetInfo.callback)
	end

	this.ClearFlyShoeInfo()
end

function MapLogic.ClearFlyShoeInfo()
	if CtrlManager.PanelIsPopuped("CoverUi") then
		CtrlManager.ClosePanel("CoverUi")
	end

	targetInfo = nil
end

--直接飞向任一可行点（pos为格子坐标）,不判断距离，直接
function MapLogic.FlyToAnyWhere(mapno, pos, radius, callback, canFlyBack)
	local FuncOpenLogic = require("Logic/FuncOpenLogic")
	local FUNCOPENDDATA = FUNCOPENDDATA
	if not FuncOpenLogic.PopUnOpenTip(FUNCOPENDDATA.FUNC_INDEX.FLY_SHOE) then
		if cantFlyCallBack then
			cantFlyCallBack(0)
		end
		return
	end

	if not this.JudgeLevelCanJump(mapno) then
		return
	end
	 --是否在押镖
    local husong = RoleStateTransition.RoleState.Husong:ToInt()
    if roleMgr.mainRole.roleState:IsInState(husong) then
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.roleProjectFollow)
        return
    end

    if MarriageLogic.MarriageCheck() then
        return
    end
    
	local VipXls = require('xlsdata/VipXls')
	if ITEMLOGIC.GetItemCountByNo(10105003) <= 0 and not VipXls[HERO.Vip].FlyShoe then
		CtrlManager.PopUpNotifyText(LANGUAGE_TIP.fieldBoss0014)  
		local BuyLogic = require('Logic/BuyLogic')
		local ItemXls = require("xlsdata/Item/ItemXls")
 		BuyLogic.BuyItem(10105003, 1, nil, LANGUAGE_TIP.fieldBoss0015, LANGUAGE_TIP.fieldBoss0016, ItemXls[10105003].GetPath, true)
		return 
	end

	targetInfo = {}
	targetInfo.sceneNo = mapno
	targetInfo.pos = pos
	targetInfo.radius = radius
	targetInfo.callback = callback

	local cmd = {}
	cmd.map_no = targetInfo.sceneNo
	cmd.x = targetInfo.pos.x
	cmd.z = targetInfo.pos.z

	Network.send("C2s_hero_canflyshoe", cmd)

	cantFlyCallBack = canFlyBack

	if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.DigTreasure) then
		roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.DigTreasure)
	end
	if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
		roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
	end
	if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.ProtectFollow) then
		roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.ProtectFollow)
	end
end
--仇人专用飞鞋
function MapLogic.EnemyFlayShoe(mapno, mapid, x,y,enemyId)
	local FuncOpenLogic = require("Logic/FuncOpenLogic")
	local FUNCOPENDDATA = FUNCOPENDDATA
	if not FuncOpenLogic.PopUnOpenTip(FUNCOPENDDATA.FUNC_INDEX.FLY_SHOE) then
		if cantFlyCallBack then
			cantFlyCallBack(0)
		end
		return
	end

	if not this.JudgeLevelCanJump(mapno) then
		return
	end
	 --是否在押镖
    local husong = RoleStateTransition.RoleState.Husong:ToInt()
    if roleMgr.mainRole.roleState:IsInState(husong) then
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.roleProjectFollow)
        return
    end

    if MarriageLogic.MarriageCheck() then
        return
    end
    
	local VipXls = require('xlsdata/VipXls')
	if ITEMLOGIC.GetItemCountByNo(10105003) <= 0 and not VipXls[HERO.Vip].FlyShoe then
		CtrlManager.PopUpNotifyText(LANGUAGE_TIP.fieldBoss0014)  
		local BuyLogic = require('Logic/BuyLogic')
		local ItemXls = require("xlsdata/Item/ItemXls")
 		BuyLogic.BuyItem(10105003, 1, nil, LANGUAGE_TIP.fieldBoss0015, LANGUAGE_TIP.fieldBoss0016, ItemXls[10105003].GetPath, true)
		return 
	end

	local cmd = {}
	cmd.uid = enemyId
	cmd.mapno = mapno
	cmd.mapid = mapid
	cmd.x = x
	cmd.y = y

	Network.send("C2s_friend_enemy_go", cmd)
end

function MapLogic.DealFlyCallBack(canfly)
	if cantFlyCallBack then
		cantFlyCallBack(canfly)
	end
end
---------------------------------------------------------------
--处理虚假传送
function MapLogic.DealFakeJump(extend)
	if not extend or #extend == 0 then return end

	local strArr = string.split(extend, ";")
	local gateInfo = {}
	for k, v in pairs(strArr) do
		local arr = string.split(v, "=")
		gateInfo[arr[1]] = tonumber(arr[2])
	end

	if gateInfo["WorldBoss"] and gateInfo["WorldBoss"] > 0 then
		if gateInfo["EnterType"] == 1 then
			Network.send("C2s_kworldboss_join", {no = gateInfo["WorldBoss"]})
		else
			Network.send("C2s_kworldboss_leave", {place_holder = gateInfo["WorldBoss"]})
		end
	elseif gateInfo["BPGCZ"] and gateInfo["BPGCZ"] > 0 then
		Network.send("C2s_ksiegewar_transform_door", {to_fightgroup = gateInfo["BPGCZ"]})
	elseif gateInfo["BackGate"] and gateInfo["BackGate"] > 0 then
		Network.send("C2s_ksiegewar_recity", {gate_no = gateInfo["BackGate"]})
	end
end

-----------------------------------------------------------------
--地图传送点等级预判断
function MapLogic.JudgeLevelCanJump(tomap)
	if not mapData[tomap] then
		logError("未知的地图信息 " .. tomap)
		return false
	end

	local HERO = HERO
	if HERO.Grade >= mapData[tomap].Level then
		return true
	else
		local LANGUAGE_TIP = LANGUAGE_TIP
		CtrlManager.PopUpNotifyText(string.format(LANGUAGE_TIP.JumpGateTip, mapData[tomap].Level))
		return false
	end
end

-----------------------------------------------------------------
--活动传送距离判断
function MapLogic.JudgeActNeedFly(mapno, x, y, z)
	if not roleMgr.mainRole then
		return false
	end

	if mapno == roleMgr.curSceneNo then
		local realPos = Util.Convert2RealPosition(x, z)
		--print("---------------------", x, y, z, realPos.x, realPos.y, realPos.z)
		if realPos == Vector3.zero then 
			return false
		end

		if Vector3.Distance(roleMgr.mainRole.transform.position, realPos) < FLY_SHOE_DISTANCE then	
			return false
		end
	end

	return true
end

-----------------------------------------------------------------
--多点寻路处理
--[[
@mapCopyId : 地图分线id
]]
function MapLogic.MultPointWalk(sceneNo, x, y, z, radius, callback, pathPos,mapCopyId)
	local canMove = false
	if pathPos then
		local pos = Util.Convert2RealPosition(pathPos.x, pathPos.z)
		canMove = worldPathfinding:BeginWorldPathfinding(pathPos.sceneNo, pathPos.x, pos.y, pathPos.z, 0.1, function()
       		canMove = worldPathfinding:BeginWorldPathfinding(sceneNo, x, y, z, radius, callback, false, mapCopyId or 0)
    	end, true,mapCopyId or 0)

    	midPointWalk = {}
		midPointWalk.sceneNo = sceneNo
		midPointWalk.pos = Vector3.New(x, y, z)
		midPointWalk.radius = radius
		midPointWalk.callback = callback
    else
	    canMove = worldPathfinding:BeginWorldPathfinding(sceneNo, x, y, z, radius, callback, false,mapCopyId or 0)
	end
	return canMove
end

-----------------------------------------------------------------
--场景安全区处理
--进入杀戮场景，添加计时器，退出安全场景，删除计时器
local TIME_INTERVAL = 3
local lastTime = 0

function MapLogic.ShowSafeSceneTip()
	-- print("----------ShowSafeSceneTip------------")
    if not roleMgr.mainRole then return end

    local map = mapData[roleMgr.curSceneNo]
    if not map then
        logError("地图数据错误")
        return
    end

    local LANGUAGE_TIP = LANGUAGE_TIP
    if map.FightType == 2 and (not map.DiduiScene or map.DiduiScene == 0) then
        HERO.inSafeArea = 1
        --CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip3)
    else
        --CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip2)

        local inSafe = false
        local pos = Util.Convert2MapPosition(roleMgr.mainRole.transform.position.x
            , roleMgr.mainRole.transform.position.z
            , roleMgr.mainRole.transform.position.y)
        if map.SecurityAreas then
            for k, v in pairs(map.SecurityAreas) do
                if (math.pow(pos.x-v.pos[1], 2) + math.pow(pos.z-v.pos[2], 2)) <= math.pow(v.r, 2) then
                    inSafe = true
                    break
                end
            end
        end

        if map.FightType == 2 then
        	 HERO.inSafeArea = inSafe and 3 or 2
		else
			HERO.inSafeArea = inSafe and 3 or 4
			if inSafe then
	            CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip0)
	        else
	            CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip1)
	        end
	    end

        --添加每隔一段时间刷新血条
        lastTime = Time.time
        this.CreateTimer()
    end

    -- print("----------ShowSafeSceneTip------------", HERO.inSafeArea)
end 

function MapLogic.ShowSafeAreaTip(x, z)
	-- print("----------ShowSafeAreaTip------------")
    local map = mapData[roleMgr.curSceneNo]
    if not map then
        logError("地图数据错误")
        return
    end

    -- if map.FightType == 2 then
    --     return
    -- end

    local inSafe = 4
    if map.FightType == 2 then
    	inSafe = (map.DiduiScene and map.DiduiScene > 0) and 2 or 1
    end

    if map.SecurityAreas then
        for k, v in pairs(map.SecurityAreas) do
            if (math.pow(x-v.pos[1], 2) + math.pow(z-v.pos[2], 2)) <= math.pow(v.r, 2) then
                inSafe = 3
                break
            end
        end
    end

    if HERO.inSafeArea == 0 then
    	if inSafe ~= HERO.inSafeArea then
	        roleMgr:UpdateAllTitleBlood()
	    end

	    HERO.inSafeArea = inSafe
    	return
    end

    --比较inSafeArea是否是nil或者false
    local LANGUAGE_TIP = LANGUAGE_TIP
    if inSafe == 3 and HERO.inSafeArea ~= 3 then
    	 CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip0)
   	elseif HERO.inSafeArea == 3 then
   		if inSafe == 4 then
   			CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip1)
   		elseif inSafe == 1 or inSafe == 2 then
   			CtrlManager.PopUpNotifyText(LANGUAGE_TIP.EnterSafeAreaTip3)
   		end
   	end

    if inSafe ~= HERO.inSafeArea then
        roleMgr:UpdateAllTitleBlood()
    end

    HERO.inSafeArea = inSafe
end

--判断是在安全区域 1安全场景不可杀人 2安全场景可敌对 3安全区域 4杀戮区域 5安全场景可仇杀 6安全场景可仇杀可敌对 7杀戮区域可仇杀
function MapLogic.ChargeIsInSafeArea(x, z)
    local map = mapData[roleMgr.curSceneNo]
    if not map then
        logError("地图数据错误")
        return
    end

    if map.SecurityAreas then
        for k, v in pairs(map.SecurityAreas) do
            if (math.pow(x-v.pos[1], 2) + math.pow(z-v.pos[2], 2)) <= math.pow(v.r, 2) then
                return 3
            end
        end
    end

    if map.FightType == 2 then
    	if map.DiduiScene and map.DiduiScene > 0 and map.Enemy and map.Enemy > 0 then
    		return 6
    	elseif map.DiduiScene and map.DiduiScene > 0 then
    		return 2
    	elseif map.Enemy and map.Enemy > 0 then
    		return 5
    	else
    		return 1
    	end
        -- return (map.DiduiScene and map.DiduiScene > 0) and 2 or 1
    else
    	if map.Enemy and map.Enemy > 0  then
    		return 7
    	end
    end

    return 4
end

-----------------------------------------------------------------
--创建计时器
function MapLogic.CreateTimer()
	UpdateBeat:Add(this.Update, this)
end

function MapLogic.Update()
	if Time.time - lastTime >= TIME_INTERVAL then
		lastTime = Time.time

		roleMgr:UpdateAllTitleBlood()
	end
end

function MapLogic.ClearTimer()
	UpdateBeat:Remove(this.Update, this)
end

-----------------------------------------------------------------
local changeSceneFunc

--跨场景回调
--保存跨场景回调
function MapLogic.SaveChangeSceneCallBack(callBack, needClear)
	if needClear then
		changeSceneFunc = {}
	end
	
	if not changeSceneFunc then
		changeSceneFunc = {}
	end
	table.insert(changeSceneFunc, callBack)
end

--跨场景回调
function MapLogic.DealChangeSceneCallBack()
	if changeSceneFunc then
		for k, v in ipairs(changeSceneFunc) do
			v()
		end
		
		changeSceneFunc = nil
	end
end

------------------------------------------------------------------------------
--客户端场景特效添加
local allClientEffectXls = {} --require("xlsdata/Map/MapClientEffectXls")

function MapLogic.CreateClientEffect(info)
	local prefabPath = "Prefab/"..info.Resource
	Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 1, nil, 0, 0, 0, function(go)
			go.name = string.format("ClientEffect_%s", info.EffectNo)
			go.transform.localScale = Vector3.one*info.Scale
			go.transform.eulerAngles = Vector3.New(0, info.Rotate, 0)
			go.transform.position = Vector3.New(info.Position[1], info.Position[2], info.Position[3])
		end)
end

function MapLogic.CreateClientEffects(sceneNo)
	local map = mapData[roleMgr.curSceneNo]
    if not map then
        logError("地图数据错误")
        return
    end

    if not map.ClientEffects or #map.ClientEffects == 0 then return end
    for i = 1, #map.ClientEffects do
    	local effInfo = allClientEffectXls[map.ClientEffects[i]]
    	if effInfo then
    		this.CreateClientEffect(effInfo)
    	end
    end
end

------------------------------------------------------------------------------
--获取场景空气墙
local airBlockData = {} --require("xlsdata/AirBlockXls")

function MapLogic.GetMapAirBlock(id)
	for k, v in pairs(airBlockData) do
		if v.ID == id then
			return v
		end
	end
end

return MapLogic