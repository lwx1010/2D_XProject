using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Base class for a setter for a game object.
    /// </summary>
    /// <typeparam name="T">Type of data to set.</typeparam>
    public abstract class GameObjectSingleSetter<T> : SingleSetter<T>
    {

        public GameObject Target;

        protected Transform cacheTrans;

        protected override void Awake()
        {
            base.Awake();
            if (Target == null)
                Target = this.gameObject;
            cacheTrans = Target.transform;
        }

        public override void OnObjectChanged(object value)
        {
            if (Target == null) return;

            base.OnObjectChanged(value);
        }

        protected virtual void Reset()
        {
            Target = this.gameObject;
            cacheTrans = Target.transform;
        }
    }
}