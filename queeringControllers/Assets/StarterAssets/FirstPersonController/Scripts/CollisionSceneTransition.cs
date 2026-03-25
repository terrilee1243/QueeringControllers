using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProximitySceneTransition : MonoBehaviour
{
    [Header("目标物体")]
    public Transform targetObject;

    [Header("触发距离")]
    public float triggerDistance = 1.5f;

    [Header("碰撞后启用的物体")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("碰撞后禁用的物体")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("场景跳转设置")]
    public string targetSceneName = "";

    [Header("延迟时间（秒）")]
    public float delay = 2f;

    private bool triggered = false;

    private void Update()
    {
        if (triggered) return;
        if (targetObject == null) return;

        float distance = Vector3.Distance(transform.position, targetObject.position);

        if (distance <= triggerDistance)
        {
            HandleTrigger();
        }
    }

    private void HandleTrigger()
    {
        triggered = true;

        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(targetSceneName);
    }

    // 在 Scene 视图中可视化触发范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}