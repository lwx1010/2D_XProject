using UnityEngine;

namespace LuaDataBind
{
    public abstract class ComponentSingleSetter<TComponent , TData> : SingleSetter<TData>
        where TComponent : Component
    {
        public TComponent Target;
        
        protected override void Awake()
        {
            base.Awake();
            if(Target == null)
                Target = this.GetComponent<TComponent>();
        }

        public override void OnObjectChanged(object value)
        {
            if (Target == null) return;

            base.OnObjectChanged(value);
        }



        protected virtual void Reset()
        {
            Target = this.GetComponent<TComponent>();
        }
    }
}