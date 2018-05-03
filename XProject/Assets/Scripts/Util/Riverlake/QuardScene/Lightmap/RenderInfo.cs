using UnityEngine;

namespace Riverlake.Scene.Lightmap
{
    /// <summary>
    /// 物件光照信息
    /// </summary>
    public struct WidgetLightmap
    {
        /// <summary>
        /// 层次信息,用来索引查询
        /// </summary>
        public string Hierarchy;
        /// <summary>
        /// 对应Lightmap中的索引
        /// </summary>
        public int LightmapIndex;
        /// <summary>
        /// Lightmap中偏移和缩放
        /// </summary>
        public Vector4 LightmapScaleOffset;
    }
}
