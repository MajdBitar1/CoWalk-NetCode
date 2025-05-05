using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SetupDummyAura : NetworkBehaviour
{
    [SerializeField] float Speed = 3f;
    [SerializeField] Transform EndPointOne, EndPointTwo;

    private Collider m_collider;
    private AuraManager m_auraManager;
    private MeshRenderer m_meshRenderer;

    private Transform m_target;

    private NetworkVariable<bool> netIsActive = new NetworkVariable<bool>();

    public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        netIsActive.Value = false;
    }

    void Start()
    {
        m_collider = GetComponent<Collider>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_auraManager = GetComponentInChildren<AuraManager>();
        m_auraManager.AuraState = false;
    }

    void Update()
    {
        if (IsServer)
        {
            ServerMoveBetweenEndPoints();
        }
    }

    void FixedUpdate()
    {
        if (netIsActive.Value)
        {
            if(m_auraManager.AuraBroken)
            {
                DeactivateServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateServerRpc()
    {
        netIsActive.Value = true;
        UpdateClientVisualsClientRpc(new Color(0f, 1f, 1f, 1f));
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateServerRpc()
    {
        netIsActive.Value = false;
        UpdateClientVisualsClientRpc(new Color(1f, 0f, 0f, 1f));
    }


    [ClientRpc]
    void UpdateClientVisualsClientRpc(Color newColor)
    {
        m_meshRenderer.material.color = newColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            m_auraManager.SetOtherPlayer(GameManager.LocalPlayerObject);
            m_auraManager.AuraState = true;
            ActivateServerRpc();
        }
    }

    void ServerMoveBetweenEndPoints()
    {
        if (!netIsActive.Value)  return;

        Debug.Log("Moving between endpoints");

        if (m_target == null)
        {
            m_target = EndPointOne;
        }

        Vector3 direction = (m_target.position - transform.position).normalized;
        float step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, m_target.position, step);
        
        if (Vector3.Distance(transform.position, m_target.position) < 0.01f)
        {
            m_target = m_target == EndPointOne ? EndPointTwo : EndPointOne;
        }
    }
}
