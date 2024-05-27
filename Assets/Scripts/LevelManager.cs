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
    [SerializeField] private float planeExtraPadding = 1.0f;  // Her bir kenara eklenecek ekstra geniþlik

    [SerializeField] private float sidewalkLength = 5.0f; 
    [SerializeField] private float sidewalkWidth = 20.0f;

    [SerializeField] private float roadLength = 10.0f;

    [SerializeField] private Material roadMat;
    [SerializeField] private Material sidewalkMat;

    void Start()
    {
        ResetTemporaryData();
        BuildLevel();
        AdjustCameraToLevel();
        PlaceSideWalk();
        PlaceRoad();
    }

    // Geçici bilgileri sýfýrla
    void ResetTemporaryData()
    {
        for (int i = 0; i < levelData.height; i++)
        {
            for (int j = 0; j < levelData.width; j++)
            {
                levelData.tempOccupiedCells[i, j] = false;
            }
        }
    }

    void AdjustCameraToLevel()
    {
        // Plane'in boyutlarýný al
        float planeWidth = levelData.width * (1 + planeSpacing) - planeSpacing + planeExtraPadding;
        float planeDepth = levelData.height + sidewalkLength + roadLength;  

        // Kamera yüksekliðini ve pozisyonunu ayarla
        float cameraHeight = Mathf.Max(planeWidth, planeDepth); 
        float backOffset = planeDepth / 2 + 10.0f;  

        Vector3 cameraPosition = new Vector3(planeWidth / 2, cameraHeight, planeDepth / 2 - backOffset);
        mainCamera.transform.position = cameraPosition;

        // Kameranýn yolun baþlangýç noktasýna bakmasýný saðla
        Vector3 lookAtPosition = new Vector3(planeWidth / 2, 0, planeDepth / 2);
        mainCamera.transform.LookAt(lookAtPosition);
    }




    void PlaceSideWalk()
    {
        // Yolun baþlangýç pozisyonunu hesapla
        Vector3 roadPosition = new Vector3(levelData.width / 2.0f, -0.01f, levelData.height + sidewalkLength / 2.0f);

        // Plane oluþtur ve yol olarak kullan
        GameObject roadPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roadPlane.transform.position = roadPosition;
        roadPlane.transform.localScale = new Vector3(sidewalkWidth / 10, 1, sidewalkLength / 10);
        
        roadPlane.GetComponent<Renderer>().material = sidewalkMat;

        // Sidewalk gridlerini yerleþtir
        float centerGridX = levelData.width / 2.0f; // Merkez gridin X pozisyonu
        float gridSpacing = 1 + planeSpacing; // Her grid arasý mesafe
        float zPosition = levelData.height + (sidewalkLength / 2.0f); // Sidewalk'ýn ortasýna yerleþtir

        // 5 grid ekle, merkezi hesapla ve oradan itibaren saða sola ekle
        for (int i = -2; i <= 2; i++) // -2'den 2'ye kadar (5 grid)
        {
            Vector3 gridPosition = new Vector3(centerGridX + i * gridSpacing, 0.1f, zPosition); // Y ekseni biraz yükseltilmiþ
            GameObject grid = Instantiate(floorPrefab, gridPosition, Quaternion.identity);
        }
    }



    void PlaceRoad()
    {
        // Sidewalk boyutlarý ve pozisyonundan yola baþlangýç pozisyonunu hesapla
        float sidewalkEndZ = levelData.height + sidewalkLength;
        Vector3 roadStartPosition = new Vector3(levelData.width / 2.0f, -0.01f, sidewalkEndZ + roadLength / 2.0f);

        // Yol plane'ini oluþtur ve konumlandýr
        GameObject roadPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roadPlane.transform.position = roadStartPosition;
        roadPlane.transform.localScale = new Vector3(sidewalkWidth / 10, 1, roadLength / 10); // Yolun ölçeðini ayarla
        
        roadPlane.GetComponent<Renderer>().material = roadMat;
    }





    void BuildLevel()
    {
        float planeWidth = levelData.width * (1 + planeSpacing) - planeSpacing + planeExtraPadding;
        float planeDepth = levelData.height * (1 + planeSpacing) - planeSpacing + planeExtraPadding;


        // Zemin plane oluþturma
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.localScale = new Vector3(planeWidth / 10, 1, planeDepth / 10);
        plane.transform.position = new Vector3((planeWidth - 1) / 2, -0.05f, (planeDepth - 1) / 2);

        // Grid baþlangýç pozisyonlarýný hesaplama
        for (int i = 0; i < levelData.height; i++)
        {
            for (int j = 0; j < levelData.width; j++)
            {
                // Burada, grid boyutlarýna göre düzeltilmiþ pozisyon hesaplama
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
                    InstantiateTunnel(position, i, j, cell.tunnelSize, cell.tunnelPassengerColors);
                    levelData.tempOccupiedCells[i, j] = true;
                }
            }
        }
    }



    void InstantiateTunnel(Vector3 position, int rowIndex, int colIndex, int tunnelSize, List<Color> tunnelPassengerColors)
    {
        GameObject tunnel = ObjectPooler.Instance.SpawnFromPool("Tunnel", position, Quaternion.identity);
        tunnel.name = $"Tunnel_{rowIndex}_{colIndex}";
        tunnel.GetComponent<Tunnel>().Initialize(levelData,rowIndex,colIndex, tunnelSize,tunnelPassengerColors ); // Initialize fonksiyonu ile tünelin konumu ve diðer baþlangýç ayarlarýný yap.
    }
}

