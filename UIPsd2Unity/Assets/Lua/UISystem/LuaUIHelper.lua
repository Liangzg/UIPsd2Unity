-- @Author: LiangZG
-- @Date:   2017-02-28 11:01:08
-- @Last Modified time: 2017-03-07 20:00:15

local LuaUIHelper = {}

function LuaUIHelper.bind( gObj , panel )
    local trans = gObj.transform
    for _,widget in pairs(panel.widgets) do
        local widgetPath = string.gsub(widget.path , "%." , "/")
        local childTrans = trans:FindChild(widgetPath)

        if childTrans then
            if type(widget.src) == "table" then
                local luaCom = widget.src.new()
                luaCom = luaCom:bind(childTrans , widget)
                panel[widget.field] = luaCom
            else
                panel[widget.field] = childTrans.gameObject:GetComponent(widget.src)
            end
        else
            Debugger.Log("Cant find Child ! Path is " .. widgetPath)
        end
    end
end

return LuaUIHelper
