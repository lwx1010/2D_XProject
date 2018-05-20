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

--初始化完成，加载Pbc，跳转场景--
function Game.OnInitOK()
    math.randomseed(os.time() + tonumber(Util.GetDeviceIdentifierString()))
    math.random(0, 100)
    math.random(0, 100)
    math.random(0, 100)

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

	-- 加载Shader Property
	this._loadShaderPropertys()

	this.EnterLogin()
end

-- 加载Shader全局参数
function Game._loadShaderPropertys()

    local shaderFiles = List_string.New()
    shaderFiles:Add("ActorPropertys")
	shaderFiles:Add("TerrianProperty")
	shaderFiles:Add("T4MTerrianProperty")

    ShaderMgr:LoadPropertys(shaderFiles:ToArray())
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

    -- map.SceneName = "10001_test"
    if mapData.LoadType == 1 then
        sceneMgr:LoadSceneViaPreloading(MainStage.new(map.SceneName))
    else
        sceneMgr:LoadSceneViaPreloading(MainStage.new(map.SceneName))
    end
    --todo 测试
    --sceneMgr:LoadSceneChunk(MainChunkStage.new(map.SceneName) , true)
end

function Game.OnLevelLoaded()
    print("================Game.OnLevelLoaded=============", cameraMgr.mainCamera)
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
