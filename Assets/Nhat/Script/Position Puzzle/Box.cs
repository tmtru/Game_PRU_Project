using UnityEngine;

public class Box : MonoBehaviour
{
	private BoxCollider boxCollider;
	private bool hasGameWon = false;

	void Start()
	{
		boxCollider = GetComponent<BoxCollider>();
		boxCollider.isTrigger = false; // Ban ð?u ch?n player
	}

	[System.Obsolete]
	private void OnCollisionEnter(Collision collision)
	{
		if (hasGameWon) return;

		if (collision.gameObject.CompareTag("Player"))
		{
			Debug.Log("Player ch?m vào Box - Win!");

			hasGameWon = true;
			boxCollider.isTrigger = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!hasGameWon) return;

		if (other.CompareTag("Player"))
		{
			Debug.Log("Player ði xuyên qua Box sau khi win");
		}
	}
}
