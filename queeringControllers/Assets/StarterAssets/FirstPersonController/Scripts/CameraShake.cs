using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("抖动参数")]
    public float duration = 0.4f;
    public float magnitude = 0.3f;
    public float frequency = 25f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake()
    {
        StopAllCoroutines();
        transform.localPosition = originalPos;
        StartCoroutine(DoShake(duration, magnitude));
    }

    public void Shake(float customDuration, float customMagnitude)
    {
        StopAllCoroutines();
        transform.localPosition = originalPos;
        StartCoroutine(DoShake(customDuration, customMagnitude));
    }

    private IEnumerator DoShake(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);

            float offsetX = Mathf.Sin(elapsed * frequency * Mathf.PI)
                            * shakeMagnitude * damper
                            * Random.Range(0.8f, 1.2f);

            float offsetY = Mathf.Cos(elapsed * frequency * 1.3f * Mathf.PI)
                            * shakeMagnitude * damper
                            * Random.Range(0.8f, 1.2f);

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}