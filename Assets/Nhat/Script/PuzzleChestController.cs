using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PuzzleChestController : MonoBehaviour
{
	public GameObject keypadPanel;
	//public Animator chestAnimator;
	public GameObject closeButton;

	[SerializeField] private PuzzleManager puzzleManager; // <- thêm SerializeField để kéo thả script này

	public bool isChestOpen = false;

	private void Update()
	{
		if (!isChestOpen && puzzleManager.CheckPuzzleSolved())
		{
			//chestAnimator.SetTrigger("ChestOpen");
			isChestOpen = true;
			keypadPanel.SetActive(false);
			closeButton.SetActive(false);
			Time.timeScale = 1f;
		}
		//else
		//{
		//	Debug.LogError($"Wrong order");
		//}

	}

	public void OnClosePanel()
	{
		if (isChestOpen)
		{
			closeButton.SetActive(false);
			keypadPanel.SetActive(false);
			Time.timeScale = 1f;
			return;
		}
		closeButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}
}