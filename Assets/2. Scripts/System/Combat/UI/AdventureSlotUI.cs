using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AdventureSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questNumberText;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private Button slotButton;

    private StageData stageData;
    private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

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

    public void SetOnClickAction(Action action)
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => action?.Invoke());
        }
    }

    public StageData GetStageData()
    {
        return stageData;
    }
}
