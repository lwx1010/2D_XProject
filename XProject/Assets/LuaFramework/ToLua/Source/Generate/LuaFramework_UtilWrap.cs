﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class LuaFramework_UtilWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(LuaFramework.Util), typeof(System.Object));
		L.RegFunction("Int", Int);
		L.RegFunction("Float", Float);
		L.RegFunction("Long", Long);
		L.RegFunction("Random", Random);
		L.RegFunction("Uid", Uid);
		L.RegFunction("GetTime", GetTime);
		L.RegFunction("Child", Child);
		L.RegFunction("Peer", Peer);
		L.RegFunction("md5", md5);
		L.RegFunction("md5file", md5file);
		L.RegFunction("ClearChild", ClearChild);
		L.RegFunction("ClearMemory", ClearMemory);
		L.RegFunction("GetFileText", GetFileText);
		L.RegFunction("AppContentPath", AppContentPath);
		L.RegFunction("StreamingAssetsPath", StreamingAssetsPath);
		L.RegFunction("Log", Log);
		L.RegFunction("LogWarning", LogWarning);
		L.RegFunction("LogError", LogError);
		L.RegFunction("CheckRuntimeFile", CheckRuntimeFile);
		L.RegFunction("CallMethod", CallMethod);
		L.RegFunction("AutoAdjustCameraRect", AutoAdjustCameraRect);
		L.RegFunction("GetDeviceIdentifierString", GetDeviceIdentifierString);
		L.RegFunction("New", _CreateLuaFramework_Util);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("DataPath", get_DataPath, null);
		L.RegVar("NetAvailable", get_NetAvailable, null);
		L.RegVar("IsWifi", get_IsWifi, null);
		L.RegVar("LuaPath", get_LuaPath, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateLuaFramework_Util(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				LuaFramework.Util obj = new LuaFramework.Util();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: LuaFramework.Util.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Int(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			object arg0 = ToLua.ToVarObject(L, 1);
			int o = LuaFramework.Util.Int(arg0);
			LuaDLL.lua_pushinteger(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Float(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			object arg0 = ToLua.ToVarObject(L, 1);
			float o = LuaFramework.Util.Float(arg0);
			LuaDLL.lua_pushnumber(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Long(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			object arg0 = ToLua.ToVarObject(L, 1);
			long o = LuaFramework.Util.Long(arg0);
			LuaDLL.tolua_pushint64(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Random(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 1);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 2);
			float o = LuaFramework.Util.Random(arg0, arg1);
			LuaDLL.lua_pushnumber(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Uid(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			string o = LuaFramework.Util.Uid(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetTime(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			long o = LuaFramework.Util.GetTime();
			LuaDLL.tolua_pushint64(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Child(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Transform, string>(L, 1))
			{
				UnityEngine.Transform arg0 = (UnityEngine.Transform)ToLua.ToObject(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				UnityEngine.GameObject o = LuaFramework.Util.Child(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.GameObject, string>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				UnityEngine.GameObject o = LuaFramework.Util.Child(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.Util.Child");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Peer(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Transform, string>(L, 1))
			{
				UnityEngine.Transform arg0 = (UnityEngine.Transform)ToLua.ToObject(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				UnityEngine.GameObject o = LuaFramework.Util.Peer(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.GameObject, string>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				UnityEngine.GameObject o = LuaFramework.Util.Peer(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.Util.Peer");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int md5(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			string o = LuaFramework.Util.md5(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int md5file(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			string o = LuaFramework.Util.md5file(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClearChild(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.Transform arg0 = (UnityEngine.Transform)ToLua.CheckObject<UnityEngine.Transform>(L, 1);
			LuaFramework.Util.ClearChild(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClearMemory(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			LuaFramework.Util.ClearMemory();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetFileText(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			string o = LuaFramework.Util.GetFileText(arg0);
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AppContentPath(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			string o = LuaFramework.Util.AppContentPath();
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StreamingAssetsPath(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			string o = LuaFramework.Util.StreamingAssetsPath();
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Log(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			LuaFramework.Util.Log(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LogWarning(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			LuaFramework.Util.LogWarning(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LogError(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			LuaFramework.Util.LogError(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CheckRuntimeFile(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			int o = LuaFramework.Util.CheckRuntimeFile();
			LuaDLL.lua_pushinteger(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CallMethod(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);
			string arg0 = ToLua.CheckString(L, 1);
			string arg1 = ToLua.CheckString(L, 2);
			object[] arg2 = ToLua.ToParamsObject(L, 3, count - 2);
			object[] o = LuaFramework.Util.CallMethod(arg0, arg1, arg2);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AutoAdjustCameraRect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.Camera arg0 = (UnityEngine.Camera)ToLua.CheckObject(L, 1, typeof(UnityEngine.Camera));
			LuaFramework.Util.AutoAdjustCameraRect(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetDeviceIdentifierString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			string o = LuaFramework.Util.GetDeviceIdentifierString();
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_DataPath(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushstring(L, LuaFramework.Util.DataPath);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_NetAvailable(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushboolean(L, LuaFramework.Util.NetAvailable);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_IsWifi(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushboolean(L, LuaFramework.Util.IsWifi);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_LuaPath(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushstring(L, LuaFramework.Util.LuaPath);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

