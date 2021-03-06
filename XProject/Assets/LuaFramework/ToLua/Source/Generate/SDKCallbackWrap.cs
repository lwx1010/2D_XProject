﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class SDKCallbackWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(SDKCallback), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("InitCallback", InitCallback);
		L.RegFunction("OnGetSDKInfo", OnGetSDKInfo);
		L.RegFunction("OnInitSuc", OnInitSuc);
		L.RegFunction("OnLoginSuc", OnLoginSuc);
		L.RegFunction("OnSwitchLogin", OnSwitchLogin);
		L.RegFunction("OnLogout", OnLogout);
		L.RegFunction("OnPaySuc", OnPaySuc);
		L.RegFunction("OnMemoryWarning", OnMemoryWarning);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int InitCallback(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			SDKCallback o = SDKCallback.InitCallback();
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnGetSDKInfo(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			string arg0 = ToLua.CheckString(L, 2);
			obj.OnGetSDKInfo(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnInitSuc(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			string arg0 = ToLua.CheckString(L, 2);
			obj.OnInitSuc(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnLoginSuc(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			string arg0 = ToLua.CheckString(L, 2);
			obj.OnLoginSuc(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnSwitchLogin(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			obj.OnSwitchLogin();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnLogout(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			obj.OnLogout();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnPaySuc(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			string arg0 = ToLua.CheckString(L, 2);
			obj.OnPaySuc(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnMemoryWarning(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			SDKCallback obj = (SDKCallback)ToLua.CheckObject(L, 1, typeof(SDKCallback));
			obj.OnMemoryWarning();
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

