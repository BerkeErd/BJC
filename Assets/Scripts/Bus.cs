using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public bool activeBus;
    public Color busColor;

    [SerializeField] private float speed;

    public Vector3 destination;

    private BusManager manager;

    private int passengersInside = 0;

    private bool isCheckedForWaitingPassengers = false;

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
                if(grid.passengerOnGrid.PassengerColor == busColor && passengersInside < 3) 
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

    public void GetPassengerIn(Passenger p)
    {
        passengersInside += 1;
        if(passengersInside >= 3)
        {
            manager.BusDeparted(this);
            ResetBus();
        }
    }

    public void ResetBus()
    {
        isCheckedForWaitingPassengers = false;
        passengersInside = 0;
    }

}
