using UnityEngine;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 대각선 이동 발사체 (Water_04 파도베기용)
    /// - 위에서 오른쪽 아래로 대각선 이동
    /// - 7연발 스킬에 사용
    /// </summary>
    public class DiagonalProjectile : SkillProjectile2D
    {
        [Header("대각선 이동 설정")]
        [SerializeField] private bool useDiagonalDirection = true;
        [SerializeField] private Vector3 customDirection = new Vector3(1f, -1f, 0f); // 오른쪽 아래 대각선

        protected virtual void Awake()
        {
            // 대각선 방향 정규화
            if (useDiagonalDirection)
            {
                _moveDirection = customDirection.normalized;
            }
        }

        public override void Initialize(Vector3 spawnPosition, double damage, bool isCritical, Vector3 direction = default)
        {
            base.Initialize(spawnPosition, damage, isCritical, direction);

            // 대각선 방향 강제 설정 (direction 파라미터 무시)
            if (useDiagonalDirection)
            {
                _moveDirection = customDirection.normalized;
            }

            // 회전 설정 (이동 방향에 맞게)
            float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 대각선 발사체 생성 (위→오른쪽 아래)
        /// </summary>
        public static DiagonalProjectile Create(DiagonalProjectile prefab, Vector3 position, double damage, bool isCritical)
        {
            if (prefab == null) return null;

            DiagonalProjectile projectile = Instantiate(prefab, position, Quaternion.identity);
            projectile.Initialize(position, damage, isCritical);
            return projectile;
        }
    }
}
