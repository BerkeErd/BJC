using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JokerManager : MonoBehaviour
{
    public static JokerManager Instance { get; private set; }

    private int undoCount;
    private int waitingGridMultiplierCount;
    private int colorPickerCount;

    [SerializeField] private int defaultJokerCount;
    [SerializeField] PassengerManager passManager;
    [SerializeField] BusManager busManager;

    [SerializeField] private TextMeshProUGUI UndoCountText;
    [SerializeField] private TextMeshProUGUI GridCountText;
    [SerializeField] private TextMeshProUGUI ColorPickerCountText;

    public List<GameObject> bonusGrids = new List<GameObject>();

    private bool isBonusGridsActive = false;

    private void UpdateTextInfos()
    {
        UndoCountText.text = undoCount.ToString();
        GridCountText.text = waitingGridMultiplierCount.ToString();
        ColorPickerCountText.text = colorPickerCount.ToString();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            LoadJokerCounts();
        }
    }

    private void LoadJokerCounts()
    {
        undoCount = PlayerPrefs.GetInt("UndoCount", defaultJokerCount);
        waitingGridMultiplierCount = PlayerPrefs.GetInt("WaitingGridMultiplierCount", defaultJokerCount); 
        colorPickerCount = PlayerPrefs.GetInt("ColorPickerCount", defaultJokerCount);
        UpdateTextInfos();
    }

    public void AddOneofEachJoker()
    {
        AddUndoJoker();
        AddGridJoker();
        AddPickerJoker();
        UpdateTextInfos();
    }

    private void AddUndoJoker()
    {
        undoCount++;
        PlayerPrefs.SetInt("UndoCount", undoCount);
        PlayerPrefs.Save();
    }

    private void AddGridJoker()
    {
        waitingGridMultiplierCount++;
        PlayerPrefs.SetInt("WaitingGridMultiplierCount", waitingGridMultiplierCount);
        PlayerPrefs.Save();
    }

    private void AddPickerJoker()
    {
        colorPickerCount++;
        PlayerPrefs.SetInt("ColorPickerCount", colorPickerCount);
        PlayerPrefs.Save();
    }

    public void UseUndo()
    {
        if (passManager.lastPassengerWentToGrid != null)
        {
                if (undoCount > 0 && GameController.Instance.isGameStarted)
                {
                    undoCount--;
                    UndoCountText.text = undoCount.ToString();
                    PlayerPrefs.SetInt("UndoCount", undoCount);
                    PlayerPrefs.Save();
                    passManager.lastPassengerWentToGrid.Undo();
                }
        }
    }

    public void UseWaitingGridMultiplier()
    {
        if(!isBonusGridsActive)
        {
            if (waitingGridMultiplierCount > 0 && GameController.Instance.isGameStarted)
            {
                waitingGridMultiplierCount--;
                GridCountText.text = waitingGridMultiplierCount.ToString();
                PlayerPrefs.SetInt("WaitingGridMultiplierCount", waitingGridMultiplierCount);
                PlayerPrefs.Save();

                foreach (var grid in bonusGrids)
                {
                    grid.SetActive(true);
                    isBonusGridsActive = true;
                }
            }
        }
       
    }

    public void UseColorPicker()
    {
        if (busManager.currentBus != null && !busManager.currentBus.isFull())
        {
            if (colorPickerCount > 0 && GameController.Instance.isGameStarted)
            {
                
                PlayerPrefs.SetInt("ColorPickerCount", colorPickerCount);
                PlayerPrefs.Save();
                foreach (var passenger in passManager.activePassengers)
                {
                    if (passenger.PassengerColor == busManager.currentBus.busColor && !passenger.isFlying)
                    {
                        passenger.FlyToBus();
                        colorPickerCount--;
                        break;
                    }
                }

                ColorPickerCountText.text = colorPickerCount.ToString();
            }
        }
    }

    // Kalan joker sayýlarýný alma
    public int GetUndoCount() => undoCount;
    public int GetWaitingGridMultiplierCount() => waitingGridMultiplierCount;
    public int GetColorPickerCount() => colorPickerCount;
}
