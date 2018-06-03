---
-- 由外部脚本工具gen_pbc_handler.py 生成,不要手动修改
--

local protobuf = require "protobuf"
local function GetFullPath(path)
	return Util.LuaPath..path
end

module('protocol.pbcs')

protobuf.register_file(GetFullPath('protocol/pbc/boxoffice/boxoffice.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/chat/chat.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/fight/fight.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/item/item.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/login/login.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/role/role.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/test/test.pb'))
protobuf.register_file(GetFullPath('protocol/pbc/wizcmd/wizcmd.pb'))
