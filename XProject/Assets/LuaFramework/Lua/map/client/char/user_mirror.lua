local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local mceil=math.ceil
local tinsert = table.insert
local ssplit = string.split
local lua_time_sec = lua_time_sec
local SERVER_USERMIRROR = Import("map/char/user_mirror.lua")			--server user

clsUserMirror = SERVER_USERMIRROR.clsUserMirror:Inherit({__ClassType = "USER"})
function clsUserMirror:__init__(vfd, x, y, z, syncData, ociData, mapLayer)
	Super(clsUserMirror).__init__(self, vfd, x, y, z, syncData, ociData, mapLayer)
	
	if IsClient() then
		self:SetDir(5)
	end
end
