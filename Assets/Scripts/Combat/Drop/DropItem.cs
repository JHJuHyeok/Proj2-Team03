using UnityEngine;

namespace Combat.Drop
{
    public enum DropType
    {
        Gold,
        Exp
    }

    /// <summary>
    /// 드롭 아이템 컴포넌트
    /// 몬스터 사망 시 생성되어 일정 시간 후 플레이어에게 이동하여 흡수됨
    /// </summary>
    public class DropItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private DropType dropType;
        [SerializeField] private float moveDelay = 2f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float collectDistance = 0.5f;
        [SerializeField] private float scatterForce = 2f;

        public DropType DropType => dropType;

        private Transform _playerTransform;
        private long _amount;
        private float _spawnTime;
        private bool _isMovingToPlayer;
        private bool _isInitialized;

        private Vector3 _scatterVelocity;
        private float _scatterDuration = 0.3f;
        private float _scatterTimer;

        /// <summary>
        /// 드롭 아이템 초기화
        /// </summary>
        /// <param name="player">플레이어 Transform</param>
        /// <param name="amount">드롭량</param>
        public void Initialize(Transform player, long amount)
        {
            _playerTransform = player;
            _amount = amount;
            _spawnTime = Time.time;
            _isMovingToPlayer = false;
            _isInitialized = true;

            // 랜덤 방향으로 흩어지는 효과
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            _scatterVelocity = new Vector3(randomDir.x, randomDir.y, 0) * scatterForce;
            _scatterTimer = 0f;
        }

        private void Update()
        {
            if (!_isInitialized || _playerTransform == null) return;

            // 초기 흩어지는 효과
            if (_scatterTimer < _scatterDuration)
            {
                _scatterTimer += Time.deltaTime;
                float t = _scatterTimer / _scatterDuration;
                // 감속 효과
                Vector3 currentVelocity = Vector3.Lerp(_scatterVelocity, Vector3.zero, t);
                transform.position += currentVelocity * Time.deltaTime;
                return;
            }

            // 딜레이 후 플레이어 방향으로 이동 시작
            if (!_isMovingToPlayer)
            {
                if (Time.time - _spawnTime >= moveDelay)
                {
                    _isMovingToPlayer = true;
                }
                return;
            }

            // 플레이어 방향으로 이동
            MoveTowardsPlayer();
        }

        private void MoveTowardsPlayer()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            // 가까워질수록 속도 증가 (빨려들어가는 효과)
            float speedMultiplier = Mathf.Lerp(1f, 3f, 1f - Mathf.Clamp01(distance / 5f));
            transform.position += direction * moveSpeed * speedMultiplier * Time.deltaTime;

            // 흡수 거리 내에 도달하면 수집
            if (distance <= collectDistance)
            {
                OnCollected();
            }
        }

        private void OnCollected()
        {
            // TODO: 실제 보상 적용 로직 (나중에 구현)
            // if (dropType == DropType.Gold)
            // {
            //     GameManager.Instance.AddGold(_amount);
            // }
            // else if (dropType == DropType.Exp)
            // {
            //     GameManager.Instance.AddExp(_amount);
            // }

            Debug.Log($"[DropItem] Collected {dropType}: {_amount}");

            // 상태 초기화
            _isInitialized = false;
            _isMovingToPlayer = false;

            // 풀에 반환
            PoolManager.Instance.ReturnPool(this);
        }

        private void OnDisable()
        {
            // 비활성화 시 상태 초기화
            _isInitialized = false;
            _isMovingToPlayer = false;
        }
    }
}
