using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownDisplay : MonoBehaviour
{
    public ChangeScene changeScene;
    public TMP_Text countdownText;
    public TMP_Text warningText;

    void Update()
    {
        if (changeScene == null) return;

        float timeLeft = changeScene.TimeLeft;

        if (countdownText != null)
            countdownText.text = Mathf.CeilToInt(timeLeft) + "s";

        if (timeLeft <= 5 && timeLeft > 0 && warningText != null)
        {
            warningText.text = Mathf.CeilToInt(timeLeft) + " seconds to the night!";
        }
        else if (warningText != null && timeLeft > 5)
        {
            warningText.text = "";
        }
    }
}
