using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Sets the value of a slider depending on a data value.
    ///   <para>Input: Number</para>
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Slider Value Setter")]
    public class SliderValueSetter : ComponentSingleSetter<UISlider, float>
    {
        protected override void OnValueChanged(float newValue)
        {
            this.Target.value = newValue;
        }
        
    }
}