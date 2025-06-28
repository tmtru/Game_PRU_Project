using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryUIManager : MonoBehaviour
{
	public GameObject fullScreenPanel;
	public GameObject closeButton;

	private void Start()
	{
		if (closeButton != null)
			closeButton.SetActive(false);
	}

	/// <summary>
	/// Hiển thị message và tự tắt sau vài giây.
	/// </summary>
	public void ShowTimedStory( float duration)
	{
		if (closeButton != null) closeButton.SetActive(false);

		StartCoroutine(ShowAndHide(duration));
	}

	/// <summary>
	/// Hiển thị message và chờ người dùng bấm nút Close.
	/// </summary>
	public void ShowManualStory()
	{
		if (closeButton != null) closeButton.SetActive(true);

		fullScreenPanel.SetActive(true);
		Time.timeScale = 0f;
	}

	/// <summary>
	/// Đóng story (gọi từ nút X).
	/// </summary>
	public void CloseStory()
	{
		fullScreenPanel.SetActive(false);
		Time.timeScale = 1f;
		if (closeButton != null) closeButton.SetActive(false);
	}

	private IEnumerator ShowAndHide(float duration)
	{
		fullScreenPanel.SetActive(true);

		yield return new WaitForSecondsRealtime(duration);
		CloseStory();
	}
}
