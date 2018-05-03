using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Gets the value of a UISlider element.
    /// </summary>
    [AddComponentMenu("Data Bind/Getters/[NGUI] Slider Value Getter")]
    public class SliderValueGetter : ComponentSingleGetter<UISlider , float>
    {
        protected override void addListener(UISlider target)
        {
            EventDelegate.Add(target.onChange, this.onTargetValueChanged);
        }

        protected override void removeListener(UISlider target)
        {
            EventDelegate.Remove(target.onChange, this.onTargetValueChanged);
        }

        protected override float getValue(UISlider target)
        {
            return target.value;
        }
    }
}