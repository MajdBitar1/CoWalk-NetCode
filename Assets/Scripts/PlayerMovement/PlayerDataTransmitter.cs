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

    // Update is called once per frame
    void Update()
    {
        //m_playerMovementData = m_playerMovementHandler.GetMovementData();
        ////m_playerMovementData.CycleDuration = m_armRhythm.GetCycleDuration();
        ////m_armRhythm.SetCurrentSpeed(m_playerMovementData.Speed);

        //CurrentSpeed = m_playerMovementData.Speed;
        //Frequency = m_playerMovementData.CycleDuration;
        //Direction = m_playerMovementData.Direction;
    }
    private void LateUpdate()
    {
        //if (m_playerNetworkInfo == null)
        //{
        //    m_playerNetworkInfo = GameManager.LocalPlayerObject.GetComponent<PlayerNetworkInfo>();
        //}
        //else
        //{
        //    m_playerNetworkInfo.SetLocalPlayerDataRpc(Direction, CurrentSpeed, Frequency);
        //}
    }

    private void FixedUpdate()
    {
        //if (GameManager.LocalPlayerObject == null)
        //{
        //    return;
        //}

        //GameManager.LocalPlayerObject.GetComponent<PlayerNetworkInfo>().SetLocalPlayerData(m_playerMovementData);
    }

    public PlayerMovementData GetPlayerData()
    {
        return m_playerMovementData;
    }
}
