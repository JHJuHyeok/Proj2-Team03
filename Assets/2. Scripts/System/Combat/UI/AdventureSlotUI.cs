using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum AdventureSlotState { Available, Cleared, Locked }

public class AdventureSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questNumberText;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private Button slotButton;

    private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
    private static readonly Color TintedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private Image _slotImage;
    private bool _clickable = true;
    private StageData stageData;

    private void Awake()
    {
        _slotImage = GetComponentInChildren<Image>();
    }

    public void SetData(StageData data, int index)
    {
        stageData = data;

        if (questNumberText != null)
        {
            questNumberText.text = index >= 0 && index < RomanNumerals.Length
                ? RomanNumerals[index]
                : (index + 1).ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

    public void SetSlotState(AdventureSlotState state)
    {
        _clickable = (state == AdventureSlotState.Available);

        if (_slotImage != null)
            _slotImage.color = (state == AdventureSlotState.Available) ? Color.white : TintedColor;

        if (questNumberText != null)
            questNumberText.gameObject.SetActive(state != AdventureSlotState.Locked);
    }

    public void SetOnClickAction(Action action)
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => { if (_clickable) action?.Invoke(); });
        }
    }

    public StageData GetStageData()
    {
        return stageData;
    }
}
