﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class LuaFramework_ThreadManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(LuaFramework.ThreadManager), typeof(Manager));
		L.RegFunction("Dispose", Dispose);
		L.RegFunction("DestroySelf", DestroySelf);
		L.RegFunction("AddEvent", AddEvent);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("PoolScheduler", get_PoolScheduler, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Dispose(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.ThreadManager obj = (LuaFramework.ThreadManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.ThreadManager));
			obj.Dispose();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DestroySelf(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.ThreadManager obj = (LuaFramework.ThreadManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.ThreadManager));
			obj.DestroySelf();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.ThreadManager obj = (LuaFramework.ThreadManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.ThreadManager));
			ThreadEvent arg0 = (ThreadEvent)ToLua.CheckObject(L, 2, typeof(ThreadEvent));
			obj.AddEvent(arg0);
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

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_PoolScheduler(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			LuaFramework.ThreadManager obj = (LuaFramework.ThreadManager)o;
			Frankfort.Threading.ThreadPoolScheduler ret = obj.PoolScheduler;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index PoolScheduler on a nil value");
		}
	}
}

