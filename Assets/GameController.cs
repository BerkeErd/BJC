using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public static event Action OnGameStart; // Oyunun baþlama event'i
    public static event Action OnGameLose; // Oyunun kaybedilme event'i
    public static GameController Instance { get; private set; }


    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] TextMeshProUGUI LevelText;

    public bool isGameStarted = false;
    private bool isGameOver = false;
    private bool firstTouchHandled = false;

    private float timerDuration = 0f; 
    private float remainingTime = 0f; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartGame()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
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
        TimerText.text = "Game Over";
        Debug.Log("Game Over");
        OnGameLose?.Invoke();
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
            remainingTime -= Time.deltaTime; // Kalan zamaný azalt
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
