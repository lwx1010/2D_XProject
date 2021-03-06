﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class AL_Resources_ResourceManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginStaticLibs("ResourceManager");
		L.RegFunction("LoadBundleAsync", LoadBundleAsync);
		L.RegFunction("LoadCacheBundleAsync", LoadCacheBundleAsync);
		L.RegFunction("LoadResource", LoadResource);
		L.RegFunction("LoadTextAssets", LoadTextAssets);
		L.RegFunction("LoadSpriteAssets", LoadSpriteAssets);
		L.RegFunction("LoadGameObjectAssets", LoadGameObjectAssets);
		L.RegFunction("LoadPrefabBundle", LoadPrefabBundle);
		L.RegFunction("LoadBytesBundle", LoadBytesBundle);
		L.RegFunction("LoadSpriteFromAtlasBundle", LoadSpriteFromAtlasBundle);
		L.RegFunction("LoadAudioClipBundleAsync", LoadAudioClipBundleAsync);
		L.EndStaticLibs();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadBundleAsync(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				string arg0 = ToLua.CheckString(L, 1);
				AL.Resources.ALoadOperation o = AL.Resources.ResourceManager.LoadBundleAsync(arg0);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 2)
			{
				string arg0 = ToLua.CheckString(L, 1);
				string arg1 = ToLua.CheckString(L, 2);
				AL.Resources.ALoadOperation o = AL.Resources.ResourceManager.LoadBundleAsync(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: AL.Resources.ResourceManager.LoadBundleAsync");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadCacheBundleAsync(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				AL.PoolCache.CachePoolAsync arg0 = (AL.PoolCache.CachePoolAsync)ToLua.CheckObject<AL.PoolCache.CachePoolAsync>(L, 1);
				string arg1 = ToLua.CheckString(L, 2);
				AL.Resources.ALoadOperation o = AL.Resources.ResourceManager.LoadCacheBundleAsync(arg0, arg1);
				ToLua.PushObject(L, o);
				return 1;
			}
			else if (count == 3)
			{
				AL.PoolCache.CachePoolAsync arg0 = (AL.PoolCache.CachePoolAsync)ToLua.CheckObject<AL.PoolCache.CachePoolAsync>(L, 1);
				string arg1 = ToLua.CheckString(L, 2);
				string arg2 = ToLua.CheckString(L, 3);
				AL.Resources.ALoadOperation o = AL.Resources.ResourceManager.LoadCacheBundleAsync(arg0, arg1, arg2);
				ToLua.PushObject(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: AL.Resources.ResourceManager.LoadCacheBundleAsync");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadResource(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			string arg0 = ToLua.CheckString(L, 1);
			System.Type arg1 = ToLua.CheckMonoType(L, 2);
			UnityEngine.Object o = AL.Resources.ResourceManager.LoadResource(arg0, arg1);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadTextAssets(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			UnityEngine.TextAsset o = AL.Resources.ResourceManager.LoadTextAssets(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadSpriteAssets(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			UnityEngine.Sprite o = AL.Resources.ResourceManager.LoadSpriteAssets(arg0);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadGameObjectAssets(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			UnityEngine.GameObject o = AL.Resources.ResourceManager.LoadGameObjectAssets(arg0);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadPrefabBundle(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			UnityEngine.GameObject o = AL.Resources.ResourceManager.LoadPrefabBundle(arg0);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadBytesBundle(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			string arg0 = ToLua.CheckString(L, 1);
			byte[] o = AL.Resources.ResourceManager.LoadBytesBundle(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadSpriteFromAtlasBundle(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			string arg0 = ToLua.CheckString(L, 1);
			string arg1 = ToLua.CheckString(L, 2);
			UnityEngine.Sprite o = AL.Resources.ResourceManager.LoadSpriteFromAtlasBundle(arg0, arg1);
			ToLua.PushSealed(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAudioClipBundleAsync(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			string arg0 = ToLua.CheckString(L, 1);
			string arg1 = ToLua.CheckString(L, 2);
			RSG.IPromise<UnityEngine.AudioClip> o = AL.Resources.ResourceManager.LoadAudioClipBundleAsync(arg0, arg1);
			ToLua.PushObject(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

