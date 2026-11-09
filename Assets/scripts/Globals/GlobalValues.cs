using UnityCommunity.UnitySingleton;
using UnityEngine;

public class GlobalValues : MonoSingleton<GlobalValues>
{
    [SerializeField]
    public float SailStateChangeRate = 0.1f;
}
