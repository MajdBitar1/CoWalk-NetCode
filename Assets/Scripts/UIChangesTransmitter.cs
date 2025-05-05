using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChangesTransmitter : MonoBehaviour
{
    [SerializeField] PlayerNetworkInfo m_localinfo, m_remoteinfo;

    [Header("Toggle for Aura")]
    [SerializeField] Toggle Aura, Footsteps, Recording;
    [Header("Text for Local and Remote Speed and Frequency")]
    [SerializeField] TextMeshProUGUI LocalSpeed, LocalFreq, RemoteSpeed, RemoteFreq;

    [Header("Debug: Make private later")]
    public bool aurastate, footstepsstate, recordingstate;

    private void Update()
    {
        if (m_localinfo != null && m_remoteinfo != null)
        {
            UpdatePlayersInfoToUI();
        }
        else
        {
            m_localinfo = GameManager.Experimenter?.GetComponent<PlayerNetworkInfo>();
            m_remoteinfo = GameManager.Participant?.GetComponent<PlayerNetworkInfo>();
        }
    }
    public void OnToggleClick()
    {
        aurastate = Aura.isOn;
        footstepsstate = Footsteps.isOn;
        recordingstate = Recording.isOn;
        TransmitToggleChange();
    }

    private void TransmitToggleChange()
    {
        GameManager.Instance.UpdateConditionsServerRpc(aurastate, footstepsstate, recordingstate);
    }

    private void UpdatePlayersInfoToUI()
    {
        LocalSpeed.text = m_localinfo.Speed.ToString();
        LocalFreq.text = m_localinfo.CycleDuration.ToString();
        RemoteSpeed.text = m_remoteinfo.Speed.ToString();
        RemoteFreq.text = m_remoteinfo.CycleDuration.ToString();
    }
}
