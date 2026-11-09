using System;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
public class WaveFunctionManager : MonoBehaviour
{
    private static WaveFunctionManager _instance;
    public static WaveFunctionManager instance
    {
        get
        {
            return _instance;
        }
    }
    [SerializeField]
    WaveFunction[] functions;


    [SerializeField]
    [Range(0, 1)]
    float spacing = 0.5f;
    [SerializeField]
    [Range(0, 5)]
    float amplitude;

    [SerializeField]
    bool debugDraw = false;
    void Awake()
    {
        _instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float[] weights = new float[functions.Length];
        float totalWeights = 0f;
        for (int i = 0; i < functions.Length; i++)
        {
            float weight = MathF.Pow(weights.Length - i, 2);
            weights[i] = weight;
            totalWeights += weight;
        }
        float wInverse = 1 / totalWeights;
        for (int i = 0; i < functions.Length; i++)
        {
            functions[i].weight = weights[i] * wInverse;
            functions[i].Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
    public Vector3 GetWaveOffset(Vector3 point)
    {


        Vector3 offset = Vector3.zero;

        foreach (WaveFunction f in functions)
        {
            offset += f.GetOffset(point, Time.time);
        }
        offset.Normalize();
        offset *= amplitude;
        if (debugDraw)
        {
            Debug.DrawLine(point, point + offset, Color.blue);
        }
        return offset;
    }

    public float GetWaterLevel(Vector3 position)
    {
        Vector3 a = GetWaveOffset(position + amplitude * Vector3.forward);
        Vector3 b = GetWaveOffset(position + amplitude * Vector3.right);
        Vector3 c = GetWaveOffset(position - amplitude * Vector3.forward);
        Vector3 d = GetWaveOffset(position - amplitude * Vector3.right);
        return (a.y + b.y + c.y + d.y) * 0.25f;
    }
}
