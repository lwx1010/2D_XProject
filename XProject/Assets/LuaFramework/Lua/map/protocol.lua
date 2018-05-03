local string=string
local table=table
local math=math
local io=io
local pairs=pairs
local ipairs=ipairs
local tostring=tostring

---负责协议模块加载
---协议模块的调度
---在base/preload.lua中启动时加载进入到全局表中

module('protocol', package.seeall)

local PBC = PBC

function regist_all_pb(proto_path)
	local proto_path = proto_path or "protocol/topbc"
	local PathTbl = posix.dir(proto_path)
	if not PathTbl then
		return
	end
	for k, v in pairs(PathTbl) do
		if v ~= "." and v ~= ".." and v ~= ".svn" then
			local TmpPath = proto_path.."/"..v
			local stat = posix.stat(TmpPath)
			if stat.type == "directory" then
				regist_all_pb(TmpPath)
			elseif stat.type == "regular" then
				if string.match(v, "aoi.pb$") then			--加载aoi.pb
					PBC.register_file(TmpPath)
				end
			else
				--nothng
			end
		end
	end
end

function load_protocol_info()
	dofile("protocol/protocol_info.lua")
end

--默认加载这个模块时，就加载所有的协议类模块
load_protocol_info()
regist_all_pb()
