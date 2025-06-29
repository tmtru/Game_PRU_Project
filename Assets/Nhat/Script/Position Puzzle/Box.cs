using UnityEngine;

public class Box : MonoBehaviour
{
	private BoxCollider boxCollider;
	private bool hasGameWon = false;

	void Start()
	{
		boxCollider = GetComponent<BoxCollider>();
		boxCollider.isTrigger = false; // Ban �?u ch?n player
	}

	[System.Obsolete]
	private void OnCollisionEnter(Collision collision)
	{
		if (hasGameWon) return;

		if (collision.gameObject.CompareTag("Player"))
		{
			Debug.Log("Player ch?m v�o Box - Win!");

			hasGameWon = true;
			boxCollider.isTrigger = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!hasGameWon) return;

		if (other.CompareTag("Player"))
		{
			Debug.Log("Player �i xuy�n qua Box sau khi win");
		}
	}
}
