
CtrlNames = {
    --Prompt = "PromptCtrl",
    --Message = "MessageCtrl",
    Login = "LoginCtrl",
    Main = "MainCtrl",
    
}


PanelNames = {
    "LoginPanel",
    "MainPanel",
}

--协议类型--
ProtocalType = {
    BINARY = 0,
    PB_LUA = 1,
    PBC = 2,
    SPROTO = 3,
}

FIHGT_CONRRECT_DISTANCE = 2

--当前使用的协议类型--
TestProtoType = ProtocalType.PBC

cameraMgr = CameraManager.GetInstance()
roleMgr = RoleManager.GetInstance()
Util = LuaFramework.Util
UtilNew = UtilNew
AppConst = AppConst
LuaHelper = LuaFramework.LuaHelper
ByteBuffer = LuaFramework.ByteBuffer
LuaComponent = LuaComponent

LuaBehaviour = LuaFramework.LuaBehaviour
panelMgr = Riverlake.PanelManager.GetSingleton()
soundMgr = LuaHelper.GetSoundManager()
networkMgr = LuaHelper.GetNetManager()
timerMgr = LuaHelper.GetTimerManager()
gameMgr = LuaHelper.GetGameManager()
sceneMgr = LuaHelper.GetSceneManager()
--cutsceneMgr = LuaHelper.GetCutsceneManager()

Application = UnityEngine.Application
WWW = UnityEngine.WWW
GameObject = UnityEngine.GameObject
SystemInfo = UnityEngine.SystemInfo
Camera = UnityEngine.Camera
Input = UnityEngine.Input
QualitySettings = UnityEngine.QualitySettings
Animator = UnityEngine.Animator
ParticleSystem = UnityEngine.ParticleSystem
Screen = UnityEngine.Screen
Renderer = UnityEngine.Renderer
ParticleRenderer = UnityEngine.ParticleRenderer
ParticleSystemRenderer = UnityEngine.ParticleSystemRenderer
SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer
Rect = UnityEngine.Rect
PlayerPrefs = UnityEngine.PlayerPrefs
BoxCollider = UnityEngine.BoxCollider

DateTime = System.DateTime

EventDelegate = EventDelegate
RotateMode = DG.Tweening.RotateMode
Sequence = DG.Tweening.DOTween.Sequence
DOTween = DG.Tweening.DOTween
MD5 = Riverlake.Crypto.MD5
resMgr = Riverlake.Resources.ResourceManager

User_Config = Config.User_Config

S2c_aoi_addself = Aoi.S2c_aoi_addself
S2c_aoi_addnpc = Aoi.S2c_aoi_addnpc
S2c_aoi_addplayer = Aoi.S2c_aoi_addplayer
Aoi_syncplayer = Aoi.Aoi_syncplayer
Aoi_syncnpc = Aoi.Aoi_syncnpc

FpsInst = Fps.Instance
SensitiveWord = SensitiveWordLogic.GetInstance()
ChatVoiceService = ChatVoiceService.Instance

Cutscene = CinemaDirector.Cutscene
Cutscene.CutsceneState = CinemaDirector.Cutscene.CutsceneState
QuadScene = Riverlake.Resources.QuadScene
LoadDelayAsync = Riverlake.Resources.LoadDelayAsync
LoadStageAsync = Riverlake.Resources.LoadStageAsync
SuperScene = Riverlake.Scene.SuperScene

-----------------------------------------------------
require "Common/SystemSetting"
GAMECONST = require "Common/GameConst"
LANGUAGE_TIP = require "language/LanguageTip"
LANGUAGE_CHOOSE = require "language/ChoosebagLanguage"
LANGUAGE_ACCOMPANYTASK = require "language/AccompanyTaskLanguage"
LANGUAGE_THREEUNIT = require "language/ThreeUnitRacesLanguage"
COMMONCTRL = require "Controller/CommonCtrl"
require "Common/ToolHelper"
require "Model/MapData"

HERO = require "Logic/Hero"
UIID = require "Logic/UiId"

HEROSKILLMGR = require "Logic/HeroSkillManager"
FIGHTMGR = require "Logic/FightManager"
PLAYERLOADER = require "Logic/PlayerLoader"
ITEMLOGIC = require "Logic/ItemLogic"
FUBENLOGIC = require "Logic/FuBenLogic"
ENTITYCREATE = require "logic/Role/EntityCreate"
HEADTITLE = require "logic/Role/HeadTitle"

MAPLOGIC = require "Logic/MapLogic"
TIPLOGIC = require('Logic/TipLogic')

require "Common.GoPool"
require "3rd.UISystem.init"
require "Common.EventManager"
require "Stage.init"
require "Controller.init"

AvatarCreator = require "Logic/AvatarCreator"