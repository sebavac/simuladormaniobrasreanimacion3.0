using UnityEngine;

public class PosePipeline : MonoBehaviour
{
    [Header("Sources")]
    public MonoBehaviour frameSourceBehaviour;
    public MonoBehaviour poseDetectorBehaviour;  // <--- AQUÍ VA EL BLAZEPOSE REAL

    private IFrameSource frameSource;
    private IPoseDetector poseDetector;

    [Header("Debug Output")]
    public Vector3[] lastLandmarks;
    public float[] lastConfidence;

    void Awake()
    {
        frameSource = frameSourceBehaviour as IFrameSource;
        poseDetector = poseDetectorBehaviour as IPoseDetector;

        if (frameSource == null)
            Debug.LogError("PosePipeline: Falta FrameSource (IA_LiveFeed)");

        if (poseDetector == null)
            Debug.LogError("PosePipeline: El Detector no implementa IPoseDetector o es nulo");
    }

    void Update()
    {
        if (frameSource == null || !frameSource.IsReady) return;
        if (poseDetector == null) return;

        // Intentamos obtener los datos reales de la IA
        if (poseDetector.TryGetLandmarks(out var lm, out var conf))
        {
            lastLandmarks = lm;
            lastConfidence = conf;
        }
    }
}