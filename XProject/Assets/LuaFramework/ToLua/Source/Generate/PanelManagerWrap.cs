﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class PanelManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(PanelManager), typeof(Riverlake.Singleton<PanelManager>));
		L.RegFunction("GetSingleton", GetSingleton);
		L.RegFunction("CreatePanel", CreatePanel);
		L.RegFunction("ClosePanel", ClosePanel);
		L.RegFunction("Clear", Clear);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetSingleton(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			PanelManager o = PanelManager.GetSingleton();
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreatePanel(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3)
			{
				PanelManager obj = (PanelManager)ToLua.CheckObject<PanelManager>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				obj.CreatePanel(arg0, arg1);
				return 0;
			}
			else if (count == 4)
			{
				PanelManager obj = (PanelManager)ToLua.CheckObject<PanelManager>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				LuaFunction arg2 = ToLua.CheckLuaFunction(L, 4);
				obj.CreatePanel(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: PanelManager.CreatePanel");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClosePanel(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			PanelManager obj = (PanelManager)ToLua.CheckObject<PanelManager>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.ClosePanel(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Clear(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			PanelManager obj = (PanelManager)ToLua.CheckObject<PanelManager>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int op_Equality(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
			UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
			bool o = arg0 == arg1;
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

