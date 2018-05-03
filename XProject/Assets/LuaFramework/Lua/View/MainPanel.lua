require "Logic/CtrlManager"

local transform;
local gameObject;

MainPanel = {}
local this = MainPanel

--启动事件--
function MainPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();

	log("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function MainPanel.InitPanel()
	this.chatPanel = transform:Find("chatPanel").gameObject
	this.mailBtn = transform:Find("TipBar/Grid/mailBtn").gameObject
	this.chatBtn = transform:Find("TipBar/Grid/chatBtn").gameObject
	this.friendNotifyBtn = transform:Find("TipBar/Grid/friendNotifyBtn").gameObject
	this.HongBaoBtn = transform:Find("TipBar/Grid/mHongBaoBtn").gameObject
	this.HongBaoText = transform:Find("TipBar/Grid/mHongBaoBtn/numLabel"):GetComponent("UILabel")
	this.HongBaoEffectGo = transform:Find("TipBar/Grid/mHongBaoBtn/tongyongyuanxingzoumadeng_1").gameObject
	this.TipBarGo = transform:Find("TipBar").gameObject
	this.TipBarGrid = transform:Find("TipBar/Grid"):GetComponent("UIGrid")
	this.rollTextPanel = transform:Find("rollTextPanel").gameObject
	this.labaRollTextPanel = transform:Find("labaRollTextPanel").gameObject
	this.joystick = transform:Find("Joystick").gameObject
	this.playerBar = transform:Find("playerbar").gameObject
	this.targetBloodPanel = transform:Find("targetbloodpanel")
	this.targetBloodPanel.gameObject:SetActive(false)

	--左边按钮
	this.rideBtn = transform:Find("rideBtn").gameObject
	this.rideSprite = this.rideBtn:GetComponent(typeof(UISprite))

	this.daZuoBtn = transform:Find("dazuobtn").gameObject
	this.daZuoSprite = this.daZuoBtn:GetComponent(typeof(UISprite))

	--MapBar
	this.mapBar = transform:Find("mapbar").gameObject
	this.worldMap = transform:Find('mapbar/worldMap').gameObject
	this.mapBarTrans = transform:Find("mapbar")
	this.pingLabel = transform:Find("mapbar/ping").gameObject:GetComponent(typeof(UILabel))

	--mapbar下面的按钮
	this.mapBarBtnsGo = transform:Find("mapbar/btns").gameObject
	this.rankingBtn = transform:Find('mapbar/btns/rankingBtn').gameObject
	this.jiaoyihangBtn = transform:Find("mapbar/btns/jiaoyihangBtn").gameObject
	this.bianQiangBtn = transform:Find("mapbar/btns/strengthenBtn").gameObject

	--在下面的任务栏
	this.taskBar = transform:Find("taskBar").gameObject
	this.topBtnBar = transform:Find("topBtnBar").gameObject

	--技能相关
	this.skillPanel = transform:Find("skillPanel").gameObject

	--顶部按钮
	--0playerbar旁边的按钮
	this.leftTopBtnGrid = transform:Find("playerbar/lefttopbtns")
	this.Rechangerbtn = transform:Find('playerbar/lefttopbtns/Rechangerbtn').gameObject
	this.RechangerTip = transform:Find('playerbar/lefttopbtns/Rechangerbtn/tip').gameObject

	this.downloadBtn = transform:Find("themeBar/themeBtns/DownloadBtn").gameObject
	this.downloadEffect = transform:Find("themeBar/themeBtns/DownloadBtn/Effect")

	--按钮容器
	this.effentPanel = transform:Find('topBtnBar/Panel').gameObject
	this.topBtnGrid1 = transform:Find("topBtnBar/Panel/views/Grid1")
	this.topBtnGrid2 = transform:Find("topBtnBar/Panel/views/Grid2")
	this.topBtnGrid3 = transform:Find("topBtnBar/Panel/views/Grid3")

	this.topPopUi1 = transform:Find("topBtnBar/topPopUi1").gameObject
	this.topPopGrid1 = transform:Find("topBtnBar/topPopUi1/btngrid")
	this.topPopUi2 = transform:Find("topBtnBar/topPopUi2").gameObject
	this.topPopGrid2 = transform:Find("topBtnBar/topPopUi2/btngrid")

	this.topPopMask = transform:Find("topBtnBar/popmask").gameObject
	this.topPopBgLeft1 = transform:Find("topBtnBar/topPopUi1/bg1")
	this.topPopBgRight1 = transform:Find("topBtnBar/topPopUi1/bg1/bg2")
	this.topPopBgLeft2 = transform:Find("topBtnBar/topPopUi2/bg1")
	this.topPopBgRight2 = transform:Find("topBtnBar/topPopUi2/bg1/bg2")

	this.bossHallBtn = transform:Find("topBtnBar/Panel/BossHallBtn").gameObject

	--第一排
	this.shopBtn = transform:Find("topBtnBar/Panel/views/Grid1/ShopBtn").gameObject

	this.rewardhall = transform:Find('topBtnBar/Panel/views/Grid1/rewardhall').gameObject
	this.rewardhalltip = transform:Find('topBtnBar/Panel/views/Grid1/rewardhall/tip').gameObject

	this.activityHallBtn = transform:Find("topBtnBar/Panel/views/Grid1/activityhallbtn").gameObject
	this.activityhalltip = transform:Find("topBtnBar/Panel/views/Grid1/activityhallbtn/tip").gameObject
	this.activityHallDoubletip = transform:Find('topBtnBar/Panel/views/Grid1/activityhallbtn/doubletip').gameObject
	this.activityHallMatchtip = transform:Find('topBtnBar/Panel/views/Grid1/activityhallbtn/Waiting').gameObject

	this.biZuoBtn = transform:Find('topBtnBar/Panel/views/Grid1/bizuobtn').gameObject
	this.biZuoBtntip = transform:Find('topBtnBar/Panel/views/Grid1/bizuobtn/tip').gameObject
	this.biZuoDoubletip = transform:Find('topBtnBar/Panel/views/Grid1/bizuobtn/doubletip').gameObject
	this.biZuoMatchtip = transform:Find('topBtnBar/Panel/views/Grid1/bizuobtn/Waiting').gameObject
	this.bossmaintip = transform:Find('topBtnBar/Panel/BossHallBtn/tip').gameObject

	--第二排
	this.firstPayBtn = transform:Find("topBtnBar/Panel/views/Grid2/firstpaybtn").gameObject
	this.dailyPayBtn = transform:Find("topBtnBar/Panel/views/Grid2/dailypaybtn").gameObject
	this.firstPayRedPointGo = this.firstPayBtn.transform:Find("tip").gameObject
	this.dailyPayRedPointGo = this.dailyPayBtn.transform:Find("tip").gameObject

	this.chaoZhiBtn = transform:Find('topBtnBar/Panel/views/Grid2/chaozhiyouhuibtn').gameObject
	this.chaoZhiBtntip = transform:Find('topBtnBar/Panel/views/Grid2/chaozhiyouhuibtn/tip').gameObject

	this.rebatectBtn = transform:Find('topBtnBar/Panel/views/Grid2/Rebatectbtn').gameObject
	this.rebatehallTip = transform:Find('topBtnBar/Panel/views/Grid2/Rebatectbtn/tip').gameObject

	this.treasureBtn = transform:Find("topBtnBar/Panel/views/Grid2/TreasureBtn").gameObject
	this.treasureTip = transform:Find("topBtnBar/Panel/views/Grid2/TreasureBtn/tip").gameObject

	this.yunYingBtn = transform:Find('topBtnBar/Panel/views/Grid2/yunyingactbtn').gameObject
	this.yunYingBtntip = transform:Find('topBtnBar/Panel/views/Grid2/yunyingactbtn/tip').gameObject
	
	--第三排
	this.sxjlBtn = transform:Find('topBtnBar/Panel/views/Grid3/shenxjlBtn').gameObject

	this.sevenday = transform:Find('topBtnBar/Panel/views/Grid3/sevenday').gameObject
	this.sevendaytip = transform:Find('topBtnBar/Panel/views/Grid3/sevenday/tip').gameObject
	--vip
	this.VIPTequanbtn = transform:Find('topBtnBar/Panel/views/Grid3/viptequanbtn').gameObject
	this.viptqTip = transform:Find('topBtnBar/Panel/views/Grid3/viptequanbtn/tip').gameObject
	--节日boss
	this.kjieribossbtn = transform:Find('topBtnBar/Panel/views/Grid3/kjieribossbtn').gameObject
	this.kjieribosstip = transform:Find('topBtnBar/Panel/views/Grid3/kjieribossbtn/tip').gameObject
	this.kjieribosstime = transform:Find('topBtnBar/Panel/views/Grid3/kjieribossbtn/timelab').gameObject
	this.kjieribosseffect = transform:Find('topBtnBar/Panel/views/Grid3/kjieribossbtn/effectcont').gameObject


	--正在进行或者准备进行的活动入口按钮
	this.activityObj = {}
	for i = 1,3 do
		table.insert(this.activityObj , transform:Find('topBtnBar/Panel/views/Grid3/activity'..i).gameObject)
	end

	--弹出按钮
	-- this.tianjianTip = transform:Find('topBtnBar/topPopUi1/btngrid/tianjianBtn/tip').gameObject
	-- this.tianjianDoubletip = transform:Find('topBtnBar/topPopUi1/btngrid/tianjianBtn/doubletip').gameObject
	-- this.tianjianBtn = transform:Find('topBtnBar/topPopUi1/btngrid/tianjianBtn').gameObject

	-- this.fubenSingletip = transform:Find("topBtnBar/topPopUi1/btngrid/FuBenSingleBtn/tip").gameObject
	-- this.jinjieDoubletip = transform:Find('topBtnBar/topPopUi1/btngrid/FuBenSingleBtn/doubletip').gameObject
	-- this.fuBenSingleBtnGo = transform:Find("topBtnBar/topPopUi1/btngrid/FuBenSingleBtn").gameObject

	-- this.multiCopytip = transform:Find("topBtnBar/topPopUi1/btngrid/MultiCopyBtn/tip").gameObject
	-- this.multiCopyBtn = transform:Find("topBtnBar/topPopUi1/btngrid/MultiCopyBtn").gameObject
	-- this.multiCopyWaiting = transform:Find("topBtnBar/topPopUi1/btngrid/MultiCopyBtn/Waiting").gameObject

	-- this.zhanchangTip = transform:Find('topBtnBar/topPopUi1/btngrid/zhanchangBtn/tip').gameObject
	-- this.zhanchangBtn = transform:Find('topBtnBar/topPopUi1/btngrid/zhanchangBtn').gameObject
	-- this.zhanchangWaiting = transform:Find("topBtnBar/topPopUi1/btngrid/zhanchangBtn/Waiting").gameObject

	this.activityBtn = transform:Find("topBtnBar/topPopUi2/btngrid/ActivityBtn").gameObject
	this.kaifuTip = transform:Find('topBtnBar/topPopUi2/btngrid/ActivityBtn/tip').gameObject
	this.activityTimeLbl = transform:Find("topBtnBar/topPopUi2/btngrid/ActivityBtn/time"):GetComponent("UILabel")

	this.accRechargeTip = transform:Find('topBtnBar/topPopUi2/btngrid/AccRechargeBtn/tip').gameObject
	this.accRechargeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/AccRechargeBtn").gameObject

	this.repetitionConsumeTip = transform:Find('topBtnBar/topPopUi2/btngrid/RepetitionConsumeBtn/tip').gameObject
	this.repetitionConsumeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/RepetitionConsumeBtn").gameObject

	this.continueRechargeTip = transform:Find('topBtnBar/topPopUi2/btngrid/ContinueRechargeBtn/tip').gameObject
	this.continueRechargeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/ContinueRechargeBtn").gameObject

	this.repetitionRechargeTip = transform:Find('topBtnBar/topPopUi2/btngrid/RepetitionRechargeBtn/tip').gameObject
	this.repetitionRechargeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/RepetitionRechargeBtn").gameObject

	this.singleRechargeTip = transform:Find('topBtnBar/topPopUi2/btngrid/SingleRechargeBtn/tip').gameObject
	this.singleRechargeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/SingleRechargeBtn").gameObject

	this.luckyEggTip = transform:Find('topBtnBar/topPopUi2/btngrid/LuckyEggBtn/tip').gameObject
	this.luckyEggBtn = transform:Find("topBtnBar/topPopUi2/btngrid/LuckyEggBtn").gameObject

	this.colorEggTip = transform:Find('topBtnBar/topPopUi2/btngrid/QicainiudanBtn/tip').gameObject
	this.colorEggBtn = transform:Find("topBtnBar/topPopUi2/btngrid/QicainiudanBtn").gameObject

	this.luckyBoxTip = transform:Find('topBtnBar/topPopUi2/btngrid/LuckyBoxBtn/tip').gameObject
	this.luckyBoxBtn = transform:Find("topBtnBar/topPopUi2/btngrid/LuckyBoxBtn").gameObject

	this.luckydrawTip = transform:Find('topBtnBar/topPopUi2/btngrid/luckydrawbtn/tip').gameObject
	this.luckyDrawBtn = transform:Find('topBtnBar/topPopUi2/btngrid/luckydrawbtn').gameObject
	--珍宝阁
	this.zhenbaogeBtn = transform:Find("topBtnBar/topPopUi2/btngrid/zhenbaogebtn").gameObject
	this.zhenbaogeRedPointGo = this.zhenbaogeBtn.transform:Find("tip").gameObject
	--选宝阁
	this.choosebagBtn = transform:Find("topBtnBar/topPopUi2/btngrid/choosebagbtn").gameObject
	this.choosebagRedPointGo = this.choosebagBtn.transform:Find("tip").gameObject
	--精彩活动
	this.wonderfulActivitybtn = transform:Find('topBtnBar/topPopUi2/btngrid/wonderfulActivitybtn').gameObject
	--狂送元宝
	this.kuangsongyuanbaoBtn = transform:Find("topBtnBar/topPopUi2/btngrid/KuangsongyuanbaoBtn").gameObject
	--充值返利
	this.rechargeRebateBtn = transform:Find("topBtnBar/topPopUi2/btngrid/RechargeRebateBtn").gameObject
	--合服
	this.MergeActBtn = transform:Find('topBtnBar/topPopUi2/btngrid/MergeActBtn').gameObject
	this.MergeActTip = transform:Find('topBtnBar/topPopUi2/btngrid/MergeActBtn/tip').gameObject
	-- 新春活动
	this.XinChunActBtn = transform:Find('topBtnBar/topPopUi2/btngrid/XinChunActBtn').gameObject
	this.XinChunActTip = transform:Find('topBtnBar/topPopUi2/btngrid/XinChunActBtn/tip').gameObject
	--跨服夺宝
	this.crossTreasureBtn = transform:Find('topBtnBar/topPopUi2/btngrid/CrossTreasureBtn').gameObject
	this.crossTreasureTip = transform:Find('topBtnBar/topPopUi2/btngrid/CrossTreasureBtn/tip').gameObject


	--底部按钮
	this.leftSwitchBtn = transform:Find("leftSwitchBtn").gameObject
	this.leftSwithTip = transform:Find('leftSwitchBtn/tip').gameObject
	this.leftSwitchSprite = transform:Find("leftSwitchBtn/switchSprite").gameObject

	this.btnsBottomTrans = transform:Find("bottomBtnBar")
	this.btnsBottomGrid = transform:Find("bottomBtnBar/bottomBox")
	this.bottomBtnBar = transform:Find("bottomBtnBar/bottomBox").gameObject
	
	this.roleBtn = transform:Find("bottomBtnBar/bottomBox/playerBtn").gameObject
	this.roletip = transform:Find('bottomBtnBar/bottomBox/playerBtn/tip').gameObject

	this.horseBtn = transform:Find("bottomBtnBar/bottomBox/partnerBtn").gameObject
	this.horstip = transform:Find('bottomBtnBar/bottomBox/partnerBtn/tip').gameObject
	-- this.SkillBtn = transform:Find("bottomBtnBar/bottomBox/skillBtn").gameObject
	-- this.skilltip = transform:Find('bottomBtnBar/bottomBox/skillBtn/tip').gameObject
	this.bagBtn = transform:Find("bottomBtnBar/bottomBox/bagBtn").gameObject

	this.xianjueBtn = transform:Find("bottomBtnBar/bottomBox/xianjueBtn").gameObject
	this.xianjueTip = transform:Find('bottomBtnBar/bottomBox/xianjueBtn/tip').gameObject

	this.duanzaoBtn = transform:Find('bottomBtnBar/bottomBox/duanzaoBtn').gameObject
	this.duanzaotip = transform:Find('bottomBtnBar/bottomBox/duanzaoBtn/tip').gameObject

	this.shenqiBtn = transform:Find('bottomBtnBar/bottomBox/shenqiBtn').gameObject
	this.shenqitip = transform:Find('bottomBtnBar/bottomBox/shenqiBtn/tip').gameObject

	this.PokedexBtn = transform:Find('bottomBtnBar/bottomBox/PokedexBtn').gameObject
	this.pokedextip = transform:Find('bottomBtnBar/bottomBox/PokedexBtn/tip').gameObject

	this.XinFaBtn = transform:Find('bottomBtnBar/bottomBox/XinFaBtn').gameObject
	this.XinFatip = transform:Find('bottomBtnBar/bottomBox/XinFaBtn/tip').gameObject

	this.bangPaiBtnGo = transform:Find("bottomBtnBar/bottomBox/BangPaiBtn").gameObject
	this.bangpaitip = transform:Find('bottomBtnBar/bottomBox/BangPaiBtn/tip').gameObject

	this.marriageBtn = transform:Find('bottomBtnBar/bottomBox/marriageBtn').gameObject
	this.marriageTip = transform:Find('bottomBtnBar/bottomBox/marriageBtn/tip').gameObject

	this.friendBtn = transform:Find('bottomBtnBar/bottomBox/friendBtn').gameObject
	this.friendtip = transform:Find('bottomBtnBar/bottomBox/friendBtn/tip').gameObject

	this.settingBtn = transform:Find('bottomBtnBar/bottomBox/settingBtn').gameObject

	--冒血cv
	this.bloodwidget = transform:Find("headAndBlood/bloodwidget")
	
	this.themeBar = transform:Find("themeBar").gameObject
	this.guajiBar =	transform:Find("guajispr").gameObject
	this.guajiBar:SetActive(false)
	this.autoRunFlag = transform:Find("autoRunSpr").gameObject
	this.husong = transform:Find('husongPanel').gameObject

	this.shenxianjuanlv = transform:Find('topBtnBar/Panel/views/Grid3/shenxjlBtn/tip').gameObject
	
	--小提示界面容器
	this.tipPanel = transform:Find('tipPanel').gameObject
	--战力提示框
	this.upgradeFightingTipUi = transform:Find('tipPanel/upgradFightingTip').gameObject
	this.fightinglab = transform:Find('tipPanel/upgradFightingTip/fightinglab').gameObject
	this.upgradefightinglab = transform:Find('tipPanel/upgradFightingTip/upzhanlibg/upgradefightinglab').gameObject
	this.upzhanlibg = transform:Find('tipPanel/upgradFightingTip/upzhanlibg').gameObject
	this.funcitontipuiPanel = transform:Find('tipPanel/FunctionTipUiPanel').gameObject
	this.sdSingleCopyTipPanel = transform:Find('tipPanel/sdSingleCopyTip').gameObject
	this.upSingleCopyTipPanel = transform:Find('tipPanel/upSingleCopyTip').gameObject
	this.openFubenUiTipPanel = transform:Find('tipPanel/openFubenUiTip').gameObject
	this.fightEffect = transform:Find('tipPanel/upgradFightingTip/effect').gameObject
	this.activityPanel = transform:Find('tipPanel/activityTip').gameObject
	this.offlinepaenl = transform:Find('tipPanel/offlineTip').gameObject
	this.offlineOkbtn = transform:Find('tipPanel/offlineTip/usebtn').gameObject
	this.offlineClosebtn = transform:Find('tipPanel/offlineTip/closebtn').gameObject
	this.titleTipPanel = transform:Find('tipPanel/ChengHaoTip').gameObject
	this.ChenJiuUpgradeTip = transform:Find('tipPanel/ChenJiuUpgradeTip').gameObject
	this.downloadTipPanel = transform:Find('tipPanel/downloadTip').gameObject
	this.fieldBossTipPanel = transform:Find('tipPanel/FieldBossTip').gameObject
	this.activityPanel.gameObject:SetActive(false) --默认不显示
	this.expTrans = transform:Find("expbar/exp")
	this.expTransBg = transform:Find("expbar/bg")

	this.rightUpViews = transform:Find("rightupviews")
	this.fuBenBarTrans = transform:Find("rightupviews/fubenPanel")
	this.leftTimeTrans = transform:Find("timePanel")
	this.marriageTimeTrans = transform:Find("marriage/marriagetimePanel")
	this.marriageInteraction = transform:Find('marriage/interaction')

	--技能提示
	this.skillWarning = transform:Find("skillwarningview").gameObject
	this.flyBtn = transform:Find("autoRunSpr/flybtn").gameObject
	this.caijiBnt = transform:Find('caijiBtn').gameObject

	--婚宴特效
	this.marryEffectPos = transform:Find('marryEffectPos').gameObject	

	--场景艺术字
	this.sceneNameTrans = transform:Find("sceneName")
	this.sceneNameBg = transform:Find("sceneName/bg"):GetComponent(typeof(UISprite))
	this.sceneNameTitle = transform:Find("sceneName/name"):GetComponent(typeof(UITexture))

	this.marryBox = transform:Find('marry').gameObject

	--物品飞行到背包效果
	this.flyToBagGo = transform:Find("notifyPanel/flyToBag").gameObject
	

	this.hasCreated = true
	this.offsetX = Game.ScreenWidthOffset
	this.AutoAdjustScreenResolution()

end

function MainPanel.AutoAdjustScreenResolution()
	if this.offsetX > 0 then
		local leftSides = 
		{
			this.joystick.transform,
			this.playerBar.transform,
			this.rideBtn.transform,
			this.daZuoBtn.transform,
			this.themeBar.transform,
			this.leftSwitchBtn.transform,
			this.btnsBottomTrans,
			this.expTrans,
			this.sceneNameTrans
		}
		for i = 1, #leftSides do
			leftSides[i].position = Vector3.New(leftSides[i].position.x - this.offsetX, leftSides[i].position.y, leftSides[i].position.z)
		end
		
		local rightSides = 
		{
			this.mapBarTrans,
			this.taskBar.transform,
			this.topBtnBar.transform,
			this.skillPanel.transform,
			this.rightUpViews
		}
		for i = 1, #rightSides do
			rightSides[i].position = Vector3.New(rightSides[i].position.x + this.offsetX, rightSides[i].position.y, rightSides[i].position.z)
		end
		local expbg = this.expTransBg:GetComponent('UISprite')
		expbg.width = ScreenResolution.GetInstance().defaultWidth
	end
end

function MainPanel.OnInit()
	local MainCtrl = require "Controller/MainCtrl"
  	this.SetVisible(false)
  	local co
  	co = coroutine.start(function()
  		coroutine.wait(0.1)
		this.SetVisible(true)
		MainCtrl.OnInit()
		coroutine.stop(co)
		end)
  	-- MainCtrl.OnInit()
end

function MainPanel.SetVisible(visible)
	gameObject:SetActive(visible)
end

function MainPanel.GetMainPanelTrans()
	return transform
end

function MainPanel.OnChangeSceneHide()
	print("--------OnChangeSceneHide-------")

	local MainCtrl = require "Controller/MainCtrl"
  	MainCtrl.OnChangeSceneHide()
end

--单击事件--
function MainPanel.OnDestroy()
	this.bloodwidget = nil
	this.hasCreated = false
	local MainCtrl = require "Controller/MainCtrl"
  	MainCtrl.OnDestroy()
end