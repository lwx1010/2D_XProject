---------------------------------
--@ 剧情跳跃相关配置
--@ 注意每个table的长度都必须相同
---------------------------------

local data = 
{
	-----------------------------------------------------
	--@ 剧情跳跃需要经过的目标点(不含起始点, 包含最终点)
	-----------------------------------------------------
	["targetPosition"] = 
	{ 
		[1] = { 64.257, 2.425, 49.36 }, 
		[2] = { 63.816, 1.735, 39.12 },
		[3] = { 56.5, 4.109, 30.5 },		
	},

	------------------------------
	--@ 对应目标点的起始垂直速度
	------------------------------
	["startVertSpeed"] = 
	{
		[1] = 10,
		[2] = 10,
		[3] = 13,
	},

	-----------------------------------------------------------------
	--@ 对应目标点的垂直方向加速度(小于0或者等于0则默认为重力加速度)
	-----------------------------------------------------------------
	["vertAccSpeed"] = 
	{
		[1] = 20,
		[2] = 19,
		[3] = 19,
	},

	-----------------------------------------------------------------
	--@ 跳跃使用动作, 1-jump01, 2-jump02, 3-jump03
	-----------------------------------------------------------------
	["action"] = 
	{
		[1] = 1,
		[2] = 3,
		[3] = 2,
	},
}

return data