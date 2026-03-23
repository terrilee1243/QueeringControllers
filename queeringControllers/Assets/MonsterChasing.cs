using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// 恐怖游戏怪物 AI
/// 功能：
///   - 持续追踪 Player
///   - 碰撞到 Player 后自动跳转下一个 Scene
/// 依赖：
///   - 怪物身上需要有 NavMeshAgent 组件
///   - 场景中需要烘焙好 NavMesh
///   - Player 身上需要有 "Player" Tag
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    [Header("追踪设置")]
    [Tooltip("追踪速度")]
    public float chaseSpeed = 4f;

    [Tooltip("开始追踪的感知距离（超出此距离则原地等待）")]
    public float detectionRange = 20f;

    [Tooltip("每隔多少秒更新一次寻路目标（越小越精准，越大越省性能）")]
    public float pathUpdateInterval = 0.2f;

    [Header("碰撞设置")]
    [Tooltip("判定碰撞到 Player 的距离阈值（作为双重保险，补充 OnCollisionEnter）")]
    public float catchDistance = 1.2f;

    [Header("场景跳转")]
    [Tooltip("勾选后使用指定场景名；不勾选则自动加载 Build Settings 里的下一个场景")]
    public bool useCustomSceneName = false;

    [Tooltip("自定义目标场景名（useCustomSceneName 为 true 时生效）")]
    public string targetSceneName = "";

    // ── 内部状态 ──────────────────────────────
    private NavMeshAgent _agent;
    private Transform _player;
    private float _pathTimer = 0f;
    private bool _caught = false;   // 防止多次触发跳转

    // ─────────────────────────────────────────
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = chaseSpeed;
        _agent.stoppingDistance = 0f;

        // 自动寻找 Player（Tag 为 "Player"）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning("[MonsterAI] 找不到 Tag 为 'Player' 的物体，请检查 Player 的 Tag 设置。");
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (_caught || _player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        // ── 距离判定追踪 ──
        if (distToPlayer <= detectionRange)
        {
            // 按间隔更新寻路，避免每帧重算路径
            _pathTimer += Time.deltaTime;
            if (_pathTimer >= pathUpdateInterval)
            {
                _pathTimer = 0f;
                _agent.SetDestination(_player.position);
            }
        }
        else
        {
            // 超出感知范围：停下等待
            _agent.ResetPath();
        }

        // ── 距离阈值捕获（双重保险）──
        if (distToPlayer <= catchDistance)
        {
            CatchPlayer();
        }
    }

    // ─────────────────────────────────────────
    //  物理碰撞捕获（主要捕获方式）
    // ─────────────────────────────────────────
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            CatchPlayer();
    }

    // CharacterController 不触发 OnCollisionEnter，用 OnControllerColliderHit 补充
    // 如果 Player 用的是 CharacterController，怪物这边额外在 Player 上挂触发器，
    // 或者直接依赖上面 Update 里的距离判定，两者都能兜底。
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            CatchPlayer();
    }

    // ─────────────────────────────────────────
    //  捕获处理 → 跳转场景
    // ─────────────────────────────────────────
    void CatchPlayer()
    {
        if (_caught) return;   // 幂等保护
        _caught = true;

        Debug.Log("[MonsterAI] 抓到 Player，跳转场景...");

        // 停止移动
        _agent.isStopped = true;

        if (useCustomSceneName && !string.IsNullOrEmpty(targetSceneName))
        {
            // 跳转到指定名称的场景
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // 自动加载 Build Settings 中的下一个场景
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextIndex);
            else
                Debug.LogWarning("[MonsterAI] 已经是最后一个场景，没有下一个场景可以跳转。请检查 Build Settings。");
        }
    }

#if UNITY_EDITOR
    // 在 Scene 视图中可视化感知范围和捕获距离
    void OnDrawGizmosSelected()
    {
        // 感知范围（黄色）
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRange);

        // 捕获距离（红色）
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, catchDistance);
    }
#endif
}