-- @Author: LiangZG
-- @Date:   2017-03-07 16:07:20
-- @Last Modified time: 2017-03-13 12:03:42

require "class"
require "helper"

require "UISystem.init"

function Main(  )
    print(" Start Lua Main ... ")

    require "LuaExport/miniMap"
    local testObj = UnityEngine.GameObject.Find("miniMap");
    testObj:AddComponent(typeof(LuaFramework.LuaBehaviour))
end
