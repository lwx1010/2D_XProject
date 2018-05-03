
namespace LuaDataBind
{

    using UnityEngine;

    /// <summary>
    ///   Sets the enabled flag of a button depending on a data value.
    ///   <para>Input: Boolean</para>
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Button Enabled Setter")]
    public class ButtonEnabledSetter : ComponentSingleSetter<UIButton, bool>
    {
        
        protected override void OnValueChanged(bool newValue)
        {
            this.Target.isEnabled = newValue;
        }
        
    }
}