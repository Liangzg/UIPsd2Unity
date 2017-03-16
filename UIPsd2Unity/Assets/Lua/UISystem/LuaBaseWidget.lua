-- @Author: LiangZG
-- @Date:   2017-03-14 20:58:49
-- @Last Modified time: 2017-03-16 17:11:25


local LuaBaseWidget = {}

function LuaBaseWidget:asSprite( )
    return self.gameObject:GetComponent(typeof(UISprite))
end

function LuaBaseWidget:asTexture( )
    return self.gameObject:GetComponent(typeof(UITexture))
end

function LuaBaseWidget:asToggle( )
    return self.gameObject:GetComponent(typeof(UIToggle))
end

function LuaBaseWidget:asInput( )
    return self.gameObject:GetComponent(typeof(UIInput))
end

function LuaBaseWidget:asPanel( )
    return self.gameObject:GetComponent(typeof(UIPanel))
end

function LuaBaseWidget:asScrollView( )
    return self.gameObject:GetComponent(typeof(UIScrollView))
end

function LuaBaseWidget:asScrollBar( )
    return self.gameObject:GetComponent(typeof(UIScrollBar))
end

function LuaBaseWidget:asSlider( )
    return self.gameObject:GetComponent(typeof(UISlider))
end

function LuaBaseWidget:asWrapContent( )
    return self.gameObject:GetComponent(typeof(UIWrapContent))
end

function LuaBaseWidget:asLabel( )
    return self.gameObject:GetComponent(typeof(UILabel))
end

return LuaBaseWidget
