require "3rd/pbc/protobuf"

local lpeg = require "lpeg"

local json = require "cjson"
local util = require "3rd/cjson/util"

local mapData = require "xlsdata/Map/MapDataXls"
local soundEffectXls = {} --require('xlsdata/client/SoundEffectXls')

local Quaternion = Quaternion;

require "Common/functions"

require "Logic/LuaClass"
require "Logic/CtrlManager"

local taskLogic = {} --require('Logic/TaskLogic')

--管理器--
Game = {};
local this = Game;

local game;
local transform;
local gameObject;
local WWW = UnityEngine.WWW;

function Game.InitViewPanels()
	for i = 1, #PanelNames do
		require ("View/"..tostring(PanelNames[i]))
	end
end

--初始化完成，加载Pbc，跳转场景--
function Game.OnInitOK()
    math.randomseed(os.time() + tonumber(Util.GetDeviceIdentifierString()))
    math.random(0, 100)
    math.random(0, 100)
    math.random(0, 100)

    --注册LuaView--
    this.InitViewPanels()
    --初始话控件管理器
    CtrlManager.Init()
    --加载pbc
	this.LoadPb()
    --注册游戏逻辑事件
    this.AddLogicEvent()
    this.InitManagerData()
    log('LuaFramework InitOK--->>>');

    -- 初始渲染质量
    local curRendererQuality = User_Config.quality
    if curRendererQuality < 0 then
        curRendererQuality = Game.GetRuntimeDefaultQuality()
    end
    SystemSetting.initRendererQuality(curRendererQuality)

    -- 本地推送
    -- local pushXls = require("xlsdata/client/PushXls")
    -- local pushJson = json.encode(pushXls)
    -- NativeSDK.StartPushService(pushJson);

    this.EnterLogin()
end


--销毁--
function Game.OnDestroy()
    this.RemoveLogicEvent()
end

-----------------------------------------------------------------
--自定义函数
-----------------------------------------------------------------

function Game.LoadPb()
    local p2pFile = "protocol/proto.conf"
    local pbFolder = "protocol/pbc"

	local ret = Network.loadPbAndP2p( pbFolder, p2pFile)
	if( not ret ) then return false, "pb error" end

	--ret = Network.registerHandlers("Lua/protocol/handler")
	--if( not ret ) then return false, "handler error" end
end

function Game.TurnToLoginScene()
    roleMgr.entityCreate:ClearWhenChangeScene()
    panelMgr.sceneUIRoot:SetActive(false)
    worldPathfinding:StopWorldPathfinding()
    sceneMgr:LoadScene(PreLoginStage.new())
end

function Game.EnterLogin()
    this.CalculateScreenWidthOffset()
    --sceneMgr:LoadScene(LoginStage.new())
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Login);
    if ctrl ~= nil then
        ctrl:Awake()
    end
end

function Game.EnterCreateRole()

end

function Game.EnterMain()

end

function Game.LoadSceneViaPreloading( sceneId )
    local map = mapData[sceneId]
    print("=====================", TableToString(map))
    if not map then
        logError("找不到对应的地图数据")
        return
    end

    sceneMgr:LoadSceneViaPreloading(MainStage.new(map.SceneName))
    --todo 测试
    --sceneMgr:LoadSceneChunk(MainChunkStage.new(map.SceneName) , true)
    print("===========LoadSceneViaPreloading=======", map.SceneName)
end

function Game.LoadWulinSceneViaPreloading( sceneId )
    local map = mapData[sceneId]
    if not map then
        logError("找不到对应的地图数据")
        return
    end

    sceneMgr:LoadSceneViaPreloading(WulinStage.new(map.SceneName) , false)
end

function Game.LoadK3v3SceneViaPreloading( sceneId )
    local map = mapData[sceneId]
    if not map then
        logError("找不到对应的地图数据")
        return
    end

    sceneMgr:LoadSceneViaPreloading(KTStage.new(map.SceneName) , false)
end

function Game.OnLevelLoaded()
    print("================Game.OnLevelLoaded=============", roleMgr.mainCamera)
end

function Game.OnMainCtrlLoaded()
    --设置主角血量
    local MainCtrl = require "Controller/MainCtrl"
    MainCtrl.UpdateHeroBlood()

     --结婚音乐 特效
    MainCtrl.SetMarryTopShow(true)

    Game.PlayBGM()

    --设置打坐状态
    if roleMgr.mainRole then
        roleMgr.mainRole.daZuo = roleMgr.mainRole.daZuo
    end

    --进入华山论剑
    local HuaShanLunJianLogic = require("Logic/HuaShanLunJianLogic")
    if roleMgr.curSceneNo == HuaShanLunJianLogic.HSLJ_SCENEN then
        HEROSKILLMGR.SetHeroGuaJi(0, false)
        if not HuaShanLunJianLogic.IsInHSLJ then
            HuaShanLunJianLogic.HSLJEnter()
        else
            HuaShanLunJianLogic.HSLJEnterOtherLayer()
        end
    else
        --设置挂机状态
        HEROSKILLMGR.EnterSceneSetGuaJi()
    end

    --如果有，客户端战斗创建
    local ClientFightMgr = require("Logic/ClientFightManager")
    ClientFightMgr.DelayToCreateFuBenFight()

    local WulinFightManager = require("Logic/WulinFightManager")
    WulinFightManager.DelayCreateWulinFight()

    --如果有，客户端竞技战斗创建
    local JingjiFightManager = require("Logic/JingjiFightManager")
    JingjiFightManager.DelayCreateJingjiFight()

   	--跨场景回调打开界面
   	local MAPLOGIC = MAPLOGIC
   	MAPLOGIC.DealChangeSceneCallBack()

    --进入节日boss
    if KJIERIBOSSLOGIC.SceneIsJRBMap(roleMgr.curSceneNo) then
        MainCtrl.ShowJieriBossBar()
    end

    --进入Boss秘境
    if roleMgr.curSceneNo == SecretBossLogic.SECRET_MAPID then
        MainCtrl.OnSceneCreate()
    end

    --进入领地战
    local BPLDZLogic = require("Logic/BPLDZLogic")
    if BPLDZLogic.JudgeIsInBPLDZMap(roleMgr.curSceneNo)then
        BPLDZLogic.BPLDZEnter()
    end

    --进入攻城战
    local BPGCZLogic = require("Logic/BPGCZLogic")
    if BPGCZLogic.JudgeIsInBPGCZMap(roleMgr.curSceneNo)then
        BPGCZLogic.EnterBPGCZ()
    end

    --进入帮会秘境
    local BPMiJingLogic = require("Logic/BPMiJingLogic")
    if BPMiJingLogic.JudgeIsInBPMJMap(roleMgr.curSceneNo) then
        if not BPMiJingLogic.IsInBPMJ then
            BPMiJingLogic.BPMJEnter()
        else
            BPMiJingLogic.BPMJChangeLayer()
        end
    end

    --如果有，显示世界bossview
    local WorldBossLogic = require("Logic/WorldBossLogic")
    WorldBossLogic.ShowWorldBossView()
    --跨服中转处理
    WorldBossLogic.ShowKFBossMidView()

   	--主界面加载完成，如若有弹劾信息保存，弹出
    local BangHuiLogic = require("Logic/BangHuiLogic")
   	if BangHuiLogic.impeachCallBack then
   		BangHuiLogic.impeachCallBack()
   		BangHuiLogic.impeachCallBack = nil
   	end
    --如果进入帮派驻地
    BangHuiLogic.EnterFactionStation()
    BangHuiLogic.EnterFactionWar()

    --如果有，显示多人副本右上角窗口
    if MultiCopyManager.isInMultiCopyScene then
        MainCtrl.ShowMultiCopyBarUI()
        HEROSKILLMGR.EnterMultiCopyScene()
    end
    --如果有，进入王府夺宝场景
    if WangfuSnatchManager.isInWangfuSnatchScene then
        MainCtrl.ShowWangfuSnatchBarUI()
    end
    -- local MAPLOGIC = MAPLOGIC
    -- MAPLOGIC.EnterSceneEndFlyShow()
    --野外首领

    --3v3准备场景
    if ThreeUnitRacesModel.inst.inReadyMap then
        local states = ThreeUnitRacesModel.inst:GetRacesLeve()
        if states.state1 == 1 then
            MainCtrl.ShowScoreBarUI()
        elseif states.state2 == 1 then
            MainCtrl.ShowKnockoutBarUI()
        end
    end

    --3v3战斗场景
    if ThreeUnitRacesModel.inst.inBattleMap then
        MainCtrl.ShowKTFightingBarUI()
    end

    if FieldBossManager.IsInFieldBossScene then
        FieldBossHandler.SendEnterScene()
        MainCtrl.ShowFieldBossBarUI()
    end
    --科举考试
    if ImperialExamManager.state == 1 then
        MainCtrl.ShowImperialExamBarUI()
    end
    --杀戮战场

    if KillingFieldsManager.IsInKillFieldScene then
        MainCtrl.ShowKillingFieldsBarUI()
    end
    local HusongLogic = require('Logic/HusongLogic')
    if HERO.IsYunBiao and HERO.IsYunBiao > 0 then
        MainCtrl.HusongStart()
        HusongLogic.curLevel = HERO.IsYunBiao
        HusongLogic.GetNpcInfo()
        if roleMgr.mainRole then
            roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.Husong)
        end
    end
    if HusongLogic.needContinue then
        HusongLogic.ContinueHusong()
        HusongLogic.needContinue = false
    end

    --只进入一次，在游戏开始运行或退出重新登录时
    if not CtrlManager.GetCtrl(CtrlNames.Main).IsLoadShowTip then
        local BagCtrlManager = require('Logic/BagCtrlManager')
        BagCtrlManager.canBatchUse = true
        BagCtrlManager.CanShowUseItem()
        CtrlManager.GetCtrl(CtrlNames.Main).IsLoadShowTip = true
        CtrlManager.GetCtrl(CtrlNames.RewardHallUi).CanShowShibei = nil
        -- PROMPTTIPICONLOGIC.RefreshRedTips({['tips'] = PROMPTTIPICONLOGIC.Rewardhall , ['isshow'] = 0 , ['extend'] = PROMPTTIPICONLOGIC.Rewardhall_sbfl_OPEN..","..1})
        -- PROMPTTIPICONLOGIC.RefreshRedTips({['tips'] = PROMPTTIPICONLOGIC.Rewardhall , ['isshow'] = 0 , ['extend'] = PROMPTTIPICONLOGIC.Rewardhall_kytb_OPEN..","..1})
        -- PROMPTTIPICONLOGIC.RefreshRedTips({['tips'] = PROMPTTIPICONLOGIC.Rewardhall , ['isshow'] = 0 , ['extend'] = PROMPTTIPICONLOGIC.Rewardhall_xehg_OPEN..","..1})
        -- PROMPTTIPICONLOGIC.RefreshRedTips({['tips'] = PROMPTTIPICONLOGIC.Rewardhall , ['isshow'] = 0 , ['extend'] = PROMPTTIPICONLOGIC.Rewardhall_invest_OPEN..","..1})
    end
    taskLogic.ShowQuickWindow(roleMgr.curSceneNo)
    if taskLogic.curTaskNo and roleMgr.sceneType == 1 then
        taskLogic.DoCurTask(taskLogic.curTaskNo)
        roleMgr.mainRole.roleState:AddState(GAMECONST.RoleState.Task)
    end
end

--切换场景调用
function Game.ChangeSceneClear()
    --清理安全区计时器
    local MAPLOGIC = MAPLOGIC
    MAPLOGIC.ClearTimer()

    --客户端战斗场景退出时 销毁客户端战斗
    FIGHTMGR.ExitSceneDealWithFight()

    roleMgr.entityCreate:RemoveEntityCreateEventListener("Game")
end

function Game.UpdateHeroAttrs(attrs)
    if attrs['Sex'] ~= nil then
        -- print("--------------------------", HERO.Sex, attrs['Sex'])
        HEROSKILLMGR.ChangeSexUpdateSkill()
    end

    local mainRole = roleMgr.mainRole
    if mainRole == nil then
        return
    end

    local PLAYERLOADER = PLAYERLOADER
    if attrs['Fashion'] ~= nil and attrs['Fashion'] ~= 0 then
        PLAYERLOADER.LoadMainRole(mainRole, {fid = HERO.Fid})
        mainRole:ChangeShader(mainRole.model)
        PLAYERLOADER.LoadOtherShape(mainRole, 1, HERO['ActivateWeapon'], HERO['ShenyiModel'])
        mainRole:ResetHeadTitle()

        if not mainRole.isShapeChanged then
            roleMgr.entityCreate:AddFastShadow(mainRole, EntityType.EntityType_Self)
            mainRole.roleAction:ResetRoleAnimation(mainRole.model)
        else
            mainRole.model:SetActive(false)
        end
    end
    if attrs['ActivateWeapon'] ~= nil and attrs['ActivateWeapon'] ~= 0  and HERO.ShenjianModelState > 0 then
        PLAYERLOADER.LoadWeapon(mainRole, 1, attrs['ActivateWeapon'])
    end
    if attrs['UpHorseModel'] ~= nil then
        if attrs['UpHorseModel'] == 1 then
            if mainRole.partnerObj.isRide == false then
                mainRole.partnerObj.isRide = true
            end
        else
            if mainRole.partnerObj.isRide == true then
                mainRole.partnerObj.isRide = false
            end
        end
    end
    if attrs['MountModel'] ~= nil and attrs['MountModel'] ~= 0 then
        if mainRole.isRide and mainRole.horse.id ~= attrs['MountModel'] then
            if mainRole.isShapeChanged then
                mainRole.horse.changedId = attrs['MountModel']
            else
                mainRole.horse:ChangeHorse(attrs['MountModel'])
            end
        end
        mainRole.horse.id = attrs['MountModel']
    end
    if attrs['LingqiModel'] ~= nil and attrs['LingqiModel'] ~= 0 then
        if mainRole.lingqi ~= attrs['LingqiModel'] then
            PLAYERLOADER.LoadLingqi(mainRole, attrs['LingqiModel'], function()
                mainRole:ChangeShader(mainRole.lingqiObj.model)
                --切换模型时需要重置animator
                mainRole.lingqiObj.roleAction:ResetRoleAnimation(mainRole.lingqiObj.model)
                mainRole.lingqiObj.model:SetActive(mainRole.showLingqiState ~= 0 and not mainRole.isShapeChanged)
            end)
        end
    end
    if attrs['ShenyiModel'] ~= nil and attrs['ShenyiModel'] ~= 0 and HERO.ShenyiModelState > 0 then
        if mainRole.shenyi ~= attrs['ShenyiModel'] then
            PLAYERLOADER.LoadShenyi(mainRole, attrs['ShenyiModel'])
            mainRole:ChangeShader(mainRole.shenyi_model)
        end
    end
    if attrs['ShenjianModel'] ~= nil and attrs['ShenjianModel'] ~= 0 and HERO.ShenjianModelState > 0 then
        if mainRole.shenjian ~= attrs['ShenjianModel'] then
            PLAYERLOADER.LoadShenjian(mainRole, attrs['ShenjianModel'])
        end
    end

    if attrs['LingyiModel'] and attrs['LingyiModel'] > 0 and mainRole.partnerObj then
        if mainRole.partnerObj.lingyi ~= attrs['LingyiModel'] then
            PLAYERLOADER.Loadlingyi(mainRole.partnerObj, 6, attrs['LingyiModel'])
        end
    end

    if attrs['LingqinModel'] ~= nil and attrs['LingqinModel'] ~= 0 and mainRole.partnerObj then
        if mainRole.partnerObj.lingqin ~= attrs['LingqinModel'] then
            PLAYERLOADER.LoadLingqin(mainRole.partnerObj, 6, attrs['LingqinModel'])
        end
    end

    if attrs['PetModel'] ~= nil and attrs['PetModel'] ~= 0 then
        -- print("-------------------", mainRole.pet, attrs['PetModel'])
        if not mainRole.petObj then
            roleMgr:CreatePet(mainRole, attrs['PetModel'])
        elseif mainRole.pet ~= attrs['PetModel'] then
            local function callback(model, name)
                mainRole.petObj.model = model
                --切换模型时需要重置animator
                mainRole.petObj.roleAction:ResetRoleAnimation(model)
                roleMgr.entityCreate:AddFastShadow(mainRole.petObj, EntityType.EntityType_Pet)
                mainRole:ChangeShader(model)
                mainRole.petObj.model:SetActive(mainRole.showPetState ~= 0 and not mainRole.isShapeChanged)
            end
            PLAYERLOADER.LoadPet(mainRole, attrs['PetModel'],callback)
        end
    end

    if attrs["DaZuo"] then
        mainRole.daZuo = attrs["DaZuo"]
    end

    if attrs["Grade"] then
        --print("------------------", HERO.Grade)
        roleMgr.mainRole.grade = attrs["Grade"]
        Util.AddAutoRecycleParticle(roleMgr.mainRole.transform, "Prefab/Other/tongyongrenwushengjitexiao")
        soundMgr:PlayUISound('levelup')
        if User_Config.internal_sdk == 1 then
            HERO.SubmitExtraData(4)
            --[[local csm = CenterServerManager.Instance
            csm:UpgradeInfo(HERO.Name, HERO.Grade)
            csm:UpdateRoleInfo(HERO.Name, HERO.Grade, HERO.Vip, "")]]
        end
    end

    if attrs["Vip"] then
        if User_Config.internal_sdk == 1 then
            local csm = CenterServerManager.Instance
            csm:UpdateRoleInfo(HERO.Name, HERO.Grade, HERO.Vip, "")
        end
    end

    if attrs["IsYunBiao"] then
        if mainRole.beautyObj ~= nil and mainRole.beautyObj.model ~= nil then
            mainRole:RecycleOtherModel(9)
        end
        if attrs["IsYunBiao"] > 0 then
            local HusongXls = require('xlsdata/HusongXls')
            mainRole:ChangeHusongTitle(HusongXls[attrs['IsYunBiao']].Title)
            roleMgr.entityCreate:CreateBeauty(mainRole, attrs["IsYunBiao"], mainRole.id)
        end
    end
    if attrs['ThugHorseModel'] and attrs['ThugHorseModel'] > 0 then
        if mainRole.partnerObj then
            if mainRole.partnerObj.horse.id ~= attrs['ThugHorseModel'] and mainRole.partnerObj.isRide == true then
                mainRole.partnerObj.horse:ChangeHorse(attrs['ThugHorseModel'])
            else
                mainRole.partnerObj.horse.id = attrs.partnerhorse_model
                mainRole.partnerObj.isRide = true
            end
        end
    end
    if attrs['ShenyiModelState'] and mainRole.shenyi_model then
        mainRole.shenyi_model:SetActive(attrs['ShenyiModelState'] ~= 0)
    end
    if attrs['ShenjianModelState'] and mainRole.weapon_model then
        local id = attrs['ShenjianModelState'] == 0 and (mainRole.sex == 1 and 21000 or 22000) or HERO.ActivateWeapon
        PLAYERLOADER.LoadWeapon(mainRole, 2, id)
    end
    if attrs['ThugModelState'] and mainRole.partnerObj then
        mainRole.partnerObj.gameObject:SetActive(attrs['ThugModelState'] ~= 0)
    end
    if attrs['LingqiModelState'] and mainRole.lingqiObj then
        mainRole.lingqiObj.model:SetActive(attrs['LingqiModelState'] ~= 0)
    end
    if attrs['LingqinModelState'] and mainRole.partnerObj and mainRole.lingqin > 0 then
        PLAYERLOADER.LoadLingqin(mainRole.partnerObj, 6, attrs['LingqinModelState'] == 0 and 31000 or mainRole.lingqin)
    end
    if attrs['LingyiModelState'] and mainRole.partnerObj and mainRole.lingyi > 0 then
        mainRole.partnerObj.lingyi_model:SetActive(attrs['LingyiModelState'] ~= 0)
    end
    if attrs['PetModelState'] and mainRole.petObj and mainRole.petStr > 0 then
        mainRole.petObj.model:SetActive(attrs['PetModelState'] ~= 0)
    end
end

function Game.OnMainQuestContinue()
    if MarriageLogic.IsWedding or MarriageLogic.ISBANQUET then return end
    local taskCtrl = require('Controller/TaskAndTeamBarCtrl')

    local canDoTrunk = false
    if taskCtrl.trunkTask then
        canDoTrunk = taskCtrl.trunkTask:DoTrunkTask()
        if not taskLogic.canTrack then taskLogic.canTrack = true end
    end
    if not canDoTrunk then
        if taskCtrl.guajiTask then
            taskLogic.GotoDoGuajiTask()
        else
            taskCtrl.ContinueDailyTask(1)
        end
    end
end

function Game.AddLogicEvent()
    -- body
    -- BUFFLOGIC.AddEvent()
end

function Game.RemoveLogicEvent()
    -- BUFFLOGIC.RemoveEvent()
end

function Game.PlayBGM()
    local map = mapData[MapData.curMapNo]
    if not map then return end
    if not map.BGM then return end
    if MarriageLogic.IsPlayMarriageBGM() then return end
    soundMgr:PlayBGM(map.BGM, map.volume)
end

--初始化管理数据
function Game.InitManagerData()
    -- MultiCopyManager.InitData()
    -- NeigongManager.InitData()

    this.initAudioCompatibles()
end

-- 游戏内下载并解压完一个资源包回调
function Game.OnOnePackFileDownload( fileName )
    local strs1 = string.split(fileName, '/')
    if strs1[1] == "scenes" then
        local strs2 = string.split(strs1[2], '.')
        local cmd = {}
        cmd.scene_name = strs2[1]
        Network.send('C2s_hero_canadd_mapno', cmd)
        log("scene file download: "..fileName)
    end
end

-- function Game.SetPreloadingTips(label)
--     local data = require('xlsdata/LoadingTipsXls')
--     if #data == 0 then return end
--     local index = math.random(#data)
--     if index > #data then
--         index = #data
--     end
--     label.text = data[index].Tips
-- end

function Game.StartUpdateScene()
    sceneMgr:Load("UpdateScene" , UpdateStage.new("UpdateScene"))
end

function Game.GetSoundEffectVol(name)
    if soundEffectXls[name] == nil then
        return 1
    else
        return soundEffectXls[name].Volume
    end
end

--- 初始化音频兼容信息
function Game.initAudioCompatibles()

    for _ , sound in pairs(soundEffectXls) do
        if not string.isEmptyOrNil(sound.Mutexs) then
            local mutexArr = string.split(sound.Mutexs , '|')
            soundMgr:AddAudioCompatible(sound.SoundEffectName , sound.Weight , mutexArr)
        end
    end
end

--- 获得运行时的最佳质量
function Game.GetRuntimeDefaultQuality( )
    -- if not this.defaultQuality then

    --     local cpuCount = SystemInfo.processorCount
    --     local systemMemory = SystemInfo.systemMemorySize

    --     if cpuCount <= 4 or systemMemory <= 2048 then
    --         this.defaultQuality = 0
    --     elseif cpuCount <= 6 and systemMemory <= 4096 then
    --         this.defaultQuality = 1
    --     else
    --         this.defaultQuality = 2
    --     end
    -- end
    return this.defaultQuality or 0
end

function Game.IsSceneLoaded(mapNo)
    local sceneName = mapData[mapNo].SceneName
    local list = gameMgr:GetSceneFileList()
    for i = 0, list.Length - 1 do
        if list[i] == sceneName then
            return true
        end
    end
    return false
end

function Game.OnEnterDynBlocking(blockWall)
    print("Enter dynamic air block")
end

function Game.OnApplicationPause(paused, lostConnect)
    local cmd = {}
    if paused then
        cmd.is_back = 1
        Network.send("C2s_hero_background", cmd)
        print(TableToString(cmd))
    elseif lostConnect == false then
        cmd.is_back = 0
        Network.send("C2s_hero_background", cmd)
        print(TableToString(cmd))
    end
    Network.OnHeartBeat2Server()

    --fps屏蔽相关处理
    if paused then
        FpsInst:CancelCheckFps()
    else
        FpsInst:StartCheckFps()
    end
end

function Game.CalculateScreenWidthOffset()
    this.ScreenWidthOffset = 0
    local screen_width = ScreenResolution.GetInstance().scaleWidth
    local standrad_width = ScreenResolution.GetInstance().designWidth
    if screen_width > standrad_width then
        this.ScreenWidthOffset = (screen_width - standrad_width) / 2
        if User_Config.quality == 2 then
            this.ScreenWidthOffset = this.ScreenWidthOffset
        else
            this.ScreenWidthOffset = this.ScreenWidthOffset + this.ScreenWidthOffset / 2
        end
    end
end