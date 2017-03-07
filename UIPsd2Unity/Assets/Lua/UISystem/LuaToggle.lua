
local LuaToggle = class("LuaToggle")


function LuaToggle:bind( trans , widget )

    local toggle = trans.gameObject:GetComponent(typeof(UIToggle))
    tolua.setpeer(toggle , self)

    if widget.onChange then
        EventDelegate.Add(toggle.onChange , EventDelegate.Callback(widget.onChange))
    end

    return toggle
end

return LuaToggle