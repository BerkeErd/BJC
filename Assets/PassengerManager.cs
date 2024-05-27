using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour
{
    private List<Passenger> allPassengers = new List<Passenger>();
    private List<Passenger> activePassengers = new List<Passenger>();
    private List<Passenger> passengersWithPaths = new List<Passenger>();

    [SerializeField] private Material passengerCanMoveMat;
    [SerializeField] private Material passengerDefaultMat;


    private void Start()
    {
        StartCoroutine(CheckPathsRegularly());
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

    IEnumerator CheckPathsRegularly()
    {
        while (true)
        {
            Debug.Log("Deneme");
            UpdatePassengersWithPaths();
            yield return new WaitForSeconds(1f);  // DEÐÝÞECEK
        }
    }

    void UpdatePassengersWithPaths()
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

}
