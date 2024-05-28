using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BusManager : MonoBehaviour
{
   
    [SerializeField] private List<Color> busColors;
    [SerializeField] private PassengerManager passengerManager; // PassengerManager referansý

    public Vector3 busSpawnPoint;
    public Vector3 waitingSpot;

    private Queue<Bus> busQueue = new Queue<Bus>();
    private int totalBusToSpawn;
    private int busSpawned = 0;


    public Bus currentBus;

    void OnEnable()
    {
        GameController.OnGameStart += HandleGameStart;
    }

    void OnDisable()
    {
        GameController.OnGameStart -= HandleGameStart;
    }

    private Color GetAColorForBus()
    {
        List<Color> passengerColors = passengerManager.getPassengersWithPathColors();

        // Renkleri say ve en çok tekrar edenleri sýrala
        var mostCommonColors = passengerColors
            .GroupBy(color => color)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(3)  // En çok tekrar eden üst 3 rengi al
            .ToList();

        Color selectedColor = Color.black;

        while (!busColors.Contains(selectedColor))
        {
           selectedColor = mostCommonColors[Random.Range(0, mostCommonColors.Count)];
        }
        
        return selectedColor;
    }

    private void HandleGameStart()
    {
        ConfigureBusColors();
        StartBusLogic();
    }

    private void StartBusLogic()
    {
        int passengerCount = passengerManager.GetTotalPassengerCount();
        totalBusToSpawn = passengerCount / 3; // Toplam yolcu sayýsýný 3'e böl ve yukarý yuvarla
        Debug.Log(totalBusToSpawn);
        for (int i = 0; i < Mathf.Min(totalBusToSpawn,3); i++)
        {
            SpawnInitialBuses();
        }
    }

    private void SpawnInitialBuses()
    {
        GameObject newBusObj = ObjectPooler.Instance.SpawnFromPool("Bus", busSpawnPoint, Quaternion.Euler(0, 90, 0));
        Bus newBus = newBusObj.GetComponent<Bus>();
        newBus.destination = waitingSpot;
        SetBusColor(newBus);
        newBus.InitializePosition(busSpawnPoint);
        busQueue.Enqueue(newBus);
        if (busQueue.Count == 1)
        {
            ActivateFrontBus();  // Ýlk otobüsü aktif yap
        }

        busSpawned++;
    }

    // Otobüs rengini ayarlayan metod
    private void SetBusColor(Bus bus)
    {

        Color busColor = GetAColorForBus();
        busColors.Remove(busColor);
        bus.SetColor(busColor);
        
    }

    // Bir otobüs depart ettiðinde çaðrýlan metod
    public void BusDeparted(Bus bus)
    {
        if(busSpawned < totalBusToSpawn)
        {
            bus.InitializePosition(busSpawnPoint);  // Otobüsü baþlangýç noktasýna geri gönder
            SetBusColor(bus);
            busQueue.Enqueue(bus);
            busQueue.Dequeue();  // Kuyruktaki otobüsü çýkar
            ActivateFrontBus();  // Kuyruðun baþýndaki yeni otobüsü aktif yap
            busSpawned++;
        }
        else
        {
            busQueue.Dequeue();
            ObjectPooler.Instance.RemoveFromPool("Bus", bus.gameObject);
            ActivateFrontBus();  // Kuyruðun baþýndaki yeni otobüsü aktif yap
        }
        
    }

    // Kuyruðun baþýndaki otobüsü aktif eden metod
    private void ActivateFrontBus()
    {
        foreach (Bus bus in busQueue)
        {
            bus.activeBus = false;  // Tüm otobüsleri önce pasif yap
        }
        if (busQueue.Count > 0)
        {
            busQueue.Peek().activeBus = true;  // Kuyruðun baþýndaki otobüsü aktif yap
        }
    }

    public void ConfigureBusColors()
    {
        Dictionary<Color, int> passengerColorCounts = passengerManager.CountPassengerColors();

        foreach (var item in passengerColorCounts)
        {
            int busCountForColor = item.Value / 3;  // Her 3 yolcu için bir otobüs
            for (int i = 0; i < busCountForColor; i++)
            {
                busColors.Add(item.Key);
            }
        }
    }

}





