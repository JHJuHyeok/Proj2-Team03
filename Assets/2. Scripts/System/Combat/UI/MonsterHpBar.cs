using UnityEngine;

/// <summary>
/// 몬스터의 HP를 SpriteRenderer 기반으로 발밑에 표시하는 컴포넌트
/// MonsterBase의 HP 비율(HPRatio)을 매 프레임 추적하여 바를 업데이트
///
/// 오브젝트 계층 구조 (몬스터 프리팹 내부):
///   Monster
///   └── HpBar (이 스크립트)
///       ├── HpBar_BG   (SpriteRenderer, 어두운 배경)
///       └── HpBar_Fill  (SpriteRenderer, 빨강/초록)
/// </summary>
public class UI_MonsterHpBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hpFillTransform;    // X 스케일로 비율 표현

    private MonsterBase _monster;
    private Vector3 _originalScale;
    private SpriteRenderer[] _barRenderers;                // 바 전체 표시/숨김용

    private void Awake()
    {
        if (hpFillTransform != null)
            _originalScale = hpFillTransform.localScale;

        _barRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void OnEnable()
    {
        // 부모에서 MonsterBase 자동 탐색
        _monster = GetComponentInParent<MonsterBase>();

    }

    private void Update()
    {
        if (_monster == null) return;

        float ratio = Mathf.Clamp01((float)_monster.HPRatio);

        // Fill 스케일 업데이트
        if (hpFillTransform != null)
        {
            hpFillTransform.localScale = new Vector3(
                _originalScale.x * ratio,
                _originalScale.y,
                _originalScale.z
            );
        }
    }

    private void SetBarVisible(bool visible)
    {
        if (_barRenderers == null) return;

        foreach (var r in _barRenderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }
}
