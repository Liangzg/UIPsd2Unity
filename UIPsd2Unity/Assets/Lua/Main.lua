-- @Author: LiangZG
-- @Date:   2017-03-07 16:07:20
-- @Last Modified time: 2017-03-07 17:43:04

require "class"
require "helper"

require "UISystem.init"

function Main(  )
    print(" Start Lua Main ... ")

    require "LuaExport/TestView"
    local testObj = UnityEngine.GameObject.Find("miniMap");
    TestView.Awake(testObj)

    TestView.Start()

    -- print(print_lua_table(TestView , 0 , 3 , true))
end
