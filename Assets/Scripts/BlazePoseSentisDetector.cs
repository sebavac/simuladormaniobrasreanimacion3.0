using UnityEngine;
using Unity.Sentis;

public class BlazePoseSentisDetector : MonoBehaviour, IPoseDetector
{
    [Header("Sentis")]
    public ModelAsset modelAsset;
    public BackendType backend = BackendType.GPUCompute;

    [Header("Frame Source (VideoFrameSource / QuestPCAFrameSource)")]
    public MonoBehaviour frameSourceBehaviour; // Debe implementar IFrameSource

    [Header("Model Input Size (ajusta según tu ONNX)")]
    public int inputWidth = 256;
    public int inputHeight = 256;

    [Header("Debug")]
    public bool logModelIO = true;
    public bool logOutputLength = false;

    private Model model;
    private Worker worker;

    void Start()
    {
        logOutputLength = true;
        if (modelAsset == null)
        {
            Debug.LogError("BlazePoseSentisDetector: modelAsset NO asignado");
            enabled = false;
            return;
        }

        model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, backend);

        if (logModelIO)
        {
            Debug.Log("== BlazePose Model Inputs ==");
            foreach (var i in model.inputs)
                Debug.Log($"Input: {i.name} shape={i.shape}");

            Debug.Log("== BlazePose Model Outputs ==");
            for (int k = 0; k < model.outputs.Count; k++)
                Debug.Log($"Output[{k}] = {model.outputs[k]}");
        }
    }

    public bool TryGetLandmarks(out Vector3[] landmarks, out float[] confidence)
    {
        landmarks = null;
        confidence = null;

        // 1) Obtener textura desde tu FrameSource
        var frameSource = frameSourceBehaviour as IFrameSource;
        if (frameSource == null || !frameSource.IsReady) return false;

        Texture tex = frameSource.GetTexture();
        if (tex == null) return false;

        // 2) Convertir textura a tensor (en tu caso sale NCHW: 1,3,H,W)
        // Sentis marca este overload obsolete, pero es el que TU API expone sin layout.
        #pragma warning disable CS0618
        using var inputNCHW = TextureConverter.ToTensor(tex, inputWidth, inputHeight, 3);
        #pragma warning restore CS0618

        var inputFloat = inputNCHW as Tensor<float>;
        if (inputFloat == null) return false;

        // 3) Descargar a CPU y reordenar NCHW -> NHWC
        float[] nchw = inputFloat.DownloadToArray();

        // Crear tensor NHWC que tu modelo espera: (1,H,W,3)
        using var inputNHWC = new Tensor<float>(new TensorShape(1, inputHeight, inputWidth, 3));

        int H = inputHeight;
        int W = inputWidth;

        int hw = H * W;
        int rBase = 0 * hw;
        int gBase = 1 * hw;
        int bBase = 2 * hw;

        // NHWC aplanado: [H][W][C]
        int idx = 0;
        for (int h = 0; h < H; h++)
        {
            int row = h * W;
            for (int w = 0; w < W; w++)
            {
                int p = row + w;

                float r = nchw[rBase + p];
                float g = nchw[gBase + p];
                float b = nchw[bBase + p];

                inputNHWC[idx++] = r;
                inputNHWC[idx++] = g;
                inputNHWC[idx++] = b;
            }
        }

        // 4) Ejecutar modelo
        worker.Schedule(inputNHWC);

        // 5) Leer output (por ahora el primero)
        var outTensor = worker.PeekOutput(0) as Tensor<float>;
        if (outTensor == null) return false;

        float[] data = outTensor.DownloadToArray();

        if (logOutputLength)
            Debug.Log("BlazePose output length = " + data.Length);

        // 6) Parse genérico: asumimos 33*3=99 floats (x,y,z)
        // (Si tu modelo devuelve otra forma, ajustamos después)
        if (data.Length < 195) return false;

        landmarks = new Vector3[33];
        confidence = new float[33];

        int stride = 5; // x, y, z, visibility, presence

        for (int i = 0; i < 33; i++)
        {
            int baseIdx = i * stride;

            float x = data[baseIdx + 0] / inputWidth;
            float y = 1f - (data[baseIdx + 1] / inputHeight);
            float z = data[baseIdx + 2];


            float visibility = data[baseIdx + 3]; // opcional
            float presence   = data[baseIdx + 4]; // opcional

            landmarks[i] = new Vector3(x, y, z);
            confidence[i] = Mathf.Clamp01(visibility);
        }

        return true;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}
