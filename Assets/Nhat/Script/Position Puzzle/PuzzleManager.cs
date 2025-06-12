using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
	public RectTransform[] pieces;  // puzzles
	public int[] correctOrder = { 1, 2, 3, 4 };  // Win order

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
				//Debug.LogError($"piece {pieceNumber} - wrongOrder: {correctOrder[i]}");
				return false;
			}
		}
		return true;
	}

	public void ShufflePieces()
	{
		for (int i = 0; i < pieces.Length; i++)
		{
			int randomIndex = Random.Range(i, pieces.Length);
			var temp = pieces[i];
			pieces[i] = pieces[randomIndex];
			pieces[randomIndex] = temp;
		}

		// Cập nhật vị trí con (nếu dùng Grid Layout Group)
		for (int i = 0; i < pieces.Length; i++)
		{
			pieces[i].SetSiblingIndex(i);
		}
	}
}