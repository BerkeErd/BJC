using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Camera mainCamera; 
    
    [SerializeField] private GameObject passengerPrefab; 
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject tunnelPrefab; 


    [SerializeField] private float planeSpacing = 0.1f;
    [SerializeField] private float planeExtraPadding = 1.0f;  // Her bir kenara eklenecek ekstra geni�lik

    [SerializeField] private float sidewalkLength = 5.0f; 
    [SerializeField] private float sidewalkWidth = 20.0f;

    [SerializeField] private float roadLength = 10.0f;

    [SerializeField] private Material roadMat;
    [SerializeField] private Material sidewalkMat;

    [SerializeField] private BusManager busmanager;
    




    void Start()
    {
        ResetTemporaryData();
        BuildLevel();
        AdjustCameraToLevel();
        PlaceSideWalk();
        PlaceRoad();
        SetLevelInfo();
    }

    // Ge�ici bilgileri s�f�rla
    void ResetTemporaryData()
    {
        for (int i = 0; i < levelData.height; i++)
        {
            for (int j = 0; j < levelData.width; j++)
            {
                levelData.tempOccupiedCells[i, j] = false;
                levelData.occupiedByTunnelCells[i, j] = false;
            }
        }
    }

    void AdjustCameraToLevel()
    {
        // Plane'in boyutlar�n� al
        float planeWidth = levelData.width * (1 + planeSpacing) - planeSpacing + planeExtraPadding;
        float planeDepth = levelData.height + sidewalkLength + roadLength;  

        // Kamera y�ksekli�ini ve pozisyonunu ayarla
        float cameraHeight = Mathf.Max(planeWidth , planeDepth ); 
        float backOffset = planeDepth / 2 + 10.0f;  

        Vector3 cameraPosition = new Vector3(planeWidth / 2, cameraHeight * 2, planeDepth / 2 - backOffset);
        mainCamera.transform.position = cameraPosition;

        // Kameran�n yolun ba�lang�� noktas�na bakmas�n� sa�la
        Vector3 lookAtPosition = new Vector3(planeWidth / 2, 0, planeDepth / 2);
        mainCamera.transform.LookAt(lookAtPosition);
    }




    void PlaceSideWalk()
    {
        // Yolun ba�lang�� pozisyonunu hesapla
        Vector3 roadPosition = new Vector3(levelData.width / 2.0f, -0.01f, levelData.height + sidewalkLength / 2.0f);

        // Plane olu�tur ve yol olarak kullan
        GameObject roadPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roadPlane.transform.position = roadPosition;
        roadPlane.transform.localScale = new Vector3(sidewalkWidth / 10, 1, sidewalkLength / 10);
        
        roadPlane.GetComponent<Renderer>().material = sidewalkMat;

        // Sidewalk gridlerini yerle�tir
        float centerGridX = levelData.width / 2.0f; // Merkez gridin X pozisyonu
        float gridSpacing = 1 + planeSpacing; // Her grid aras� mesafe
        float zPosition = levelData.height + (sidewalkLength / 2.0f); // Sidewalk'�n ortas�na yerle�tir

        busmanager.waitingSpot = new Vector3(centerGridX, 0.1f, zPosition + roadLength/2);

        // 5 grid ekle, merkezi hesapla ve oradan itibaren sa�a sola ekle
        for (int i = -2; i <= 2; i++) // -2'den 2'ye kadar (5 grid)
        {
            Vector3 gridPosition = new Vector3(centerGridX + i * gridSpacing, 0.1f, zPosition); // Y ekseni biraz y�kseltilmi�
            GameObject grid = Instantiate(floorPrefab, gridPosition, Quaternion.identity);
            grid.AddComponent<WaitingGrid>();
        }
    }



    void PlaceRoad()
    {
        // Sidewalk boyutlar� ve pozisyonundan yola ba�lang�� pozisyonunu hesapla
        float sidewalkEndZ = levelData.height + sidewalkLength;
        Vector3 roadStartPosition = new Vector3(levelData.width / 2.0f, -0.01f, sidewalkEndZ + roadLength / 2.0f);
        busmanager.busSpawnPoint = new Vector3(levelData.width * -3, -0.01f, sidewalkEndZ + roadLength / 2.0f);
        busmanager.busDespawnPoint = new Vector3(levelData.width * 3, -0.01f, sidewalkEndZ + roadLength / 2.0f);
        // Yol plane'ini olu�tur ve konumland�r
        GameObject roadPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roadPlane.transform.position = roadStartPosition;
        roadPlane.transform.localScale = new Vector3(sidewalkWidth / 10, 1, roadLength / 10); // Yolun �l�e�ini ayarla
        
        roadPlane.GetComponent<Renderer>().material = roadMat;
    }





    void BuildLevel()
    {
        float planeWidth = levelData.width * (1 + planeSpacing) - planeSpacing + planeExtraPadding;
        float planeDepth = levelData.height * (1 + planeSpacing) - planeSpacing + planeExtraPadding;


        // Zemin plane olu�turma
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.localScale = new Vector3(planeWidth / 10, 1, planeDepth / 10);
        plane.transform.position = new Vector3((planeWidth - 1) / 2, -0.05f, (planeDepth - 1) / 2);

        // Grid ba�lang�� pozisyonlar�n� hesaplama
        for (int i = 0; i < levelData.height; i++)
        {
            for (int j = 0; j < levelData.width; j++)
            {
                // Burada, grid boyutlar�na g�re d�zeltilmi� pozisyon hesaplama
                float xPos = j + (j * planeSpacing) + (planeExtraPadding / 2);
                float zPos = (levelData.height - 1 - i) + ((levelData.height - 1 - i) * planeSpacing) + (planeExtraPadding / 2);
                Vector3 position = new Vector3(xPos, 0, zPos);

                GameObject Floor = ObjectPooler.Instance.SpawnFromPool("Floor", position, Quaternion.identity);

                LevelData.LevelGridCell cell = levelData.gridCells[i * levelData.width + j];
                cell.Position = position;
                if (cell.isOccupied)
                {
                    GameObject Passenger = ObjectPooler.Instance.SpawnFromPool("Passenger", position, Quaternion.identity);
                    Passenger.GetComponentInChildren<Passenger>().Initialize(levelData,i, j, cell.passengerColor);
                    levelData.tempOccupiedCells[i, j] = true;
                }
                else if (cell.isBlocked)
                {
                    ObjectPooler.Instance.SpawnFromPool("Block", position, Quaternion.identity);
                    levelData.tempOccupiedCells[i, j] = true;
                }

                if (cell.isTunnel)
                {
                    InstantiateTunnel(position, i, j, cell.tunnelSize, cell.tunnelPassengerColors, cell.tunnelDirection);
                    levelData.tempOccupiedCells[i, j] = true;
                }
            }
        }
    }

    void SetLevelInfo()
    {
        GameController.Instance.SetLevelInfos(levelData.timer, levelData.name);
    }



    void InstantiateTunnel(Vector3 position, int rowIndex, int colIndex, int tunnelSize, List<Color> tunnelPassengerColors, TunnelDirection tunnelDirection)
    {
        GameObject tunnel = ObjectPooler.Instance.SpawnFromPool("Tunnel", position, Quaternion.identity);
        Tunnel tunnelComponent = tunnel.GetComponent<Tunnel>();
        tunnel.name = $"Tunnel_{rowIndex}_{colIndex}";
        tunnelComponent.Initialize(levelData,rowIndex,colIndex, tunnelSize,tunnelPassengerColors, tunnelDirection); // Initialize fonksiyonu ile t�nelin konumu ve di�er ba�lang�� ayarlar�n� yap.
    }
}

