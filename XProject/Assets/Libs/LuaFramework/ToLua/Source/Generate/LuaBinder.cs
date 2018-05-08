using System;
using LuaInterface;

public static class LuaBinder
{
    static IBinder binder;
	public static void Bind(LuaState L, IBinder binder)
	{
        //throw new LuaException("Please generate LuaBinder files first!");
        binder.Bind(L);
    }
}
