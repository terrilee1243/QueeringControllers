using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 黑屏打字机效果
/// 挂在黑屏上的 TextMeshProUGUI 物体上
/// 按任意键跳过直接显示全文
/// </summary>
public class TypewriterText : MonoBehaviour
{
    [Tooltip("打字速度（每秒打几个字符）")]
    public float charsPerSecond = 20f;

    [Tooltip("开始打字前的延迟（秒）")]
    public float startDelay = 0.5f;

    private TextMeshProUGUI _tmp;
    private string _fullText;
    private bool _isTyping = false;
    private bool _isComplete = false;
    private Coroutine _typingCoroutine;

    // 外部订阅：打字完成时触发
    public System.Action OnComplete;

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _tmp.text = "";
        _fullText = _tmp.text;
        gameObject.SetActive(false);
    }

    void Update()
    {
        // 打字过程中按任意键 → 跳过，立即显示全文
        if (_isTyping && !_isComplete && Input.anyKeyDown)
            SkipToEnd();
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>从 StartScreenManager 调用，传入文字内容并开始打字</summary>
    public void StartTyping(string text)
    {
        _fullText = text;
        gameObject.SetActive(true);
        _tmp.text = "";
        _isComplete = false;
        _typingCoroutine = StartCoroutine(TypeRoutine());
    }

    // ─────────────────────────────────────────────────────────
    IEnumerator TypeRoutine()
    {
        _isTyping = true;
        yield return new WaitForSeconds(startDelay);

        float interval = 1f / charsPerSecond;
        int index = 0;

        while (index <= _fullText.Length)
        {
            _tmp.text = _fullText.Substring(0, index);
            index++;
            yield return new WaitForSeconds(interval);
        }

        FinishTyping();
    }

    // ─────────────────────────────────────────────────────────
    void SkipToEnd()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);
        FinishTyping();
    }

    void FinishTyping()
    {
        _tmp.text = _fullText;
        _isTyping = false;
        _isComplete = true;
        OnComplete?.Invoke();
    }

    // ─────────────────────────────────────────────────────────
    public void Hide()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);
        _tmp.text = "";
        gameObject.SetActive(false);
        _isTyping = false;
        _isComplete = false;
    }

    public bool IsComplete => _isComplete;
}