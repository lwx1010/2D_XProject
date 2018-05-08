using UnityEngine;
using System.Collections;

namespace Riverlake
{
    public class ObjectPoolDontDestroyDetector : MonoBehaviour
    {
        void OnDestroy()
        {
            if (ObjectPoolDontDestroy.IsSpawned(gameObject))
                gameObject.RemoveDontDestoySpawned();
        }
    }
}


