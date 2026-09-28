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
    [Tooltip("Quanto da caixa a miniatura ocupa (0.7 = 70%, deixando margem)")]
    [Range(0.1f, 1f)] public float previewFill = 0.7f;

    [Header("Hold")]
    [Tooltip("Caixa onde a peça guardada aparece (opcional)")]
    public Transform holdPreview;
    [Tooltip("Treme quando o jogador tenta guardar duas vezes na mesma peça")]
    public UIPunch holdRefusedPunch;

    [Header("Metal parcial")]
    public Sprite metalSprite;
    public RuntimeAnimatorController metalAnimator;
    public int metalStartLines = 3;
    public int linesPerExtraMetal = 8;
    public int metalMaxPerPiece = 3;
    [Range(0f, 1f)] public float metalChance = 0.45f;

    [Header("Peça inteira de metal")]
    public int fullMetalStartLines = 30;
    [Range(0f, 1f)] public float fullMetalChance = 0.05f;

    [Header("Runas")]
    [Tooltip("Multiplicador do peso das runas quando a pressão está no máximo")]
    public float runeMaxBoost = 3f;
    [Tooltip("Com quantas colunas de metal o boost chega ao máximo")]
    public int runeBoostFullAt = 4;

    int nextIndex;
    int[] nextMetalPlan;

    int holdIndex = -1;
    int[] holdPlan;
    bool holdUsedThisPiece;

    Tetromino currentPiece;

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

        holdIndex = -1;
        holdPlan = null;
        BuildMiniature(holdPreview, -1, null);

        PickNext();
        Spawn();
    }

    public void Spawn()
    {
        SpawnPiece(nextIndex, nextMetalPlan);
        PickNext();
    }

    void SpawnPiece(int index, int[] plan)
    {
        var piece = Instantiate(tetrominosGameplay[index], transform.position, Quaternion.identity);

        ApplyMetalPlan(piece, plan);

        var t = piece.GetComponent<Tetromino>();
        if (t != null)
        {
            t.pieceIndex = index;
            t.metalPlan = plan;
            t.Init(this);

            currentPiece = t;

            if (!t.ValidMove() && GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }

        holdUsedThisPiece = false;
    }

    public bool TryHold()
    {
        if(holdUsedThisPiece)
        {
            if (holdRefusedPunch != null) holdRefusedPunch.Play();
            return false;
        }

        if (currentPiece == null) return false;

        int outIndex = currentPiece.pieceIndex;
        int[] outPlan = currentPiece.metalPlan;

        Destroy(currentPiece.gameObject);
        currentPiece = null;

        if (holdIndex < 0)
        {
            holdIndex = outIndex;
            holdPlan = outPlan;

            SpawnPiece(nextIndex, nextMetalPlan);
            PickNext();
        }
        else
        {
            int inIndex = holdIndex;
            int[] inPlan = holdPlan;

            holdIndex = outIndex;
            holdPlan = outPlan;

            SpawnPiece(inIndex, inPlan);
        }

        holdUsedThisPiece = true;
        BuildMiniature(holdPreview, holdIndex, holdPlan);

        return true;
    }

    void PickNext()
    {
        nextIndex = GetWeightedIndex();
        nextMetalPlan = PlanMetal(nextIndex);

        BuildMiniature(nextPiecePreview, nextIndex, nextMetalPlan);
    }

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
            count = total;
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

    // Serve as duas caixas: a da próxima peça e a da reserva.
    // index < 0 esvazia a caixa.
    void BuildMiniature(Transform box, int index, int[] plan)
    {
        if (!box) return;

        for (int i = box.childCount - 1; i >= 0; i--)
        {
            Transform velho = box.GetChild(i);
            velho.SetParent(null);
            Destroy(velho.gameObject);
        }

        if (index < 0) return;

        var mini = Instantiate(tetrominosGameplay[index], box);

        if (mini.TryGetComponent<Tetromino>(out var t)) t.enabled = false;

        ApplyMetalPlan(mini, plan);

        mini.transform.localPosition = Vector3.zero;
        mini.transform.localRotation = Quaternion.identity;

        FitPreview(mini.transform, box);
    }

    void FitPreview(Transform preview, Transform box)
    {
        var renderers = preview.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return;

        // mede a peça no tamanho original
        preview.localScale = Vector3.one;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        // Espaço disponível: tirado da própria arte da caixa.
        // Assim, mexer no tamanho da caixa ajusta a miniatura sozinho.
        var boxArt = box.GetComponentInParent<SpriteRenderer>();

        float available = boxArt != null
            ? Mathf.Min(boxArt.bounds.size.x, boxArt.bounds.size.y) * previewFill
            : previewMaxSize;

        float largest = Mathf.Max(b.size.x, b.size.y);
        float scale = largest > 0f ? available / largest : 1f;
        scale = Mathf.Min(scale, previewMaxScale);

        preview.localScale = Vector3.one * scale;

        b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        Vector3 delta = box.position - b.center;
        preview.position += new Vector3(delta.x, delta.y, 0f);

        if (boxArt != null)
        {
            foreach (var r in renderers)
            {
                r.sortingLayerID = boxArt.sortingLayerID;
                r.sortingOrder = boxArt.sortingOrder + 1;
            }
        }
    }

    float WeightOf(int i, bool useWeights, float runeBoost)
    {
        float w = useWeights ? Mathf.Max(0f, weights[i]) : 1f;
        return isRunePiece[i] ? w * runeBoost : w;
    }

    int GetWeightedIndex()
    {
        int n = tetrominosGameplay.Length;
        bool useWeights = weights != null && weights.Length == n;

        int metalColumns = GridManager.MetalColumnCount();
        bool allowRune = metalColumns > 0;

        float pressure = Mathf.Clamp01(metalColumns / (float)Mathf.Max(1, runeBoostFullAt));
        float runeBoost = Mathf.Lerp(1f, runeMaxBoost, pressure);

        float total = 0f;
        for (int i = 0; i < n; i++)
        {
            if (isRunePiece[i] && !allowRune) continue;
            total += WeightOf(i, useWeights, runeBoost);
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

            acc += WeightOf(i, useWeights, runeBoost);
            if (r <= acc) return i;
        }

        for (int i = n - 1; i >= 0; i--)
            if (!isRunePiece[i] || allowRune) return i;

        return 0;
    }
}