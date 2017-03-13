-- @Author: LiangZG
-- @Date:   2017-03-09 20:50:44
-- @Last Modified time: 2017-03-13 14:16:22

local LuaScript = class("LuaScript")

function LuaScript:bind( root , widget )
    local tab = require(widget.requirePath)
	tab = tab.new()
	tab.gameObject = root.gameObject
	tab.transform = root

	tab:Awake()
    return tab
end

return LuaScript