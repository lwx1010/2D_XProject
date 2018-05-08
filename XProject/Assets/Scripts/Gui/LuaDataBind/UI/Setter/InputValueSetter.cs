using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the value of a UIInput depending on the string data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Input Value Setter")]
    public class InputValueSetter : ComponentSingleSetter<UIInput, string>
    {
        protected override void OnValueChanged(string newValue)
        {
                this.Target.value = newValue;
        }
        
    }
}