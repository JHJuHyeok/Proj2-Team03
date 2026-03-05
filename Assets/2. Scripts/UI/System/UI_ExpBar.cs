using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ExpBar : MonoBehaviour
{
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TMP_Text _expValueText;
    [SerializeField] private TMP_Text _expPercentText;

    private void OnEnable()
    {
        LevelManager.Instance.OnExpChanged += UpdateBar;
        // √ ±‚»≠
        LevelManager.Instance.NotifyExpChanged();
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnExpChanged -= UpdateBar;
    }

    private void UpdateBar(double current, double max, float ratio)
    {
        _expSlider.value = ratio;
        if (_expValueText != null)
            _expValueText.text = $"{current:F0}/{max:F0}";
        if (_expPercentText != null)
            _expPercentText.text = ratio.ToString("P2");
    }
}
