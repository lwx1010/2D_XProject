using System;
using System.Collections.Generic;
using System.ComponentModel;
using BundleChecker.ResoucreAttribute;
using UnityEditor;
using UnityEngine;

namespace BundleChecker
{
    /// <summary>
    /// AssetBundle包内详细
    /// </summary>
    public class BundleDetailView
    {
        private EditorBundleBean curBundle;

        
        private int curTabIndex = 0;
        private int lastTabIndex = 0;

        private string selectAsset ="";
        private List<ResoucresBean> resList;
        private Vector2 scrollPos = Vector2.zero;

        private string tipStr = "* AssetBundle内部资源详情(红色资源为内置资或丢失的资源)";
        public void OnGUI()
        {
            GUI.color = Color.yellow;
            GUILayout.Label(tipStr);
            GUI.color = Color.white;

            drawTitle();

            GUILayoutHelper.DrawHeader("详情");

            if (lastTabIndex != curTabIndex)
            {
                lastTabIndex = curTabIndex;
                searchFilter = "";
                GUIUtility.keyboardControl = 0;
            }
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

            if (curTabIndex == 0)
            {
                drawAllAssets();
            }else if (curTabIndex == 1)
                drawTextureAssets();
            else if (curTabIndex == 2)
                drawMaterialAssets();
            else if (curTabIndex == 3)
                drawMeshAssets();
            else if (curTabIndex == 4)
                drawShaderAssets();
        }

        private void drawTitle()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(curTabIndex == 0, "所有资源", "ButtonLeft"))curTabIndex = 0;
            if (GUILayout.Toggle(curTabIndex == 1, "Texture", "ButtonMid")) curTabIndex = 1;
            if (GUILayout.Toggle(curTabIndex == 2, "Matrial", "ButtonMid")) curTabIndex = 2;
            if (GUILayout.Toggle(curTabIndex == 3, "Mesh", "ButtonMid")) curTabIndex = 3;
            if (GUILayout.Toggle(curTabIndex == 4, "Shader", "ButtonRight")) curTabIndex = 4;
            GUILayout.EndHorizontal();
        }

        #region ----------所有资源--------------

        private string searchFilter = "";
        private void drawAllAssets()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Asset名称" , GUILayout.Width(200));
            GUILayout.Label("类型", GUILayout.Width(100));
            GUILayout.Label("所属其它AssetBundle");
            GUILayout.Label("是否冗余", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayoutHelper.DrawSeparator();
            
            scrollPos = GUILayout.BeginScrollView(scrollPos , GUILayout.MinHeight(ABMainChecker.MainChecker.Height * 0.4f));
            int indexRow = 0;
            foreach (ResoucresBean res in resList)
            {
                if (string.IsNullOrEmpty(searchFilter) || res.Name.Contains(searchFilter))
                {
                    indexRow++;
                    drawRowAsset(res , indexRow);                   
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayoutHelper.DrawSeparator();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
                drawDepdencieBundle();
                GUILayout.Space(10);
                drawBeDepdencieBundle();
            GUILayout.EndHorizontal();
            
        }
        /// <summary>
        /// 绘制每条资源信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="indexRow"></param>
        private void drawRowAsset(ResoucresBean res ,int indexRow)
        {
            GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;

            GUI.color = selectAsset == res.Name ? Color.green : Color.white;
            if (GUILayout.Button(res.Name, EditorStyles.label, GUILayout.Width(200)))
            {
                selectAsset = res.Name;
            }
           
            GUILayout.Label(res.ResourceType, GUILayout.Width(100));

            //具体的ab名称                
            int column = Mathf.Max(1, (int)((ABMainChecker.MainChecker.Width - 380) / 150));
            if (res.IncludeBundles.Count > 1)
            {
                drawBundleGrid(curBundle , res, column);


            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制GriLayout布局的Bundle信息
        /// </summary>
        /// <param name="res"></param>
        /// <param name="column"></param>
        public static void drawBundleGrid(EditorBundleBean curBundle , ResoucresBean res, int column)
        {
            GUILayout.BeginVertical();
            int countIndex = 0;
            int endIndex = 0;
            for (int i = 0, maxCount = res.IncludeBundles.Count; i < maxCount; i++)
            {
                EditorBundleBean depBundle = res.IncludeBundles[i];
                if (depBundle == curBundle) continue;

                if (countIndex % column == 0)
                {
                    endIndex = countIndex + column - 1;
                    GUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(depBundle.BundleName, GUILayout.Width(140)))
                {
                    ABMainChecker.MainChecker.DetailBundleView.SetCurrentBundle(depBundle);
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

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("GO", GUILayout.Width(70), GUILayout.Height(25)))
            {
                ABMainChecker.MainChecker.AssetView.SetResoucre(res);
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        private Vector2 depScrollPos = Vector2.zero;
        /// <summary>
        /// 依赖的Bundle
        /// </summary>
        private void drawDepdencieBundle()
        {
            float width = ABMainChecker.MainChecker.Width*0.5f - 10;
            GUILayout.BeginVertical(GUILayout.Width(width));
            GUILayoutHelper.DrawHeader("依赖的AssetBundle");
            depScrollPos = GUILayout.BeginScrollView(depScrollPos);

            int indexRow = 0;
            foreach (EditorBundleBean depBundle in curBundle.GetAllDependencies())
            {
                indexRow++;
                GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                //Name
                GUILayout.Label(depBundle.BundleName);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private Vector2 beDepScrollPos = Vector2.zero;
        /// <summary>
        /// 被依赖的Bundle
        /// </summary>
        private void drawBeDepdencieBundle()
        {
            float width = ABMainChecker.MainChecker.Width * 0.5f - 10;
            GUILayout.BeginVertical(GUILayout.Width(width));
            GUILayoutHelper.DrawHeader("被其它AssetBundle依赖");
            beDepScrollPos = GUILayout.BeginScrollView(beDepScrollPos);

            int indexRow = 0;
            foreach (EditorBundleBean bundle in curBundle.GetBedependencies())
            {
                indexRow++;
                GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                //Name
                GUILayout.Label(bundle.BundleName);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        #endregion


        #region --------------绘制Texture分页--------------------

        private ResPropertyGUI[] texPropertys = new[]
        {
            new ResPropertyGUI{Title = "贴图名称" , PropertyName = ResourceGlobalProperty.Name, GuiWidth = 150},
            new ResPropertyGUI{Title = "宽度" , PropertyName = TextureAttribute.WIDTH, GuiWidth = 80},
            new ResPropertyGUI{Title = "高度" , PropertyName = TextureAttribute.HEIGHT, GuiWidth = 80},
            new ResPropertyGUI{Title = "内存占用" , PropertyName = TextureAttribute.MEMORYSIZE, GuiWidth = 100},
            new ResPropertyGUI{Title = "压缩格式" , PropertyName = TextureAttribute.FORMAT, GuiWidth = 150},
            new ResPropertyGUI{Title = "MipMap" , PropertyName = TextureAttribute.MIPMAP, GuiWidth = 80},
            new ResPropertyGUI{Title = "Read/Write" , PropertyName = TextureAttribute.READWRITE, GuiWidth = 80},
            new ResPropertyGUI{Title = "AB数量" , PropertyName = ResourceGlobalProperty.AssetBundles, GuiWidth = -1f},
        };
        /// <summary>
        /// 贴图信息
        /// </summary>
        private void drawTextureAssets()
        {
            drawAssetsAttribute(texPropertys , EResoucresTypes.TextureType);
        }

        #endregion

        #region -------------------材质分页----------------------------
        private ResPropertyGUI[] matPropertys = new[]
        {
            new ResPropertyGUI{Title = "材质名称" , PropertyName = ResourceGlobalProperty.Name, GuiWidth = 150},
            new ResPropertyGUI{Title = "Shader" ,  PropertyName = MaterialAttribute.DEP_SHADER, GuiWidth = 150},
            new ResPropertyGUI{Title = "引用贴图" ,  PropertyName = MaterialAttribute.DEP_TEXTURES, GuiWidth = - 0.5f},
            new ResPropertyGUI{Title = "AssetBundle" ,  PropertyName = ResourceGlobalProperty.AssetBundles, GuiWidth = - 0.5f},
        };
        private void drawMaterialAssets()
        {
           drawAssetsAttribute(matPropertys , EResoucresTypes.MatrialType);
        }
        #endregion

        #region --------------------网格信息-----------------------------
        private ResPropertyGUI[] meshPropertys = new[]
        {
            new ResPropertyGUI{Title = "Mesh名称" , PropertyName = ResourceGlobalProperty.Name, GuiWidth = 150},
            new ResPropertyGUI{Title = "AB数量" ,  PropertyName = ResourceGlobalProperty.ABCount, GuiWidth = 80},
            new ResPropertyGUI{Title = "AssetBundle" ,  PropertyName = ResourceGlobalProperty.AssetBundles, GuiWidth = - 1f},
        };
        #endregion
        private void drawMeshAssets()
        {
            drawAssetsAttribute(meshPropertys, EResoucresTypes.MeshType);
        }

        #region ------------------Shader -----------------------------

        private void drawShaderAssets()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Shader名称", GUILayout.Width(200));
            GUILayout.Label("AB数量", GUILayout.Width(80));
            GUILayout.Label("所属AssetBundle");
            GUILayout.Label("详情", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayoutHelper.DrawSeparator();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            List<ResoucresBean> resList = curBundle.GetAllAssets(EResoucresTypes.ShaderType);
            int index = 0;
            foreach (ResoucresBean res in resList)
            {
                if (string.IsNullOrEmpty(searchFilter) || res.Name.Contains(searchFilter))
                    drawShaderRowAsset(res, index++);
            }
            GUILayout.EndScrollView();
        }


        private void drawShaderRowAsset(ResoucresBean res, int indexRow)
        {
            GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;

            GUI.color = selectAsset == res.Name ? Color.green : Color.white;
            if (GUILayout.Button(res.Name, EditorStyles.label, GUILayout.Width(200)))
            {
                selectAsset = res.Name;
            }

            GUILayout.Label(res.IncludeBundles.Count.ToString(), GUILayout.Width(80));

            //具体的ab名称                
            if (res.IncludeBundles.Count > 1)
            {
                drawBundleGrid(curBundle , res, 4);
            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }

        #endregion


        private void drawAssetsAttribute(ResPropertyGUI[] propertyArr , string resType)
        {
            GUILayout.BeginHorizontal();
            foreach (ResPropertyGUI rpg in propertyArr)
            {
                if(rpg.GuiWidth > 0)
                    GUILayout.Label(rpg.Title , GUILayout.MaxWidth(rpg.GuiWidth));
                else
                    GUILayout.Label(rpg.Title);
            }
            GUILayout.EndHorizontal();

            GUILayoutHelper.DrawSeparator();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int indexRow = 0;
            foreach (ResoucresBean res in curBundle.GetAllAssets())
            {
                if (res.ResourceType != resType || (!string.IsNullOrEmpty(searchFilter) && !res.Name.Contains(searchFilter))) continue;


                indexRow++;
                GUI.backgroundColor = indexRow % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                if (res.RawRes == null)
                {
                    GUI.color = Color.red;
                    GUILayout.Label(res.Name);
                    GUI.color = Color.white;
                }
                else
                {
                    res.RawRes.SetPropertyGUI(propertyArr);
                    res.RawRes.OnGUI();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public void SetCurrentBundle(EditorBundleBean bundle)
        {
            curTabIndex = 0;
            this.curBundle = bundle;
            resList = bundle.GetAllAssets();
            resList.Sort((x , y)=>x.IncludeBundles.Count.CompareTo(y.IncludeBundles.Count) * -1);

            foreach (ResoucresBean res in resList)
                res.LoadRawAsset();

            string title = string.Format("<color=white>[AssetBundle]<color=green>{0}</color></color>", curBundle.BundleName);

            ABMainChecker.MainChecker.SetCurrentView(ABMainChecker.EView.BundleDetailView , title);
        }
    }
}