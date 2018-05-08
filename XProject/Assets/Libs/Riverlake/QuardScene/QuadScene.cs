/************************************************************
* Author: LiangZG
* Time : 2017-11-1
************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using Riverlake.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riverlake.Scene
{
    /// <summary>
    /// 场景距焦器
    /// </summary>
    public class QuadScene : MonoBehaviour
    {
        public enum ELookType
        {
            Target , Camera
        }

        private QuadTree<ISceneWidget> qt;
        /// <summary>
        /// 距焦对象
        /// </summary>
        public Transform Target;
        /// <summary>
        /// 是否使用静态批合并
        /// </summary>
        [HideInInspector]
        public bool IsStaticBatch = true;

        [HideInInspector]
        public Vector2 CellSize;

        [HideInInspector]
        public Rect SceneBound;

        [HideInInspector]
        public Vector3 CellNum;

        // swap-in number of cells , InDistance = CellInCount * CellSize
        public int CellInCount = 1;
        // swap-out number of cells, MaxDistance = CellMaxCount * CellSize 
        //  (would be larger than swap-in to prevent poping)
        public int CellMaxCount = 2;
        /// <summary>
        /// 检测的间隔时间
        /// </summary>
        private float intervalTime = 0.2f;

        private float lastTime;

        private Vector2 minInBound;
        private Vector2 maxOutBound;
        private Vector3 screenCenter;
        private ELookType lookType = ELookType.Target;
        private Camera targetCam;
        private RaycastHit hit;
        
        private Rect maxBound;
        private Plane[] maxFrustums;

        private QTLeafNode<ISceneWidget> focusLeafNode;
        /// <summary>
        /// 当前正在显示中的叶结点
        /// </summary>
        private List<QTLeafNode<ISceneWidget>> showingNodes;

        private List<QTLeafNode<ISceneWidget>> enterLeafs;
        private List<QTLeafNode<ISceneWidget>> exitLeafs;
        
        private StaticBatchWidgets staticBatch;
        private SceneWidgetLoader widgetLoader;

        public Transform CacheTrans { get; private set; }

        public QTLeafNode<ISceneWidget> FocusLeafNode
        {
            get { return focusLeafNode; }
        }

        

        private void Awake()
        {
            widgetLoader = new SceneWidgetLoader(this);
            staticBatch = new StaticBatchWidgets(this);

            showingNodes = new List<QTLeafNode<ISceneWidget>>();
            enterLeafs = new List<QTLeafNode<ISceneWidget>>();
            exitLeafs = new List<QTLeafNode<ISceneWidget>>();

            CacheTrans = this.transform;
        }
        
        public void Initlize(Vector3 initTargetPos)
        {
            Vector2 point = new Vector2(initTargetPos.x, initTargetPos.z);

            QTLeafNode<ISceneWidget> insideLeaf = qt.InsideLeafNode(ref point);
            focusLeafNode = insideLeaf;

            //处理进入的结点
            Vector3 initMinInBound = qt.CellSize * 1.5f;
            Rect inBound = new Rect(focusLeafNode.Bound.center.x - initMinInBound.x * 0.5f,
                                     focusLeafNode.Bound.center.y - initMinInBound.y * 0.5f,
                                     initMinInBound.x, initMinInBound.y);
            List<QTLeafNode<ISceneWidget>> inLeafs = qt.OverlapLeafNodes(ref inBound);
            enterLeafs = calEnterNodes(inLeafs);
            this.swapEnter(enterLeafs);
        }

        /// <summary>
        /// 初始化场景区域大小
        /// </summary>
        /// <param name="sceneBinPath">相对于Res目录的.bytes文件路径</param>
        public void LoadSceneBin(string sceneBinPath)
        {
            byte[] fileBytes = ResourceManager.LoadBytes(sceneBinPath);

            SceneDecoding decoding = new SceneDecoding();
            decoding.Decode(fileBytes);
            
            qt = new QuadTree<ISceneWidget>(SceneBound, (int)Mathf.Max(CellSize.x , CellSize.y));

            minInBound = qt.CellSize * CellInCount;
            maxOutBound = qt.CellSize * CellMaxCount;

            for (int i = 0; i < decoding.SceneWidgets.Length; i++)
            {
                SceneWidget.SceneWidgetData swd = decoding.SceneWidgets[i];

                SceneWidget sw = new SceneWidget(this, swd);
                Insert(sw);
                //sw.OnBoundEnter();   //测试
            }
        }

        /// <summary>
        /// 设置观察目标
        /// </summary>
        /// <param name="target"></param>
        public void LookAt(Transform target)
        {
            lookType = ELookType.Target;
            this.Target = target;
        }

        /// <summary>
        /// 设置观察目标为摄相机的中心点
        /// </summary>
        /// <param name="cam"></param>
        public void LookCameraAt(Transform cam)
        {
            lookType = ELookType.Camera;
            this.Target = cam;
            targetCam = cam.GetComponent<Camera>();
            screenCenter = new Vector3(Screen.width*0.5f, Screen.height*0.5f);
        }

        /// <summary>
        /// 插入场景物件
        /// </summary>
        /// <param name="widget">物件对象</param>
        public void Insert(ISceneWidget widget)
        {
            qt.Insert(widget , widget.Bounds);
        }
        /// <summary>
        /// 删除物件
        /// </summary>
        /// <param name="widget">物件对象</param>
        public void Remove(ISceneWidget widget)
        {
            qt.Remove(widget);
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            this.LoadSceneBin(string.Format("SceneItemBin/{0}" , sceneName));
        }


        public void Update()
        {
            widgetLoader.Update();

            if (Target == null) return;

            Vector3 worldPos;
            if (lookType == ELookType.Camera && targetCam != null)
            {
                Ray ray = targetCam.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out hit))
                {
                    worldPos = hit.point;
                    Vector2 point = new Vector2(worldPos.x , worldPos.z);

                    QTLeafNode<ISceneWidget> insideLeaf = qt.InsideLeafNode(ref point);
                    if (insideLeaf == focusLeafNode)
                    {
                        focusLeafNode = insideLeaf;

                        //处理进入的结点
                        Matrix4x4 minMatrix = targetCam.projectionMatrix * Matrix4x4.Scale(new Vector3(1.2f, 1, 1.2f));
                        minMatrix = minMatrix * targetCam.worldToCameraMatrix;
                        Plane[] frustums = GeometryUtility.CalculateFrustumPlanes(minMatrix);
                        List<QTLeafNode<ISceneWidget>> inLeafs = this.OverlapLeafNodes(frustums);
                        enterLeafs = calEnterNodes(inLeafs);

                        //查询不在显示区域的结点
                        Matrix4x4 maxMatrix = targetCam.projectionMatrix * Matrix4x4.Scale(new Vector3(1.5f, 1, 1.5f));
                        maxMatrix *= targetCam.worldToCameraMatrix;
                        maxFrustums = GeometryUtility.CalculateFrustumPlanes(maxMatrix);
                        exitLeafs = this.OverlapLeafNodes(maxFrustums);
                        exitLeafs = calExitNodes(exitLeafs);
                    }                    
                }
            }
            else if (lookType == ELookType.Target)
            {
                worldPos = Target.position;
                Vector2 point = new Vector2(worldPos.x, worldPos.z);

                QTLeafNode<ISceneWidget> insideLeaf = qt.InsideLeafNode(ref point);
                if (insideLeaf != focusLeafNode)
                {
                    focusLeafNode = insideLeaf;

                    //处理进入的结点
                    Rect inBound = new Rect(focusLeafNode.Bound.center.x - minInBound.x * 0.5f,
                                                focusLeafNode.Bound.center.y - minInBound.y * 0.5f,
                                                minInBound.x, minInBound.y);
                    List<QTLeafNode<ISceneWidget>> inLeafs = qt.OverlapLeafNodes(ref inBound);
                    enterLeafs = calEnterNodes(inLeafs);

                    //处理退出的结点
                    maxBound = new Rect(focusLeafNode.Bound.center.x - maxOutBound.x * 0.5f,
                                                focusLeafNode.Bound.center.y - maxOutBound.y * 0.5f,
                                                maxOutBound.x, maxOutBound.y);
                    exitLeafs = qt.OverlapLeafNodes(ref maxBound);
                    exitLeafs = calExitNodes(exitLeafs);
                }
            }

            float curTime = Time.realtimeSinceStartup;
            if (curTime - lastTime > intervalTime)
            {
                lastTime = curTime;

                this.swapEnter(enterLeafs);

                if (lookType == ELookType.Target)
                    this.swapExit(exitLeafs, ref maxBound);
                else if (lookType == ELookType.Camera)
                    this.swapExitCamera(exitLeafs, maxFrustums);

                this.staticBatch.Update();
            }

        }

        /// <summary>
        /// 添加物件
        /// </summary>
        /// <param name="sceneWidget">物件</param>
        public void LoadWidgets(SceneWidget sceneWidget)
        {
            widgetLoader.LoadWidgets(sceneWidget);
        }
        
        /// <summary>
        /// 计算新增的进入可视范围的分区
        /// </summary>
        /// <returns>新增的结点列表</returns>
        private List<QTLeafNode<ISceneWidget>> calEnterNodes(List<QTLeafNode<ISceneWidget>> inLeafs)
        {
            for (int i = inLeafs.Count - 1; i >= 0; i--)
            {
                if(showingNodes.Contains(inLeafs[i]))
                    inLeafs.RemoveAt(i);
            }
            
            return inLeafs;
        }

        /// <summary>
        /// 计算超出范围需要退出的分区
        /// </summary>
        /// <returns>退出的结点列表</returns>
        private List<QTLeafNode<ISceneWidget>> calExitNodes(List<QTLeafNode<ISceneWidget>> maxBoundLeafs)
        {
            
            List<QTLeafNode<ISceneWidget>> exits = new List<QTLeafNode<ISceneWidget>>();

            for (int i = 0 , count = showingNodes.Count; i < count; i++)
            {
                if(!maxBoundLeafs.Contains(showingNodes[i]))
                    exits.Add(showingNodes[i]);
            }
            return exits;
        } 

        /// <summary>
        /// 处理物件进入到可视区
        /// </summary>
        /// <param name="leafs"></param>
        private void swapEnter(List<QTLeafNode<ISceneWidget>> leafs)
        {
//            if (!isEnterCompleted(focusLeafNode))
//            {
//                List<ISceneWidget> widgets = focusLeafNode.AllDatums;
//                for (int j = 0, widgetCount = widgets.Count; j < widgetCount; j++)
//                {
//                    if (widgets[j].IsEnterCompleted()) continue;
//
//                    widgets[j].OnBoundEnter();
//                }
//            }

            for (int i = leafs.Count - 1; i >= 0; i--)
            {
                if (isEnterCompleted(leafs[i]))
                {
                    //进入显示的叶点进行静态合并
                    if (IsStaticBatch)
                        staticBatch.AddStaticWideget(leafs[i]);
                    
                    leafs.RemoveAt(i);
                    continue;
                }

                List<ISceneWidget> widgets = leafs[i].AllDatums;
                for (int j = 0 , widgetCount = widgets.Count; j < widgetCount; j++)
                {
                    if(widgets[j].IsEnterCompleted())   continue;

                    widgets[j].OnBoundEnter();
                }

                showingNodes.Add(leafs[i]);
            }
        }

        /// <summary>
        /// 判断指定叶分区的物件是否已全部完成进入操作
        /// </summary>
        /// <param name="leaf">指定的叶分区</param>
        /// <returns>true表示完成</returns>
        private bool isEnterCompleted(QTLeafNode<ISceneWidget> leaf)
        {
            List<ISceneWidget> widgets = leaf.AllDatums;
            for (int i = 0 , count = widgets.Count; i < count; i++)
            {
                if (!widgets[i].IsEnterCompleted())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 处理物件退出可视区
        /// </summary>
        /// <param name="leafs"></param>
        private void swapExit(List<QTLeafNode<ISceneWidget>> leafs , ref Rect maxBound)
        {
            if (leafs == null) return;

            for (int i = leafs.Count - 1; i >= 0; i--)
            {
                if (isExitCompleted(leafs[i]))
                {
                    showingNodes.Remove(leafs[i]);
                    leafs.RemoveAt(i);
                    continue;
                }

                List<ISceneWidget> widgets = leafs[i].InsideDatas;
                for (int j = 0, widgetCount = widgets.Count; j < widgetCount; j++)
                {
                    if (widgets[j].IsExitCompleted()) continue;

                    widgets[j].OnBoundExit();
                }
                showingNodes.Remove(leafs[i]);

                List<ISceneWidget> crossWidgets = leafs[i].CrossDatas;
                for (int j = 0, widgetCount = crossWidgets.Count; j < widgetCount; j++)
                {
                    ISceneWidget childWidget = crossWidgets[j];
                    Rect childBounds = childWidget.Bounds;
                    if (childWidget.IsExitCompleted() || QTMath.IsOverlaps(ref childBounds, ref maxBound)) continue;

                    childWidget.OnBoundExit();
                }
            }
        }

        /// <summary>
        /// 处理物件退出Camera可视区
        /// </summary>
        private void swapExitCamera(List<QTLeafNode<ISceneWidget>> leafs, Plane[] frustums)
        {
            for (int i = 0, count = leafs.Count; i < count; i++)
            {
                if (isExitCompleted(leafs[i])) continue;

                List<ISceneWidget> widgets = leafs[i].InsideDatas;
                for (int j = 0, widgetCount = widgets.Count; j < widgetCount; j++)
                {
                    if (widgets[j].IsExitCompleted()) continue;

                    widgets[j].OnBoundExit();
                }
                showingNodes.Remove(leafs[i]);

                List<ISceneWidget> crossWidgets = leafs[i].CrossDatas;
                for (int j = 0, widgetCount = crossWidgets.Count; j < widgetCount; j++)
                {
                    ISceneWidget childWidget = crossWidgets[j];
                    Bounds childBounds = childWidget.Bounds3;
                    if (childWidget.IsExitCompleted() || QTMath.IsOverlapFrustums(frustums , ref childBounds)) continue;

                    childWidget.OnBoundExit();
                }
            }
        }

        /// <summary>
        /// 判断指定叶分区的物件是否已全部完成退出操作
        /// </summary>
        /// <param name="leaf">指定的叶分区</param>
        /// <returns>true表示完成</returns>
        private bool isExitCompleted(QTLeafNode<ISceneWidget> leaf)
        {
            List<ISceneWidget> insideWidgets = leaf.InsideDatas;
            for (int i = 0, count = insideWidgets.Count; i < count; i++)
            {
                if (!insideWidgets[i].IsExitCompleted())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 是否已经全部进入完成
        /// </summary>
        /// <returns></returns>
        public bool IsEnterDone()
        {
            if (enterLeafs == null) return false;

            for (int i = enterLeafs.Count - 1; i >= 0; i--)
            {
                if (!isEnterCompleted(enterLeafs[i]))
                {
                    return false;
                }
            }

//            if (focusLeafNode == null) return false;
//
//            if (!isEnterCompleted(focusLeafNode))
//            {
//                return false;
//            }
            return true;
        }

        /// <summary>
        /// 获得与Camera视面Plane相交的叶结点
        /// </summary>
        /// <param name="frumsh"></param>
        /// <returns></returns>
        public List<QTLeafNode<ISceneWidget>> OverlapLeafNodes(Plane[] frumsh)
        {
            List<QTLeafNode<ISceneWidget>> leafs = new List<QTLeafNode<ISceneWidget>>();
            findOverlapLeafNode(leafs, qt.Root, frumsh);
            return leafs;
        }


        /// <summary>
        /// 查找相交的叶结点
        /// </summary>
        /// <param name="node">查询结点</param>
        /// <param name="leafs">记录容器</param>
        /// <param name="frustums">Camera视锤体切面</param>
        /// <returns></returns>
        private static void findOverlapLeafNode<T>(List<QTLeafNode<T>> leafs, QTBaseNode<T> node, Plane[] frustums)
        {
            Bounds curBound = node.Bounds3;
            if (!QTMath.IsOverlapFrustums(frustums, ref curBound)) return;

            if (node.NodeType == QTBaseNode<T>.ENodeType.Leaf)
            {
                leafs.Add(node as QTLeafNode<T>);
                return;
            }

            QTPointNode<T> newPointNode = node as QTPointNode<T>;
            for (int i = 0, length = newPointNode.ChildNodes.Length; i < length; i++)
            {
                findOverlapLeafNode(leafs, newPointNode.ChildNodes[i], frustums);
            }

        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(qt != null)
                traverseAllNode(qt.Root);


            if (focusLeafNode != null)
            {
                if (lookType == ELookType.Target)
                {
                    Rect inBound = new Rect(focusLeafNode.Bound.center.x - minInBound.x * 0.5f,
                        focusLeafNode.Bound.center.y - minInBound.y * 0.5f,
                        minInBound.x, minInBound.y);
                    DrawRect(inBound, 0.5f, Color.green, 0);

                    //处理退出的结点
                    DrawRect(maxBound, 0.5f, Color.red, 0);
                }
            }
        }

        private void traverseAllNode(QTBaseNode<ISceneWidget> node)
        {
            if (node.NodeType == QTBaseNode<ISceneWidget>.ENodeType.Leaf)
            {
                QTLeafNode<ISceneWidget> leaf = node as QTLeafNode<ISceneWidget>;
                Color c = getDebugColor(leaf);
                DrawRect(leaf.Bound, 0.1f, c, 0.2f);
            }
            else if(node.NodeType == QTBaseNode<ISceneWidget>.ENodeType.Point)
            {
                QTPointNode<ISceneWidget> subRootNode = node as QTPointNode<ISceneWidget>;

                for (int i = 0, count = subRootNode.ChildNodes.Length; i < count; i++)
                {
                    traverseAllNode(subRootNode.ChildNodes[i]);
                }
            }
               
        }


        private Color getDebugColor(QTLeafNode<ISceneWidget> leaf)
        {
            if (leaf == focusLeafNode)
                return Color.blue;
            
            if (enterLeafs.Contains(leaf))
                return Color.green;

            if (exitLeafs.Contains(leaf))
                return Color.red;

            if (showingNodes.Contains(leaf))
                return  Color.white;

            return Color.gray;
        }
        
        private void DrawRect(Rect r, float y, Color c, float padding = 0.0f)
        {
            Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
            Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
            Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
            Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
        }
#endif

        private void OnDestroy()
        {
            this.enterLeafs.Clear();
            this.exitLeafs.Clear();
            this.showingNodes.Clear();
            this.widgetLoader.Clear();
        }
    }
}