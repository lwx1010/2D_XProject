using System.Collections.Generic;
using System.Linq;
using LuaInterface;
using UnityEngine;

namespace LuaDataBind
{

    /// <summary>
    ///   Base class which adds game objects for each item of an ItemsSetter.
    /// </summary>
    /// <typeparam name="TBehaviour"></typeparam>
    public abstract class GameObjectCellsSetter<TBehaviour> : CellsSetter<TBehaviour>
        where TBehaviour : MonoBehaviour
    {

        /// <summary>
        ///   Items.
        /// </summary>
        private readonly List<Item> items = new List<Item>();

        /// <summary>
        ///   Prefab to create the items from.
        /// </summary>
        public GameObject Prefab;
        
        protected override void ClearItems()
        {
            foreach (var item in this.items)
            {
                Destroy(item.GameObject);
            }
            this.items.Clear();
        }

        protected override void CreateItem(object itemModel)
        {
            var item = this.Target.gameObject.AddChild(this.Prefab);
            LuaContext itemContext = item.GetComponent<LuaContext>();
            if (itemModel is LuaTable)
            {
                LuaTable itemLuaModel = itemModel as LuaTable;
                itemContext.LuaModel = itemLuaModel;

                LuaFunction func = itemLuaModel.GetLuaFunction("onObjectChanged");
                func.BeginPCall();
                func.Push(itemLuaModel);
                func.Push(item);
                func.PCall();
                func.EndPCall();
            }
            this.items.Add(new Item { GameObject = item, Context = itemModel });
        }

        protected override void RemoveItem(object itemContext)
        {
            // Get item.
            var item = this.items.FirstOrDefault(existingItem => existingItem.Context == itemContext);
            if (item == null)
            {
                Debug.LogWarning(string.Format("No item found for collection item {0}", itemContext), this);
                return;
            }

            // Destroy item.
            Destroy(item.GameObject);
            this.items.Remove(item);
        }

        private class Item
        {

            public object Context { get; set; }

            public GameObject GameObject { get; set; }

        }
    }
}