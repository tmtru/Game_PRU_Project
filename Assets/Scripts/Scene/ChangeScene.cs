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

    private float timer;
    public float TimeLeft => timer;
    private bool isFading = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        timer = countdown;
        SetFadeAlpha(1f);
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (isFading) return;

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
        SceneManager.LoadScene(next);
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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
