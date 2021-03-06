﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class MessageBoxWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(MessageBox), typeof(ModalBox));
		L.RegFunction("Show", Show);
		L.RegFunction("Close", Close);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("PrefabResourceName", get_PrefabResourceName, set_PrefabResourceName);
		L.RegVar("Localize", get_Localize, set_Localize);
		L.RegVar("LocalizeTitleAndMessage", get_LocalizeTitleAndMessage, set_LocalizeTitleAndMessage);
		L.RegFunction("OnDialogClicked", MessageBox_OnDialogClicked);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Show(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				string arg0 = ToLua.CheckString(L, 1);
				MessageBox o = MessageBox.Show(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<string>(L, 2))
			{
				string arg0 = ToLua.CheckString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				MessageBox o = MessageBox.Show(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<MessageBox.OnDialogClicked>(L, 2))
			{
				string arg0 = ToLua.CheckString(L, 1);
				MessageBox.OnDialogClicked arg1 = (MessageBox.OnDialogClicked)ToLua.ToObject(L, 2);
				MessageBox o = MessageBox.Show(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<string, MessageBox.OnDialogClicked>(L, 2))
			{
				string arg0 = ToLua.CheckString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				MessageBox.OnDialogClicked arg2 = (MessageBox.OnDialogClicked)ToLua.ToObject(L, 3);
				MessageBox o = MessageBox.Show(arg0, arg1, arg2);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<MessageBox.OnDialogClicked, MessageBoxButtons>(L, 2))
			{
				string arg0 = ToLua.CheckString(L, 1);
				MessageBox.OnDialogClicked arg1 = (MessageBox.OnDialogClicked)ToLua.ToObject(L, 2);
				MessageBoxButtons arg2 = (MessageBoxButtons)ToLua.ToObject(L, 3);
				MessageBox o = MessageBox.Show(arg0, arg1, arg2);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 4)
			{
				string arg0 = ToLua.CheckString(L, 1);
				string arg1 = ToLua.CheckString(L, 2);
				MessageBox.OnDialogClicked arg2 = (MessageBox.OnDialogClicked)ToLua.CheckDelegate<MessageBox.OnDialogClicked>(L, 3);
				MessageBoxButtons arg3 = (MessageBoxButtons)ToLua.CheckObject(L, 4, typeof(MessageBoxButtons));
				MessageBox o = MessageBox.Show(arg0, arg1, arg2, arg3);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: MessageBox.Show");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Close(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			MessageBox obj = (MessageBox)ToLua.CheckObject<MessageBox>(L, 1);
			obj.Close();
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
	static int get_PrefabResourceName(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushstring(L, MessageBox.PrefabResourceName);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Localize(IntPtr L)
	{
		try
		{
			ToLua.Push(L, MessageBox.Localize);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_LocalizeTitleAndMessage(IntPtr L)
	{
		try
		{
			LuaDLL.lua_pushboolean(L, MessageBox.LocalizeTitleAndMessage);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_PrefabResourceName(IntPtr L)
	{
		try
		{
			string arg0 = ToLua.CheckString(L, 2);
			MessageBox.PrefabResourceName = arg0;
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_Localize(IntPtr L)
	{
		try
		{
			System.Func<string,string> arg0 = (System.Func<string,string>)ToLua.CheckDelegate<System.Func<string,string>>(L, 2);
			MessageBox.Localize = arg0;
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_LocalizeTitleAndMessage(IntPtr L)
	{
		try
		{
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			MessageBox.LocalizeTitleAndMessage = arg0;
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int MessageBox_OnDialogClicked(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);
			LuaFunction func = ToLua.CheckLuaFunction(L, 1);

			if (count == 1)
			{
				Delegate arg1 = DelegateTraits<MessageBox.OnDialogClicked>.Create(func);
				ToLua.Push(L, arg1);
			}
			else
			{
				LuaTable self = ToLua.CheckLuaTable(L, 2);
				Delegate arg1 = DelegateTraits<MessageBox.OnDialogClicked>.Create(func, self);
				ToLua.Push(L, arg1);
			}
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

