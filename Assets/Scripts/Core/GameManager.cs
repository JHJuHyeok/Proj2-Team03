using UnityEngine;
using System;

/*
GameManager
- 게임의 "상태"만 관리
- UI, 사운드, 연출 직접 제어 X
- 상태 변경 이벤트만 발생
*/
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 게임 전체 상태
    public enum GameState
    {
        Lobby,
        Battle,
        Menu
    }

    public GameState CurrentState { get; private set; }

    // 상태 변경 알림 이벤트
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        // 싱글톤 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 최초 상태는 로비
        SetState(GameState.Lobby);
    }

    // 상태 변경 함수 (외부에서 호출)
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // UIManager, 다른 시스템들에게 알림
        OnGameStateChanged?.Invoke(CurrentState);
    }
}
