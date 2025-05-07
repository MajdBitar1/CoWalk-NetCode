using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetworkInfo : NetworkBehaviour
{
    public NetworkVariable<float> Speed = new NetworkVariable<float>();
    public NetworkVariable<float> CycleDuration = new NetworkVariable<float>();
    public NetworkVariable<Vector3> Direction = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Speed.Value = 0f;
            CycleDuration.Value = 1f;
            Direction.Value = Vector3.zero;
        }
    }

    public void UpdateValues(Vector3 direction, float speed, float cycleDuration)
    {
        if (IsOwner)
        {
            SetLocalPlayerDataServerRpc(direction, speed, cycleDuration);
        }
        else
        {
            Debug.LogError("[PlayerNetworkInfo] Attempting to update values from non-owner client.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLocalPlayerDataServerRpc(
        Vector3 direction,
        float speed,
        float cycleDuration)
    {
        Direction.Value = direction;
        Speed.Value = speed;
        CycleDuration.Value = cycleDuration;
    }


}
