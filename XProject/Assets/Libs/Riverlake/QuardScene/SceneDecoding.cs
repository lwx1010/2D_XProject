using UnityEngine;
using System.IO;
using Riverlake.Scene.Lightmap;

namespace Riverlake.Scene
{
    /// <summary>
    /// 场景解码
    /// </summary>
    public class SceneDecoding
    {
        /// <summary>
        /// 场景物件
        /// </summary>
        public SceneWidget.SceneWidgetData[] SceneWidgets;

        /// <summary>
        /// 解码场景导出数据
        /// </summary>
        /// <param name="fileBytes"></param>
        public void Decode(byte[] fileBytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
                stream.Flush();

                stream.Seek(0, SeekOrigin.Begin);

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int count = reader.ReadInt32();
                    SceneWidgets = new SceneWidget.SceneWidgetData[count];
                    for (int i = 0; i < count; i++)
                    {
                        SceneWidget.SceneWidgetData swd = readSceneWidget(reader);
                        SceneWidgets[i] = swd;
                    }
                }
            }
        }


        /// <summary>
        /// 读取物件数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static SceneWidget.SceneWidgetData readSceneWidget(BinaryReader reader)
        {
            SceneWidget.SceneWidgetData swd = new SceneWidget.SceneWidgetData();
            swd.PrefabPath = reader.ReadString();

            float boundX = reader.ReadSingle();
            float boundY = reader.ReadSingle();
            float boundWidth = reader.ReadSingle();
            float boundHeight = reader.ReadSingle();
            swd.Bounds = new Rect(boundX , boundY , boundWidth , boundHeight);

            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float posZ = reader.ReadSingle();
            swd.Position = new Vector3(posX , posY , posZ);


            float rotationX = reader.ReadSingle();
            float rotationY = reader.ReadSingle();
            float rotationZ = reader.ReadSingle();
            swd.Rotation = new Vector3(rotationX, rotationY, rotationZ);


            float scaleX = reader.ReadSingle();
            float scaleY = reader.ReadSingle();
            float scaleZ = reader.ReadSingle();
            swd.Scale = new Vector3(scaleX, scaleY, scaleZ);

            readLightmapsWidget(reader , swd);

            return swd;
        }

        /// <summary>
        /// 读取光照贴图信息
        /// </summary>
        /// <param name="reader"></param>
        private static void readLightmapsWidget(BinaryReader reader , SceneWidget.SceneWidgetData swd)
        {
            int length = reader.ReadInt32();
            if (length == 0) return;

            swd.Lightmaps = new WidgetLightmap[length];

            for (int i = 0; i < length; i++)
            {
                WidgetLightmap wl = new WidgetLightmap();

                wl.Hierarchy = reader.ReadString();
                wl.LightmapIndex = reader.ReadInt32();

                float scaleOffsetX = reader.ReadSingle();
                float scaleOffsetY = reader.ReadSingle();
                float scaleOffsetZ = reader.ReadSingle();
                float scaleOffsetW = reader.ReadSingle();
                wl.LightmapScaleOffset = new Vector4(scaleOffsetX , scaleOffsetY , scaleOffsetZ , scaleOffsetW);

                swd.Lightmaps[i] = wl;
            }
        }
    }
}

