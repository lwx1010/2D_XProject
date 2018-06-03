
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

Util = LuaFramework.Util
AppConst = AppConst
LuaHelper = LuaFramework.LuaHelper
ByteBuffer = LuaFramework.ByteBuffer
LuaComponent = LuaComponent

LuaBehaviour = LuaFramework.LuaBehaviour
panelMgr = AL.PanelManager.GetSingleton()
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
List_string = System.Collections.Generic.List_string

EventDelegate = EventDelegate
RotateMode = DG.Tweening.RotateMode
Sequence = DG.Tweening.DOTween.Sequence
DOTween = DG.Tweening.DOTween
MD5 = AL.Crypto.MD5
resMgr = AL.Resources.ResourceManager

User_Config = Config.User_Config

FpsInst = Fps.Instance
SensitiveWord = SensitiveWordLogic.GetInstance()
ChatVoiceService = ChatVoiceService.Instance

Cutscene = CinemaDirector.Cutscene
Cutscene.CutsceneState = CinemaDirector.Cutscene.CutsceneState
LoadDelayAsync = AL.Resources.LoadDelayAsync
LoadStageAsync = AL.Resources.LoadStageAsync

-----------------------------------------------------
require "Common/SystemSetting"
GAMECONST = require "Common/GameConst"
LANGUAGE_TIP = require "language/LanguageTip"
LANGUAGE_CHOOSE = require "language/ChoosebagLanguage"
LANGUAGE_ACCOMPANYTASK = require "language/AccompanyTaskLanguage"
LANGUAGE_THREEUNIT = require "language/ThreeUnitRacesLanguage"
require "Common/ToolHelper"

HERO = require "Logic/Hero"
UIID = require "Logic/UiId"

ITEMLOGIC = require "Logic/ItemLogic"

TIPLOGIC = require('Logic/TipLogic')

require "xlsdata.init"
require "Common.GoPool"
require "3rd.UISystem.init"
require "Common.EventManager"
require "Stage.init"

require "View.init"
require "Model.init"
require "Controller.init"

