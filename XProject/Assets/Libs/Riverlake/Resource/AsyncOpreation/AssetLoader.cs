/*************************************
 * Author: luweixing
 * Email : kinglucn@hotmail.com
 * Time  : 2018-03-16 17:37:50
 *************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using UnityEngine;

namespace Riverlake.Resources
{
    /// <summary>
    /// 预设加载器
    /// </summary>
    public class AssetLoader
    {
        private AsyncContain contain;
        private List<AssetWidget> loadList = new List<AssetWidget>();
        private bool isLoading;
        private MonoBehaviour behaviour;
        public AssetLoader(MonoBehaviour behaviour)
        {
            this.contain = new AsyncContain();
            this.behaviour = behaviour;
        }

        /// <summary>
        /// 添加物件
        /// </summary>
        /// <param name="sceneWidget">物件</param>
        public void LoadAsset(AssetWidget widget)
        {
            loadList.Add(widget);
        }

        public void Update()
        {
            if (isLoading || loadList.Count == 0) return;

            loadList.Sort(sortAssetLoaderWidget);
            for (int i = 0, count = loadList.Count; i < count; i++)
            {
                AssetWidget uiWidget = loadList[i];

                ALoadOperation loader = ResourceManager.LoadBundleAsync(uiWidget.Name);
                loader.OnFinish = uiWidget.callback;

                this.contain.AddLoader(loader, 1);
            }
            loadList.Clear();

            isLoading = true;
            this.behaviour.StartCoroutine(this.asyncLoading());
        }

        private IEnumerator asyncLoading()
        {
            while (contain.MoveNext())
                yield return null;

            isLoading = false;
            contain.Reset();
        }

        private int sortAssetLoaderWidget(AssetWidget a, AssetWidget b)
        {
            if (a.weight > b.weight) return -1;
            if (a.weight < b.weight) return 1;
            return 0;
        }


        public void Clear()
        {
            this.contain.Reset();
        }
    }
}
