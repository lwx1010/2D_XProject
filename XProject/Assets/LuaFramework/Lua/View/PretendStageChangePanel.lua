-- @Author: LiangZG
-- @Last Modified time: 2018-04-03 11:56:46
-- @Desc:

PretendStageChangePanel = {}
local this = PretendStageChangePanel

--由LuaBehaviour自动调用
function PretendStageChangePanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform


end

function PretendStageChangePanel.show( onShowDel )
    this.onShow = onShowDel
    create.isLoading = true
end

--由LuaBehaviour自动调用
function PretendStageChangePanel.OnInit()
    if this.onShow then this.onShow()   end
end

function PretendStageChangePanel.onProgressAction( progress )
    print("loading progress:" .. progress)
    if this.loadingBar then
        this.loadingBar.value = progress
    end
end