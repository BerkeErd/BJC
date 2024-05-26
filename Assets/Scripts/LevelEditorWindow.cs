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

    private void ClearGrid()
    {
        foreach (var cell in currentLevel.gridCells)
        {
            cell.isOccupied = false;
            cell.isBlocked = false;  
            cell.passengerColor = Color.clear; 
        }
    }


    private void DrawBusAndWaitingArea()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space((float)currentLevel.waitingAreaSize/2 * 50); // Ortaya hizalamak için
        GUILayout.Label(new GUIContent("BUS"), GUILayout.Width(150), GUILayout.Height(50)); 
        EditorGUILayout.EndHorizontal();

       
        EditorGUILayout.BeginHorizontal();
        for (int j = 0; j < currentLevel.waitingAreaSize; j++) 
        {
            GUILayout.Box("WAIT", GUILayout.Width(50), GUILayout.Height(50));
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLevelGrid()
    {
        for (int i = 0; i < currentLevel.height; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < currentLevel.width; j++)
            {
                int index = i * currentLevel.width + j;
                LevelData.LevelGridCell cell = currentLevel.gridCells[index];
                string label = cell.isBlocked ? "Blocked" : (cell.isOccupied ? "X" : "O");
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = cell.isOccupied ? cell.passengerColor : (cell.isBlocked ? Color.black : Color.white);

                if (GUILayout.Button(label, GUILayout.Width(50), GUILayout.Height(50)))
                {
                    HandleCellInteraction(ref cell);
                }

                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void HandleCellInteraction(ref LevelData.LevelGridCell cell)
    {
        if (Event.current.button == 0 && !cell.isBlocked) 
        {
            cell.isOccupied = !cell.isOccupied;
            if (cell.isOccupied)
            {
                cell.passengerColor = availableColors[selectedColorIndex];
            }
        }
        else if (Event.current.button == 1)
        {
            cell.isBlocked = !cell.isBlocked;
            if (cell.isBlocked)
            {
                cell.isOccupied = false; 
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
            if (count % 3 != 0) // Eðer üçün katý deðilse
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

        // Mevcut renk sayýmlarýný hesapla ve boþ hücre indekslerini topla
        for (int i = 0; i < currentLevel.gridCells.Length; i++)
        {
            if (!currentLevel.gridCells[i].isOccupied && !currentLevel.gridCells[i].isBlocked)
            {
                emptyCellIndices.Add(i);
            }
            else if (currentLevel.gridCells[i].isOccupied)
            {
                if (colorCount.ContainsKey(currentLevel.gridCells[i].passengerColor))
                    colorCount[currentLevel.gridCells[i].passengerColor]++;
                else
                    colorCount[currentLevel.gridCells[i].passengerColor] = 1;
            }
        }

        // Mevcut renklerin 3'ün katýna tamamlanmasý
        foreach (var pair in new Dictionary<Color, int>(colorCount))
        {
            int remainder = pair.Value % 3;
            if (remainder != 0)
            {
                int neededToAdd = 3 - remainder;
                if (emptyCellIndices.Count < neededToAdd) return; // Yeterli boþ hücre yoksa iþlemi bitir

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

        // Kalan boþ hücreleri rastgele renklerle 3'ün katý þekilde doldurma
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
