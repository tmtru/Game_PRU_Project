using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
	public static ColorManager Instance;
	public List<ColorCell> cells;

	public GameObject closeButton;
	public GameObject keypadPanel;

	[SerializeField]
	private List<int> targetColorIndexes = new List<int> { 1, 0, 1, 1, 2, 0, 0, 1, 2 }; // green 1, yellow 2, red 0
	public bool isChestOpen = false;
	private void Awake()
	{
		if (Instance == null) Instance = this;
	}

	public void CheckWinCondition()
	{
		if (cells.Count == 0 || targetColorIndexes.Count != cells.Count)
		{
			Debug.LogWarning($"cell count: {cells.Count}");
			Debug.LogWarning($"targetColorIndexes.Count: {targetColorIndexes.Count}");
			Debug.LogWarning("Thiếu dữ liệu màu mục tiêu hoặc số lượng không khớp.");
			return;
		}

		int targetColor = cells[0].colorIndex;

		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].colorIndex != targetColorIndexes[i])
			{
				return;
			}
		}
		isChestOpen = true;
		Debug.Log("🎉 Bạn đã thắng!");
	}
	public void OnClosePanel()
	{
		closeButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}
}
