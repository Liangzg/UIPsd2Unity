-- @Author: LiangZG
-- @Date:   2017-02-28 10:16:38
-- @Last Modified time: 2017-03-07 19:31:25


local LuaPanel = class("LuaPanel")

function LuaPanel:bind( trans , widget )
    local panel = trans.gameObject:GetComponent(typeof(UIPanel))
    tolua.setpeer(panel , self)

    return panel
end

return LuaPanel