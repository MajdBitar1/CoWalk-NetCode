using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementHandler))]
[RequireComponent(typeof(ComputeArmRhythm))]
public class PlayerDataTransmitter : MonoBehaviour
{
    PlayerMovementData m_playerMovementData;
    PlayerMovementHandler m_playerMovementHandler;
    ComputeArmRhythm m_armRhythm;

    [SerializeField] PlayerNetworkInfo m_playerNetworkInfo;

    [Tooltip("The player's current movement data")]
    public float CurrentSpeed = 0;
    public float Frequency = 0;
    public Vector3 Direction = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        m_playerMovementHandler = GetComponent<PlayerMovementHandler>();
        m_armRhythm = GetComponent<ComputeArmRhythm>();
        m_playerMovementData = new PlayerMovementData(transform.position, Vector3.zero, 0, 0);
    }

    public PlayerMovementData GetPlayerData()
    {
        return m_playerMovementData;
    }
}
