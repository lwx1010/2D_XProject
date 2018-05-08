﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class Riverlake_Resources_ALoadOperationWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(Riverlake.Resources.ALoadOperation), typeof(System.Object));
		L.RegFunction("MoveNext", MoveNext);
		L.RegFunction("Reset", Reset);
		L.RegFunction("OnLoad", OnLoad);
		L.RegFunction("IsDone", IsDone);
		L.RegFunction("Finish", Finish);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("OnFinish", get_OnFinish, set_OnFinish);
		L.RegVar("Progress", get_Progress, null);
		L.RegVar("assetPath", get_assetPath, null);
		L.RegVar("Current", get_Current, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int MoveNext(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 1);
			bool o = obj.MoveNext();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Reset(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 1);
			obj.Reset();
			return 0;
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
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 1);
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
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 1);
			bool o = obj.IsDone();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Finish(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 1);
			Riverlake.Resources.ALoadOperation arg0 = (Riverlake.Resources.ALoadOperation)ToLua.CheckObject<Riverlake.Resources.ALoadOperation>(L, 2);
			obj.Finish(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_OnFinish(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)o;
			System.Action<Riverlake.Resources.ALoadOperation> ret = obj.OnFinish;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index OnFinish on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Progress(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)o;
			float ret = obj.Progress;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index Progress on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_assetPath(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)o;
			string ret = obj.assetPath;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index assetPath on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Current(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)o;
			object ret = obj.Current;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index Current on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_OnFinish(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.Resources.ALoadOperation obj = (Riverlake.Resources.ALoadOperation)o;
			System.Action<Riverlake.Resources.ALoadOperation> arg0 = (System.Action<Riverlake.Resources.ALoadOperation>)ToLua.CheckDelegate<System.Action<Riverlake.Resources.ALoadOperation>>(L, 2);
			obj.OnFinish = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index OnFinish on a nil value");
		}
	}
}

