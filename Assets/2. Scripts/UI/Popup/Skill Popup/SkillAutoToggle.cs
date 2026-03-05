using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SkillAutoToggle
- Slider를 AUTO 토글처럼 사용
- 값이 0.5 이상이면 ON
- 값이 0.5 미만이면 OFF
- 스킬 자동사용 시스템에 이벤트 전달
*/

public class SkillAutoToggle : MonoBehaviour
{
    [SerializeField] private Slider autoSlider;

    private bool isAuto;

    private void Awake()
    {
        if (autoSlider == null)
            autoSlider = GetComponent<Slider>();

        autoSlider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        bool newState = value >= 0.5f;

        if (isAuto == newState)
            return;

        isAuto = newState;

        Debug.Log("Skill Auto : " + isAuto);

        // 여기서 스킬 시스템에 전달
        SkillAutoSystem.Instance.SetAuto(isAuto);
    }
}