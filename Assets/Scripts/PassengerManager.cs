using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour
{
    private List<Passenger> allPassengers = new List<Passenger>();
    public List<Passenger> activePassengers = new List<Passenger>();
    private List<Passenger> passengersWithPaths = new List<Passenger>();

    public Passenger lastPassengerWentToGrid = null;

    [SerializeField] private Material passengerCanMoveMat;
    [SerializeField] private Material passengerDefaultMat;


    public int GetTotalMovingPassengers()
    {
        int movingPassengersCount = 0;
        foreach (var passanger in allPassengers)
        {
            if(passanger.isMoving)
            {
                movingPassengersCount += 1;
            }
        }

        return movingPassengersCount;
    }


    // Toplam yolcu sayýsýný döndürür
    public int GetTotalPassengerCount()
    {
        return allPassengers.Count;
    }

    public List<Color> getPassengersWithPathColors()
    {
        List<Color> Colors = new List<Color>();

        foreach (var passenger in passengersWithPaths)
        {
            Colors.Add(passenger.PassengerColor);
        }

        return Colors;
    }


    void OnEnable()
    {
        GameController.OnGameStart += HandleGameStart;
        Passenger.OnPassengerMoved += HandlePassengerMoved;
    }

    void OnDisable()
    {
        GameController.OnGameStart -= HandleGameStart;
        Passenger.OnPassengerMoved -= HandlePassengerMoved;
    }

    private void HandleGameStart()
    {
        UpdatePassengersWithPaths();
    }

    public void ActivatePassenger(Passenger passenger)
    {
        if (!activePassengers.Contains(passenger))
        {
            activePassengers.Add(passenger);
        }
    }

    public void DeactivatePassenger(Passenger passenger)
    {
        activePassengers.Remove(passenger);
        passengersWithPaths.Remove(passenger);
    }

    public void RegisterPassenger(Passenger passenger)
    {
        if (!allPassengers.Contains(passenger))
        {
            allPassengers.Add(passenger);
        }
    }

    public void UnregisterPassenger(Passenger passenger)
    {
        allPassengers.Remove(passenger);
        activePassengers.Remove(passenger);
        passengersWithPaths.Remove(passenger);
    }

    public void UpdatePassengersWithPaths()
    {
        passengersWithPaths.Clear(); 
        foreach (var passenger in activePassengers)
        {
            Renderer renderer = passenger.GetComponentInChildren<Renderer>();
            Color originalColor = renderer.material.color; 

            if (passenger.CanMoveToFirstRow())
            {
                renderer.material = passengerCanMoveMat;
                renderer.material.color = originalColor; 
                passengersWithPaths.Add(passenger);
            }
            else
            {
                renderer.material = passengerDefaultMat;
                renderer.material.color = originalColor;
            }
        }
    }

    public Dictionary<Color, int> CountPassengerColors()
    {
        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();
        foreach (var passenger in allPassengers)
        {
            if (colorCounts.ContainsKey(passenger.PassengerColor))
            {
                colorCounts[passenger.PassengerColor]++;
            }
            else
            {
                colorCounts.Add(passenger.PassengerColor, 1);
            }
        }
        return colorCounts;
    }
    

    private void HandlePassengerMoved(Passenger passenger)
    {
        UpdatePassengersWithPaths();
    }

}
