using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Riverlake
{
    /// <summary>
    /// 应用系统级的API接口
    /// </summary>
    public class AppSystem : Singleton<AppSystem>
    {
        const long KBSize = 1024 * 1024;

        #region -----Private Attributes----------
        private float delta;

        //----------fps-------------------
        private int fps = 0;
        private float updateInterval = 0.5f;
        private float accum = 0; // FPS accumulated over the interval
        private int frames = 100; // Frames drawn over the interval
        private float remainTime = 0.5f; // Remaining time for current interval

        //-----------memory----------------
        private int memory; //单位MB
        private float totalMemory;
        #endregion

        #region Public Attributes
        /// <summary>
        /// 当前的系统帧数
        /// </summary>
        public static int FPS
        {
            get { return Instance.fps; }
        }
        /// <summary>
        /// 当前系统使用的内存
        /// </summary>
        public static int Memory
        {
            get
            {
                Instance.updateMemorySample();
                return Instance.memory;
            }
        }

        #endregion
        
        public void OnUpdate()
        {
            delta = Time.deltaTime;

            this.updateFps();
        }

        /// <summary>
        /// 更新FPS的计算
        /// </summary>
        private void updateFps()
        {
            remainTime -= delta;
            accum += Time.timeScale / delta;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (remainTime <= 0)
            {
                fps = (int)(accum / frames);
                remainTime = updateInterval;
                accum = 0.0f;
                frames = 0;
            }
        }

        /// <summary>
        /// 更新内存采样
        /// </summary>
        private void updateMemorySample()
        {
            memory = (int)(Profiler.GetTotalAllocatedMemoryLong() / KBSize);
        }

        /// <summary>
        /// 获取当前设备信息
        /// </summary>
        public static string Dump()
        {
            StringBuilder systemInfo = new StringBuilder();
            systemInfo.AppendLine("******************************************************");
            systemInfo.AppendLine("******************************************************");
            systemInfo.AppendLine("** 设备模型：" + SystemInfo.deviceModel);
            systemInfo.AppendLine("** 设备名称：" + SystemInfo.deviceName);
            systemInfo.AppendLine("** 设备类型：" + SystemInfo.deviceType);
            systemInfo.AppendLine("** 设备唯一标识符：" + SystemInfo.deviceUniqueIdentifier);
            systemInfo.AppendLine("** 显卡标识符：" + SystemInfo.graphicsDeviceID);
            systemInfo.AppendLine("** 显卡设备名称：" + SystemInfo.graphicsDeviceName);
            systemInfo.AppendLine("** 显卡厂商：" + SystemInfo.graphicsDeviceVendor);
            systemInfo.AppendLine("** 显卡厂商ID:" + SystemInfo.graphicsDeviceVendorID);
            systemInfo.AppendLine("** 显卡支持版本:" + SystemInfo.graphicsDeviceVersion);
            systemInfo.AppendLine("** 显存（M）：" + SystemInfo.graphicsMemorySize);
            systemInfo.AppendLine("** 显卡支持Shader层级：" + SystemInfo.graphicsShaderLevel);
            systemInfo.AppendLine("** 支持最大图片尺寸：" + SystemInfo.maxTextureSize);
            systemInfo.AppendLine("** npotSupport：" + SystemInfo.npotSupport);
            systemInfo.AppendLine("** 操作系统：" + SystemInfo.operatingSystem);
            systemInfo.AppendLine("** CPU处理核数：" + SystemInfo.processorCount);
            systemInfo.AppendLine("** CPU类型：" + SystemInfo.processorType);
            systemInfo.AppendLine("** supportedRenderTargetCount：" + SystemInfo.supportedRenderTargetCount);
            systemInfo.AppendLine("** supports3DTextures：" + SystemInfo.supports3DTextures);
            systemInfo.AppendLine("** supportsAccelerometer：" + SystemInfo.supportsAccelerometer);
            systemInfo.AppendLine("** supportsComputeShaders：" + SystemInfo.supportsComputeShaders);
            systemInfo.AppendLine("** supportsGyroscope：" + SystemInfo.supportsGyroscope);
            systemInfo.AppendLine("** supportsImageEffects：" + SystemInfo.supportsImageEffects);
            systemInfo.AppendLine("** supportsInstancing：" + SystemInfo.supportsInstancing);
            systemInfo.AppendLine("** supportsLocationService：" + SystemInfo.supportsLocationService);
            systemInfo.AppendLine("** supportsRenderToCubemap：" + SystemInfo.supportsRenderToCubemap);
            systemInfo.AppendLine("** supportsShadows：" + SystemInfo.supportsShadows);
            systemInfo.AppendLine("** supportsSparseTextures：" + SystemInfo.supportsSparseTextures);
            systemInfo.AppendLine("** supportsVibration：" + SystemInfo.supportsVibration);
            systemInfo.AppendLine("** 内存大小：" + SystemInfo.systemMemorySize);
            systemInfo.AppendLine("******************************************************");
            systemInfo.AppendLine("******************************************************");

            return systemInfo.ToString();
        }
    }


}
