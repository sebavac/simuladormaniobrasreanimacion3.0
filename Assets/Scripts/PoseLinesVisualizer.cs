using UnityEngine;
using System.Collections.Generic;

public class PoseLinesVisualizer : MonoBehaviour
{
    public PosePipeline pipeline;
    public GameObject linePrefab;

    [Header("Sincronización")]
    // ESTA ES LA REFERENCIA CLAVE QUE FALTABA EN TUS CAPTURAS
    public PoseSkeletonVisualizer esqueletoMaestro;

    private GameObject _contenedorLineas;
    private List<LineRenderer> _lines = new List<LineRenderer>();

    private readonly int[,] _connections = new int[,]
    {
        {11, 12}, {11, 13}, {13, 15}, {12, 14}, {14, 16}, // Brazos
        {11, 23}, {12, 24}, {23, 24}, // Torso
        {23, 25}, {25, 27}, {24, 26}, {26, 28} // Piernas
    };

    void Start()
    {
        if (_contenedorLineas != null) Destroy(_contenedorLineas);
        _contenedorLineas = new GameObject("Contenedor_Lineas");
        // Importante: No lo hacemos hijo del PoseManager para que no herede doble transformación
        // _contenedorLineas.transform.SetParent(transform, false); 

        for (int i = 0; i < _connections.GetLength(0); i++)
        {
            GameObject lineObj = Instantiate(linePrefab, _contenedorLineas.transform);
            if (lineObj.TryGetComponent<LineRenderer>(out LineRenderer lr))
            {
                lr.useWorldSpace = true; // Las líneas deben usar coordenadas mundiales
                lr.positionCount = 2;
                lr.startWidth = 0.015f;
                lr.endWidth = 0.015f;
                _lines.Add(lr);
            }
        }
    }

    void LateUpdate()
    {
        // Si no hay maestro o no hay detección, no hacemos nada
        if (esqueletoMaestro == null || pipeline == null || pipeline.lastConfidence == null) return;

        for (int i = 0; i < _lines.Count; i++)
        {
            int startIdx = _connections[i, 0];
            int endIdx = _connections[i, 1];

            // Le preguntamos a los PUNTOS dónde están exactamente
            Vector3 posA = esqueletoMaestro.ObtenerPosicionHueso(startIdx);
            Vector3 posB = esqueletoMaestro.ObtenerPosicionHueso(endIdx);

            // Si ambos puntos existen (no son 0,0,0), dibujamos la línea entre ellos
            if (posA != Vector3.zero && posB != Vector3.zero)
            {
                _lines[i].SetPosition(0, posA);
                _lines[i].SetPosition(1, posB);
                _lines[i].enabled = true;
            }
            else
            {
                _lines[i].enabled = false;
            }
        }
    }
}