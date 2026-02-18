using UnityEngine;

public class SystemCalibration : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform sistemaPaciente; // El objeto padre 'SISTEMA_PACIENTE'
    public Transform botonEsfera;     // La esfera que usaremos de botón

    [Header("Manos VR")]
    public OVRHand manoIzquierda;
    public OVRHand manoDerecha;

    [Header("Configuración Visual")]
    public Material matBloqueado; // Verde (Listo)
    public Material matEditando;  // Rojo/Naranja (Moviendo)
    public float radioBoton = 0.1f;

    private bool _modoEdicion = false;
    private bool _estaPellizcando = false;
    private OVRHand _manoActiva;
    private Vector3 _offsetPosicion;
    private Quaternion _offsetRotacion;
    private Renderer _rendererBoton;

    // Cooldown para no activar/desactivar el botón 100 veces por segundo
    private float _cooldownBoton = 0f;

    void Start()
    {
        _rendererBoton = botonEsfera.GetComponent<Renderer>();
        ActualizarColor();
    }

    void Update()
    {
        if (_cooldownBoton > 0) _cooldownBoton -= Time.deltaTime;

        // 1. LÓGICA DEL BOTÓN (Tocar la esfera para cambiar modo)
        CheckBoton(manoDerecha);
        CheckBoton(manoIzquierda);

        // 2. LÓGICA DE MOVIMIENTO (Solo si estamos en Modo Edición)
        if (_modoEdicion)
        {
            GestionarMovimiento();
        }
    }

    void CheckBoton(OVRHand mano)
    {
        if (!mano.IsTracked || _cooldownBoton > 0) return;

        float distancia = Vector3.Distance(mano.PointerPose.position, botonEsfera.position);

        // Si tocamos el botón (con el dedo índice o palma cerca)
        if (distancia < radioBoton)
        {
            ToggleModo();
            _cooldownBoton = 1.0f; // Esperar 1 segundo antes de poder tocarlo otra vez
        }
    }

    void ToggleModo()
    {
        _modoEdicion = !_modoEdicion;
        ActualizarColor();

        // Si salimos del modo edición, soltamos todo
        if (!_modoEdicion)
        {
            _estaPellizcando = false;
            _manoActiva = null;
        }
    }

    void ActualizarColor()
    {
        if (_rendererBoton != null)
        {
            _rendererBoton.material = _modoEdicion ? matEditando : matBloqueado;
        }
    }

    void GestionarMovimiento()
    {
        // A. DETECTAR INICIO DE PELLIZCO (En cualquier mano)
        if (!_estaPellizcando)
        {
            if (CheckPinch(manoDerecha)) IniciarArrastre(manoDerecha);
            else if (CheckPinch(manoIzquierda)) IniciarArrastre(manoIzquierda);
        }
        // B. MOVER MIENTRAS SE PELLIZCA
        else if (_manoActiva != null)
        {
            // Si sigue pellizcando...
            if (_manoActiva.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                // Movemos el sistema manteniendo el Offset calculado al inicio
                // Esto hace que se sienta como "agarrar el aire" y arrastrar el mundo
                sistemaPaciente.position = _manoActiva.PointerPose.position + _offsetPosicion;

                // Opcional: Si quieres rotarlo también con la muñeca
                // sistemaPaciente.rotation = _manoActiva.PointerPose.rotation * _offsetRotacion;
            }
            else
            {
                // Soltó el pellizco
                _estaPellizcando = false;
                _manoActiva = null;
            }
        }
    }

    bool CheckPinch(OVRHand mano)
    {
        return mano.IsTracked && mano.GetFingerIsPinching(OVRHand.HandFinger.Index);
    }

    void IniciarArrastre(OVRHand mano)
    {
        _estaPellizcando = true;
        _manoActiva = mano;

        // Calculamos la diferencia entre donde está la mano y donde está el objeto
        // Así, cuando muevas la mano, el objeto se moverá RELATIVO a esa distancia.
        // ¡Esto permite mover objetos lejanos sin tocarlos!
        _offsetPosicion = sistemaPaciente.position - mano.PointerPose.position;
        _offsetRotacion = Quaternion.Inverse(mano.PointerPose.rotation) * sistemaPaciente.rotation;
    }
}