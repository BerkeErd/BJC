using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static event Action OnGameStart; // Oyunun baþlama event'i

    private bool isGameStarted = false;

    private Bus currentBus;
    private BusManager busManager;
    private List<GameObject> waitingCells;



    public void StartGame()
    {
        Debug.Log("Game Started");
        OnGameStart?.Invoke(); // Event'i tetikle
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.K))
        {
            if(!isGameStarted)
            {
                isGameStarted = true;
                StartGame();
            }
        }
    }
}

