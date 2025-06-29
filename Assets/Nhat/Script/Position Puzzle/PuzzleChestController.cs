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
		if (!isChestOpen && puzzleManager.CheckPuzzleSolved())
		{
			chestAnimator.SetTrigger("PlayAnim");
			isChestOpen = true;
			keypadPanel.SetActive(false);
			closeButton.SetActive(false);
			Time.timeScale = 1f;
			if (boxCollider != null)
				boxCollider.isTrigger = false;
		}

	}

	public void OnClosePanel()
	{
		closeButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}
}