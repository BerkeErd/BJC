using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static event Action OnGameStart; // Oyunun baþlama event'i
    public static GameController Instance { get; private set; }

    public bool isGameStarted = false;
    private bool firstTouchHandled = false;

    private Bus currentBus;
    private BusManager busManager;
    private List<GameObject> waitingCells;

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
        isGameStarted = true;
        OnGameStart?.Invoke(); 
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !isGameStarted)
        {
            StartGame();
        }
        else if (isGameStarted && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !firstTouchHandled)
        {
            firstTouchHandled = true; 
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

