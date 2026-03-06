using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SlayerLegend.Resource
{
    /// <summary>
    /// AssetBundle 로딩 시스템
    /// 작성자: 조민희
    /// 에디터 모드에서는 아틀라스 텍스처에서 스프라이트 추출
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
        private static AssetBundleLoader _instance;
        public static AssetBundleLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[AssetBundleLoader]");
                    _instance = go.AddComponent<AssetBundleLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 스프라이트 캐시
        private readonly Dictionary<string, Sprite> spriteCache = new();
        // 텍스처가 null인 스프라이트들의 원본 정보 (아틀라스에서 재생성용)
        private readonly Dictionary<string, Sprite> nullTextureSprites = new();
        // 아틀라스 텍스처 캐시
        private Texture2D atlasTexture;
        // 초기화 여부
        private bool isInitialized = false;

        // 아틀라스 텍스처 경로들 (sword_00~sword_23이 참조하는 텍스처)
        // GUID: f9156a63d0d922643bdb5bc364d243b3
        private static readonly string[] AtlasTexturePaths = new string[]
        {
            // 무기/악세서리 아이콘이 포함된 아틀라스 (경로 수정됨)
            "Assets/Resource/Slayer Legend/Image/Texture2D/sactx-0-2048x2048-ASTC 5x5-___Atlas_BasicPixelUIs-5147ba61_0.png",
            "Assets/Resource/Slayer Legend/Image/Texture2D/sactx-0-2048x2048-ASTC 5x5-___Atlas_BasicPixelUIs-5147ba61.png",
            "Assets/Resource/Slayer Legend/Image/Texture2D/sactx-0-2048x2048-ASTC 5x5-Atlas_Skill_FirePillar-329c929b.png",
            "Assets/Resource/Slayer Legend/Image/Texture2D/sactx-0-2048x2048-ETC2-___Atlas_BasicPixelUIs-5147ba61.png",
        };

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

#if UNITY_EDITOR
            Debug.Log("[AssetBundleLoader] 에디터 모드: 아틀라스에서 스프라이트 추출");
            LoadAllSpritesFromAtlasTextures();
#else
            Debug.Log("[AssetBundleLoader] 빌드 모드: AssetBundle 사용");
            LoadBundle("skin");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 모든 아틀라스 텍스처에서 스프라이트 로드
        /// </summary>
        private void LoadAllSpritesFromAtlasTextures()
        {
            int totalSprites = 0;

            foreach (string atlasPath in AtlasTexturePaths)
            {
                // 텍스처와 모든 서브 에셋 로드
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);

                if (allAssets == null || allAssets.Length == 0)
                {
                    Debug.LogWarning($"[AssetBundleLoader] 아틀라스 로드 실패: {atlasPath}");
                    continue;
                }

                Debug.Log($"[AssetBundleLoader] 아틀라스 로드: {atlasPath}, 에셋 수: {allAssets.Length}");

                foreach (Object asset in allAssets)
                {
                    if (asset is Sprite sprite && sprite.texture != null)
                    {
                        string spriteName = sprite.name;
                        if (!string.IsNullOrEmpty(spriteName) && !spriteCache.ContainsKey(spriteName))
                        {
                            spriteCache[spriteName] = sprite;
                            totalSprites++;
                        }
                    }
                }
            }

            // 추가로 직접 스프라이트 에셋 파일들도 로드
            LoadDirectSpriteAssets();

            Debug.Log($"[AssetBundleLoader] 아틀라스에서 {totalSprites}개 스프라이트 추출 완료, 총 캐시: {spriteCache.Count}");
        }

        /// <summary>
        /// 직접 스프라이트 에셋 파일 로드 (무기 아이콘용)
        /// </summary>
        private void LoadDirectSpriteAssets()
        {
            // 먼저 아틀라스 텍스처를 로드하여 참조 해결 준비
            foreach (string atlasPath in AtlasTexturePaths)
            {
                atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                if (atlasTexture != null)
                {
                    Debug.Log($"[AssetBundleLoader] 아틀라스 텍스처 미리 로드: {atlasPath}");
                    break;
                }
            }

            string weaponPath = "Assets/Resource/Slayer Legend/Bookmark UI/Equip UI/Weapon";
            string accessoryPath = "Assets/Resource/Slayer Legend/Bookmark UI/Equip UI/Accessory";

            LoadSpritesFromDirectory(weaponPath);
            LoadSpritesFromDirectory(accessoryPath);
        }

        /// <summary>
        /// 디렉토리에서 스프라이트 로드
        /// </summary>
        private void LoadSpritesFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"[AssetBundleLoader] 디렉토리 없음: {directoryPath}");
                return;
            }

            string[] assetGuids = AssetDatabase.FindAssets("a:assets", new[] { directoryPath });

            // 분리: .asset 파일과 PNG 파일 목록
            var assetFiles = new System.Collections.Generic.List<string>();
            var imageFiles = new System.Collections.Generic.List<string>();

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".asset"))
                {
                    assetFiles.Add(assetPath);
                }
                else if (!assetPath.EndsWith(".meta"))
                {
                    imageFiles.Add(assetPath);
                }
            }

            int assetCount = 0;
            int recreatedCount = 0;
            int skippedCount = 0;

            // 1단계: .asset 파일 먼저 처리 (아틀라스 참조 스프라이트들)
            foreach (string assetPath in assetFiles)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    // sword_00~sword_23은 WeaponList.json에서 사용하는 이름들 - 상세 로그
                    bool isImportant = sprite.name.StartsWith("sword_") &&
                                       sprite.name.Length <= 8 &&
                                       char.IsDigit(sprite.name[6]);

                    // 악세서리도 상세 로그
                    bool isAccessory = sprite.name.StartsWith("Acc_");

                    if (isImportant || isAccessory)
                    {
                        Debug.Log($"[AssetBundleLoader] 스프라이트 발견: {sprite.name}, texture={(sprite.texture != null ? sprite.texture.name : "NULL")}, inCache={spriteCache.ContainsKey(sprite.name)}");
                    }

                    if (!spriteCache.ContainsKey(sprite.name))
                    {
                        assetCount++;
                        if (sprite.texture != null)
                        {
                            // 텍스처가 있는 경우 정상적으로 캐시에 추가
                            spriteCache[sprite.name] = sprite;

                            // 악세서리의 경우 JSON 이름으로도 별칭 추가 (Acc_0XX → acce_XX 매핑)
                            // JSON: acce_00, acce_01, ... (2자리)
                            // Asset: Acc_000, Acc_001, ... (3자리)
                            if (sprite.name.StartsWith("Acc_") && sprite.name.Length == 7)
                            {
                                // Acc_000 → 000 → int 0 → "00" → acce_00
                                string numberPart = sprite.name.Substring(4); // "000", "001", ...
                                if (int.TryParse(numberPart, out int num))
                                {
                                    string jsonName = $"acce_{num:D2}"; // acce_00, acce_01, ...
                                    if (!spriteCache.ContainsKey(jsonName))
                                    {
                                        spriteCache[jsonName] = sprite;
                                        Debug.Log($"[AssetBundleLoader] 악세서리 별칭 추가: {sprite.name} → {jsonName}");
                                    }
                                }
                            }

                            if (isImportant)
                            {
                                Debug.Log($"[AssetBundleLoader] 스프라이트 로드: {sprite.name} (texture: {sprite.texture.name})");
                            }
                        }
                        else
                        {
                            // 텍스처가 null인 경우: 아틀라스에서 스프라이트 재생성 시도
                            if (atlasTexture != null)
                            {
                                Sprite recreatedSprite = Sprite.Create(
                                    atlasTexture,
                                    sprite.rect,
                                    sprite.pivot,
                                    sprite.pixelsPerUnit
                                );
                                recreatedSprite.name = sprite.name;
                                spriteCache[sprite.name] = recreatedSprite;
                                recreatedCount++;
                                Debug.Log($"[AssetBundleLoader] 아틀라스에서 스프라이트 재생성: {sprite.name} (rect: {sprite.rect})");
                            }
                            else
                            {
                                // 아틀라스 텍스처가 없으면 원본 저장 (나중에 시도)
                                nullTextureSprites[sprite.name] = sprite;
                                Debug.LogWarning($"[AssetBundleLoader] 스프라이트 로드됨 but 텍스처 null: {sprite.name} (path: {assetPath})");
                            }
                        }
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
            }

            // 2단계: PNG 파일 처리 (독립 텍스처 스프라이트들)
            foreach (string assetPath in imageFiles)
            {
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object asset in allAssets)
                {
                    if (asset is Sprite sprite && sprite.texture != null)
                    {
                        string spriteName = sprite.name;
                        if (!string.IsNullOrEmpty(spriteName) && !spriteCache.ContainsKey(spriteName))
                        {
                            spriteCache[spriteName] = sprite;
                        }
                    }
                }
            }

            Debug.Log($"[AssetBundleLoader] 디렉토리 {System.IO.Path.GetFileName(directoryPath)}: .asset {assetCount}개 로드, 재생성 {recreatedCount}개, 건너뜀 {skippedCount}개");
        }
#endif
        private void LoadBundle(string label)
        {
            // 1. 어드레서블을 통해 skin 라벨 모든 스프라이트 로드
            Addressables.LoadAssetsAsync<Sprite>(label, (sprite) =>
            {
                // 2. 각 스프라이트가 로드될 때마다 실행되는 콜백
                if (sprite != null)
                {
                    string spriteName = sprite.name;
                    if (!string.IsNullOrEmpty(spriteName) && !spriteCache.ContainsKey(spriteName))
                    {
                        spriteCache[spriteName] = sprite;
                    }
                }
            }).Completed += (handle) =>
            {
                // 3. 로드 완료 후 처리
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[AssetBundleLoader] 빌드 모드: {label} 번들 로드 완료. 총 캐시: {spriteCache.Count}");
                }
                else
                {
                    Debug.LogError($"[AssetBundleLoader] 번들 로드 실패: {label}");
                }
            };
        }

        /// <summary>
        /// 스프라이트 로드
        /// </summary>
        public Sprite LoadSpriteFromBundle(string spriteName, string bundleName = "skin")
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            // 캐시 확인
            if (spriteCache.TryGetValue(spriteName, out var cachedSprite))
            {
                if (cachedSprite.texture != null)
                {
                    return cachedSprite;
                }
                else
                {
                    Debug.LogWarning($"[AssetBundleLoader] 캐시된 스프라이트의 텍스처가 null: {spriteName}");
                    // 제거하지 않고 계속 진행 (아래에서 재생성 시도)
                }
            }

            // 악세서리 이름 매핑 시도: acce_XX → Acc_0XX
            string mappedName = GetMappedSpriteName(spriteName);
            if (mappedName != spriteName && spriteCache.TryGetValue(mappedName, out var mappedSprite))
            {
                if (mappedSprite.texture != null)
                {
                    // 원본 이름으로도 캐시에 추가 (다음 검색용)
                    spriteCache[spriteName] = mappedSprite;
                    return mappedSprite;
                }
            }

            // nullTextureSprites에 있는 경우 아틀라스에서 재생성 시도
#if UNITY_EDITOR
            if (nullTextureSprites.TryGetValue(spriteName, out var originalSprite) && atlasTexture != null)
            {
                Sprite recreatedSprite = Sprite.Create(
                    atlasTexture,
                    originalSprite.rect,
                    originalSprite.pivot,
                    originalSprite.pixelsPerUnit
                );
                recreatedSprite.name = originalSprite.name;

                // 캐시 업데이트
                spriteCache[spriteName] = recreatedSprite;
                nullTextureSprites.Remove(spriteName);

                Debug.Log($"[AssetBundleLoader] 요청 시 스프라이트 재생성: {spriteName}");
                return recreatedSprite;
            }

            // 매핑된 이름으로도 재생성 시도
            if (nullTextureSprites.TryGetValue(mappedName, out var mappedOriginalSprite) && atlasTexture != null)
            {
                Sprite recreatedSprite = Sprite.Create(
                    atlasTexture,
                    mappedOriginalSprite.rect,
                    mappedOriginalSprite.pivot,
                    mappedOriginalSprite.pixelsPerUnit
                );
                recreatedSprite.name = spriteName;

                // 원본 이름으로 캐시에 추가
                spriteCache[spriteName] = recreatedSprite;
                nullTextureSprites.Remove(mappedName);

                Debug.Log($"[AssetBundleLoader] 요청 시 스프라이트 재생성 (매핑): {spriteName} → {mappedName}");
                return recreatedSprite;
            }
#endif

            // 에디터 모드에서는 이미 초기화 시 로드됨
#if !UNITY_EDITOR
            // 빌드 모드: AssetBundle에서 로드
            var sprite = LoadSpriteFromAssetBundle(spriteName, bundleName);
            if (sprite != null)
            {
                spriteCache[spriteName] = sprite;
            }
            return sprite;
#else
            Debug.LogWarning($"[AssetBundleLoader] 스프라이트를 찾을 수 없음: {spriteName}");
            return null;
#endif
        }

        /// <summary>
        /// JSON의 spriteName을 실제 에셋 이름으로 매핑
        /// </summary>
        private string GetMappedSpriteName(string spriteName)
        {
            // 악세서리: acce_XX → Acc_0XX
            // JSON: acce_00, acce_01, ... (2자리)
            // Asset: Acc_000, Acc_001, ... (3자리)
            if (spriteName.StartsWith("acce_") && spriteName.Length == 7)
            {
                string numberPart = spriteName.Substring(5); // "00", "01", ...
                if (int.TryParse(numberPart, out int num))
                {
                    return $"Acc_{num:D3}"; // Acc_000, Acc_001, ...
                }
            }

            return spriteName;
        }

        /// <summary>
        /// 빌드 모드: AssetBundle에서 스프라이트 로드
        /// </summary>
        private Sprite LoadSpriteFromAssetBundle(string spriteName, string bundleName)
        {
            // AssetBundle 로드 로직 (빌드 시 사용)
            // TODO: 빌드 시 구현
            return null;
        }

        /// <summary>
        /// 캐시 클리어
        /// </summary>
        public void ClearCache()
        {
            spriteCache.Clear();
            nullTextureSprites.Clear();
            atlasTexture = null;
        }

        private void OnDestroy()
        {
            ClearCache();
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
    }
}
