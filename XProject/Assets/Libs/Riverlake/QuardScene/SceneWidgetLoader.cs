/************************************************************
* @Author: LiangZG
* @Time : 2018-1-2
************************************************************/
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using Riverlake;
using UnityEngine;
using Riverlake.Resources;

namespace Riverlake.Scene
{
    public class LoadFactor
    {
        public float Max = 1.0f;
        public float Weight = 1.0f;
    }

    /// <summary>
    /// 物件加载器
    /// </summary>
    public class SceneWidgetLoader
    {

        private QuadScene quadScene;
        private AsyncContain contain;
        private List<SceneWidget> loadList = new List<SceneWidget>();
        private StaticBatchWidgets staticBatch;
        private bool isLoading;

        private LoadFactor boundFactor = new LoadFactor()
        {
            Max = 1000 , Weight = 0.4f
        };

        private LoadFactor disFactor = new LoadFactor()
        {
            Max = 2000 , Weight = 0.6f
        };

        public SceneWidgetLoader(QuadScene quadScene)
        {
            this.quadScene = quadScene;
            contain = new AsyncContain();
        }

        /// <summary>
        /// 添加物件
        /// </summary>
        /// <param name="sceneWidget">物件</param>
        public void LoadWidgets(SceneWidget sceneWidget)
        {
            loadList.Add(sceneWidget);
        }

        private IEnumerator asyncLoading()
        {
            while (contain.MoveNext())
            {
                yield return Yielders.EndOfFrame;
            }
            isLoading = false;
            contain.Reset();
        }


        public void Update()
        {
            if (isLoading || loadList.Count == 0)   return;
            
            loadList.Sort(sortWidgetLoading);

//            StringBuilder buf = new StringBuilder();
            for (int i = 0, count = loadList.Count; i < count; i++)
            {
                SceneWidget sceneWidget = loadList[i];

                ALoadOperation loader = ResourceManager.LoadAssetAsync(sceneWidget.WidgetData.PrefabPath);
                loader.OnFinish = sceneWidget.LoadWidgetFinish;

                this.contain.AddLoader(loader, 1);

//                float factor = calculateLoadFactor(sceneWidget, this.quadScene.FocusLeafNode.Bounds3.center);
//                buf.AppendLine(string.Format("factor:{0}", factor));
            }
//            Debug.Log(buf.ToString());
            loadList.Clear();

            isLoading = true;
            this.quadScene.StartCoroutine(this.asyncLoading());
            
        }

        /// <summary>
        ///  排序加载组件
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int sortWidgetLoading(SceneWidget x, SceneWidget y)
        {
            Vector3 pos = this.quadScene.FocusLeafNode.Bounds3.center;
            float factorX = calculateLoadFactor(x, pos);
            float factorY = calculateLoadFactor(y, pos);

            if (factorX > factorY) return -1;
            if (factorX < factorY) return 1;

            return 0;
        }


        private float calculateLoadFactor(SceneWidget widget , Vector3 position)
        {
            float boundSize = widget.Bounds.size.sqrMagnitude;
            float disfactor = widget.Distance(position);

            float totalFactor = calculatePercent(boundFactor, boundSize);
            totalFactor += disFactor.Weight - calculatePercent(disFactor, disfactor);

            return totalFactor;
        }

        /// <summary>
        /// 估算参数的百分比
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private float calculatePercent(LoadFactor factor, float value)
        {
            float normalized = QTMath.Percent(0, factor.Max, value);

            return normalized * Mathf.Clamp01(factor.Weight);
        }


        public void Clear()
        {
            this.contain.Reset();
        }
    }

}

