using UnityEngine;
using SlayerLegend.Skill;

public class SkillTestInitializer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SkillController skillController;

    private void Start()
    {
        // DataManager에서 스킬 데이터 로드
        var fireballData = DataManager.Instance.skills.Get("fireball");
        var iceSpearData = DataManager.Instance.skills.Get("ice_spear");

        // 스킬 생성 및 추가
        if (fireballData != null)
        {
            var fireball = skillController.CreateActiveSkill(fireballData);
            fireball.transform.SetParent(transform);
            skillController.AddActiveSkill(fireball);
            fireball.SetActive(true); // 조민희 추가: 스킬 활성화
            Debug.Log("[Test] 파이어볼 스킬 추가 완료");
        }

        if (iceSpearData != null)
        {
            var iceSpear = skillController.CreateActiveSkill(iceSpearData);
            iceSpear.transform.SetParent(transform);
            skillController.AddActiveSkill(iceSpear);
            iceSpear.SetActive(true); // 조민희 추가: 스킬 활성화
            Debug.Log("[Test] 얼음 창 스킬 추가 완료");
        }
    }
}