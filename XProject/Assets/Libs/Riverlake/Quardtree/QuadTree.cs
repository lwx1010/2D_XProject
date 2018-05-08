/************************************************************
* Author: LiangZG
* Time : 2017-10-30
************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Riverlake.Scene
{
    /// <summary>
    /// 四叉树
    /// </summary>
    public class QuadTree<T> where T:class
    {
        public delegate QTBaseNode<T> QtCreateNode(Rect bnd);
        
        protected QTPointNode<T> root;
        /// <summary>
        /// 空间大小
        /// </summary>
        public Vector2 CellSize { get; private set; }

        private Dictionary<T , QTBaseNode<T>> table = new Dictionary<T, QTBaseNode<T>>();

        public QTPointNode<T> Root
        {
            get { return root; }
        }

        public QuadTree(Rect bound, int cellSize)
        {
            if(cellSize <= 0)
                throw new ArgumentException("cellSize", "cellSize cannot be less then 1.");
            
            Rebuild(ref bound , cellSize);
        } 

        /// <summary>
        /// 向四叉对中插入结点
        /// </summary>
        /// <param name="item">数据item</param>
        /// <param name="bound">位置区域信息</param>
        public void Insert(T item, Rect bound)
        {
            root.Insert(item ,ref bound);
        }

        /// <summary>
        /// 从四叉树中删除结点
        /// </summary>
        /// <param name="item">数据item</param>
        public void Remove(T item)
        {
            root.Remove(item);   
        }

        /// <summary>
        /// 从四叉树中检索指定矩形内的元素（元素的边界被指定矩形包含）
        /// </summary>
        /// <param name="bound">指定矩形区域</param>
        /// <returns></returns>
        public IEnumerable<T> InsideItems(ref Rect bound)
        {
            return Root.InsideItems(ref bound);
        }

        /// <summary>
        /// 从四叉树中检索指定矩形内的元素（元素的边界跟指定矩形有交集）
        /// </summary>
        /// <param name="bound">查询矩形区域</param>
        /// <returns></returns>
        public IEnumerable<T> OverlapItems(ref Rect bound)
        {
            return Root.OverlapItems(ref bound);
        }

        /// <summary>
        /// 用指定的包含所有元素的边界重新构建四叉树
        /// </summary>
        /// <param name="bound">指定的边界</param>
        /// <param name="minCellSize">最小单元尺寸</param>
        public void Rebuild(ref Rect bound , int minCellSize)
        {
            root = new QTPointNode<T>(ref bound);
            CellSize = calMinBounds(bound.width, bound.height, minCellSize);

            BuildNode(root , minCellSize);
        }


        /// <summary>
        /// 判断指定矩形的的元素个数是否是小于thresholdCount参数指定的值（与指定矩形相交即可算作矩形内的元素）
        /// </summary>
        /// <param name="bounds">指定的边界</param>
        /// <param name="thresholdCount">临界值数量</param>
        /// <returns></returns>
//        public bool PredicateCount(ref Rect bounds, int thresholdCount)
//        {
//            return false;
//        }


        /// <summary>
        /// 重新构建指定四叉树结点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="minCellSize">最小单元尺寸</param>
        public void BuildNode(QTPointNode<T> node , int minCellSize)
        {
            // parameters
            float subWidth = node.Bound.width * 0.5f;
            float subHeight = node.Bound.height * 0.5f;
            bool isPartible = subWidth >= minCellSize && subHeight >= minCellSize;

            // create subnodes
            QtCreateNode _nodeCreator = (bnd) => { return new QTPointNode<T>(ref bnd); };
            QtCreateNode _leafCreator = (bnd) => { return new QTLeafNode<T>(ref bnd ); };

            QtCreateNode creator = isPartible ? _nodeCreator : _leafCreator;
            node.SetChildNode(
                            creator(new Rect(node.Bound.xMin, node.Bound.yMin, subWidth, subHeight)),
                            creator(new Rect(node.Bound.xMin + subWidth, node.Bound.yMin, subWidth, subHeight)),
                            creator(new Rect(node.Bound.xMin, node.Bound.yMin + subHeight, subWidth, subHeight)),
                            creator(new Rect(node.Bound.xMin + subWidth, node.Bound.yMin + subHeight, subWidth, subHeight))
                            );

            // do it recursively
            if (isPartible)
            {
                for (int i = 0; i < node.ChildNodes.Length; i++)
                {
                    QTPointNode<T> subNode = node.ChildNodes[i] as QTPointNode<T>;
                    BuildNode(subNode , minCellSize);
                }
            }
        }

        /// <summary>
        /// 查询包含指定点的叶结点
        /// </summary>
        /// <param name="point">指定点</param>
        /// <returns>包含指定点的叶结点</returns>
        public QTLeafNode<T> InsideLeafNode(ref Vector2 point)
        {
            return findContainLeafNode(root , ref point);
        }

        /// <summary>
        /// 查询相交区域的叶结点
        /// </summary>
        /// <param name="bound">相交区域</param>
        /// <returns>多个相交叶结点</returns>
        public List<QTLeafNode<T>> OverlapLeafNodes(ref Rect bound)
        {
            List<QTLeafNode<T>> leafs = new List<QTLeafNode<T>>();
            findOverlapLeafNode(leafs , root , ref bound);
            return leafs;
        }

        /// <summary>
        /// 查询指定点的叶结点
        /// </summary>
        /// <param name="node">启初结点</param>
        /// <param name="point">指定点</param>
        /// <returns>相交区域指定点的叶结点</returns>
        private static QTLeafNode<T> findContainLeafNode(QTBaseNode<T> node, ref Vector2 point)
        {
            if (!node.Bound.Contains(point)) return null;

            if (node.NodeType == QTBaseNode<T>.ENodeType.Leaf)
                return node as QTLeafNode<T>;

            QTPointNode<T> newPointNode = node as QTPointNode<T>;
            for (int i = 0, length = newPointNode.ChildNodes.Length; i < length; i++)
            {
                QTLeafNode<T> targetNode = findContainLeafNode(newPointNode.ChildNodes[i], ref point);

                if (targetNode != null) return targetNode;
            }

            return null;
        }
        /// <summary>
        /// 查找相交的叶结点
        /// </summary>
        /// <param name="node">查询结点</param>
        /// <param name="leafs">记录容器</param>
        /// <returns></returns>
        private static void findOverlapLeafNode(List<QTLeafNode<T>> leafs , QTBaseNode<T> node, ref Rect bound)
        {
            Rect curBound = node.Bound;
            if (!QTMath.IsOverlaps(ref curBound, ref bound)) return ;

            if (node.NodeType == QTBaseNode<T>.ENodeType.Leaf)
            {
                leafs.Add(node as QTLeafNode<T>);
                return ;
            }

            QTPointNode<T> newPointNode = node as QTPointNode<T>;
            for (int i = 0, length = newPointNode.ChildNodes.Length; i < length; i++)
            {
                findOverlapLeafNode(leafs , newPointNode.ChildNodes[i], ref bound);
            }
            
        }


        /// <summary>
        /// 估算最小空间区域尺寸
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="minCellSize"></param>
        /// <returns></returns>
        public Vector2 calMinBounds(float width , float height, int minCellSize)
        {
            if(width < minCellSize || height < minCellSize)
                return new Vector2(width, height);

            return calMinBounds(width * 0.5f, height * 0.5f, minCellSize);
        }
    }

}

