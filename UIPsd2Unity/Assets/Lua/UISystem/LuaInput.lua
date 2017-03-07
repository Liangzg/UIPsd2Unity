-- @Author: LiangZG
-- @Date:   2017-02-28 10:13:57
-- @Last Modified time: 2017-03-07 19:47:36


local LuaInput = class("LuaInput")

function LuaInput:bind( trans , widget )
    local input = trans.gameObject:GetComponent(typeof(UIInput))
    tolua.setpeer(input , self)

	if widget.onChange then
		EventDelegate.Add(input.onChange , EventDelegate.Callback(widget.onChange))
	end

    if widget.onSubmit then
        EventDelegate.Add(input.onSubmit , EventDelegate.Callback(widget.onSubmit))
    end

    return input
end

return LuaInput