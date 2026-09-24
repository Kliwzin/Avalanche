using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominosGameplay;
    public GameObject[] tetrominosPreview;
    public Transform nextPiecePreview;

    [Header("Spawn Weights (mesmo tamanho dos arrays)")]
    [Min(0f)] public float[] weights;

    [Header("Metal")]
    public Sprite metalSprite;
    public RuntimeAnimatorController metalAnimator;
    [Tooltip("Linhas limpas antes de o metal começar a aparecer")]
    public int metalStartLines = 5;
    [Tooltip("A cada tantas linhas limpas, +1 bloco de metal por peça")]
    public int linesPerExtraMetal = 15;
    public int metalMaxPerPiece = 3;
    [Range(0f, 1f)] public float metalChance = 0.35f;

    int nextIndex;
    bool[] isRunePiece;

    void Start()
    {
        if (weights != null && weights.Length > 0 && weights.Length != tetrominosGameplay.Length)
            Debug.LogWarning("[Spawner] 'weights' tem tamanho diferente de 'tetrominosGameplay'. Os pesos estão sendo ignorados.", this);

        // Descobre uma vez só quais prefabs são peças de runa
        isRunePiece = new bool[tetrominosGameplay.Length];
        for (int i = 0; i < tetrominosGameplay.Length; i++)
            isRunePiece[i] = tetrominosGameplay[i].GetComponentInChildren<RuneBlock>(true) != null;

        PickNext();
        Spawn();
    }

    public void Spawn()
    {
        var piece = Instantiate(tetrominosGameplay[nextIndex], transform.position, Quaternion.identity);

        if (!isRunePiece[nextIndex]) ApplyMetal(piece);

        var t = piece.GetComponent<Tetromino>();
        if (t != null)
        {
            t.Init(this);
            if (!t.ValidMove()) GameManager.Instance.GameOver();
        }

        PickNext();
    }

    // Transforma alguns blocos da peça em metal. Quanto mais o jogador avança,
    // mais blocos de metal por peça.
    void ApplyMetal(GameObject piece)
    {
        if (metalSprite == null) return;
        if (ScoreManager.linesCleared < metalStartLines) return;
        if (Random.value > metalChance) return;

        Transform root = piece.transform.GetChild(0);
        int total = root.childCount;
        if (total <= 1) return;

        int extra = (ScoreManager.linesCleared - metalStartLines) / Mathf.Max(1, linesPerExtraMetal);
        int count = Mathf.Clamp(1 + extra, 1, metalMaxPerPiece);
        count = Mathf.Min(count, total - 1);

        int[] idx = new int[total];
        for (int i = 0; i < total; i++) idx[i] = i;

        for (int i = 0; i < count; i++)
        {
            int j = Random.Range(i, total);
            int tmp = idx[i]; idx[i] = idx[j]; idx[j] = tmp;

            Transform b = root.GetChild(idx[i]);

            if (!b.TryGetComponent<MetallicBlock>(out _))
                b.gameObject.AddComponent<MetallicBlock>();

            if (b.TryGetComponent<SpriteRenderer>(out var sr))
                sr.sprite = metalSprite;

            if (metalAnimator != null)
            {
                if (!b.TryGetComponent<Animator>(out var anim))
                    anim = b.gameObject.AddComponent<Animator>();

                anim.runtimeAnimatorController = metalAnimator;
                anim.enabled = true;
            }
        }
    }

    void PickNext()
    {
        nextIndex = GetWeightedIndex();

        if (nextPiecePreview && tetrominosPreview != null && nextIndex < tetrominosPreview.Length)
        {
            foreach (Transform child in nextPiecePreview)
                Destroy(child.gameObject);

            var preview = Instantiate(tetrominosPreview[nextIndex], nextPiecePreview);
            preview.transform.localPosition = Vector3.zero;
            preview.transform.localRotation = Quaternion.identity;
            preview.transform.localScale = Vector3.one;
        }
    }

    // Sorteio ponderado com uma regra: peça de runa só sai se houver metal no tabuleiro.
    int GetWeightedIndex()
    {
        bool allowRune = GridManager.HasAnyMetal();
        int n = tetrominosGameplay.Length;
        bool useWeights = weights != null && weights.Length == n;

        float total = 0f;
        for (int i = 0; i < n; i++)
        {
            if (isRunePiece[i] && !allowRune) continue;
            total += useWeights ? Mathf.Max(0f, weights[i]) : 1f;
        }

        if (total <= 0f)
        {
            for (int i = 0; i < n; i++)
                if (!isRunePiece[i]) return i;

            return 0;
        }

        float r = Random.value * total;
        float acc = 0f;

        for (int i = 0; i < n; i++)
        {
            if (isRunePiece[i] && !allowRune) continue;

            acc += useWeights ? Mathf.Max(0f, weights[i]) : 1f;
            if (r <= acc) return i;
        }

        for (int i = n - 1; i >= 0; i--)
            if (!isRunePiece[i] || allowRune) return i;

        return 0;
    }
}