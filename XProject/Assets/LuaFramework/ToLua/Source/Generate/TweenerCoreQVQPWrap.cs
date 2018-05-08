﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class TweenerCoreQVQPWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>), typeof(DG.Tweening.Tweener), "TweenerCoreQVQP");
		L.RegFunction("ChangeStartValue", ChangeStartValue);
		L.RegFunction("ChangeEndValue", ChangeEndValue);
		L.RegFunction("ChangeValues", ChangeValues);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("startValue", get_startValue, set_startValue);
		L.RegVar("endValue", get_endValue, set_endValue);
		L.RegVar("changeValue", get_changeValue, set_changeValue);
		L.RegVar("plugOptions", get_plugOptions, set_plugOptions);
		L.RegVar("getter", get_getter, set_getter);
		L.RegVar("setter", get_setter, set_setter);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeStartValue(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				DG.Tweening.Tweener o = obj.ChangeStartValue(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				DG.Tweening.Tweener o = obj.ChangeStartValue(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>.ChangeStartValue");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeEndValue(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				DG.Tweening.Tweener o = obj.ChangeEndValue(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<float>(L, 3))
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				DG.Tweening.Tweener o = obj.ChangeEndValue(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<bool>(L, 3))
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				bool arg1 = LuaDLL.lua_toboolean(L, 3);
				DG.Tweening.Tweener o = obj.ChangeEndValue(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 4)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
				bool arg2 = LuaDLL.luaL_checkboolean(L, 4);
				DG.Tweening.Tweener o = obj.ChangeEndValue(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>.ChangeEndValue");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeValues(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				DG.Tweening.Tweener o = obj.ChangeValues(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 4)
			{
				DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)ToLua.CheckObject<DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>>(L, 1);
				object arg0 = ToLua.ToVarObject(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
				DG.Tweening.Tweener o = obj.ChangeValues(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>.ChangeValues");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_startValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 ret = obj.startValue;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_endValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 ret = obj.endValue;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index endValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_changeValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 ret = obj.changeValue;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index changeValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_plugOptions(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Plugins.Options.QuaternionOptions ret = obj.plugOptions;
			ToLua.PushValue(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index plugOptions on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_getter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Core.DOGetter<UnityEngine.Quaternion> ret = obj.getter;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index getter on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_setter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Core.DOSetter<UnityEngine.Quaternion> ret = obj.setter;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index setter on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_startValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
			obj.startValue = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_endValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
			obj.endValue = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index endValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_changeValue(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
			obj.changeValue = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index changeValue on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_plugOptions(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Plugins.Options.QuaternionOptions arg0 = StackTraits<DG.Tweening.Plugins.Options.QuaternionOptions>.Check(L, 2);
			obj.plugOptions = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index plugOptions on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_getter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Core.DOGetter<UnityEngine.Quaternion> arg0 = (DG.Tweening.Core.DOGetter<UnityEngine.Quaternion>)ToLua.CheckDelegate<DG.Tweening.Core.DOGetter<UnityEngine.Quaternion>>(L, 2);
			obj.getter = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index getter on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_setter(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions> obj = (DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>)o;
			DG.Tweening.Core.DOSetter<UnityEngine.Quaternion> arg0 = (DG.Tweening.Core.DOSetter<UnityEngine.Quaternion>)ToLua.CheckDelegate<DG.Tweening.Core.DOSetter<UnityEngine.Quaternion>>(L, 2);
			obj.setter = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index setter on a nil value");
		}
	}
}

