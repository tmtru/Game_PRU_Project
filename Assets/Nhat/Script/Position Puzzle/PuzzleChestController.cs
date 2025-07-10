using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PuzzleChestController : MonoBehaviour
{
	public GameObject keypadPanel;
	public Animator chestAnimator;
	public GameObject closeButton;
	public BoxCollider2D boxCollider;

	[SerializeField] private PuzzleManager puzzleManager;

	public GameObject itemPrefab;
	public Transform spawnPoint;
	public AudioSource chestOpening;
	private bool solving = false;
	public bool isChestOpen = false;
	private void Start()
	{
		if (boxCollider == null)
		{
			Transform chestTransform = transform.parent.Find("Chest_2");
			if (chestTransform != null)
			{
				boxCollider = chestTransform.GetComponent<BoxCollider2D>();
			}
		}
	}
	private void Update()
	{
		if (!isChestOpen && !solving && puzzleManager.CheckPuzzleSolved())
		{
			StartCoroutine(DelaySolve());
			solving = true;
		}
	}
	public void Solve()
	{
		chestAnimator.SetTrigger("PlayAnim");
		isChestOpen = true;
		keypadPanel.SetActive(false);
		closeButton.SetActive(false);
		chestOpening.Play();
		Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
		Time.timeScale = 1f;
		if (boxCollider != null)
			boxCollider.isTrigger = false;
	}

	public void OnClosePanel()
	{
		closeButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}
	IEnumerator DelaySolve()
	{                 
		yield return new WaitForSecondsRealtime(1f);
		Solve();
	}
}