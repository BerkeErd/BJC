using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public bool activeBus;
    public bool nextBus;

    public bool isColorSet = false;
    public Color busColor;

    public float speed;

    public Vector3 destination;

    private BusManager manager;

    private bool isCheckedForWaitingPassengers = false;

    private List<Passenger> Passengers = new List<Passenger>();

    public List<GameObject> PassengerChairs;

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
        else if(nextBus)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination - Vector3.right * 10f, speed * Time.deltaTime);

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
                    grid.passengerOnGrid.MoveToBusFromWaitingGrid(this);
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
        Debug.Log($"Passenger count: {Passengers.Count}");
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
        if (!Passengers.Contains(p) && !isFull())
        {
            IncreasePassengerCount(p);
        }
        SitPassengerToChair(p);
        CheckDepartCondition();
    }

    private void CheckDepartCondition()
    {
        if (isFull() && ReadyToMove())
        {
            manager.BusDeparted(this);
        }
    }

    private void SitPassengerToChair(Passenger p)
    {
        p.PlaySpawnAnimation();
        p.gameObject.transform.position = PassengerChairs[Passengers.IndexOf(p)].transform.position;
        p.transform.rotation = Quaternion.Euler(0, 90, 0);
        p.transform.parent = transform;
    }

    public void ResetBus()
    {
        activeBus = false;
        ResetChairPassengers();
        nextBus = false;
        isColorSet = false;
        Passengers = new List<Passenger>();
        isCheckedForWaitingPassengers = false;
    }

    public void ResetChairPassengers()
    {
        foreach (var passenger in Passengers)
        {
            ObjectPooler.Instance.ReturnToPool("Passenger", passenger.gameObject);
        }
    }

}
