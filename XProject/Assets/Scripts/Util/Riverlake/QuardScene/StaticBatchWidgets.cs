using System.Collections.Generic;
using UnityEngine;

namespace Riverlake.Scene
{
    /// <summary>
    /// 处理叶结点中物件的静态批
    /// </summary>
    public class StaticBatchWidgets
    {

        private Queue<QTLeafNode<ISceneWidget>> onBatchs = new Queue<QTLeafNode<ISceneWidget>>();
        
        private List<QTLeafNode<ISceneWidget>> waitBatchs = new List<QTLeafNode<ISceneWidget>>();
        
        private bool isBatch;

        //静态合并记录
        private HashSet<ISceneWidget> batchWidgetsLog;
        private HashSet<QTLeafNode<ISceneWidget>> staticBatchLeafLog;

        private QuadScene sceneLooker;
        public StaticBatchWidgets(QuadScene looker)
        {
            this.sceneLooker = looker;

            batchWidgetsLog = new HashSet<ISceneWidget>();
            staticBatchLeafLog = new HashSet<QTLeafNode<ISceneWidget>>();
        }

        /// <summary>
        /// 添加需要静态批合并的根结点
        /// </summary>
        /// <param name="root">根结点</param>
        public void AddStaticWideget(QTLeafNode<ISceneWidget> leaf)
        {
            if (staticBatchLeafLog.Contains(leaf) || waitBatchs.Contains(leaf)) return;
            
            waitBatchs.Add(leaf);
            isBatch = true;
        }

        public void Update()
        {
            if (!isBatch) return;

            if (waitBatchs.Count > 0)
            {
                for (int i = 0 , count = waitBatchs.Count; i < count; i++)
                {
                    onBatchs.Enqueue(waitBatchs[i]);
                }
                waitBatchs.Clear();
            }
            
            QTLeafNode<ISceneWidget> leaf = onBatchs.Dequeue();
            staticBatchLeaf(leaf);
            isBatch = onBatchs.Count > 0;
        }


        /// <summary>
        /// 静态合并网格
        /// </summary>
        /// <param name="leaf"></param>
        private void staticBatchLeaf(QTLeafNode<ISceneWidget> leaf)
        {
            staticBatchLeafLog.Add(leaf);

            List<ISceneWidget> allDatums = leaf.AllDatums;
            if (allDatums.Count == 0) return;


            GameObject staticBatchGO = new GameObject("_StaticWidgets");
            Transform staticBatchTrans = staticBatchGO.transform;
            staticBatchTrans.SetParent(this.sceneLooker.CacheTrans);

            List<GameObject> widgetRoots = new List<GameObject>();
            for (int i = 0, count = allDatums.Count; i < count; i++)
            {
                if (batchWidgetsLog.Contains(allDatums[i])) continue;

                widgetRoots.Add(allDatums[i].Widget);
                batchWidgetsLog.Add(allDatums[i]);

                allDatums[i].Widget.transform.SetParent(staticBatchTrans);
            }

            if (widgetRoots.Count == 0)
            {
                GameObject.Destroy(staticBatchGO);
                return;
            }

            staticBatchTrans.SetAsFirstSibling();

            StaticBatchingUtility.Combine(widgetRoots.ToArray(), staticBatchGO);
        }

    }
}