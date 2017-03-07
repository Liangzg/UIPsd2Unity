--[[--
创建一个类
~~~ lua
-- 定义名为 Shape 的基础类
local Shape = class("Shape")
-- ctor() 是类的构造函数，在调用 Shape.new() 创建 Shape 对象实例时会自动执行
function Shape:ctor(shapeName)
    self.shapeName = shapeName
    printf("Shape:ctor(%s)", self.shapeName)
end
-- 为 Shape 定义个名为 draw() 的方法
function Shape:draw()
    printf("draw %s", self.shapeName)
end
--
-- Circle 是 Shape 的继承类
local Circle = class("Circle", Shape)
function Circle:ctor()
    -- 如果继承类覆盖了 ctor() 构造函数，那么必须手动调用父类构造函数
    -- 类名.super 可以访问指定类的父类
    Circle.super.ctor(self, "circle")
    self.radius = 100
end
function Circle:setRadius(radius)
    self.radius = radius
end
-- 覆盖父类的同名方法
function Circle:draw()
    printf("draw %s, raidus = %0.2f", self.shapeName, self.raidus)
end
--
local Rectangle = class("Rectangle", Shape)
function Rectangle:ctor()
    Rectangle.super.ctor(self, "rectangle")
end
--
local circle = Circle.new()             -- 输出: Shape:ctor(circle)
circle:setRaidus(200)
circle:draw()                           -- 输出: draw circle, radius = 200.00
local rectangle = Rectangle.new()       -- 输出: Shape:ctor(rectangle)
rectangle:draw()                        -- 输出: draw rectangle
~~~
### 高级用法
class() 除了定义纯 Lua 类之外，还可以从 C++ 对象继承类。
比如需要创建一个工具栏，并在添加按钮时自动排列已有的按钮，那么我们可以使用如下的代码：
~~~ lua
-- 从 CCNode 对象派生 Toolbar 类，该类具有 CCNode 的所有属性和行为
local Toolbar = class("Toolbar", function()
    return display.newNode() -- 返回一个 CCNode 对象
end)
-- 构造函数
function Toolbar:ctor()
    self.buttons = {} -- 用一个 table 来记录所有的按钮
end
-- 添加一个按钮，并且自动设置按钮位置
function Toolbar:addButton(button)
    -- 将按钮对象加入 table
    self.buttons[#self.buttons + 1] = button
    -- 添加按钮对象到 CCNode 中，以便显示该按钮
    -- 因为 Toolbar 是从 CCNode 继承的，所以可以使用 addChild() 方法
    self:addChild(button)
    -- 按照按钮数量，调整所有按钮的位置
    local x = 0
    for _, button in ipairs(self.buttons) do
        button:setPosition(x, 0)
        -- 依次排列按钮，每个按钮之间间隔 10 点
        x = x + button:getContentSize().width + 10
    end
end
~~~
class() 的这种用法让我们可以在 C++ 对象基础上任意扩展行为。
既然是继承，自然就可以覆盖 C++ 对象的方法：
~~~ lua
function Toolbar:setPosition(x, y)
    -- 由于在 Toolbar 继承类中覆盖了 CCNode 对象的 setPosition() 方法
    -- 所以我们要用以下形式才能调用到 CCNode 原本的 setPosition() 方法
    getmetatable(self).setPosition(self, x, y)
    printf("x = %0.2f, y = %0.2f", x, y)
end
~~~
**注意:** Lua 继承类覆盖的方法并不能从 C++ 调用到。也就是说通过 C++ 代码调用这个 CCNode 对象的 setPosition() 方法时，并不会执行我们在 Lua 中定义的 Toolbar:setPosition() 方法。
@param string classname 类名
@param [mixed super] 父类或者创建对象实例的函数
@return table
]]
function class(classname, super)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k,v in pairs(super) do cls[k] = v end
            cls.__create = super.__create
            cls.super    = super
        else
            cls.__create = super
            cls.ctor = function() end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.new(...)
            local instance = cls.__create(...)
            -- copy fields from class to native object
            for k,v in pairs(cls) do instance[k] = v end
            instance.class = cls
            instance:ctor(...)
            return instance
        end

    else
        -- inherited from Lua Object
        if super then
            cls = {}
            setmetatable(cls, {__index = super})
            cls.super = super
        else
            cls = {ctor = function() end}
        end

        cls.__cname = classname
        cls.__ctype = 2 -- lua
        cls.__index = cls

        function cls.new(...)
            local instance = setmetatable({}, cls)
            instance.class = cls
            instance:ctor(...)
            return instance
        end
    end

    return cls
end


-- 提供假名以避免和 moonscript 发生冲突
function quick_class(classname, super)
  return class(classname, super)
end


--[[--
如果对象是指定类或其子类的实例，返回 true，否则返回 false
~~~ lua
local Animal = class("Animal")
local Duck = class("Duck", Animal)
print(iskindof(Duck.new(), "Animal")) -- 输出 true
~~~
@param mixed obj 要检查的对象
@param string classname 类名
@return boolean
]]
function iskindof(obj, classname)
    local t = type(obj)
    local mt
    if t == "table" then
        mt = getmetatable(obj)
    elseif t == "userdata" then
        mt = tolua.getpeer(obj)
    end

    while mt do
        if mt.__cname == classname then
            return true
        end
        mt = mt.super
    end

    return false
end


function asClass(obj , classname)
    return iskindof(obj , classname) and self or nil
end

--[[--
载入一个模块
import() 与 require() 功能相同，但具有一定程度的自动化特性。
假设我们有如下的目录结构：
~~~
app/
app/classes/
app/classes/MyClass.lua
app/classes/MyClassBase.lua
app/classes/data/Data1.lua
app/classes/data/Data2.lua
~~~
MyClass 中需要载入 MyClassBase 和 MyClassData。如果用 require()，MyClass 内的代码如下：
~~~ lua
local MyClassBase = require("app.classes.MyClassBase")
local MyClass = class("MyClass", MyClassBase)
local Data1 = require("app.classes.data.Data1")
local Data2 = require("app.classes.data.Data2")
~~~
假如我们将 MyClass 及其相关文件换一个目录存放，那么就必须修改 MyClass 中的 require() 命令，否则将找不到模块文件。
而使用 import()，我们只需要如下写：
~~~ lua
local MyClassBase = import(".MyClassBase")
local MyClass = class("MyClass", MyClassBase)
local Data1 = import(".data.Data1")
local Data2 = import(".data.Data2")
~~~
当在模块名前面有一个"." 时，import() 会从当前模块所在目录中查找其他模块。因此 MyClass 及其相关文件不管存放到什么目录里，我们都不再需要修改 MyClass 中的 import() 命令。
这在开发一些重复使用的功能组件时，会非常方便。
我们可以在模块名前添加多个"." ，这样 import() 会从更上层的目录开始查找模块。
~
不过 import() 只有在模块级别调用（也就是没有将 import() 写在任何函数中）时，才能够自动得到当前模块名。如果需要在函数中调用 import()，那么就需要指定当前模块名：
~~~ lua
# MyClass.lua
# 这里的 ... 是隐藏参数，包含了当前模块的名字，所以最好将这行代码写在模块的第一行
local CURRENT_MODULE_NAME = ...
local function testLoad()
    local MyClassBase = import(".MyClassBase", CURRENT_MODULE_NAME)
    # 更多代码
end
~~~
@param string moduleName 要载入的模块的名字
@param [string currentModuleName] 当前模块名
@return module
]]
function import(moduleName, currentModuleName)
    local currentModuleNameParts
    local moduleFullName = moduleName
    local offset = 1

    while true do
        if string.byte(moduleName, offset) ~= 46 then -- .
            moduleFullName = string.sub(moduleName, offset)
            if currentModuleNameParts and #currentModuleNameParts > 0 then
                moduleFullName = table.concat(currentModuleNameParts, ".") .. "." .. moduleFullName
            end
            break
        end
        offset = offset + 1

        if not currentModuleNameParts then
            if not currentModuleName then
                local n,v = debug.getlocal(3, 1)
                currentModuleName = v
            end

            currentModuleNameParts = string.split(currentModuleName, ".")
        end
        table.remove(currentModuleNameParts, #currentModuleNameParts)
    end

    return require(moduleFullName)
end



function implement( srcObj , interface )
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif srcObj[object] then
            return srcObj[object]
        end

        for key, value in pairs(object) do            
            srcObj[_copy(key)] = _copy(value)
        end
    end
    _copy(interface)
end