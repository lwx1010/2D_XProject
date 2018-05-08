using System.Collections;
using UnityEngine;

namespace LuaDataBind
{
    /// <summary>
    ///   Set the items of a UIGrid depending on the items of the collection data value.
    /// </summary>
    [AddComponentMenu("Data Bind/Setters/[NGUI] Grid Items Setter")]
    public sealed class GridCellsSetter : GameObjectCellsSetter<UIGrid>
    {

        public override void OnChangedFinish()
        {
            // Reposition grid.
            this.StartCoroutine(this.RepositionGrid());
        }

        private IEnumerator RepositionGrid()
        {
            // Reposition now.
            this.Target.Reposition();

            // Wait one frame and reposition again, items may not be created yet.
            yield return Yielders.EndOfFrame;

            this.Target.Reposition();
        }
    }
}