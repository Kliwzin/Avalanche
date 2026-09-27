using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominosGameplay;
    public Transform nextPiecePreview;

    [Header("Spawn Weights (mesmo tamanho do array de peças)")]
    [Min(0f)] public float[] weights;

    [Header("Preview")]
    [Tooltip("Maior dimensão da miniatura, em células")]
    public float previewMaxSize = 3f;
    [Tooltip("Limite de aumento: 1 = nunca maior que no jogo")]
    public float previewMaxScale = 1f;

    [Header("Metal parcial")]
    public Sprite metalSprite;
    public RuntimeAnimatorController metalAnimator;
    public int metalStartLines = 5;
    public int linesPerExtraMetal = 15;
    public int metalMaxPerPiece = 3;
    [Range(0f, 1f)] public float metalChance = 0.35f;

    [Header("Peça inteira de metal")]
    public int fullMetalStartLines = 60;
    [Range(0f, 1f)] public float fullMetalChance = 0.05f;

    int nextIndex;
    int[] nextMetalPlan;

    bool[] isRunePiece;
    bool[] isMetalPiece;

    void Start()
    {
        int n = tetrominosGameplay.Length;

        if (weights != null && weights.Length > 0 && weights.Length != n)
            Debug.LogWarning("[Spawner] 'weights' tem tamanho diferente de 'tetrominosGameplay'. Os pesos estão sendo ignorados.", this);

        isRunePiece = new bool[n];
        isMetalPiece = new bool[n];

        for (int i = 0; i < n; i++)
        {
            isRunePiece[i] = tetrominosGameplay[i].GetComponentInChildren<RuneBlock>(true) != null;
            isMetalPiece[i] = tetrominosGameplay[i].GetComponentInChildren<MetallicBlock>(true) != null;
        }

        PickNext();
        Spawn();
    }

    public void Spawn()
    {
        // O metal sumiu desde o sorteio: troca a runa por outra peça
        if (isRunePiece[nextIndex] && !GridManager.HasAnyMetal())
            PickNext();

        var piece = Instantiate(tetrominosGameplay[nextIndex], transform.position, Quaternion.identity);

        ApplyMetalPlan(piece, nextMetalPlan);

        var t = piece.GetComponent<Tetromino>();
        if (t != null)
        {
            t.Init(this);
            if (!t.ValidMove()) GameManager.Instance.GameOver();
        }

        PickNext();
    }

    void PickNext()
    {
        nextIndex = GetWeightedIndex();
        nextMetalPlan = PlanMetal(nextIndex);
        BuildPreview();
    }

    // Decide AGORA quais blocos serão de metal, para miniatura e peça combinarem
    int[] PlanMetal(int index)
    {
        if (metalSprite == null) return null;
        if (isRunePiece[index] || isMetalPiece[index]) return null;

        int lines = ScoreManager.linesCleared;
        if (lines < metalStartLines) return null;

        int total = tetrominosGameplay[index].transform.GetChild(0).childCount;
        if (total <= 1) return null;

        int count;

        if (lines >= fullMetalStartLines && Random.value < fullMetalChance)
        {
            count = total;                                   // peça inteira
        }
        else
        {
            if (Random.value > metalChance) return null;

            int extra = (lines - metalStartLines) / Mathf.Max(1, linesPerExtraMetal);
            count = Mathf.Min(1 + extra, metalMaxPerPiece, total - 1);
        }

        if (count <= 0) return null;

        int[] idx = new int[total];
        for (int i = 0; i < total; i++) idx[i] = i;

        for (int i = 0; i < count; i++)
        {
            int j = Random.Range(i, total);
            int tmp = idx[i]; idx[i] = idx[j]; idx[j] = tmp;
        }

        int[] plan = new int[count];
        System.Array.Copy(idx, plan, count);
        return plan;
    }

    void ApplyMetalPlan(GameObject piece, int[] plan)
    {
        if (plan == null) return;

        Transform root = piece.transform.GetChild(0);

        foreach (int i in plan)
            if (i < root.childCount) MakeMetal(root.GetChild(i));
    }

    void MakeMetal(Transform b)
    {
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

    // A miniatura é a própria peça de jogo, sem lógica, encolhida e centralizada
    void BuildPreview()
    {
        if (!nextPiecePreview) return;

        foreach (Transform child in nextPiecePreview)
            Destroy(child.gameObject);

        var preview = Instantiate(tetrominosGameplay[nextIndex], nextPiecePreview);

        // Desliga NA HORA: se deixar rodar um frame, ela tenta cair e entrar no grid
        if (preview.TryGetComponent<Tetromino>(out var t)) t.enabled = false;

        ApplyMetalPlan(preview, nextMetalPlan);

        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        FitPreview(preview.transform);

        CenterPreview(preview.transform);
    }

    void CenterPreview(Transform preview)
    {
        var renderers = preview.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        preview.position += nextPiecePreview.position - b.center;
    }

    // Escala a miniatura para caber na caixa e depois centraliza
    void FitPreview(Transform preview)
    {
        var renderers = preview.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return;

        // mede a peça no tamanho original
        preview.localScale = Vector3.one;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        float largest = Mathf.Max(b.size.x, b.size.y);
        float scale = largest > 0f ? previewMaxSize / largest : 1f;
        scale = Mathf.Min(scale, previewMaxScale);

        preview.localScale = Vector3.one * scale;

        // mede de novo, já escalada, e centraliza
        b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        preview.position += nextPiecePreview.position - b.center;
    }

    // Sorteio ponderado, com a regra de runa só aparecer se houver metal
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