using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public bool activeBus;
    public Color busColor;

    public float speed;

    public Vector3 destination;

    private BusManager manager;

    private bool isCheckedForWaitingPassengers = false;

    public List<Passenger> Passengers = new List<Passenger>();

    private void Start()
    {
        manager = FindObjectOfType<BusManager>();
    }

    private void Update()
    {
        if(activeBus)
        {
            manager.currentBus = this;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, destination) < 0.1f)
            {
                ArriveAtDestination();
            }
        }
    }

    private void ArriveAtDestination()
    {
        if(!isCheckedForWaitingPassengers)
        {
           isCheckedForWaitingPassengers = true;
           CheckWaitingPassengers();
        }
        
    }

    private void CheckWaitingPassengers()
    {
        foreach (var grid in GameObject.FindObjectsOfType<WaitingGrid>())
        {
            if (!grid.isEmpty)
            {
                if(grid.passengerOnGrid.PassengerColor == busColor && !isFull()) 
                {
                    grid.passengerOnGrid.GetInsideofBus(this);
                    grid.isEmpty = true;
                    grid.passengerOnGrid = null;
                }
            }
        }
    }

    public void SetColor(Color color)
    {
        busColor = color;
        GetComponentInChildren<Renderer>().material.color = color;
    }

    public void InitializePosition(Vector3 position)
    {
        transform.position = position;
    }

    public void IncreasePassengerCount(Passenger p)
    {
        Passengers.Add(p);
    }

    public bool isFull()
    {
        return Passengers.Count >= 3;
    }

    public bool ReadyToMove()
    {
        foreach (var passenger in Passengers)
        {
            if (passenger.isMoving)
                return false;
        }
        return true;
    }

    public void GetPassengerIn(Passenger p)
    {
        if(isFull() && ReadyToMove())
        {
            manager.BusDeparted(this);
            ResetBus();
        }
    }

    public void ResetBus()
    {
        Passengers = new List<Passenger>();
        isCheckedForWaitingPassengers = false;
    }

}
