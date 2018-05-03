using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Sets the items of a UIPopupList depending on a string data list.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Popup List Items Setter")]
    public class PopupListCellsSetter : CellsSetter<UIPopupList>
    {

        protected override void ClearItems()
        {
            this.Target.Clear();
        }

        protected override void CreateItem(object itemContext)
        {
            var itemValue = this.CreateItemValue(itemContext);
            this.Target.AddItem(itemValue, itemContext);
        }

        protected override void RemoveItem(object itemContext)
        {
            var itemValue = this.CreateItemValue(itemContext);
            this.Target.items.Remove(itemValue);
        }

        private string CreateItemValue(object itemContext)
        {
            return itemContext != null ? itemContext.ToString() : string.Empty;
        }
        
    }
}