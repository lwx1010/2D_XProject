﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class NativeSDKWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(NativeSDK), typeof(System.Object));
		L.RegFunction("StartPushService", StartPushService);
		L.RegFunction("New", _CreateNativeSDK);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateNativeSDK(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				NativeSDK obj = new NativeSDK();
				ToLua.PushSealed(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: NativeSDK.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StartPushService(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			NativeSDK.StartPushService(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

