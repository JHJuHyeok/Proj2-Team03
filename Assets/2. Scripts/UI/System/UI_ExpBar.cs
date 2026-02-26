using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ExpBar : MonoBehaviour
{
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TMP_Text _expText;

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
        if (_expText != null)
            _expText.text = $"{current:F0}/{max:F0}";
    }
}
