using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    // [Header("Swimmer Setup")]
    // [SerializeField]
    // float swimmerOffsetX = 0.5f;

    // [SerializeField]
    // float swimmerOffsetY = 0;

    // [SerializeField]
    // float swimmerOffsetZ = 1;

    [Header("Settings")]
    [SerializeField]
    public BuoyancySettings settings;

    public float buoyancy = .3f;

    [SerializeField]
    public float damping = 1f;

    // [SerializeField]

    Swimmer[] Swimmers
    {
        get => GetComponentsInChildren<Swimmer>();
    }
    Transform swimmerParent;

    Rigidbody body;

    //
    //  ######  ########    ###    ########  ########
    // ##    ##    ##      ## ##   ##     ##    ##
    // ##          ##     ##   ##  ##     ##    ##
    //  ######     ##    ##     ## ########     ##
    //       ##    ##    ######### ##   ##      ##
    // ##    ##    ##    ##     ## ##    ##     ##
    //  ######     ##    ##     ## ##     ##    ##
    //
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Build(
        List<Vector3> localPositions,
        BuoyancySettings settings,
        List<float> weights = null
    )
    {
        this.settings = settings;
        if (weights != null && weights.Count != localPositions.Count)
        {
            throw new System.Exception(
                $"Weights for Buyoancy swimmers need to be null or same length as localPositions.\nlocalPositions Count: {localPositions.Count}\nweights Count : {weights.Count}"
            );
        }
        var swimmerChildren = GetComponentsInChildren<Swimmer>();
        foreach (var s in swimmerChildren)
        {
            if (Application.isPlaying)
                Destroy(s);
            else
                DestroyImmediate(s);
        }
        body = body != null ? body : GetComponent<Rigidbody>();

        for (int i = 0; i < localPositions.Count; i++)
        {
            var swimmer = new GameObject(
                $"Swimmer {i}"
            ).AddComponent<Swimmer>();
            swimmer.transform.parent = transform;
            swimmer.body = body;
            swimmer.transform.localPosition = localPositions[i];
            swimmer.weight =
                weights == null ? 1f / localPositions.Count : weights[i];
            swimmer.buoyancy = this;
        }
    }

    void FixedUpdate()
    {
        SimBuoyancy();
    }

    void SimBuoyancy()
    {
        foreach (var swimmer in Swimmers)
        {
            float depth = swimmer.GetDepth();
            Vector3 force = depth * settings.Force * -Physics.gravity;
            Vector3 pointV = body.GetPointVelocity(swimmer.transform.position);
            force -= Vector3.up * (pointV.y * settings.Damping);

            Vector3 forcePoint =
                body.worldCenterOfMass
                + (swimmer.transform.position - body.worldCenterOfMass)
                    * settings.BuoyancyTorqueFactor;

            body.AddForceAtPosition(
                force * Time.fixedDeltaTime * swimmer.weight,
                forcePoint
            );
        }
    }
}
