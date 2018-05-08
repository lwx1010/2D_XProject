
local GameConst = {}

--主角性别
GameConst.HERO_SEX = {
	MALE = 1,
	FEMALE = 2,
}

--奖励类型
GameConst.REWARD_TAG = {
	REWARD_TAG_CASH = "cash",				--银两
	REWARD_TAG_EXP = "exp",					--角色经验
	REWARD_TAG_EXP_PARTNER = "exp_partner",	--随从经验
	REWARD_TAG_ITEM = "item",				--道具
	REWARD_TAG_PARTNER = "partner",			--随从
	REWARD_TAG_VIGOR = "vigor",				--精力
	REWARD_TAG_PHYSICAL = "physical",		--体力
	REWARD_TAG_BINDYUANBAO = "bindyuanbao",	--绑定元宝
	REWARD_TAG_SHENGWANG = "shengwang",   	--声望
	REWARD_TAG_YUANBAO = "yuanbao",         --元宝

	REWARD_TAG_JJCHONOR = "jjchonor",		--荣誉
	REWARD_TAG_LILIAN = "lilian",			--历练
	REWARD_TAG_GONGXUN = "gongxun",			--功勋
	REWARD_TAG_CLUBDONATE = "clubdonate",	--帮贡
	REWARD_TAG_LINGQI = "lingqi", 			--灵气
}

--pk模式
GameConst.PKMODE = {
	PKMODE_PEACE 	= 0,		--和平
	PKMODE_WHOLE 	= 1,		--全体
	PKMODE_TEAM 	= 2,		--队伍
	PKMODE_CLUB 	= 3,		--帮派
	PKMODE_EVIL 	= 4,		--善恶
	PKMODE_SERVER 	= 5,		--本服
	PKMODE_HOSTILECLUB = 6,		--敌帮
}

--技能面板类型：1.主动,2.神器,3.怒气,4.轻功,5.被动, 6.打坐
GameConst.MartialUiType = {
	MARTIAL_UI_ZHUDONG = 1,
	MARITAL_UI_SHENQI = 2,
	MARITAL_UI_NUQI = 3,
	MARTIAL_UI_QINGGONG = 4,
	MARTIAL_UI_PASSIVE = 5,
	MARTIAL_UI_DAZUO = 6,
}

--道具品阶框
GameConst.ItemRareColor = {
	"green",
	"blue",
	"purple",
	"orange",	--金色
	"golden",	--红色
}

--副本小类型 1 进阶副本、2 经验副本、3 章节副本、4 会员副本、5 江湖试炼、6 神器副本、7 武林试炼、8 注灵副本、9 帮派副本
GameConst.FuBenFType = {
	TYPE_JINJIE = 1,
	TYPE_JINGYAN = 2,
	TYPE_ZHANGJIE = 3,
	TYPE_VIP = 4,
	TYPE_JIANGHU = 5,
	TYPE_SHENQI = 6,
	TYPE_WULIN = 7,
	TYPE_ZHULING = 8,
	TYPE_BANGHUI = 9,
	TYPE_MULTI = 10,
	TYPE_XIANJUE = 11,
	TYPE_LINGYUAN = 12,
}

GameConst.RoleState = {
	None = 0,
	Jumping = 1,
	SkillActing = 2,
	SkillMoving = 4,
	Fight = 8,
	Dead = 16,
	-- Dazuo = 32,
	-- Guaji = 64,
	-- OpenUI = 128,
	-- PathComputeComplete = 256,
	-- Dead = 512,
	-- FlyShoe = 1024,
	-- Husong = 2048,
	-- ProtectFollow = 4096,
	-- ShowGuide = 8192,
	-- Task = 16384,
	-- Caiji = 32768,
	-- Activity = 65536,
	-- DigTreasure = 131072,
}


-----------------系统设置
GameConst.Setting_Default_MainRoleLevel = 20  -- 20级的限制，20以前屏蔽友方及其它玩家

return GameConst