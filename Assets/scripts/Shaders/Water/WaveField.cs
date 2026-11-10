using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class WaveField : MonoSingleton<WaveField>
{
    [SerializeField]
    public Wave[] waves;

    [SerializeField]
    Material m;

    [Header("Gizzies")]
    [SerializeField]
    bool showGizmos = true;

    float[] waveSpeeds = new float[100];
    float[] amplitudes = new float[100];
    float[] waveLengths = new float[100];
    Vector4[] directions = new Vector4[100];

    float GetScaledTime(int index)
    {
        return Time.fixedTime * waves[index].Speed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void FixedUpdate()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            waveSpeeds[i] = waves[i].Speed;
            amplitudes[i] = waves[i].Amplitude;
            waveLengths[i] = waves[i].WaveLength;
            directions[i] = waves[i].Direction;
        }
        m.SetInt("_WaveCount", waves.Length);
        m.SetFloat("_CTime", Time.fixedTime);
        m.SetFloatArray("_Speeds", waveSpeeds);
        m.SetFloatArray("_Amplitudes", amplitudes);
        m.SetFloatArray("_WaveLengths", waveLengths);
        m.SetVectorArray("_Directions", directions);
    }

    Vector3 GetOffset(Vector3 worldPosition)
    {
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < waves.Length; i++)
        {
            Vector2 dir = waves[i].Direction;
            Vector2 xzPos = new(worldPosition.x, worldPosition.z);
            float phase =
                Vector2.Dot(xzPos, dir) / waves[i].WaveLength
                + GetScaledTime(i);
            float cosPhase = Mathf.Cos(phase);
            offset += new Vector3(
                cosPhase * dir.x,
                Mathf.Sin(phase) * waves[i].Amplitude,
                cosPhase * dir.y
            );
        }

        return offset;
    }

    public float GetHeight(Vector3 position)
    {
        Vector3 p = new(position.x, 0, position.z);

        for (int i = 0; i < 3; i++)
        {
            Vector3 off = GetOffset(p);
            p = new(position.x - off.x, 0, position.z - off.z);
        }

        return GetOffset(p).y;
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            for (int x = -5; x <= 5; x++)
            {
                for (int z = -5; z <= 5; z++)
                {
                    Vector3 start = new(x, 0, z);
                    Vector3 end = start + GetOffset(new(x, 0, z)); //new(x, GetHeight(start), z);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(end, .1f);
                    Gizmos.color = Color.blueViolet;
                    Gizmos.DrawSphere(start, .1f);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}
