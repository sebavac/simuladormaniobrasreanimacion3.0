using UnityEngine;
using UnityEngine.UI;

public class Mark : MonoBehaviour
{
    [Header("Inputs")]
    public PosePipeline pipeline;
    public RawImage videoView;

    [Header("Point Prefab")]
    public RectTransform pointPrefab;
    public int landmarkCount = 33;
    public float pointSize = 10f;

    [Header("Line Prefab")]
    public RectTransform linePrefab;
    public float lineThickness = 4f;

    [Header("Options")]
    public bool mirrorX = false;
    public bool showGridWhenNoLandmarks = true;

    private RectTransform[] points;
    private RectTransform[] lines;
    private RectTransform overlayRect;

    // Conexiones (a,b) estilo BlazePose (suficientes para “esqueleto”)
    private (int a, int b)[] connections;

    void Awake()
    {
        // Validaciones
        if (pointPrefab == null)
        {
            Debug.LogError("Mark: PointPrefab no asignado");
            enabled = false;
            return;
        }
        if (linePrefab == null)
        {
            Debug.LogError("Mark: LinePrefab no asignado");
            enabled = false;
            return;
        }

        overlayRect = GetComponent<RectTransform>();
        if (overlayRect == null)
        {
            Debug.LogError("Mark: Este script debe estar en un UI con RectTransform (ej: PoseOverlay)");
            enabled = false;
            return;
        }

        // Crear puntos
        points = new RectTransform[landmarkCount];
        for (int i = 0; i < landmarkCount; i++)
        {
            var p = Instantiate(pointPrefab, overlayRect);
            p.name = $"LM_{i}";
            p.sizeDelta = new Vector2(pointSize, pointSize);
            points[i] = p;
        }

        // Conexiones
        BuildConnections();

        // Crear líneas
        lines = new RectTransform[connections.Length];
        for (int i = 0; i < connections.Length; i++)
        {
            var l = Instantiate(linePrefab, overlayRect);
            l.name = $"LN_{i}_{connections[i].a}_{connections[i].b}";
            l.sizeDelta = new Vector2(50f, lineThickness);
            lines[i] = l;
        }
    }

    void Update()
    {
        if (points == null || points.Length < landmarkCount) return;

        bool hasLandmarks = pipeline != null &&
                            pipeline.lastLandmarks != null &&
                            pipeline.lastLandmarks.Length >= landmarkCount;

        // 1) Si NO hay landmarks: grilla (debug)
        if (!hasLandmarks)
        {
            if (showGridWhenNoLandmarks)
            {
                ShowGrid();
                // Igual dibujamos líneas basadas en la grilla
                DrawSkeletonFromPoints();
            }
            else
            {
                // Oculta si quieres
                SetAllVisible(false);
            }
            return;
        }

        // 2) Si hay landmarks: posicionar sobre video
        if (videoView == null) return;

        SetAllVisible(true);

        RectTransform videoRect = videoView.rectTransform;
        Rect rct = videoRect.rect;

        for (int i = 0; i < landmarkCount; i++)
        {
            Vector3 lm = pipeline.lastLandmarks[i];

            float x = lm.x;
            float y = lm.y;

            if (mirrorX) x = 1f - x;

            // Coordenadas locales del RawImage (centro)
            float localX = (x - 0.5f) * rct.width;
            float localY = (y - 0.5f) * rct.height;

            // Pasar a overlay local
            Vector3 worldPos = videoRect.TransformPoint(new Vector3(localX, localY, 0));
            Vector3 overlayLocal = overlayRect.InverseTransformPoint(worldPos);

            points[i].anchoredPosition = new Vector2(overlayLocal.x, overlayLocal.y);
        }

        // Dibujar líneas
        DrawSkeletonFromPoints();
    }

    // ---------------- Helpers ----------------

    void ShowGrid()
    {
        int cols = 11;          // 11 x 3 = 33
        float spacing = 25f;

        for (int i = 0; i < landmarkCount; i++)
        {
            int c = i % cols;
            int r = i / cols;
            points[i].anchoredPosition = new Vector2(50 + c * spacing, -50 - r * spacing);
        }
    }

    void SetAllVisible(bool visible)
    {
        // Puntos
        for (int i = 0; i < points.Length; i++)
            if (points[i] != null) points[i].gameObject.SetActive(visible);

        // Líneas
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
                if (lines[i] != null) lines[i].gameObject.SetActive(visible);
        }
    }

    void BuildConnections()
    {
        // Conexiones “main” BlazePose / MediaPipe Pose (33)
        connections = new (int, int)[]
        {
            // Cabeza / cara
            (0,1),(1,2),(2,3),(3,7),
            (0,4),(4,5),(5,6),(6,8),
            (9,10),

            // Torso
            (11,12),
            (11,23),(12,24),
            (23,24),

            // Brazo izq
            (11,13),(13,15),
            (15,17),(15,19),(15,21),
            (17,19),(19,21),

            // Brazo der
            (12,14),(14,16),
            (16,18),(16,20),(16,22),
            (18,20),(20,22),

            // Pierna izq
            (23,25),(25,27),
            (27,29),(29,31),
            (27,31),

            // Pierna der
            (24,26),(26,28),
            (28,30),(30,32),
            (28,32),
        };
    }

    void DrawSkeletonFromPoints()
    {
        if (lines == null || connections == null) return;

        for (int i = 0; i < connections.Length; i++)
        {
            int a = connections[i].a;
            int b = connections[i].b;

            if (a < 0 || a >= points.Length || b < 0 || b >= points.Length) continue;
            if (points[a] == null || points[b] == null) continue;

            Vector2 p1 = points[a].anchoredPosition;
            Vector2 p2 = points[b].anchoredPosition;

            DrawLine(lines[i], p1, p2);
        }
    }

    void DrawLine(RectTransform line, Vector2 p1, Vector2 p2)
    {
        if (line == null) return;

        Vector2 dir = (p2 - p1);
        float length = dir.magnitude;

        // Si dos puntos están iguales, escondemos esa línea
        if (length < 0.001f)
        {
            line.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!line.gameObject.activeSelf) line.gameObject.SetActive(true);
        }

        line.anchoredPosition = (p1 + p2) * 0.5f;
        line.sizeDelta = new Vector2(length, lineThickness);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
