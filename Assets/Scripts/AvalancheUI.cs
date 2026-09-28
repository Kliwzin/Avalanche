using TMPro;
using UnityEngine;

public class AvalancheUI : MonoBehaviour
{
    public TMP_Text counterText;

    [Tooltip("Com tantas peças ou menos, o texto pisca")]
    public int urgentAt = 3;
    [Tooltip("Opcional: pulsa toda vez que o número muda")]
    public UIPunch punch;

    public Color normalColor = Color.white;
    public Color urgentColor = new Color(1f, 0.35f, 0.2f);

    int lastShown = -1;

    void Update()
    {
        var av = AvalancheManager.Instance;
        if (av == null || counterText == null) return;

        int n = av.PiecesUntilAvalanche;

        if (n != lastShown)
        {
            lastShown = n;
            counterText.text = $"AVALANCHE EM {n}";

            if (punch != null) punch.Play();
        }

        Color c = n <= urgentAt ? urgentColor : normalColor;

        if (n <= urgentAt)
            c.a = 0.4f + 0.6f * Mathf.PingPong(Time.unscaledTime * 3f, 1f);

        counterText.color = c;
    }
}