local table = table
local math = math
local pairs = pairs
needLoad, MAP_NO, MAP_ID, MAP_MAX_X, MAP_MAX_Y = ... 
MAP_NO, MAP_ID, MAP_MAX_X, MAP_MAX_Y = tonumber(MAP_NO), tonumber(MAP_ID), tonumber(MAP_MAX_X), tonumber(MAP_MAX_Y)
IS_FIGHT_MAP = true
IS_RET_MOVE = false
--print( MAP_NO, " ", MAP_ID, " ", MAP_MAX_X, " ", MAP_MAX_Y)

--print("~~~~~~~~~~~~:", func_call)
func_call = func_call or {}

IS_CLIENT = true
function IsServer()
	return 
end
function IsClient()
	return IS_CLIENT
end

EVILCAL = true

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
os.exit = nil 

CALLOUT = {
	RemoveAll = function() end
}

--保存对象所在的位置	
--layer = {																			
--	x = {
--		y = {}		--弱表
--	}
--}
MAP_LAYER_CHAR = {[1] = {}}
MAP_LAYER_DATA = {[1] = {}}

function lua_PreloadClear()
	MAP_LAYER_DATA = {[1] = {}}
	MAP_LAYER_CHAR = {[1] = {}}
	IS_USERDIE_PARTNERIDE = true
end

DOFILELIST = 
{
		"map/client/export.lua",
		"base/class.lua",
		"base/extend.lua",
		"map/client/import.lua",
		"base/common_const.lua",
		"map/map_const.lua",
		"map/engine2lua.lua",
		"map/timer.lua",									
		"map/client/global.lua",		--放到最后面
}

local function OnStart()
	sys = sys or {}
	sys.path = sys.path or {}
	math.randomseed(os.time()+17423987)
end

local function do_preload ()
	for _,file in pairs(DOFILELIST) do
		--print ("==============preload ..... ",file)
		if IsServer() then
			dofile(file)
		else
			file = string.sub(file, 1, -5)
			local func, err = loadfile(file)
			func()
		end
	end
	
	--把自己也添加进去
	table.insert(DOFILELIST, "map/client/preload.lua")
end

-----开始执行加载逻辑----
print("needLoad:", needLoad)
if needLoad then 
	OnStart()
	do_preload()
else
	_G._ImportModule = {}
	local file = "map/client/global.lua"
	file = string.sub(file, 1, -5)
	local func, err = loadfile(file)
	func()
end


sys.dump = function() end