using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChangesTransmitter : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] Toggle Aura;
    [SerializeField] Toggle BlinkingLight;
    [SerializeField] Toggle GuidingArrow;
    [SerializeField] Toggle PPGrayEffect;
    [SerializeField] Toggle Recording;

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

    [SerializeField] FeedbackManager feedbackManager;

    private void Start()
    {
        feedbackManager = FindAnyObjectByType<FeedbackManager>();
    }
    public void OnToggleAura()
    {
        if (!Aura.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(1);
    }
    public void OnToggleBlinking()
    {
        if (!BlinkingLight.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(2);
    }
    public void OnToggleArrow()
    {
        if (!GuidingArrow.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(3);
    }
    public void OnToggleGrayEffect()
    {
        if (!PPGrayEffect.isOn)
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
        feedbackManager.UpdateExperimentStateServerRpc(4);
    }
    public void OnToggleTracing()
    {
        //feedbackManager.UpdateExperimentStateServerRpc(1);
    }

    private void resetAll()
    {
        Aura.isOn = false;
        BlinkingLight.isOn = false;
        GuidingArrow.isOn = false;
        PPGrayEffect.isOn = false;
    }

    private void Update()
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
        LocalFreq.text = m_localinfo.CycleDuration.Value.ToString();
    }

    private void UpdateRemotePlayerUI()
    {
        if (m_remoteinfo == null)
        {
            m_remoteinfo = GameManager.RemotePlayerObject?.GetComponent<PlayerNetworkInfo>();
            return;
        }
        RemoteSpeed.text = Mathf.Round(m_remoteinfo.Speed.Value).ToString();
        RemoteFreq.text = m_remoteinfo.CycleDuration.Value.ToString();
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
