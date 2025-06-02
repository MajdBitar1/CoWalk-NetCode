using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIChangesTransmitter : MonoBehaviour
{
    [SerializeField] FeedbackManager feedbackManager;

    [Header("Toggles")]
    [SerializeField] Toggle Aura;
    [SerializeField] Toggle GuidingArrow;
    [SerializeField] Toggle Tracing;

    [Header("Local Stats")]
    [SerializeField] TextMeshProUGUI LocalSpeed;
    [SerializeField] TextMeshProUGUI LocalRealSpeed;

    [Header("Remote Stats")]
    [SerializeField] TextMeshProUGUI RemoteSpeed;
    [SerializeField] TextMeshProUGUI RemoteRealSpeed;

    [Header("Group Stats")]
    [SerializeField] TextMeshProUGUI GroupSeparationDistance;
    [SerializeField] TextMeshProUGUI GroupAngleDifference;

    [Header("Cooldown Duration")]
    [SerializeField] float CooldownDuration = 5f;

    private PlayerNetworkInfo m_localinfo, m_remoteinfo;
    private Vector3 LocalPrevPosition, RemotePrevPosition;
    private bool TwoToggleSafeFlag = false;


    private void Start()
    {
        feedbackManager = FindAnyObjectByType<FeedbackManager>();
        LocalPrevPosition = Vector3.zero;
        RemotePrevPosition = Vector3.zero;

        Aura.onValueChanged.AddListener(OnToggleAura);
        GuidingArrow.onValueChanged.AddListener(OnToggleArrow);
        Tracing.onValueChanged.AddListener(OnToggleTracing);
    }
    public void OnToggleAura(bool isOn)
    {
        if (TwoToggleSafeFlag) return;
        if (isOn)
        {
            //Raise The Safety Flag and Turn off The Arrow
            TwoToggleSafeFlag = true;
            GuidingArrow.isOn = false;
            TwoToggleSafeFlag = false;
            feedbackManager.UpdateExperimentStateServerRpc(1);
        }
        else
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }

    }
    public void OnToggleArrow(bool isOn)
    {
        if (TwoToggleSafeFlag) return;
        if (isOn)
        {
            //Raise The Safety Flag and Turn off The Aura
            TwoToggleSafeFlag = true;
            Aura.isOn = false;
            TwoToggleSafeFlag = false;
            feedbackManager.UpdateExperimentStateServerRpc(2);
        }
        else
        {
            feedbackManager.UpdateExperimentStateServerRpc(0);
        }
    }

    public void OnToggleTracing(bool isOn)
    {
        feedbackManager.UpdateTracingStateServerRpc(isOn);
        StartCoroutine(ReenableToggleAfterDelay(CooldownDuration));
    }

    IEnumerator ReenableToggleAfterDelay(float delay)
    {
        Tracing.interactable = false;
        yield return new WaitForSeconds(delay);
        Tracing.interactable = true;
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
        LocalRealSpeed.text = Velocity.ToString();
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
        RemoteRealSpeed.text = RemoteVelo.ToString();
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
