using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Sets the value of a popup list depending on the string data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Popup List Value Setter")]
    public class PopupListValueSetter : ComponentSingleSetter<UIPopupList, string>
    {
        protected override void OnValueChanged(string newValue)
        {
            // Get text value for data value.
            var dataValueIndex = this.Target.itemData.IndexOf(newValue);
            string textValue = newValue;
            if (dataValueIndex >= 0)
            {
                textValue = this.Target.items[dataValueIndex];
            }

            // Checking if value changed as the NGUI popup list doesn't do it internally.
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (textValue != this.Target.value)
            {
                this.Target.value = textValue;
            }
        }
        
    }
}