local skynet = require "skynet"
local sharedata = require "skynet.sharedata"
local lutil = require "util.core"
local is_websocket = (skynet.getenv("is_websocket") == "true") and true or false
local posix = require "posix"

local string = string
local table = table
local math = math
local io = io
local tonumber = tonumber
local pairs = pairs

protoid_name = {}
--将一个str以del分割为若干个table中的元素
--n为分割次数
function string.split( line, sep, maxsplit )
	if string.len(line) == 0 then
		return {}
	end
	sep = sep or ' '
	maxsplit = maxsplit or 0
	local retval = {}
	local pos = 1   
	local step = 0
	while true do   
		local from, to = string.find(line, sep, pos, true)
		step = step + 1
		if (maxsplit ~= 0 and step > maxsplit) or from == nil then
			local item = string.sub(line, pos)
			table.insert( retval, item )
			break
		else
			local item = string.sub(line, pos, from-1)
			table.insert( retval, item )
			pos = to + 1
		end
	end     
	return retval 
end

--初始所有协议表
function init_all()
	local f = assert(io.open("protocol/proto.conf", 'r'))
  	while true do
		local line = f:read("*line")
		if line == nil then
	        break
	    end
	    local d = string.split(line, ",")
	    local n = string.split(d[2], ".")
	    local proto_id = tonumber(d[1])
	    local name = n[2]
	    protoid_name[name]=proto_id
		protoid_name[proto_id]=name
	end
	f:close()
end
init_all()

if is_websocket then
	local pbData = require "protocol.wspb"
	assert(type(pbData) == "table")
	sharedata.new("pb_data", {
		protoid_name = protoid_name,
		pb_data = pbData,
	})
else
	local PBC = require "protocol.protobuf"
	function regist_all_pb(proto_path)
		local proto_path = proto_path or "protocol/pbc"
		local PathTbl = posix.dir(proto_path)
		if not PathTbl then
			return
		end
		for k,v in pairs(PathTbl) do
			if v ~= "." and v ~= ".." and v ~= ".svn" then
				local TmpPath = proto_path.."/"..v
				local stat = posix.stat(TmpPath)
				if stat.type == "directory" then
					regist_all_pb(TmpPath)
				elseif stat.type == "regular" then
					if string.match(v, "%.pb$") then
						PBC.register_file(TmpPath)
					end
				else
					--nothng
				end
			end
		end
	end
	regist_all_pb()
	lutil.save_pbenv(PBC.get_cenv())
	sharedata.new("pb_data", {protoid_name = protoid_name})
end

