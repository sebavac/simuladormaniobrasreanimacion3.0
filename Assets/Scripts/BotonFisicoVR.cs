using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BotonFisicoVR : MonoBehaviour
{
    [Header("Manos VR")]
    public OVRHand manoIzquierda;
    public OVRHand manoDerecha;

    [Header("Configuración")]
    [Tooltip("Distancia en metros para considerar el toque (0.04 = 4cm)")]
    public float distanciaToque = 0.04f;

    [Header("¿Qué debe hacer al tocarse?")]
    public UnityEvent alPresionar;

    private Button miBotonUI;
    private Color colorNormal;
    private bool yaPresionado = false;

    void Start()
    {
        miBotonUI = GetComponent<Button>();
        if (miBotonUI != null)
        {
            colorNormal = miBotonUI.colors.normalColor;
        }
    }

    void Update()
    {
        // Si no hay ninguna mano detectada, no hacemos nada
        if ((manoIzquierda == null || !manoIzquierda.IsTracked) &&
            (manoDerecha == null || !manoDerecha.IsTracked))
        {
            return;
        }

        // Posición central de tu botón en el mundo 3D
        Vector3 posBoton = transform.position;

        // Calculamos la distancia de cada mano al botón
        float distIzq = (manoIzquierda != null && manoIzquierda.IsTracked) ? Vector3.Distance(posBoton, manoIzquierda.PointerPose.position) : 999f;
        float distDer = (manoDerecha != null && manoDerecha.IsTracked) ? Vector3.Distance(posBoton, manoDerecha.PointerPose.position) : 999f;

        // Si alguna mano entra en el área del botón (por defecto 4 centímetros)
        if (distIzq < distanciaToque || distDer < distanciaToque)
        {
            if (!yaPresionado)
            {
                yaPresionado = true;

                // Feedback visual: oscurecemos el botón para saber que lo tocamos
                if (miBotonUI != null)
                {
                    ColorBlock cb = miBotonUI.colors;
                    cb.normalColor = Color.gray;
                    miBotonUI.colors = cb;
                }

                // Dispara el evento (en tu caso, Girar el maniquí)
                alPresionar.Invoke();
            }
        }
        else
        {
            // Cuando la mano se aleja, reseteamos el botón
            if (yaPresionado)
            {
                yaPresionado = false;

                // Vuelve a su color original
                if (miBotonUI != null)
                {
                    ColorBlock cb = miBotonUI.colors;
                    cb.normalColor = colorNormal;
                    miBotonUI.colors = cb;
                }
            }
        }
    }
}