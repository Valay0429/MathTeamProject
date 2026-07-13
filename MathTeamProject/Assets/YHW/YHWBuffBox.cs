using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class YHWBuffBox : MonoBehaviour
{
    [SerializeField] private YHWBuffSO buffSO;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;

        if (buffSO != null)
            ApplyVisual();
    }

    public void SetBuff(YHWBuffSO so)
    {
        buffSO = so;
        ApplyVisual();
    }

    public YHWBuffSO GetBuff() => buffSO;

    private void ApplyVisual()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = buffSO.boxColor;

        if (buffSO.boxSprite != null)
            spriteRenderer.sprite = buffSO.boxSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (buffSO == null) return;

        IYHWBuffReceiver receiver = other.GetComponent<IYHWBuffReceiver>();
        if (receiver == null) return;

        receiver.ApplyBuff(buffSO);

        if (buffSO.pickupEffectPrefab != null)
            Instantiate(buffSO.pickupEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}