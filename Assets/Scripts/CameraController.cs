using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform levelContainer; // Levelinizin transformunu referans al�n
    public float cameraHeight = 50.0f; // Kamera y�ksekli�i
    public float offsetZ = -10.0f; // Kameran�n levelin arkas�na ne kadar uzakta olaca��

    void Start()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        // Kamera, levelin merkezine bakacak �ekilde ayarlan�yor
        Vector3 levelSize = levelContainer.GetComponent<Renderer>().bounds.size;
        Vector3 levelCenter = levelContainer.position;

        // Kamera y�ksekli�ini ve pozisyonunu ayarla
        Vector3 cameraPosition = new Vector3(levelCenter.x, cameraHeight, levelCenter.z + offsetZ - levelSize.z / 2);
        transform.position = cameraPosition;

        // Kameran�n levelin merkezine bakmas�n� sa�la
        transform.LookAt(levelCenter);
    }

 
        void LateUpdate()
        {
            if (levelContainer.GetComponent<Renderer>() != null)
            {
                Bounds bounds = levelContainer.GetComponent<Renderer>().bounds;
                transform.position = new Vector3(bounds.center.x, bounds.size.y, bounds.center.z - bounds.size.z);
                transform.LookAt(bounds.center);
            }
        }
    

}

