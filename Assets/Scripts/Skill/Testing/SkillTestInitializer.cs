using UnityEngine;
using SlayerLegend.Skill;

// [주혁] -DataManager 정적 클래스 전환에 의해 코드 수정(17, 18, 19)

public class SkillTestInitializer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SkillController skillController;

    [Header("발사체 프리팹")]
    [SerializeField] private SkillProjectile2D fireballProjectile;
    [SerializeField] private IceSpearProjectile iceSpearProjectile;  // 조민희 추가: 관통형 얼음 창
    [SerializeField] private SkillProjectile2D meteorProjectile;

    private void Start()
    {
        // DataManager에서 스킬 데이터 로드
        var fireballData = DataManager.skills.Get("fireball");
        var iceSpearData = DataManager.skills.Get("ice_spear");
        var meteorData = DataManager.skills.Get("meteor");

        // 스킬 생성 및 추가
        if (fireballData != null)
        {
            var fireball = skillController.CreateActiveSkill(fireballData);
            fireball.transform.SetParent(transform);
            fireball.SetProjectilePrefab(fireballProjectile);
            fireball.SetFireDirection(Vector3.right);  // 조민희 추가: 오른쪽 발사
            fireball.SetSpawnOffset(Vector3.zero);
            fireball.SetTestCooldown(0.5f);  // 조민희 추가: 테스트용 0.5초 쿨타임
            fireball.SetTestNoManaCost(true);  // 조민희 추가: 테스트용 무한 마나
            skillController.AddActiveSkill(fireball);
            fireball.SetActive(true);
            Debug.Log("[Test] 파이어볼 스킬 추가 완료");
        }

        if (iceSpearData != null)
        {
            var iceSpear = skillController.CreateActiveSkill(iceSpearData);
            iceSpear.transform.SetParent(transform);
            iceSpear.SetProjectilePrefab(iceSpearProjectile);
            iceSpear.SetFireDirection(Vector3.right);  // 조민희 추가: 오른쪽 발사
            iceSpear.SetSpawnOffset(Vector3.zero);
            iceSpear.SetTestCooldown(0.5f);  // 조민희 추가: 테스트용 0.5초 쿨타임
            iceSpear.SetTestNoManaCost(true);  // 조민희 추가: 테스트용 무한 마나
            skillController.AddActiveSkill(iceSpear);
            iceSpear.SetActive(true);
            Debug.Log("[Test] 얼음 창 스킬 추가 완료");
        }

        if (meteorData != null)
        {
            var meteor = skillController.CreateActiveSkill(meteorData);
            meteor.transform.SetParent(transform);
            meteor.SetProjectilePrefab(meteorProjectile);
            meteor.SetFireDirection(Vector3.down);   // 조민희 추가: 아래쪽 발사
            meteor.SetSpawnOffset(new Vector3(0, 10, 0));  // 위쪽 10에서 발사
            meteor.SetRandomXRange(1f, 3f);  // 조민희 추가: X좌표 1~3 랜덤
            meteor.SetTestCooldown(0.5f);  // 조민희 추가: 테스트용 0.5초 쿨타임
            meteor.SetTestNoManaCost(true);  // 조민희 추가: 테스트용 무한 마나
            skillController.AddActiveSkill(meteor);
            meteor.SetActive(true);
            Debug.Log("[Test] 메테오 스킬 추가 완료");
        }
    }
}