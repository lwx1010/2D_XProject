using LuaInterface;
using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Base class for a setter which uses a collection or an integer to determine how many
    ///   items are shown beneath the game object of the target behaviour.
    /// </summary>
    /// <typeparam name="TBehaviour">Type of mono behaviour.</typeparam>
    public abstract class CellsSetter<TBehaviour> : ComponentSingleSetter<TBehaviour, LuaTable>
        where TBehaviour : MonoBehaviour
    {
        /// <summary>
        ///   Clears all created items.
        /// </summary>
        protected abstract void ClearItems();

        /// <summary>
        ///   Creates an item for the specified item context.
        /// </summary>
        /// <param name="itemContext">Item context for the item to create.</param>
        protected abstract void CreateItem(object itemContext);


        /// <summary>
        ///   Called when the items of the control changed.
        /// </summary>
        public virtual void OnChangedFinish()
        {
        }

        protected override void OnValueChanged(LuaTable itemModel)
        {
            this.ClearItems();

            LuaTable itemLuaModel = itemModel as LuaTable;
            LuaFunction func = itemLuaModel.GetLuaFunction("count");
            func.BeginPCall();
            func.Push(itemLuaModel);
            func.PCall();
            int count = (int)func.CheckNumber();
            func.EndPCall();

            LuaFunction getFunc = itemLuaModel.GetLuaFunction("get");
                
            for (int i = 0; i < count; i++)
            {
                getFunc.BeginPCall();
                getFunc.Push(itemLuaModel);
                getFunc.Push(i);
                getFunc.PCall();
                object value = getFunc.CheckVariant();
                CreateItem(value);
                getFunc.EndPCall();
            }

            this.OnChangedFinish();
            
        }

        /// <summary>
        ///   Removes the item with the specified item context.
        /// </summary>
        /// <param name="itemContext">Item context of the item to remove.</param>
        protected abstract void RemoveItem(object itemContext);


        public void OnCollectionCleared()
        {
            if (Target == null) return;
            this.ClearItems();
        }

        public void OnCollectionItemAdded(object item )
        {
            if (Target == null) return;
            // Create game object for item.
            this.CreateItem(item);
            
        }

        public void OnCollectionItemRemoved(object itemContext)
        {
            if (Target == null) return;
            // Remove item for this context.
            this.RemoveItem(itemContext);
        }
    }
}