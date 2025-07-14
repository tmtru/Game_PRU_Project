using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public string targetSceneName; // Tên scene sẽ áp dụng teleport
	public Vector3 playerSpawnPosition;

	public void SetPlayerSpawn(Vector3 pos, string sceneName)
	{
		playerSpawnPosition = pos;
		targetSceneName = sceneName;
	}

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
}

