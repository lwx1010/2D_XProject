-- @Author: LiangZG
-- @Date:   2017-04-27 11:40:53
-- @Last Modified time: 2018-01-03 17:32:41
-- @Desc:  主场景逻辑

local mapData = require "xlsdata/Map/MapDataXls"
local jumpGateData = {} --require("xlsdata/Map/MapJumpGateXls")
local fakeJumpGateData = {} --require("xlsdata/Map/MapFakeJumpGateXls")
local xunLuoNpcData = {} --require("xlsdata/Npc/XunLuoNpcXls")
local airBlockData = {} --require("xlsdata/AirBlockXls")

local MainStage = class("MainStage" , BaseStage)

local cacheWindowTbl

function MainStage:ctor(stageName)
    Game.currentStage = self
    MainStage.super.ctor(self, stageName , "PreLoadingScene")
    self.isLoadingScene = true

    print("-------------MainStage:ctor--------------"..stageName)
end

function MainStage:CacheWindow(name)
    cacheWindowTbl = cacheWindowTbl or {}
    table.insert(cacheWindowTbl, name)
end

function MainStage:onShowCacheWindows()
    if not cacheWindowTbl then return end
    for _, v in pairs(cacheWindowTbl) do
        CtrlManager.PopUpPanel(v)
    end
    cacheWindowTbl = {}
end

function MainStage:onEnter( transiter , stageLoader )
	if MapData.curMapNo == 1012 then --在竞技场中 取消屏蔽其他玩家
        User_Config.blockAllianPlayer = 0
        User_Config.blockOtherPlayer = 0
        User_Config.blockOtherPartner = 0
        User_Config.blockOtherLingqi = 0
        User_Config.blockOtherPet = 0
    end

    local map = mapData[MapData.curMapNo]
    if map.CutsceneTriggers and map.CutsceneTriggers == 1 then
        local resTriggersObj = resMgr:LoadPrefab("Prefab/Cutscene/Triggers/TriggerGroup" .. map.SceneName )
        if not resTriggersObj then
            Util.LogError("找不到对应场景的剧情信息 sceneNo:" .. MapData.curMapNo .. "    sceneName:" .. map.SceneName)
            return
        end

        local triggerGroup = newObject(resTriggersObj)

        --预加载
        cutsceneMgr:PreLoadCutscene(triggerGroup , stageLoader)

        if CutsceneTriggerCtrl.inst:isTrigger(100101) then
            --resMgr:LoadPrefab("Prefab/Cutscene/Cutscene100101") --预加载
            --cutsceneMgr:Trigger(100101)
            WorldView1Panel.show(1)
        end
    end

    if map.IsQuadScene == 1 then --切分场景
        stageLoader:AddLoader(LoadDelayAsync(2.5) , 50)
        stageLoader.SceneLoader.OnFinish = function (loader)
            self:initQuadScene(map)
        end
    end
end

--- 初始化四分场景
function MainStage:initQuadScene(map)

    --print(string.format("%s_scene" , map.SceneName))

    local quadScene = GameObject.Find(string.format("%s_scene" , map.SceneName))
    if quadScene then
        self.quadScene = quadScene:GetComponent(typeof(QuadScene))

        local heroInfo = roleMgr:GetHeroInfo()
        local worldPos = Util.Convert2RealPosition(heroInfo.nmsg.x, heroInfo.nmsg.z)
        self.quadScene:Initlize(worldPos)
    end
end

function MainStage:onShow()
    self.isLoadingScene = false
    local map = mapData[MapData.curMapNo]
    if not map then
        logError("地图数据错误")
        return
    end

	-- 发送创角成功协议,用于服务器打点
	-- Network.SendLoginSuccess()

	-- roleMgr.positionSync:SetProtoInterval(200);
	cameraMgr.mainCamera = Camera.main
	print("================MainStage.Show=============", cameraMgr.mainCamera)
    cameraMgr.mainCamera.transform.rotation = Quaternion.Euler(map.CR1[1], map.CR1[2], map.CR1[3]);
    --摄像机fov 最大和最小
    local fov = string.split(map.FOV1, ",")
    if #fov > 1 then
        cameraMgr.cameraFov = tonumber(fov[1])
        cameraMgr.cameraFovMin = tonumber(fov[2])
    else
        cameraMgr.cameraFov = tonumber(fov[1])
        cameraMgr.cameraFovMin = tonumber(fov[1])
    end

    cameraMgr.mainCamera.fieldOfView = cameraMgr.cameraFov
    --剧情触发器
    if map.CutsceneTriggers and map.CutsceneTriggers == 1 then

        if CutsceneTriggerCtrl.inst:isTrigger(100101) then
            self.playCutscene = true
            EventManager.SendEvent("WorldViewPlayAnimation")
            cutsceneMgr:OnCutsceneEvent()
        end
    end

    --摄像机Y轴偏移 最大和最小
    local yOffset = string.split(map.YOffset1, ",")
    if #yOffset > 1 then
        cameraMgr.cameraOffsetY = tonumber(yOffset[1])
        cameraMgr.cameraOffsetYMin = tonumber(yOffset[2])
    else
    cameraMgr.cameraOffsetY = tonumber(yOffset[1])
        cameraMgr.cameraOffsetYMin = tonumber(yOffset[1])
    end

    --摄像机与主角之间的距离 最大 最小
    local cdistance = string.split(map.CDistance, ",")
    if #yOffset > 1 then
        cameraMgr.cameraDistance =  tonumber(cdistance[1])
        cameraMgr.cameraDistanceMin = tonumber(cdistance[2])
    else
        cameraMgr.cameraDistance =  tonumber(cdistance[1])
        cameraMgr.cameraDistanceMin = tonumber(cdistance[1])
    end

    --摄像机Y轴偏移 最大和最小
    local xAngle = string.split(map.XAngle, ",")
    if #xAngle > 1 then
        cameraMgr.xAngleMax = tonumber(xAngle[1])
        cameraMgr.xAngleMin = tonumber(xAngle[2])
    else
        cameraMgr.xAngleMax = tonumber(xAngle[1])
        cameraMgr.xAngleMin = tonumber(xAngle[1])
    end

    -- roleMgr.maxClimbHeight = map.MaxClimbHeight
    Game.PlayBGM()

    local cmd = {}
    cmd.map_no = MapData.curMapNo
    cmd.map_id = MapData.curMapId
    Network.send("C2s_aoi_createmap_ok", cmd)

    -- 请求主角道具列表
    -- local ItemLogic = require("Logic/ItemLogic")
    -- ItemLogic.SendToGetAllItemsOnline()

    -- if Event.EventExist(EventType.Hero_Attrs) == false then
    --     Event.AddListener(EventType.Hero_Attrs, Game.UpdateHeroAttrs)
    --     Game.UpdateHeroAttrs(HERO)
    -- end
    --场景更新
    -- MapUpdateManager.Update(MapData.curMapNo)

    -- roleMgr.entityCreate:AddEntityCreateEventListener("Game", handler(self , self.OnMainRoleLoaded), nil)
    -- roleMgr.entityCreate:AddEntityCreateEventListener("CollectPanel", nil, CollectPanel.NpcBuffEventRegister)

    -- 开启线程下载资源
    gameMgr:StartDownloadPackFiles()

    if map.IsQuadScene == 0 or not map.IsQuadScene then
         -- PretendStageChangePanel.onClickClose( )
    else
        local finishHandler = handler(self , self.onWaitQuadSceneFinish)
        self.loadedWaitHandler = coroutine.start(finishHandler)
    end

    SkillUiPanel.show()

    print("=-==========================")
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Main);
    if ctrl ~= nil and not ctrl.init then
        ctrl.OnCreate()
    end
end

--- 等待四分场景加载完成
function MainStage:onWaitQuadSceneFinish()
    local waitTime = os.time()
    while not self.quadScene:IsEnterDone()  do
        coroutine.wait(0.01)
    end

    -- PretendStageChangePanel.onClickClose( )
    coroutine.stop(self.loadedWaitHandler)
    self.loadedWaitHandler = nil
    print("wait loaded :" .. (os.time() - waitTime))
end


function MainStage:OnMainRoleLoaded(entity, sync)
    if entity.entityType ~= EntityType.EntityType_Self then
        return
    end

    if self.quadScene then
        self.quadScene:LookAt(entity.cacheTrans)
    end

    --- 如果当前有剧情动画处理
    if self.playCutscene then
        --EventManager.SendEvent("WorldViewPlayAnimation")
        -- 预加载
        --cutsceneMgr:PlayCurrentCutscene()
        --cutsceneMgr:Trigger(100101)
        --cutsceneMgr:SetSceneShadow(false) --更新阴影设置
        RoleStateTransition.firstEnter = false
        entity.controller.CanMovePosition = false
        --entity.move:StopPath()
        --HEROSKILLMGR.SetHeroGuaJi(0, false)
    end

    -- print("=-==========================")
    -- local ctrl = CtrlManager.GetCtrl(CtrlNames.Main);
    -- if ctrl ~= nil and not ctrl.init then
    --     ctrl.OnCreate()
    -- end

    -- roleMgr:CreateJumpGate(jumpGateData)
    -- roleMgr:CreateFakeJumpGate(fakeJumpGateData)
    -- roleMgr:CreateXunLuoNpcs(xunLuoNpcData)
    -- roleMgr:CreateAirBlock(airBlockData)
    --玩家方向特效父节点
    -- entity.directionEffectTrans:SetParent(roleMgr.entityCreate.cf.cameraParentTrans)
    -- entity.directionEffectTrans.localPosition = Vector3.zero
    -- entity.directionEffectTrans.gameObject:SetActive(false)

    local chatCtrl = CtrlManager.GetCtrl(CtrlNames.Chat)
    if chatCtrl ~= nil then
        chatCtrl:Awake()
    end

    if HERO.IsYunBiao and HERO.IsYunBiao > 0 then
        if MainPanel.hasCreated then
            roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.Husong)
        end
    end

    --帮派秘境
    local BPMiJingLogic = require("Logic/BPMiJingLogic")
    if BPMiJingLogic.IsInBPMJ and ctrl and ctrl.BPMJBarCtrl then
        ctrl.BPMJBarCtrl.UpdateDirShow()
    end

    local MAPLOGIC = MAPLOGIC
    MAPLOGIC.ShowSafeSceneTip()
    MAPLOGIC.CreateClientEffects()
    self:onShowCacheWindows()
end

function MainStage:onExit( )
    print("LoginState onExit <----")
    MinimapModel.inst:clearNpc()
    FieldBossManager.AttackTarget = nil
    self.quadScene = nil
end

return MainStage

