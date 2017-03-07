-- @Author: LiangZG
-- @Date:   2017-02-28 10:14:44
-- @Last Modified time: 2017-03-07 18:00:09

local LuaText = class("LuaText")

function LuaText:bind( trans , widget )
    local lab = trans.gameObject:GetComponent(typeof(UILabel))
    tolua.setpeer(lab , self)
    return lab
end

return LuaText


