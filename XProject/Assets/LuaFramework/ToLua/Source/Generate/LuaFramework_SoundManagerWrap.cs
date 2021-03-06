﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class LuaFramework_SoundManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(LuaFramework.SoundManager), typeof(Manager));
		L.RegFunction("AddAudioCompatible", AddAudioCompatible);
		L.RegFunction("CanPlayBackSound", CanPlayBackSound);
		L.RegFunction("PlayBGM", PlayBGM);
		L.RegFunction("CanPlaySoundEffect", CanPlaySoundEffect);
		L.RegFunction("PlaySkillSound", PlaySkillSound);
		L.RegFunction("PlaySound", PlaySound);
		L.RegFunction("PlayEffectSound", PlayEffectSound);
		L.RegFunction("PlayTalkSound", PlayTalkSound);
		L.RegFunction("PlayButtonDownSound", PlayButtonDownSound);
		L.RegFunction("PlayUISound", PlayUISound);
		L.RegFunction("ChangeVolume", ChangeVolume);
		L.RegFunction("CloseSound", CloseSound);
		L.RegFunction("StopBGM", StopBGM);
		L.RegFunction("PauseMusic", PauseMusic);
		L.RegFunction("ReplayMusic", ReplayMusic);
		L.RegFunction("StopEffectSound", StopEffectSound);
		L.RegFunction("StopSound", StopSound);
		L.RegFunction("IsPlaying", IsPlaying);
		L.RegFunction("IsActiveLayer", IsActiveLayer);
		L.RegFunction("SetActiveLayer", SetActiveLayer);
		L.RegFunction("Clear", Clear);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("SoundLayer", get_SoundLayer, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddAudioCompatible(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			string[] arg2 = ToLua.CheckStringArray(L, 4);
			obj.AddAudioCompatible(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CanPlayBackSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			bool o = obj.CanPlayBackSound();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayBGM(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			obj.PlayBGM(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CanPlaySoundEffect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			LuaFramework.SoundManager.SoundType arg0 = (LuaFramework.SoundManager.SoundType)ToLua.CheckObject(L, 2, typeof(LuaFramework.SoundManager.SoundType));
			bool o = obj.CanPlaySoundEffect(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlaySkillSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 10);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			UnityEngine.Vector3 arg3 = ToLua.ToVector3(L, 5);
			int arg4 = (int)LuaDLL.luaL_checknumber(L, 6);
			bool arg5 = LuaDLL.luaL_checkboolean(L, 7);
			float arg6 = (float)LuaDLL.luaL_checknumber(L, 8);
			float arg7 = (float)LuaDLL.luaL_checknumber(L, 9);
			string arg8 = ToLua.CheckString(L, 10);
			obj.PlaySkillSound(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlaySound(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				obj.PlaySound(arg0);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<float>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				obj.PlaySound(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<LuaFramework.SoundManager.SoundType>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				LuaFramework.SoundManager.SoundType arg1 = (LuaFramework.SoundManager.SoundType)ToLua.ToObject(L, 3);
				obj.PlaySound(arg0, arg1);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes<LuaFramework.SoundManager.SoundType, float>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				LuaFramework.SoundManager.SoundType arg1 = (LuaFramework.SoundManager.SoundType)ToLua.ToObject(L, 3);
				float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
				obj.PlaySound(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes<float, bool>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				bool arg2 = LuaDLL.lua_toboolean(L, 4);
				obj.PlaySound(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes<float, float>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
				obj.PlaySound(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 5 && TypeChecker.CheckTypes<float, float, bool>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				float arg1 = (float)LuaDLL.lua_tonumber(L, 3);
				float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
				bool arg3 = LuaDLL.lua_toboolean(L, 5);
				obj.PlaySound(arg0, arg1, arg2, arg3);
				return 0;
			}
			else if (count == 5 && TypeChecker.CheckTypes<LuaFramework.SoundManager.SoundType, float, float>(L, 3))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				LuaFramework.SoundManager.SoundType arg1 = (LuaFramework.SoundManager.SoundType)ToLua.ToObject(L, 3);
				float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
				float arg3 = (float)LuaDLL.lua_tonumber(L, 5);
				obj.PlaySound(arg0, arg1, arg2, arg3);
				return 0;
			}
			else if (count == 6)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				LuaFramework.SoundManager.SoundType arg1 = (LuaFramework.SoundManager.SoundType)ToLua.CheckObject(L, 3, typeof(LuaFramework.SoundManager.SoundType));
				float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
				float arg3 = (float)LuaDLL.luaL_checknumber(L, 5);
				bool arg4 = LuaDLL.luaL_checkboolean(L, 6);
				obj.PlaySound(arg0, arg1, arg2, arg3, arg4);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.SoundManager.PlaySound");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayEffectSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 4);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			UnityEngine.Vector3 arg1 = ToLua.ToVector3(L, 3);
			bool arg2 = LuaDLL.luaL_checkboolean(L, 4);
			obj.PlayEffectSound(arg0, arg1, arg2);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayTalkSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 5);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			float arg1 = (float)LuaDLL.luaL_checknumber(L, 3);
			float arg2 = (float)LuaDLL.luaL_checknumber(L, 4);
			UnityEngine.Vector3 arg3 = ToLua.ToVector3(L, 5);
			obj.PlayTalkSound(arg0, arg1, arg2, arg3);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayButtonDownSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			obj.PlayButtonDownSound();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayUISound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			string arg0 = ToLua.CheckString(L, 2);
			obj.PlayUISound(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeVolume(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.ChangeVolume(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CloseSound(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			bool arg0 = LuaDLL.luaL_checkboolean(L, 2);
			obj.CloseSound(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StopBGM(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			obj.StopBGM();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PauseMusic(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			obj.PauseMusic();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ReplayMusic(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			obj.ReplayMusic();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StopEffectSound(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				obj.StopEffectSound();
				return 0;
			}
			else if (count == 3)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				obj.StopEffectSound(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.SoundManager.StopEffectSound");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StopSound(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<string>(L, 2))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.ToString(L, 2);
				obj.StopSound(arg0);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<LuaFramework.SoundManager.SoundType>(L, 2))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				LuaFramework.SoundManager.SoundType arg0 = (LuaFramework.SoundManager.SoundType)ToLua.ToObject(L, 2);
				obj.StopSound(arg0);
				return 0;
			}
			else if (count == 3)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				obj.StopSound(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.SoundManager.StopSound");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsPlaying(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				bool o = obj.IsPlaying(arg0, arg1);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else if (count == 4)
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				string arg0 = ToLua.CheckString(L, 2);
				int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
				string arg2 = ToLua.CheckString(L, 4);
				bool o = obj.IsPlaying(arg0, arg1, arg2);
				LuaDLL.lua_pushboolean(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.SoundManager.IsPlaying");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsActiveLayer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			LuaFramework.SoundManager.SoundType arg0 = (LuaFramework.SoundManager.SoundType)ToLua.CheckObject(L, 2, typeof(LuaFramework.SoundManager.SoundType));
			bool o = obj.IsActiveLayer(arg0);
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetActiveLayer(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<int, bool>(L, 2))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				bool arg1 = LuaDLL.lua_toboolean(L, 3);
				obj.SetActiveLayer(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<LuaFramework.SoundManager.SoundType, bool>(L, 2))
			{
				LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
				LuaFramework.SoundManager.SoundType arg0 = (LuaFramework.SoundManager.SoundType)ToLua.ToObject(L, 2);
				bool arg1 = LuaDLL.lua_toboolean(L, 3);
				obj.SetActiveLayer(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: LuaFramework.SoundManager.SetActiveLayer");
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
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)ToLua.CheckObject(L, 1, typeof(LuaFramework.SoundManager));
			obj.Clear();
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
	static int get_SoundLayer(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			LuaFramework.SoundManager obj = (LuaFramework.SoundManager)o;
			int ret = obj.SoundLayer;
			LuaDLL.lua_pushinteger(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index SoundLayer on a nil value");
		}
	}
}

