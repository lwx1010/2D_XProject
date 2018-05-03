
using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the fill amount of a UISprite depending on the float data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Sprite Fill Amount Setter")]
    public class SpriteFillAmountSetter : ComponentSingleSetter<UISprite, float>
    {

        protected override void OnValueChanged(float newValue)
        {
            if (this.Target != null)
            {
                this.Target.fillAmount = newValue;
            }
        }
    }
}