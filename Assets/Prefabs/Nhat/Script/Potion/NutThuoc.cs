using UnityEngine;

public class NutThuoc : MonoBehaviour
{
	public Potion thuoc;
	public PhatronManager quanLy;

	public void KhiClick()
	{
		Debug.Log("Da click vao potion");
		quanLy.ThemLuaChon(thuoc);
	}
}
