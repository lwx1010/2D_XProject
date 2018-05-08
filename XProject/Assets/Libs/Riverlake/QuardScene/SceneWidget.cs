using System;
using UnityEngine;
using LuaFramework;
using Riverlake.Scene.Lightmap;
using Riverlake.Resources;

namespace Riverlake.Scene
{
    /// <summary>
    /// 场景单一物件数据
    /// </summary>
    public class SceneWidget : ISceneWidget {

        /// <summary>
        /// 场景物件数据结构
        /// </summary>
        [Serializable]
        public class SceneWidgetData
        {
            public string PrefabPath;
            public Rect Bounds;

            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;

            public WidgetLightmap[] Lightmaps;
        }

        private SceneWidgetData widget;
        
        public enum EBoundStatus
        {
            None, Enter, Exit , Loading , InitFinish
        }

        private EBoundStatus boundState = EBoundStatus.None;
        private EBoundStatus selfState = EBoundStatus.None;

        private GameObject gameObject;
        private Transform cacheTrans;
        private QuadScene sceneLook;

        private Renderer[] renderers;
        #region ---------Public Attribute--------
        public Rect Bounds { get { return widget.Bounds; } }

        public Bounds Bounds3
        {
            get
            {
                Vector3 center = new Vector3(Bounds.center.x, 0, Bounds.center.y);
                Vector3 size = new Vector3(Bounds.size.x, 0, Bounds.size.y);
                return new Bounds(center, size);
            }
        }

        public SceneWidgetData WidgetData
        {
            get { return widget; }
        }

        public GameObject Widget { get { return this.gameObject; } }

        #endregion


        public SceneWidget(QuadScene sceneLook , SceneWidgetData widgetData)
        {
            this.sceneLook = sceneLook;
            this.widget = widgetData;
        }

        public void LoadWidgetFinish(ALoadOperation loader)
        {
            GameObject resObj = loader.GetAsset<GameObject>();
            if(resObj == null)
                Debug.LogError("Cant find Widget!" + this.WidgetData.PrefabPath);
            GameObject mainGO = GameObject.Instantiate(resObj);
            mainGO.transform.SetParent(sceneLook.CacheTrans);

            mainGO.transform.position = widget.Position;
            mainGO.transform.rotation = Quaternion.Euler(widget.Rotation);

            Vector3 parentLossyScael = sceneLook.CacheTrans.lossyScale;
            mainGO.transform.localScale = new Vector3(widget.Scale.x / parentLossyScael.x , 
                                                      widget.Scale.y / parentLossyScael.y , 
                                                      widget.Scale.z / parentLossyScael.z);

            selfState = EBoundStatus.InitFinish;
            gameObject = mainGO;
            cacheTrans = mainGO.transform;

            setLightmaps(mainGO);

            if (boundState == EBoundStatus.Enter)
                this.OnBoundEnter();
            else if(boundState == EBoundStatus.Exit)
                this.OnBoundExit();
            
        }

        /// <summary>
        /// 设置对应的光照信息
        /// </summary>
        /// <param name="go"></param>
        private void setLightmaps(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            Transform rootTrans = go.transform;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer childRenderer = renderers[i];
                string hierarchy = QuadSceneUtil.HierarchyText(childRenderer.transform, rootTrans);
                for (int j = 0; j < widget.Lightmaps.Length; j++)
                {
                    WidgetLightmap wl = widget.Lightmaps[j];
                    if (!wl.Hierarchy.Equals(hierarchy)) continue;

                    childRenderer.lightmapIndex = wl.LightmapIndex;
                    childRenderer.lightmapScaleOffset = wl.LightmapScaleOffset;

                    break;
                }
            }
        }
        
        public float Distance(Vector3 position)
        {
            return Vector3.Distance(this.widget.Position, position);
        }


        public void OnBoundEnter()
        {
            boundState = EBoundStatus.Enter;

            if (selfState == EBoundStatus.None)
            {
                selfState = EBoundStatus.Loading;
                this.sceneLook.LoadWidgets(this);
            }

            if (selfState != EBoundStatus.InitFinish) return;
            Renderer[] widgetRendererArr = Renderers;
            foreach (Renderer childRenderer in widgetRendererArr)
            {
                childRenderer.enabled = true;
            }
        }

        public void OnBoundExit()
        {
            boundState = EBoundStatus.Exit;
            if (selfState != EBoundStatus.InitFinish) return;

            Renderer[] widgetRendererArr = Renderers;
            foreach (Renderer childRenderer in widgetRendererArr)
            {
                childRenderer.enabled = false;
            }
        }

        public bool IsEnterCompleted()
        {
            if (selfState != EBoundStatus.InitFinish) return false;
            Renderer[] widgetRendererArr = Renderers;
            foreach (Renderer childRenderer in widgetRendererArr)
            {
                if (!childRenderer.enabled) return false;
            }
            return true;
        }

        public bool IsExitCompleted()
        {
            if (selfState != EBoundStatus.InitFinish) return false;

            Renderer[] widgetRendererArr = Renderers;
            foreach (Renderer childRenderer in widgetRendererArr)
            {
                if (childRenderer.enabled) return false;
            }
            return true;
        }

        public Renderer[] Renderers
        {
            get
            {
                if (renderers == null)
                {
                    renderers = gameObject.GetComponentsInChildren<Renderer>();
                }
                return renderers;
            }
        }
    }
}

