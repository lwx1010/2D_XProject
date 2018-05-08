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
		[1] = { -126.92, 28.71, 86.47 }, 
		[2] = { -107.52, 30.77, 83.92 }, 
		[3] = { -107.65, 35.406, 100.34 }, 
		[4] = { -83.50001, 30.634, 105.5 }, 
	},

	------------------------------
	--@ 对应目标点的起始垂直速度
	------------------------------
	["startVertSpeed"] = 
	{
		[1] = 10,
		[2] = 15,
		[3] = 21,
		[4] = 11,
	},

	-----------------------------------------------------------------
	--@ 对应目标点的垂直方向加速度(小于0或者等于0则默认为重力加速度)
	-----------------------------------------------------------------
	["vertAccSpeed"] = 
	{
		[1] = 22,
		[2] = 18,
		[3] = 22,
		[4] = 20,
	},

	-----------------------------------------------------------------
	--@ 跳跃使用动作, 1-jump01, 2-jump02, 3-jump03
	-----------------------------------------------------------------
	["action"] = 
	{
		[1] = 1,
		[2] = 3,
		[3] = 2,
		[4] = 2,
	},
}

return data