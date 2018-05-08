using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Gets the value of a UIInput element.
    /// </summary>
    [AddComponentMenu("Data Bind/Getters/[NGUI] Input Value Getter")]
    public class InputValueGetter : ComponentSingleGetter<UIInput , string>
    {
        protected override void addListener(UIInput target)
        {
            EventDelegate.Add(target.onChange, this.onTargetValueChanged);
        }

        protected override void removeListener(UIInput target)
        {
            EventDelegate.Remove(target.onChange, this.onTargetValueChanged);
        }

        protected override string getValue(UIInput target)
        {
            return Target.value; 
        }
    }
}