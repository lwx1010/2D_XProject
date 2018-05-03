local LANGUAGE_TIP = LANGUAGE_TIP
local TipLogic = {}
local this = TipLogic
TipLogic.isCarent = false

TipLogic.normalTip_Title = nil
TipLogic.normalTip_Content = nil
TipLogic.normalTip_OkCallback = nil
TipLogic.normalTip_NoCallback = nil
TipLogic.normalTip_OkLabel = nil
TipLogic.normalTip_CancelLabel = nil
TipLogic.normalTip_CloseNotCB = nil

function TipLogic.PopupNormalTip(content, okCallback, noCallback, title, okLabel, cancelLabel, closeNotCB)
	this.normalTip_Content = content
	this.normalTip_OkCallback = okCallback
	this.normalTip_NoCallback = noCallback
	this.normalTip_Title = title and title or LANGUAGE_TIP.warmTip
	this.normalTip_OkLabel = okLabel and okLabel or LANGUAGE_TIP.yes
	this.normalTip_CancelLabel = cancelLabel and cancelLabel or LANGUAGE_TIP.no
	TipLogic.normalTip_CloseNotCB = closeNotCB 
	CtrlManager.PopUpPanel('Tip')
end

function TipLogic.PopupTip(content,  title)
	TipLogic.PopupNormalTip(content , function()
        
    end , nil , title)
end

function TipLogic.RemoveNormalTip()
	TipLogic.normalTip_Title = nil
	TipLogic.normalTip_Content = nil
	TipLogic.normalTip_OkCallback = nil
	TipLogic.normalTip_NoCallback = nil
	TipLogic.isCarent = false
end

--弹出helptip  showtype 默认为提示语  1 是称号说明提示
function TipLogic.PopUpHelpTipUi(tipId , showtype)
	local allHelpTipXls = require("xlsdata/HelpTipXls")
	if not allHelpTipXls[tipId] then 
		return
	end

	if not CtrlManager.PanelIsPopuped("HelpTipUi") then
		CtrlManager.PopUpPanel("HelpTipUi", false)
	end

	local HelpTipUiCtrl = CtrlManager.GetCtrl("HelpTipUiCtrl")
	HelpTipUiCtrl.SetShowInfo(allHelpTipXls[tipId].TipTitle, allHelpTipXls[tipId].TipContent , showtype)
end

return TipLogic