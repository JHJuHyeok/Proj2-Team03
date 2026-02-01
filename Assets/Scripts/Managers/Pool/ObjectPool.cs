using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 특정 컴포넌트(T)의 오브젝트 풀을 관리하는 클래스
public class ObjectPool<T> where T : MonoBehaviour
{
    private T _prefab;
    private Queue<T> _pool = new Queue<T>();

    public Transform Root { get; private set; }

    public ObjectPool(T prefab, int initCount, Transform parent = null)
    {
        _prefab = prefab;
        if (parent != null)
        {
            Root = parent;
        }
        else
        {
            Root = new GameObject($"{prefab.name}_pool").transform;
        }

        Root.SetParent(parent, false);
        for (int i = 0; i < initCount; i++)
        {
            var inst = Object.Instantiate(prefab, Root);
            inst.name = prefab.name;
            inst.gameObject.SetActive(false);
            _pool.Enqueue(inst);
        }
    }

    public T Dequeue()
    {
        if (_pool.Count == 0) return null;
        var inst = _pool.Dequeue();
        inst.gameObject.SetActive(true);
        return inst;
    }

    public void Enqueue(T instance)
    {
        if (instance == null) return;
        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
    }
}