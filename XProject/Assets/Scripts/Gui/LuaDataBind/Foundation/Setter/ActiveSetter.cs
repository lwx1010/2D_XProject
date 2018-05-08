using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Setter which activates/deactivates a game object depending on the boolean data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[DB] Active Setter")]
    public class ActiveSetter : GameObjectSingleSetter<bool>
    {
        protected override void OnValueChanged(bool value)
        {
            this.Target.SetActive(value);
        }
    }
}