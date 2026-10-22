using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimeStopReason { HitStop, Reward, Pause }

// 시간 멈춤을 이유별로 기록해서 한 곳에서 관리: 하나라도 켜져 있으면 멈추고, 전부 꺼져야 다시 흐름
// (히트스톱이 끝나도 일시정지나 보상 선택의 멈춤이 풀리지 않게 하려는 용도)
public static class TimeControl
{
    private static int _flags;

    public static void Set(TimeStopReason reason, bool stopped)
    {
        int bit = 1 << (int)reason;
        _flags = stopped ? _flags | bit : _flags & ~bit;
        Time.timeScale = _flags == 0 ? 1f : 0f;
    }

    public static void Reset()
    {
        _flags = 0;
        Time.timeScale = 1f;
    }

    // 씬이 바뀌면 멈춤을 모두 풀어서 재시작·타이틀 이동 뒤에 멈춘 채로 남지 않게 함
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Reset();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Reset();
}
