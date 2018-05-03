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
local tinsert = table.insert
local ssplit = string.split
local SERVER_USER = Import("map/char/user.lua")			--server user

clsUser = SERVER_USER.clsUser:Inherit({__ClassType = "USER"})
function clsUser:__init__(vfd, x, y, z, syncData, ociData)
	Super(clsUser).__init__(self, vfd, x, y, z, syncData, ociData, 1)
end

--function clsUser:AddMap()
--end
--function clsUser:SendMove()
--end
--function clsUser:Move()
--end
--function clsUser:MoveTo()
--end
--function clsUser:JumpTo()
--end
--function clsUser:LeaveMap()
--end