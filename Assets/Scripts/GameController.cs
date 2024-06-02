using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public static event Action OnGameStart; // Oyunun baþlama event'i
    public static event Action OnGameWon; // Oyunun kazanýlma event'i
    public static event Action OnGameLose; // Oyunun kaybetme event'i
    public static GameController Instance { get; private set; }


    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] TextMeshProUGUI LevelText;

    [SerializeField] GameObject LoseScreen;
    [SerializeField] GameObject WinScreen;
    [SerializeField] TextMeshProUGUI TapToStartText;

    public bool isGameStarted = false;
    private bool isGameOver = false;
    private bool firstTouchHandled = false;

    private float timerDuration = 0f; 
    private float remainingTime = 0f; 

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public int GetWaitingGridsCount()
    {
        int WaitingGridCount = GameObject.FindObjectsOfType<WaitingGrid>().Length;
        return WaitingGridCount;
    }


    public void StartGame()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            TapToStartText.gameObject.SetActive(false);
            remainingTime = timerDuration; 
            OnGameStart?.Invoke();
        }
    }

    public void SetLevelInfos(float Timer, string LevelName)
    {
        timerDuration = Timer;
        remainingTime = Timer;
        LevelText.text = LevelName;
    }

    public void LoseGame()
    {
        isGameStarted = false;
        isGameOver = true;
        LoseScreen.GetComponent<Animator>().Play("FallScreenFallAnim");
        TimerText.text = "Game Over";
        OnGameLose?.Invoke();
    }

    public void WinLevel()
    {
        if(!isGameOver)
        {
            WinScreen.GetComponent<Animator>().Play("WinScreenFallAnim");
            isGameStarted = false;
            isGameOver = true;
        }
        
    }

    public void ToggleWinLevelEvent()
    {
        OnGameWon?.Invoke();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!isGameStarted && !isGameOver)
            {
                StartGame();
            }
            else if (!firstTouchHandled)
            {
                firstTouchHandled = true;
            }
        }

        if (isGameStarted && remainingTime > 0)
        {
            TimerText.text = remainingTime.ToString("F0") + " Seconds";
            remainingTime -= Time.deltaTime; 
            if (remainingTime <= 0)
            {
                LoseGame();
            }
        }
    }

    public bool IsFirstTouchHandled()
    {
        return firstTouchHandled;
    }

    public void ResetFirstTouchHandled()
    {
        firstTouchHandled = false;
    }
}
