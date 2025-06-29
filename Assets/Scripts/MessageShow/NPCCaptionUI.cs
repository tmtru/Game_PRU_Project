using UnityEngine;
using TMPro;

public class NPCCaptionUI : MonoBehaviour
{
    public TextMeshProUGUI captionText;
    public GameObject captionCanvas;
    public float yOffset = 2.5f;

    private Transform playerCamera;
    private float timer = 0f;
    private float duration = 0f;
    private bool isShowing = false;

    private void Start()
    {
        playerCamera = Camera.main.transform;
        Hide();
    }

    private void Update()
    {
        if (isShowing)
        {
            // Luôn xoay UI về phía camera
            //transform.LookAt(playerCamera);
            //transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position);

            // Đếm ngược thời gian hiển thị
            if (duration > 0)
            {
                timer += Time.deltaTime;
                if (timer >= duration)
                {
                    Hide();
                }
            }
        }
    }

    public void ShowMessage(float showDuration)
    {
        captionCanvas.SetActive(true);
        timer = 0f;
        duration = showDuration;
        isShowing = true;
    }

    public void Hide()
    {
        captionCanvas.SetActive(false);
        isShowing = false;
    }
}
