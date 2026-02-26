using System;
using UnityEngine;
using UnityEngine.UI;

public class UICheckBox : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button checkBtn;      // 클릭 영역(버튼)
    [SerializeField] private GameObject onCheckBtn; // 체크 표시(ON 이미지/오브젝트)

    [Header("Option Key (Optional Save)")]
    [Tooltip("비우면 저장 안 함. 예) option_auto_summon")]
    [SerializeField] private string prefsKey = "option_auto_summon";

    [Header("Default")]
    [SerializeField] private bool defaultOn = false;

    // 현재 상태
    public bool IsOn { get; private set; }

    // 값 변경 이벤트 (옵션 매니저에서 구독 가능)
    public event Action<bool> OnValueChanged;

    private void Awake()
    {
        // 1) 저장값 있으면 불러오고, 없으면 defaultOn
        bool initial = defaultOn;

        if (!string.IsNullOrEmpty(prefsKey) && PlayerPrefs.HasKey(prefsKey))
        {
            initial = PlayerPrefs.GetInt(prefsKey, defaultOn ? 1 : 0) == 1;
        }

        Set(initial, invokeEvent: false, save: false);
    }

    private void OnEnable()
    {
        if (checkBtn != null)
            checkBtn.onClick.AddListener(Toggle);
    }

    private void OnDisable()
    {
        if (checkBtn != null)
            checkBtn.onClick.RemoveListener(Toggle);
    }

    public void Toggle()
    {
        Set(!IsOn);
    }


    // 체크박스 상태를 강제로 설정
    public void Set(bool on, bool invokeEvent = true, bool save = true)
    {
        IsOn = on;

        if (onCheckBtn != null)
            onCheckBtn.SetActive(IsOn);

        if (save && !string.IsNullOrEmpty(prefsKey))
        {
            PlayerPrefs.SetInt(prefsKey, IsOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        if (invokeEvent)
            OnValueChanged?.Invoke(IsOn);
    }
}