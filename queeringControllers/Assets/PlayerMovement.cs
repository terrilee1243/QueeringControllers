using UnityEngine;

/// <summary>
/// 恐怖游戏专用第一人称控制器
/// 操作说明：
///   W + E 同时按下 → 向前冲
///   A           → 向左转视角
///   D           → 向右转视角
///   其余键盘按键全部禁用
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class HorrorPlayerController : MonoBehaviour
{
    [Header("冲刺设置")]
    [Tooltip("向前冲的移动速度")]
    public float dashSpeed = 8f;

    [Header("视角旋转设置")]
    [Tooltip("左右转头的旋转速度（度/秒）")]
    public float lookSpeed = 90f;

    [Header("重力设置")]
    public float gravity = -9.81f;

    // 内部引用
    private CharacterController _controller;
    private float _verticalVelocity = 0f;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // 禁用所有可能存在于旧脚本上的输入
        DisableLegacyInputComponents();

        // 锁定并隐藏鼠标光标（恐怖游戏常规设置）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    // ─────────────────────────────────────────
    //  视角：A 向左转 / D 向右转
    // ─────────────────────────────────────────
    void HandleLook()
    {
        float rotationDirection = 0f;

        // 只响应 A / D，其他键完全无视
        if (Input.GetKey(KeyCode.A)) rotationDirection = -1f; // 向左
        if (Input.GetKey(KeyCode.D)) rotationDirection = 1f; // 向右

        if (rotationDirection != 0f)
        {
            float rotationAmount = rotationDirection * lookSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }
    }

    // ─────────────────────────────────────────
    //  移动：W + E 同时按下 → 向前冲
    // ─────────────────────────────────────────
    void HandleMovement()
    {
        // 必须同时按下 W 和 E 才触发冲刺，缺一不可
        bool wHeld = Input.GetKey(KeyCode.W);
        bool eHeld = Input.GetKey(KeyCode.E);
        bool dashing = wHeld && eHeld;

        // 重力始终生效
        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f; // 保持贴地，避免浮空累积
        else
            _verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = Vector3.zero;

        if (dashing)
            move = transform.forward * dashSpeed;

        move.y = _verticalVelocity;

        _controller.Move(move * Time.deltaTime);
    }

    // ─────────────────────────────────────────
    //  禁用旧有的输入相关组件
    // ─────────────────────────────────────────
    void DisableLegacyInputComponents()
    {
        // 如果 import 的 prefab 带有 Unity 自带的 FirstPersonController 或类似脚本，一并禁用
        // 根据你使用的资源包，类名可能不同，按需增删
        string[] legacyScripts = new string[]
        {
            "FirstPersonController",
            "StarterAssetsInputs",
            "PlayerInput",           // Unity Input System 组件
            "ThirdPersonController",
            "BasicRigidBodyPush",
            "PlayerInputManager",
        };

        foreach (string scriptName in legacyScripts)
        {
            System.Type t = System.Type.GetType(scriptName);
            if (t == null)
                // 尝试在 UnityEngine 命名空间下查找
                t = System.Type.GetType("UnityEngine." + scriptName + ", UnityEngine");
            if (t == null) continue;

            Component comp = GetComponent(t);
            if (comp is Behaviour beh)
            {
                beh.enabled = false;
                Debug.Log($"[HorrorPlayerController] 已禁用旧组件: {scriptName}");
            }
        }
    }

#if UNITY_EDITOR
    // 编辑器下在 Scene 视图画出朝向，方便调试
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
#endif
}