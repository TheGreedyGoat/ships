using UnityEngine;
using System;
using UnityEngine.Animations;
using System.IO.Compression;

[Serializable]
public class WaveFunction
{
    [SerializeField]
    [Range(0, 5)]
    float frequency = 1;

    [SerializeField]
    [Range(0, 10)]
    float timeOffsetScale;

    // [SerializeField]
    // [Range(-1, 1)]
    [HideInInspector]
    public float weight;

    [SerializeField]
    [Range(0, 2 * Mathf.PI)]
    float waveAngle = 0;
    Vector2 waveDirection;
    public void Init()
    {
    }
    public Vector3 GetOffset(Vector3 point, float time)
    {
        waveDirection = new Vector2(Mathf.Cos(waveAngle), Mathf.Sin(waveAngle));

        float x = point.x * waveDirection.x + point.z * waveDirection.y;
        float offset = frequency * x + time * timeOffsetScale;
        // The offset if the wave would follow the x axis
        Vector2 xOffset = weight * new Vector2(Mathf.Cos(offset), Mathf.Sin(offset));
        return new Vector3(xOffset.x * waveDirection.x, xOffset.y, xOffset.x * waveDirection.y);
    }
}
