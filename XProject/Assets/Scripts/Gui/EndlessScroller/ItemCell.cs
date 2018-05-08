using UnityEngine;
using System.Collections;
using Riverlake;

namespace LuaFramework
{
    /// <summary>
    /// Word Item.
    /// </summary>
    public class ItemCell : EndlessCell
    {
        public Transform mParentTrans;
        public static int index = 0;
        public int itemId;
        public bool isChange = false;    
        Transform mTrans;
        EndlessScroller table = null;

        private int width;
        private int height;

        // Memory Current position
        void Awake()
        {
            mTrans = transform;
            var child = mTrans.Find("bg");
            width = child.GetComponent<UIWidget>().width;
            height = child.GetComponent<UIWidget>().height;
            index++;
        }

        // Clear Outside Word Item
        public override void UpdateTableItem()
        {
            if (mParentTrans == null) return;
            if (table == null)
                table = mParentTrans.GetComponent<EndlessScroller>();
            Vector3 pos = mParentTrans.localPosition + mTrans.localPosition;
            if (pos.y > table.cellHeight * 4f || pos.y < table.cellHeight * -6f || isChange)
            {
                gameObject.Recycle();
                LuaFramework.Util.CallMethod("ITEMLOGIC", "OnBagItemRemoved", itemId);
            }

        }

        void Update()
        {
            UpdateTableItem();
        }

        public override bool IsPointOverItemCell(Vector3 point)
        {
            var cellPos = UICamera.currentCamera.WorldToScreenPoint(mTrans.position);

            return (point.x >= cellPos.x - width * 0.4f && point.x <= cellPos.x + width * 0.4f) && (point.y >= cellPos.y - height * 0.4f && point.y <= cellPos.y + height * 0.4f);
        }

        public override void OnClick()
        {
            LuaFramework.Util.CallMethod("ITEMLOGIC", "OnClickItemInfo", itemId); 
        }
    }
}
