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
        /// </summary>
        public Sprite LoadSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Debug.LogWarning("[ResourceManager] spriteName이 null 또는 비어있음");
                return null;
            }

            // ★ 최소 수정: 공백 제거
            string normalizedName = spriteName.Trim();

            if (_spriteCache.TryGetValue(normalizedName, out var cachedSprite))
                return cachedSprite;

            // 여러 경로 시도
            string[] paths = {
                $"Skill/skillicon/skill_fire/{normalizedName}",
                $"Skill/skillicon/skill_water/{normalizedName}",
                $"Skill/skillicon/skill_earth/{normalizedName}",
                $"Skill/skillicon/skill_wind/{normalizedName}",
                $"Skill/skillicon/skill_none/{normalizedName}",
                $"Skill/skillicon/{normalizedName}",
                $"Skills/Icons/{normalizedName}",
                $"Slayer Legend/Image/icon/{normalizedName}",
                $"Sprites/{normalizedName}",

                // 조민희 수정 - 장비 아이콘 로드 경로 추가
                $"Slayer Legend/Bookmark UI/Equip UI/Weapon/{normalizedName}",
                $"Slayer Legend/Bookmark UI/Equip UI/Accessory/{normalizedName}",
                // 조민희 수정 끝

                normalizedName
            };

            foreach (string path in paths)
            {
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    _spriteCache[normalizedName] = sprite;
                    Debug.Log($"[ResourceManager] 스프라이트 로드 성공: {normalizedName} → {path}");
                    return sprite;
                }
            }

            // ★ 추가: 대소문자 / _ - 공백 보정 탐색
            var correctedSprite = TryLoadSpriteIgnoreCase(normalizedName);
            if (correctedSprite != null)
            {
                _spriteCache[normalizedName] = correctedSprite;
                Debug.LogWarning($"[ResourceManager] 자동보정 로드 성공: {normalizedName} -> {correctedSprite.name}");
                return correctedSprite;
            }

            // 조민희 추가 - AssetBundleLoader에서 스프라이트 로드 시도
            var bundleSprite = AssetBundleLoader.Instance.LoadSpriteFromBundle(normalizedName, "skin");
            if (bundleSprite != null && bundleSprite.texture != null)
            {
                _spriteCache[normalizedName] = bundleSprite;
                Debug.Log($"[ResourceManager] AssetBundleLoader에서 스프라이트 로드 성공: {normalizedName}, texture={bundleSprite.texture.name}");
                return bundleSprite;
            }
            else if (bundleSprite != null && bundleSprite.texture == null)
            {
                Debug.LogWarning($"[ResourceManager] 스프라이트 로드됨 but 텍스처 null: {normalizedName}");
            }
            else if (bundleSprite == null)
            {
                Debug.LogWarning($"[ResourceManager] AssetBundleLoader에서 스프라이트 찾기 실패: {normalizedName}");
            }
            // 조민희 추가 끝

            Debug.LogWarning($"[ResourceManager] Sprite not found: {normalizedName}");
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

        // ------------------------
        // 자동보정 함수
        // ------------------------

        private Sprite TryLoadSpriteIgnoreCase(string spriteName)
        {
            string target = NormalizeForCompare(spriteName);

            string[] folders = {
                "Skill/skillicon/skill_fire",
                "Skill/skillicon/skill_water",
                "Skill/skillicon/skill_earth",
                "Skill/skillicon/skill_wind",
                "Skill/skillicon/skill_none",
                "Skill/skillicon",
                "Skills/Icons",
                "Slayer Legend/Image/icon",
                "Sprites",
                "Slayer Legend/Bookmark UI/Equip UI/Weapon",
                "Slayer Legend/Bookmark UI/Equip UI/Accessory"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>(folders[i]);

                if (sprites == null || sprites.Length == 0)
                    continue;

                for (int j = 0; j < sprites.Length; j++)
                {
                    if (sprites[j] == null) continue;

                    if (NormalizeForCompare(sprites[j].name) == target)
                    {
                        return sprites[j];
                    }
                }
            }

            return null;
        }

        private string NormalizeForCompare(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Trim()
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .ToLower();
        }
    }
}