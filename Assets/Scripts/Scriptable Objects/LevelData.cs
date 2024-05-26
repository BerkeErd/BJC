using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level/Create New Level", order = 1)]
public class LevelData : ScriptableObject
{
    public int width;
    public int height;
    public LevelGridCell[] gridCells; // Grid hücrelerini tutacak dizi

    [System.Serializable]
    public class LevelGridCell
    {
        public bool isOccupied; // Hücre dolu mu?
        public Color passengerColor; // Yolcunun rengi
        public bool isBlocked; // Hücre engellenmiþ mi?
    }
}

