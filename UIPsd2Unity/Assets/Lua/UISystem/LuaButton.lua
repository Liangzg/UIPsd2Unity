
local LuaButton = class("LuaButton")

function LuaButton:bind( root , widget )
    local btn = root.gameObject:GetComponent(typeof(UIButton))
    tolua.setpeer(btn , self)

    self.transform = root
    self.gameObject = root.gameObject

    if not widget.onClick then  return btn  end

    EventDelegate.Add(btn.onClick , EventDelegate.Callback(widget.onClick))

    return btn
end

function LuaButton:luaButton( )
    print("---Lua Button function ----")
end

return LuaButton