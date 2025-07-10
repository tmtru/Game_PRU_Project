using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private RectTransform rectTransform;
	private CanvasGroup canvasGroup;
	private Vector2 originalPosition;
	private Canvas canvas;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvas = GetComponentInParent<Canvas>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		originalPosition = rectTransform.anchoredPosition;
		canvasGroup.blocksRaycasts = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		canvasGroup.blocksRaycasts = true;

		// T?m m?nh khác ðang ðý?c th? lên
		PuzzlePiece targetPiece = null;

		foreach (PuzzlePiece piece in Object.FindObjectsByType<PuzzlePiece>(FindObjectsSortMode.None))
		{
			if (piece == this) continue;

			if (RectTransformUtility.RectangleContainsScreenPoint(
				piece.rectTransform, Mouse.current.position.ReadValue(), eventData.pressEventCamera))
			{
				targetPiece = piece;
				break;
			}
		}

		if (targetPiece != null)
		{
			// Swap v? trí hi?n th?
			Vector2 tempPos = targetPiece.rectTransform.anchoredPosition;
			targetPiece.rectTransform.anchoredPosition = originalPosition;
			rectTransform.anchoredPosition = tempPos;

			// Swap sibling index ð? c?p nh?t th? t? trong Hierarchy
			int thisIndex = rectTransform.GetSiblingIndex();
			int targetIndex = targetPiece.rectTransform.GetSiblingIndex();
			rectTransform.SetSiblingIndex(targetIndex);
			targetPiece.rectTransform.SetSiblingIndex(thisIndex);
		}
		else
		{
			// Tr? v? v? trí ban ð?u
			rectTransform.anchoredPosition = originalPosition;
		}
	}
}