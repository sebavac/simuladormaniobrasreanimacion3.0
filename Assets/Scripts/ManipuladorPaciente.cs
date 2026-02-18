using UnityEngine;

public class ManipuladorPaciente : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetoAMover; // Arrastra aquí el 'PoseManager'
    public OVRHand manoIzquierda;
    public OVRHand manoDerecha;

    [Header("Configuración")]
    public float distanciaAgarre = 0.1f; // 10cm para poder agarrarlo
    public Material materialNormal;
    public Material materialAgarrado;

    private bool _agarrado = false;
    private OVRHand _manoActiva;
    private Vector3 _offset; // Para que no salte al centro de la mano
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (materialNormal == null) materialNormal = _renderer.material;
    }

    void Update()
    {
        if (_agarrado)
        {
            // 1. Mover el objeto mientras se mantiene el pellizco
            if (_manoActiva != null && _manoActiva.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                // Mover el PoseManager manteniendo la distancia relativa (offset)
                objetoAMover.position = _manoActiva.PointerPose.position + _offset;
            }
            else
            {
                Soltar();
            }
        }
        else
        {
            // 2. Detectar intento de agarre
            VerificarAgarre(manoDerecha);
            VerificarAgarre(manoIzquierda);
        }
    }

    void VerificarAgarre(OVRHand mano)
    {
        if (!mano.IsTracked) return;

        // Distancia entre la esfera y el "puntero" de la mano (entre pulgar e índice)
        float distancia = Vector3.Distance(transform.position, mano.PointerPose.position);

        if (distancia < distanciaAgarre)
        {
            // Si está cerca Y hace el gesto de pinza (Pinch)
            if (mano.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                Agarrar(mano);
            }
        }
    }

    void Agarrar(OVRHand mano)
    {
        _agarrado = true;
        _manoActiva = mano;

        // Calculamos la diferencia para que el arrastre sea suave
        _offset = objetoAMover.position - mano.PointerPose.position;

        if (materialAgarrado != null) _renderer.material = materialAgarrado;
    }

    void Soltar()
    {
        _agarrado = false;
        _manoActiva = null;
        _renderer.material = materialNormal;
    }
}