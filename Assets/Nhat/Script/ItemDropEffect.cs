using UnityEngine;
using DG.Tweening;

public class ItemDropEffect : MonoBehaviour
{
	public float jumpPower = 1f;
	public float duration = 2f;
	public ParticleSystem glowEffect; // Gắn hiệu ứng phát sáng nếu có

	void Start()
	{
		Vector3 jumpTarget = transform.position + new Vector3(0, 1.5f, 0);

		// Di chuyển bay lên rồi rơi xuống
		transform.DOJump(jumpTarget, jumpPower, 1, duration)
			.SetEase(Ease.OutQuad)
			.OnComplete(() =>
			{
				Debug.Log("Hoàn tất hiệu ứng nhảy vật phẩm!");
			});

		// (Optional) scale to lên nhẹ
		transform.localScale = Vector3.zero;
		transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
	}
	private bool landed = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!landed && collision.gameObject.CompareTag("Ground"))
        {
            landed = true;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
			rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; // Dừng hoàn toàn
			if (glowEffect != null)
			{
				Debug.Log("🌟 Đã gọi glowEffect.Play()");
				glowEffect.Play();
			}
			else
			{
				Debug.LogWarning("⚠️ glowEffect chưa được gán trong Inspector!");
			}
			Debug.Log("📦 Vật phẩm đã chạm đất và đứng yên");
        }
    }
}
