using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChangesTransmitter : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] Toggle Aura;
    [SerializeField] Toggle Footsteps;
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
    private bool aurastate, footstepsstate, recordingstate;
    public void OnToggleClick()
    {
        aurastate = Aura.isOn;
        footstepsstate = Footsteps.isOn;
        recordingstate = Recording.isOn;
        int result = GameManager.Instance.TryUpdateConditions(aurastate, footstepsstate, recordingstate);
        if (result < 0)
        {
            resetAll();
            Debug.LogError("[UIChangesTransmitter] Error updating conditions");
            return;
        }
    }

    private void resetAll()
    {
        aurastate = false;
        footstepsstate = false;
        recordingstate = false;
        Aura.isOn = false;
        Footsteps.isOn = false;
        Recording.isOn = false;
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
