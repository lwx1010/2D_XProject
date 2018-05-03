-- @Author: LiangZG
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc: 自动测试流程中的异常捕获界面

ExceptionPanel = {}
local this = ExceptionPanel

--由LuaBehaviour自动调用
function ExceptionPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="BtnExit",path="BtnExit",src=LuaButton, onClick = this._onClickClose },
		{field="descLab",path="CurrentContext.GameObject.DescTitle",src=LuaText},
		{field="BtnContinue",path="BtnContinue",src=LuaButton, onClick = this._onContinue },

	}
	LuaUIHelper.bind(this.gameObject , ExceptionPanel )
end

function ExceptionPanel.show( cutTest , condition, stackTrace )
    this.cutTest= cutTest
    
    local txt = {[0]=condition , [1] = stackTrace}
    this.desc= table.concat(txt , "\n")
    createPanel("Exception", nil, false)
end

--由LuaBehaviour自动调用
function ExceptionPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , ExceptionPanel)

    this.descLab.text = this.desc
end

--- 关闭界面
function ExceptionPanel._onClickClose( )
    panelMgr:ClosePanel("Exception")

    this.cutTest:Stop()
end

--- 点击继续
function ExceptionPanel._onContinue()
    panelMgr:ClosePanel("Exception")
    
    this.cutTest:Play()
end

--由LuaBehaviour自动调用
function ExceptionPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--由LuaBehaviour自动调用--
function ExceptionPanel.OnDestroy()

end