/************************************************************
* Author: LiangZG
* Time : 2017-10-30
************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Riverlake.Scene
{
    /// <summary>
    /// 包含叶结点的根结点
    /// </summary>
    public class QTPointNode<T> : QTBaseNode<T>
    {

        private QTBaseNode<T>[] childNodeArr;
        /// <summary>
        /// 数据缓存
        /// </summary>
        private List<T> datas = new List<T>(); 
        public QTBaseNode<T>[] ChildNodes
        {
            get { return childNodeArr; }
        }

        public QTPointNode(ref Rect bound) : base(ref bound)
        {
            nodeType = ENodeType.Point;
        }


        /// <summary>
        /// 设置子结点
        /// </summary>
        /// <param name="topLeft">左上角区域结点</param>
        /// <param name="topRight">右上角区域结点</param>
        /// <param name="bottomLeft">左下角区域结点</param>
        /// <param name="bottomRight">右下角区域结点</param>
        public void SetChildNode(QTBaseNode<T> topLeft, QTBaseNode<T> topRight, QTBaseNode<T> bottomLeft, QTBaseNode<T> bottomRight)
        {
            childNodeArr = new QTBaseNode<T>[]
            {
                topLeft , topRight , bottomLeft , bottomRight
            };
        }

        /// <summary>
        /// 将数据插入到结点
        /// </summary>
        /// <param name="item">数据item</param>
        /// <param name="bound">位置区域信息</param>
        public override void Insert(T item, ref Rect bound)
        {
            if (!QTMath.IsOverlaps(ref _bound , ref bound)) return;

            for (int i = 0 , count = childNodeArr.Length; i < count; i++)
            {
                childNodeArr[i].Insert(item , ref bound);
            }
        }

        /// <summary>
        /// 删除结点内的数据
        /// </summary>
        /// <param name="item">数据item</param>
        public override void Remove(T item)
        {
            for (int i = 0, count = childNodeArr.Length; i < count; i++)
            {
                childNodeArr[i].Remove(item);
            }
        }
        
        public override IEnumerable<T> InsideItems(ref Rect bound)
        {
            if (!QTMath.IsInside(ref _bound, ref bound))
                return Enumerable.Empty<T>();

            datas.Clear();

            for (int i = 0, length = ChildNodes.Length; i < length; i++)
            {
                IEnumerable<T> items = ChildNodes[i].InsideItems(ref bound);

                datas.AddRange(items);
            }

            return datas;
        }

        /// <summary>
        /// 从四叉树中检索指定矩形内的元素（元素的边界跟指定矩形有交集）
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public override IEnumerable<T> OverlapItems(ref Rect bound)
        {
            if (!QTMath.IsOverlaps(ref _bound , ref bound))
                return Enumerable.Empty<T>();

            datas.Clear();
            
            for (int i = 0, length = ChildNodes.Length; i < length; i++)
            {
                IEnumerable<T>  items = ChildNodes[i].OverlapItems(ref bound);
                
                datas.AddRange(items);
            }

            return datas;
        }

    }

}

