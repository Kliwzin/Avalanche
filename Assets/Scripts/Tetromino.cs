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

    [Header("Movimento lateral")]
    [Tooltip("Espera antes de a tecla segurada começar a repetir")]
    [SerializeField] float dasDelay = 0.17f;
    [Tooltip("Intervalo entre repetições depois que começou")]
    [SerializeField] float dasRepeat = 0.05f;

    // Deslocamentos testados na rotação, em ordem de preferência.
    static readonly Vector3[] kicks =
    {
        Vector3.zero,               // no lugar
        Vector3.right,              // empurra 1 para a direita
        Vector3.left,               // empurra 1 para a esquerda
        Vector3.right * 2f,         // 2 para a direita (peça I na parede)
        Vector3.left * 2f,          // 2 para a esquerda
        Vector3.up,                 // floor kick
        Vector3.up + Vector3.right,
        Vector3.up + Vector3.left,
    };

    float previousTime;
    bool hardDropping;

    float horizontalTimer;
    int lastDirection;

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

        HandleHorizontal();

        if (Input.GetKeyDown(KeyCode.UpArrow)) TryRotate();

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

    void HandleHorizontal()
    {
        int dir = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) dir -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) dir += 1;

        // Nenhuma tecla, ou as duas ao mesmo tempo: não anda
        if (dir == 0)
        {
            lastDirection = 0;
            return;
        }

        // Primeiro toque nessa direção: move na hora e arma a espera
        if (dir != lastDirection)
        {
            lastDirection = dir;
            horizontalTimer = dasDelay;
            TryMove(Vector3.right * dir);
            return;
        }

        // Tecla continua segurada: repete depois da espera
        horizontalTimer -= Time.deltaTime;

        if (horizontalTimer <= 0f)
        {
            horizontalTimer = dasRepeat;
            TryMove(Vector3.right * dir);
        }
    }

    void TryRotate()
    {
        Vector3 original = transform.position;

        transform.Rotate(0, 0, 90);

        foreach (Vector3 kick in kicks)
        {
            transform.position = original + kick;
            if (ValidMove()) return;
        }

        // Nenhum deslocamento serviu: desfaz a rotação
        transform.position = original;
        transform.Rotate(0, 0, -90);
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
            Vector2 pos = GridManager.Round(blocksRoot.GetChild(i).position);

            if (pos.y >= GridManager.height)
            {
                if (GameManager.Instance != null) GameManager.Instance.GameOver();
                return false;
            }

            if (pos.x < 0 || pos.x >= GridManager.width || pos.y < 0)
            {
                Debug.LogError($"[Tetromino] Bloco fora do tabuleiro em {pos} na peça {name}.", this);
                return false;
            }
        }

        for (int i = 0; i < blocksRoot.childCount; i++)
        {
            Transform child = blocksRoot.GetChild(i);
            Vector2 pos = GridManager.Round(child.position);
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