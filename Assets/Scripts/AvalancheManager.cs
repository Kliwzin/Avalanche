using System.Collections;
using UnityEngine;

public class AvalancheManager : MonoBehaviour
{
    public static AvalancheManager Instance;

    [Header("Avalanche normal")]
    public GameObject avalancheBlockPrefab;

    [Header("Avalanche de runas")]
    [Tooltip("Prefabs dos BLOCOS de runa (não as peças)")]
    public GameObject[] runeAvalanchePrefabs;
    [Tooltip("Colunas presas por metal para a avalanche virar de runas")]
    public int runeAvalancheMetalColumns = 4;

    [Header("Animação da queda")]
    public float fallTime = 1.5f;
    public float stagger = 0.12f;

    [Header("Ritmo")]
    [Tooltip("Peças entre uma avalanche e a próxima, no começo")]
    public int startInterval = 14;
    [Tooltip("Menor intervalo possível")]
    public int minInterval = 6;
    [Tooltip("A cada tantas linhas limpas, o intervalo diminui em 1")]
    public int linesToSpeedUp = 6;
    [Tooltip("Variação aleatória do intervalo, para mais e para menos")]
    public int intervalJitter = 3;

    [Header("Aviso")]
    public GameObject warningUI;
    [Tooltip("Aviso alternativo quando for de runas. Vazio = usa o normal.")]
    public GameObject runeWarningUI;
    public float warningTime = 1.2f;

    int piecesSinceLast;
    int currentTarget;

    void Awake()
    {
        Instance = this;
        piecesSinceLast = 0;
        RollNextInterval();

        if (warningUI != null) warningUI.SetActive(false);
        if (runeWarningUI != null) runeWarningUI.SetActive(false);
    }

    int BaseInterval
    {
        get
        {
            int reduction = ScoreManager.linesCleared / Mathf.Max(1, linesToSpeedUp);
            return Mathf.Max(minInterval, startInterval - reduction);
        }
    }

    void RollNextInterval()
    {
        int b = BaseInterval;
        int lo = Mathf.Max(1, b - intervalJitter);
        int hi = b + intervalJitter;

        currentTarget = Random.Range(lo, hi + 1);
    }

    public int PiecesUntilAvalanche => Mathf.Max(0, currentTarget - piecesSinceLast);

    bool NextIsRuneAvalanche =>
        runeAvalanchePrefabs != null
        && runeAvalanchePrefabs.Length > 0
        && GridManager.MetalColumnCount() >= runeAvalancheMetalColumns;

    public IEnumerator MaybeTrigger(float stepDelay)
    {
        piecesSinceLast++;

        if (piecesSinceLast < currentTarget) yield break;

        piecesSinceLast = 0;

        bool rune = NextIsRuneAvalanche;

        GameObject aviso = (rune && runeWarningUI != null) ? runeWarningUI : warningUI;

        if (aviso != null)
        {
            aviso.SetActive(true);
            yield return new WaitForSeconds(warningTime);
            aviso.SetActive(false);
        }

        if (rune)
            yield return GridManager.DropRuneAvalanche(runeAvalanchePrefabs, fallTime, stagger);
        else
            yield return GridManager.DropAvalancheRow(avalancheBlockPrefab, fallTime, stagger);

        yield return GridManager.ResolveLinesAndFall(stepDelay);

        RollNextInterval();
    }
}