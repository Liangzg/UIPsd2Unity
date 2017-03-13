-- @Author: LiangZG
-- @Date:   2017-02-28 10:13:57
-- @Last Modified time: 2017-03-13 10:44:02


local LuaInput = class("LuaInput")

function LuaInput:bind( trans , widget ,behaviour)
    local input = trans.gameObject:GetComponent(typeof(UIInput))
    tolua.setpeer(input , self)

	if widget.onChange then
        behaviour:AddValueChange(trans.gameObject , widget.onChange)
	end

    if widget.onSubmit then
        behaviour:AddSubmit(trans.gameObject , widget.onSubmit)
    end

    return input
end

return LuaInput