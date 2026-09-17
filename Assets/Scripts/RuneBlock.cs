using System.Collections;
using UnityEngine;

public class RuneBlock : MonoBehaviour
{
    [Header("Result")]
    public Sprite columnResultSprite;

    [Header("FX")]
    public GameObject beamCellPrefab;
    public float beamLifeTime = 0.4f;

    [Header("Animation")]
    public Animator animator;
    public string explodeTrigger = "Explode";
    public float paintDelay = 0.12f;

    bool used;

    public IEnumerator Activate(int columnX)
    {
        if (used) yield break;
        used = true;

        if (animator) animator.SetTrigger(explodeTrigger);

        GridManager.SpawnBeamColumn(columnX, beamCellPrefab, beamLifeTime);

        yield return new WaitForSeconds(paintDelay);

        if (columnResultSprite)
            GridManager.PaintColumnAndConvertMetal(columnX, columnResultSprite);

        Destroy(gameObject);
    }
}