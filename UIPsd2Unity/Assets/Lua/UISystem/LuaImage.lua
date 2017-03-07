-- @Author: LiangZG
-- @Date:   2017-02-28 10:15:21
-- @Last Modified time: 2017-03-07 19:14:41

local LuaImage = class("LuaImage")

function LuaImage:bind( trans , widget )

    local sprite = trans.gameObject:GetComponent(typeof(UISprite))
    tolua.setpeer(sprite , self)

    return sprite
end

return LuaImage