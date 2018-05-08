using System.Text;
using UnityEngine;

namespace Riverlake.Scene
{
    /// <summary>
    /// 常用工具集
    /// </summary>
    public sealed class QuadSceneUtil
    {
        /// <summary>
        /// 获得相对于根结点的层次信息
        /// </summary>
        /// <param name="trans">查询结点transform</param>
        /// <param name="root">相对的根结点信息</param>
        public static string HierarchyText(Transform trans, Transform root)
        {
            StringBuilder buf = new StringBuilder();
            findHierarchyText(trans , root , buf);
            return buf.ToString();
        }


        public static void findHierarchyText(Transform trans, Transform root, StringBuilder buf)
        {
            if (trans == root) return;

            buf.Append(trans.name).Append(".");

            findHierarchyText(trans.parent, root, buf);
        }
    }


    /// <summary>
    /// 四元树数据计算
    /// </summary>
    public sealed class QTMath
    {
        /// <summary>
        /// 目标Target的边界是否被指定矩形区域（Src）包含
        /// </summary>
        /// <param name="src">查询区域</param>
        /// <param name="target">包含矩形区域</param>
        /// <returns>如果src包含target，则返回true</returns>
        public static bool IsInside(ref Rect src, ref Rect target)
        {
            bool outXSide = target.xMin < src.xMin || target.xMax > src.xMax;
            bool outYSide = target.yMin < src.yMin || target.yMax > src.yMax;
            return !outXSide && !outYSide;
        }

        /// <summary>
        /// 目标元素（target）的边界跟指定源矩形（src）有交集
        /// </summary>
        /// <param name="src">查询源区域</param>
        /// <param name="target">检测矩形区域</param>
        /// <returns>如果src与target相交，则返回true</returns>
        public static bool IsOverlaps(ref Rect src, ref Rect target)
        {
            return src.Overlaps(target);
        }

        /// <summary>
        /// 目标区域（target）的边界是否同Camera的显示区域有交集
        /// </summary>
        /// <param name="frustums">Camera视锤切面</param>
        /// <param name="bound">检测矩形区域</param>
        /// <returns>如果src与target相交，则返回true</returns>
        public static bool IsOverlapFrustums(Plane[] frustums, ref Bounds bound)
        {
            return GeometryUtility.TestPlanesAABB(frustums, bound);
        }


        /// <summary>
        /// 元素的边界跟指定矩形有交集
        /// </summary>
        /// <param name="bound">原矩形区域</param>
        /// <param name="targetCenter">目标的中心</param>
        /// <param name="targetRadius">目标的半径</param>
        /// <returns>如果src与target相交，则返回true</returns>
        public static bool IsOverlaps(ref Rect bound, ref Vector2 targetCenter, float targetRadius)
        {
            bool xOutside = targetCenter.x + targetRadius < bound.xMin || targetCenter.x - targetRadius > bound.xMax;
            bool yOutside = targetCenter.y + targetRadius < bound.yMin || targetCenter.y - targetRadius > bound.yMax;
            bool outside = xOutside || yOutside;
            return !outside;
        }

        /// <summary>
        /// 计算百分比
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Percent(float min, float max, float value)
        {
            if (float.Equals(min, max)) return 0.0f;

            float clamped = Mathf.Clamp(value, min, max);
            return (clamped - min) / (max - min);
        }
    }
}