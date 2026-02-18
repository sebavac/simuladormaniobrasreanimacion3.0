using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para Corutinas

public class PCWebcamSource : MonoBehaviour, IFrameSource
{
    [Header("Monitor de Calibración")]
    public RawImage monitorUI;

    [Header("Selección de Cámara")]
    [Tooltip("0 suele ser la integrada, 1 la USB externa")]
    public int indiceCamara = 0;

    private WebCamTexture _webCamTexture;
    private bool _isSafeToPlay = false;

    // Propiedades
    public int Width => _webCamTexture != null ? _webCamTexture.width : 640;
    public int Height => _webCamTexture != null ? _webCamTexture.height : 480;
    public bool IsReady => _isSafeToPlay && _webCamTexture != null && _webCamTexture.isPlaying;

    void Start()
    {
        // En lugar de iniciar de golpe, usamos una Corutina para no congelar Unity
        StartCoroutine(IniciarCamaraSegura());
    }

    IEnumerator IniciarCamaraSegura()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("ERROR CRÍTICO: Unity no detecta ninguna webcam.");
            yield break;
        }

        int index = Mathf.Clamp(indiceCamara, 0, devices.Length - 1);
        string nombreCamara = devices[index].name;
        Debug.Log($"Intentando conectar con: {nombreCamara}...");

        // Configuración inicial
        _webCamTexture = new WebCamTexture(nombreCamara, 640, 480, 30);

        // Esperamos un frame para dejar respirar al sistema
        yield return null;

        // INICIO CRÍTICO
        _webCamTexture.Play();

        // Esperamos hasta que la cámara realmente empiece a enviar datos
        float timeout = 0f;
        while (!_webCamTexture.isPlaying && timeout < 5f)
        {
            timeout += Time.deltaTime;
            yield return null; // Esperamos al siguiente frame
        }

        if (!_webCamTexture.isPlaying)
        {
            Debug.LogError("TIMEOUT: La cámara no respondió en 5 segundos. Reinicia el PC o cambia de USB.");
        }
        else
        {
            Debug.Log("Webcam iniciada correctamente.");
            _isSafeToPlay = true;

            if (monitorUI != null)
            {
                monitorUI.texture = _webCamTexture;
                // Arreglo visual para que no se vea rotada o invertida
                monitorUI.rectTransform.localEulerAngles = new Vector3(0, 0, -_webCamTexture.videoRotationAngle);
            }
        }
    }

    public Texture GetTexture()
    {
        return _webCamTexture;
    }

    void OnDestroy()
    {
        if (_webCamTexture != null) _webCamTexture.Stop();
    }
}