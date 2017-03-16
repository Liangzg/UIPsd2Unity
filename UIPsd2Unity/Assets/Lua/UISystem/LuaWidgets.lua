-- @Author: LiangZG
-- @Date:   2017-03-16 16:49:14
-- @Last Modified time: 2017-03-16 17:12:22

--UI组件

-------------脚本-----------------
LuaScript = class("LuaScript")

function LuaScript:bind( root , widget )
    local tab = require(widget.requirePath)
    tab = tab.new()
    tab.gameObject = root.gameObject
    tab.transform = root

    tab:Awake()
    return tab
end


--------------基础图片---------------

LuaImage = class("LuaImage")
implement(LuaImage , LuaBaseWidget)

function LuaImage:bind( trans , widget )

    local sprite = trans.gameObject:GetComponent(typeof(UISprite))
    tolua.setpeer(sprite , self)

    return sprite
end

--------------基础文本------------------------

LuaText = class("LuaText")
implement(LuaText , LuaBaseWidget)

function LuaText:bind( trans , widget )
    local lab = trans.gameObject:GetComponent(typeof(UILabel))
    tolua.setpeer(lab , self)
    return lab
end


-----------------按妞------------------------
LuaButton = class("LuaButton")
implement(LuaButton , LuaBaseWidget)

function LuaButton:bind( root , widget , behaviour)
    -- local btn = root.gameObject:GetComponent(typeof(UIButton))
    -- tolua.setpeer(btn , self)

    local btn = self

    self.transform = root
    self.gameObject = root.gameObject

    if widget.onClick then
        behaviour:AddClick(root.gameObject , widget.onClick)

        btn = root.gameObject:GetComponent(typeof(UIEventListener))
        tolua.setpeer(btn , self)
    else
        print("cant find button click !~")
    end

    return btn
end

function LuaButton:luaButton( )
    print("---Lua Button function ----")
end


-------------------Toggle 切换按钮-----------------
LuaToggle = class("LuaToggle")
implement(LuaToggle , LuaBaseWidget)

function LuaToggle:bind( trans , widget , behaviour)

    local toggle = trans.gameObject:GetComponent(typeof(UIToggle))
    tolua.setpeer(toggle , self)

    if widget.onChange then
        behaviour:AddToggleChange(trans.gameObject , widget.onChange)
    end

    return toggle
end

----------------输入框--------------------
LuaInput = class("LuaInput")
implement(LuaInput , LuaBaseWidget)

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

------------------Slider 进度条----------------------
LuaSlider = class("LuaSlider")
implement(LuaSlider , LuaBaseWidget)

function LuaSlider:bind( trans , widget ,behaviour)
    local com = trans.gameObject:GetComponent(typeof(UISlider))
    tolua.setpeer(com , self)

    if widget.onValueChange then
        behaviour:AddProgressBarChange(trans.gameObject , widget.onValueChange)
    end

    return com
end

------------------Slider 进度条----------------------
LuaScrollBar = class("LuaScrollBar")
implement(LuaScrollBar , LuaBaseWidget)

function LuaScrollBar:bind( trans , widget ,behaviour)
    local com = trans.gameObject:GetComponent(typeof(UIScrollBar))
    tolua.setpeer(com , self)

    if widget.onValueChange then
        behaviour:AddProgressBarChange(trans.gameObject , widget.onValueChange)
    end

    return com
end

--------------Panel-----------------
LuaPanel = class("LuaPanel")
implement(LuaPanel , LuaBaseWidget)

function LuaPanel:bind( trans , widget )
    local panel = trans.gameObject:GetComponent(typeof(UIPanel))
    tolua.setpeer(panel , self)

    return panel
end

