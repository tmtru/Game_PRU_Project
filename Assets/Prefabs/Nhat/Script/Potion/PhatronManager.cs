using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhatronManager : MonoBehaviour
{
	public GameObject closeButton;
	public GameObject mixButton;
	public GameObject keypadPanel;
	public bool isChestOpen = false;

	public PotionSlot[] oLuaChon;
	public GameObject bangGoiY;
	public GameObject ruong;
	public GameObject hieuUngThanhCong;
	public GameObject hieuUngThatBai;
	public AudioSource amThanhThanhCong;
	public AudioSource amThanhThatBai;

	public GameObject itemPrefab;         // Prefab vật phẩm
	public Transform spawnPoint;
	public BoxCollider2D boxCollider;
	public Animator chestAnimator;
	// Danh sách tổ hợp đúng
	private readonly List<string[]> toHopDung = new List<string[]> {
		new string[] { "Y", "F", "G" }
	};

	// R, Y, Pu, Pi, F, G, B
	private int demLuaChon = 0;

	public void ThemLuaChon(Potion thuoc)
	{
		if (demLuaChon >= 3) return;
		oLuaChon[demLuaChon].DatThuoc(thuoc);
		demLuaChon++;
	}



	public void NutPhaTron()
	{
		Debug.Log("Pha thuốc");

		if (demLuaChon < 3) return;

		List<string> maDaChon = new List<string>();
		foreach (var o in oLuaChon)
			maDaChon.Add(o.ma);

		maDaChon.Sort();

		foreach (var toHop in toHopDung)
		{
			var sapXep = new List<string>(toHop);
			sapXep.Sort();

			if (sapXep.Count == maDaChon.Count && !sapXep.Except(maDaChon).Any())
			{
				ThanhCong();
				return;
			}
		}

		ThatBai();
	}

	void ThanhCong()
	{
		
		isChestOpen = true;
		if (hieuUngThanhCong) hieuUngThanhCong.SetActive(true);
		if (amThanhThanhCong) amThanhThanhCong.Play();
		OnClosePanel();
		chestAnimator.SetTrigger("PlayAnim");
		Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
		boxCollider.isTrigger = false;


	}

	void ThatBai()
	{
		Debug.Log("Sai tổ hợp!");
		if (hieuUngThatBai) hieuUngThatBai.SetActive(true);
		if (amThanhThatBai) amThanhThatBai.Play();
		NutClear();
	}
	public void NutClear()
	{
		foreach (var o in oLuaChon) o.XoaThuoc();
		demLuaChon = 0;
	}
	public void BatTatBangGoiY()
	{
		bangGoiY.SetActive(!bangGoiY.activeSelf);
	}

	public void OnClosePanel()
	{
		NutClear();
		closeButton.SetActive(false);
		mixButton.SetActive(false);
		keypadPanel.SetActive(false);
		Time.timeScale = 1f;
	}
}
