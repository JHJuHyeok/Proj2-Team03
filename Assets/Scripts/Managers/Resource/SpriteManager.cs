using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using System.Threading.Tasks;

public class SpriteManager : Singleton<SpriteManager>
{
    // Dictionary<호출 아틀라스 명칭, 아틀라스>
    private Dictionary<string, SpriteAtlas> _atlasCache = new();

    /// <summary>
    /// 로드된 아틀라스에서 스프라이트 탐색
    /// </summary>
    /// <param name="atlasAddress"> 아틀라스 주소 </param>
    /// <param name="spriteName"> 스프라이트 명칭 </param>
    /// <returns></returns>
    public async Task<Sprite> GetSprite(string atlasAddress, string spriteName)
    {
        // 이미 로드된 아틀라스가 있는지 확인
        if (!_atlasCache.TryGetValue(atlasAddress, out SpriteAtlas atlas))
        {
            // 없다면 어드레서블로 아틀라스 로드
            AsyncOperationHandle<SpriteAtlas> handle = Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                atlas = handle.Result;
                _atlasCache[atlasAddress] = atlas;
            }
            else
            {
                Debug.Log("아틀라스 로드 실패");
                return null;
            }
        }

        Sprite targetSprite = atlas.GetSprite(spriteName);

        if (targetSprite == null)
        {
            Debug.Log("아틀라스에 해당하는 스프라이트가 없습니다.");
        }

        return targetSprite;
    }

    public async Task GetResources()
    {
        // 게임 시작 시 아틀라스 파일 불러오기
    }
}
