using System.Collections;
using System.Collections.Generic;
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
    public float spawnInterval = 1.0f;
    private PassengerManager passengerManager;
    public TunnelDirection spawnDirection;
    private List<GameObject> placeholderPassengers = new List<GameObject>();
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
        StartCoroutine(CheckAndSpawnPassenger());
        SetTunnelExit(); 
    }

    private void SetTunnelExit()
    {
        Vector2Int newExitCell = new Vector2Int(colIndex, rowIndex);
        switch (spawnDirection)
        {
            case TunnelDirection.Up:
                newExitCell += Vector2Int.down;
                break;
            case TunnelDirection.Down:
                newExitCell += Vector2Int.up;
                break;
            case TunnelDirection.Left:
                newExitCell += Vector2Int.left;
                break;
            case TunnelDirection.Right:
                newExitCell += Vector2Int.right;
                break;
        }
        exitCell = newExitCell;

        if (IsInsideGrid(exitCell))
        {
            BlockTheExitCell();
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


    IEnumerator CheckAndSpawnPassenger()
    {
        while (tunnelSize > 0)
        {
            yield return new WaitForSeconds(spawnInterval); // DEÐÝÞECEK
            TrySpawnPassenger();
        }
    }

    private void TrySpawnPassenger()
    {

        if (IsInsideGrid(exitCell) && !IsCellOccupied(exitCell.y, exitCell.x))
        {
            SpawnPassenger(exitCell.y, exitCell.x, passengerColors[spawnedCount % passengerColors.Count]);
            spawnedCount++;
            tunnelSize--;
        }

        if(tunnelSize == 0)
        {
            UnblockTheExitCell();
            StopCoroutine(CheckAndSpawnPassenger());
        }
        else
        {
            BlockTheExitCell();
        }
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
            passengerManager.UnregisterPassenger(placeholderPassengers[spawnedCount].GetComponent<Passenger>());
            Vector3 spawnPosition = levelData.gridCells[row * levelData.width + col].Position;
            GameObject newPassenger = ObjectPooler.Instance.SpawnFromPool("Passenger", spawnPosition, Quaternion.identity);
            Passenger passengerComponent = newPassenger.GetComponent<Passenger>();
            passengerComponent.hasTunnel = true;
            passengerComponent.tunnel = this;
            passengerComponent.Initialize(levelData, row, col, color);
            levelData.tempOccupiedCells[row, col] = true; // Geçici olarak iþgal et
        }
    }

    private void OnEnable()
    {
        Passenger.OnPassengerMoved += HandlePassengerMoved;
    }

    private void OnDisable()
    {
        Passenger.OnPassengerMoved -= HandlePassengerMoved;
    }

    private void HandlePassengerMoved(Passenger passenger)
    {
        if(tunnelSize > 0)
        {
            TrySpawnPassenger();
        }
    }
}



