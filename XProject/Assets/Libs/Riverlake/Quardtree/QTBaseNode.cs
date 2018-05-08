/************************************************************
* Author: LiangZG
* Time : 2017-10-30
************************************************************/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Riverlake.Scene
{
    /// <summary>
    /// 公共父结点
    /// </summary>
    public abstract class QTBaseNode<T>
    {

        public enum ENodeType
        {
            Point , Leaf 
        }

        protected Rect _bound;

        protected Bounds _bounds3D;

        protected ENodeType nodeType;
        /// <summary>
        /// 区域信息
        /// </summary>
        public Rect Bound
        {
            get { return _bound; }
        }
        /// <summary>
        /// 3d区域信息
        /// </summary>
        public Bounds Bounds3
        {
            get
            {
                if(_bounds3D.size == Vector3.zero)
                    _bounds3D = new Bounds(new Vector3(_bound.center.x, 0, _bound.center.y),
                                  new Vector3(_bound.size.x, 0, _bound.size.y));
                return _bounds3D;
            }
        }

        /// <summary>
        /// 结点类型
        /// </summary>
        public ENodeType NodeType
        {
            get { return nodeType; }
        }



        public QTBaseNode(ref Rect bound)
        {
            _bound = bound;
           
        }


        #region ------容器查询-----
        /// <summary>
        /// 将数据插入到指定位置的结点
        /// </summary>
        /// <param name="item">数据item</param>
        /// <param name="bound">指定矩形区域</param>
        public abstract void Insert(T item, ref Rect bound);

        /// <summary>
        /// 删除指定数据
        /// </summary>
        /// <param name="item">数据item</param>
        public abstract void Remove(T item);

        /// <summary>
        /// 从四叉树中检索指定矩形内的元素（元素的边界被指定矩形包含）
        /// </summary>
        /// <param name="bound">指定矩形区域</param>
        /// <returns></returns>
        public abstract IEnumerable<T> InsideItems(ref Rect bound);

        /// <summary>
        /// 从四叉树中检索指定矩形内的元素（元素的边界跟指定矩形有交集）
        /// </summary>
        /// <param name="bound">指定矩形区域</param>
        /// <returns></returns>
        public abstract IEnumerable<T> OverlapItems(ref Rect bound);
        #endregion
    }
}

