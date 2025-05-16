using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChangesTransmitter : MonoBehaviour
{
    [SerializeField] FeedbackManager feedbackManager;

    [Header("Toggles")]
    [SerializeField] Toggle Aura;
    [SerializeField] Toggle GuidingArrow;
    [SerializeField] Toggle Tracing;
    //[SerializeField] Toggle BlinkingLight;

    [Header("Local Stats")]
    [SerializeField] TextMeshProUGUI LocalSpeed;
    [SerializeField] TextMeshProUGUI LocalFreq;

    [Header("Remote Stats")]
    [SerializeField] TextMeshProUGUI RemoteSpeed;
    [SerializeField] TextMeshProUGUI RemoteFreq;

    [Header("Group Stats")]
    [SerializeField] TextMeshProUGUI GroupSeparationDistance;
    [SerializeField] TextMeshProUGUI GroupAngleDifference;

    private PlayerNetworkInfo m_localinfo, m_remoteinfo;

    private Vector3 LocalPrevPosition, RemotePrevPosition;


    private void Start()
    {
        feedbackManager = FindAnyObjectByType<FeedbackManager>();
        LocalPrevPosition = Vector3.zero;
        RemotePrevPosition = Vector3.zero;
    }
    public void OnToggleAura()
    {
        if (!Aura.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(1);
    }
    public void OnToggleArrow()
    {
        if (!GuidingArrow.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(2);
    }
    //public void OnToggleBlinking()
    //{
    //    if (!BlinkingLight.isOn)
    //    {
    //        feedbackManager.UpdateExperimentStateServerRpc(0);
    //    }
    //    feedbackManager.UpdateExperimentStateServerRpc(3);
    //    resetAll();
    //    BlinkingLight.isOn = true;
    //}

    public void OnToggleTracing()
    {
        feedbackManager.UpdateTracingStateServerRpc(Tracing.isOn);
    }

    private void resetAll()
    {
        try
        {
            Aura.isOn = false;
            //BlinkingLight.isOn = false;
            GuidingArrow.isOn = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UIChangesTransmitter] Error resetting toggles: " + e.Message);
        }
    }

    private void LateUpdate()
    {
        UpdateLocalPlayerUI();
        UpdateRemotePlayerUI();
        UpdateGroupStats();
    }

    private void UpdateLocalPlayerUI()
    {
        if (m_localinfo == null)
        {
            m_localinfo = GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>();
            return;
        }
        LocalSpeed.text = Mathf.Round(m_localinfo.Speed.Value).ToString();
        UpdateRealLocalSpeed();
    }

    private void UpdateRealLocalSpeed()
    {
        float DistanceCovered = Vector3.Distance(m_localinfo.transform.position, LocalPrevPosition);
        float Velocity = Mathf.Round(DistanceCovered * 100 / Time.fixedDeltaTime) / 100;
        LocalFreq.text = Velocity.ToString();
        LocalPrevPosition = m_localinfo.transform.position;
    }

    private void UpdateRemotePlayerUI()
    {
        if (m_remoteinfo == null)
        {
            m_remoteinfo = GameManager.RemotePlayerObject?.GetComponent<PlayerNetworkInfo>();
            return;
        }
        RemoteSpeed.text = Mathf.Round(m_remoteinfo.Speed.Value).ToString();
        UpdateRealRemoteSpeed();
    }
    private void UpdateRealRemoteSpeed()
    {
        float DistanceCovered = Vector3.Distance(m_remoteinfo.transform.position, RemotePrevPosition);
        float RemoteVelo = Mathf.Round(DistanceCovered * 100 / Time.fixedDeltaTime) / 100;
        RemoteFreq.text = RemoteVelo.ToString();
        RemotePrevPosition = m_remoteinfo.transform.position;
    }

    private void UpdateGroupStats()
    {
        if (m_localinfo == null || m_remoteinfo == null)
        {
            return;
        }
        float SepDistance = Mathf.Round(Vector3.Distance(m_localinfo.transform.position,m_remoteinfo.transform.position));
        float AngleDiff = Mathf.Round(Vector3.Angle(m_localinfo.Direction.Value, m_remoteinfo.Direction.Value));
        GroupSeparationDistance.text = SepDistance.ToString();
        GroupAngleDifference.text = AngleDiff.ToString();
    }
}
