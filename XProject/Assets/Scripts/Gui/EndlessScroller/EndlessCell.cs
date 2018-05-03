using UnityEngine;
using System.Collections;

public abstract class EndlessCell : MonoBehaviour
{
    public abstract void UpdateTableItem();
    public abstract void OnClick();
    public abstract bool IsPointOverItemCell(Vector3 point);
}
