
using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the text of a UILabel depending on the string data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Label Text Setter")]
    public class LabelTextSetter : ComponentSingleSetter<UILabel, string>
    {

        protected override void OnValueChanged(string newValue)
        {
           this.Target.text = newValue;
        }
    }
}