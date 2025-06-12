using UnityEngine;
using UnityEngine.UI;

public class ColorCell : MonoBehaviour
{
	public Image image;
	public int colorIndex = 0;
	public Color[] colors;

	private void Start()
	{
		SetColor();
	}

	public void OnClick()
	{
		colorIndex = (colorIndex + 1) % colors.Length;
		SetColor();
		ColorManager.Instance.CheckWinCondition();
	}

	void SetColor()
	{
		image.color = colors[colorIndex];
	}
}
