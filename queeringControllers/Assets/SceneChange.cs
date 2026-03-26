using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// 恐怖游戏开始界面控制器
/// 流程：显示背景图 → 点击START → 黑屏2s → 播放视频1 → 黑屏1s → 闪现视频2 → 加载GameScene
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    [Header("=== 场景引用 ===")]
    [Tooltip("背景图片 RawImage 组件")]
    public RawImage backgroundImage;

    [Tooltip("挂在 BackgroundImage 上的 Glitch 脚本")]
    public StartScreenGlitch glitchEffect;

    [Tooltip("START 按钮")]
    public Button startButton;

    [Tooltip("视频播放器")]
    public VideoPlayer videoPlayer;

    [Tooltip("VideoPlayer 用的 Audio Source（挂在同一个物体上）")]
    public AudioSource videoAudioSource;

    [Header("=== 各阶段音乐 ===")]
    [Tooltip("开始界面的背景音乐（循环播放）")]
    public AudioClip musicStartScreen;

    [Tooltip("点击 START 按钮时的音效")]
    public AudioClip sfxButtonClick;

    [Tooltip("第一段黑屏期间的音乐")]
    public AudioClip musicBlackScreen;

    [Tooltip("第一段视频播放时的额外背景音（如不需要留空）")]
    public AudioClip musicVideo1;

    // 用来播放上面这些音频的专属 AudioSource（代码自动创建，无需手动添加）
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    [Tooltip("播放视频时显示的 RawImage（全屏覆盖）")]
    public RawImage videoDisplay;

    [Tooltip("第二段闪现视频的 VideoClip")]
    public VideoClip flashVideoClip;

    [Tooltip("第一段视频结束后，带音乐的黑屏时长（秒）")]
    public float postVideo1BlackDuration = 1.5f;

    [Tooltip("第一段视频结束后黑屏的音乐")]
    public AudioClip musicPostVideo1Black;

    [Tooltip("第一段视频结束后黑屏等待的时间（秒）")]
    public float midBlackScreenDuration = 1f;

    [Tooltip("黑色淡入淡出遮罩 Image（alpha 从0到1）")]
    public Image fadeOverlay;

    [Tooltip("黑屏打字机文字组件（挂在黑屏上的 TypewriterText）")]
    public TypewriterText typewriterText;

    [Header("=== 场景设置 ===")]
    [Tooltip("视频播完后要加载的场景名称")]
    public string nextSceneName = "GameScene";

    [Header("=== 动画时长 ===")]
    [Tooltip("各阶段切换前的静默时长（秒）")]
    public float silenceDuration = 1f;

    [Tooltip("点击START后黑屏等待的时间（秒）")]
    public float blackScreenDuration = 2f;

    [Tooltip("视频结束后，淡入黑色的时间（秒）")]
    public float fadeInDuration = 1.2f;

    // ── 内部状态 ──────────────────────────────────────────────
    private bool _isStarted = false;
    private RenderTexture _renderTexture;
    private bool _firstVideoDone = false;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        // 初始状态：视频层隐藏，遮罩全透明
        videoDisplay.gameObject.SetActive(false);
        SetFadeAlpha(0f);

        // 创建两个专用 AudioSource（BGM 循环 + SFX 单次）
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        // 播放开始界面背景音乐
        PlayBGM(musicStartScreen);

        // 绑定按钮事件
        startButton.onClick.AddListener(OnStartClicked);

        // 配置 VideoPlayer（不自动播放，播完触发回调）
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnAnyVideoFinished;

        // 为 VideoPlayer 创建 RenderTexture 并绑定到 RawImage
        PrepareVideoTexture();
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>点击 START 按钮（按钮事件绑定用）</summary>
    void OnStartClicked() => TriggerStart();

    /// <summary>公开入口：PressAnyKey 或按钮都可调用</summary>
    public void TriggerStart()
    {
        if (_isStarted) return;
        _isStarted = true;

        startButton.interactable = false;

        // 按钮音效
        PlaySFX(sfxButtonClick);

        // 停止 glitch 效果
        if (glitchEffect != null)
            glitchEffect.StopEffects();

        StartCoroutine(StartSequence());
    }

    // ─────────────────────────────────────────────────────────
    IEnumerator StartSequence()
    {
        // 1. 开始界面阶段结束：停音乐，静默1s
        _bgmSource.Stop();
        yield return new WaitForSeconds(silenceDuration);

        // 2. 黑屏
        backgroundImage.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        SetFadeAlpha(1f);
        PlayBGM(musicBlackScreen);

        // 3. 启动打字机（同时进行，不阻塞音乐）
        bool typewriterDone = false;
        if (typewriterText != null)
        {
            typewriterText.OnComplete = () => typewriterDone = true;
            typewriterText.StartTyping(
                "You are an astronaut aboard a space station where a lab experiment has escaped and become a deadly creature.\n\n" +
                "To survive, you must collect enough energy to restore systems while avoiding the monster stalking you in the dark.\n\n" +
                "These are 2 clips of the creature you will face."
            );
        }
        else
        {
            typewriterDone = true;
        }

        // 4. 等黑屏时长 且 打字完成，两个条件都满足才继续
        float blackElapsed = 0f;
        while (blackElapsed < blackScreenDuration || !typewriterDone)
        {
            blackElapsed += Time.deltaTime;
            yield return null;
        }

        // 5. 黑屏阶段结束：隐藏文字，停音乐，静默
        if (typewriterText != null) typewriterText.Hide();
        _bgmSource.Stop();
        yield return new WaitForSeconds(silenceDuration);

        // 4. 显示视频层，切换到视频1音乐，开始播放
        PlayBGM(musicVideo1);
        videoDisplay.gameObject.SetActive(true);
        SetFadeAlpha(0f);
        videoPlayer.Play();

        // 4. 等待视频播完（由 OnVideoFinished 回调处理）
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>任意视频播完的统一回调</summary>
    void OnAnyVideoFinished(VideoPlayer vp)
    {
        if (!_firstVideoDone)
        {
            _firstVideoDone = true;
            StartCoroutine(MidBlackThenFlash());
        }
        else
        {
            StartCoroutine(LoadNextScene());
        }
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>视频1结束 → 黑屏1s → 播放闪现视频2</summary>
    IEnumerator MidBlackThenFlash()
    {
        // 1. 立即黑屏（遮罩盖住）
        SetFadeAlpha(1f);
        videoPlayer.Stop();

        // 2. 播放带音乐的黑屏（1.5s）
        PlayBGM(musicPostVideo1Black);
        yield return new WaitForSeconds(postVideo1BlackDuration);

        // 3. 静默等待（midBlackScreenDuration），让音乐收尾
        _bgmSource.Stop();
        yield return new WaitForSeconds(midBlackScreenDuration);

        // 3. 换成第二段视频，重新绑定音频（切换 clip 后音频会断）
        videoPlayer.clip = flashVideoClip;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        SetFadeAlpha(0f);
        videoPlayer.Play();
    }

    // ─────────────────────────────────────────────────────────
    IEnumerator LoadNextScene()
    {
        // 1. 视频2结束：停音乐，静默1s
        _bgmSource.Stop();
        videoPlayer.Stop();
        yield return new WaitForSeconds(silenceDuration);

        // 2. 黑色遮罩淡入
        yield return StartCoroutine(FadeToBlack(fadeInDuration));

        // 2. 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        while (!asyncLoad.isDone)
            yield return null;
    }

    // ─────────────────────────────────────────────────────────
    // 辅助：黑色遮罩淡入（fadeOverlay alpha 0 → 1）
    IEnumerator FadeToBlack(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(0f, 1f, elapsed / duration));
            yield return null;
        }
        SetFadeAlpha(1f);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }

    // ─────────────────────────────────────────────────────────
    void PlayBGM(AudioClip clip)
    {
        if (_bgmSource == null) return;
        if (clip == null) { _bgmSource.Stop(); return; }
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    void PlaySFX(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>创建 RenderTexture 并绑定到 VideoPlayer + RawImage</summary>
    void PrepareVideoTexture()
    {
        // 用屏幕分辨率创建 RenderTexture
        _renderTexture = new RenderTexture(
            Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = _renderTexture;
        videoDisplay.texture = _renderTexture;
    }

    // ─────────────────────────────────────────────────────────
    void OnDestroy()
    {
        // 清理 RenderTexture 防止内存泄漏
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
}