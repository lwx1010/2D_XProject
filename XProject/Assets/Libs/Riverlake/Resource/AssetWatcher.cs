using UnityEngine;
using System.Collections;

namespace Riverlake
{
    public sealed class AssetWatcher : MonoBehaviour
    {

        public string assetName;

        void OnEnable()
        {
            AssetBundleManager.Instance.LoadAsset(assetName, gameObject);
        }

        void OnDestroy()
        {
            if (AssetBundleManager.Instance != null)
                AssetBundleManager.Instance.UnloadAsset(assetName, gameObject);
        }
    }
}
