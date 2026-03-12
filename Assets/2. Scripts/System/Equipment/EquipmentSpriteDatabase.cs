using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SlayerLegend.Resource
{
    /// <summary>
    /// 장비 스프라이트 데이터베이이스
    /// 작성자: 조민희
    /// 에디터에서 미리 스프라이트 참조를 설정하여 런타임에 사용
    /// 아틀라스 텍스처 참조 문제를 해결하기 위한 방법
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentSpriteDatabase", menuName = "Resources/Databases")]
    public class EquipmentSpriteDatabase : ScriptableObject
    {
        [System.Serializable]
        public class SpriteEntry
        {
            public string spriteName;
            public Sprite sprite;
        }

        [Header("무기 스프라이트")]
        [SerializeField] private List<SpriteEntry> weaponSprites = new List<SpriteEntry>();

        [Header("악세서리 스프라이트")]
        [SerializeField] private List<SpriteEntry> accessorySprites = new List<SpriteEntry>();

        // 런타임 조회용 딕셔너리
        private Dictionary<string, Sprite> spriteLookup;
        private bool isInitialized = false;

        private static EquipmentSpriteDatabase _instance;
        public static EquipmentSpriteDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Resources에서 로드 시도
                    _instance = Resources.Load<EquipmentSpriteDatabase>("Databases/EquipmentSpriteDatabase");
                }
                return _instance;
            }
        }

        /// <summary>
        /// 초기화 (딕셔너리 구성)
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            spriteLookup = new Dictionary<string, Sprite>();

            foreach (var entry in weaponSprites)
            {
                if (!string.IsNullOrEmpty(entry.spriteName) && entry.sprite != null)
                {
                    spriteLookup[entry.spriteName] = entry.sprite;
                }
            }

            foreach (var entry in accessorySprites)
            {
                if (!string.IsNullOrEmpty(entry.spriteName) && entry.sprite != null)
                {
                    spriteLookup[entry.spriteName] = entry.sprite;
                }
            }

            isInitialized = true;
        }

        /// <summary>
        /// 스프라이트 이름으로 스프라이트 조회
        /// </summary>
        public Sprite GetSprite(string spriteName)
        {
                if (!isInitialized)
                {
                    Initialize();
                }

                if (spriteLookup.TryGetValue(spriteName, out var sprite))
                {
                    return sprite;
                }

                return null;
        }

        /// <summary>
        /// 스프라이트가 등록되어 있는지 확인
        /// </summary>
        public bool HasSprite(string spriteName)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            return spriteLookup.ContainsKey(spriteName);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 전용: 무기 스프라이트 자동 추가
        /// </summary>
        [ContextMenu("무기 스프라이트 자동 추가")]
        private void AutoAddWeaponSprites()
        {
                weaponSprites.Clear();

                // sword_00 ~ sword_23 자동 추가
                string[] spriteGuids = UnityEditor.AssetDatabase.FindAssets("sword_ t:Sprite", new[] { "Assets/Resource/Slayer Legend/Bookmark UI/Equip UI/Weapon" });

                foreach (string guid in spriteGuids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

                    if (sprite != null)
                    {
                        string spriteName = System.IO.Path.GetFileNameWithoutExtension(path);
                        weaponSprites.Add(new SpriteEntry { spriteName = spriteName, sprite = sprite });
                        Debug.Log($"[EquipmentSpriteDatabase] 추가: {spriteName}");
                    }
                }

                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[EquipmentSpriteDatabase] 무기 스프라이트 {weaponSprites.Count}개 추가 완료");
            }
#endif
    }
}
