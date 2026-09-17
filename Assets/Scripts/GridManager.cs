using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static int width = 10;
    public static int height = 20;

    public static Transform[,] grid = new Transform[width, height];

    public static bool isAnimating { get; private set; }

    public static Vector2 Round(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

    public static IEnumerator ResolveLinesAndFall(float stepDelay = 0.06f)
    {
        if (isAnimating) yield break;
        isAnimating = true;

        bool removedAny = false;

        for (int y = 0; y < height; y++)
        {
            if (!IsLineFull(y)) continue;

            DeleteLineKeepMetal(y);
            ScoreManager.AddLineScore();
            removedAny = true;
        }

        if (removedAny)
            yield return CollapseAnimated(stepDelay);

        isAnimating = false;
    }

    static bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (grid[x, y] == null) return false;
        return true;
    }

    static void DeleteLineKeepMetal(int y)
    {
        for (int x = 0; x < width; x++)
        {
            var t = grid[x, y];
            if (t == null) continue;

            if (t.GetComponent<MetallicBlock>() != null) continue;

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

                    if (t.GetComponent<MetallicBlock>() != null) continue;

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

            var sr = t.GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = newSprite;

            var anim = t.GetComponent<Animator>();
            if (anim) anim.enabled = false;

            var metal = t.GetComponent<MetallicBlock>();
            if (metal) UnityEngine.Object.Destroy(metal);

            var metalParent = t.GetComponentInParent<MetallicBlock>();
            if (metalParent) UnityEngine.Object.Destroy(metalParent);

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