using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Base class for a getter for a component which modifies a single data value.
    /// </summary>
    /// <typeparam name="TComponent">Type of component to get value from.</typeparam>
    /// <typeparam name="TData">Type of data which is modified.</typeparam>
    public abstract class ComponentSingleGetter<TComponent , TData> : MonoBehaviour , IBinding
        where TComponent : Component
    {

        public string path;

        public TComponent Target;

        public LuaContext Context { get; set; }
        
        public string Path { get { return path; } }

        public void OnObjectChanged(object value)
        {
            if (Context == null) return;

            Context.SetValue(Path , value);
        }

        #region Monobehviour 生命周期

        protected virtual void Awake()
        {
            Context = this.GetComponentInParent<LuaContext>();
            if (Target == null)
                Target = this.GetComponent<TComponent>();
        }

        protected virtual void OnEnable()
        {
            addListener(Target);
        }
        protected virtual void Start()
        {
            
        }

        protected virtual void OnDisable()
        {
            removeListener(Target);
        }

        protected virtual void OnDestroy()
        {

        }
        protected virtual void Reset()
        {
            if (Target == null)
                Target = this.GetComponent<TComponent>();
        }



        #endregion

        /// <summary>
        ///   Register listener at target to be informed if its value changed.
        ///   The target is already checked for null reference.
        /// </summary>
        /// <param name="target">Target to add listener to.</param>
        protected abstract void addListener(TComponent target);

        /// <summary>
        ///   Remove listener from target which was previously added in AddListener.
        ///   The target is already checked for null reference.
        /// </summary>
        /// <param name="target">Target to remove listener from.</param>
        protected abstract void removeListener(TComponent target);


        /// <summary>
        ///   Current data value.
        /// </summary>
        public object Value
        {
            get
            {
                return this.Target != null ? this.getValue(this.Target) : default(TData);
            }
        }

        /// <summary>
        ///   Derived classes should return the current value to set if this method is called.
        ///   The target is already checked for null reference.
        /// </summary>
        /// <param name="target">Target to get value from.</param>
        /// <returns>Current value to set.</returns>
        protected abstract TData getValue(TComponent target);

        /// <summary>
        ///   Has to be called by derived classes when the value may have changed.
        /// </summary>
        protected void onTargetValueChanged()
        {
            OnObjectChanged( Value);
        }
    }
}