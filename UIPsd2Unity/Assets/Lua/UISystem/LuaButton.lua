
local LuaButton = class("LuaButton")

function LuaButton:bind( root , widget , behaviour)
    local btn = root.gameObject:GetComponent(typeof(UIButton))
    tolua.setpeer(btn , self)

    self.transform = root
    self.gameObject = root.gameObject

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

return LuaButton