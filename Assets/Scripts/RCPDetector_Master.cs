using UnityEngine;
using UnityEngine.UI;

public class RCPDetector_Master : MonoBehaviour
{
    [Header("Referencias Generales")]
    public PoseSkeletonVisualizer visualizadorEsqueleto;
    public Transform objetivoPecho; // El cubo rojo

    [Header("Referencias OVR (Manos y Esqueletos)")]
    public OVRHand manoIzquierda;
    public OVRHand manoDerecha;
    public OVRSkeleton esqueletoIzq; // <--- NUEVO: Arrastra OVRHandPrefab (Left)
    public OVRSkeleton esqueletoDer; // <--- NUEVO: Arrastra OVRHandPrefab (Right)

    [Header("Interfaz UI")]
    public Slider barraCompresion;
    public Text textoFeedback;
    public Image rellenoBarra;

    [Header("Calibración Anatómica")]
    [Range(0f, 0.5f)] public float bajadaEsternon = 0.15f;
    public float alturaCuboOffset = 0.0f;

    [Header("Configuración RCP")]
    public float umbralContacto = 0.15f;

    [Header("Validación 1: Manos Juntas")]
    public bool requerirDosManos = true;
    public float distanciaManosApiladas = 0.12f;

    [Header("Validación 2: Manos Abiertas")]
    public bool validarApertura = true;
    [Tooltip("Distancia mínima entre Muñeca e Índice para considerar mano abierta")]
    public float umbralApertura = 0.07f;

    [Header("Validación 3: Orientación")]
    public bool validarOrientacion = true;
    [Range(0, 90)] public float toleranciaAngulo = 50f;

    [Header("Corrección Física")]
    public bool invertirDireccionCompresion = false;

    void Update()
    {
        // ---------------------------------------------------------
        // 1. POSICIONAMIENTO DEL CUBO (Lógica Original)
        // ---------------------------------------------------------
        Vector3 hombroI = visualizadorEsqueleto.ObtenerPosicionHueso(11);
        Vector3 hombroD = visualizadorEsqueleto.ObtenerPosicionHueso(12);
        Vector3 caderaI = visualizadorEsqueleto.ObtenerPosicionHueso(23);
        Vector3 caderaD = visualizadorEsqueleto.ObtenerPosicionHueso(24);

        if (hombroI == Vector3.zero || caderaI == Vector3.zero)
        {
            ActualizarUI("BUSCANDO PACIENTE...", Color.gray, 0);
            return;
        }

        Vector3 centroHombros = (hombroI + hombroD) / 2f;
        Vector3 centroCaderas = (caderaI + caderaD) / 2f;
        Vector3 direccionColumna = (centroCaderas - centroHombros).normalized;
        Vector3 vectorHombros = (hombroD - hombroI).normalized;
        Vector3 normalPecho = Vector3.Cross(direccionColumna, vectorHombros).normalized;

        if (objetivoPecho != null)
        {
            Vector3 posicionFinal = centroHombros
                                  + (direccionColumna * bajadaEsternon)
                                  + (normalPecho * alturaCuboOffset);
            objetivoPecho.position = posicionFinal;
            // El pecho mira hacia "arriba" (normalPecho) y su "arriba" local apunta a la cabeza (-direccionColumna)
            objetivoPecho.rotation = Quaternion.LookRotation(normalPecho, -direccionColumna);
        }

        // ---------------------------------------------------------
        // 2. DETECCIÓN BÁSICA
        // ---------------------------------------------------------
        OVRHand manoActiva = ObtenerManoActiva(objetivoPecho.position);

        if (manoActiva == null)
        {
            ActualizarUI("MANOS PERDIDAS", Color.gray, 0);
            return;
        }

        Vector3 posMano = manoActiva.PointerPose.position;
        float distanciaAlCubo = Vector3.Distance(posMano, objetivoPecho.position);

        // Chequeo de distancia global
        if (distanciaAlCubo > umbralContacto)
        {
            ActualizarUI("ACÉRCATE AL PECHO", Color.white, 0);
            return;
        }

        // ---------------------------------------------------------
        // 3. VALIDACIONES ESTRICTAS (NUEVO BLOQUE)
        // ---------------------------------------------------------

        // A. Validar que las manos estén abiertas (NO PUÑOS)
        if (validarApertura && !SonManosAbiertas())
        {
            ActualizarUI("ABRE LAS MANOS", Color.yellow, 0);
            return;
        }

        // B. Validar Orientación (Palmas mirando al pecho)
        if (validarOrientacion)
        {
            // El 'forward' del objetivoPecho apunta hacia el cielo (saliendo del esternón).
            // El 'up' de la mano (PointerPose.up) usualmente sale de la palma.
            // Si la palma mira al pecho, el vector Up de la mano y el Forward del pecho son opuestos (180°) 
            // O paralelos dependiendo de tu OVRHand prefab. 
            // Usaremos Transform.up que es más estándar en OVRHandPrefab.

            float angulo = Vector3.Angle(manoActiva.transform.up, objetivoPecho.forward);

            // Nota: Dependiendo de cómo esté tu prefab de mano, "Palma abajo" podría ser angulo 0 o 180.
            // Asumimos que quieres que sean paralelos (0 grados de diferencia entre Up mano y Up pecho).
            if (angulo > toleranciaAngulo)
            {
                ActualizarUI("ALINEA LAS PALMAS", Color.red, 0);
                return;
            }
        }

        // C. Validar Manos Juntas
        if (requerirDosManos)
        {
            if (!manoIzquierda.IsTracked || !manoDerecha.IsTracked)
            {
                ActualizarUI("USA AMBAS MANOS", Color.yellow, 0);
                return;
            }

            float distManos = Vector3.Distance(manoIzquierda.PointerPose.position, manoDerecha.PointerPose.position);

            if (distManos > distanciaManosApiladas)
            {
                ActualizarUI("JUNTA LAS MANOS", Color.yellow, 0);
                return;
            }
        }

        // ---------------------------------------------------------
        // 4. LÓGICA DE COMPRESIÓN
        // ---------------------------------------------------------
        Plane planoPecho = new Plane(objetivoPecho.forward, objetivoPecho.position);
        float distanciaAlPlano = planoPecho.GetDistanceToPoint(posMano);

        float compresionActual = 0;

        if (invertirDireccionCompresion)
            compresionActual = distanciaAlPlano;
        else
            compresionActual = -distanciaAlPlano;

        if (compresionActual < 0) compresionActual = 0;

        // Feedback de Profundidad
        if (compresionActual <= 0.015f)
        {
            ActualizarUI("🖐️ LISTO - EMPUJA", Color.cyan, 0);
        }
        else
        {
            float cm = compresionActual * 100f;
            if (compresionActual >= 0.05f && compresionActual <= 0.08f)
                ActualizarUI($"¡EXCELENTE! {cm:F1}cm", Color.green, compresionActual);
            else if (compresionActual > 0.08f)
                ActualizarUI($"¡DEMASIADO! {cm:F1}cm", Color.red, compresionActual);
            else
                ActualizarUI($"EMPUJA {cm:F1}cm", Color.yellow, compresionActual);
        }
    }

    // --- FUNCIONES AUXILIARES ---

    OVRHand ObtenerManoActiva(Vector3 objetivo)
    {
        if (!manoDerecha.IsTracked && !manoIzquierda.IsTracked) return null;
        float distD = manoDerecha.IsTracked ? Vector3.Distance(manoDerecha.PointerPose.position, objetivo) : 999;
        float distI = manoIzquierda.IsTracked ? Vector3.Distance(manoIzquierda.PointerPose.position, objetivo) : 999;
        return distD < distI ? manoDerecha : manoIzquierda;
    }

    bool SonManosAbiertas()
    {
        if (esqueletoIzq == null || esqueletoDer == null) return true; // Seguridad por si olvidas asignar

        float apIzq = ObtenerApertura(esqueletoIzq);
        float apDer = ObtenerApertura(esqueletoDer);

        // Ambas manos deben estar abiertas
        return (apIzq > umbralApertura && apDer > umbralApertura);
    }

    float ObtenerApertura(OVRSkeleton esqueleto)
    {
        if (esqueleto.Bones.Count == 0) return 0; // Si no hay huesos, retorna cerrado

        // Mide distancia entre Muñeca (Bone 0) y Punta Índice (Bone 20 aprox)
        // Usamos Hand_Index3 que es la punta del dedo índice
        return Vector3.Distance(
            esqueleto.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position,
            esqueleto.Bones[(int)OVRSkeleton.BoneId.Hand_Index3].Transform.position
        );
    }

    void ActualizarUI(string texto, Color color, float valor)
    {
        if (textoFeedback) { textoFeedback.text = texto; textoFeedback.color = color; }
        if (barraCompresion) { barraCompresion.value = valor; }
        if (rellenoBarra) { rellenoBarra.color = color; }
    }
}