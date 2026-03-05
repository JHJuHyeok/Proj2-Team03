using UnityEngine;
using TMPro;
using System;

/// <summary>
/// 갱신이 필요한 재화 텍스트에 부착할 스크립트
/// </summary>
public class UI_CurrencyText : MonoBehaviour
{
    [SerializeField] private CurrencyType _targetType;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private string _format = "{0:0,00#}";     // 숫자 포맷 설정

    private void OnEnable()
    {
        // 이벤트 구독 : 재화 변경 시마다 갱신
        CurrencyManager.Instance.OnCurrencyChanged += Refresh;
        // 초기값 설정
        Refresh(_targetType, CurrencyManager.Instance.GetAmount(_targetType));
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= Refresh;
    }

    private void Refresh(CurrencyType type, double amount)
    {
        if (type != _targetType) return;
        _amountText.text = string.Format(_format, amount);
    }
}
