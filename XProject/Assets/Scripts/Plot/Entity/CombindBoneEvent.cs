using CinemaDirector;
using UnityEngine;

namespace Assets.Scripts.Plot
{
    /// <summary>
    /// 骨骼绑定
    /// </summary>
    [CutsceneItem("Common", "Combind Bone", CutsceneItemGenre.ActorItem, CutsceneItemGenre.TransformItem)]
    public class CombindBoneEvent : CinemaActorEvent
    {

        public TrackGroup Target;

        /// <summary>
        /// 绑定到当前Actor的指定骨骼点
        /// </summary>
        public string BoneName;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 OffsetPosition;
        /// <summary>
        /// 旋转
        /// </summary>
        public Vector3 OffsetRotation;



        public override void Trigger(GameObject Actor)
        {
            if (Target == null)
            {
                Debug.LogError("还没有指定被绑定的对象！！");
                return;
            }

            if (string.IsNullOrEmpty(BoneName))
            {
                Debug.LogError("请设置绑定骨骼点！");
                return;
            }

            Transform boneTrans = findChild(Actor.transform , BoneName);
            if (boneTrans == null)
            {
                Debug.LogError(string.Format("{0}找不到指定的骨骼点{1}！" , this.TimelineTrack.TrackGroup.gameObject.name , BoneName));
                return;
            }

            Transform targetActor = GetTragetActorTransform();
            targetActor.SetParent(boneTrans);
            targetActor.localPosition = OffsetPosition;
            targetActor.localRotation = Quaternion.Euler(OffsetRotation);
        }


        public Transform GetTragetActorTransform()
        {
            ActorTrackGroup targetTrackGroup = Target as ActorTrackGroup;
            if (targetTrackGroup) return targetTrackGroup.Actor;

            EntityTrackGroup entityTrackGroup = Target as EntityTrackGroup;
            if (entityTrackGroup) return entityTrackGroup.Actor;

            Debug.LogError("找不到目标对应的演员！");
            return null;
        }

        public Transform findChild(Transform trans , string childName)
        {
            Transform child = trans.Find(childName);
            if(child != null)   return child;

            for (int i = 0 , count = trans.childCount; i < count; i++)
            {
                child = findChild(trans.GetChild(i), childName);
                if(child != null)   return child;
            }
            return null;
        }
    }
}