using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 场景开始时显示 Mission 图片 5 秒
/// 期间游戏暂停，5 秒后自动消失，游戏开始运转
/// </summary>
public class MissionPanel : MonoBehaviour
{
    [Header("把你的 Mission 图片拖到这里")]
    public Texture missionImage;

    [Header("显示时长（秒）")]
    public float displayDuration = 5f;

    [Header("边缘淡出设置")]
    [Range(0.05f, 0.4f)]
    public float fadeWidth = 0.15f;

    private GameObject _panel;
    private float _timer = 0f;
    private bool _done = false;

    void Awake()
    {
        BuildUI();
        Time.timeScale = 0f;  // 暂停游戏
    }

    void Update()
    {
        if (_done) return;

        // unscaledDeltaTime 不受 timeScale 影响，暂停时也能计时
        _timer += Time.unscaledDeltaTime;

        if (_timer >= displayDuration)
        {
            _done = true;
            _panel.SetActive(false);
            Time.timeScale = 1f;  // 恢复游戏
        }
    }

    void BuildUI()
    {
        // ── Canvas ──
        GameObject canvasGO = new GameObject("MissionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 998;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── 面板容器 ──
        _panel = new GameObject("MissionPanel");
        _panel.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(1920 * 0.8f, 1080 * 0.8f);

        // ── Mask 容器（四边淡出）──
        GameObject maskGO = new GameObject("Mask");
        maskGO.transform.SetParent(_panel.transform, false);
        RectTransform maskRT = maskGO.AddComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = Vector2.zero;
        maskRT.offsetMax = Vector2.zero;

        Image maskImg = maskGO.AddComponent<Image>();
        maskImg.sprite = CreateSoftEdgeSprite(256, 256, fadeWidth);

        Mask mask = maskGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // ── 图片 ──
        GameObject imgGO = new GameObject("Image");
        imgGO.transform.SetParent(maskGO.transform, false);
        RawImage rawImg = imgGO.AddComponent<RawImage>();
        rawImg.texture = missionImage;

        RectTransform imgRT = imgGO.GetComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = Vector2.zero;
        imgRT.offsetMax = Vector2.zero;
    }

    Sprite CreateSoftEdgeSprite(int width, int height, float fadeRatio)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / (width - 1);
                float ny = (float)y / (height - 1);

                float minDist = Mathf.Min(nx, 1f - nx, ny, 1f - ny);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(minDist / fadeRatio));

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Vector4.zero);
    }
}