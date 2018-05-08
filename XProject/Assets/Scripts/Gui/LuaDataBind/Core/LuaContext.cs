using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace LuaDataBind
{
    public class LuaContext : MonoBehaviour
    {
         
        private List<IBinding> binders = new List<IBinding>();
        /// <summary>
        /// Lua 数据Model
        /// </summary>
        public LuaTable LuaModel { get; set; }
        
        public IBinding[] Binders
        {
            get
            {
                if (binders.Count == 0)
                {
                    Setter[] bindingSetterArr = this.GetComponentsInChildren<Setter>();
                    binders.AddRange(bindingSetterArr);
                }
                return binders.ToArray();
            }
        }

        private void Awake()
        {

        }


        
        private void OnDestroy()
        {

        }

        private void OnDisable()
        {

        }

        public void RegistBinder(IBinding binding)
        {
            if (binders.Contains(binding)) return;

            binders.Add(binding);
            
            object defalutValue = GetValue(binding.Path);
            if(defalutValue != null)
                binding.OnObjectChanged(defalutValue);
        }


        public object GetValue(string key)
        {
            if (LuaModel == null || LuaModel[key] == null) return null;

            return LuaModel[key];
        }

        public void SetValue(string key, object newValue)
        {
            if (LuaModel == null ) return ;
            LuaTable metaTab = LuaModel.GetMetaTable();
//            Debug.Log("key:" + key + ",Val:" + LuaModel[key] + ",newVal:" + newValue + ",meta:" + metaTab[key]);         
//            metaTab[key] = newValue;
            LuaModel[key] = newValue;
//            Debug.Log("-----------key:" + key + ",Val:" + LuaModel[key] + ",newVal:" + newValue + ",meta:" + metaTab[key]);

        }
    }
}