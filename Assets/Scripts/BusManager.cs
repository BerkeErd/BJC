using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BusManager : MonoBehaviour
{
   
    [SerializeField] private List<Color> busColors;
    [SerializeField] private PassengerManager passengerManager; // PassengerManager referansý

    public Vector3 busSpawnPoint;
    public Vector3 busDespawnPoint;
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
        var colorCounts = passengerColors
            .GroupBy(color => color)
            .Select(group => new { Color = group.Key, Count = group.Count() })
            .Where(colorInfo => colorInfo.Count > 0) 
            .OrderByDescending(colorInfo => colorInfo.Count)
            .ToList();

        List<Color> mostCommonColors = colorCounts
            .Take(2) // En çok tekrar eden üst 2 rengi al
            .Select(colorInfo => colorInfo.Color)
            .ToList();
        
        Color selectedColor = Color.black; // Varsayýlan renk olarak siyah belirliyoruz.

        // Deneme sayýsýný sýnýrlayarak sonsuz döngüden kaçýn
        int attempts = 0;
        while (!busColors.Contains(selectedColor) && attempts < 10)
        {
            if (mostCommonColors.Count > 0)
            {
                selectedColor = mostCommonColors[Random.Range(0,mostCommonColors.Count)];
            }
            attempts++;
        }

        // Eðer uygun renk bulunamazsa, bir varsayýlan renk kullan
        if (!busColors.Contains(selectedColor))
        {
            selectedColor = busColors.Any() ? busColors[Random.Range(0,busColors.Count)] : Color.black;
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
        for (int i = 0; i < Mathf.Min(totalBusToSpawn,3); i++)
        {
            SpawnInitialBuses();
        }
        ActivateFrontBus();
    }

    private void SpawnInitialBuses()
    {
        GameObject newBusObj = ObjectPooler.Instance.SpawnFromPool("Bus", busSpawnPoint, Quaternion.Euler(0, 90, 0));
        Bus newBus = newBusObj.GetComponent<Bus>();
        newBus.destination = waitingSpot;
        
        newBus.InitializePosition(busSpawnPoint);
        busQueue.Enqueue(newBus);
        

        busSpawned++;
    }

    // Otobüs rengini ayarlayan metod
    private void SetBusColor(Bus bus)
    {
        if(!bus.isColorSet)
        {
            Color busColor = GetAColorForBus();
            busColors.Remove(busColor);
            bus.SetColor(busColor);
            bus.isColorSet = true;
        }
    }
    
    public void BusDeparted(Bus bus)
    {
        currentBus = null;
        bus.activeBus = false;
        StartCoroutine(MoveBusToDespawnPoint(bus));
    }

    private IEnumerator MoveBusToDespawnPoint(Bus bus)
    {
        Vector3 targetPosition = busDespawnPoint;
        bool isMoving = true;

        while (isMoving)
        {
            float step = bus.speed * Time.deltaTime; // Adým hýzý, isterseniz ayarlayabilirsiniz
            bus.transform.position = Vector3.MoveTowards(bus.transform.position, targetPosition, step);

            if (Vector3.Distance(bus.transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                ProcessBusDeparture(bus);
            }

            yield return null;
        }
    }

    private void ProcessBusDeparture(Bus bus)
    {
        bus.ResetBus();

        if (busSpawned < totalBusToSpawn)
        {
            bus.InitializePosition(busSpawnPoint);  // Otobüsü baþlangýç noktasýna geri gönder
            busQueue.Enqueue(bus);
            busQueue.Dequeue();  // Kuyruktaki otobüsü çýkar
            ActivateFrontBus();  // Kuyruðun baþýndaki yeni otobüsü aktif yap
            busSpawned++;
        }
        else
        {
            busQueue.Dequeue();
            ObjectPooler.Instance.ReturnToPool("Bus", bus.gameObject);
            ActivateFrontBus();  // Kuyruðun baþýndaki yeni otobüsü aktif yap
        }


    }

    private void ActivateFrontBus()
    {
        foreach (Bus bus in busQueue)
        {
            bus.activeBus = false;  
            bus.nextBus = false;  
        }

        if (busQueue.Count > 0)
        {
            Bus frontBus = busQueue.Peek();
            frontBus.activeBus = true;  // Kuyruðun baþýndaki otobüsü aktif yap
            SetBusColor(frontBus);

            if (busQueue.Count > 1)
            {
                Bus nextBus = busQueue.ElementAt(1);
                nextBus.nextBus = true;
                SetBusColor(nextBus);
            }
        }
        else
        {
            GameController.Instance.WinLevel();
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





