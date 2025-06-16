using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeReference]
    public string scene1;
    [SerializeReference]
    public string scene2;
    [SerializeReference]
    public float changeTime;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeTime)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == scene1)
            {
                SceneManager.LoadScene(scene2);
            }
            else if (currentScene == scene2)
            {
                SceneManager.LoadScene(scene1);
            }

            timer = 0f;
        }
    }
}