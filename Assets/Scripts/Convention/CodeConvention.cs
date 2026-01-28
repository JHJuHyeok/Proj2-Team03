
// ###코드 컨벤션###

#region 줄 바꿈 규칙

/*
// 잘못됨!
private void FuncMethod(){

}

// 맞음
private void FuncMethod()
{

}

키워드나 함수, 클래스를 정의하는 등 새 괄호를 열 때는 줄바꿈을 한다
*/

#endregion

#region 접근 제한자 명시

// void Awake() { }         <- 틀렸음
// private void Awake() { } <- 맞음

// private, public과 같은 접근 제한자 명시할 것

#endregion

#region 명명 규칙

/* 변수 명명
 
 * private, protected 필드는 _를 붙일 것
 * 단, 매개변수나 로컬 변수는 그대로

 private int count;     <- 틀렸음
 private int _count;    <- 맞음


 [SerializeField] private string _name;     <- 틀렸음
 [SerializeField] private string name;      <- 맞음


 // 매개변수
 public int GetValue(List<int> _list) { }   <- 틀렸음
 public int GetValue(List<int> list) { }    <- 맞음


 // 로컬변수
 private void Awake()
 {
    int _count = 1;         <- 틀렸음
 }
 private void Awake()
 {
    int count = 1;          <- 맞음
 }

*/

/* 프로퍼티는 대문자
 
 public int count { get; set; }     <- 틀렸음
 public int Count { get; set; }     <- 맞음

*/

/* 풀네임 권장
 
 * 변수나 함수의 이름은 역할이 잘 드러나도록
 * 변수의 이름은 명사, 함수의 이름은 동사
 * 축약 X, 되도록 풀네임
 
 private int _idx;      <- 비추
 private int _index;    <- 권장

 private SpriteRenderer _sr;                <- 비추
 private SpriteRenderer _spriteRenderer;    <- 권장

 private int Count() { }                    <- 비추
 private int GetCountEnemyInScreen() { }    <- 권장

*/

#endregion

#region 필드/프로퍼티

/* 지나친 전역 변수 사용은 지양
 
 public int count;                  <- 비추
 public int Count { get; set; }     <- 권장


 // 외부에서 접근해야 하는 경우 프로퍼티로 관리

 public float Hp
 {
     get => _hp;
     set { _hp = value; }
 }
 private float _hp;

*/

#endregion

#region 주석

// 주석은 간단명료하게

/// <summary>
/// 메서드 주석은 summary를 이용
/// </summary>
//public void FuncMethod() { }

#endregion

#region 인터페이스

// 형용사구로 이름 정의, 메서드 앞에 'I' 접두사 사용
// public interface IAttackable { }
// public interface IDamageable { }

#endregion

#region 이벤트

/* 상황 명시
 
 public event Action OpeningDoor;       // 이벤트 전
 public event Action DoorOpened;        // 이벤트 후

 public event Action<int> PointsScored; 	            // 점수를 버는 이벤트 명명
 public event Action<CustomEventArgs> ThingHappened;    // 어떠한 이벤트 발생에 대한 명명

 // 이벤트 발생 메서드 예
 public void OnDoorOpened()
 {
     DoorOpened?.Invoke();
 }
 
 public void OnPointsScored(int points)
 {
     PointsScored?.Invoke(points);
 }
*/

#endregion

#region 코드 구분

// region 구분은 가독성이 높아지지만 일반적인 스크립트에선 눈이 어지러워지는 단점이 있음

/* 기능 주석으로 구분
 
 // 플레이어 움직임
 private RigidBody _rigidBody;
 [SerializeField] private float moveSpeed;

*/

#endregion