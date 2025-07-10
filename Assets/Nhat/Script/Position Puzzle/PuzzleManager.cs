using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
	public RectTransform[] pieces;  // puzzles
	public int[] correctOrder = { 1, 4,2,3,9,8,5,7,6};  // Win order
	public bool CheckPuzzleSolved()
	{
		if (pieces.Length != correctOrder.Length)
		{
			Debug.LogError($"pieces.Length ({pieces.Length}) != correctOrder.Length ({correctOrder.Length})");
			return false;
		}

		// Cập nhật danh sách pieces[] dựa trên thứ tự sibling
		for (int i = 0; i < pieces.Length; i++)
		{
			pieces[i] = pieces[0].parent.GetChild(i).GetComponent<RectTransform>();
		}

		for (int i = 0; i < pieces.Length; i++)
		{
			int pieceNumber;
			// Dùng regex để tách số cuối cùng
			var match = Regex.Match(pieces[i].name, @"\d+");
			if (!match.Success || !int.TryParse(match.Value, out pieceNumber))
			{
				Debug.LogError($"Không thể parse name '{pieces[i].name}' thành số nguyên.");
				return false;
			}

			if (pieceNumber != correctOrder[i])
			{
				Debug.LogError($"❌ Sai tại vị trí {i}: hiện là Image{pieceNumber}, đúng phải là Image{correctOrder[i]}");
				// 1 3 4
				return false;
			}
		}
		Debug.LogError($"Win");
		return true;
	}
	
}