using System;
using UnityEngine;

[Serializable]
public class Wave
{
    [SerializeField, Range(0, 2)]
    public float Speed;

    [SerializeField, Range(0, 10)]
    public float Amplitude;

    [SerializeField, Range(0.1f, 100f)]
    public float WaveLength;

    [SerializeField, Range(0, 2 * Mathf.PI)]
    public float Angle;

    public Vector2 Direction => new(Mathf.Cos(Angle), Mathf.Sin(Angle));
}
