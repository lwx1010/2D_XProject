using UnityEngine;

public class MultiRowWrapContent : UIWrapContent
{
    public int RowNum = 6;

    protected override void ResetChildPositions()
    {
        for (int i = 0, imax = mChildren.Count; i < imax; ++i)
        {
            Transform t = mChildren[i];
            t.localPosition = mHorizontal ? new Vector3(i * itemSize, 0f, 0f) : new Vector3((i % RowNum) * itemSize, -(i / RowNum) * itemSize, 0f);
            UpdateItem(t, i);
        }
    }

    protected override void UpdateItem(Transform item, int index)
    {
        if (onInitializeItem != null)
        {
            int yIndex = Mathf.Abs(Mathf.RoundToInt(item.localPosition.y / itemSize));
            int xIndex = Mathf.Abs(Mathf.RoundToInt(item.localPosition.x / itemSize));
            int realIndex = RowNum * yIndex + xIndex;
            onInitializeItem(item.gameObject, index, realIndex);
        }
    }
    //重写这个函数主要是去掉排序  
    //if (mHorizontal) mChildren.Sort(UIGrid.SortHorizontal);  
    //  else mChildren.Sort(UIGrid.SortVertical);  
    public override void SortBasedOnScrollMovement()
    {
        if (!CacheScrollView()) return;

        // Cache all children and place them in order  
        mChildren.Clear();
        for (int i = 0; i < mTrans.childCount; ++i)
        {
            Transform t = mTrans.GetChild(i);
            if (!t.gameObject.activeInHierarchy) continue;
            mChildren.Add(t);
        }

        ResetChildPositions();
    }

    public override void WrapContent()
    {
        float extents = itemSize * (mChildren.Count / RowNum) * 0.5f;
        Vector3[] corners = mPanel.worldCorners;

        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = corners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
        bool allWithinRange = true;
        float ext2 = extents * 2f;

        if (mHorizontal)
        {
            float min = corners[0].x - itemSize;
            float max = corners[2].x + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.x - center.x;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x += ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.x - mTrans.localPosition.x;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }
        else
        {
            float min = corners[0].y - itemSize;
            float max = corners[2].y + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.y - center.y;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y += ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y -= ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += mPanel.clipOffset.y - mTrans.localPosition.y;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }
        mScroll.restrictWithinPanel = !allWithinRange;
        mScroll.InvalidateBounds();
    }
}