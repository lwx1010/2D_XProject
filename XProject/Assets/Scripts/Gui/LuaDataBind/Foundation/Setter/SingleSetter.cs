using System;

namespace LuaDataBind
{
    public abstract class SingleSetter<T> : Setter
    {
        public override void OnObjectChanged(object value)
        {
            object val = Convert.ChangeType(value, typeof (T));
            T newVal = val is T ? (T)val : default(T);

            this.OnValueChanged(newVal);
        }

        protected abstract void OnValueChanged(T newValue);
    }
}