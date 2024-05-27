using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tunnel : MonoBehaviour
{
    public int rowIndex;
    public int colIndex;
    public LevelData levelData;

    public void Initialize(int row, int col, LevelData data)
    {
        rowIndex = row;
        colIndex = col;
        levelData = data;

       
        StartCoroutine(CheckAndSpawnPassenger());
    }

    IEnumerator CheckAndSpawnPassenger()
    {
        while (true)
        {
            yield return new WaitForSeconds(1); 
            // Tünelin önünü kontrol et
            int frontIndex = rowIndex - 1;
            if (frontIndex >= 0 && !levelData.gridCells[frontIndex * levelData.width + colIndex].isOccupied)
            {
                Vector3 spawnPosition = transform.position + Vector3.forward; 
            }
        }
    }
}


