local Network = Network
local ItemLogic = require('Logic/ItemLogic')
local RoleUiLogic = {} --require('Logic/RoleUiLogic')
-- require('Logic/GodSuitLogic')
Network['S2c_item_list'] = function(pb)
	ItemLogic.ReceiveItemList(pb)
end

Network['S2c_item_equiped'] = function(pb)
	ItemLogic.RecieveCharacterEquips(pb)
end

Network['S2c_item_baseinfo'] = function(pb)
	ItemLogic.ReceiveItemBaseInfo(pb)
end

Network['S2c_item_del'] = function(pb)
	ItemLogic.ReceiveItemDelete(pb)
end

Network['S2c_item_add'] = function(pb)
	ItemLogic.ReceiveItemAdd(pb)
end

Network['S2c_item_frame_info'] = function(pb)

end

Network['S2c_item_equip'] = function(pb)
	if pb.pos > 10 and pb.pos <= 50 then  --其他养成装备
		ItemLogic.UpdateEquipPos(pb.item_id, pb.pos)
		if CtrlManager.PanelIsPopuped('RoleUi') then
			RoleUiLogic.UpdateOtherNewEquip()
		end
	elseif pb.pos >= 51 and pb.pos <= 58 then--玩家装备
		CtrlManager.PopUpNotifyText("装备成功")
		ItemLogic.UpdateEquipPos(pb.item_id, pb.pos)
		GodSuitLogic.UpdataGodEquip()
	end
end

Network['S2c_item_unequip'] = function(pb)
	ItemLogic.UnLoadEquip(pb.item_id)
	GodSuitLogic.UpdataGodEquip()
end

Network['S2c_item_buy'] = function(pb)
	if pb.isok == 1 then
		if CtrlManager.PanelIsPopuped('ShopBuy') then
			local BuyLogic = require('Logic/BuyLogic')
			BuyLogic.ShowSuccessTip()
			local ShopBuyCtrl = require('Controller/ShopBuyCtrl')
			ShopBuyCtrl.BuySuccess()
		end

		if CtrlManager.PanelIsPopuped('ShoppingItem') then
			CtrlManager.GetCtrl(CtrlNames.ShoppingItem).BuySuccess()
		end
	end
	Event.Brocast(EventName.ITEM_BUY_EVENT, pb.isok)
end

Network['S2c_item_equipattr'] = function(pb)
	-- local DuanZaoLogic = require('Logic/DuanZaoLogic')
	DuanZaoLogic.RecieveProperty(pb)
end

Network['S2c_item_movenums'] = function(pb)
	ItemLogic.SetBagOneKeyItemList()
end