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
    private SkinnedMeshRenderer m_meshRenderer;

    private Transform m_target;

    private Transform InitialLocaiton;

    private NetworkVariable<bool> netIsActive = new NetworkVariable<bool>();
    private bool Resetting = false;

    public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        netIsActive.Value = false;
    }

    void Start()
    {
        InitialLocaiton = transform;
        m_collider = GetComponent<Collider>();
        m_meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        m_auraManager = GetComponentInChildren<AuraManager>();
        m_auraManager.AuraState = false;
    }

    void Update()
    {
        if (IsServer)
        {
            if (Resetting)
            {
                ReturnToCenter();
            }
            if (!netIsActive.Value)
            {
                return;
            }
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
        Resetting = true;
    }

    [ClientRpc]
    void UpdateClientVisualsClientRpc(Color newColor)
    {
        m_meshRenderer.materials[1].color = newColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (m_auraManager.SetOtherPlayer(GameManager.LocalPlayerObject) < 0)
            {
                Debug.LogError("[SetupDummyAura] Other Player is null");
                return;
            }
            m_auraManager.AuraState = true;
            ActivateServerRpc();
        }
    }

    void ReturnToCenter()
    {
        m_target = InitialLocaiton;
        if (Vector3.Distance(transform.position, m_target.position) < 0.01f)
        {
            transform.position = InitialLocaiton.position;
            transform.rotation = InitialLocaiton.rotation;
            Resetting = false;
            return;
        }

        Vector3 direction = (m_target.position - transform.position).normalized;
        //transform.rotation = Quaternion.LookRotation(direction) ;
        //transform.forward = ;
        transform.forward = Vector3.RotateTowards(transform.forward, -1 * Vector3.Cross(direction, Vector3.up), 2 * Speed * Time.deltaTime, 0.0f);
        float step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, m_target.position, step);
    }
    void ServerMoveBetweenEndPoints()
    {
        // Initial Target
        if (m_target == null)
        {
            m_target = EndPointOne;
        }

        Vector3 direction = (m_target.position - transform.position).normalized;
        //transform.rotation = Quaternion.LookRotation(direction) ;
        //transform.forward = ;
        transform.forward = Vector3.RotateTowards(transform.forward, -1 * Vector3.Cross(direction, Vector3.up), 2 * Speed * Time.deltaTime, 0.0f);
        float step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, m_target.position, step);
        
        if (Vector3.Distance(transform.position, m_target.position) < 0.01f)
        {
            m_target = m_target == EndPointOne ? EndPointTwo : EndPointOne;
        }
    }
}
