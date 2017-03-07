
--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
	GameObject.Destroy(obj);
end

function newObject(prefab)
	return GameObject.Instantiate(prefab);
end

--创建面板--
function createPanel(name)
	PanelManager:CreatePanel(name);
end

function child(str)
	return transform:FindChild(str);
end

function subGet(childNode, typeName)
	return child(childNode):GetComponent(typeName);
end

function findPanel(str)
	local obj = find(str);
	if obj == nil then
		error(str.." is null");
		return nil;
	end
	return obj:GetComponent("BaseLua");
end


--[[--
提供一组常用函数，以及对 Lua 标准库的扩展
]]

--[[--
输出格式化字符串
~~~ lua
printf("The value = %d", 100)
~~~
@param string fmt 输出格式
@param [mixed ...] 更多参数
]]
function printf(fmt, ...)
    print(string.format(tostring(fmt), ...))
end

--[[--
检查并尝试转换为数值，如果无法转换则返回 0
@param mixed value 要检查的值
@param [integer base] 进制，默认为十进制
@return number
]]
function checknumber(value, base)
    return tonumber(value, base) or 0
end

--[[--
检查并尝试转换为整数，如果无法转换则返回 0
@param mixed value 要检查的值
@return integer
]]
function checkint(value)
    return math.round(checknumber(value))
end

--[[--
检查并尝试转换为布尔值，除了 nil 和 false，其他任何值都会返回 true
@param mixed value 要检查的值
@return boolean
]]
function checkbool(value)
    return (value ~= nil and value ~= false)
end

--[[--
检查值是否是一个表格，如果不是则返回一个空表格
@param mixed value 要检查的值
@return table
]]
function checktable(value)
    if type(value) ~= "table" then value = {} end
    return value
end

--[[--
如果表格中指定 key 的值为 nil，或者输入值不是表格，返回 false，否则返回 true
@param table hashtable 要检查的表格
@param mixed key 要检查的键名
@return boolean
]]
function isset(hashtable, key)
    local t = type(hashtable)
    return (t == "table" or t == "userdata") and hashtable[key] ~= nil
end

--[[--
深度克隆一个值
~~~ lua
-- 下面的代码，t2 是 t1 的引用，修改 t2 的属性时，t1 的内容也会发生变化
local t1 = {a = 1, b = 2}
local t2 = t1
t2.b = 3    -- t1 = {a = 1, b = 3} <-- t1.b 发生变化
-- clone() 返回 t1 的副本，修改 t2 不会影响 t1
local t1 = {a = 1, b = 2}
local t2 = clone(t1)
t2.b = 3    -- t1 = {a = 1, b = 2} <-- t1.b 不受影响
~~~
@param mixed object 要克隆的值
@return mixed
]]
function clone(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local new_table = {}
        lookup_table[object] = new_table
        for key, value in pairs(object) do
            new_table[_copy(key)] = _copy(value)
        end
        return setmetatable(new_table, getmetatable(object))
    end
    return _copy(object)
end



function internale(lua_table,indent,depth,isLimit)
  local str = {}
  local prefix = string.rep("    ", indent)
  table.insert(str,"\n"..prefix.."{\n")
  for k, v in pairs(lua_table) do
      if type(k) == "string" then
          k = string.format("%q", k)
      else
          k = string.format("%s", tostring(k))
      end

      local szSuffix = ""

      if type(v) == "string" then
          szSuffix = string.format("%q", v)
      elseif type(v) == "number" or type(v) == "userdata" then
          szSuffix = tostring(v)
      elseif type(v) == "table" then
          if isLimit then
              if (depth > 0) then
                  szSuffix = internale(v,indent + 1,depth - 1,isLimit)
              else
                  szSuffix = tostring(v)
              end
          else

              szSuffix = print_lua_table(v,indent + 1,depth - 1)
          end
      else
         szSuffix = tostring(v)
      end

      local szPrefix = string.rep("    ", indent+1)
      table.insert(str,szPrefix.."["..k.."]".." = "..szSuffix..",\n")
   end

   table.insert(str,prefix.."}\n")
   return table.concat(str, '')
end

--輸出信息，缩进，遍历深度,不设置上限
function print_lua_table(lua_table, indent,depth,isLimit)
    indent = indent or 0
    depth = depth or 1
    isLimit = isLimit or false
    local str = ""

    if lua_table == nil then
        str = tostring(lua_table)
    end

    if type(lua_table) == "string" then
        str = string.format("%q", lua_table)
    end

    if type(lua_table) == "userdata" or type(lua_table) == "number" or type(lua_table) == "function" or type(lua_table) == "boolean" then
        str = tostring(lua_table)
    end

    if type(lua_table) == "table" then
        str = internale(lua_table,indent,depth,isLimit)
    end

    return str
end


--[[--
检查指定的文件或目录是否存在，如果存在返回 true，否则返回 false
可以使用 CCFileUtils:fullPathForFilename() 函数查找特定文件的完整路径，例如：
~~~ lua
local path = CCFileUtils:sharedFileUtils():fullPathForFilename("gamedata.txt")
if io.exists(path) then
    ....
end
~~~
@param string path 要检查的文件或目录的完全路径
@return boolean
]]
function io.exists(path)
    local file = io.open(path, "r")
    if file then
        io.close(file)
        return true
    end
    return false
end

--[[--
读取文件内容，返回包含文件内容的字符串，如果失败返回 nil
io.readfile() 会一次性读取整个文件的内容，并返回一个字符串，因此该函数不适宜读取太大的文件。
@param string path 文件完全路径
@return string
]]
function io.readfile(path)
    local file = io.open(path, "r")
    if file then
        local content = file:read("*a")
        io.close(file)
        return content
    end
    return nil
end

--[[--
以字符串内容写入文件，成功返回 true，失败返回 false
"mode 写入模式" 参数决定 io.writefile() 如何写入内容，可用的值如下：
-   "w+" : 覆盖文件已有内容，如果文件不存在则创建新文件
-   "a+" : 追加内容到文件尾部，如果文件不存在则创建文件
此外，还可以在 "写入模式" 参数最后追加字符 "b" ，表示以二进制方式写入数据，这样可以避免内容写入不完整。
**Android 特别提示:** 在 Android 平台上，文件只能写入存储卡所在路径，assets 和 data 等目录都是无法写入的。
@param string path 文件完全路径
@param string content 要写入的内容
@param [string mode] 写入模式，默认值为 "w+b"
@return boolean
]]
function io.writefile(path, content, mode)
    mode = mode or "w+b"
    local file = io.open(path, mode)
    if file then
        if file:write(content) == nil then return false end
        io.close(file)
        return true
    else
        return false
    end
end

--[[--
拆分一个路径字符串，返回组成路径的各个部分
~~~ lua
local pathinfo  = io.pathinfo("/var/app/test/abc.png")
-- 结果:
-- pathinfo.dirname  = "/var/app/test/"
-- pathinfo.filename = "abc.png"
-- pathinfo.basename = "abc"
-- pathinfo.extname  = ".png"
~~~
@param string path 要分拆的路径字符串
@return table
]]
function io.pathinfo(path)
    local pos = string.len(path)
    local extpos = pos + 1
    while pos > 0 do
        local b = string.byte(path, pos)
        if b == 46 then -- 46 = char "."
            extpos = pos
        elseif b == 47 then -- 47 = char "/"
            break
        end
        pos = pos - 1
    end

    local dirname = string.sub(path, 1, pos)
    local filename = string.sub(path, pos + 1)
    extpos = extpos - pos
    local basename = string.sub(filename, 1, extpos - 1)
    local extname = string.sub(filename, extpos)
    return {
        dirname = dirname,
        filename = filename,
        basename = basename,
        extname = extname
    }
end

--[[--
返回指定文件的大小，如果失败返回 false
@param string path 文件完全路径
@return integer
]]
function io.filesize(path)
    local size = false
    local file = io.open(path, "r")
    if file then
        local current = file:seek()
        size = file:seek("end")
        file:seek("set", current)
        io.close(file)
    end
    return size
end
