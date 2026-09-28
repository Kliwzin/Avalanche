using System.Collections;
using UnityEngine;

public class UIPunch : MonoBehaviour
{
    public float from = 1.6f;
    public float duration = 0.25f;

    [Tooltip("Tocar sozinho quando o objeto é ativado")]
    public bool playOnEnable = true;

    Coroutine running;

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    public void Play()
    {
        if (!gameObject.activeInHierarchy) return;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(t / duration);
            float eased = 1f - (1f - k) * (1f - k);

            transform.localScale = Vector3.one * Mathf.Lerp(from, 1f, eased);
            yield return null;
        }

        transform.localScale = Vector3.one;
        running = null;
    }
}