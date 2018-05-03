using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the state of a UIToggle depending on the bool data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Toggle State Setter")]
    public class ToggleStateSetter : ComponentSingleSetter<UIToggle , bool>
    {
        protected override void OnValueChanged(bool value)
        {
            this.Target.value = value;
        }
    }
}