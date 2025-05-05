using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerOneTP : MonoBehaviour
{
    [Range(1, 2)]
    public int m_playerNumber;
    [SerializeField] TeleportationManager m_tpManager;
    //[SerializeField] private Material BasicMat, ActiveMat;

    //public MeshRenderer[] matRenderers;

    private Collider m_collider;


    void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
        if (m_tpManager == null)
        {
            m_tpManager = FindObjectOfType<TeleportationManager>();
        }
        //PopulateMeshRenderers();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (m_playerNumber == 1)
            {
                //RAISE EVENT THAT P1 IS READY TO TELEPORT
                GameManager.Instance.DefineExperimenter(other.gameObject);
            }
            else
            {
                //RAISE EVENT THAT P2 IS READY TO TELEPORT
                GameManager.Instance.DefineParticipant(other.gameObject);
            }
            m_tpManager.PlayerReadyCheck(true, m_playerNumber);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            //RAISE EVENT THAT P1 IS NOT READY ANYMORE TO TELEPORT
            m_tpManager.PlayerReadyCheck(false, m_playerNumber);
        }
    }
}
