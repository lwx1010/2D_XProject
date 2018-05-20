mMAX_NUMBER = 2000000000 --protobuff int32 最大数
mSTRINGTYPE = type("string")	--字符串类型
mNUMBERTYPE = type(1)			--数字类型
mTABLETYPE =  type({})			--数组类型
mFUNCTYPE = type(function() end)

---------------------------------------------------------------------------------------------
--职业类型
PROFESSION_SWORD = 1	--剑
PROFESSION_LANCE = 2	--枪
PROFESSION_THORN = 3	--刺

--道具类型
ITEM_TYPE_NORMAL = 1		--物品
ITEM_TYPE_EQUIP  = 2		--装备

--道具子类型
ITEM_NORMAL_OTHER 		= 99	--其他物品
ITEM_NORMAL_RECOVER		= 1		--恢复类

---装备子类
EP_NULL			=	0		--未装备
EP_WEAPON		=	1		--武器
EP_RING			=   2		--戒指
EP_NECKLACE		=	3		--项链
EP_HEAD			=	4		--头盔
EP_ARMOR		=   5		--铠甲
EP_SHOULDER		=	6		--护肩
EP_BELT			=	7		--腰带
EP_SHOES		=	8		--鞋子
EP_TALISMAN		=	9		--护符
EP_MEDAL		=	10		--勋章
EP_FASHION		=	11		--时装
EP_WEDRING		=	12		--婚戒

--道具绑定
mUNBIND = 0	--未绑定
mBINDED = 1	--已绑定
--背包类型编号
mNORMAL_FRAME 		= 1		--材料
mMAJOREQUIP_FRAME	= 2		--主角装备

--问题答案
QUESTION_ANSWER_YES		= 1		--同意
QUESTION_ANSWER_NO		= 2		--拒绝
QUESTION_ANSWER_CANCEL  = 3		--取消

--问题对话框类型
QUESTION_TYPE_NORMAL    = 100 		--普通弹出框

--NPC对话回答按钮类型
NPCTALK_BTN_TYPE0 = 0				--没有确认,没有取消
NPCTALK_BTN_TYPE1 = 1				--有确认,没有取消
NPCTALK_BTN_TYPE2 = 2				--没有确认,有取消
NPCTALK_BTN_TYPE3 = 3				--有确认,有取消
NPCTALK_BTN_TYPE4 = 4				--下一个对话
--NPC对话回答类型
NPCTALK_ANS_TYPE0 = 0				--是
NPCTALK_ANS_TYPE1 = 1				--否
NPCTALK_ANS_TYPE2 = 2				--下一个对话
NPCTALK_ANS_TYPE3 = 3				--公用的
NPCTALK_ANS_TYPE4 = 4				--公用的
NPCTALK_ANS_TYPE5 = 5				--公用的

--奖励标签
REWARD_TAG_CASH = "cash"				--银两
REWARD_TAG_EXP = "exp"					--角色经验
REWARD_TAG_EXP_PARTNER = "exp_partner"	--随从经验
REWARD_TAG_ITEM = "item"				--道具
REWARD_TAG_PARTNER = "partner"			--随从
REWARD_TAG_VIGOR = "vigor"				--精力
REWARD_TAG_PHYSICAL = "physical"		--体力
REWARD_TAG_BINDYUANBAO = "bindyuanbao"	--绑定元宝
REWARD_TAG_YUANBAO = "yuanbao"			--元宝

REWARD_TAG_TBL = {
	[REWARD_TAG_CASH] = mNUMBERTYPE,
	[REWARD_TAG_EXP] = mNUMBERTYPE,
	[REWARD_TAG_EXP_PARTNER] = mNUMBERTYPE,
	[REWARD_TAG_ITEM] = mTABLETYPE,
	[REWARD_TAG_PARTNER] = mTABLETYPE,
	[REWARD_TAG_VIGOR] = mNUMBERTYPE,
	[REWARD_TAG_PHYSICAL] = mNUMBERTYPE,
	[REWARD_TAG_BINDYUANBAO] = mNUMBERTYPE,
	[REWARD_TAG_YUANBAO] = mNUMBERTYPE,
}

----------------------------聊天系统-------------------------
--玩家发送信息给玩家
WORLD_CHANNEL 		= 	1		--世界
CLUB_CHANNEL		= 	2		--公会
TEAM_CHANNEL 		= 	3		--组队
PRIVATE_CHANNEL		=	4		--私聊
AREA_CHANNEL		=	5		--区域
CAMP_CHANNEL		=	8		--阵营
ALL_CHANNEL			=	9		--综合

--系统发送信息给玩家
SYS_ROLL 				= 1			--系统广播（上屏跑马灯）
SYS_PROMT_BOX 			= 2			--提示信息（中屏渐变,常用(UserObj:Notify(msg))）
SYS_DIALOG 				= 3			--消息框,需要确认消失
SYS_SYSTEM				= 4			--系统信息
SYS_NOTICE 				= 5			--公告信息
SYS_NOTICE_SPEC			= 6			--特殊系统信息(主界面消息提示, 如夫妻上下线)

LOG_ITEM_TBL = {
    [mNORMAL_FRAME] 	= "item_logchange",
    [mMAJOREQUIP_FRAME] = "item_logchange",
}

--地图状态:
S_MAP_NOT 	= 1					--不在地图
S_MAP_ADD	= 2					--在进入
S_MAP_IN	= 3					--在地图
S_MAP_LEAVE	= 4					--在离开地图

--默认地图
MAP_START = 1001				--新手村
MAP_NORMAL = 1001				--默认地图
CONVEY_DOOR_DIS = 10			--传送门触发距离

--与npc对话的xy距离
NPCTALK_DISTANCE = 5		

HEARTBEAT_SOCKET = 5		--客户端心跳时间

--技能主动 被动类型
SKILL_TYPE_ACTIVE = 1
SKILL_TYPE_PASSIVE = 2

---------------重连字段
IS_RELOGIN = "relogin"

---------------攻击类型
ATTACK_TYPE_PHYSICAL 	= 1		--物理
ATTACK_TYPE_MAGIC		= 2		--魔法

--------------邮件类型
MAIL_KIND_SYSTEM = 1	--系统
MAIL_KIND_USER   = 2	--玩家

--------------邮件读后是否删除
READ_DEL	=	1				--读后就删
READ_UN_DEL =	0				--读后不删

----------------boss类型
BOSS_TYPE_WORLD 		= 1			--世界boss
BOSS_TYPE_BOX			= 2			--箱子

----------------npc默认巡逻范围
NPC_PATROLRANGE = 5

----------------跳转场景类型
CHANGE_MAP_TYPE_FLY 	= 1			--飞鞋
CHANGE_MAP_TYPE_CONVEY	= 2			--传送点

----------------装备品阶颜色
ITEM_COLOR_GREEN 	= 1				--一阶绿装(不投放)
ITEM_COLOR_BLUE		= 2				--二阶蓝装
ITEM_COLOR_PURPLE 	= 3				--三阶紫装
ITEM_COLOR_GOLD 	= 4				--四阶金装(不投放)
ITEM_COLOR_ORANGE 	= 5				--五阶橙装	

----------------地图类型
SCENE_TYPE = {				-- 注意: loadnmesh.lua因为preload需要等待http返回加载, 所以没有用到而直接使用2来判断客户端副本
	CITY = 1,						-- 主城类场景
	CLIENT_FUBEN = 2,				-- 客户端副本
	SERVER_FUBEN = 3,				-- 服务端副本
}
