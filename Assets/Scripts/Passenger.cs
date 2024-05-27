using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    private Color _passengerColor;
    public LevelData levelData;
    public int rowIndex;
    public int colIndex;
    [SerializeField] private float moveSpeed = 3f;


    public Color PassengerColor
    {
        get => _passengerColor;
        set
        {
            _passengerColor = value;
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = value;
            }
        }
    }

    public void OnMouseUpAsButton()
    {
        Debug.Log("Passenger t�kland�!");
        List<Vector3> path = FindPathToExit();
        if (path.Count > 0)
        {
            ClearOccupiedCell();
            StartCoroutine(FollowPath(path));
        }
        else
        {
            Debug.Log("��k��a giden yol bulunamad�.");
        }
    }


    public void Initialize(LevelData data, int row, int col, Color color)
    {
        levelData = data;
        rowIndex = row;
        colIndex = col;
        PassengerColor = color; 
    }


    void ClearOccupiedCell()
    {
        if (rowIndex >= 0 && rowIndex < levelData.height && colIndex >= 0 && colIndex < levelData.width)
        {
            levelData.tempOccupiedCells[rowIndex, colIndex] = false; 
        }
    }

    List<Vector3> FindPathToExit()
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector3> path = new List<Vector3>();

        Vector2Int start = new Vector2Int(colIndex, rowIndex);
        queue.Enqueue(start);
        cameFrom[start] = start;  

        if (rowIndex == 0) // E�er zaten ilk sat�rda ise, yol ba�lang�� noktas�n� i�erir
        {
            path.Add(levelData.gridCells[rowIndex * levelData.width + colIndex].Position);
            return path;
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current.y == 0)  // �lk sat�ra ula��nca yolu geri izle
            {
                Vector2Int step = current;
                while (step != start)
                {
                    path.Add(levelData.gridCells[step.y * levelData.width + step.x].Position);
                    step = cameFrom[step];
                }
                path.Reverse();
                return path;
            }

            List<Vector2Int> neighbors = GetNeighbors(current);
            foreach (var neighbor in neighbors)
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return path; // E�er yol bulunamazsa bo� liste d�n
    }


    IEnumerator FollowPath(List<Vector3> path)
    {
        foreach (Vector3 targetPosition in path)
        {
            transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));

            float journey = 0f;  // Hareketin ba�lang�c�nda yolculuk s�resini s�f�rla.
            while (journey < 1f)  // Interpolasyon 1'e ula�ana kadar devam et.
            {
                journey += Time.deltaTime * moveSpeed;  // Zamanla �arp�lan h�z, yolculu�u art�r�r.
                transform.position = Vector3.Lerp(transform.position, targetPosition, journey);  // Mevcut pozisyondan hedefe do�ru yumu�ak ge�i� yap.
                yield return null;  // Bir sonraki frame'e kadar bekle.
            }
        }

        ObjectPooler.Instance.RemoveFromPool("Passenger", gameObject);  // Pooling ile yok etme i�lemi
    }

    List<Vector2Int> GetNeighbors(Vector2Int current)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // D�rt temel y�n: Yukar�, A�a��, Sol, Sa�
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, -1),  // Yukar�
        new Vector2Int(0, 1),   // A�a��
        new Vector2Int(-1, 0),  // Sol
        new Vector2Int(1, 0)    // Sa�
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(current.x + dir.x, current.y + dir.y);
            if (IsInsideGrid(neighborPos) && !IsCellOccupied(neighborPos.y, neighborPos.x))
            {
                neighbors.Add(neighborPos);
            }
        }

        return neighbors;
    }


    bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < levelData.width && pos.y >= 0 && pos.y < levelData.height;
    }

    bool IsCellOccupied(int row, int col)
    {
        if (IsInsideGrid(new Vector2Int(col, row)))
        {
            return levelData.tempOccupiedCells[row, col];
        }
        return true; // Grid d���ndaysa veya h�cre doluysa, me�gul kabul et
    }

  

}
