using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂在触发物体上（需要有 Collider 并勾选 Is Trigger）
/// 只有指定的物体碰到时才会跳转场景
/// </summary>
public class TriggerSceneLoad : MonoBehaviour
{
    [Header("拖入指定的触发物体")]
    public GameObject targetObject;

    [Header("要跳转的场景名")]
    public string targetSceneName;

    private bool _triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (targetObject == null) return;

        // 判断碰到的是否是指定物体（或其子物体）
        if (other.gameObject == targetObject || other.transform.IsChildOf(targetObject.transform))
        {
            _triggered = true;
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
