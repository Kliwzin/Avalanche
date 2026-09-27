using System.Collections;
using UnityEngine;

public class AvalancheManager : MonoBehaviour
{
    public static AvalancheManager Instance;

    [Header("Bloco que cai na avalanche")]
    public GameObject avalancheBlockPrefab;

    [Header("Animação da queda")]
    [Tooltip("Tempo que cada bloco leva para cair")]
    public float fallTime = 1.5f;
    [Tooltip("Atraso entre uma coluna e a seguinte")]
    public float stagger = 0.12f;

    [Header("Ritmo")]
    [Tooltip("Peças entre uma avalanche e a próxima, no começo")]
    public int startInterval = 20;
    [Tooltip("Menor intervalo possível")]
    public int minInterval = 8;
    [Tooltip("A cada tantas linhas limpas, o intervalo diminui em 1")]
    public int linesToSpeedUp = 10;

    [Header("Aviso")]
    public GameObject warningUI;
    public float warningTime = 1.2f;

    int piecesSinceLast;

    void Awake()
    {
        Instance = this;
        piecesSinceLast = 0;
        if (warningUI != null) warningUI.SetActive(false);
    }

    public int CurrentInterval
    {
        get
        {
            int reduction = ScoreManager.linesCleared / Mathf.Max(1, linesToSpeedUp);
            return Mathf.Max(minInterval, startInterval - reduction);
        }
    }

    public int PiecesUntilAvalanche => Mathf.Max(0, CurrentInterval - piecesSinceLast);

    public IEnumerator MaybeTrigger(float stepDelay)
    {
        piecesSinceLast++;

        if (piecesSinceLast < CurrentInterval) yield break;

        piecesSinceLast = 0;

        if (warningUI != null)
        {
            warningUI.SetActive(true);
            yield return new WaitForSeconds(warningTime);
            warningUI.SetActive(false);
        }

        yield return GridManager.DropAvalancheRow(avalancheBlockPrefab, fallTime, stagger);
        yield return GridManager.ResolveLinesAndFall(stepDelay);
    }
}