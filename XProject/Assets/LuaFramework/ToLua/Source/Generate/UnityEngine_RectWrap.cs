﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class UnityEngine_RectWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(UnityEngine.Rect), null);
		L.RegFunction("MinMaxRect", MinMaxRect);
		L.RegFunction("Set", Set);
		L.RegFunction("Contains", Contains);
		L.RegFunction("Overlaps", Overlaps);
		L.RegFunction("NormalizedToPoint", NormalizedToPoint);
		L.RegFunction("PointToNormalized", PointToNormalized);
		L.RegFunction("GetHashCode", GetHashCode);
		L.RegFunction("Equals", Equals);
		L.RegFunction("ToString", ToString);
		L.RegFunction("New", _CreateUnityEngine_Rect);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("zero", get_zero, null);
		L.RegVar("x", get_x, set_x);
		L.RegVar("y", get_y, set_y);
		L.RegVar("position", get_position, set_position);
		L.RegVar("center", get_center, set_center);
		L.RegVar("min", get_min, set_min);
		L.RegVar("max", get_max, set_max);
		L.RegVar("width", get_width, set_width);
		L.RegVar("height", get_height, set_height);
		L.RegVar("size", get_size, set_size);
		L.RegVar("xMin", get_xMin, set_xMin);
		L.RegVar("yMin", get_yMin, set_yMin);
		L.RegVar("xMax", get_xMax, set_xMax);
		L.RegVar("yMax", get_yMax, set_yMax);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateUnityEngine_Rect(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.Check(L, 1);
				UnityEngine.Rect obj = new UnityEngine.Rect(arg0);
				ToLua.PushValue(L, obj);
				return 1;
			}
			else if (count == 2)
			{
				UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 1);
				UnityEngine.Vector2 arg1 = ToLua.ToVector2(L, 2);
				UnityEngine.Rect obj = new UnityEngine.Rect(arg0, arg1);
				ToLua.PushValue(L, obj);
				return 1;
			}
			else if (count == 4)
			{
				float arg0 = (float)LuaDLL.luaL_checknumber(L, 1);
				float arg1 = (float)LuaDLL.luaL_checknumber(L, 2);
				float arg2 = (float)LuaDLL.luaL_checknumber(L, 3);
				float arg3 = (float)LuaDLL.luaL_checknumber(L, 4);
				UnityEngine.Rect obj = new UnityEngine.Rect(arg0, arg1, arg2, arg3);
				ToLua.PushValue(L, obj);
				return 1;
			}
			else if (count == 0)
			{
				UnityEngine.Rect obj = new UnityEngine.Rect();
				ToLua.PushValue(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: UnityEngine.Rect.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int MinMaxRect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 1);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 2);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg3 = (float)LuaDLL.luaL_checknumber(L, 4);
			UnityEngine.Rect o = UnityEngine.Rect.MinMaxRect(arg0, arg1, arg2, arg3);
			ToLua.PushValue(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Set(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 5);
			UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
			obj.Set(arg0, arg1, arg2, arg3);
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Contains(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Vector3>(L, 2))
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
				bool o = obj.Contains(arg0);
				LuaDLL.lua_pushboolean(L, o);
				ToLua.SetBack(L, 1, obj);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Vector2>(L, 2))
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
				bool o = obj.Contains(arg0);
				LuaDLL.lua_pushboolean(L, o);
				ToLua.SetBack(L, 1, obj);
				return 1;
			}
			else if (count == 3)
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				UnityEngine.Vector3 arg0 = ToLua.ToVector3(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				bool o = obj.Contains(arg0, arg1);
				LuaDLL.lua_pushboolean(L, o);
				ToLua.SetBack(L, 1, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UnityEngine.Rect.Contains");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Overlaps(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.Check(L, 2);
				bool o = obj.Overlaps(arg0);
				LuaDLL.lua_pushboolean(L, o);
				ToLua.SetBack(L, 1, obj);
				return 1;
			}
			else if (count == 3)
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.Check(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				bool o = obj.Overlaps(arg0, arg1);
				LuaDLL.lua_pushboolean(L, o);
				ToLua.SetBack(L, 1, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UnityEngine.Rect.Overlaps");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int NormalizedToPoint(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.Check(L, 1);
			UnityEngine.Vector2 arg1 = ToLua.ToVector2(L, 2);
			UnityEngine.Vector2 o = UnityEngine.Rect.NormalizedToPoint(arg0, arg1);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PointToNormalized(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.Check(L, 1);
			UnityEngine.Vector2 arg1 = ToLua.ToVector2(L, 2);
			UnityEngine.Vector2 o = UnityEngine.Rect.PointToNormalized(arg0, arg1);
			ToLua.Push(L, o);
			return 1;
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
			UnityEngine.Rect arg0 = StackTraits<UnityEngine.Rect>.To(L, 1);
			UnityEngine.Rect arg1 = StackTraits<UnityEngine.Rect>.To(L, 2);
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
	static int GetHashCode(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
			int o = obj.GetHashCode();
			LuaDLL.lua_pushinteger(L, o);
			ToLua.SetBack(L, 1, obj);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Equals(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
			object arg0 = ToLua.ToVarObject(L, 2);
			bool o = obj.Equals(arg0);
			LuaDLL.lua_pushboolean(L, o);
			ToLua.SetBack(L, 1, obj);
			return 1;
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
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				string o = obj.ToString();
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else if (count == 2)
			{
				UnityEngine.Rect obj = (UnityEngine.Rect)ToLua.CheckObject(L, 1, typeof(UnityEngine.Rect));
				string arg0 = ToLua.CheckString(L, 2);
				string o = obj.ToString(arg0);
				LuaDLL.lua_pushstring(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: UnityEngine.Rect.ToString");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_zero(IntPtr L)
	{
		try
		{
			ToLua.PushValue(L, UnityEngine.Rect.zero);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_x(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.x;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index x on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_y(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.y;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index y on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_position(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 ret = obj.position;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index position on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_center(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 ret = obj.center;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index center on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_min(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 ret = obj.min;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index min on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_max(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 ret = obj.max;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index max on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_width(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.width;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index width on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_height(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.height;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index height on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_size(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 ret = obj.size;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index size on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_xMin(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.xMin;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index xMin on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_yMin(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.yMin;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index yMin on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_xMax(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.xMax;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index xMax on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_yMax(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float ret = obj.yMax;
			LuaDLL.lua_pushnumber(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index yMax on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_x(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.x = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index x on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_y(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.y = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index y on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_position(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
			obj.position = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index position on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_center(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
			obj.center = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index center on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_min(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
			obj.min = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index min on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_max(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
			obj.max = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index max on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_width(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.width = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index width on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_height(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.height = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index height on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_size(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			UnityEngine.Vector2 arg0 = ToLua.ToVector2(L, 2);
			obj.size = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index size on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_xMin(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.xMin = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index xMin on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_yMin(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.yMin = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index yMin on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_xMax(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.xMax = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index xMax on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_yMax(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			UnityEngine.Rect obj = (UnityEngine.Rect)o;
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.yMax = arg0;
			ToLua.SetBack(L, 1, obj);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index yMax on a nil value");
		}
	}
}

