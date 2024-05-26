using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform levelContainer; // Levelinizin transformunu referans alýn
    public float cameraHeight = 50.0f; // Kamera yüksekliði
    public float offsetZ = -10.0f; // Kameranýn levelin arkasýna ne kadar uzakta olacaðý

    void Start()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        // Kamera, levelin merkezine bakacak þekilde ayarlanýyor
        Vector3 levelSize = levelContainer.GetComponent<Renderer>().bounds.size;
        Vector3 levelCenter = levelContainer.position;

        // Kamera yüksekliðini ve pozisyonunu ayarla
        Vector3 cameraPosition = new Vector3(levelCenter.x, cameraHeight, levelCenter.z + offsetZ - levelSize.z / 2);
        transform.position = cameraPosition;

        // Kameranýn levelin merkezine bakmasýný saðla
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

