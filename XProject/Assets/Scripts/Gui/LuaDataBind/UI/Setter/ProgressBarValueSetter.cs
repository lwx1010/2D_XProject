using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the bar value of a UIProgressBar depending on the float data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Progress Bar Value Setter")]
    public class ProgressBarValueSetter : ComponentSingleSetter<UIProgressBar, float>
    {

        protected override void OnValueChanged(float newValue)
        {
              this.Target.value = newValue;
        }
    }
}