-- @Author: LiangZG
-- @Last Modified time: 2017-05-09 14:34:37
-- @Desc: 公告界面

NoticesPanel = {}
local this = NoticesPanel

--由LuaBehaviour自动调用
function NoticesPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="rightScroll",path="RightViewGroup",src=LuaPanel},
		{field="noticeTitleLab",path="RightViewGroup.viewport.1titleTime.1title",src=LuaText},
		{field="noticeLab",path="RightViewGroup.viewport.2contextWidget.2context",src=LuaText},
		{field="btnClose",path="BtnClose",src=LuaButton, onClick = this._onClickClose },
		{field="srcCell",path="LeftScroll.cell",src="GameObject"},
		{field="leftViewport",path="LeftScroll.viewport",src="UITable"},
		{field="noticePosBar",path="VScroll Bar",src=LuaScrollBar },
		{field="mask",path="mask",src=LuaImage },

	}
	LuaUIHelper.bind(this.gameObject , NoticesPanel )
end


function NoticesPanel.show( maskAlpha)
	this.maskAlpha = maskAlpha

	createPanel('Login/Notices', nil, false)
end

--是否需要登录显示
function NoticesPanel.IsNeedShow( )
	local UserConfig = User_Config
	local yearDay = tostring(os.date("%x" , os.time()))
	local curCount = tostring(NoticeModel.inst:count())
	-- Debugger.Log("curTime:" .. tostring(yearDay))

	local lastNoticeTime = UserConfig.GetString("noticesTime")
	local noticeNum = UserConfig.GetString("noticesNum")

	if yearDay ~= lastNoticeTime or noticeNum ~= curCount  then
		UserConfig.SetString("noticesTime" , yearDay)
		UserConfig.SetString("noticesNum" , curCount)
		UserConfig.Save()
		return true
	end
	return false
end

--由LuaBehaviour自动调用
function NoticesPanel.Start()
	if this.maskAlpha then
		this.mask.color = Color.RGBA255(255,255,255,0)
	end
end

--由LuaBehaviour自动调用
function NoticesPanel.OnInit()
	LuaUIHelper.addUIDepth(this.gameObject , NoticesPanel)

	this.cellPool = GoPool.reset(this.cellPool)
	this.cells = {}

	local notices = NoticeModel.inst.notices
    if not notices then
        this.noticeTitleLab.transform.parent.gameObject:SetActive(false)
	    this.noticeLab.text = "暂无公告"
        return
    end

	table.sort(notices , function ( a , b )
		local at = TimeConverter.ConvertDateTimeInt(TimeConverter.Parse(a.announceTime))
		local bt = TimeConverter.ConvertDateTimeInt(TimeConverter.Parse(b.announceTime))
		return at > bt
	end)

	local i = 0
	for _,notice in ipairs(notices) do
		local cellData = { SubTitle = notice.headline }

        --time
        cellData.Title = string.format(LANGUAGE_TIP.Notices_Time_Format , DateTime.Parse(notice.announceTime):ToString("yyyy-MM-dd"))

		--local context = {[1] = notice.announceTime , [2] = notice.announceContxt}
		--cellData.Text = table.concat( context, "\n")
        cellData.Text = notice.announceContxt

		this._createNoticeCells(i .. "Cell" , cellData)
		i = i + 1
	end

	this.leftViewport.repositionNow = true

	this.cells[1].mainToggle.value = true
end


--创建公告单元

function NoticesPanel._createNoticeCells( cellName , cellData )

	local cellObj = GoPool.swapnGameObject( this.cellPool , this.srcCell , this.leftViewport.transform)
	cellObj.name = cellName
	-- Debugger.Log("---->gObj:" .. print_lua_table(cellObj , 0 , 3 , true) )

	local newCell = NoticesPanel.NCell.new(cellObj)
	newCell:init(cellData)

	this.cells[#this.cells + 1] = newCell
end

--由LuaBehaviour自动调用
function NoticesPanel.OnClose()
	LuaUIHelper.removeUIDepth(this.gameObject)
end

--关闭按钮
function NoticesPanel._onClickClose( )

	panelMgr:ClosePanel("Notices")
end

--设置显示公告
--@cellData table
function NoticesPanel.setNoticeContext( cellData )
	this.noticeTitleLab.text = cellData.Title
	this.noticeLab.text = cellData.Text

	this.noticePosBar.value = 0
	local scrollView = this.rightScroll:asScrollView()
	scrollView:UpdatePosition()
end

--由LuaBehaviour自动调用--
function NoticesPanel.OnDestroy()
	GoPool.clear(this.cellPool)
end

NoticesPanel.NCell = class("NoticesPanel_Cell")

function NoticesPanel.NCell:ctor( gObj )
	self.mainObj = gObj
	--self.cellData
	self.mainToggle = gObj:GetComponent(typeof(UIToggle))
	NoticesPanel.behaviour:AddToggleChange(gObj , handler(self , self._onToggle))
end

function NoticesPanel.NCell:init( cellData )
	self.cellData = cellData

	local titleN = self.mainObj.transform:Find("title")
	titleN = titleN:GetComponent(typeof(UILabel))
	titleN.text = cellData.SubTitle

	titleN = self.mainObj.transform:Find("checkmark/title")
	titleN = titleN:GetComponent(typeof(UILabel))
	titleN.text = cellData.SubTitle
end

function NoticesPanel.NCell:_onToggle( toggle )
	if not toggle.value then	return  end

	NoticesPanel.setNoticeContext(self.cellData)
end