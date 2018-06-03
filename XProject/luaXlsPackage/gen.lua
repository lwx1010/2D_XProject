--$Id: gen.lua 45927 2008-07-09 06:30:35Z zhj $
--__auto_local__start--
local string=string
local table=table
local math=math
local io=io
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
--__auto_local__end--


local Actions ={}


local function TellUser(who,...)
	if not who then return end
	local msg = UTIL.Serialize(...) 
	who:TellMe(msg)
end 


local function GenNormal(who, File)
	local GenFile=Import(File)
	GenFile.DoGen(who)
end

--生成跳转点
local function GenTele(who)
	local GenFile=Import("autocode/gen/npc/gen_scene_tele.lua")
	GenFile.DoGen(who)
end

--生成NPC数据表
function GenNpcDat(who)
	local GenFile=Import("autocode/gen/npc/gen_npcdata.lua")
	GenFile.DoGen(who)
	GenFightNpcDat(who)
	GenLanguageData(who)
	GenGoblinData(who)
	return true
end

--生成战斗NPC数据表
function GenFightNpcDat(who)
	local GenFile=Import("autocode/gen/npc/gen_fightnpcdata.lua")
	GenFile.DoGen(who)
	return true
end

--生成场景NPC
function GenSceneNpc()
	local GenFile=Import("autocode/gen/npc/gen_scene_npc.lua")
	GenFile.DoGen(who)
	return true
end 

--生成场景数据
function GenSceneData()
	local GenFile=Import("autocode/gen/npc/gen_scene_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成阵法
function GenLineUpData(who)
	local GenFile=Import("autocode/gen/lineup/gen_lineup_expdata.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/lineup/gen_lineup_data.lua")
	GenFile.DoGen(who)
	GenLineUpPosData(who)
	return true
end

function GenLineUpPosData()
	local GenFile=Import("autocode/gen/lineup/gen_lineup_pos.lua")
	GenFile.DoGen(who)
	return true
end


--生成区域数据
function GenAreaData()
	local GenFile=Import("autocode/gen/npc/gen_scene_area.lua")
	GenFile.DoGen(who)
	GenTele(who)
	return true
end

--生成语言包
function GenLanguageData()
	local GenFile=Import("autocode/gen/language/npc.lua")
	GenFile.DoGen(who)
	local GenFile2=Import("autocode/gen/language/npc2.lua")
	GenFile2.DoGen(who)
	local GenFile2=Import("autocode/gen/language/name.lua")
	GenFile2.DoGen(who)
	GenTalkData()
	return true
end

--生成经验表
function GenExpData()
	local GenFile=Import("autocode/gen/exp/gen_expdata.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/exp/gen_matialexpdata.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/exp/gen_skillexpdata.lua")
	GenFile.DoGen(who)
	return true
end

--生成闲话表
function GenTalkData()
	local GenFile=Import("autocode/gen/talk/gen_talkdata.lua")
	GenFile.DoGen(who)
	return true
end

--生成装备数据
function GenEquipData()
	local GenFile=Import("autocode/gen/equip/gen_equipdata.lua")
	GenFile.DoGen(who)
	return true
end

--生成buff互斥表
function GenBuffData()
	local GenFile=Import("autocode/gen/skill/gen_buffdata.lua")
	GenFile.DoGen(who)
	return true
end 

--生成角色初始化数据
function GenHeroData()
	local GenFile=Import("autocode/gen/hero/gen_herodata.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/hero/gen_newbiedata.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/hero/gen_photo.lua")
	GenFile.DoGen(who)
	return true
end 

--生成武学属性表
function GenMartialData()
	local GenFile=Import("autocode/gen/skill/gen_martialdata.lua")
	GenFile.DoGen(who)
	return true
end


function GenTrunkTaskData(who, s)
--	--主线任务对话文字
--	_DEBUG("begin talkdata")
--	local GenFile=Import("autocode/gen/task/trunk_task/gen_trunk_task_talkdata.lua")
--	GenFile.DoGen(who)
--	_DEBUG("end talkdata")
--	
--	--主线任务描述文字
--	_DEBUG("begin task_desc")
--	GenFile = Import("autocode/gen/task/trunk_task/gen_trunk_task_desc.lua")
--	GenFile.DoGen(who)
--	_DEBUG("end task_desc")

	--主线任务数据
	_DEBUG("begin task_data")
	local GenFile = Import("autocode/gen/task/trunk_task/gen_trunk_task_data.lua")
	GenFile.DoGen(who)
	_DEBUG("end task_data")
	return true
end

--生成同伴数据表
function GenPartnerData(who)
	local GenFile=Import("autocode/gen/partner/gen_partnerdata.lua")
	GenFile.DoGen(who)
	return true
end

--生成武学，心法，招式数据表，与招式文件
function GenMartialAll(who)
	local GenFile=Import("autocode/gen/skill/gen_martialskill.lua")
	GenFile.DoGen(who)
	GenSkillData(who)
	GenSkillBuff(who)
	local GenFile=Import("autocode/gen/skill/gen_martialprop.lua")
	GenFile.DoGen(who)
	return true
end

function GenSkillData(who)
	local GenFile=Import("autocode/gen/skill/gen_martial.lua")
	GenFile.DoGen(who)
	return true
end

function GenSkillBuff(who)
	local GenFile=Import("autocode/gen/skill/gen_skillbuff.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/skill/gen_martial_zizhe.lua")
	GenFile.DoGen(who)
	return true
end 

function GenItemFile(who, s)
	--local GenFile=Import("autocode/gen/item/gen_item.lua")
	local GenFile=Import("autocode/gen/item/gen_item_new.lua")
	GenFile.DoGen(who)
	return true
end

function GenItemData(who, s)
	local GenFile=Import("autocode/gen/item/gen_item_data.lua")
	GenFile.DoGen(who)
	return true
end


function GenFamilyData(who, s)
	local FAMILY_GEN_FILE = {
		"autocode/gen/family/gen_family_data.lua", 		--门派数据
		"autocode/gen/family/gen_master_npc_data.lua", 		--武学NPC数据
		"autocode/gen/family/gen_learning_martial_data.lua", 	--武学数据
		"autocode/gen/family/gen_family_new_data.lua", 	--门派新数据
	}
	for _, GenFilePath in pairs(FAMILY_GEN_FILE) do
		local GenFile=Import(GenFilePath)
		GenFile.DoGen(who)
	end
	return true
end


function GenPotentialData(who, s)
	local GenFile = Import("autocode/gen/potential/gen_equip_potential_data.lua")
	GenFile.DoGen(who)
	return true
end


function GenSellData(who, s)
	local GenFile=Import("autocode/gen/trade/gen_sell_data.lua")
	GenFile.DoGen(who)
	return true
end


function GenYunBiaoData(who, s)
	local GenFile=Import("autocode/gen/task/yun_biao/gen_yun_biao_data.lua")
	GenFile.DoGen(who)
	return true
end


function GenAllScene()
	GenSceneData()
	GenAreaData()
	GenTele()
	GenSceneNpc()
end

function GenAccPasswd(who)
	local GenFile=Import("autocode/gen/acc/gen_acc.lua")
	GenFile.DoGen(who)
	return true
end 


function GenBonusData(who)
	local GenFile=Import("autocode/gen/bonus/gen_bonus_data.lua")
	GenFile.DoGen(who)
	return true
end 

function GenGoblinData(who)
	local GenFile=Import("autocode/gen/goblin/gen_goblindata.lua")
	GenFile.DoGen(who)
	return true
end 

function GenFaqData(who)
local GenFile=Import("autocode/gen/faq/gen_faq_data.lua")
	GenFile.DoGen(who)
	return true
end 

function GenReward(who)
	local GenFile=Import("autocode/gen/reward/gen_reward.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/reward/gen_stat_reward.lua")
	GenFile.DoGen(who)
	return true
end 

function GenReward2(who)
	local GenFile=Import("autocode/gen/reward/gen_stat_reward.lua")
	GenFile.DoGen(who)
	return true
end

function GenItemMerge(who)
	local GenFile=Import("autocode/gen/item/gen_item_merge_data.lua")
	GenFile.DoGen(who)
	return true
end 


function GenGuideData(who)
	local GenFile=Import("autocode/gen/guide/gen_guide_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenMartyr(who)
	local GenFile=Import("autocode/gen/martyr/gen_martyr.lua")
	GenFile.DoGen(who)
	return true
end
function GenWorldBossData(who)
	local GenFile=Import("autocode/gen/worldboss/gen_worldboss.lua")
	GenFile.DoGen(who)
	return true
end 
function GenWulinData(who)
	local GenFile=Import("autocode/gen/wulin/gen_wulin.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/wulin/gen_wulinreward.lua")
	GenFile.DoGen(who)
	local GenFile=Import("autocode/gen/wulin/gen_wulin_cham.lua")
	GenFile.DoGen(who)
	return true
end 

local function GenXinMoData(who)
	local GenFile=Import("autocode/gen/xinmo/gen_xinmo_data.lua")
	GenFile.DoGen(who)
	return true
end 


local function GenBattleData(who)
	local GenFile=Import("autocode/gen/battle/gen_battle_data.lua")
	GenFile.DoGen(who)
	return true
end


local function GenDynamicNpcData(who)
	local GenFile=Import("autocode/gen/npc/gen_dynamic_npc_data.lua")
	GenFile.DoGen(who)
	return true
end

local function GenAIData(who)
	local GenFile=Import("autocode/gen/ai/gen_aidata.lua")
	GenFile.DoGen(who)
	return true
end

function GenItemProp(who)
	local GenFile=Import("autocode/gen/item/gen_item_prop.lua")
	GenFile.DoGen(who)
	return true
end

function GenTaolin(who)
	local GenFile=Import("autocode/gen/taolin/gen_taolin.lua")
	GenFile.DoGen(who)
	return true
end 

function GenShengxiao(who)
	local GenFile=Import("autocode/gen/shengxiao/gen_shengxiao.lua")
	GenFile.DoGen(who)
	return true
end

function GenWizard(who)
	local GenFile=Import("autocode/gen/wiz/gen_wiz.lua")
	GenFile.DoGen(who)
	return true
end


function GenBuff(who)
	local File = "autocode/gen/buff/gen_buff_data.lua"
	GenNormal(who, File)
end


function GenEnYiData(who)
	local FileList  = {"autocode/gen/enyi/gen_enyi_data.lua", "autocode/gen/enyi/gen_enyi_name.lua", "autocode/gen/enyi/gen_enyi_qa.lua"}
	for _, File in pairs(FileList) do
		GenNormal(who, File)
	end
end


function GenItem(who)
	
	local GenFile=Import("autocode/gen/item/gen_item_data.lua")
	GenFile.DoGen(who)
	
--	local GenFile=Import("autocode/gen/item/gen_item.lua")
	local GenFile=Import("autocode/gen/item/gen_item_new.lua")
	GenFile.DoGen(who)
	
	
	local GenFile=Import("autocode/gen/item/gen_item_prop.lua")
	GenFile.DoGen(who)
	
	return true
end

--商城数据
function GenShopData(who)
	local GenFile=Import("autocode/gen/trade/gen_shop_data.lua")
	GenFile.DoGen(who)
end

function GenPartyBonus(who)
	local GenFile=Import("autocode/gen/partybonus/gen_corp_host.lua")
	GenFile.DoGen(who)
end 

function GenRidefashion(who)
	local GenFile=Import("autocode/gen/ridefashion/gen_ridefashion.lua")
	GenFile.DoGen(who)
end

function GenBranchTask(who)
	GenNormal(who, "autocode/gen/task/branch_task/gen_branch_task_data.lua")
end

function GenFuBen(who)
	local FileList = {"autocode/gen/fuben/gen_fuben_task_data.lua", "autocode/gen/fuben/gen_fuben_data.lua"} 
	for _, File in pairs(FileList) do
		GenNormal(who, File)
	end
end


function GenGridCost(who)
	local GenFile = Import("autocode/gen/item/gen_grid_cost.lua")
	GenFile.DoGen(who)
end

function GenAll(who)
		for key,func in pairs(Actions) do
			if key ~= "help" and key ~= "genall" and key ~= "martial_all" and key ~= "reward" and key ~= "reward2" then
				GenAutoCode(who,key)
			end
		end
end

function GenItemXlProp(who)
	local GenFile = Import("autocode/gen/item/gen_item_xl_prop.lua")
	GenFile.DoGen(who)
end

function GenItemXlUpgrade(who)
	local GenFile = Import("autocode/gen/item/gen_item_xl_upgrade.lua")
	GenFile.DoGen(who)
end


function GenJiBu(who)
	local GenFile = Import("autocode/gen/jibu/gen_jibu_data.lua")
	GenFile.DoGen(who)
end

function GenItemValue(who)
	local GenFile = Import("autocode/gen/item/gen_item_value.lua")
	GenFile.DoGen(who)
end

function GenItemStreng(who)
	local GenFile = Import("autocode/gen/item/gen_item_streng.lua")
	GenFile.DoGen(who)
	local GenFile = Import("autocode/gen/item/gen_item_streng_move.lua")
	GenFile.DoGen(who)
end

function GenVip(who)
	local GenFile = Import("autocode/gen/vip/gen_vip.lua")
	GenFile.DoGen(who)
end 

function GenGiftBox(who)
	local GenFile = Import("autocode/gen/giftbox/gen_giftbox_data.lua")
	GenFile.DoGen(who)
end

function GenVipShop(who)
	local GenFile = Import("autocode/gen/vip/gen_vip_shop.lua")
	GenFile.DoGen(who)
end

function GenActivity(who)
	local GenFile = Import("autocode/gen/activity/gen_activity.lua")
	GenFile.DoGen(who)
end 

function GenItemKindProp(who)
	local GenFile = Import("autocode/gen/item/gen_item_kind_prop.lua")
	GenFile.DoGen(who)
end


function GenOrg(who)
	local GenFile = Import("autocode/gen/org/gen_org_data.lua")
	GenFile.DoGen(who)
end

function GenLeague(who)
	local GenFile = Import("autocode/gen/league/gen_league_data.lua")
	GenFile.DoGen(who)
end

--add by 彭彭
--生成藏宝图
function GenCangBaoDat(who)
	local GenFile=Import("autocode/gen/cangbao/gen_cang_bao_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成对穿肠
function GenDuilianDat(who)
	local GenFile=Import("autocode/gen/duilian/gen_duilian_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成排行榜
function GenRankListDat(who)
	local GenFile=Import("autocode/gen/ranklist/gen_ranklist_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成同伴招募
function GenBuyPartnerDat(who)
	local GenFile=Import("autocode/gen/buypartner/gen_buypartner_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成祭奠英烈
function GenYingLieDat(who)
	local GenFile=Import("autocode/gen/yinglie/gen_yinglie_data.lua")
	GenFile.DoGen(who)
	return true
end

--生成坐骑
function GenMountsDat(who)
	local GenFile=Import("autocode/gen/mounts/gen_mounts_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenLoginReward(who)
	local GenFile=Import("autocode/gen/loginreward/gen_loginreward_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenUserName(who)
	local GenFile=Import("autocode/gen/user/gen_user_name.lua")
	GenFile.DoGen(who)
	return true
end

function GenXiaZhiLu(who)
	local GenFile=Import("autocode/gen/xiazhilu/gen_xiazhilu_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenEveryday(who)
	local GenFile=Import("autocode/gen/everyday/gen_everyday_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenMingYang(who)
	local GenFile=Import("autocode/gen/mingyang/gen_mingyang_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenEffect(who)
	local GenFile=Import("autocode/gen/effect/gen_effect_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenSaima(who)
	local GenFile=Import("autocode/gen/saima/gen_saima_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenSihai(who)
	local GenFile=Import("autocode/gen/sihai/gen_sihai_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenGradeReward(who)
	local GenFile=Import("autocode/gen/gradereward/gen_gradereward_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenJishou(who)
	local GenFile=Import("autocode/gen/jishou/gen_jishou_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenLeaTask(who)
	local GenFile=Import("autocode/gen/leatask/gen_leatask_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenPata(who)
	local GenFile=Import("autocode/gen/pata/gen_pata_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenOrgFuben(who)
	local GenFile=Import("autocode/gen/orgfuben/gen_orgfuben_data.lua")
	GenFile.DoGen(who)
	return true
end

function GenQA(who)
	local GenFile=Import("autocode/gen/qa/gen_qa_data.lua")
	GenFile.DoGen(who)
end

function GenChongzhiRoll(who)
	local GenFile=Import("autocode/gen/chongzhiroll/gen_chongzhiroll_data.lua")
	GenFile.DoGen(who)
end
---------彭彭

--生成成就数据
function GenAchieve(who)
	local GenFile=Import("autocode/gen/achieve/gen_achieve.lua")
	GenFile.DoGen(who)
	return true
end

--生成称号数据
function GenTitle(who)
	local GenFile=Import("autocode/gen/title/gen_title.lua")
	GenFile.DoGen(who)
	return true
end

function GenForbitLogin(who)
	local GenFile = Import("autocode/gen/forbitlogin/gen_forbitlogin.lua")
	GenFile.DoGen(who)
end

function GenItemTzProp(who)
	local GenFile = Import("autocode/gen/item/gen_item_tz_prop.lua")
	GenFile.DoGen(who)
end

function GenSuitUpgrade(who)
	local GenFile = Import("autocode/gen/item/gen_suit_upgrade.lua")
	GenFile.DoGen(who)
end

function GenGemData(who)
	local GenFile = Import("autocode/gen/gem/gen_gem_data.lua")
	GenFile.DoGen(who)
end


function GenEquipEff(who)
	local GenFile = Import("autocode/gen/item/gen_equip_eff.lua")
	GenFile.DoGen(who)
end

function GenItemUpgrade(who)
	local GenFile = Import("autocode/gen/item/gen_item_upgrade.lua")
	GenFile.DoGen(who)
end

function GenTimeControl(who)
	local GenFile = Import("autocode/gen/timecontrol/gen_time_control.lua")
	GenFile.DoGen(who)
end

function GenSkillZizhi(who)
	local GenFile = Import("autocode/gen/skill/gen_skill_zizhi.lua")
	GenFile.DoGen(who)
end

function GenCarryData(who)
	local GenFile = Import("autocode/gen/trade/gen_carry_data.lua")
	GenFile.DoGen(who)
end

function GenBroadcast(who)
	local GenFile = Import("autocode/gen/broadcast/gen_broadcast.lua")
	GenFile.DoGen(who)
end

function GenMingWang(who)
	local GenFile = Import("autocode/gen/trade/gen_mingwang_data.lua")
	GenFile.DoGen(who)
end

function GenShengWang(who)
	local GenFile = Import("autocode/gen/trade/gen_shengwang_data.lua")
	GenFile.DoGen(who)
end

function GenShouXi(who)
	local GenFile = Import("autocode/gen/shouxi/gen_shouxi_data.lua")
	GenFile.DoGen(who)
end

function GenUnionBattle(who)
	local GenFile = Import("autocode/gen/unionbattle/gen_unionbattle.lua")
	GenFile.DoGen(who)
end 

function GenKey(who)
	local GenFile = Import("autocode/gen/key/gen_key.lua")
	GenFile.DoGen(who)
end

function GenJHTitle(who)
	local GenFile = Import("autocode/gen/jhtitle/gen_jhtitle_data.lua")
	GenFile.DoGen(who)
end

function GenSignin(who)
	local GenFile = Import("autocode/gen/signin/gen_signin.lua")
	GenFile.DoGen(who)
end

function GenTreasure(who)
	local GenFile = Import("autocode/gen/treasure/gen_treasure.lua")
	GenFile.DoGen(who)
end

function GenNewactivity(who)
	local GenFile = Import("autocode/gen/newactivity/gen_newactivity.lua")
	GenFile.DoGen(who)
end

function GenMountsSkin(who)
	local GenFile = Import("autocode/gen/mounts/gen_mounts_skin.lua")
	GenFile.DoGen(who)
end

function GenMergeBonus(who)
	local GenFile = Import("autocode/gen/merge/gen_merge.lua")
	GenFile.DoGen(who)
end

function GenFieldBoss(who)
	local GenFile = Import("autocode/gen/fieldboss/gen_fieldboss.lua")
	GenFile.DoGen(who)
end


function GenDanji(who)
	local GenFile=Import("autocode/gen/danji/gen_danji.lua")
	GenFile.DoGen(who)
	return true
end 

function GenLeagueLunch(who)
	local GenFile = Import("autocode/gen/league/gen_league_npc_data.lua")
	GenFile.DoGen(who)
end

function GenLeaguereward(who)
	local GenFile = Import("autocode/gen/league/gen_league_reward.lua")
	GenFile.DoGen(who)
end

function GenOrgBuilding(who)
	local GenFile = Import("autocode/gen/org/gen_org_building.lua")
	GenFile.DoGen(who)
end

function GenOrgTask(who)
    local GenFile = Import("autocode/gen/org/gen_org_task.lua")
    GenFile.DoGen(who)
end

function GenOrgTalk(who)
    local GenFile = Import("autocode/gen/org/gen_org_talk.lua")
    GenFile.DoGen(who)
end

function GenBobin(who)
	local GenFile = Import("autocode/gen/bobin/gen_bobin.lua")
	GenFile.DoGen(who)
end

function GenZhongQiulData(who)
	local GenFile = Import("autocode/gen/zhongqiu/gen_zhongqiu_data.lua")
	GenFile.DoGen(who)
	return true
end 

function GenActiveDegree(who)
	local GenFile = Import("autocode/gen/activedegree/gen_activedegree.lua")
	GenFile.DoGen(who)
	return true
end

function GenOrgSnatch(who)
	local GenFile = Import("autocode/gen/orgsnatch/gen_orgsnatch.lua")
	GenFile.DoGen(who)
	return true
end

function GenLeagueFight(who)
    local GenFile = Import("autocode/gen/leaguefight/gen_league_fight.lua")
    GenFile.DoGen(who)
    return true
end

function GenPayRule(who)
	local GenFile = Import("autocode/gen/pay/gen_pay_rule.lua")
	GenFile.DoGen(who)
	return true
end

--后台运营活动数据  by旭光
function GenpartybaseData(who)
	local GenFile = Import("autocode/gen/partybase/gen_partybase_data.lua")
	GenFile.DoGen(who)
end

--累计充值活动数据 by旭光
function GenP1001Data(who)
	local GenFile = Import("autocode/gen/partybase/p1001/gen_p1001_data.lua")
	GenFile.DoGen(who)
end

--每日在线活动数据 by智华
function GenP1002Data(who)
	local GenFile = Import("autocode/gen/partybase/p1002/gen_p1002_data.lua")
	GenFile.DoGen(who)
end

--副本运营活动数据 by智华
function GenP1003Data(who)
	local GenFile = Import("autocode/gen/partybase/p1003/gen_p1003_data.lua")
	GenFile.DoGen(who)
end

--同伴运营活动数据 by旭光
function GenP1004Data(who)
	local GenFile = Import("autocode/gen/partybase/p1004/gen_p1004_data.lua")
	GenFile.DoGen(who)
end

--单笔充值活动数据 by旭光
function GenP1005Data(who)
	local GenFile = Import("autocode/gen/partybase/p1005/gen_p1005_data.lua")
	GenFile.DoGen(who)
end

--限时商城活动数据 by旭光
function GenP1006Data(who)
	local GenFile = Import("autocode/gen/partybase/p1006/gen_p1006_data.lua")
	GenFile.DoGen(who)
end

--运营活动：锄恶济困  智华
function GenP1007Data(who)
	local GenFile = Import("autocode/gen/partybase/p1007/gen_p1007_data.lua")
	GenFile.DoGen(who)
end

--运营活动：稀世宝石  智华
function GenP1008Data(who)
	local GenFile = Import("autocode/gen/partybase/p1008/gen_p1008_data.lua")
	GenFile.DoGen(who)
end

--运营活动：累计消费  智华
function GenP1009Data(who)
	local GenFile = Import("autocode/gen/partybase/p1009/gen_p1009_data.lua")
	GenFile.DoGen(who)
end

--运营活动：蛋生来了  智华
function GenP1010Data(who)
	local GenFile = Import("autocode/gen/partybase/p1010/gen_p1010_data.lua")
	GenFile.DoGen(who)
end

--运营活动：合服活动 武林榜 by旭光
function GenP1011Data(who)
	local GenFile = Import("autocode/gen/partybase/p1011/gen_p1011_data.lua")
	GenFile.DoGen(who)
end

--运营活动：合服活动 英雄擂 by旭光
function GenP1012Data(who)
	local GenFile = Import("autocode/gen/partybase/p1012/gen_p1012_data.lua")
	GenFile.DoGen(who)
end

--运营活动：合服活动 携手闯险关 by旭光
function GenP1013Data(who)
	local GenFile = Import("autocode/gen/partybase/p1013/gen_p1013_data.lua")
	GenFile.DoGen(who)
end

--运营活动：腊八活动 by旭光
function GenP1014Data(who)
	local GenFile = Import("autocode/gen/partybase/p1014/gen_p1014_data.lua")
	GenFile.DoGen(who)
end
-- 运营活动：同伴招募活动 by fj
function GenP1015Data(who)
	local GenFile = Import("autocode/gen/partybase/p1015/gen_p1015_data.lua")
	GenFile.DoGen(who)
end
-- 运营活动：装备强化活动 by fj
function GenP1016Data(who)
	local GenFile = Import("autocode/gen/partybase/p1016/gen_p1016_data.lua")
	GenFile.DoGen(who)
end
-- 运营活动：坐骑培养活动 by fj
function GenP1017Data(who)
	local GenFile = Import("autocode/gen/partybase/p1017/gen_p1017_data.lua")
	GenFile.DoGen(who)
end
-- 运营活动：同伴境界培养 by fj
function Genp1018Data(who)
	local GenFile = Import("autocode/gen/partybase/p1018/gen_p1018_data.lua")
	GenFile.DoGen(who)
end
--运营活动：充值返还 by fj
function GenP1019Data(who)
	local GenFile = Import("autocode/gen/partybase/p1019/gen_p1019_data.lua")
	GenFile.DoGen(who)
end
--运营活动：年兽 add by fj
function GenP1020Data(who)
	local GenFile = Import("autocode/gen/partybase/p1020/gen_p1020_data.lua")
	GenFile.DoGen(who)
end
function GenP1021Data(who)
	local GenFile = Import("autocode/gen/partybase/p1021/gen_p1021_data.lua")
	GenFile.DoGen(who)
end
--运营活动: add by fj
function GenP1022Data(who)
	local GenFile = Import("autocode/gen/partybase/p1022/gen_p1022_data.lua")
	GenFile.DoGen(who)
end
-- 运营活动: add by fj
function GenP1023Data(who)
	local GenFile = Import("autocode/gen/partybase/p1023/gen_p1023_data.lua")
	GenFile.DoGen(who)
end
--运营活动：收集字奖励 add by fj
function GenP1025Data(who)
	local GenFile = Import("autocode/gen/partybase/p1025/gen_p1025_data.lua")
	GenFile.DoGen(who)
end
--运营活动：红包 add by fj
function GenP1026Data(who)
	local GenFile = Import("autocode/gen/partybase/p1026/gen_p1026_data.lua")
	GenFile.DoGen(who)
end
--运营活动：红包 add by fj
function GenP1028Data(who)
	local GenFile = Import("autocode/gen/partybase/p1028/gen_p1028_data.lua")
	GenFile.DoGen(who)
end
--开服活动
function GenUnifyAct(who)
	local GenFile = Import("autocode/gen/unifyact/gen_unifyact.lua")
	GenFile.DoGen(who)    
end
function GenOrgWallFight(who)
    local GenFile = Import("autocode/gen/orgwallfight/gen_orgwallfight.lua")
    GenFile.DoGen(who)
    return true
end
--运营活动：宝石大赢家
function GenP1030Data(who)
	local GenFile = Import("autocode/gen/partybase/p1030/gen_p1030_data.lua")
	GenFile.DoGen(who)
end

function GenAllParty(who)
	GenP1001Data(who)
	GenP1002Data(who)
	GenP1003Data(who)
	GenP1004Data(who)
	GenP1005Data(who)
	GenP1006Data(who)
	GenP1007Data(who)
	GenP1008Data(who)
	GenP1009Data(who)
	GenP1010Data(who)
	GenP1011Data(who)
	GenP1012Data(who)
	GenP1013Data(who)
	GenP1014Data(who)
	GenP1015Data(who)
	GenP1016Data(who)
	GenP1017Data(who)
	Genp1018Data(who)
	GenP1019Data(who)
	GenP1020Data(who)
	GenP1021Data(who)
	GenP1022Data(who)
	GenP1023Data(who)
	GenP1025Data(who)
	GenP1026Data(who)
	GenP1028Data(who)
	GenP1030Data(who)
end

function GenFanhuanData(who)
	local GenFile = Import("autocode/gen/fanhuan/gen_fanhuan.lua")
	GenFile.DoGen(who)
end 

function GenTrainData(who)
	local GenFile = Import("autocode/gen/user/gen_user_train.lua")
	GenFile.DoGen(who)
end

--跨服战数据
function GenServerBattle(who)
	local GenFile = Import("autocode/gen/serverbattle/gen_serverbattle.lua")
	--GenFile.DoGen(who)
	local GenFile = Import("autocode/gen/serverbattle/gen_battleshop.lua")
	GenFile.DoGen(who)
	local GenFile = Import("autocode/gen/serverbattle/gen_battlebonus.lua")
	GenFile.DoGen(who)
end

--法宝系统
function GenMagicSys(who)
    local GenFile = Import("autocode/gen/magicsys/gen_exclusive_props.lua")
    GenFile.DoGen(who)
    local GenFile = Import("autocode/gen/magicsys/gen_stone.lua")
    GenFile.DoGen(who)
end

function GenZhuanShu(who)
	local GenFile = Import("autocode/gen/zhuanshu/gen_zhuanshu.lua")
	GenFile.DoGen(who)
end


function GenRewardRoll(who)
	local GenFile = Import("autocode/gen/reward/gen_reward_roll.lua")
	GenFile.DoGen(who)
end

function GenChatTitle(who)
	local GenFile = Import("autocode/gen/chattitle/gen_chat_title.lua")
	GenFile.DoGen(who)    
end

function GenChuangguan(who)
	local GenFile = Import("autocode/gen/chuangguan/gen_chuangguan.lua")
	GenFile.DoGen(who)    
end 

function GenFuncMgr(who)
    local GenFile = Import("autocode/gen/funcmgr/gen_func_mgr.lua")
    GenFile.DoGen(who)
end

--新手提示 by旭光
function GenNewBieData(who)
	local GenFile = Import("autocode/gen/newbie/gen_newbie_data.lua")
	GenFile.DoGen(who)
end

--剧情动画脚本  by旭光
function GenanimscriptData(who)
	local GenFile = Import("autocode/gen/animscript/gen_animscript_data.lua")
	GenFile.DoGen(who)
end

--神秘商人
function GenMysBusinessData(who)
	local GenFile = Import("autocode/gen/mysbusiness/gen_mysbusiness.lua")
	GenFile.DoGen(who)
end

function GetEsotericaData(who)
	local GenFile = Import("autocode/gen/esoterica/gen_esoterica_data.lua")
	GenFile.DoGen(who)
end

function GenMailData(who)
	local GenFile = Import("autocode/gen/mail/gen_mail_data.lua")
	GenFile.DoGen(who)
end

----Function 02/06/13 By Xxy----------------------------------
-- 战败提示
function GenFightTips(who)
	local GenFile = Import("autocode/gen/fighttips/gen_fighttips.lua")
	GenFile.DoGen(who)
end

--操作接口
Actions = 
{ 
	help = "help",
	genall = GenAll,
	
	--scene_data = GenSceneData, --生成场景数据与场景文件
	--scene_area = GenAreaData,  --生成场景区域
	--scene_tele = GenTele,      --生成跳转点
	--scene_npc  = GenSceneNpc,  --生成场景NPC
	scene_all  = GenAllScene,  --生成所有场景相关数据
	npc_data = GenNpcDat,      --生成NPC数据表与NPC文件
	equip_eff = GenEquipEff,   --
	
	--add by 彭彭
	cangbao_data = GenCangBaoDat, --生成藏宝图相关数据
	duilian_data = GenDuilianDat, --生成对穿肠相关数据
	ranklist = GenRankListDat,    --生成排行榜配置数据
	buypartner = GenBuyPartnerDat,--生成同伴招募数据
	yinglie = GenYingLieDat,	  --生成祭奠英烈数据
	mounts = GenMountsDat,		  --生成坐骑数据
	loginreward = GenLoginReward, --生成每日登陆
	username = GenUserName,		  --生成玩家姓名
	xiazhilu = GenXiaZhiLu,		  --生成侠之路
	everyday = GenEveryday,		  --生成每日推荐
	mingyang = GenMingYang, 	  --生成名扬天下
	effect = GenEffect,			  --生成主线特效
	saima = GenSaima,			  --生成田忌赛马
	sihai = GenSihai,			  --生成四海传奇
	gradereward = GenGradeReward, --生成功能开放预告
	jishou = GenJishou,			  --生成寄售行
	leatask = GenLeaTask,		  --生成盟会任务
	pata = GenPata,				  --生成爬塔
	orgfuben = GenOrgFuben,		  --生帮派副本
	qa = GenQA,					  --生成任务问题
	chongzhiroll = GenChongzhiRoll, --生成充值轮盘
	---------彭彭
	
	--fight_npc_data = GenFightNpcDat,      --生成战斗NPC数据表
	lineup = GenLineUpData,
	language = GenLanguageData,
	exp_data = GenExpData,		--生成经验表
	--talk_data = GenTalkData,	--生成闲话表
	--equip_data = GenEquipData,	--生成装备数据
	--buff_data = GenBuffData,	--生成Buff互斥表
	hero_data = GenHeroData,	--生成角色初始化数据
	--lineup_pos = GenLineUpPosData, --战斗阵位表
	--martial_data = GenMartialData, --生成武学属性表
	trunk_task_data = GenTrunkTaskData,  	--主线任务数据
	partner_data = GenPartnerData,		--生成同伴数据表
	
	--item_file = GenItemFile,  --物品文件生成
	--item_data = GenItemData,  --物品数据
	--goblin_data = GenGoblinData,  --野外怪数据表

	family_data = GenFamilyData,  --门派数据

	--potential_data = GenPotentialData,  --武器与人物资质对应数据
	martial_all = GenMartialAll,	--生成武学数据表，与招式文件
	--acc_data = GenAccPasswd,		--生成账号密码表,用于测试
	sell_data = GenSellData,--生成售卖NPC售卖的数据
	yun_biao_data = GenYunBiaoData, --生成运镖数据
	--bonus_data = GenBonusData,--生成奖励数据
	faq_data = GenFaqData, --侠者十问题库
	reward= GenReward,--生成奖励表数据
	reward2 = GenReward2,
	item_merge= GenItemMerge,--生成奖励表数据
	guide = GenGuideData,--生成指引数据
	battle = GenBattleData ,--NPC战斗配置数据
	dynamic_npc = GenDynamicNpcData, --动态战斗NPC配置数据
	worldboss = GenWorldBossData,--世界Boss数据
	martyr = GenMartyr,--祭奠英烈玩法数据
	xinmo = GenXinMoData,--心魔数据
	aidata = GenAIData,--AI数据
	item = GenItem,--Item各种数据
	item_prop = GenItemProp,--item prop数据
	taolin = GenTaolin, --桃林副本数据
	shengxiao = GenShengxiao, --生肖数据
	wiz = GenWizard, --生成巫师权限
	buff = GenBuff, --生成BUFF数据
	wulin = GenWulinData, --武林数据
	enyi = GenEnYiData, --恩义令数据
	shop = GenShopData,--商城数据
	--skilldata = GenSkillData, --招式数据
	--skillbuff = GenSkillBuff, --招式BUFF
	--partybonus = GenPartyBonus, --生成激活码
	branch_task = GenBranchTask, --生成支线任务
	gridcost = GenGridCost, --生成格子扩展费用
	fuben = GenFuBen, --生成副本数据
	itemxlprop = GenItemXlProp, --生成物品洗炼属性表
	itemxlupgrade = GenItemXlUpgrade, --生成物品洗炼表
	jibu = GenJiBu, --生成缉捕大盗的数据
	itemvalue = GenItemValue, --生成装备评分表
	itemstreng = GenItemStreng, --生成强化转移表
	vip = GenVip,
	giftbox = GenGiftBox, --生成宝箱
	vipshop = GenVipShop,	--vip商品
	activity = GenActivity, --生成活动数据
	itemkindprop = GenItemKindProp,	--生成物品分类属性表
	org = GenOrg,	--帮派数据
	league = GenLeague, --盟会数据
	achieve = GenAchieve, --生成成就数据
	title = GenTitle, --生成称号数据
	forbitlogin = GenForbitLogin, --禁止登录
	itemtzprop = GenItemTzProp, --套装属性
	suitupgrade = GenSuitUpgrade, --套装升阶
	gem = GenGemData, --宝石
	itemupgrade = GenItemUpgrade, --装备升阶
	timecontrol = GenTimeControl, --活动时间控制表
	skillzizhi = GenSkillZizhi,	--武学资质表
	carryshop = GenCarryData, --随身商店
	broadcast = GenBroadcast, --公告信息设置
	mingwang = GenMingWang, --名望
	shengwang = GenShengWang, --声望
	shouxi = GenShouXi, --首席弟子
	unionbattle = GenUnionBattle,	--纵横
	key = GenKey,
	jhtitle = GenJHTitle, --江湖名号
	treasure = GenTreasure, --聚宝盆
	signin = GenSignin, --每日签到
	newactivity = GenNewactivity, --7月开服活动
	mountsskin = GenMountsSkin, --坐骑换肤
	mergeserver = GenMergeBonus, --合服补偿
	danji = GenDanji,
	fieldboss = GenFieldBoss,   --野外boss
	leaguelunch = GenLeagueLunch, --盟会午餐
	leaguereward = GenLeaguereward, --午宴奖励
	orgbuilding = GenOrgBuilding, --帮派建筑
	orgtask = GenOrgTask, --帮派任务
	orgtalk = GenOrgTalk, --帮派任务对话
	bobin = GenBobin, --博饼活动
	zhongqiu = GenZhongQiulData, --中秋活动数据
	
	activedegree = GenActiveDegree,
	orgsnatch = GenOrgSnatch, --霸者之者数据
	leaguefight = GenLeagueFight, --帮战
	pay = GenPayRule, --扣费表格
	partybase = GenpartybaseData,  --	后台运营活动数据
	allparty = GenAllParty,
	fanhuan = GenFanhuanData, --返回元宝

	user_train = GenTrainData, --角色培养
	serverbattle = GenServerBattle, --跨服战数据

	zhuanshu = GenZhuanShu,
	rewardroll = GenRewardRoll,

	magicsys = GenMagicSys, --法宝系统
    unifyact = GenUnifyAct, --开服活动
    chattitle = GenChatTitle, --聊天标题
    chuangguan = GenChuangguan, --闯关副本
    funcmgr = GenFuncMgr,   --活动功能控制
  
    newbie = GenNewBieData, --新手提示
    animscript = GenanimscriptData, --剧情动画脚本数据
    mysbusiness = GenMysBusinessData, --神秘商人
    
    esoterica = GetEsotericaData,		--秘笈轮盘几率数据
    ridefashion = GenRidefashion , 		--生成坐骑皮肤，时装，称号相关静态属性
    orgwallfight = GenOrgWallFight,     --帮派城墙战
    fighttips = GenFightTips,			--战败提示
    mail = GenMailData,				--邮件配表 by旭光
}

local ActionKeys = table.keys(Actions)
table.sort(ActionKeys)
local USAGE = "Usage List:\n\t\t\t"..table.concat(ActionKeys, "\n\t\t\t")..'\n'

function GenAutoCode(who,s)
	if s == "help" then
		TellUser(who,USAGE)
		return
	end
	local File = Actions[s]
	if not File then
		TellUser(who,USAGE)
		return
	end
	if type(File) == 'function' then
		TellUser(who,string.format("Generating %s Start...",s))
		File(who,s)
		TellUser(who,string.format("Generating %s End...",s))
		return
	end
end

function MainGen(who,arg)
	local p = arg or "help"
	GenAutoCode(who,p)
end
