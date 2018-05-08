PLAYER_TYPE 		= 1
NPC_TYPE 			= 2
ITEM_TYPE 			= 3
PARTNER_TYPE		= 4		

CHANGE_MAPPOS_ADD	= 1			--加入地图
CHANGE_MAPPOS_MOVE	= 2			--移动
CHANGE_MAPPOS_LEAVE = 3			--离开地图

BATTLE_CAMP_1 = 1				--混乱阵营
BATTLE_CAMP_2 = 2				--秩序阵营

NORMAL_SLOPE 		= 0.8
NORMAL_NPC_RADIUS 	= 10			--普通npc视野
CAN_ATTACK_RADIUS 	= 12			--不能比8大,因为一个格子的宽度最小是8									注意，玩家攻击的时候需要判断

CHAR_TYPE_TBL = {
	[PLAYER_TYPE] = true,
	[NPC_TYPE] = true,
	[ITEM_TYPE] = true,
	[PARTNER_TYPE] = true,
}

--          ^ +y
-- 6  	    5          4
--          |
--          |            +x
---7-------------------3-->
--		  	|           
--		  	|
-- 8	    1          2
MOVE_DIR = {
	{0,-1},			--1
	{1,-1},			--2
	{1,0},			--3
	{1,1},			--4
	{0,1},			--5
	{-1,1},			--6
	{-1,0},			--7
	{-1,-1},		--8
}

MOVE_ADJUST = {			--目标pos-当前pos
	[1] = {
		[1] = {MOVE_DIR[8], MOVE_DIR[1], MOVE_DIR[7], MOVE_DIR[2], MOVE_DIR[6], MOVE_DIR[3], MOVE_DIR[5], MOVE_DIR[4]},
		[0] = {MOVE_DIR[7], MOVE_DIR[8], MOVE_DIR[6], MOVE_DIR[1], MOVE_DIR[5], MOVE_DIR[2], MOVE_DIR[4], MOVE_DIR[3]},
		[-1] = {MOVE_DIR[6], MOVE_DIR[7], MOVE_DIR[5], MOVE_DIR[8], MOVE_DIR[4], MOVE_DIR[1], MOVE_DIR[3], MOVE_DIR[2]},
	},
	[0] = {
		[1] = {MOVE_DIR[1], MOVE_DIR[2], MOVE_DIR[8], MOVE_DIR[3], MOVE_DIR[7], MOVE_DIR[4], MOVE_DIR[6], MOVE_DIR[5]},
		[-1] = {MOVE_DIR[5], MOVE_DIR[6], MOVE_DIR[4], MOVE_DIR[3], MOVE_DIR[7], MOVE_DIR[2], MOVE_DIR[8], MOVE_DIR[1]},
	},
	[-1] = {
		[1] = {MOVE_DIR[2], MOVE_DIR[3], MOVE_DIR[1], MOVE_DIR[4], MOVE_DIR[8], MOVE_DIR[5], MOVE_DIR[7], MOVE_DIR[6]},
		[0] = {MOVE_DIR[3], MOVE_DIR[4], MOVE_DIR[2], MOVE_DIR[5], MOVE_DIR[1], MOVE_DIR[6], MOVE_DIR[8], MOVE_DIR[7]},
		[-1] = {MOVE_DIR[4], MOVE_DIR[5], MOVE_DIR[3], MOVE_DIR[2], MOVE_DIR[6], MOVE_DIR[1], MOVE_DIR[7], MOVE_DIR[8]},
	}
}

Z_ACCURATE = 0.11				--z轴精确度

--地图类型
MAP_FIGNT = 1

---------------------------死亡类型
DIE_WORLDBOSS = 1;
DIE_RANDOMBOSS = 2;

---------------------------复活类型
RELIVE_WORLDBOSS = 1;
RELIVE_RANDOMBOSS = 2;
RELIVE_FORBIDDENPVE = 3;

ACTIVE_NPC = 1

-------------------------------------------------------------ai----------------------------------------------------------------------
--radical2_2 = 0.7			--根号2/2
c_nBlockCnt = 1
c_nBlockMaxCnt = 3
c_nHaltCnt = 40				--追随者在被追随者旁边的停留步数c_nHaltCnt后移动一下
c_nSleepCnt = 3				--npc周围走的停止步数

--AI,EVENT状态--
AI_CONTINUE 		= 1
AI_NEXT 			= 2
AI_EXCEPTION 		= 3
AI_CANCELTARGET		= 4
AI_NULL 			= 5

EVENT_BEATTACK		= 1					--被攻击
EVENT_TOATTACK		= 2					--指使追随者(例如宠物)攻击

AI_WALK_TIME 		= 30
AI_ATTACK_TIME 		= 36
AI_DESTROY_TIME 	= 30

SPEED_SLOW			= 3.5
SPEED_NORMAL		= 7
SPEED_FAST			= 14

SPEED_TIME = {
	[SPEED_SLOW] = 4,
	[SPEED_NORMAL] = 2,
	[SPEED_FAST] = 1,
}

DISTANCE_ADJUST_SPEED = {
	[1] = function(speed)
		return speed / 1.4
	end,
	[2] = function(speed)
		return speed
	end,
	[4] = function(speed)
		return speed / 1.4
	end,
	[5] = function(speed)
		return speed / 1.4
	end,
	[8] = function(speed)
		return (speed / 1.4 + speed) / 2
	end,
}

--地图死亡状态
DIE_STATE_GHOST = 1				--死亡为幽灵状态

--战斗自动的状态
AUTOFIGHT_TYPE_NORMAL = 0		--手动攻击
AUTOFIGHT_TYPE_ATTACK = 1		--自动攻击
AUTOFIGHT_TYPE_SKILL = 2		--自动技能

-------------------------------------------------------------战斗属性------------------------------------------------------
FIGHT_SHOW_HP 		= "ShowHp"
FIGHT_SHOW_ADDBUFF 	= "ShowABuff"

SKILL_ALL_COOLTIME = 0.3						--秒数

--伤害类型
HURT_TYPE_00 	= 0 	--普通伤害
HURT_TYPE_01	= 1		--暴击伤害
HURT_TYPE_02	= 2		--破格伤害
HURT_TYPE_03	= 3		--暴击破格伤害
HURT_TYPE_04	= 4		--miss
HURT_TYPE_05 	= 5		--无敌
HURT_TYPE_06	= 6		--非攻击技能加血
HURT_TYPE_07	= 7		--添加buff
HURT_TYPE_08	= 8		--跳闪
HURT_TYPE_09	= 9		--buff加减血
HURT_TYPE_10	= 10	--打坐加血
HURT_TYPE_11	= 11	--护盾伤害
HURT_TYPE_12	= 12	--隐身无敌伤害
HURT_TYPE_13	= 13	--变身血量

--战斗指令
FIGHT_PHYSIC_ATTACK 	= 1 	--物理攻击
FIGHT_MAGIC_ATTACK 		= 2 	--法术攻击

SKILL_MTYPE_NORMAL = 1			--普通
SKILL_MTYPE_MAGIC = 2			--技能
SKILL_MTYPE_HETIJI = 3			--合体技

SKILL_TYPE_INITIATIVE = 1		--主动技能
SKILL_TYPE_PASSIVE = 2			--被动技能

PANELSKILL_TYPE_INITIATIVE 	= 1	--主动技能
PANELSKILL_TYPE_ARTIFACT 	= 2	--神器技能
PANELSKILL_TYPE_ANGER 		= 3	--怒气技能
PANELSKILL_TYPE_FLY			= 4	--轻功技能
PANELSKILL_TYPE_PASSIVE		= 5	--被动技能
PANELSKILL_TYPE_OTHPASSIVE	= 6 --其他被动技能

BUFF_DIZZINESS 		= 7			--眩晕
BUFF_BIND			= 8			--缠绕
BUFF_FREEZE			= 9			--冰冻
BUFF_PETRIFACTION	= 10		--石化
BUFF_INVINCIBLE		= 11		--无敌
BUFF_IMM_CONTROL	= 12		--免疫控制类
BUFF_IMM_DEBUFF		= 13		--免疫debuff类
BUFF_IMM_CONTROL2	= 40000501	--免疫控制类
BUFF_IMM_DEBUFF2	= 40000511	--免疫debuff类

BUFF_TYPE1 = 1					--控制类
BUFF_TYPE2 = 2					--减益
BUFF_TYPE3 = 3					--增益
BUFF_TYPE4 = 4					--特殊
BUFF_TYPE5 = 5					--非战斗类型
BUFF_TYPE6 = 6					--变羊
BUFF_TYPE7 = 7					--无敌
BUFF_TYPE8 = 8					--隐身buff
BUFF_TYPE9 = 9					--变身buff
BUFF_TYPE10 = 10				--任务加速buff
BUFF_TYPE11	= 11				--免疫眩晕buff

BUFF_SUBTYPE1 = 1				-- 控制类中的眩晕类buff

SP_MAX = 500
COST_SP_TYPE = 1

SINGLE_TARGET_TYPE = 100			--单体攻击
MULTI_TARGET_TYPE = 101				--多体攻击

SKILL_SHAPE_LINE = 1				--技能攻击范围是线
SKILL_SHAPE_CIRCLE = 2				--技能攻击范围是圆
SKILL_SHAPE_SECTOR = 3				--技能攻击范围是扇形
SKILL_SHAPE_ALL = 4					--如果是玩家则所有随从和玩家,如果是npc则是所有npc
SKILL_ATTAREA_MAX = 30				--最大攻击范围半径为20

FIGHT_EFF_NAME 		= "feff"				--战斗临时属性
FIGHT_STILLEFF_NAME = "fseff"				--战斗临时持续属性

--主动技能
CMD_DO 			= "cmd_do"
CMD_DO_HIT		= "cmd_do_hit"
--被动技能
PASS_HP			= "pass_hp"			--当自身hp低于，（注意:敌方的不判断）
PASS_BATTLE		= "pass_battle"		--当进入战斗模式
PASS_DO			= "pass_do"			--指令执行阶段
PASS_SDO		= "pass_sdo"		--技能指令执行阶段
PASS_HIT		= "pass_hit"		--命中敌人时
PASS_SHIT		= "pass_shit"		--技能命中敌人时(前一个包含普通和合体技能)
PASS_BEHIT		= "pass_behit"		--受击时
PASS_DIE		= "pass_die"		--死亡的时候
PASS_DAPPSHIELD	= "pass_dappshield"	--护盾消失时
OVER_PASSHIT	= "over_passhit"	--叠加被动技能触发
PASS_RELIVE		= "pass_relive"		--复活时机
PASS_ADDBUFF	= "pass_addbuff"	--添加buff时机
CMD_FUBEN_S		= "cmd_fuben_s"		--进入副本

mFIGHT_Id 			= 	"Id"			--战士ID
mFIGHT_Grade		= 	"Grade"			--战士等级
mFIGHT_Name			=	"Name"			--名字
mFIGHT_Ap			=	"Ap"         	--攻击	
mFIGHT_Dp			=	"Dp"         	--防御
mFIGHT_Ma			=	"Ma"			--魔攻
mFIGHT_Mr			=	"Mr"			--魔防
mFIGHT_Hp			=	"Hp"         	--生命	
mFIGHT_HpMax		=	"HpMax"	  		--最大生命
mFIGHT_Speed		=	"Speed"      	--行动速度	

mFIGHT_HitRate		=	"HitRate"    	--命中	
mFIGHT_Dodge		=	"Dodge"      	--躲避

mFIGHT_Double		=	"Double"	    --暴击几率 
mFIGHT_Tenacity		=	"Tenacity"	    --暴击抗性 
mFIGHT_Parry		=	"Parry"			--格挡
mFIGHT_ReParry		=	"ReParry"		--破格

mFIGHT_HitOk		=   "HitOk"         --必定命中
mFIGHT_AbsHurt		=	"AbsHurt"		--绝对伤害
mFIGHT_SkillRate	=	"SkillRate"		--技能系数
mFIGHT_DoubleHurt	=	"DoubleHurt"	--暴击伤害
mFIGHT_ReDoubleHurt	=	"ReDoubleHurt"	--暴击减免
mFIGHT_Hurt			=	"Hurt"			--伤害加成
mFIGHT_ReHurt		=	"ReHurt"		--伤害减免

mFIGHT_AbsDouble	=	"AbsDouble"		--必定暴击
mFIGHT_DoubleOtherHurt = "DoubleOtherHurt"	--暴击伤害减少相加

mFIGHT_PartnerAp		=	"PartnerAp"			--伙伴攻击
mFIGHT_PartnerHurt		=	"PartnerHurt"		--伙伴伤害
mFIGHT_PartnerReHurt	=	"PartnerReHurt"		--伙伴伤害减免
mFIGHT_ReSlow			=	"ReSlow"			--减速免疫
mFIGHT_ReDizzy			=	"ReDizzy"			--眩晕免疫
mFIGHT_ReBleed			=	"ReBleed"			--流血免疫
mFIGHT_PartnerExtraHurt	=	"PartnerExtraHurt"	--伙伴额外伤害
mFIGHT_MagicAp		=	"MagicAp"		--灵器攻击
mFIGHT_MagicHurt	=	"MagicHurt"		--灵器伤害


mFIGHT_MovePush				=	"MovePush"			--推
mFIGHT_MovePull				=	"MovePull"			--拉
mFIGHT_AddSelfHurt			=	"AddSelfHurt"		--增加自身伤害
mFIGHT_AddOtherHurt 		= 	"AddOtherHurt"		--别人增加伤害
mFIGHT_SubSelfHurt			=	"SubSelfHurt"		--自身减少伤害
mFIGHT_SubOtherHurt			=	"SubOtherHurt"		--别人减少伤害
mFIGHT_ReHpRate				=	"ReHpRate"			--回血技能增加比率效果
mFIGHT_ReHpValue			=	"ReHpValue"			--回血技能添加值效果
mFIGHT_SubReHpRate			=	"SubReHpRate"		--回血技能减少比率效果
mFIGHT_SubReHpValue			=	"SubReHpValue"		--回血技能减少值效果
mFIGHT_BackHurtRate			=	"BackHurtRate"		--受到攻击的x%返还给攻击者
mFIGHT_DelBuffType			=	"DelBuffType"		--驱散某种类型的buff
mFIGHT_AbsorbHpRateByHurt	= 	"AbsorbHpRateByHurt"--按照伤害的X%比例吸血
mFIGHT_AbsorbHpRateByHpMax	=	"AbsorbHpRateByHpMax" --按照最大血量的X%比例吸血
mFIGHT_ExtraSkill			=	"ExtraSkill"		--额外获得一个技能
mFIGHT_Shield				=	"Shield"			--护盾
mFIGHT_WarBowman			=	"WarBowman"			--战士克制弓手		万分比
mFIGHT_BowmanWizard			=	"BowmanWizard"		--弓手克制法师		万分比
mFIGHT_WizardWar			=	"WizardWar"			--法师克制战士		万分比
mFIGHT_AddNpcProp			=	"AddNpcProp"		--增加npc属性
mFIGHT_ControlCUseSkill		=	"ControlCUseSkill"	--控制类是否可以出某技能
mFIGHT_NotDie				=	"NotDie"			--不死
mFIGHT_STILLPROPHP			=	"StillPropByHp"		--属性持续到血量
mFIGHT_FixedShield			=	"FixedShield"		--固定护盾
mFIGHT_FixedShieldAddHurt	=	"FixedShieldAddHurt"--固定护盾每次加伤害
mFIGHT_FixedShieldSubHurt	=	"FixedShieldSubHurt"--固定护盾每次减伤害
mFIGHT_FrameRate			=	"FrameRate"			--正数伤害系数
mFIGHT_FixedShieldHpMax		=	"FixedShieldHpMax"	--最大固定伤害
mFIGHT_FixedHpHurt			=	"FixedHpHurt"		--固定伤害

mFIGHT_AtkTime 				=	"AtkTime"			--攻击时间间隔

mFIGHT_AddSelfHurtRate		=	"AddSelfHurtRate"		--增加自身伤害	万分比
mFIGHT_AddOtherHurtRate		= 	"AddOtherHurtRate"		--别人增加伤害	万分比
mFIGHT_SubSelfHurtRate		=	"SubSelfHurtRate"		--自身减少伤害	万分比
mFIGHT_SubOtherHurtRate		=	"SubOtherHurtRate"		--别人减少伤害	万分比

mFIGHT_FixedAddSelfHurtRate		=	"FixedAddSelfHurtRate"		--增加自身伤害	万分比
mFIGHT_FixedAddOtherHurtRate	= 	"FixedAddOtherHurtRate"		--别人增加伤害	万分比
mFIGHT_FixedSubSelfHurtRate		=	"FixedSubSelfHurtRate"		--自身减少伤害	万分比
mFIGHT_FixedSubOtherHurtRate	=	"FixedSubOtherHurtRate"		--别人减少伤害	万分比

mFIGHT_ResetHpMax			=	"ResetHpMax"			--重置最大血量
mFIGHT_OldResetHpMax		=	"OldResetHpMax"			--旧的最大血量

mFIGHT_ExchangeMartial		=	"ExchangeMartial"		--重置技能
mFIGHT_OldExchangeMartial	=	"OldExchangeMartial"	--旧的技能

mFIGHT_FixedHurtRateAvoid	=	"FixedHurtRateAvoid"	--免疫固定伤害
mFIGHT_RoundAddBuff			=	"RoundAddBuff"			--给周围类型加buff

mFIGHT_AddBuffProp			=	"AddBuffProp"			--给buff添加属性

mFIGHT_NotHitOk				=   "NotHitOk"              --必定不命中

--增加属性的名字
WARRIOR_MIRROR_FRATE_NAME = {
	mFIGHT_Ap,
	mFIGHT_Dp,
	mFIGHT_Ma,
	mFIGHT_Mr,
	mFIGHT_Hp,
	mFIGHT_HpMax,
	
	mFIGHT_HitRate,
	mFIGHT_Dodge,
	
	mFIGHT_Double,
	mFIGHT_Tenacity,
	mFIGHT_Parry,
	mFIGHT_ReParry,
	
	mFIGHT_DoubleHurt,
	mFIGHT_ReDoubleHurt,
	
	mFIGHT_Hurt,
	mFIGHT_ReHurt,
	mFIGHT_AbsHurt,
}

WARRIOR_DATA_NAME = {						--战斗需要的属性
	mFIGHT_Id,
	mFIGHT_Grade,
	mFIGHT_Name,
	mFIGHT_Ap,
	mFIGHT_Dp,
	mFIGHT_Ma,
	mFIGHT_Mr,
	mFIGHT_Hp,
	mFIGHT_HpMax,
	mFIGHT_Speed,
	
	mFIGHT_HitRate,
	mFIGHT_Dodge,
	
	mFIGHT_Double,
	mFIGHT_Tenacity,
	mFIGHT_Parry,
	mFIGHT_ReParry,
	
	mFIGHT_DoubleHurt,
	mFIGHT_ReDoubleHurt,
	
	mFIGHT_Hurt,
	mFIGHT_ReHurt,
	mFIGHT_AbsHurt,
	
	mFIGHT_AtkTime,
	
	mFIGHT_PartnerAp,
	mFIGHT_PartnerHurt,
	mFIGHT_PartnerReHurt,
	mFIGHT_ReSlow,
	mFIGHT_ReDizzy,
	mFIGHT_ReBleed,
	mFIGHT_PartnerExtraHurt,
	mFIGHT_MagicAp,
	mFIGHT_MagicHurt,
}

WARRIOR_NOTPARTNER_DATA_NAME = {
	mFIGHT_Ap,
	mFIGHT_Dp,
	mFIGHT_Ma,
	mFIGHT_Mr,
	mFIGHT_Hp,
	mFIGHT_HpMax,
	mFIGHT_Speed,
	
	mFIGHT_HitRate,
	mFIGHT_Dodge,
	
	mFIGHT_Double,
	mFIGHT_Tenacity,
	mFIGHT_Parry,
	mFIGHT_ReParry,		
	
	mFIGHT_DoubleHurt,
	mFIGHT_ReDoubleHurt,
	
	mFIGHT_Hurt,
	mFIGHT_ReHurt,
	mFIGHT_AbsHurt,
}

WARRIOR_BUFFPROP_NAME = {
	[mFIGHT_Ap] = true,
	[mFIGHT_Dp] = true,
	[mFIGHT_Ma] = true,
	[mFIGHT_Mr] = true,
	[mFIGHT_Hp] = true,
	[mFIGHT_HpMax] = true,
	[mFIGHT_Speed] = true,
	
	[mFIGHT_HitRate] = true,
	[mFIGHT_Dodge] = true,
	
	[mFIGHT_Double] = true,
	[mFIGHT_Tenacity] = true,
	[mFIGHT_Parry] = true,
	[mFIGHT_ReParry] = true,		
	
	[mFIGHT_DoubleHurt] = true,
	[mFIGHT_ReDoubleHurt] = true,
	
	[mFIGHT_Hurt] = true,
	[mFIGHT_ReHurt] = true,
	[mFIGHT_AbsHurt] = true,
	[mFIGHT_DoubleOtherHurt] = true,
	[mFIGHT_Shield] = true,
	
	[mFIGHT_AbsHurt] = true,
	[mFIGHT_DoubleHurt] = true,
	[mFIGHT_ReDoubleHurt] = true,
	[mFIGHT_Hurt] = true,
	[mFIGHT_ReHurt] = true,
	[mFIGHT_DoubleOtherHurt] = true,
}

function EliteVarFunc(grade)
	if grade <= 30 then
		return 500
	else
		return 500 + 20 * (grade - 30)
	end
end

-------------------变羊的行走范围
SHEEP_RANGE = 2
-------------------是否看到别人
NOT_SEE_OTHER = 1

-------------------pk模式
PKMODE_PEACE 		= 0		--和平
PKMODE_WHOLE 		= 1		--全体
PKMODE_TEAM 		= 2		--队伍
PKMODE_CLUB 		= 3		--帮派
PKMODE_EVIL 		= 4		--善恶
PKMODE_SERVER 		= 5		--本服
PKMODE_HOSTILECLUB	= 6		--敌对帮派

PK_MODE_CHECK = {
	[PKMODE_PEACE] = true,
	[PKMODE_WHOLE] = true,
	[PKMODE_TEAM] = true,
	[PKMODE_CLUB] = true,
	[PKMODE_EVIL] = true,
	[PKMODE_SERVER] = true,
	[PKMODE_HOSTILECLUB] = true,
}

-------------------善恶状态
EVILSTATE_WHITE 	= 0		--白名
EVILSTATE_YELLOW 	= 1		--黄名
EVILSTATE_RED 		= 2		--红名

-------------------善恶时间
EVIL_COOLTIME		= 300
EVIL_KILLVALUE		= 10			--击杀一个人的善恶值增加多少
EVIL_BUFFID			= 100005
EVIL_BUFFID_CLIENT	= 100006
EVIL_REDVALUE		= 40
EVIL_KILLNPC_CNT	= 60

--可以被击退
CAN_PUSH = 1

PARTNER_HITTYPE = 1
MAGIC_HITTYPE = 2

MAX_FIYTIMENO = 100
-------------------移动状态
MOVETYPE_SPRINT	= 1		--冲刺
MOVETYPE_FLY1	= 2		--跳跃1
MOVETYPE_FLY2	= 3		--跳跃2
MOVETYPE_FLY3	= 4		--同步状态
MOVETYPE_FLYF	= 5		--跳跃结束
MOVETYPE_PJUMP	= 6		--剧情跳

-------------------返回场景逻辑返回主逻辑信息
RETMAP_TYPE_NPCDIE_REWARD 	= 1		--npc死亡奖励
RETMAP_MARTIALEXP			= 2		--技能熟练度
RETMAP_FLYDODGE				= 3		--跳闪值
RETMAP_NOTFIGHT_BUFFHP		= 4		--血包数值
RETMAP_RETHP				= 5		--血量
RETMAP_TYPE_NPCDIE_TASK 	= 6		--任务npc死亡
RETMAP_WORLDBOSS_DELBUFF	= 7		--世界boss 护盾破灭
RETMAP_PKMODE_KILL			= 8 	--战斗模式杀死玩家
RETMAP_DAZUO				= 9 	--打坐
RETMAP_CLUBBOSS_DELBUFF		= 10	--帮派boss 护盾破灭
RETMAP_SP					= 11	--保存sp
RETMAP_DOUBLE_XIULIAN		= 12	--双修
RETMAP_SHIELDBUFF			= 13	--护盾信息
RETMAP_CLUBDIDUI			= 14 	--击杀敌对帮派玩家
RETMAP_YEWAIBOSS_REWARD		= 15	--野外boss奖励
RETMAP_YEWAIBOSS_BELONG		= 16	--野外boss归属
RETMAP_CLIENT_INSTATE		= 17	--客户端进入地图
RETMAP_SYS_TIPS				= 18	--系统广播
RETMAP_RELIVE				= 19	--客户端副本复活
RETMAP_SHIFTDEL_TER			= 20	--帮派领地战机械加积分
RETMAP_SHIFTDEL_SIEGE		= 21	--跨服攻城战机械
RETMAP_WALKSHOW				= 22	--婚礼游行结束回调
RETMAP_SYN_CAB_POS			= 23	--婚礼马车位置同步
RETMAP_PLAYER_POS			= 24	--角色坐标信息
RETMAP_JIERIBOSS_DELBUFF	= 25	--节日boss 护盾破灭
RETMAP_SECRETBOSS_REWARD	= 26	--密境boss奖励
-------------------跳闪值
MAX_FLYDODGE = 6
FLYDODGE_COOLTIME = 150
FLYDODGE_COOLTIME_CLIENT = 15

-------------------NPC是否可见
CANSEE_TYPE_CANT		= 0			--不可见
CANSEE_TYPE_CAN			= 1			--可见

BUFFOVER_TYPE_NOT		= 0			--不覆盖
BUFFOVER_TYPE_BIGGERE	= 1			--大于等于
BUFFOVER_TYPE_BIGGER	= 2			--大于

-------------------副本npc对话触发条件
FUBEN_TALK_TRIGGER_HURT			= 1			--首次受到伤害
FUBEN_TALK_TRIGGER_SKILLTIPS	= 2			--技能预警

-------------------玩家镜像类型----------------------
USER_MIRROR_NORMAL		= 0					--默认机器人
USER_MIRROR_WULIN 		= 1					--武林机器人
USER_MIRROR_WALKONLY	= 2					--只走机器人

-------------------副本检测数据----------------------
FUBEN_CHECK_VAR = {
	[mFIGHT_Ap] = 1.2,
	[mFIGHT_Dp] = 1.2,
	[mFIGHT_PartnerAp] = 1.2,
	[mFIGHT_PartnerHurt] = 1.2,
	[mFIGHT_MagicAp] = 1.2,
	[mFIGHT_MagicHurt] = 1.2,
	[mFIGHT_DoubleHurt] = 1.2,
	[mFIGHT_Hurt] = 1.2,
	[mFIGHT_Double] = 1.2,
}

------------------------3V3状态互克表------------------
K3V3_STATE_CONST = {
	{1,1.2,0.8},
	{0.8,1,1.2},
	{1.2,0.8,1}
}