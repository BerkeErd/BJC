using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TunnelDirection
{
    Up,
    Down,
    Left,
    Right
}

public class Tunnel : MonoBehaviour
{
    public LevelData levelData;
    public List<Color> passengerColors;
    public int rowIndex;
    public int colIndex;
    public int tunnelSize;
    private int spawnedCount = 0;
    private bool isSpawning = false;
    public float spawnInterval = 1.0f;
    private PassengerManager passengerManager;
    public TunnelDirection spawnDirection;
    private List<GameObject> placeholderPassengers = new List<GameObject>();
    [SerializeField] private TextMeshPro countText;
    Vector2Int exitCell;

    private void Start()
    {
        passengerManager = FindObjectOfType<PassengerManager>();
        InitializePassengers(); // Tüm passenger'larý önceden oluþtur ve kaydet
    }

    public void Initialize(LevelData data, int row, int col, int size, List<Color> passangers, TunnelDirection tunnelDirection)
    {
        levelData = data;
        rowIndex = row;
        colIndex = col;
        tunnelSize = size;
        passengerColors = passangers;
        spawnDirection = tunnelDirection;
        countText.text = tunnelSize.ToString();
        SetTunnelExit(); 
    }

    private void SetTunnelExit()
    {
        Vector2Int newExitCell = new Vector2Int(colIndex, rowIndex);
        Quaternion rotation = Quaternion.identity;
        switch (spawnDirection)
        {
            case TunnelDirection.Up:
                newExitCell += Vector2Int.down;
                rotation = Quaternion.Euler(0, 0, 0); // Yüzünü yukarý çevir
                break;
            case TunnelDirection.Down:
                newExitCell += Vector2Int.up;
                rotation = Quaternion.Euler(0, 180, 0); // Yüzünü aþaðý çevir
                break;
            case TunnelDirection.Left:
                newExitCell += Vector2Int.left;
                rotation = Quaternion.Euler(0, 270, 0); // Yüzünü sola çevir
                break;
            case TunnelDirection.Right:
                newExitCell += Vector2Int.right;
                rotation = Quaternion.Euler(0, 90, 0); // Yüzünü saða çevir
                break;
        }
        exitCell = newExitCell;

        if (IsInsideGrid(exitCell))
        {
            BlockTheExitCell();
            transform.rotation = rotation; 
        }
    }
    

    private void BlockTheExitCell()
    {
        levelData.occupiedByTunnelCells[exitCell.y, exitCell.x] = true;
    }

    private void UnblockTheExitCell()
    {
        levelData.occupiedByTunnelCells[exitCell.y, exitCell.x] = false;
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < levelData.width && pos.y >= 0 && pos.y < levelData.height;
    }

    private void InitializePassengers()
    {
        for (int i = 0; i < tunnelSize; i++)
        {
            Color passengerColor = passengerColors[i % passengerColors.Count];

            // Passenger GameObject oluþtur
            GameObject passengerGO = new GameObject("PassengerPlaceholder");
            Passenger passengerComponent = passengerGO.AddComponent<Passenger>();
            passengerComponent.Initialize(levelData, rowIndex, colIndex, passengerColor);
            passengerManager.RegisterPassenger(passengerComponent);
            passengerGO.SetActive(false);
            placeholderPassengers.Add(passengerGO);
        }
    }

    private void TrySpawnPassenger()
    {
        if (tunnelSize <= 0)
        {
            UnblockTheExitCell();
        }
        else
        {
            if (IsInsideGrid(exitCell) && !IsCellOccupied(exitCell.y, exitCell.x) && !isSpawning)
            {
                isSpawning = true;
                SpawnPassenger(exitCell.y, exitCell.x, passengerColors[spawnedCount]);
            }
            BlockTheExitCell();
        }
        passengerManager.UpdatePassengersWithPaths();
    }

    private bool IsCellOccupied(int row, int col)
    {
        // Geçici meþguliyet durumuna göre kontrol et
        return levelData.tempOccupiedCells[row, col];
    }

    private void SpawnPassenger(int row, int col, Color color)
    {
        if (!IsCellOccupied(row, col)) // Meþgul deðilse yolcu yarat
        {
            levelData.tempOccupiedCells[exitCell.y, exitCell.x] = true; // Geçici olarak iþgal et
            tunnelSize--;
            countText.text = tunnelSize.ToString();
            passengerManager.UnregisterPassenger(placeholderPassengers[spawnedCount].GetComponent<Passenger>());
            Vector3 spawnPosition = levelData.gridCells[row * levelData.width + col].Position;
            GameObject newPassenger = ObjectPooler.Instance.SpawnFromPool("Passenger", spawnPosition, Quaternion.identity);
            Passenger passengerComponent = newPassenger.GetComponent<Passenger>();
            passengerComponent.Initialize(levelData, row, col, color);
            spawnedCount++;
        }

        isSpawning = false;
    }

    private void OnEnable()
    {
        Passenger.OnPassengerMoved += HandlePassengerMoved;
        GameController.OnGameStart += HandleGameStart;
    }

    private void OnDisable()
    {
        Passenger.OnPassengerMoved -= HandlePassengerMoved;
        GameController.OnGameStart -= HandleGameStart;
    }

    private void HandlePassengerMoved(Passenger passenger)
    {
        if (tunnelSize >= 0)
        {
            TrySpawnPassenger();
        }
    }

    private void HandleGameStart()
    {
        TrySpawnPassenger();
    }
}



