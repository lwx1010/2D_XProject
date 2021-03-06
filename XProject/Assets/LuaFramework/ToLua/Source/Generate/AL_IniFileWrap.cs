﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class AL_IniFileWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(AL.IniFile), typeof(System.Object));
		L.RegFunction("Clear", Clear);
		L.RegFunction("Read_From_String", Read_From_String);
		L.RegFunction("Read_From_Resource", Read_From_Resource);
		L.RegFunction("Read_From_File", Read_From_File);
		L.RegFunction("goTo", goTo);
		L.RegFunction("GetValue_String", GetValue_String);
		L.RegFunction("GetValue_Int", GetValue_Int);
		L.RegFunction("GetValue_Float", GetValue_Float);
		L.RegFunction("IsContainsName", IsContainsName);
		L.RegFunction("SetSelctor", SetSelctor);
		L.RegFunction("SetString", SetString);
		L.RegFunction("SetInt", SetInt);
		L.RegFunction("SetFloat", SetFloat);
		L.RegFunction("RemoveSelector", RemoveSelector);
		L.RegFunction("ToString", ToString);
		L.RegFunction("New", _CreateAL_IniFile);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateAL_IniFile(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				AL.IniFile obj = new AL.IniFile();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes<bool>(L, 1))
			{
				bool arg0 = LuaDLL.lua_toboolean(L, 1);
				AL.IniFile obj = new AL.IniFile(arg0);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes<string>(L, 1))
			{
				string arg0 = ToLua.ToString(L, 1);
				AL.IniFile obj = new AL.IniFile(arg0);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else if (count == 2)
			{
				string arg0 = ToLua.CheckString(L, 1);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 2);
				AL.IniFile obj = new AL.IniFile(arg0, arg1);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: AL.IniFile.New");
			}
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
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Read_From_String(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.Read_From_String(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Read_From_Resource(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.Read_From_Resource(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Read_From_File(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.Read_From_File(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int goTo(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			bool o = obj.goTo(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetValue_String(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				string o = obj.GetValue_String(arg0);
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else if (count == 3)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				string arg1 = ToLua.CheckString(L, 3);
				string o = obj.GetValue_String(arg0, arg1);
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: AL.IniFile.GetValue_String");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetValue_Int(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				int o = obj.GetValue_Int(arg0);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 3)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				int o = obj.GetValue_Int(arg0, arg1);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: AL.IniFile.GetValue_Int");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetValue_Float(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				float o = obj.GetValue_Float(arg0);
				LuaDLL.lua_pushnumber(L, o);
				return 1;
			}
			else if (count == 3)
			{
				AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				float o = obj.GetValue_Float(arg0, arg1);
				LuaDLL.lua_pushnumber(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: AL.IniFile.GetValue_Float");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsContainsName(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			bool o = obj.IsContainsName(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetSelctor(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetSelctor(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			string arg1 = ToLua.CheckString(L, 3);
			obj.SetString(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetInt(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			obj.SetInt(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetFloat(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			obj.SetFloat(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveSelector(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.RemoveSelector(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ToString(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			AL.IniFile obj = (AL.IniFile)ToLua.CheckObject<AL.IniFile>(L, 1);
			string o = obj.ToString();
			LuaDLL.lua_pushstring(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

