using UnityEngine;

public class AvalancheBlock : MonoBehaviour
{
    [Tooltip("Sprite do bloco já pousado, sem poeira")]
    public Sprite settledSprite;

    public void Land()
    {
        if (TryGetComponent<Animator>(out var anim))
            anim.enabled = false;

        if (settledSprite && TryGetComponent<SpriteRenderer>(out var sr))
            sr.sprite = settledSprite;
    }
}