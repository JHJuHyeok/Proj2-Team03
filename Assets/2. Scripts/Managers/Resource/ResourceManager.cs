using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Resource
{
    /// <summary>
    /// 스킬 아이콘, 프리팹 등의 리소스를 로드하고 캐싱하는 매니저
    /// 팀원의 JSON 데이터(spriteName)를 Unity 리소스(Sprite)로 변환
    /// </summary>
    public class ResourceManager
    {
        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ResourceManager();
                return _instance;
            }
        }

        // 리소스 캐시
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// 스프라이트 이름으로 스프라이트 로드
        /// 경로: Resources/Skills/Icons/{spriteName}
        /// </summary>
        public Sprite LoadSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Debug.LogWarning("[ResourceManager] spriteName이 null 또는 비어있음");
                return null;
            }

            if (_spriteCache.TryGetValue(spriteName, out var cachedSprite))
                return cachedSprite;

            // 여러 경로 시도
            string[] paths = {
                $"Skill/skillicon/skill_fire/{spriteName}",
                $"Skill/skillicon/skill_water/{spriteName}",
                $"Skill/skillicon/skill_earth/{spriteName}",
                $"Skill/skillicon/skill_wind/{spriteName}",
                $"Skill/skillicon/skill_none/{spriteName}",
                $"Skill/skillicon/{spriteName}",
                $"Skills/Icons/{spriteName}",
                $"Slayer Legend/Image/icon/{spriteName}",
                $"Sprites/{spriteName}",
                // 조민희 수정 - 장비 아이콘 로드 경로 추가
                $"Slayer Legend/Bookmark UI/Equip UI/Weapon/{spriteName}",      // 무기 아이콘
                $"Slayer Legend/Bookmark UI/Equip UI/Accessory/{spriteName}",   // 악세서리 아이콘
                // 조민희 수정 끝
                spriteName
            };

            foreach (string path in paths)
            {
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    _spriteCache[spriteName] = sprite;
                    Debug.Log($"[ResourceManager] 스프라이트 로드 성공: {spriteName} → {path}");
                    return sprite;
                }
            }

            // 조민희 추가 - AssetBundleLoader에서 스프라이트 로드 시도
            var bundleSprite = AssetBundleLoader.Instance.LoadSpriteFromBundle(spriteName, "skin");
            if (bundleSprite != null && bundleSprite.texture != null)
            {
                _spriteCache[spriteName] = bundleSprite;
                Debug.Log($"[ResourceManager] AssetBundleLoader에서 스프라이트 로드 성공: {spriteName}, texture={bundleSprite.texture.name}");
                return bundleSprite;
            }
            else if (bundleSprite != null && bundleSprite.texture == null)
            {
                Debug.LogWarning($"[ResourceManager] 스프라이트 로드됨 but 텍스처 null: {spriteName}");
            }
            else if (bundleSprite == null)
            {
                Debug.LogWarning($"[ResourceManager] AssetBundleLoader에서 스프라이트 찾기 실패: {spriteName}");
            }
            // 조민희 추가 끝

            Debug.LogWarning($"[ResourceManager] Sprite not found: {spriteName}");
            return null;
        }

        /// <summary>
        /// 경로로 프리팹 로드
        /// </summary>
        public GameObject LoadPrefab(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath))
                return null;

            if (_prefabCache.TryGetValue(prefabPath, out var cachedPrefab))
                return cachedPrefab;

            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                _prefabCache[prefabPath] = prefab;
                return prefab;
            }

            Debug.LogWarning($"Prefab not found: {prefabPath}");
            return null;
        }

        /// <summary>
        /// 캐시 비우기 (메모리 정리)
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
            _prefabCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
