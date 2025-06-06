using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortingOrder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int offset = 0;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Lower Y = closer to camera = should be in front
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-(transform.position.y * 100)) + offset;
    }
}
