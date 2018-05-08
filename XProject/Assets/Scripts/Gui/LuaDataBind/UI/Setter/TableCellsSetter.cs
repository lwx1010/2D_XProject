using System.Collections;
using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the items of a UITable depending on the items of the collection data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Table Items Setter")]
    public sealed class TableCellsSetter : GameObjectCellsSetter<UITable>
    {

        public override void OnChangedFinish()
        {
            // Reposition table.
            this.StartCoroutine(this.Reposition());
        }

        private IEnumerator Reposition()
        {
            // Reposition now.
            this.Target.Reposition();

            // Wait one frame and reposition again, items may not be created yet.
            yield return Yielders.EndOfFrame;

            this.Target.Reposition();
        }
    }
}