using UnityEngine;
using System.Collections;
using LuaInterface;

public class LuaClientStarter : MonoBehaviour {

    private LuaState lua;
    private LuaLoader loader;
    private LuaLooper loop = null;

    // Use this for initialization
    void Awake()
    {
        loader = new LuaLoader();
        lua = new LuaState();
        this.OpenLibs();
        lua.LuaSetTop(0);

        LuaBinder.Bind(lua);
        LuaCoroutine.Register(lua, this);
    }

    public void Start()
    {
        InitLuaPath();
        this.lua.Start();    //启动LUAVM
        this.StartMain();
        this.StartLooper();
    }

    void StartLooper()
    {
        loop = gameObject.AddComponent<LuaLooper>();
        loop.luaState = lua;
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    protected void OpenCJson()
    {
        lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        lua.OpenLibs(LuaDLL.luaopen_cjson);
        lua.LuaSetField(-2, "cjson");

        lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        lua.LuaSetField(-2, "cjson.safe");
    }

    void StartMain()
    {
        lua.DoFile("Main.lua");

        LuaFunction main = lua.GetFunction("Main");
        main.Call();
        main.Dispose();
        main = null;
    }

    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    void OpenLibs()
    {
        lua.OpenLibs(LuaDLL.luaopen_pb);
//        lua.OpenLibs(LuaDLL.luaopen_sproto_core);
//        lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
        lua.OpenLibs(LuaDLL.luaopen_lpeg);
        lua.OpenLibs(LuaDLL.luaopen_bit);
        lua.OpenLibs(LuaDLL.luaopen_socket_core);

        this.OpenCJson();
    }

    /// <summary>
    /// 初始化Lua代码加载路径
    /// </summary>
    void InitLuaPath()
    {
        string rootPath = Application.dataPath;
        lua.AddSearchPath(rootPath + "/Lua");
        lua.AddSearchPath(rootPath + "/Libs/ToLua/Lua");
        
    }


    public object[] DoFile(string filename)
    {
        return lua.DoFile(filename);
    }

    // Update is called once per frame
    public object[] CallFunction(string funcName, params object[] args)
    {
        LuaFunction func = lua.GetFunction(funcName);
        if (func != null)
        {
            return func.Call(args);
        }
        return null;
    }

    public void LuaGC()
    {
        lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
    }

    public void Close()
    {
        if (loop != null) loop.Destroy();
        loop = null;

        if (lua != null) lua.Dispose();
        lua = null;
        loader = null;
    }
}
