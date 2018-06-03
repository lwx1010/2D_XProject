local function postProcess(RawData)
	local Data = {}
	for _,v in ipairs(RawData)  do
		if not Data[v.HunShouNo] then
			Data[v.HunShouNo] = {}
		end
		if not Data[v.HunShouNo][v.Step] then
			Data[v.HunShouNo][v.Step] = {}
		end
		local tmp = {
			Exp = v.Exp,
			DuanWeiCost = v.DuanWeiCost,
			DuanWeiProp = v.DuanWeiProp
		}
		Data[v.HunShouNo][v.Step][v.DuanWei] = tmp
	end
	return Data
end

cfg =
{
	xls="hunshou/hunshou.xls",
	sheet=3,
	columns={"HunShouNo","Step","DuanWei","Exp","DuanWeiCost","DuanWeiProp"},
	outputName="Hunshou/HunShouPassageXls",
	postProcess=postProcess,
	desc="hunshou",
}