using UnityEngine;
using System.Collections;

/// <summary>
/// 场景开始时：
///   1. 游戏暂停，玩家看向正前方 1 秒
///   2. 自动回头看 2 秒
///   3. 转回正前方，游戏开始运转
///
/// 挂到摄像机物体上（PlayerFollowCamera 或 MainCamera）
/// </summary>
public class OpeningLookBack : MonoBehaviour
{
    [Header("时间设置")]
    [Tooltip("看向正前方的时间（秒）")]
    public float lookForwardDuration = 1f;

    [Tooltip("回头看的时间（秒）")]
    public float lookBackDuration = 2f;

    [Header("转头速度")]
    [Tooltip("越大转得越快越突然")]
    public float turnSpeed = 6f;

    void Start()
    {
        Time.timeScale = 0f;  // 暂停游戏
        StartCoroutine(OpeningSequence());
    }

    IEnumerator OpeningSequence()
    {
        // ── 第一阶段：看正前方 1 秒 ──
        yield return new WaitForSecondsRealtime(lookForwardDuration);

        // ── 第二阶段：转向后方 ──
        Quaternion forwardRot = transform.rotation;
        Quaternion backRot = forwardRot * Quaternion.Euler(0f, 180f, 0f);

        yield return TurnTo(backRot, 0.3f);   // 0.3s 内转到后面

        // ── 停留回头看 ──
        yield return new WaitForSecondsRealtime(lookBackDuration);

        // ── 第三阶段：转回正前方 ──
        yield return TurnTo(forwardRot, 0.3f);

        // ── 游戏开始 ──
        Time.timeScale = 1f;
    }

    // 用 unscaledTime 插值转头（不受 timeScale 影响）
    IEnumerator TurnTo(Quaternion target, float duration)
    {
        Quaternion start = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.rotation = target;
    }
}