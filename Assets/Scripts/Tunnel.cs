using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        passengerManager = FindObjectOfType<PassengerManager>();
        InitializePassengers(); // Tüm passenger'larý önceden oluþtur ve kaydet
    }

    public void Initialize(LevelData data, int row, int col, int size, List<Color> passangers)
    {
        levelData = data;
        rowIndex = row;
        colIndex = col;
        tunnelSize = size;
        passengerColors = passangers;
        StartCoroutine(CheckAndSpawnPassenger());
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
        int frontIndex = rowIndex - 1;
        if (frontIndex >= 0 && !IsCellOccupied(frontIndex, colIndex))
        {
            SpawnPassenger(frontIndex, colIndex, passengerColors[spawnedCount]);
            spawnedCount++;
            tunnelSize -= 1;
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
            Vector3 spawnPosition = levelData.gridCells[row * levelData.width + col].Position;
            GameObject newPassenger = ObjectPooler.Instance.SpawnFromPool("Passenger", spawnPosition, Quaternion.identity);
            Passenger passengerComponent = newPassenger.GetComponent<Passenger>();
            passengerComponent.Initialize(levelData, row, col, color);
            levelData.tempOccupiedCells[row, col] = true; // Geçici olarak iþgal et
        }
    }
}



