using System;
using System.Collections.Generic;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class WaveValues : MonoSingleton<WaveValues>
{
    [SerializeField] Material m;
    [SerializeField] float value;

    [SerializeField][Range(0, 1)] float[] amplitudes;
    [SerializeField][Range(0, 1)] float[] frequencies;

    [SerializeField][Range(0, 5)] float[] waveSpeeds;
    [SerializeField] List<Vector4> directions;

    [Header("Gizzies")]
    [SerializeField] bool showGizmos = true;

    float GetScaledTime(int index)
    {
        return Time.fixedTime / waveSpeeds[index];
    }

    float totalAmplitude
    {
        get
        {
            float total = 0;
            foreach (float a in amplitudes)
            {
                total += a;
            }
            return total;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void FixedUpdate()
    {
        m.SetFloat("_CTime", Time.fixedTime);
        m.SetFloatArray("_TimeScales", waveSpeeds);
        m.SetFloatArray("amplitudes", amplitudes);
        m.SetFloatArray("frequencies", frequencies);
        m.SetVectorArray("directions", directions);



    }

    Vector3 GetOffset(Vector3 worldPosition)
    {
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < 2; i++)
        {

            Vector2 dir = new(directions[i].x, directions[i].y);
            dir.Normalize();
            Vector2 xzPos = new(worldPosition.x, worldPosition.z);
            float phase = Vector2.Dot(xzPos, dir)
                * frequencies[i]
                + GetScaledTime(i)
                ;
            float cosPhase = Mathf.Cos(phase);
            offset += new Vector3(cosPhase * dir.x, Mathf.Sin(phase) * amplitudes[i], cosPhase * dir.y);
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
            for (int x = -15; x <= 15; x++)
            {
                for (int z = -5; z <= 5; z++)
                {
                    Vector3 start = new(x, 0, z);
                    Vector3 end = new(x, GetHeight(start), z);

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
