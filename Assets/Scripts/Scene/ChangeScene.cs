using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;
    public float countdown = 10f;
    public string scene1 = "Scene1";
    public string scene2 = "Scene2";

    [Header("Day Counter Settings")]
    [SerializeField] private bool resetDayOnGameStart = false; // Checkbox để chọn có reset hay không
    [SerializeField] private bool saveDayCountPermanently = true; // Có lưu vĩnh viễn không

    private float timer;
    public float TimeLeft => timer;
    private bool isFading = false;

    // Biến đếm ngày
    [SerializeField] private int dayCount = 0;
    [SerializeField] private bool hasCompletedCycle = false; // Kiểm tra xem đã hoàn thành 1 chu kỳ scene1->scene2 chưa
    [SerializeField] private string currentSceneName = ""; // Hiển thị scene hiện tại

    public int DayCount => dayCount;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load số ngày đã lưu từ PlayerPrefs
        LoadDayCount();
    }

    void Start()
    {
        // Kiểm tra có reset mỗi lần khởi động không
        if (resetDayOnGameStart)
        {
            ResetDayCount();
            Debug.Log("Đã reset số ngày khi khởi động game");
        }

        timer = countdown;
        SetFadeAlpha(1f);
        StartCoroutine(FadeIn());

        // Debug để xem số ngày hiện tại
        Debug.Log("Ngày hiện tại: " + dayCount);
    }

    void Update()
    {
        if (isFading) return;

        // Cập nhật tên scene hiện tại để hiển thị trong Inspector
        currentSceneName = SceneManager.GetActiveScene().name;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            StartCoroutine(FadeAndLoadScene());
            timer = countdown;
        }
    }

    IEnumerator FadeIn()
    {
        isFading = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetFadeAlpha(1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        isFading = false;
    }

    IEnumerator FadeAndLoadScene()
    {
        isFading = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetFadeAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        string current = SceneManager.GetActiveScene().name;
        string next = current == scene1 ? scene2 : scene1;

        // Kiểm tra và cập nhật số ngày
        CheckAndUpdateDayCount(current, next);

        SceneManager.LoadScene(next);
    }

    void CheckAndUpdateDayCount(string currentScene, string nextScene)
    {
        // Nếu đang ở scene1 và sắp chuyển sang scene2
        if (currentScene == scene1 && nextScene == scene2)
        {
            hasCompletedCycle = true;
        }
        // Nếu đang ở scene2 và sắp chuyển sang scene1, và đã hoàn thành scene1->scene2 trước đó
        else if (currentScene == scene2 && nextScene == scene1 && hasCompletedCycle)
        {
            dayCount++;
            hasCompletedCycle = false; // Reset cho chu kỳ tiếp theo
            SaveDayCount();
            Debug.Log("Hoàn thành 1 ngày! Tổng số ngày: " + dayCount);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    void SetFadeAlpha(float a)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }

    // Lưu số ngày vào PlayerPrefs
    void SaveDayCount()
    {
        if (saveDayCountPermanently)
        {
            PlayerPrefs.SetInt("DayCount", dayCount);
            PlayerPrefs.Save();
        }
    }

    // Load số ngày từ PlayerPrefs
    void LoadDayCount()
    {
        if (saveDayCountPermanently)
        {
            dayCount = PlayerPrefs.GetInt("DayCount", 0); // Mặc định là 0 nếu chưa có dữ liệu
        }
        else
        {
            dayCount = 0; // Luôn bắt đầu từ 0 nếu không lưu vĩnh viễn
        }
    }

    // Hàm reset số ngày (có thể gọi từ ngoài nếu cần)
    public void ResetDayCount()
    {
        dayCount = 0;
        hasCompletedCycle = false;
        SaveDayCount();
        Debug.Log("Đã reset số ngày về 0");
    }

    // Hàm kiểm tra dữ liệu PlayerPrefs
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void CheckPlayerPrefs()
    {
        int savedDays = PlayerPrefs.GetInt("DayCount", -1);
        if (savedDays == -1)
        {
            Debug.Log("Chưa có dữ liệu ngày được lưu trong PlayerPrefs");
        }
        else
        {
            Debug.Log($"Dữ liệu PlayerPrefs - Số ngày đã lưu: {savedDays}");
        }
    }

    // Hàm test để force tăng ngày (chỉ dùng trong Editor)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ForceAddDay()
    {
        dayCount++;
        SaveDayCount();
        Debug.Log($"[TEST] Đã force tăng ngày lên: {dayCount}");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}