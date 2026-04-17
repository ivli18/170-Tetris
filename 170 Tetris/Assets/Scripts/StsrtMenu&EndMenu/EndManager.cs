using UnityEngine;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    public GameObject levelTitle, linesTitle;
    public static int levelCount;
    public static int linesCleared;
    public TMPro.TextMeshProUGUI levelText;
    public TMPro.TextMeshProUGUI linesText;
    bool leavingScene = false;
    public Camera mainCamera;
    public float shrinkRate; 
    float time = 0;
    public AudioManager audioManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelText.SetText(levelCount.ToString());
        linesText.SetText(linesCleared.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if(leavingScene)
        {
            time += Time.deltaTime;
            mainCamera.orthographicSize = 5 * Mathf.Pow(shrinkRate, time);
            if(mainCamera.orthographicSize <= 0.1f)
            {
                mainCamera.orthographicSize = 0;
                SceneManager.LoadScene("StartScreen");
            }
        }
    }

    public void EndButtonPressed()
    {
        leavingScene = true;
        audioManager.PlaySoundStart();
        hideUI();
    }

    public void hideUI()
    {
        levelTitle.SetActive(false);
        linesTitle.SetActive(false);
        levelText.gameObject.SetActive(false);
        linesText.gameObject.SetActive(false);
    }
}
