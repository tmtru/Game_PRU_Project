using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
	[SerializeField] private StoryUIManager storyUI;

	public bool autoClose = true;
	public float duration = 3f;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && storyUI != null)
		{
			if (autoClose)
			{
				storyUI.ShowTimedStory(duration);
			}
			else
			{
				storyUI.ShowManualStory();
			}
		}
	}
}
