using UnityEngine;

public class SkeletonVisualizer : MonoBehaviour
{
    public PosePipeline pipeline; // Arrastra tu PosePipeline
    public GameObject bonePrefab; // Un cilindro o esfera pequeña
    private GameObject[] visualPoints = new GameObject[33];

    void Start()
    {
        for (int i = 0; i < 33; i++)
        {
            visualPoints[i] = Instantiate(bonePrefab, transform);
        }
    }

    void Update()
    {
        if (pipeline.lastLandmarks == null) return;

        for (int i = 0; i < pipeline.lastLandmarks.Length; i++)
        {
            // Convertir coordenadas 0-1 a coordenadas de pantalla y luego a 3D
            Vector3 point = pipeline.lastLandmarks[i];

            // Invertimos Y porque MediaPipe suele venir invertido respecto a Unity
            float screenX = point.x * Screen.width;
            float screenY = (1 - point.y) * Screen.height;

            // Proyectamos a una distancia fija (ej. 2 metros frente al usuario)
            // En realidad mixta, esto requiere ajuste de profundidad dinámico
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenX, screenY, 2.0f));

            visualPoints[i].transform.position = worldPos;
        }
    }
}