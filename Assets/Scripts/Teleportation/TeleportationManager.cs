using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class TeleportationManager : NetworkBehaviour
{
    public NetworkVariable<bool> m_playerOneReady = new NetworkVariable<bool>();
    public NetworkVariable<bool> m_playerTwoReady = new NetworkVariable<bool>();
    [SerializeField] private GameObject _StartingArea;
    [SerializeField] TracingSetup tracingSetup;


    private bool ExperimenterReadyViz = false;
    private bool ParticipantReadyViz = false;

    [SerializeField] MeshRenderer[] ExperimenterAreaRenderers;
    [SerializeField] MeshRenderer[] ParticipantAreaRenderers;
    [SerializeField] Material DefaultMat, ExperimenterReadyMat, ParticipantReadyMat;

    private void Awake()
    {
        _StartingArea.SetActive(true);
    }

    //If the Value of m_playerOneReady or m_playerTwoReady changes, this function will be called
    private void OnEnable()
    {
        m_playerOneReady.OnValueChanged += BothPlayersReadyCheck;
        m_playerTwoReady.OnValueChanged += BothPlayersReadyCheck;
    }

    private void OnDisable()
    {
        m_playerOneReady.OnValueChanged -= BothPlayersReadyCheck;
        m_playerTwoReady.OnValueChanged -= BothPlayersReadyCheck;
    }

    public void PlayerReadyCheck(bool state, int playerNumber)
    {
        if (playerNumber == 1)
        {
            SetPlayerOneReadyServerRpc(state);
        }
        else if (playerNumber == 2)
        {
            SetPlayerTwoReadyServerRpc(state);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerOneReadyServerRpc(bool state)
    {
        m_playerOneReady.Value = state; // Server modifies the value
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTwoReadyServerRpc(bool state)
    {
        m_playerTwoReady.Value = state; // Server modifies the value
    }

    private void BothPlayersReadyCheck(bool previousValue, bool newValue)
    {
        UpdateExperimenterArea(m_playerOneReady.Value);
        UpdateParticipantArea(m_playerTwoReady.Value);
        if (m_playerOneReady.Value && m_playerTwoReady.Value)
        {
            // USE A TIMER TO TELEPORT PLAYERS
            Debug.Log("Both players are ready");
            StartCoroutine(TeleportPlayers());
        }
    }

    IEnumerator TeleportPlayers()
    {
        yield return new WaitForSeconds(2.0f);
        tracingSetup.InitiateTracing();
        _StartingArea.SetActive(false);
    }

    void UpdateParticipantArea(bool state)
    {
        if (ParticipantReadyViz != state)
        {
            var TargetMat = DefaultMat;
            if (state)
            {
                TargetMat = ParticipantReadyMat;
            }
            for (int i = 0; i < ParticipantAreaRenderers.Length; i++)
            {
                ParticipantAreaRenderers[i].material = TargetMat;
            }
            ParticipantReadyViz = state;
        }
    }
    void UpdateExperimenterArea(bool state)
    {
        if (ExperimenterReadyViz != state)
        {
            var TargetMat = DefaultMat;
            if (state)
            {
                TargetMat = ExperimenterReadyMat;
            }
            for (int i = 0; i < ExperimenterAreaRenderers.Length; i++)
            {
                ExperimenterAreaRenderers[i].material = TargetMat;
            }
            ExperimenterReadyViz = state;
        }
    }
}
