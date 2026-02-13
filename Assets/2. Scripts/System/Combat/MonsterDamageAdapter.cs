using UnityEngine;
using SlayerLegend.Skill;

namespace SlayerLegend.Combat
{
    /*
    MonsterBase를 IDamageable로 변환하는 어댑터 컴포넌트
    팀원 파일(MonsterBase.cs)을 수정하지 않고 스킬 시스템과 연동
    
    작성자: 조민희
    작성일: 2025-02-06
    설명: 어댑터 패턴으로 MonsterBase.TakeDamage(double)를 IDamageable.TakeDamage(float)로 변환
    
    작성자: 신태환
    작성일: 2025-02-12
    설명:  IDamageable TakeDamage double로 변경돼서 void IDamageable.TakeDamage(float damage) 제거
    */
    public class MonsterDamageAdapter : MonoBehaviour
    {
        private MonsterBase _monster;

        private void Awake()
        {
            // 같은 GameObject의 MonsterBase 찾기
            _monster = GetComponent<MonsterBase>();

            if (_monster == null)
            {
                Debug.LogWarning($"[MonsterDamageAdapter] MonsterBase 컴포넌트를 찾을 수 없습니다: {gameObject.name}");
            }
        }

        // 선택사항: 외부에서 직접 MonsterBase에 접근할 수 있는 프로퍼티
        public MonsterBase Monster => _monster;
    }
}
