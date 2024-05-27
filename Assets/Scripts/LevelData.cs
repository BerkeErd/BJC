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
    public LevelGridCell[] gridCells; // Grid hücrelerini tutacak dizi

    public int waitingAreaSize = 5;

    [System.Serializable]
    public class LevelGridCell
    {
        public bool isOccupied;
        public Color passengerColor;
        public bool isBlocked;
        public bool isTunnel;
        public int tunnelSize = 1; // Tünel boyutu
        public List<Color> tunnelPassengerColors = new List<Color>(); // Tünel içindeki yolcularýn renkleri
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
}


