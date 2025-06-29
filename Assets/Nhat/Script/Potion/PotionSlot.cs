using UnityEngine;
using UnityEngine.UI;
public class PotionSlot : MonoBehaviour
{
	public Image hinhAnhUI;
	public string ma;
	public Sprite backGround;
	public void DatThuoc(Potion thuoc)
	{
		ma = thuoc.ma;

		if (hinhAnhUI != null)
		{
			hinhAnhUI.sprite = thuoc.hinhAnh;
			Color mau = thuoc.mauSac;
			if (mau.a == 0) mau.a = 1f;
			hinhAnhUI.color = mau;
		}
	}

	public void XoaThuoc()
	{
		ma = "";

		if (hinhAnhUI != null)
		{
			hinhAnhUI.sprite = backGround;
		}
	}
}
