using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UR = UnityEngine.Resources;
using Riverlake.Resources;

namespace Riverlake
{
    public class ProgressBar : MonoBehaviour
    {
        public static ProgressBar curProgressBar;
        private static GameObject barGo;

        public static readonly string PrefabResourceName = "Progress Bar";
        private static readonly string DefaultBgPath = "Other/loading/ui_loading_box";
        private static readonly string DefaultFgPath = "Other/loading/ui_loading_01";

        public Image progress;
        public Image progressBg;
        public Text message;

        private Action onFinish;

        private float speed = 0.05f;
        private float nextMaxProgress = 1;
        private float lastProgress;
        private float lastTime;

        public static ProgressBar ShowCurBar()
        {
            if (curProgressBar != null)
                return curProgressBar;
            return Show();
        }

        public static ProgressBar Show(Action onFinished = null, Image.FillMethod fillMethod = Image.FillMethod.Horizontal, bool leftFill = true)
        {
            return Show(DefaultBgPath, DefaultFgPath, onFinished, fillMethod, leftFill); ;
        }

        public static ProgressBar Show(string bgPath, string fgPath, Action onFinished = null, Image.FillMethod fillMethod = Image.FillMethod.Horizontal, bool leftFill = true)
        {
            if (barGo != null) GameObject.DestroyImmediate(barGo);

            barGo = null;
            curProgressBar = null;
            var tempGo = Instantiate(ResourceManager.LoadGameObjectAssets(PrefabResourceName));
            barGo = tempGo;
            GameObject.DontDestroyOnLoad(barGo);
            curProgressBar = tempGo.GetComponent<ProgressBar>();
            curProgressBar.onFinish = onFinished;

            curProgressBar.progressBg.sprite = UR.Load<Sprite>(bgPath) as Sprite;
            curProgressBar.progressBg.SetNativeSize();
            curProgressBar.progress.sprite = UR.Load<Sprite>(fgPath) as Sprite;
            curProgressBar.progress.SetNativeSize();

            curProgressBar.progress.type = Image.Type.Filled;
            curProgressBar.progress.fillMethod = fillMethod;
            curProgressBar.progress.fillOrigin = leftFill ? 0 : 1;
            curProgressBar.progress.fillAmount = 0;
            return curProgressBar;
        }

        public void UpdateProgress(float value)
        {
            if (value > 0 && value < 1)
            {
                float dis = value - lastProgress;
                progress.fillAmount = Mathf.Max(progress.fillAmount, value);
                nextMaxProgress = value + dis;

                if (progress.fillAmount >= nextMaxProgress)
                {
                    nextMaxProgress = progress.fillAmount + dis;
                    speed = 0.02f;
                }
                else
                {
                    float useTime = Time.realtimeSinceStartup - lastTime;
                    speed = Mathf.Min(dis / useTime, 0.02f);
                }
                nextMaxProgress = Mathf.Max(nextMaxProgress, progress.fillAmount);
            }
            else
            {
                progress.fillAmount = value;
                speed = 0.02f;
                nextMaxProgress = 1;
            }

            lastProgress = value;
            lastTime = Time.realtimeSinceStartup;
        }

        public void SetMessage(string msg)
        {
            message.text = msg;
        }

        public void Hide()
        {
            if (barGo != null) DestroyImmediate(barGo);
            barGo = null;
            curProgressBar = null;
        }

        private void OnDestroy()
        {
            barGo = null;
            curProgressBar = null;
        }
    }
}
