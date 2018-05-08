local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local NORMAL_NPC_RADIUS = NORMAL_NPC_RADIUS
local lua_time_sec = lua_time_sec
local tinsert = table.insert
local SERVER_NPC = Import("map/char/npc.lua")			--server npc

clsNpc = SERVER_NPC.clsNpc:Inherit({__ClassType = "NPC"})
function clsNpc:__init__(x, y, z, syncData, ociData, mapLayer, hpRate)
	Super(clsNpc).__init__(self, x, y, z, syncData, ociData, 1 , hpRate)
end

--function clsNpc:AddMap()
--end
--function clsNpc:SendMove()
--end
--function clsNpc:Move()
--end
--function clsNpc:MoveTo()
--end
--function clsNpc:JumpTo()
--end
--function clsNpc:LeaveMap()
--end
