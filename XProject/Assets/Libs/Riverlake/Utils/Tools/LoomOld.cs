using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
#if NETFX_CORE && UNITY_METRO && !UNITY_EDITOR
using System.Threading.Tasks;
#endif
using System.Linq;

public class LoomOld : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    private static LoomOld _current;
    private int _count;
    public static LoomOld Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
        initialized = true;
    }

    static bool initialized;

    static public void Initialize()
    {
        if (!initialized)
        {

            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("LoomOld");
            _current = g.AddComponent<LoomOld>();
            GameObject.DontDestroyOnLoad(g);
        }

    }

    private List<Action> _actions = new List<Action>();
    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }
    public static void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }

#if NETFX_CORE
    public static async void RunAsync(Action a)
#else
    public static void RunAsync(Action a)
#endif

    {
        Initialize();
        while (numThreads >= maxThreads)
        {
#if NETFX_CORE
            await Task.Delay(TimeSpan.FromMilliseconds(1));
#else
            Thread.Sleep(1);
#endif
        }
        Interlocked.Increment(ref numThreads);
#if NETFX_CORE
        Task.Factory.StartNew(() => RunAction(a));
#else
        ThreadPool.QueueUserWorkItem(RunAction, a);
#endif
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }

    void OnDisable()
    {
        if (_current == this)
        {

            _current = null;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    List<Action> _currentActions = new List<Action>();

    // Update is called once per frame
    void Update()
    {
        lock (_actions)
        {
            if (_actions.Count > 0)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }
        }
        for (int i = 0; i < _currentActions.Count; ++i)
        {
            _currentActions[i]();
        }
        _currentActions.Clear();
        lock (_delayed)
        {
            if (_delayed.Count >  0)
            {
                _currentDelayed.Clear();
                _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                for (int i = 0; i < _currentDelayed.Count; ++i)
                    _delayed.Remove(_currentDelayed[i]);
            }
        }
        for (int i = 0; i < _currentDelayed.Count; ++i)
        {
            _currentDelayed[i].action();
        }
        _currentDelayed.Clear();
    }
}
