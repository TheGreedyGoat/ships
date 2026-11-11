using UnityCommunity.UnitySingleton;
using UnityEngine;

public class PlayerCannons : MonoSingleton<PlayerCannons>
{
    Player player => Player.Instance;

    Ship ship => player.Ship;

    ShipCannons cannons => player.Cannons;

    CameraMovement cam => player.Cam;

    void Start() { }
}
