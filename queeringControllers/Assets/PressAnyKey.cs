using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 挂在 "Press Any Button" Text 物体上
/// 按任意键后：文字消失，触发 StartScreenManager 开始流程
/// </summary>
public class PressAnyKey : MonoBehaviour
{
    [Tooltip("文字闪烁的速度（次/秒）")]
    public float blinkSpeed = 1.8f;

    [Tooltip("StartScreenManager 脚本引用")]
    public StartScreenManager startScreenManager;

    [Tooltip("START 按钮（触发后禁用，防止重复点击）")]
    public Button startButton;

    private TextMeshProUGUI _text;
    private bool _triggered = false;

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (_triggered) return;

        // 闪烁效果
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed * Mathf.PI));
        Color c = _text.color;
        c.a = alpha;
        _text.color = c;

        // 检测任意键盘键 或 手柄任意键
        if (Input.anyKeyDown)
        {
            _triggered = true;
            StartCoroutine(OnAnyKeyPressed());
        }
    }

    // ─────────────────────────────────────────────────────────
    IEnumerator OnAnyKeyPressed()
    {
        // 1. 文字立刻隐藏
        _text.enabled = false;

        // 短暂等一帧，确保渲染刷新
        yield return null;

        // 2. 触发 StartScreenManager 的开始流程
        if (startScreenManager != null)
            startScreenManager.TriggerStart();
    }
}