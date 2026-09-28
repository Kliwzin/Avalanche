using System.Collections;
using UnityEngine;

public class UIPunch : MonoBehaviour
{
    [Tooltip("Escala inicial em relação à normal. Acima de 1 = estufa. Abaixo = encolhe.")]
    public float from = 1.6f;
    public float duration = 0.25f;

    [Tooltip("Tocar sozinho quando o objeto é ativado")]
    public bool playOnEnable = true;

    Vector3 baseScale;
    bool captured;
    Coroutine running;

    void Awake()
    {
        baseScale = transform.localScale;
        captured = true;
    }

    void OnEnable()
    {
        if (!captured)
        {
            baseScale = transform.localScale;
            captured = true;
        }

        if (playOnEnable) Play();
    }

    void OnDisable()
    {
        if (captured) transform.localScale = baseScale;
        running = null;
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

            transform.localScale = baseScale * Mathf.Lerp(from, 1f, eased);
            yield return null;
        }

        transform.localScale = baseScale;
        running = null;
    }
}