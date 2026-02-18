using UnityEngine;
using UnityEngine.UI;

public class ControladorCongelamientoVR : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El script que mueve el esqueleto")]
    public PoseSkeletonVisualizer visualizador;

    [Tooltip("La mano que usaremos como botón (ej. Mano Izquierda)")]
    public OVRHand manoBoton;

    [Header("Configuración del Gesto")]
    [Tooltip("Elige qué dedo usar junto al pulgar. Recomendado: Middle (Dedo Medio)")]
    public OVRHand.HandFinger dedoGatillo = OVRHand.HandFinger.Middle; // <--- LA MAGIA ESTÁ AQUÍ

    [Header("Feedback UI (Opcional)")]
    [Tooltip("Un texto para avisarte si está congelado o no")]
    public Text textoEstadoIA;

    private bool _estabaPellizcando = false;

    void Start()
    {
        ActualizarTexto();
    }

    void Update()
    {
        if (visualizador == null || manoBoton == null) return;

        if (manoBoton.IsTracked)
        {
            // Ahora detecta el pellizco con el dedo que hayas elegido en el Inspector
            bool estaPellizcando = manoBoton.GetFingerIsPinching(dedoGatillo);

            if (estaPellizcando && !_estabaPellizcando)
            {
                visualizador.congelarEsqueleto = !visualizador.congelarEsqueleto;
                ActualizarTexto();
            }

            _estabaPellizcando = estaPellizcando;
        }
    }

    void ActualizarTexto()
    {
        if (textoEstadoIA == null) return;

        if (visualizador.congelarEsqueleto)
        {
            textoEstadoIA.text = "IA CONGELADA (LISTO PARA RCP)";
            textoEstadoIA.color = Color.cyan;
        }
        else
        {
            textoEstadoIA.text = "IA BUSCANDO PACIENTE...";
            textoEstadoIA.color = Color.green;
        }
    }
}