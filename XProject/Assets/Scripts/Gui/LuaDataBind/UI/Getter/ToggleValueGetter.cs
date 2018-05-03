using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Gets the value of a UITogge element.
    /// </summary>
    [AddComponentMenu("Data Bind/Getters/[NGUI] Toggle Value Getter")]
    public class ToggleValueGetter : ComponentSingleGetter<UIToggle , bool>
    {
        protected override void addListener(UIToggle target)
        {
            EventDelegate.Add(target.onChange, this.onTargetValueChanged);
        }

        protected override void removeListener(UIToggle target)
        {
            EventDelegate.Remove(target.onChange, this.onTargetValueChanged);
        }

        protected override bool getValue(UIToggle target)
        {
            return target.value;
        }
    }
}