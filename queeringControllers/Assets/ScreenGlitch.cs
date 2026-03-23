using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开始界面 Glitch + 雪花屏效果
/// 挂在 BackgroundImage 节点上即可，会自动创建所需的覆盖层
/// </summary>
public class StartScreenGlitch : MonoBehaviour
{
    [Header("=== 雪花屏设置 ===")]
    [Tooltip("雪花覆盖层的不透明度上限（0~1）")]
    public float staticMaxAlpha = 0.18f;

    [Tooltip("每隔多少帧更新一次雪花纹理（越小越密集但越耗性能，推荐4~8）")]
    public int staticUpdateInterval = 5;

    [Header("=== Glitch 设置 ===")]
    [Tooltip("触发一次 glitch 的最短间隔（秒）")]
    public float glitchIntervalMin = 1.5f;

    [Tooltip("触发一次 glitch 的最长间隔（秒）")]
    public float glitchIntervalMax = 5f;

    [Tooltip("每次 glitch 持续的时长（秒）")]
    public float glitchDuration = 0.12f;

    [Tooltip("画面位移的最大像素数")]
    public float glitchMaxOffset = 22f;

    [Tooltip("是否启用颜色通道偏移闪烁")]
    public bool enableColorShift = true;

    // ── 内部引用 ──────────────────────────────────────────────
    private RawImage _bg;                   // 背景 RawImage（即挂载目标）
    private RawImage _staticOverlay;        // 雪花覆盖层
    private Texture2D _noiseTex;            // 雪花噪声纹理
    private Color32[] _noisePixels;         // 像素缓冲
    private RectTransform _bgRect;
    private Vector2 _originalAnchoredPos;
    private Color _originalColor;

    private int _frameCount = 0;
    private bool _glitchActive = false;
    private bool _stopped = false;          // 点击 START 后停止

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        _bg = GetComponent<RawImage>();
        _bgRect = GetComponent<RectTransform>();
        _originalAnchoredPos = _bgRect.anchoredPosition;
        _originalColor = _bg.color;

        CreateStaticOverlay();
        CreateNoiseTex();
    }

    void Start()
    {
        StartCoroutine(GlitchLoop());
    }

    // ─────────────────────────────────────────────────────────
    void Update()
    {
        if (_stopped) return;

        _frameCount++;
        if (_frameCount % staticUpdateInterval == 0)
            UpdateNoise();
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>外部调用：点击 START 后停止所有效果</summary>
    public void StopEffects()
    {
        _stopped = true;
        StopAllCoroutines();

        // 还原位置和颜色
        _bgRect.anchoredPosition = _originalAnchoredPos;
        _bg.color = _originalColor;

        // 隐藏雪花层
        if (_staticOverlay != null)
            _staticOverlay.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────
    // 雪花：在背景上方创建一个 RawImage 覆盖层
    void CreateStaticOverlay()
    {
        GameObject go = new GameObject("StaticOverlay");
        go.transform.SetParent(transform, false);  // 作为 BackgroundImage 的子节点

        _staticOverlay = go.AddComponent<RawImage>();
        _staticOverlay.color = new Color(1f, 1f, 1f, staticMaxAlpha);
        _staticOverlay.raycastTarget = false;

        // 撑满父节点
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // 雪花：创建噪声纹理（低分辨率，放大后天然有颗粒感）
    void CreateNoiseTex()
    {
        int w = 160, h = 120;
        _noiseTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        _noiseTex.filterMode = FilterMode.Point;  // 像素风，不模糊
        _noisePixels = new Color32[w * h];
        UpdateNoise();
        _staticOverlay.texture = _noiseTex;
    }

    // 雪花：每帧随机刷新像素
    void UpdateNoise()
    {
        for (int i = 0; i < _noisePixels.Length; i++)
        {
            // 大多数像素是透明的，少数是白点/灰点
            float r = Random.value;
            if (r < 0.06f)
            {
                byte v = (byte)(Random.value > 0.5f ? 255 : 180);
                _noisePixels[i] = new Color32(v, v, v, 255);
            }
            else
            {
                _noisePixels[i] = new Color32(0, 0, 0, 0);
            }
        }
        _noiseTex.SetPixels32(_noisePixels);
        _noiseTex.Apply();
    }

    // ─────────────────────────────────────────────────────────
    // Glitch 循环：随机间隔触发一次 glitch
    IEnumerator GlitchLoop()
    {
        while (!_stopped)
        {
            float wait = Random.Range(glitchIntervalMin, glitchIntervalMax);
            yield return new WaitForSeconds(wait);

            if (!_stopped)
                yield return StartCoroutine(DoGlitch());
        }
    }

    IEnumerator DoGlitch()
    {
        _glitchActive = true;
        float elapsed = 0f;

        // 随机决定这次 glitch 的"风格"
        bool doHorizontalShift = Random.value > 0.3f;
        bool doStaticBurst = Random.value > 0.4f;
        bool doColorJitter = enableColorShift && Random.value > 0.5f;
        int pulseCount = Random.Range(2, 6);  // glitch 内部抖动次数

        // 临时大幅提高雪花不透明度（静电爆发感）
        if (doStaticBurst)
            _staticOverlay.color = new Color(1f, 1f, 1f, Mathf.Min(staticMaxAlpha * 4f, 0.7f));

        float pulseDuration = glitchDuration / pulseCount;

        for (int p = 0; p < pulseCount; p++)
        {
            // 随机位移
            if (doHorizontalShift)
            {
                float ox = Random.Range(-glitchMaxOffset, glitchMaxOffset);
                float oy = Random.Range(-glitchMaxOffset * 0.3f, glitchMaxOffset * 0.3f);
                _bgRect.anchoredPosition = _originalAnchoredPos + new Vector2(ox, oy);
            }

            // 颜色通道抖动（偏红或偏绿，增加不适感）
            if (doColorJitter)
            {
                float r = Random.Range(0.8f, 1.2f);
                float g = Random.Range(0.8f, 1.1f);
                float b = Random.Range(0.7f, 1.0f);
                _bg.color = new Color(
                    Mathf.Clamp01(_originalColor.r * r),
                    Mathf.Clamp01(_originalColor.g * g),
                    Mathf.Clamp01(_originalColor.b * b),
                    _originalColor.a
                );
            }

            yield return new WaitForSeconds(pulseDuration);
        }

        // 还原
        _bgRect.anchoredPosition = _originalAnchoredPos;
        _bg.color = _originalColor;
        _staticOverlay.color = new Color(1f, 1f, 1f, staticMaxAlpha);

        _glitchActive = false;
    }

    // ─────────────────────────────────────────────────────────
    void OnDestroy()
    {
        if (_noiseTex != null)
            Destroy(_noiseTex);
    }
}