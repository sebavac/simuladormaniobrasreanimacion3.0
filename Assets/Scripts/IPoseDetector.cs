using UnityEngine;

public interface IPoseDetector
{
    bool TryGetLandmarks(out Vector3[] landmarks, out float[] confidence);
}
