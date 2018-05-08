using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Sets the local position of a game object depending on a Vector3 data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[DB] Local Position Setter")]
    public class LocalPositionSetter : GameObjectSingleSetter<Vector3>
    {
        protected override void OnValueChanged(Vector3 newValue)
        {
            this.cacheTrans.localPosition = newValue;
        }
    }
}