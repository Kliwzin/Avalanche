using UnityEngine;
using System.Collections;

public class Tetromino : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField] float startFallTime = 1f;
    [SerializeField] float minFallTime = 0.2f;
    [SerializeField] float scoreToMaxSpeed = 3000f;

    [Header("Hard Drop")]
    [SerializeField] float hardDropStepDelay = 0.02f;

    [Header("Line Fall Visual")]
    [SerializeField] float lineFallStepDelay = 0.06f;

    float previousTime;
    bool hardDropping;

    Spawner spawner;

    public void Init(Spawner s) => spawner = s;

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (GridManager.isAnimating) return;
        if (hardDropping) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            previousTime = Time.time;
            StartCoroutine(HardDropVisual());
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector3.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector3.right);

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0, 0, 90);
            if (!ValidMove()) transform.Rotate(0, 0, -90);
        }

        float fall = GetCurrentFallTime();
        float currentFall = Input.GetKey(KeyCode.DownArrow) ? fall / 10f : fall;

        if (Time.time - previousTime > currentFall)
        {
            transform.position += Vector3.down;

            if (!ValidMove())
            {
                transform.position += Vector3.up;
                LockAndResolve();
                return;
            }

            previousTime = Time.time;
        }
    }

    void TryMove(Vector3 delta)
    {
        transform.position += delta;
        if (!ValidMove()) transform.position -= delta;
    }

    public bool ValidMove()
    {
        Transform blocksRoot = transform.GetChild(0);

        for (int i = 0; i < blocksRoot.childCount; i++)
        {
            Transform child = blocksRoot.GetChild(i);
            Vector2 pos = GridManager.Round(child.position);

            if (pos.x < 0 || pos.x >= GridManager.width) return false;
            if (pos.y < 0) return false;

            if (pos.y < GridManager.height && GridManager.grid[(int)pos.x, (int)pos.y] != null)
                return false;
        }
        return true;
    }

    void LockAndResolve()
    {
        if (!AddToGrid()) { enabled = false; return; }

        RuneBlock rune = GetComponentInChildren<RuneBlock>(true);

        DetachBlocksFromPiece();

        if (rune != null)
        {
            int colX = (int)GridManager.Round(rune.transform.position).x;
            StartCoroutine(RuneFlow(rune, colX));
        }
        else
        {
            StartCoroutine(NormalFlow());
        }

        enabled = false;
    }

    bool AddToGrid()
    {
        Transform blocksRoot = transform.GetChild(0);

        for (int i = 0; i < blocksRoot.childCount; i++)
        {
            Transform child = blocksRoot.GetChild(i);
            Vector2 pos = GridManager.Round(child.position);

            if (pos.x < 0 || pos.x >= GridManager.width || pos.y < 0)
                return false;

            if (pos.y >= GridManager.height)
            {
                GameManager.Instance.GameOver();
                return false;
            }

            GridManager.grid[(int)pos.x, (int)pos.y] = child;
        }
        return true;
    }

    void DetachBlocksFromPiece()
    {
        Transform root = transform.GetChild(0);
        for (int i = root.childCount - 1; i >= 0; i--)
            root.GetChild(i).SetParent(null, true);
    }

    float GetCurrentFallTime()
    {
        float t = Mathf.Clamp01((float)ScoreManager.score / scoreToMaxSpeed);
        t *= t;
        return Mathf.Lerp(startFallTime, minFallTime, t);
    }

    IEnumerator NormalFlow()
    {
        yield return GridManager.ResolveLinesAndFall(lineFallStepDelay);
        spawner.Spawn();
        Destroy(gameObject);
    }

    IEnumerator RuneFlow(RuneBlock rune, int colX)
    {
        yield return rune.Activate(colX);
        yield return GridManager.ResolveLinesAndFall(lineFallStepDelay);
        spawner.Spawn();
        Destroy(gameObject);
    }

    IEnumerator HardDropVisual()
    {
        hardDropping = true;

        while (true)
        {
            transform.position += Vector3.down;

            if (!ValidMove())
            {
                transform.position += Vector3.up;
                LockAndResolve();
                yield break;
            }

            ScoreManager.AddDropScore();

            if (hardDropStepDelay <= 0f) yield return null;
            else yield return new WaitForSeconds(hardDropStepDelay);
        }
    }
}