using UnityEngine;

namespace LuaDataBind
{
    
    /// <summary>
    ///   Set the sprite name of a UISprite depending on the string data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Sprite Name Setter")]
    public class SpriteNameSetter : ComponentSingleSetter<UISprite, string>
    {

        protected override void OnValueChanged(string newValue)
        {
            this.Target.spriteName = newValue;
        }
    }
}