--直接传递进map的协议id,不走主goroutine的
--在主goroutine直接加载此文件

local io = io
local tonumber = tonumber
local table = table

local toMapProtoName = {				--手动加入到map的协议名字
	"C2s_aoi_skill_act",
	"C2s_aoi_skill_calinfo",
	"C2s_aoi_skill_hit",
	"C2s_aoi_move",
	"C2s_aoi_player_fly",
	"C2s_aoi_player_fly_finished",
	"C2s_aoi_bufftips",
	"C2s_aoi_sprint",
	"C2s_aoi_searchnpc",
	"C2s_yewaiboss_belonglist",
	"C2s_yewaiboss_scorelist",
	"C2s_aoi_shapeshift_del",
}

function GetToMapProtoIds()
	local protoIds = {}
	for _, _protoName in pairs(toMapProtoName) do
		local protoid = GET_PROTOID(_protoName)
		if protoid then
			table.insert(protoIds, protoid)
		end
	end
	return protoIds
end