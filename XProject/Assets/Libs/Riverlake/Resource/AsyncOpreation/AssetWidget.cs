using LuaFramework;
using System;

namespace AL.Resources
{
    public class AssetWidget
    {
        public string Name { get; private set; }
        public int weight { get; private set; }
        public Action<ALoadOperation> callback { get; private set; }

        public AssetWidget(string name, int weight, Action<ALoadOperation> callback)
        {
            this.Name = name;
            this.weight = weight;
            this.callback = callback;
        }
    }
}
