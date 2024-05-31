using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{

    public static event Action<Passenger> OnPassengerMoved; // Yolcu hareket ettiðinde tetiklenecek event


    private Color _passengerColor;
    public LevelData levelData;
    public int rowIndex;
    public int colIndex;
    [SerializeField] private float moveSpeed = 3f;
    private Animator animator;

    private PassengerManager manager;

    public bool isMoving = false;
    private Vector3 targetPosition;
    private BusManager busManager;

    private Vector3 originalScale = new Vector3(0.01f, 0.01f, 0.01f);

    void Start()
    {
        manager = FindObjectOfType<PassengerManager>();
        busManager = GameObject.FindObjectOfType<BusManager>();
        animator = GetComponent<Animator>();
        manager.RegisterPassenger(this);
    }


    void OnEnable()
    {
        manager = FindObjectOfType<PassengerManager>();
        if (manager != null)
        {
            manager.ActivatePassenger(this);
        }
        PlaySpawnAnimation();
    }

    void OnDisable()
    {
        if (manager != null)
        {
            manager.DeactivatePassenger(this);
        }
    }

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.UnregisterPassenger(this);
        }
    }

    public bool CanMoveToFirstRow()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Baþlangýç noktasýný kuyruða ekle
        Vector2Int start = new Vector2Int(colIndex, rowIndex);
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Ýlk satýra ulaþýldý mý kontrol et
            if (current.y == 0)
            {
                return true;
            }

            // Dört yönde hareket et
            List<Vector2Int> possibleMoves = new List<Vector2Int>()
        {
            new Vector2Int(current.x, current.y - 1), // Yukarý
            new Vector2Int(current.x, current.y + 1), // Aþaðý
            new Vector2Int(current.x - 1, current.y), // Sol
            new Vector2Int(current.x + 1, current.y)  // Sað
        };

            foreach (var move in possibleMoves)
            {
                // Hareketin grid sýnýrlarý içinde, ziyaret edilmediði ve meþgul olmadýðý kontrol edilir
                if (IsInsideGrid(move) && !visited.Contains(move) && !IsCellOccupied(move.y, move.x) && !IsCellOccupiedByTunnel(move.y, move.x))
                {
                    queue.Enqueue(move);
                    visited.Add(move); // Hücreyi ziyaret edildi olarak iþaretle
                }
            }
        }

        return false; // Uygun bir yol bulunamadý
    }



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

    private void GoToWaitingCells()
    {
        if (CheckAvailableWaitingGrids(gameObject))
        {
            ClearOccupiedCell();
        }
        else
        {
            GameController.Instance.LoseGame();
        }
    }

    public void OnMouseUpAsButton()
    {
        if (GameController.Instance.isGameStarted && GameController.Instance.IsFirstTouchHandled() && !isMoving)
        {
            List<Vector3> path = FindPathToExit();
            if (path.Count > 0)
            {
                ClearOccupiedCell();
                OnPassengerMoved?.Invoke(this);
                manager.DeactivatePassenger(this);
                StartCoroutine(FollowPath(path));
            }
            else
            {
                Debug.Log("Çýkýþa giden yol bulunamadý.");
            }
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

        if (rowIndex == 0) // Eðer zaten ilk satýrda ise, yol baþlangýç noktasýný içerir
        {
            path.Add(levelData.gridCells[rowIndex * levelData.width + colIndex].Position);
            return path;
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current.y == 0)  // Ýlk satýra ulaþýnca yolu geri izle
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

        return path; // Eðer yol bulunamazsa boþ liste dön
    }


    IEnumerator FollowPath(List<Vector3> path)
    {
        isMoving = true;
        animator.SetBool("isMoving", isMoving);

        foreach (Vector3 targetPosition in path)
        {
            transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));

            float journey = 0f;  // Hareketin baþlangýcýnda yolculuk süresini sýfýrla.
            while (journey < 1f)  // Interpolasyon 1'e ulaþana kadar devam et.
            {
                journey += Time.deltaTime * moveSpeed;  // Zamanla çarpýlan hýz, yolculuðu artýrýr.
                transform.position = Vector3.Lerp(transform.position, targetPosition, journey);  // Mevcut pozisyondan hedefe doðru yumuþak geçiþ yap.
                yield return null;  // Bir sonraki frame'e kadar bekle.
            }
        }


       StartCoroutine(CheckForBus());

    }


    IEnumerator CheckForBus()
    {
        while (busManager.currentBus == null || busManager.currentBus.isFull())
        {
            yield return new WaitForSeconds(1f); // Bir saniye bekle ve tekrar kontrol et
        }

        if (busManager.currentBus.busColor != PassengerColor)
        {
            GoToWaitingCells();
        }
        else
        {
            MoveToBusIfNooneisWaiting(busManager.currentBus);
        }
    }


    List<Vector2Int> GetNeighbors(Vector2Int current)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Dört temel yön: Yukarý, Aþaðý, Sol, Sað
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, -1),  // Yukarý
        new Vector2Int(0, 1),   // Aþaðý
        new Vector2Int(-1, 0),  // Sol
        new Vector2Int(1, 0)    // Sað
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(current.x + dir.x, current.y + dir.y);
            if (IsInsideGrid(neighborPos) && !IsCellOccupied(neighborPos.y, neighborPos.x) && !IsCellOccupiedByTunnel(neighborPos.y, neighborPos.x))
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
        return true; // Grid dýþýndaysa veya hücre doluysa, meþgul kabul et
    }

    bool IsCellOccupiedByTunnel(int row, int col)
    {
        if (IsInsideGrid(new Vector2Int(col, row)))
        {
            return levelData.occupiedByTunnelCells[row, col];
        }
        return true; // Grid dýþýndaysa veya hücre doluysa, meþgul kabul et
    }

    public void MoveToBusIfNooneisWaiting(Bus bus)
    {
        if(bus.activeBus && bus.busColor == PassengerColor && !CheckIfSameColorPassengerWaiting() && !bus.isFull())
        {
            bus.IncreasePassengerCount(this);
            StartCoroutine(MoveToBusAndGetIn(bus));
        }
        else
        {
            GoToWaitingCells();
        }
        
    }

    public void GetInsideofBus(Bus bus)
    {
        isMoving = false;
        animator.SetBool("isMoving", isMoving);
        bus.GetPassengerIn(this);
    }

    private IEnumerator MoveToBusAndGetIn(Bus bus)
    {
        
        isMoving = true;
        animator.SetBool("isMoving", isMoving);

        while (isMoving)
        {
            targetPosition = bus.transform.position;

            float step = moveSpeed * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                GetInsideofBus(bus);
            }

            yield return null;
        }
    }


    bool CheckAvailableWaitingGrids(GameObject passangerObject)
    {
        foreach (var grid in GameObject.FindObjectsOfType<WaitingGrid>())
        {
           if(grid.isEmpty)
            {
                grid.passengerOnGrid = this;
                passangerObject.transform.position = grid.transform.position;
                animator.SetBool("isMoving", false);
                grid.isEmpty = false;
                return true;
            }
        }

        return false;
    }

    bool CheckIfSameColorPassengerWaiting()
    {
        foreach (var grid in GameObject.FindObjectsOfType<WaitingGrid>())
        {
            if (!grid.isEmpty)
            {
                if(grid.passengerOnGrid.PassengerColor == PassengerColor)
                return true;
            }
        }

        return false;

    }

    private IEnumerator PlayGrowthAnimation()
    {
        Vector3 targetScale = originalScale * 1f;
        float currentTime = 0f;

        while (currentTime < 1)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, currentTime / 1);
            currentTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public void PlaySpawnAnimation()
    {
        StartCoroutine(PlayGrowthAnimation());
    }

    public void Despawn()
    {
        ObjectPooler.Instance.ReturnToPool("Passenger", gameObject);
    }

}
