﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class Riverlake_ObjectPoolWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(Riverlake.ObjectPool), typeof(UnityEngine.MonoBehaviour));
		L.RegFunction("PushToPool", PushToPool);
		L.RegFunction("AsyncPushAvatarToPool", AsyncPushAvatarToPool);
		L.RegFunction("AsyncPushToPool", AsyncPushToPool);
		L.RegFunction("CreateStartupPools", CreateStartupPools);
		L.RegFunction("CreatePool", CreatePool);
		L.RegFunction("Spawn", Spawn);
		L.RegFunction("Recycle", Recycle);
		L.RegFunction("RecycleFromLua", RecycleFromLua);
		L.RegFunction("RecycleAll", RecycleAll);
		L.RegFunction("IsSpawned", IsSpawned);
		L.RegFunction("CountPooled", CountPooled);
		L.RegFunction("CountSpawned", CountSpawned);
		L.RegFunction("CountAllPooled", CountAllPooled);
		L.RegFunction("GetPooled", GetPooled);
		L.RegFunction("GetSpawned", GetSpawned);
		L.RegFunction("DestroyPooled", DestroyPooled);
		L.RegFunction("DestroyAll", DestroyAll);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegConstant("POOL_INIT_SIZE", 5);
		L.RegVar("pooledObjNames", get_pooledObjNames, set_pooledObjNames);
		L.RegVar("startupPoolMode", get_startupPoolMode, set_startupPoolMode);
		L.RegVar("startupPools", get_startupPools, set_startupPools);
		L.RegVar("instance", get_instance, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PushToPool(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3)
			{
				Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				UnityEngine.GameObject o = obj.PushToPool(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 4)
			{
				Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				UnityEngine.Transform arg2 = (UnityEngine.Transform)ToLua.CheckObject<UnityEngine.Transform>(L, 4);
				UnityEngine.GameObject o = obj.PushToPool(arg0, arg1, arg2);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 7)
			{
				Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				UnityEngine.Transform arg2 = (UnityEngine.Transform)ToLua.CheckObject<UnityEngine.Transform>(L, 4);
				float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
				float arg4 = (float)LuaDLL.luaL_checknumber(L, 6);
				float arg5 = (float)LuaDLL.luaL_checknumber(L, 7);
				UnityEngine.GameObject o = obj.PushToPool(arg0, arg1, arg2, arg3, arg4, arg5);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 10)
			{
				Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				UnityEngine.Transform arg2 = (UnityEngine.Transform)ToLua.CheckObject<UnityEngine.Transform>(L, 4);
				float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
				float arg4 = (float)LuaDLL.luaL_checknumber(L, 6);
				float arg5 = (float)LuaDLL.luaL_checknumber(L, 7);
				float arg6 = (float)LuaDLL.luaL_checknumber(L, 8);
				float arg7 = (float)LuaDLL.luaL_checknumber(L, 9);
				float arg8 = (float)LuaDLL.luaL_checknumber(L, 10);
				UnityEngine.GameObject o = obj.PushToPool(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.PushToPool");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AsyncPushAvatarToPool(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
			string arg0 = ToLua.CheckString(L, 2);
			System.Action<UnityEngine.GameObject,string> arg1 = (System.Action<UnityEngine.GameObject,string>)ToLua.CheckDelegate<System.Action<UnityEngine.GameObject,string>>(L, 3);
			System.Collections.IEnumerator o = obj.AsyncPushAvatarToPool(arg0, arg1);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AsyncPushToPool(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 8);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)ToLua.CheckObject(L, 1, typeof(Riverlake.ObjectPool));
			string arg0 = ToLua.CheckString(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			UnityEngine.Transform arg2 = (UnityEngine.Transform)ToLua.CheckObject<UnityEngine.Transform>(L, 4);
			float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
			float arg4 = (float)LuaDLL.luaL_checknumber(L, 6);
			float arg5 = (float)LuaDLL.luaL_checknumber(L, 7);
			System.Action<UnityEngine.GameObject,string> arg6 = (System.Action<UnityEngine.GameObject,string>)ToLua.CheckDelegate<System.Action<UnityEngine.GameObject,string>>(L, 8);
			obj.AsyncPushToPool(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateStartupPools(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			Riverlake.ObjectPool.CreateStartupPools();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreatePool(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<UnityEngine.GameObject, int>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				int arg1 = (int)LuaDLL.lua_tonumber(L, 2);
				Riverlake.ObjectPool.CreatePool(arg0, arg1);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Component, int>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				int arg1 = (int)LuaDLL.lua_tonumber(L, 2);
				Riverlake.ObjectPool.CreatePool(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.CreatePool");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Spawn(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Component, UnityEngine.Transform>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.GameObject, UnityEngine.Vector3>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.Vector3 arg1 = ToLua.ToVector3(L, 2);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.GameObject, UnityEngine.Transform>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0, arg1);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Component, UnityEngine.Vector3>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Vector3 arg1 = ToLua.ToVector3(L, 2);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.GameObject, UnityEngine.Vector3, UnityEngine.Quaternion>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.Vector3 arg1 = ToLua.ToVector3(L, 2);
				UnityEngine.Quaternion arg2 = ToLua.ToQuaternion(L, 3);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.GameObject, UnityEngine.Transform, UnityEngine.Vector3>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.Vector3 arg2 = ToLua.ToVector3(L, 3);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.Component, UnityEngine.Transform, UnityEngine.Vector3>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.Vector3 arg2 = ToLua.ToVector3(L, 3);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.Component, UnityEngine.Vector3, UnityEngine.Quaternion>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Vector3 arg1 = ToLua.ToVector3(L, 2);
				UnityEngine.Quaternion arg2 = ToLua.ToQuaternion(L, 3);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 4 && TypeChecker.CheckTypes<UnityEngine.Component, UnityEngine.Transform, UnityEngine.Vector3, UnityEngine.Quaternion>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.Vector3 arg2 = ToLua.ToVector3(L, 3);
				UnityEngine.Quaternion arg3 = ToLua.ToQuaternion(L, 4);
				UnityEngine.Component o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2, arg3);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 4 && TypeChecker.CheckTypes<UnityEngine.GameObject, UnityEngine.Transform, UnityEngine.Vector3, UnityEngine.Quaternion>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				UnityEngine.Transform arg1 = (UnityEngine.Transform)ToLua.ToObject(L, 2);
				UnityEngine.Vector3 arg2 = ToLua.ToVector3(L, 3);
				UnityEngine.Quaternion arg3 = ToLua.ToQuaternion(L, 4);
				UnityEngine.GameObject o = Riverlake.ObjectPool.Spawn(arg0, arg1, arg2, arg3);
				ToLua.PushSealed(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.Spawn");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Recycle(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.Recycle(arg0);
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.Recycle(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.Recycle");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RecycleFromLua(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			Riverlake.ObjectPool.RecycleFromLua(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RecycleAll(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				Riverlake.ObjectPool.RecycleAll();
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.RecycleAll(arg0);
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.RecycleAll(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.RecycleAll");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsSpawned(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			bool o = Riverlake.ObjectPool.IsSpawned(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CountPooled(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				int o = Riverlake.ObjectPool.CountPooled(arg0);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				int o = Riverlake.ObjectPool.CountPooled(arg0);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.CountPooled");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CountSpawned(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				int o = Riverlake.ObjectPool.CountSpawned(arg0);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				int o = Riverlake.ObjectPool.CountSpawned(arg0);
				LuaDLL.lua_pushinteger(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.CountSpawned");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CountAllPooled(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			int o = Riverlake.ObjectPool.CountAllPooled();
			LuaDLL.lua_pushinteger(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPooled(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			System.Collections.Generic.List<UnityEngine.GameObject> arg1 = (System.Collections.Generic.List<UnityEngine.GameObject>)ToLua.CheckObject(L, 2, typeof(System.Collections.Generic.List<UnityEngine.GameObject>));
			bool arg2 = LuaDLL.luaL_checkboolean(L, 3);
			System.Collections.Generic.List<UnityEngine.GameObject> o = Riverlake.ObjectPool.GetPooled(arg0, arg1, arg2);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetSpawned(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.CheckObject(L, 1, typeof(UnityEngine.GameObject));
			System.Collections.Generic.List<UnityEngine.GameObject> arg1 = (System.Collections.Generic.List<UnityEngine.GameObject>)ToLua.CheckObject(L, 2, typeof(System.Collections.Generic.List<UnityEngine.GameObject>));
			bool arg2 = LuaDLL.luaL_checkboolean(L, 3);
			System.Collections.Generic.List<UnityEngine.GameObject> o = Riverlake.ObjectPool.GetSpawned(arg0, arg1, arg2);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DestroyPooled(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.DestroyPooled(arg0);
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.DestroyPooled(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.DestroyPooled");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DestroyAll(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.Component>(L, 1))
			{
				UnityEngine.Component arg0 = (UnityEngine.Component)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.DestroyAll(arg0);
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<UnityEngine.GameObject>(L, 1))
			{
				UnityEngine.GameObject arg0 = (UnityEngine.GameObject)ToLua.ToObject(L, 1);
				Riverlake.ObjectPool.DestroyAll(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Riverlake.ObjectPool.DestroyAll");
			}
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
	static int get_pooledObjNames(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			System.Collections.Generic.Dictionary<string,UnityEngine.GameObject> ret = obj.pooledObjNames;
			ToLua.PushSealed(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index pooledObjNames on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_startupPoolMode(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			Riverlake.ObjectPool.StartupPoolMode ret = obj.startupPoolMode;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startupPoolMode on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_startupPools(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			Riverlake.ObjectPool.StartupPool[] ret = obj.startupPools;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startupPools on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_instance(IntPtr L)
	{
		try
		{
			ToLua.PushSealed(L, Riverlake.ObjectPool.instance);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_pooledObjNames(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			System.Collections.Generic.Dictionary<string,UnityEngine.GameObject> arg0 = (System.Collections.Generic.Dictionary<string,UnityEngine.GameObject>)ToLua.CheckObject(L, 2, typeof(System.Collections.Generic.Dictionary<string,UnityEngine.GameObject>));
			obj.pooledObjNames = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index pooledObjNames on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_startupPoolMode(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			Riverlake.ObjectPool.StartupPoolMode arg0 = (Riverlake.ObjectPool.StartupPoolMode)ToLua.CheckObject(L, 2, typeof(Riverlake.ObjectPool.StartupPoolMode));
			obj.startupPoolMode = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startupPoolMode on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_startupPools(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Riverlake.ObjectPool obj = (Riverlake.ObjectPool)o;
			Riverlake.ObjectPool.StartupPool[] arg0 = ToLua.CheckObjectArray<Riverlake.ObjectPool.StartupPool>(L, 2);
			obj.startupPools = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index startupPools on a nil value");
		}
	}
}

