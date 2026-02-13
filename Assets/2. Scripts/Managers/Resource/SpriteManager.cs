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

    // Dictionary<ȣ�� ��Ʋ�� ��Ī, ��Ʋ��>
    private static Dictionary<string, SpriteAtlas> _atlasCache = new();

    /// <summary>
    /// �ε�� ��Ʋ�󽺿��� ��������Ʈ Ž��
    /// </summary>
    /// <param name="atlasAddress"> ��Ʋ�� �ּ� </param>
    /// <param name="spriteName"> ��������Ʈ ��Ī </param>
    /// <returns></returns>
    public static async Task<Sprite> GetSprite(string atlasAddress, string spriteName)
    {
        // �̹� �ε�� ��Ʋ�󽺰� �ִ��� Ȯ��
        if (!_atlasCache.TryGetValue(atlasAddress, out SpriteAtlas atlas))
        {
            // ���ٸ� ��巹������ ��Ʋ�� �ε�
            AsyncOperationHandle<SpriteAtlas> handle = Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                atlas = handle.Result;
                _atlasCache[atlasAddress] = atlas;
            }
            else
            {
                Debug.Log("��Ʋ�� �ε� ����");
                return null;
            }
        }

        Sprite targetSprite = atlas.GetSprite(spriteName);

        if (targetSprite == null)
        {
            Debug.Log("��Ʋ�󽺿� �ش��ϴ� ��������Ʈ�� �����ϴ�.");
        }

        return targetSprite;
    }

    public static async Task LoadAllAtlasAsync()
    {
        // 1. ���� ���� �� ��Ʋ�� ���� �ҷ�����
        AsyncOperationHandle<IList<SpriteAtlas>> handle = 
            Addressables.LoadAssetsAsync<SpriteAtlas>("Atlas", null);

        await handle.Task;

        // 2. ��ųʸ��� ȣ���� ��Ʋ�󽺵� ����
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
