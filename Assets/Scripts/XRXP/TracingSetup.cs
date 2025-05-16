using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TracingSetup : MonoBehaviour
{
    [SerializeField] FeedbackManager feedbackManager;
    private DataToTrace TracingData;
    private PlayerNetworkInfo ExperimenterInfo, ParticipantInfo;
    bool isRecording = false;

    // Start is called before the first frame update
    void Start()
    {
        isRecording = false;
    }

    public void InitiateTracing()
    {
        if (isRecording)
        {
            return;
        }
        Debug.Log("[Tracing] Initiating tracing");
        StartCoroutine(BeginTracingRoutine());
    }

    public void EndTracing()
    {
        if (!isRecording)
        {
            return;
        }
        Debug.Log("[Tracing] End tracing");
        StartCoroutine(EndTracingRoutine());
    }

    private int SetupDataSources()
    {
        if (ExperimenterInfo == null || ParticipantInfo == null)
        {
            ExperimenterInfo = GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>();
            ParticipantInfo = GameManager.RemotePlayerObject?.GetComponent<PlayerNetworkInfo>();
            return -1;
        }
        return 1;
    }

    private void Update()
    {
        if (isRecording)
        {
            if (SetupDataSources()<0)
            {
                Debug.LogError("[TRACINGSETUP] Data sources not set up yet");
                return;
            }
            UpdateTracingData();
        }
    }   

    private void UpdateTracingData()
    {
        //TracingData.SetStateData(feedbackManager.AuraState.Value, feedbackManager.AuraState.Value);

        TracingData.SetIndividualData(
            ExperimenterInfo.Speed.Value,
            ExperimenterInfo.CycleDuration.Value,
            ParticipantInfo.Speed.Value,
            ParticipantInfo.CycleDuration.Value
            );

        TracingData.SetPositionData(
            ExperimenterInfo.transform.position,
            ParticipantInfo.transform.position,
            ExperimenterInfo.Direction.Value,
            ParticipantInfo.Direction.Value
            );
    }

    private IEnumerator BeginTracingRoutine()
    {
        isRecording = true;
        XPXRManager.Recorder.StartSession();
        Debug.Log("[TRACINGSETUP] Start tracing");
        XPXRManager.Recorder.StartSession(environmentProperties: new Dictionary<string, string>(){
            {"AuraState", TracingData.AuraState.ToString()},
            {"FootState", TracingData.Footstate.ToString()},
            {"SeparationDistance", TracingData.SeparationDistance.ToString()},
            {"SeparationAngle", TracingData.SeparationAngle.ToString()},
            {"ExperimenterSpeed", TracingData.ExperimenterSpeed.ToString()},
            {"ParticipantSpeed", TracingData.ParticipantSpeed.ToString()},
            {"ExperimenterFreq", TracingData.ExperimenterFreq.ToString()},
            {"ParticipantFreq", TracingData.ParticipantFreq.ToString()},
            {"ExperimenterPosition", TracingData.ExperimenterPosition.ToString()},
            {"ParticipantPosition", TracingData.ParticipantPosition.ToString()}
        });
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator EndTracingRoutine()
    {
        XPXRManager.Recorder.StopSession();
        yield return new WaitForSeconds(1f);
        XPXRManager.Recorder.StopSession();
        Debug.Log("[TRACINGSETUP] End of the trial");
        while (XPXRManager.Recorder.TransfersState() != 0)
        {
            yield return new WaitForSeconds(1);
        }
        XPXRManager.Recorder.EndTracing();
        isRecording = false;
        Debug.Log("End of the record");
    }
}
