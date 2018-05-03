using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BundleChecker
{
    /// <summary>
    /// AssetBundle检测工具主入口
    /// </summary>
    public class ABMainChecker : EditorWindow
    {
        private ABOverview overview = new ABOverview();
        
        private BundleDetailView bundleDetailView = new BundleDetailView();

        private AssetDistributeView assetView = new AssetDistributeView();

        public enum EView
        {
            OverView, BundleDetailView , AssetDistributeView
        }

        private EView curView = EView.OverView;

        public static ABMainChecker MainChecker;

        public Dictionary<string , EditorBundleBean> BundleList = new Dictionary<string, EditorBundleBean>();
        /// <summary>
        /// 资源池
        /// </summary>
        public Dictionary<string , ResoucresBean> ResourceDic = new Dictionary<string, ResoucresBean>();
        /// <summary>
        /// 丢失的资源
        /// </summary>
        public List<ResoucresBean> MissingRes = new List<ResoucresBean>();

        private GUIStyle titleLabStyle = new GUIStyle();
        private string subPageTitle = "";
        [MenuItem("AssetBundlePacker/Bundle 检测查询")]
        public static void ShowChecker()
        {
            MainChecker = EditorWindow.GetWindow<ABMainChecker>();
            MainChecker.OnInit();
            MainChecker.Show();
        }

        private void OnInit()
        {
            titleLabStyle.alignment = TextAnchor.MiddleCenter;
            titleLabStyle.fontSize = 25;
            titleLabStyle.fontStyle = FontStyle.Bold;
            titleLabStyle.richText = true;

            overview.Initlization();
        }


        private void OnGUI()
        {
            switch (curView)
            {
                    case EView.OverView:
                    overview.OnGUI();
                    break;
                default:
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("< Back" , GUILayout.Width(100) , GUILayout.Height(30)))
                    {
                        curView = EView.OverView;
                    }

                    GUILayout.Label(subPageTitle , titleLabStyle);
                    GUILayout.Space(100);
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    NGUIEditorTools.DrawSeparator();
                    break;
            }

            switch (curView)
            {
                case EView.BundleDetailView:
                    bundleDetailView.OnGUI();
                    break;
                case EView.AssetDistributeView:
                    assetView.OnGUI();
                    break;
            }
            GUILayout.Space(10);
        }

        public BundleDetailView DetailBundleView { get { return bundleDetailView;} }

        public AssetDistributeView AssetView { get { return assetView; } }

        public void SetCurrentView(EView view , string title)
        {
            this.curView = view;
            this.subPageTitle = title;
        }


        public float Width
        {
            get { return MainChecker.position.width; }
        }

        public float Height
        {
            get { return MainChecker.position.height;}
        }

        public void Clear()
        {
            this.ResourceDic.Clear();
            this.BundleList.Clear();
            this.MissingRes.Clear();
        }
    }
}