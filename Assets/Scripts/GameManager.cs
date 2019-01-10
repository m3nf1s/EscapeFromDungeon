using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2.0f; // задержка 
    public static GameManager instance = null; //
    public LevelGeneration levelScript; //доступ к генератору уровней
    public bool canMove; // переменная, которая позволяет персонажам двигаться по карте или нет

    private Text levelText;
    private GameObject levelImage; 
    private int level = 0;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        levelScript = GetComponent<LevelGeneration>();
    }

    void InitGame()
    {
        canMove = false;

        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Floor " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", 2.0f);
        levelScript.SceneSetup();

    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        canMove = true;
    }

    public void GameOver()
    {
        levelText.text = "You were caught \n on the " + level + " floor";
        levelImage.SetActive(true);
        enabled = false;

        Invoke("Quit", 4.0f);
    }

    void Quit()
    {
        Destroy(gameObject);
        Destroy(GameObject.Find("SoundManager"));
        SceneManager.LoadScene(0);
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        level++;
        InitGame();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
}
