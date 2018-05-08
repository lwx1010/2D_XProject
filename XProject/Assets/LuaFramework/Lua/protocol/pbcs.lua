---
-- 由外部脚本工具gen_pbc_handler.py 生成,不要手动修改
--

local protobuf = require "protobuf"
local function GetFullPath(path)
	return Util.LuaPath..path
end

module('protocol.pbcs')

protobuf.register_file(GetFullPath('protocol/pbc/all/all.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/aoi/aoi.pb'))
