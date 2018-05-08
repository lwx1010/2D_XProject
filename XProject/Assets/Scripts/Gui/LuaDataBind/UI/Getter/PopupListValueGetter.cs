using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Gets the text value of a UIPopupList element.
    /// </summary>
    [AddComponentMenu("Data Bind/Getters/[NGUI] Popup List Value Getter")]
    public class PopupListValueGetter : ComponentSingleGetter<UIPopupList, string>
    {
        #region Methods

        protected override void addListener(UIPopupList target)
        {
            EventDelegate.Add(target.onChange, this.onTargetValueChanged);
        }

        protected override string getValue(UIPopupList target)
        {
            return target.value;
        }

        protected override void removeListener(UIPopupList target)
        {
            EventDelegate.Remove(target.onChange, this.onTargetValueChanged);
        }

        #endregion
    }
}