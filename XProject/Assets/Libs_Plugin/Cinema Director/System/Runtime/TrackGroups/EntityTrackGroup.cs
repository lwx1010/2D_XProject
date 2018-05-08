using System;
using LuaFramework;
using RSG;
using UnityEngine;
using Riverlake.Resources;

namespace CinemaDirector
{
    /// <summary>
    /// The EntityTrackGroup maintains an Entity and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("Entity Track Group", TimelineTrackGenre.ActorTrack)]
    public class EntityTrackGroup : ActorTrackGroup
    {
        /// <summary>
        /// 是否是玩家
        /// </summary>
        public bool Self;
        /// <summary>
        /// 是否显示武器
        /// </summary>
        public bool IsWeapon = true;

        /// <summary>
        /// 主模型资源名
        /// </summary>
        public string Model;
        /// <summary>
        /// 翅膀
        /// </summary>
        public string Wings;
        /// <summary>
        /// 武器
        /// </summary>
        public string Weapon;
        /// <summary>
        /// 武器的位置
        /// </summary>
        public Vector3 WeaponPos;
        /// <summary>
        /// 武器旋转
        /// </summary>
        public Vector3 WeaponeRotation;
        /// <summary>
        /// 坐骑
        /// </summary>
        public string Horse;

        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public override Transform Actor
        {
            get
            {
                if (actor == null)
                    actor = transform.Find("_Entity");
                return actor;
            }
            set { actor = value; }
        }

        public override void Optimize()
        {
            base.Optimize();

            if (Self)   return;

            if (string.IsNullOrEmpty(Model))
            {
                Debug.LogError("请设置主模型的资源名！结点：" + this.gameObject);
                return;
            }

            actor = this.transform.Find("_Entity");
            if (actor == null)
            {
                Transform entity = new GameObject("_Entity").transform;
                entity.SetParent(this.transform);
                actor = entity;
            }

            this.loadEntityAssets();
        }


        private void loadEntityAssets()
        {
            //ResourceManager resMgr = AppFacade.Instance.GetManager<ResourceManager>();

            new Promise<GameObject>((s, j) =>
            {
                GameObject modelGO = ResourceManager.LoadPrefab(string.Format("Prefab/{0}", Model));
                modelGO = GameObject.Instantiate(modelGO);

                Animator modelAnimator = modelGO.GetComponentInChildren<Animator>();
                if (modelAnimator) modelAnimator.enabled = true;
                ////Util.SetParent(modelGO, Actor.gameObject);

                s.Invoke(modelGO);
            }).Then((go) =>
            {
                if (!string.IsNullOrEmpty(Wings) && Wings != "0")
                {
                    GameObject wingeGO = ResourceManager.LoadPrefab(string.Format("Prefab/Model/wings/{0}", Wings));
                    wingeGO = GameObject.Instantiate(wingeGO);
                    Animator animator = wingeGO.GetComponentInChildren<Animator>();
                    animator.enabled = true;
                    ////Util.SetParent(wingeGO, Util.Find(go.transform, "chibang01").gameObject);
                    wingeGO.transform.localRotation = Quaternion.Euler(0, -90, 0);
                }
                //                Debug.Log("Load Wings ~" + Convert.ToString(Wings) + ",isSelf:" + Self);
            }).Then((go) =>
            {
                if (!string.IsNullOrEmpty(Weapon) && IsWeapon)
                {
                    GameObject weaponGO = ResourceManager.LoadPrefab(string.Format("Prefab/Model/weapon/{0}", Weapon));
                    weaponGO = GameObject.Instantiate(weaponGO);
                    ////Util.SetParent(weaponGO, Util.Find(go.transform, "wuqi01").gameObject);
                    weaponGO.transform.localPosition = WeaponPos;
                    weaponGO.transform.localRotation = Quaternion.Euler(WeaponeRotation);

                    PrefabLoader prefabLoader = weaponGO.GetComponentInChildren<PrefabLoader>();
                    prefabLoader.SetEffectLayer("Plot");
                }
                //                Debug.Log("Load Weapon ~" + Convert.ToString(Weapon) + ",isSelf:" + Self);
            }).Catch((e) =>
            {
                Debug.LogException(e);
            }).Done(go =>
            {
                
            });
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Self && Application.isPlaying)
            {
                //SceneEntity mainRole = RoleManager.GetInstance().mainRole;
                //if (mainRole == null) return;
                
                //Model = string.Format("Story/player/{0}_01", mainRole.sex == 1 ? "100001" : "100101");
                //Weapon = mainRole.weapon == 0 ? null : mainRole.weapon.ToString();
                //if (mainRole.weapon_model != null)
                //{
                //    WeaponPos = mainRole.weapon_model.transform.localPosition;
                //    WeaponeRotation = mainRole.weapon_model.transform.localRotation.eulerAngles;
                //}
                //Wings = mainRole.shenyi == 0 ? null : mainRole.shenyi.ToString();
                
                //this.loadEntityAssets();
            }
        }
    }
}