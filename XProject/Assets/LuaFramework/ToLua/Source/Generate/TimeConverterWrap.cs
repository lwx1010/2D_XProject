﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class TimeConverterWrap
{
	public static void Register(LuaState L)
	{
		L.BeginStaticLibs("TimeConverter");
		L.RegFunction("ConvertIntDateTime", ConvertIntDateTime);
		L.RegFunction("ConvertDateTimeInt", ConvertDateTimeInt);
		L.RegFunction("ConvertDateTimeLong", ConvertDateTimeLong);
		L.RegFunction("CovertToString", CovertToString);
		L.RegFunction("ConvertToHoursString", ConvertToHoursString);
		L.RegFunction("ConvertToDateString", ConvertToDateString);
		L.RegFunction("ConvertToLogDateString", ConvertToLogDateString);
		L.RegFunction("ConvertToDateString1", ConvertToDateString1);
		L.RegFunction("Parse", Parse);
		L.EndStaticLibs();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertIntDateTime(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			double arg0 = (double)LuaDLL.luaL_checknumber(L, 1);
			System.DateTime o = TimeConverter.ConvertIntDateTime(arg0);
			ToLua.PushValue(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertDateTimeInt(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			System.DateTime arg0 = StackTraits<System.DateTime>.Check(L, 1);
			double o = TimeConverter.ConvertDateTimeInt(arg0);
			LuaDLL.lua_pushnumber(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertDateTimeLong(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			System.DateTime arg0 = StackTraits<System.DateTime>.Check(L, 1);
			long o = TimeConverter.ConvertDateTimeLong(arg0);
			LuaDLL.tolua_pushint64(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CovertToString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 1);
			string o = TimeConverter.CovertToString(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertToHoursString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 1);
			string o = TimeConverter.ConvertToHoursString(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertToDateString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			double arg0 = (double)LuaDLL.luaL_checknumber(L, 1);
			string o = TimeConverter.ConvertToDateString(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertToLogDateString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			double arg0 = (double)LuaDLL.luaL_checknumber(L, 1);
			string o = TimeConverter.ConvertToLogDateString(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ConvertToDateString1(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			double arg0 = (double)LuaDLL.luaL_checknumber(L, 1);
			string o = TimeConverter.ConvertToDateString1(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Parse(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			System.DateTime o = TimeConverter.Parse(arg0);
			ToLua.PushValue(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

