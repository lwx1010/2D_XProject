local function postProcess(RawData)
	local Data = {}
	for _,v in ipairs(RawData)  do
		if not Data[v.HunShouNo] then
			Data[v.HunShouNo] = {}
		end
		local tmp = {
			Cost = v.Cost,
			UpSkill = v.UpSkill,
			Rate = v.Rate
		}
		Data[v.HunShouNo][v.StarLevel] = tmp
	end
	return Data
end

cfg =
{
	xls="hunshou/hunshou.xls",
	sheet=4,
	columns={"HunShouNo","StarLevel","Cost","Exp","UpSkill","Rate"},
	outputName="Hunshou/HunShouStarXls",
	postProcess=postProcess,
	desc="hunshou",
}