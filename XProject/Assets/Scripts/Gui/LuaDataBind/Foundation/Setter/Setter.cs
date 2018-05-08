using UnityEngine;

namespace LuaDataBind
{
    public abstract class Setter : MonoBehaviour , IBinding
    {
        public string path;

        public string Path { get { return path; } }
        
        public LuaContext Context { get; set; }

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            LuaContext parentContext = this.GetComponentInParent<LuaContext>();
            parentContext.RegistBinder(this);
            Context = parentContext;
        }

        protected virtual void OnDisable()
        {

        }

        protected virtual void OnDestroy()
        {

        }


        public virtual void OnObjectChanged(object value)
        {
            
        }
    }
}