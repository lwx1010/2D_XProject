/********************************************************************************
** Author： LiangZG
** Email :  game.liangzg@foxmail.com
*********************************************************************************/

/// <summary>
/// 缓存清理策略
/// </summary>
public class CacheDestroy<K , V>
{
    public virtual bool CanDestroy(Node<K, V> node)
    {
        return true;
    }

    public virtual void DestroyNode(Node<K ,V> node)
    {}
}