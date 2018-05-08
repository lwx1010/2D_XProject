using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the value of a UIWidget depending on the color data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Widget Color Setter")]
    public class WidgetColorSetter : ComponentSingleSetter<UIWidget , Color>
    {
        protected override void OnValueChanged(Color value)
        {
            this.Target.color = value;
        }
    }
}