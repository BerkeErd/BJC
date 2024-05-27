using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevel;
    private Color[] availableColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
    private string[] colorNames = new string[] { "Red", "Blue", "Green", "Yellow", "Magenta" };
    private int selectedColorIndex = 0;
    private bool AddTunnelMode = false; // T�nel ekleme modu de�i�keni
    private LevelData.LevelGridCell selectedTunnelCell = null; // Se�ilen t�nel h�cresini sakla


    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        currentLevel = (LevelData)EditorGUILayout.ObjectField("Level Data", currentLevel, typeof(LevelData), false);

        selectedColorIndex = EditorGUILayout.Popup("Passenger Color", selectedColorIndex, colorNames);


        if (GUILayout.Button(AddTunnelMode ? "Disable Tunnel Mode" : "Enable Tunnel Mode"))
        {
            AddTunnelMode = !AddTunnelMode;
            Repaint(); // Editor penceresini yeniden �izdir
            if (AddTunnelMode)
                Debug.Log("Tunnel mode activated. Click on a grid cell to place a tunnel.");
            else
                Debug.Log("Tunnel mode deactivated.");
        }

        if (selectedTunnelCell != null && selectedTunnelCell.isTunnel)
        {
            EditorGUILayout.LabelField("Selected Tunnel Properties", EditorStyles.boldLabel);

            // Tunnel size'� otomatik olarak passenger listesinin uzunlu�una ayarla
            selectedTunnelCell.tunnelSize = selectedTunnelCell.tunnelPassengerColors.Count;
            EditorGUILayout.LabelField("Tunnel Size", selectedTunnelCell.tunnelSize.ToString());

            if (GUILayout.Button($"Add {colorNames[selectedColorIndex]} Passenger"))
            {
                selectedTunnelCell.tunnelPassengerColors.Add(availableColors[selectedColorIndex]);
                selectedTunnelCell.tunnelSize = selectedTunnelCell.tunnelPassengerColors.Count; // Passenger ekledikten sonra boyutu g�ncelle
            }

            for (int i = 0; i < selectedTunnelCell.tunnelPassengerColors.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                string colorName = GetColorName(selectedTunnelCell.tunnelPassengerColors[i]);
                EditorGUILayout.LabelField($"Passenger {i + 1} Color", colorName);
                if (GUILayout.Button("Remove"))
                {
                    selectedTunnelCell.tunnelPassengerColors.RemoveAt(i);
                    selectedTunnelCell.tunnelSize = selectedTunnelCell.tunnelPassengerColors.Count; // Passenger ��kard�ktan sonra boyutu g�ncelle
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Clear All Passengers"))
            {
                selectedTunnelCell.tunnelPassengerColors.Clear();
                selectedTunnelCell.tunnelSize = 0; // T�m passenger'lar� temizledikten sonra boyutu s�f�rla
            }
        }


        if (currentLevel != null)
        {
            DrawBusAndWaitingArea();
            DrawLevelGrid();
            if (GUILayout.Button("Save Level"))
            {
                if (CheckColorMultiplesOfThree())
                {
                    EditorUtility.SetDirty(currentLevel);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Level saved successfully!");
                }
                else
                {
                    EditorUtility.DisplayDialog("Save Error", "Each color must have a multiple of three passengers to save.", "OK");
                    Debug.Log("Save failed: Each color must have multiples of three passengers.");
                }
            }
        }


        if (GUILayout.Button("Random Fill"))
        {
            RandomFillGrid();
        }

        if (GUILayout.Button("Clear"))
        {
            ClearGrid(); 
        }



        

    }

    private string GetColorName(Color color)
    {
        for (int i = 0; i < availableColors.Length; i++)
        {
            if (availableColors[i] == color)
                return colorNames[i];
        }
        return "Unknown"; // E�le�me bulunamazsa
    }


    private void ClearGrid()
    {
        foreach (var cell in currentLevel.gridCells)
        {
            cell.isOccupied = false;
            cell.isBlocked = false;
            cell.isTunnel = false;
            cell.passengerColor = Color.clear; 
        }
    }


    private void DrawBusAndWaitingArea()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space((float)currentLevel.waitingAreaSize/2 * 50); // Ortaya hizalamak i�in
        GUILayout.Label(new GUIContent("BUS"), GUILayout.Width(150), GUILayout.Height(50)); 
        EditorGUILayout.EndHorizontal();

       
        EditorGUILayout.BeginHorizontal();
        for (int j = 0; j < currentLevel.waitingAreaSize; j++) 
        {
            GUILayout.Box("WAIT", GUILayout.Width(50), GUILayout.Height(50));
        }
        EditorGUILayout.EndHorizontal();
    }


    void DrawLevelGrid()
{
    for (int i = 0; i < currentLevel.height; i++)
    {
        EditorGUILayout.BeginHorizontal();
        for (int j = 0; j < currentLevel.width; j++)
        {
            int index = i * currentLevel.width + j;
            LevelData.LevelGridCell cell = currentLevel.gridCells[index];
            string label = cell.isBlocked ? "B" : (cell.isTunnel ? $"{cell.tunnelSize}" : (cell.isOccupied ? "X" : "O"));
            Color cellColor = cell.isOccupied ? cell.passengerColor : (cell.isBlocked ? Color.gray : (cell.isTunnel ? Color.cyan : Color.white));
            GUI.backgroundColor = cellColor;

            if (GUILayout.Button(label, GUILayout.Width(570 / Mathf.Max(currentLevel.height,currentLevel.width)), GUILayout.Height(570 / Mathf.Max(currentLevel.height, currentLevel.width))))
            {
                    if (cell.isTunnel)
                    {
                        selectedTunnelCell = cell; 
                    }
                    else
                    {
                        selectedTunnelCell = null; 
                    }

                    HandleCellInteraction(ref cell, i); 
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();
    }
}




    private void SetCellAsTunnel(ref LevelData.LevelGridCell cell)
    {
        cell.isTunnel = true;
        cell.isBlocked = false;
        cell.isOccupied = false;
        Debug.Log("Tunnel placed at cell.");
    }

    void HandleCellInteraction(ref LevelData.LevelGridCell cell, int rowIndex)
    {
        if (Event.current.button == 0) 
        {
            
            if (AddTunnelMode && !cell.isBlocked && !cell.isTunnel)
            {
                // E�er ilk sat�rdaysa t�nel eklemeyi engelle
                if (rowIndex != 0)
                {
                    cell.isTunnel = true;
                    cell.isOccupied = false;  
                    cell.isBlocked = false;
                    Debug.Log("Tunnel placed at cell.");
                }
            }
            else if (!cell.isBlocked && !cell.isTunnel)
            {
                // Normal modda, t�nel veya engel yoksa, h�creyi doldur et veya bo�alt
                cell.isOccupied = !cell.isOccupied;
                if (cell.isOccupied)
                {
                    cell.passengerColor = availableColors[selectedColorIndex];  // Se�ili renk ile yolcu rengini ayarla
                }
                else
                {
                    cell.passengerColor = Color.clear;  // Yolcu h�cresini bo�alt�rken rengi temizle
                }
            }
        }
        else if (Event.current.button == 1) // Sa� t�k kontrol�
        {
            // Engelleme veya engel kald�rma i�lemleri
            cell.isBlocked = !cell.isBlocked;
            if (cell.isBlocked)
            {
                cell.isOccupied = false;
                cell.isTunnel = false;
                cell.passengerColor = Color.clear; 
            }
        }
    }





    private bool CheckColorMultiplesOfThree()
    {
        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

       
        foreach (var cell in currentLevel.gridCells)
        {
            if (cell.isOccupied)
            {
                if (colorCount.ContainsKey(cell.passengerColor))
                    colorCount[cell.passengerColor]++;
                else
                    colorCount[cell.passengerColor] = 1;
            }
        }

       
        foreach (var count in colorCount.Values)
        {
            if (count % 3 != 0) // E�er ���n kat� de�ilse
            {
                return false; // Kaydetmeye izin verme
            }
        }

        return true; 
    }

    private void RandomFillGrid()
    {
        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
        List<int> emptyCellIndices = new List<int>();

        // Mevcut renk say�mlar�n� hesapla ve bo� h�cre indekslerini topla
        for (int i = 0; i < currentLevel.gridCells.Length; i++)
        {
            var cell = currentLevel.gridCells[i];
            if (!cell.isOccupied && !cell.isBlocked && !cell.isTunnel)
            {
                emptyCellIndices.Add(i);
            }
            if (cell.isOccupied)
            {
                if (colorCount.ContainsKey(cell.passengerColor))
                    colorCount[cell.passengerColor]++;
                else
                    colorCount[cell.passengerColor] = 1;
            }
            if (cell.isTunnel && cell.tunnelPassengerColors.Count > 0)
            {
                foreach (Color color in cell.tunnelPassengerColors)
                {
                    if (colorCount.ContainsKey(color))
                        colorCount[color]++;
                    else
                        colorCount[color] = 1;
                }
            }
        }

        // Mevcut renklerin 3'�n kat�na tamamlanmas�
        foreach (var pair in new Dictionary<Color, int>(colorCount))
        {
            int remainder = pair.Value % 3;
            if (remainder != 0)
            {
                int neededToAdd = 3 - remainder;
                if (emptyCellIndices.Count < neededToAdd) return; // Yeterli bo� h�cre yoksa i�lemi bitir

                for (int i = 0; i < neededToAdd; i++)
                {
                    int randomIndex = emptyCellIndices[UnityEngine.Random.Range(0, emptyCellIndices.Count)];
                    emptyCellIndices.Remove(randomIndex);
                    currentLevel.gridCells[randomIndex].isOccupied = true;
                    currentLevel.gridCells[randomIndex].passengerColor = pair.Key;
                    colorCount[pair.Key]++;
                }
            }
        }

        // Kalan bo� h�creleri rastgele renklerle 3'�n kat� �ekilde doldurma
        int numColors = availableColors.Length;
        while (emptyCellIndices.Count >= 3)
        {
            Color randomColor = availableColors[UnityEngine.Random.Range(0, numColors)];
            for (int i = 0; i < 3; i++) // Her renkten 3 adet ekleyerek doldur
            {
                if (emptyCellIndices.Count == 0) break;
                int randomIndex = emptyCellIndices[UnityEngine.Random.Range(0, emptyCellIndices.Count)];
                emptyCellIndices.Remove(randomIndex);
                currentLevel.gridCells[randomIndex].isOccupied = true;
                currentLevel.gridCells[randomIndex].passengerColor = randomColor;
                if (!colorCount.ContainsKey(randomColor)) colorCount[randomColor] = 0;
                colorCount[randomColor]++;
            }
        }
    }




}
