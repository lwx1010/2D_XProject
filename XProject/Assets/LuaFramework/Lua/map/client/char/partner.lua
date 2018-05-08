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
local SERVER_PARTNER = Import("map/char/partner.lua")			--server partner

clsPartner = SERVER_PARTNER.clsPartner:Inherit({__ClassType = "PARTENR"})
function clsPartner:__init__(ociData, ownerId, mapLayer)
	Super(clsPartner).__init__(self, ociData, ownerId, mapLayer)
end