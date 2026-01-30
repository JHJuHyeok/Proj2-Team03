using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


// 여러 오브젝트 풀을 생성하고 관리하는 싱글톤 매니저 클래스
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, object> _pools = new Dictionary<string, object>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePool<T>(T prefab, int initCount, Transform parent = null) where T : MonoBehaviour
    {
        if (prefab == null) return;

        string key = prefab.name;
        if (_pools.ContainsKey(key)) return;
        _pools.Add(key, new ObjectPool<T>(prefab, initCount, parent));
    }

    public T GetFromPool<T>(T prefab) where T : MonoBehaviour
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab.name, out var box))
        {
            return null;
        }

        var pool = box as ObjectPool<T>;

        if (pool != null) return pool.Dequeue();
        else return null;

    }

    public void ReturnPool<T>(T instance) where T : MonoBehaviour
    {
        if (instance == null) return;
        if (!_pools.TryGetValue(instance.gameObject.name, out var box))
        {
            Destroy(instance.gameObject);
            return;
        }

        var pool = box as ObjectPool<T>;
        if (pool != null) pool.Enqueue(instance);
        else return;
    }

    public void ClearPool()
    {
        _pools.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("모든 풀이 초기화 되었습니다.");
    }
}