using JetBrains.Annotations;

namespace LuaDataBind
{
    public interface IBinding
    {
        
        string Path { get; }

        LuaContext Context { get; set; }

        void OnObjectChanged(object value);
    }
}