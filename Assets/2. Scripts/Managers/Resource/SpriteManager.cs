using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using System.Threading.Tasks;

// [신태환] - AtlasBase 추가
public static class SpriteManager
{
    public const string AtlasBase = "Assets/Resource/Atlas/";

    // Dictionary<??? ????? ???, ?????>
    private static Dictionary<string, SpriteAtlas> _atlasCache = new();

    /// <summary>
    /// ?ε?? ???????? ????????? ???
    /// </summary>
    /// <param name="atlasAddress"> ????? ??? </param>
    /// <param name="spriteName"> ????????? ??? </param>
    /// <returns></returns>
    public static async Task<Sprite> GetSprite(string atlasAddress, string spriteName)
    {
        // ??? ?ε?? ?????? ????? ???
        if (!_atlasCache.TryGetValue(atlasAddress, out SpriteAtlas atlas))
        {
            // ????? ????????? ????? ?ε?
            AsyncOperationHandle<SpriteAtlas> handle = Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                atlas = handle.Result;
                _atlasCache[atlasAddress] = atlas;
            }
            else
            {
                return null;
            }
        }

        Sprite targetSprite = atlas.GetSprite(spriteName);

        return targetSprite;
    }

    public static async Task LoadAllAtlasAsync()
    {
        // 1. ???? ???? ?? ????? ???? ???????
        AsyncOperationHandle<IList<SpriteAtlas>> handle = 
            Addressables.LoadAssetsAsync<SpriteAtlas>("Atlas", null);

        await handle.Task;

        // 2. ??????? ????? ?????? ????
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _atlasCache.Clear();

            foreach (var atlas in handle.Result)
            {
                if (!_atlasCache.ContainsKey(atlas.name))
                {
                    _atlasCache.Add(atlas.name, atlas);
                }
            }
        }
    }
}
