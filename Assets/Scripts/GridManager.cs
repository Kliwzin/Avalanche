using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public const int width = 10;
    public const int height = 20;

    public static Transform[,] grid = new Transform[width, height];

    public static bool isAnimating { get; private set; }

    public static Vector2 Round(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

    public static bool IsMetal(Transform t)
    {
        return t != null
            && t.TryGetComponent<MetallicBlock>(out var m)
            && m.enabled;
    }

    public static bool HasAnyMetal()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (IsMetal(grid[x, y])) return true;

        return false;
    }

    public static void RemoveFromGrid(Transform t)
    {
        if (t == null) return;

        Vector2 p = Round(t.position);
        int x = (int)p.x;
        int y = (int)p.y;

        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (grid[x, y] == t) grid[x, y] = null;
    }

    public static int TopOfColumn(int x)
    {
        for (int y = height - 1; y >= 0; y--)
            if (grid[x, y] != null) return y;

        return -1;
    }

    public static void ResetGrid()
    {
        grid = new Transform[width, height];
        isAnimating = false;
    }

    public static IEnumerator ResolveLinesAndFall(float stepDelay = 0.06f)
    {
        if (isAnimating) yield break;
        isAnimating = true;

        int chain = 0;

        while (chain < 30)
        {
            int linesThisPass = 0;

            for (int y = 0; y < height; y++)
            {
                if (!IsLineFull(y)) continue;

                DeleteLine(y);
                linesThisPass++;
            }

            if (linesThisPass == 0) break;

            chain++;
            ScoreManager.AddLineScore(linesThisPass, chain);

            yield return CollapseAnimated(stepDelay);
        }

        isAnimating = false;
    }

    public static IEnumerator DropAvalancheRow(GameObject blockPrefab, float fallTime = 0.35f, float stagger = 0.03f)
    {
        if (blockPrefab == null) yield break;

        // 1ª passada: só confere se cabe. Mesmo padrão do AddToGrid.
        int[] targets = new int[width];
        for (int x = 0; x < width; x++)
        {
            targets[x] = TopOfColumn(x) + 1;

            if (targets[x] >= height)
            {
                if (GameManager.Instance != null) GameManager.Instance.GameOver();
                yield break;
            }
        }

        isAnimating = true;

        // 2ª passada: cria os blocos acima do tabuleiro e já registra no grid
        float startY = height + 1;
        Transform[] blocks = new Transform[width];

        for (int x = 0; x < width; x++)
        {
            var go = UnityEngine.Object.Instantiate(blockPrefab, new Vector3(x, startY, 0f), Quaternion.identity);
            blocks[x] = go.transform;
            grid[x, targets[x]] = go.transform;
        }

        // Anima todos caindo, cada coluna começando um pouco depois da anterior
        float total = fallTime + (width - 1) * stagger;
        float t = 0f;

        while (t < total)
        {
            t += Time.deltaTime;

            for (int x = 0; x < width; x++)
            {
                float k = Mathf.Clamp01((t - x * stagger) / fallTime);
                k *= k;

                blocks[x].position = new Vector3(x, Mathf.Lerp(startY, targets[x], k), 0f);
            }

            yield return null;
        }

        for (int x = 0; x < width; x++)
            blocks[x].position = new Vector3(x, targets[x], 0f);

        for (int x = 0; x < width; x++)
            if (blocks[x].TryGetComponent<Animator>(out var anim)) anim.enabled = false;

        isAnimating = false;
    }

    static bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Transform t = grid[x, y];

            if (t == null) return false;
            if (IsMetal(t)) return false;
        }
        return true;
    }

    static void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Transform t = grid[x, y];
            if (t == null) continue;

            UnityEngine.Object.Destroy(t.gameObject);
            grid[x, y] = null;
        }
    }

    static IEnumerator CollapseAnimated(float stepDelay)
    {
        bool moved;

        do
        {
            moved = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    Transform t = grid[x, y];
                    if (t == null) continue;
                    if (IsMetal(t)) continue;

                    if (grid[x, y - 1] == null)
                    {
                        grid[x, y - 1] = t;
                        grid[x, y] = null;

                        t.position += Vector3.down;
                        moved = true;
                    }
                }
            }

            if (moved)
            {
                if (stepDelay <= 0f) yield return null;
                else yield return new WaitForSeconds(stepDelay);
            }

        } while (moved);
    }

    public static void PaintColumnAndConvertMetal(int x, Sprite newSprite)
    {
        if (x < 0 || x >= width) return;

        for (int y = 0; y < height; y++)
        {
            Transform t = grid[x, y];
            if (t == null) continue;

            if (t.TryGetComponent<SpriteRenderer>(out var sr)) sr.sprite = newSprite;
            if (t.TryGetComponent<Animator>(out var anim)) anim.enabled = false;
            if (t.TryGetComponent<MetallicBlock>(out var metal))
            {
                metal.enabled = false;
                UnityEngine.Object.Destroy(metal);
            }

            t.localScale = Vector3.one;
        }
    }

    public static void SpawnBeamColumn(int x, GameObject beamCellPrefab, float lifeTime = 0.4f)
    {
        if (!beamCellPrefab) return;
        if (x < 0 || x >= width) return;

        for (int y = 0; y < height; y++)
        {
            Vector3 pos = new Vector3(x, y + 1, -1f);
            var fx = UnityEngine.Object.Instantiate(beamCellPrefab, pos, Quaternion.identity);
            UnityEngine.Object.Destroy(fx, lifeTime);
        }
    }
}