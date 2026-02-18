using UnityEngine;

public class MockPoseDetector : MonoBehaviour, IPoseDetector
{
    public bool TryGetLandmarks(out Vector3[] landmarks, out float[] confidence)
    {
        landmarks = new Vector3[33];
        confidence = new float[33];

        // Pose "humana" básica (frontal), con un pequeño movimiento en brazos
        float t = Time.time;

        // Centro del cuerpo
        Vector2 hipCenter = new Vector2(0.5f, 0.35f);
        Vector2 shoulderCenter = new Vector2(0.5f, 0.65f);

        // Hombros
        Vector2 lShoulder = shoulderCenter + new Vector2(-0.12f, 0.00f); // 11
        Vector2 rShoulder = shoulderCenter + new Vector2( 0.12f, 0.00f); // 12

        // Cadera
        Vector2 lHip = hipCenter + new Vector2(-0.10f, 0.00f); // 23
        Vector2 rHip = hipCenter + new Vector2( 0.10f, 0.00f); // 24

        // Brazos (mueven un poco)
        float armWave = Mathf.Sin(t) * 0.08f;

        Vector2 lElbow = lShoulder + new Vector2(-0.10f, -0.10f + armWave); // 13
        Vector2 lWrist = lElbow   + new Vector2(-0.08f, -0.12f);           // 15

        Vector2 rElbow = rShoulder + new Vector2( 0.10f, -0.10f - armWave); // 14
        Vector2 rWrist = rElbow    + new Vector2( 0.08f, -0.12f);           // 16

        // Piernas
        Vector2 lKnee = lHip + new Vector2(-0.03f, -0.18f); // 25
        Vector2 lAnk  = lKnee + new Vector2(-0.02f, -0.18f); // 27

        Vector2 rKnee = rHip + new Vector2( 0.03f, -0.18f); // 26
        Vector2 rAnk  = rKnee + new Vector2( 0.02f, -0.18f); // 28

        // Cabeza (simple)
        Vector2 nose = new Vector2(0.5f, 0.80f); // 0

        // Relleno inicial: todo al centro
        for (int i = 0; i < 33; i++)
        {
            landmarks[i] = new Vector3(0.5f, 0.5f, 0);
            confidence[i] = 1f;
        }

        // Asignar puntos clave (índices MediaPipe Pose)
        landmarks[0]  = new Vector3(nose.x, nose.y, 0);

        landmarks[11] = new Vector3(lShoulder.x, lShoulder.y, 0);
        landmarks[12] = new Vector3(rShoulder.x, rShoulder.y, 0);

        landmarks[13] = new Vector3(lElbow.x, lElbow.y, 0);
        landmarks[14] = new Vector3(rElbow.x, rElbow.y, 0);

        landmarks[15] = new Vector3(lWrist.x, lWrist.y, 0);
        landmarks[16] = new Vector3(rWrist.x, rWrist.y, 0);

        landmarks[23] = new Vector3(lHip.x, lHip.y, 0);
        landmarks[24] = new Vector3(rHip.x, rHip.y, 0);

        landmarks[25] = new Vector3(lKnee.x, lKnee.y, 0);
        landmarks[26] = new Vector3(rKnee.x, rKnee.y, 0);

        landmarks[27] = new Vector3(lAnk.x, lAnk.y, 0);
        landmarks[28] = new Vector3(rAnk.x, rAnk.y, 0);

        // Manos (dedos básicos para que se vean conexiones)
        landmarks[17] = landmarks[15];
        landmarks[19] = landmarks[15];
        landmarks[21] = landmarks[15];

        landmarks[18] = landmarks[16];
        landmarks[20] = landmarks[16];
        landmarks[22] = landmarks[16];

        // Clamps
        for (int i = 0; i < 33; i++)
        {
            landmarks[i].x = Mathf.Clamp01(landmarks[i].x);
            landmarks[i].y = Mathf.Clamp01(landmarks[i].y);
        }

        return true;
    }
}
