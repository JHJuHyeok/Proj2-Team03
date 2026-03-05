using UnityEngine;

/*
자동 스킬 사용 여부 관리
*/

public class SkillAutoSystem : MonoBehaviour
{
    public static SkillAutoSystem Instance;

    private bool autoEnabled;

    private void Awake()
    {
        Instance = this;
    }

    public void SetAuto(bool value)
    {
        autoEnabled = value;

        Debug.Log("Auto Skill = " + autoEnabled);
    }

    public bool IsAuto()
    {
        return autoEnabled;
    }
}