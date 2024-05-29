using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Level", menuName = "Level/Create New Level", order = 1)]
public class LevelData : ScriptableObject
{
    
    [Range(2,30)]
    public int width;
    [Range(2, 30)]
    public int height;
    public LevelGridCell[] gridCells; // Grid h�crelerini tutacak dizi

    public int waitingAreaSize = 5;

    //[HideInInspector]
    public bool[,] tempOccupiedCells;
    public bool[,] occupiedByTunnelCells;

    void OnEnable()
    {
        // Diziyi ba�lat
        tempOccupiedCells = new bool[height, width];
        occupiedByTunnelCells = new bool[height, width];
    }

    [System.Serializable]
    public class LevelGridCell
    {
        public Vector3 Position;
        public bool isOccupied;
        public Color passengerColor;
        public bool isBlocked;
        public bool isTunnel;
        public TunnelDirection tunnelDirection;
        public int tunnelSize = 1; // T�nel boyutu
        public List<Color> tunnelPassengerColors = new List<Color>(); // T�nel i�indeki yolcular�n renkleri
    }

    private void OnValidate()
    {
        ResizeGrid();
    }

    private void ResizeGrid()
    {
        if (gridCells == null || gridCells.Length != width * height)
        {
            gridCells = new LevelGridCell[width * height];
            for (int i = 0; i < gridCells.Length; i++)
            {
                gridCells[i] = new LevelGridCell();
            }
        }
    }

    // Toplam yolcu ve t�nel say�s�n� hesaplayan metot
    public (int totalPassengers, int totalTunnels) CalculatePassengerAndTunnelCounts()
    {
        int totalPassengers = 0;
        int totalTunnels = 0;

        foreach (var cell in gridCells)
        {
            if (cell.isOccupied)
            {
                totalPassengers++;  // Her i�gal edilmi� h�cre i�in yolcu say�s�n� artt�r
            }
            if (cell.isTunnel)
            {
                totalTunnels++;  // Her t�nel i�in t�nel say�s�n� artt�r
                totalPassengers += cell.tunnelSize;  // T�nel boyutu kadar yolcu say�s�n� artt�r
            }
        }

        return (totalPassengers, totalTunnels);
    }
}


