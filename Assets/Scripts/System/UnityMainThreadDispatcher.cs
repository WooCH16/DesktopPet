using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// STA 스레드(트레이 스레드 등)에서 Unity 메인 스레드 작업을 예약한다.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static readonly object _lock = new object();
    private static UnityMainThreadDispatcher _instance;

    public static void Enqueue(Action action)
    {
        lock (_lock)
            _queue.Enqueue(action);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }
}
