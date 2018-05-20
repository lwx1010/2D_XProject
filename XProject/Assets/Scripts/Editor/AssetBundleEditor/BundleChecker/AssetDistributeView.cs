using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BundleChecker
{
    /// <summary>
    /// 资源分布页
    /// </summary>
    public class AssetDistributeView
    {
        private Vector2 scrollPos = Vector2.zero;
        private int indexRow;

        private ResoucresBean curRes;
        private string searchFilter = "";
        private string tipStr = "资源详细的Bundle分布";
        public void OnGUI()
        {

            if (curRes.Dependencies.Count > 0)
            {
                drawDependencieAsset();
                GUILayoutHelper.DrawSeparator();
            }

            drawAllBundles();
        }


        public void SetResoucre(ResoucresBean res)
        {
            curRes = res;
            searchFilter = "";
            GUIUtility.keyboardControl = 0;

            string title = string.Format("<color=white>[{1}]<color=green>{0}</color></color>", curRes.Name, curRes.ResourceType);

            ABMainChecker.MainChecker.SetCurrentView(ABMainChecker.EView.AssetDistributeView , title);
        }
        /// <summary>
        /// 冗余的资源
        /// </summary>
        private void drawAllBundles()
        {
            EditorGUILayout.HelpBox(tipStr , MessageType.Info);
            //all assets
            GUILayoutHelper.DrawHeader("All AssetBundle");

            //Search
            GUILayout.BeginHorizontal();
            {
                searchFilter = EditorGUILayout.TextField("", searchFilter, "SearchTextField", GUILayout.Width(Screen.width - 20f));

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    searchFilter = "";
                    GUIUtility.keyboardControl = 0;
                }
            }
            GUILayout.EndHorizontal();

//            GUILayout.BeginHorizontal();
//            GUILayout.Toggle(false, "所属AssetBundle名称", "ButtonLeft");
//            //GUILayout.Toggle(false, "依赖数量", "ButtonMid", GUILayout.Width(80));
//            //GUILayout.Toggle(false, "所属AssetBundle文件", "ButtonMid");
//            GUILayout.Toggle(false, "详细", "ButtonRight", GUILayout.Width(100));
//            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos , false , true);


            GUILayout.BeginVertical();
            int countIndex = 0;
            int endIndex = 0;
            int column = 4;
            int width = (int)(ABMainChecker.MainChecker.Width - 50)/column;
            for (int i = 0, maxCount = curRes.IncludeBundles.Count; i < maxCount; i++)
            {
                EditorBundleBean bundle = curRes.IncludeBundles[i];

                if (countIndex % column == 0)
                {
                    endIndex = countIndex + column - 1;
                    GUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(bundle.BundleName , GUILayout.Width(width)))
                {
                    ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(bundle);
                }

                if (countIndex == endIndex)
                {
                    endIndex = 0;
                    GUILayout.EndHorizontal();
                }
                countIndex++;
            }
            if (endIndex != 0) GUILayout.EndHorizontal();
            GUILayout.EndVertical();

//            foreach (EditorBundleBean bundle in curRes.IncludeBundles)
//            {
//                if(string.IsNullOrEmpty(searchFilter) || bundle.BundleName.Contains(searchFilter))
//                    drawBundle(bundle);
//            }
            GUILayout.EndScrollView();
        }

        private void drawBundle(EditorBundleBean bundle)
        {
            indexRow++;
            GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;

//          名称
            GUILayout.Label(bundle.BundleName);

//          查询
            GUILayout.Space(15);
            if (GUILayout.Button("GO", GUILayout.Width(50), GUILayout.Height(25)))
            {
                ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(bundle);
                            
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }

        #region -------------------依赖的资源----------------------------

        private void drawDependencieAsset()
        {
            GUILayoutHelper.DrawHeader("Dependencies");

            int column = 3;
            int columnWidth = Mathf.Max( 1, (int)(ABMainChecker.MainChecker.Width - 30)/ column);

            GUILayout.BeginVertical();
            int endIndex = 0;
            string missingStr = "{0}(missing)";
            int i = 0;
            foreach (ResoucresBean depRes in curRes.Dependencies.Values)
            {
                if (i % column == 0)
                {
                    endIndex = i + column - 1;
                    GUILayout.BeginHorizontal();
                }

                if (depRes.IsMissing)
                {
                    GUI.backgroundColor = Color.red ;
                    GUILayout.Button(string.Format(missingStr, depRes.Name), GUILayout.Width(columnWidth));
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    if (GUILayout.Button(depRes.Name, GUILayout.Width(columnWidth)))
                    {
                        ABMainChecker.MainChecker.AssetView.SetResoucre(depRes);
                    }
                }

                GUILayout.Space(5);

                if (i == endIndex)
                {
                    endIndex = 0;
                    GUILayout.EndHorizontal();
                }
                i ++;
            }
            if (endIndex != 0) GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        

        #endregion

    }
}