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
                if (CheckColorMultiplesOfThree()) // Renk sayýmlarýný kontrol et
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

}
