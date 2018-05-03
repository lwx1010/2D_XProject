local ItemConst = {}
local this = ItemConst

ItemConst.RARE_NAMES = {
	[1] = 'ui_box_green',
	[2] = 'ui_box_blue',
	[3] = 'ui_box_purple',
	[4] = 'ui_box_orange',
	[5] = 'ui_box_golden',
}

ItemConst.RARE_COLORS = {
	[1] = '13af3f',
	[2] = '1f93d6',
	[3] = 'be28cc',
	[4] = 'e86800',
	[5] = 'ff0000',
}

ItemConst.RARE_COLORS_DARK = {
	[1] = 'a7ff77',
	[2] = '2bd5ff',
	[3] = 'f161ff',
	[4] = 'ff9946',
	[5] = 'ff5050',
}

ItemConst.REWARD_TAG_CASH = "cash"				--银两
ItemConst.REWARD_TAG_EXP = "exp"					--角色经验
ItemConst.REWARD_TAG_EXP_PARTNER = "exp_partner"	--随从经验
ItemConst.REWARD_TAG_ITEM = "item"				--道具
ItemConst.REWARD_TAG_PARTNER = "partner"			--随从
ItemConst.REWARD_TAG_VIGOR = "vigor"				--精力
ItemConst.REWARD_TAG_PHYSICAL = "physical"		--体力
ItemConst.REWARD_TAG_BINDYUANBAO = "bindyuanbao"	--绑定元宝
ItemConst.REWARD_TAG_SHENGWANG = "shengwang"		--声望
ItemConst.REWARD_TAG_JJCHONOR = "jjchonor"		--荣誉
ItemConst.REWARD_TAG_LILIAN = "lilian"			--历练
ItemConst.REWARD_TAG_GONGXUN = "gongxun"			--功勋
ItemConst.REWARD_TAG_CLUBDONATE = "clubdonate"	--帮贡
ItemConst.REWARD_TAG_YUANBAO = "yuanbao"        --元宝


ItemConst.ITEMNO_BINDYUANBAO = 10102001
ItemConst.ITEMNO_CLUBDONATE = 10199001
ItemConst.ITEMNO_SHENGWANG = 10199014
ItemConst.ITEMNO_JJCHONOR = 10199015
ItemConst.ITEMNO_GONGXUN = 10199040
ItemConst.ITEMNO_LILIAN = 10199025
ItemConst.ITEMNO_YUANBAO = 10102999

--道具类型
ItemConst.ITEM_TYPE_NORMAL = 1		--物品
ItemConst.ITEM_TYPE_EQUIP  = 2		--装备
ItemConst.ITEM_TYPE_MANUAL = 3		--图鉴
ItemConst.ITEM_TYPE_JEWEL  = 4		--宝石

return ItemConst