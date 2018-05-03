-- @Author: LiangZG
-- @Date:   2017-03-16 16:49:14
-- @Last Modified time: 2017-05-16 15:35:57

--UI组件

-------------脚本-----------------
LuaScript = class("LuaScript")

function LuaScript:bind( root , widget , behaviour)
    local tab = require(widget.requirePath)
    tab = tab.new()
    tab:Awake(root.gameObject)
    return tab
end


--------------基础图片---------------

LuaImage = class("LuaImage")
implement(LuaImage , LuaBaseWidget)

function LuaImage:bind( trans , widget )

    local sprite = trans.gameObject:GetComponent(typeof(UnityEngine.UI.Image))
    tolua.setpeer(sprite , self)

    return sprite
end

--------------基础文本------------------------

LuaText = class("LuaText")
implement(LuaText , LuaBaseWidget)

function LuaText:bind( trans , widget )
    local lab = trans.gameObject:GetComponent(typeof(UnityEngine.UI.Text))
    tolua.setpeer(lab , self)
    return lab
end


-----------------按妞------------------------
LuaButton = class("LuaButton")
implement(LuaButton , LuaBaseWidget)

function LuaButton:bind( root , widget , behaviour)
    local btn = root.gameObject:GetComponent(typeof(UnityEngine.UI.Button))
    tolua.setpeer(btn , self)

    if widget.onClick then
        behaviour:AddClick(root.gameObject , widget.onClick)
    else
        print("cant find button click !~")
    end

    return btn
end

function LuaButton:luaButton( )
    print("---Lua Button function ----")
end


-------------------Toggle 切换按钮-----------------
LuaToggle = class("LuaToggle")
implement(LuaToggle , LuaBaseWidget)

function LuaToggle:bind( trans , widget , behaviour)

    local toggle = trans.gameObject:GetComponent(typeof(UnityEngine.UI.Toggle))
    tolua.setpeer(toggle , self)

    if widget.onChange then
        behaviour:AddToggleChange(trans.gameObject , widget.onChange)
    end

    return toggle
end

----------------输入框--------------------
LuaInput = class("LuaInput")
implement(LuaInput , LuaBaseWidget)

function LuaInput:bind( trans , widget ,behaviour)
    local input = trans.gameObject:GetComponent(typeof(UnityEngine.UI.InputField))
    tolua.setpeer(input , self)

    if widget.onChange then
        behaviour:AddValueChange(trans.gameObject , widget.onChange)
    end

    if widget.onSubmit then
        behaviour:AddSubmit(trans.gameObject , widget.onSubmit)
    end

    return input
end

------------------Slider 进度条----------------------
LuaSlider = class("LuaSlider")
implement(LuaSlider , LuaBaseWidget)

function LuaSlider:bind( trans , widget ,behaviour)
    local com = trans.gameObject:GetComponent(typeof(UnityEngine.UI.Slider))
    tolua.setpeer(com , self)

    if widget.onValueChange then
        behaviour:AddProgressBarChange(trans.gameObject , widget.onValueChange)
    end

    return com
end

------------------Slider 进度条----------------------
LuaScrollBar = class("LuaScrollBar")
implement(LuaScrollBar , LuaBaseWidget)

function LuaScrollBar:bind( trans , widget ,behaviour)
    local com = trans.gameObject:GetComponent(typeof(UnityEngine.UI.Scrollbar))
    tolua.setpeer(com , self)

    if widget.onValueChange then
        behaviour:AddProgressBarChange(trans.gameObject , widget.onValueChange)
    end

    return com
end

-------------------常规组件扩展----------------------------
--比如所有继续自Component的组件
LuaUiComponent = class("LuaUiComponent")
implement(LuaUiComponent , LuaBaseWidget)

function LuaUiComponent:bind( trans , widget )
     local com = trans.gameObject
     if widget.src ~= "GameObject" then
        com = com:GetComponent(widget.src)
     end
     tolua.setpeer(com , self)
     return com
end
