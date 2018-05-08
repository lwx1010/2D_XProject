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
    /// 四叉树中管理实际数据的叶结点
    /// </summary>
    public class QTLeafNode<T> : QTBaseNode<T>
    {

        private List<T> _insideDatas = new List<T>(); 
        /// <summary>
        /// 边界相交叉的数据
        /// </summary>
        private List<T> _crossDatas = new List<T>();
        /// <summary>
        /// 完全包含的数据
        /// </summary>
        public List<T> InsideDatas {  get{    return _insideDatas;  } }

        /// <summary>
        /// 边界相交叉的数据
        /// </summary>
        public List<T> CrossDatas   {   get { return _crossDatas; } }

        /// <summary>
        /// 相交的全部数据
        /// </summary>
        public List<T> AllDatums
        {
            get
            {
                List<T> overlapItems = new List<T>();
                overlapItems.AddRange(_insideDatas);
                overlapItems.AddRange(_crossDatas);
                return overlapItems;
            }
        } 

        public QTLeafNode(ref Rect bound) : base(ref bound)
        {
            nodeType = ENodeType.Leaf;
        }


        public override void Insert(T item,ref Rect bound)
        {
            if (!QTMath.IsOverlaps(ref _bound , ref bound)) return;

            if(QTMath.IsInside(ref _bound , ref bound))
                _insideDatas.Add(item);
            else
                _crossDatas.Add(item);
        }

        public override void Remove(T item)
        {
            _insideDatas.Remove(item);
            _crossDatas.Remove(item);
        }

        public override IEnumerable<T> OverlapItems(ref Rect bound)
        {
            if (!QTMath.IsOverlaps(ref _bound, ref bound))
                return Enumerable.Empty<T>();
            return AllDatums;
        }
        
        public override IEnumerable<T> InsideItems(ref Rect bound)
        {
            if (!QTMath.IsInside(ref _bound, ref bound))
                return Enumerable.Empty<T>();

            return _insideDatas;
        }
    }

}

