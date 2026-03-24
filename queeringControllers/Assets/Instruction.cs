using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 把你做好的 Instruction 图片拖进 Inspector 的槽里
/// 场景开始自动显示，按 K 切换开关
/// 图片四边渐渐消失融入背景，提示文字固定右下角
/// </summary>
public class InstructionPanel : MonoBehaviour
{
    [Header("把你的 Instruction 图片拖到这里")]
    public Texture instructionImage;

    [Header("边缘淡出设置")]
    [Tooltip("淡出区域的宽度占面板比例（0.1 = 10%）")]
    [Range(0.05f, 0.4f)]
    public float fadeWidth = 0.15f;

    // ── 内部 ──
    private GameObject _panel;
    private bool _isVisible = true;
    private float _timer = 0f;
    private const float LOCK_DURATION = 5f;

    void Awake()
    {
        BuildUI();
    }

    void Update()
    {
        if (!_isVisible) return;

        _timer += Time.deltaTime;

        if (_timer >= LOCK_DURATION && Input.anyKeyDown)
        {
            _isVisible = false;
            _panel.SetActive(false);
        }
    }

    void BuildUI()
    {
        // ── Canvas ──
        GameObject canvasGO = new GameObject("InstructionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── 面板容器 ──
        _panel = new GameObject("Panel");
        _panel.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(1920 * 0.8f, 1080 * 0.8f);

        // ── 图片（带边缘淡出的 RawImage，用自定义 alpha 纹理做 mask）──
        // 先用一个父容器做 mask
        GameObject maskGO = new GameObject("Mask");
        maskGO.transform.SetParent(_panel.transform, false);
        RectTransform maskRT = maskGO.AddComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = Vector2.zero;
        maskRT.offsetMax = Vector2.zero;

        // Mask 组件用边缘渐变的 sprite
        Image maskImg = maskGO.AddComponent<Image>();
        maskImg.sprite = CreateSoftEdgeSprite(256, 256, fadeWidth);
        maskImg.type = Image.Type.Sliced;  // 拉伸但保留边缘比例

        Mask mask = maskGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;  // 隐藏 mask 本身，只留遮罩效果

        // 图片放在 mask 里
        GameObject imgGO = new GameObject("Image");
        imgGO.transform.SetParent(maskGO.transform, false);
        RawImage rawImg = imgGO.AddComponent<RawImage>();
        rawImg.texture = instructionImage;

        RectTransform imgRT = imgGO.GetComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = Vector2.zero;
        imgRT.offsetMax = Vector2.zero;

        // ── 提示文字：固定屏幕右下角 ──
        GameObject hintGO = new GameObject("HintText");
        hintGO.transform.SetParent(canvasGO.transform, false);  // 挂在 Canvas 上，不随 panel 隐藏

        TextMeshProUGUI hint = hintGO.AddComponent<TextMeshProUGUI>();
        hint.text = "> Press any key to continue";
        hint.fontSize = 22;
        hint.color = new Color(1f, 1f, 1f, 0.85f);
        hint.alignment = TextAlignmentOptions.Right;

        RectTransform hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(1f, 0f);   // 右下角
        hintRT.anchorMax = new Vector2(1f, 0f);
        hintRT.pivot = new Vector2(1f, 0f);
        hintRT.anchoredPosition = new Vector2(-30f, 20f);
        hintRT.sizeDelta = new Vector2(700f, 40f);
    }

    // ─────────────────────────────────────────
    //  生成一张中间白色、四边渐渐淡出到透明的 Sprite
    //  用来做 Mask，让图片的边缘自然消失
    // ─────────────────────────────────────────
    Sprite CreateSoftEdgeSprite(int width, int height, float fadeRatio)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / (width - 1);  // 0~1
                float ny = (float)y / (height - 1);  // 0~1

                // 距离四边的最近归一化距离
                float distLeft = nx;
                float distRight = 1f - nx;
                float distBottom = ny;
                float distTop = 1f - ny;
                float minDist = Mathf.Min(distLeft, distRight, distBottom, distTop);

                // 在 fadeRatio 范围内从 0 渐变到 1
                float alpha = Mathf.Clamp01(minDist / fadeRatio);

                // 用平滑曲线让过渡更柔和
                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        // 创建 Sprite，border 设为 0（Simple 模式拉伸）
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            Vector4.zero
        );
        return sprite;
    }
}