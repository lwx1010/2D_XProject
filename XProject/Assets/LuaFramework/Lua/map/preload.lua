local table = table
local math = math
local pairs = pairs
local mceil = math.ceil
MAP_NO, MAP_ID = ... 


IS_DELAY_RETMOVE = true			--延迟返回移动
IS_FIGHT_MAP = true
IS_RET_MOVE = true				--所有都发送移动回主逻辑
--这是一个全局的模块，游戏启动的时候第一个载入的脚本。

IS_MAPSTATE = true
IS_SERVER = true
function IsServer()
	return IS_SERVER
end
function IsClient()
	return
end

EVILCAL = true			--计算善恶值

function _SkillVarNoFunc(value, tbl)
	for _, _tbl in pairs(tbl) do
		if _tbl[1] <= value and value <= _tbl[2] then
			return _tbl[3]
		end
	end
	return 0
end

debug.excepthook = debug.traceback
Reversion = 14177
BootTime = nil
os.exit = nil 

func_call = {}

local p = "3rd/pbc" 
local old_path = package.path
package.path = string.format("%s;%s/?.lua;%s/?/init.lua;",old_path,p,p)
local json_path = "3rd/cjson/?.so;"
local protobuff_path = "3rd/protobuf/?.so;"
package.cpath = string.format("%s;%s;%s",json_path,protobuff_path,package.cpath)
dofile("3rd/pbc/pbc.lua")

PBC = require "pbc"

CALLOUT = {
	RemoveAll = function() end
}

--保存对象所在的位置	
--layer = {																			
--	x = {
--		y = {}		--弱表
--	}
--}
MAP_LAYER_CHAR = {}

MAP_DATA_OBJ, MAP_MAX_X, MAP_MAX_Y = lmapdata.hasmap(MAP_NO)
assert(MAP_DATA_OBJ and MAP_MAX_X and MAP_MAX_Y, "not MAP_DATA_OBJ, MAP_MAX_X, MAP_MAX_Y")

require "setting/map/map_data"
require "map/map_const"
local allData = GetMapData()
local oneData = allData[MAP_NO]

SCENE_TYPE = oneData.SceneType
SECURITY_AREAS = oneData.SecurityAreas
SCENE_NAME = oneData.Name
ENTER_POS = oneData.EnterPos
DIDUISECNE = {}
FLAG = nil
SHOW_YELLOW = nil
for mapNo, data in pairs(allData) do
	if data.DiduiScene == 1 then 
		if mapNo == MAP_NO then 
			FLAG = true
			if not data.DiduiShowYellow or data.DiduiShowYellow == 1 then
				SHOW_YELLOW = true
			end
		end	
	end
end

ONE_GRID_X, ONE_GRID_Y = oneData.GridSize[1], oneData.GridSize[2]
if oneData.FightType ~= MAP_FIGNT then
	IS_FIGHT_MAP = false
end
if not ONE_GRID_X then
	ONE_GRID_X = 12
end
if not ONE_GRID_Y then
	ONE_GRID_Y = 12
end
MIN_GRID_XY = ONE_GRID_X > ONE_GRID_Y and ONE_GRID_Y or ONE_GRID_X
MAP_LAYER_DATA = {}
local MAP_OBJ = laoi.map_new(MAP_MAX_X, MAP_MAX_Y, ONE_GRID_X, ONE_GRID_Y)	--创建map对象
assert(MAP_OBJ, "not MAP_OBJ")
MAP_LAYER_DATA[1] = MAP_OBJ	
MAP_LAYER_CHAR[1] = {}		

MAP_MOVE_BC_CNT = oneData.MoveBroadcastCnt
if oneData.FightType == MAP_FIGNT then
	MAP_MOVE_BC_CNT = nil
end

MAP_DIE_TYPE = oneData.DieState
if oneData.IsCreateStart == 1 then
	IS_RET_MOVE = true
end

if oneData.EvilCal and oneData.EvilCal == 0 then
	EVILCAL = false
end

PK_ENEMY = oneData.Enemy == 1  --仇杀模型

IS_K3V3MAP = MAP_NO == 1164  --3V3地图

function lua_PreloadClear()
	MAP_LAYER_DATA = {}
	MAP_LAYER_CHAR = {}
	local MAP_OBJ = laoi.map_new(MAP_MAX_X, MAP_MAX_Y, ONE_GRID_X, ONE_GRID_Y)	--创建map对象
	assert(MAP_OBJ, "not MAP_OBJ")
	MAP_LAYER_DATA[1] = MAP_OBJ	
	MAP_LAYER_CHAR[1] = {}	
end

--模块的载入顺序是敏感的
--大家尽量少使用dofile，那是必须全局载入的相对模块
--此Table会被其他模块访问，这些模块不允许被Import
--dofile调用的模块不允许使用__FILE__宏

-----------------------------------------------------------注意，尽量少加载没用到的东西，因为副本开多了可能内存就增加比较快

DOFILELIST = 
{
		"global/engine_export.lua",--引擎导入函数
		"base/class.lua",
--		"base/import.lua",
		"base/extend.lua",
		"common/common_const.lua",
		"map/map_const.lua",
		"map/engine2lua.lua",								--引擎到lua的函数
		"map/timer.lua",									
		"base/log.lua",
		"map/protocol.lua",
		"base/cross.lua",
		"base/net.lua",
		"base/efun.lua",
		"map/global.lua",--放到最后面
}

local function OnStart()
	BootTime = os.time()
	sys = sys or {}
	sys.path = sys.path or {}
	table.insert(sys.path,posix.getcwd())

	--初始化随机数种子 
	math.randomseed(os.time()+17423987)
end

local function do_preload ()
	for _,file in pairs(DOFILELIST) do
--		print ("preload ..... ",file)
		dofile(file)
	end
	--把自己也添加进去
	table.insert(DOFILELIST, "map/preload.lua")
end

--这个函数是引擎回调的回调的秒数后面有
--设置
function perform_gc()
--	print("perform_gc", os.time(),collectgarbage("count"), MAP_NO, MAP_ID)
	collectgarbage("step", 512)
end

-----开始执行加载逻辑----
OnStart()
do_preload()
local ext_tbl = extdata2table(_ExtData) or {}
_WorldGrade = ext_tbl.worldgrade or 0

local IsTestServer = cfgData.IsTestServer
local function tracebackAndVarieble(msg)  
    local allMsg = msg .. "\n" .. debug.traceback()
    if IsTestServer then
    	print(allMsg)
    end
    go_log_go_error("../log/calllua.log", -2, allMsg)
end  

__G__TRACKBACK__ = tracebackAndVarieble