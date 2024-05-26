using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level/Create New Level", order = 1)]
public class LevelData : ScriptableObject
{
    public int width;
    public int height;
    public LevelGridCell[] gridCells; // Grid h�crelerini tutacak dizi

    public int waitingAreaSize = 5;

    [System.Serializable]
    public class LevelGridCell
    {
        public bool isOccupied; // H�cre dolu mu?
        public Color passengerColor; // Yolcunun rengi
        public bool isBlocked; // H�cre engellenmi� mi?
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


