using UnityEngine;

public class PoseSkeletonVisualizer : MonoBehaviour
{
    public PosePipeline pipeline;
    public GameObject jointPrefab;

    [Header("Ajustes Visuales")]
    [Tooltip("Distancia (Z) desde el origen del contenedor. 0 para plano.")]
    public float distanciaProfundidad = 0f;

    [Tooltip("Tamaño de los puntos.")]
    public float escalaPuntos = 0.03f;

    [Header("Calibración")]
    [Tooltip("Marca esto si te ves como en un espejo.")]
    public bool modoEspejo = true;
    [Tooltip("¡MARCA ESTO SI EL ESQUELETO ESTÁ DE CABEZA!")]
    public bool invertirY = false; // <--- AQUÍ ESTÁ LA OPCIÓN QUE FALTABA

    [Header("Transformación del Maniquí")]
    public Vector3 posicionManiqui = new Vector3(0, 0, 1.5f);
    public Vector3 rotacionManiqui = new Vector3(0, 0, 0);
    public float escala = 1.0f;

    private GameObject _contenedor;
    private GameObject[] _joints = new GameObject[33];

    void Start()
    {
        if (_contenedor != null) Destroy(_contenedor);
        _contenedor = new GameObject("Contenedor_Esqueleto_Paciente");
        _contenedor.transform.SetParent(transform, false);

        for (int i = 0; i < 33; i++)
        {
            _joints[i] = Instantiate(jointPrefab, _contenedor.transform);
            _joints[i].name = $"Joint_{i}";
            _joints[i].transform.localScale = Vector3.one * escalaPuntos;
        }
    }

    void Update()
    {
        // Actualizamos la transformación del contenedor padre
        _contenedor.transform.localPosition = posicionManiqui;
        _contenedor.transform.localEulerAngles = rotacionManiqui;
        _contenedor.transform.localScale = Vector3.one * escala;

        if (pipeline == null || pipeline.lastLandmarks == null || pipeline.lastLandmarks.Length < 33) return;

        for (int i = 0; i < _joints.Length; i++)
        {
            Vector3 raw = pipeline.lastLandmarks[i];

            // 1. Lógica Espejo (X)
            float rawX = modoEspejo ? (1.0f - raw.x) : raw.x;

            // 2. Lógica Invertir Vertical (Y) - CORREGIDA
            // BlazePose da Y=0 arriba y Y=1 abajo. Unity es al revés.
            // Si invertirY es FALSE, hacemos (1 - raw.y) para corregirlo.
            // Si invertirY es TRUE, usamos raw.y directo para voltearlo.
            float rawY = invertirY ? raw.y : (1.0f - raw.y);

            // Conversión a Metros (Centrado en 0,0) y aplanado (Z=0)
            float x = (rawX - 0.5f) * 2.0f;
            float y = (rawY - 0.5f) * 1.5f;
            float z = distanciaProfundidad; // Usamos 0 para que sea plano

            _joints[i].transform.localPosition = new Vector3(x, y, z);

            // Solo mostramos si la confianza es suficiente
            _joints[i].SetActive(pipeline.lastConfidence[i] > 0.3f);
        }
    }

    // Método para que otros scripts sepan dónde están los huesos
    public Vector3 ObtenerPosicionHueso(int indice)
    {
        if (_joints != null && indice >= 0 && indice < _joints.Length && _joints[indice] != null)
        {
            return _joints[indice].transform.position;
        }
        return Vector3.zero;
    }
}