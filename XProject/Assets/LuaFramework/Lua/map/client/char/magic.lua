local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local tinsert = table.insert
local c_aPoint = MOVE_DIR
local SERVER_MAGIC = Import("map/char/magic.lua")			--server magic

clsMagic = SERVER_MAGIC.clsMagic:Inherit({__ClassType = "MAGIC"})
function clsMagic:__init__(ociData, ownerId, mapLayer)
	Super(clsMagic).__init__(self, ociData, ownerId, mapLayer)
end