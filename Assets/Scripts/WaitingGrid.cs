using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingGrid : MonoBehaviour
{
    public Passenger passengerOnGrid;
    public bool isEmpty = true;

    public void EmptyGrid()
    {
        isEmpty = true;
        passengerOnGrid = null;
    }
}
