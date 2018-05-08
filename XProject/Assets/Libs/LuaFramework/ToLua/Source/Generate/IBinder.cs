using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaInterface
{
    public interface IBinder
    {
        void Bind(LuaState L);
    }
}
