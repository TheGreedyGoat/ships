using UnityEngine;

class PlaneWave : MonoBehaviour
{

    void Update()
    {
        var pos = transform.position;

        transform.position = new Vector3(pos.x, EnvironmentManager.instance.GetWaterLevel(pos), pos.y);
    }
}