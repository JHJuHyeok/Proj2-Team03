using UnityEngine;

/// <summary>
/// 플레이어의 HP/Mana를 SpriteRenderer 기반으로 발밑에 표시하는 컴포넌트
/// PlayerCombatStats의 이벤트를 구독하여 실시간 업데이트
///
/// 사용법:
///   1. 플레이어 오브젝트 하위에 빈 오브젝트 "HpManaBar"를 만든다 (위치: 발밑)
///   2. 이 스크립트를 "HpManaBar"에 부착한다
///   3. 자식으로 HP/Mana Fill용 SpriteRenderer를 배치한다
///   4. Inspector에서 PlayerCombatStats와 Fill Transform들을 할당한다
///
/// 오브젝트 계층 구조:
///   Player
///   └── HpManaBar (이 스크립트)
///       ├── HpBar_BG   (SpriteRenderer, 배경)
///       ├── HpBar_Fill  (SpriteRenderer, 초록/빨강)
///       ├── ManaBar_BG  (SpriteRenderer, 배경)
///       └── ManaBar_Fill (SpriteRenderer, 파랑)
/// </summary>
public class UI_PlayerHpMana : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerCombatStats playerStats;

    [Header("HP Bar")]
    [SerializeField] private Transform hpFillTransform;      // X 스케일로 비율 표현

    [Header("Mana Bar")]
    [SerializeField] private Transform manaFillTransform;    // X 스케일로 비율 표현

    private Vector3 _hpOriginalScale;
    private Vector3 _manaOriginalScale;

    private void Awake()
    {
        // 원래 스케일 저장 (비율 계산의 기준)
        if (hpFillTransform != null)
            _hpOriginalScale = hpFillTransform.localScale;

        if (manaFillTransform != null)
            _manaOriginalScale = manaFillTransform.localScale;
    }

    private void OnEnable()
    {
        // 자동으로 부모에서 찾기
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerCombatStats>();

        if (playerStats != null)
        {
            playerStats.OnHpChanged += UpdateHpBar;
            playerStats.OnManaChanged += UpdateManaBar;

            // 초기값 반영
            UpdateHpBar(playerStats.CurrentHealth, playerStats.MaxHealth);
            UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
        }
        else
        {
            Debug.LogWarning("[UI_PlayerHpMana] PlayerCombatStats를 찾을 수 없습니다.");
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHpChanged -= UpdateHpBar;
            playerStats.OnManaChanged -= UpdateManaBar;
        }
    }
    

    private void UpdateHpBar(double current, double max)
    {
        if (hpFillTransform == null) return;

        float ratio = max > 0 ? Mathf.Clamp01((float)(current / max)) : 0f;
        hpFillTransform.localScale = new Vector3(
            _hpOriginalScale.x * ratio,
            _hpOriginalScale.y,
            _hpOriginalScale.z
        );
    }

    private void UpdateManaBar(double current, double max)
    {
        if (manaFillTransform == null) return;

        float ratio = max > 0 ? Mathf.Clamp01((float)(current / max)) : 0f;
        manaFillTransform.localScale = new Vector3(
            _manaOriginalScale.x * ratio,
            _manaOriginalScale.y,
            _manaOriginalScale.z
        );
    }
}
