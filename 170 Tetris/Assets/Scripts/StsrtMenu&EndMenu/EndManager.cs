using UnityEngine;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    bool leavingScene = false;
    public Camera mainCamera;
    public float shrinkRate; 
    float time = 0;
    public AudioManager audioManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
    }
}
