using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public Image fadeImage;
    [SerializeField]
    public float fadeDuration;
    [SerializeField]
    public string scene1;
    [SerializeField]
    public string scene2;
    [SerializeField]
    public float countdown;
    private float timer;
    public float TimeLeft => timer;

    void Start()
    {
        timer = countdown;
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            StartCoroutine(FadeAndLoadScene());
            timer = countdown;
        }
    }

    IEnumerator FadeIn()
    {
        float t = fadeDuration;
        Color c = fadeImage.color;
        while (t > 0)
        {
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        string current = SceneManager.GetActiveScene().name;
        string next = current == scene1 ? scene2 : scene1;
        SceneManager.LoadScene(next);
    }
}