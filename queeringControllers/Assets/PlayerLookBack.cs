using UnityEngine;
using System.Collections;

/// <summary>
/// 每隔 5 秒自动向后看 0.7 秒，然后转回来
/// 挂到 Player 的摄像机物体（PlayerFollowCamera 或 MainCamera）上
/// </summary>
public class AutoLookBack : MonoBehaviour
{
    [Header("时间设置")]
    [Tooltip("多少秒触发一次回头")]
    public float interval = 5f;

    [Tooltip("回头持续时间（秒）")]
    public float lookBackDuration = 0.7f;

    [Header("回头速度")]
    [Tooltip("摄像机转向的速度，越大越快")]
    public float turnSpeed = 8f;

    // ── 内部 ──
    private float _timer = 0f;
    private bool _isLookingBack = false;
    private Quaternion _originalRot;
    private Quaternion _targetRot;

    void Update()
    {
        if (_isLookingBack) return;

        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            _timer = 0f;
            StartCoroutine(LookBackRoutine());
        }
    }

    IEnumerator LookBackRoutine()
    {
        _isLookingBack = true;

        // 记录当前朝向，计算向后 180 度的目标朝向
        _originalRot = transform.rotation;
        _targetRot = _originalRot * Quaternion.Euler(0f, 180f, 0f);

        // 转向后面
        float elapsed = 0f;
        while (elapsed < lookBackDuration * 0.3f)   // 用 30% 时间转过去
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, elapsed / (lookBackDuration * 0.3f) * turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = _targetRot;

        // 停留
        yield return new WaitForSeconds(lookBackDuration);

        // 转回来
        elapsed = 0f;
        float returnDuration = 0.25f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, _originalRot, elapsed / returnDuration);
            yield return null;
        }
        transform.rotation = _originalRot;

        _isLookingBack = false;
    }
}