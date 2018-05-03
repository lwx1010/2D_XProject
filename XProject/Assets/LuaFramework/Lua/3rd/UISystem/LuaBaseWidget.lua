-- @Author: LiangZG
-- @Date:   2017-03-14 20:58:49
-- @Last Modified time: 2017-03-22 10:46:48


local LuaBaseWidget = {}

function LuaBaseWidget:asImage( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Image))
end

function LuaBaseWidget:asText( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Text))
end

function LuaBaseWidget:asButton( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Button))
end

function LuaBaseWidget:asInput( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.InputField))
end

function LuaBaseWidget:asToggle( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Toggle))
end

function LuaBaseWidget:asScrollView( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.UIScrollView))
end

function LuaBaseWidget:asScrollBar( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Scrollbar))
end

function LuaBaseWidget:asSlider( )
    return self.gameObject:GetComponent(typeof(UnityEngine.UI.Slider))
end

function LuaBaseWidget:as( comStr )
    return self.gameObject:GetComponent(comStr)
end

function LuaBaseWidget:asArray(com)
    return self.gameObject:GetComponents(typeof(com))
end

function LuaBaseWidget:asArrayInChild(com)
    return self.gameObject:GetComponentsInChildren(typeof(com))
end

return LuaBaseWidget
