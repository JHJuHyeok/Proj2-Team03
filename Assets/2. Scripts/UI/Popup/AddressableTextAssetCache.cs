using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/*
AddressableTextAssetCache
-Addressables Address(키)로 TextAsset(JSON)을 로드하고 캐싱하는 유틸
-같은 키 재요청 시 즉시 반환
-완료 콜백 기반이라 async/await 없이 사용 가능
*/
public static class AddressableTextAssetCache
{
    private static readonly Dictionary<string, TextAsset> cache = new Dictionary<string, TextAsset>(64);
    private static readonly Dictionary<string, AsyncOperationHandle<TextAsset>> handles = new Dictionary<string, AsyncOperationHandle<TextAsset>>(64);

    //TextAsset을 Address(키)로 요청
    public static void Load(string address, Action<TextAsset> onLoaded)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[AddressableTextAssetCache] address is null or empty.");
            onLoaded?.Invoke(null);
            return;
        }

        if (cache.TryGetValue(address, out TextAsset cached))
        {
            onLoaded?.Invoke(cached);
            return;
        }

        if (handles.TryGetValue(address, out AsyncOperationHandle<TextAsset> running))
        {
            if (running.IsDone)
            {
                TextAsset ta = running.Status == AsyncOperationStatus.Succeeded ? running.Result : null;
                if (ta != null)
                {
                    cache[address] = ta;
                }
                onLoaded?.Invoke(ta);
            }
            else
            {
                running.Completed += h =>
                {
                    TextAsset ta = h.Status == AsyncOperationStatus.Succeeded ? h.Result : null;
                    if (ta != null)
                    {
                        cache[address] = ta;
                    }
                    onLoaded?.Invoke(ta);
                };
            }
            return;
        }

        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);
        handles[address] = handle;

        handle.Completed += h =>
        {
            TextAsset ta = h.Status == AsyncOperationStatus.Succeeded ? h.Result : null;
            if (ta != null)
            {
                cache[address] = ta;
            }
            onLoaded?.Invoke(ta);
        };
    }

    //필요하면 전체 캐시를 비울 수 있음
    public static void Clear()
    {
        cache.Clear();
        handles.Clear();
    }
}