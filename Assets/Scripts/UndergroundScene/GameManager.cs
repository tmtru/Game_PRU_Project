using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public Vector3 playerSpawnPosition;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); // Không bị phá hủy khi chuyển scene
		}
		else
		{
			Destroy(gameObject); // Singleton
		}
	}

	public void SetPlayerSpawn(Vector3 pos)
	{
		playerSpawnPosition = pos;
	}
}

