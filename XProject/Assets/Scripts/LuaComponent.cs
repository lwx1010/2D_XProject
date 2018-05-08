using UnityEngine;
using System.Collections;
using LuaInterface;
using LuaFramework;
using System.Collections.Generic;

public sealed class LuaComponent : MonoBehaviour
{
    //Lua表
    public LuaTable table;

    private LuaFunction _callback;

    private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();

    //添加LUA组件  
    public static LuaTable Add(GameObject go, LuaTable tableClass)
    {
        //LuaFunction fun = tableClass.GetLuaFunction("New");
        //if (fun == null)
        //    return null;

        //object[] rets = fun.Call(tableClass);
        //if (rets.Length != 1)
        //    return null;

        LuaComponent cmp = go.transform.GetOrAddComponent<LuaComponent>();
        cmp.table = tableClass;
        //cmp._callback = callback;
        cmp.CallAwake();
        return cmp.table;
    }

    //获取lua组件
    public static LuaTable Get(GameObject go, LuaTable table)
    {
        LuaComponent[] cmps = go.GetComponents<LuaComponent>();
        var count = cmps.Length;
        for(var i=0;i<count;i++)
        {
            string mat1 = table.ToString();
            string mat2 = cmps[i].table.GetMetaTable().ToString();
            if (mat1 == mat2)
            {
                return cmps[i].table;
            }
        }
        return null;
    }
    //删除LUA组件的方法略，调用Destory()即可  

    void CallAwake()
    {
        LuaFunction fun = table.GetLuaFunction("OnCreate");
        if (fun != null)
            fun.Call(table, gameObject);
    }

    void OnEnable()
    {
        if (table == null)
            return;
        LuaFunction fun = table.GetLuaFunction("OnEnable");
        if (fun != null)
            fun.Call(table, gameObject);
    }

    void Start()
    {
        LuaFunction fun = table.GetLuaFunction("Start");
        if (fun != null)
            fun.Call(table, gameObject);

        if (_callback != null)
            _callback.Call();
    }

    void OnDestroy()
    {
        if (table == null)
            return;
        LuaFunction fun = table.GetLuaFunction("OnDestroy");
        if (fun != null)
            fun.Call(table);
    }

    void Update()
    {
        //效率问题有待测试和优化
        //可在lua中调用UpdateBeat替代
        //LuaFunction fun = table.GetLuaFunction("Update");
        //if (fun != null)
        //    fun.Call(table, gameObject);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        //略
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    public void AddClick(GameObject go, LuaFunction luafunc)
    {
        //if (go == null || luafunc == null) return;
        //buttons.Add(go.name, luafunc);
        //UIEventListener.Get(go).onClick = delegate (GameObject o) {
        //    LuaBehaviour.ExtendEventDeal(go, "luaComponent");
        //    luafunc.Call(go);
        //};
    }

    /// <summary>
    /// 添加选择事件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="luafunc"></param>
    public void AddSelect(GameObject go, LuaFunction luafunc)
    {
        if (go == null || luafunc == null) return;
        //UIEventListener.Get(go).onSelect = delegate (GameObject o, bool selected)
        //{
        //    //if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
        //    //    return;

        //    luafunc.Call(go, selected);
        //};
    }
}