using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetworkInfo : NetworkBehaviour
{
    public float Speed;
    public float CycleDuration;
    public Vector3 Direction;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Speed = 0f;
            CycleDuration = 1f;
            Direction = Vector3.zero;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetLocalPlayerDataRpc(
        Vector3 direction,
        float speed,
        float cycleDuration)
    {
        Direction = direction;
        Speed = speed;
        CycleDuration = cycleDuration;
    }
}
