﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class AL_Resources_LoadDelayAsyncWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(AL.Resources.LoadDelayAsync), typeof(AL.Resources.ALoadOperation));
		L.RegFunction("OnLoad", OnLoad);
		L.RegFunction("IsDone", IsDone);
		L.RegFunction("New", _CreateAL_Resources_LoadDelayAsync);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateAL_Resources_LoadDelayAsync(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				float arg0 = (float)LuaDLL.luaL_checknumber(L, 1);
				AL.Resources.LoadDelayAsync obj = new AL.Resources.LoadDelayAsync(arg0);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: AL.Resources.LoadDelayAsync.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnLoad(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			AL.Resources.LoadDelayAsync obj = (AL.Resources.LoadDelayAsync)ToLua.CheckObject<AL.Resources.LoadDelayAsync>(L, 1);
			obj.OnLoad();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsDone(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			AL.Resources.LoadDelayAsync obj = (AL.Resources.LoadDelayAsync)ToLua.CheckObject<AL.Resources.LoadDelayAsync>(L, 1);
			bool o = obj.IsDone();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}
