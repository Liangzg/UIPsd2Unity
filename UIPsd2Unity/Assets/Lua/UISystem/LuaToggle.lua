
local LuaToggle = class("LuaToggle")


function LuaToggle:bind( trans , widget , behaviour)

    local toggle = trans.gameObject:GetComponent(typeof(UIToggle))
    tolua.setpeer(toggle , self)

    if widget.onChange then
        behaviour:AddToggleChange(trans.gameObject , widget.onChange)
    end

    return toggle
end

return LuaToggle