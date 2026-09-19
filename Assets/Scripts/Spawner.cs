using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominosGameplay;
    public GameObject[] tetrominosPreview;
    public Transform nextPiecePreview;

    [Header("Spawn Weights (mesmo tamanho dos arrays)")]
    [Min(0f)] public float[] weights;

    int nextIndex;

    void Start()
    {
        if (weights != null && weights.Length > 0 && weights.Length != tetrominosGameplay.Length)
            Debug.LogWarning("[Spawner] 'weights' tem tamanho diferente de 'tetrominosGameplay'. Os pesos estão sendo ignorados.", this);

        PickNext();
        Spawn();
    }

    public void Spawn()
    {
        var piece = Instantiate(tetrominosGameplay[nextIndex], transform.position, Quaternion.identity);

        var t = piece.GetComponent<Tetromino>();
        if (t != null)
        {
            t.Init(this);
            if (!t.ValidMove())
                GameManager.Instance.GameOver();
        }

        PickNext();
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

    int GetWeightedIndex()
    {
        if (weights == null || weights.Length != tetrominosGameplay.Length)
            return Random.Range(0, tetrominosGameplay.Length);

        float total = 0f;
        for (int i = 0; i < weights.Length; i++)
            total += Mathf.Max(0f, weights[i]);

        if (total <= 0f)
            return Random.Range(0, tetrominosGameplay.Length);

        float r = Random.value * total;
        float acc = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            acc += Mathf.Max(0f, weights[i]);
            if (r <= acc) return i;
        }

        return weights.Length - 1;
    }
}