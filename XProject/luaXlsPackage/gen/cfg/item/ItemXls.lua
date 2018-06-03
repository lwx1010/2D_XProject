local function Process(XlsInst)
	XlsInst:SetVarFunction("RelaKey", [[{%s}]])
end

cfg =
{
	xls={"item/equip.xls", "item/item.xls"},
	sheet={{1,2}, {1,2}},
	outputName="Item/ItemXls",
	key="ItemNo",
	postProcess=function(data) 
		local ret = {}
		for _,xlsdata in pairs(data) do
			for _,sheetdata in pairs(xlsdata) do
				for rowno, rowdata in pairs(sheetdata) do
					if rowdata then						
						local item = {}
						item.ItemNo = rowdata.ItemNo
						item.frame = rowdata.AppFrameNo
						item.Name = rowdata.Name
						item.NickName = rowdata.NickName
						item.IconNo = rowdata.IconNo
						item.FemaleIcon = rowdata.FemaleIcon
						item.Rare = rowdata.Rare
						item.Kind = rowdata.Kind
						item.SubKind = rowdata.SubKind
						item.Info = rowdata.Info
						item.GetPath = rowdata.GetPath
						item.HpMax = rowdata.HpMax
						item.Ap = rowdata.Ap
						item.Dp = rowdata.Dp
						item.HitRate = rowdata.HitRate
						item.Dodge = rowdata.Dodge
						item.Double = rowdata.Double
						item.Step = rowdata.Step
						item.ExAp = rowdata.ExAp
						item.ExHpMax = rowdata.ExHpMax
						item.ExDp = rowdata.ExDp
						item.FitEquip = rowdata.FitEquip
						item.ShowGrade = rowdata.ShowGrade
						item.BaseScore = rowdata.BaseScore
						item.NeedGrade = rowdata.NeedGrade
						item.OverLap = rowdata.OverLap
						item.CanArrange = rowdata.CanArrange
						item.VIP = rowdata.VIP
						item.CanBatch = rowdata.CanBatch
						item.CanUse = rowdata.CanUse or 0
						item.Price = rowdata.Price
						item.RelaKey = rowdata.RelaKey
						item.UpScore = rowdata.UpScore
						item.NeedScore = rowdata.NeedScore
						item.Grade = rowdata.Grade
						item.IconStr = rowdata.IconStr
						item.UiId = rowdata.UiId
						item.CanDrop = rowdata.CanDrop
						item.CanTimeLimited = rowdata.CanTimeLimited
						item.FuncNo = rowdata.FuncNo
						item.IsGetInfo = rowdata.IsGetInfo
						item.DoubleHurt = rowdata.DoubleHurt
						item.ReDoubleHurt = rowdata.ReDoubleHurt
						item.Hurt = rowdata.Hurt
						item.ReHurt = rowdata.ReHurt
						item.Melting = rowdata.Melting
						item.JiShouGroup = rowdata.JiShouGroup
						item.JiShouPrice = rowdata.JiShouPrice
						item.TitleNo = rowdata.TitleNo
						item.AddIcon = rowdata.AddIcon
						item.AtlasPath = rowdata.AtlasPath
						item.SetNo = rowdata.SetNo
						item.ExtraNum = rowdata.ExtraNum
						item.CanPushUse = rowdata.CanPushUse
						item.StarLimit = rowdata.StarLimit
						item.LevelLimit = rowdata.LevelLimit
						item.AddRate = rowdata.AddRate
						table.insert(ret, item)

					end
				end
			end 
		end
		
		return ret
	end,
	beforeProcess=Process,
	desc="µÀ¾ß±í¸ñ",
}
