using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂到 Button 上，点击后跳转到指定场景
/// </summary>
public class ClickButtonChangeScene : MonoBehaviour
{
    [Header("要跳转的场景名")]
    public string targetSceneName;

    public void OnButtonClick()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}