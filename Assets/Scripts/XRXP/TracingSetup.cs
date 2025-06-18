using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XPXR.Recorder.Models;

public class TracingSetup : MonoBehaviour
{
    [SerializeField] FeedbackManager feedbackManager;
    private DataToTrace TracingData;
    private PlayerNetworkInfo LocalPlayerInfo, RemotePlayerInfo;
    bool isRecording;

    public bool DEBUG = false;

    private float Timer = 0f;

    public float LoggingPeriod = 0.5f;

    private int currentState = 0;
    // Start is called before the first frame update
    void Start()
    {
        isRecording = false;
    }

    private void OnApplicationQuit()
    {
        if (isRecording)
        {
            EndTracing();
        }
    }

    public void InitiateTracing()
    {

        if (isRecording)
        {
            return;
        }
        SetupDataSources();
        Debug.Log("[TRACING] Initiating tracing");
        StartCoroutine(BeginTracingRoutine());
    }

    public void EndTracing()
    {
        if (!isRecording)
        {
            return;
        }
        Debug.Log("[TRACING] End tracing");
        StartCoroutine(EndTracingRoutine());
    }

    private int SetupDataSources()
    {
        int value = 0;
        if (LocalPlayerInfo == null)
        {
            LocalPlayerInfo = GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>();
            value ++;
        }
        if (RemotePlayerInfo == null)
        {
            RemotePlayerInfo = GameManager.RemotePlayerObject?.GetComponent<PlayerNetworkInfo>();
            value ++;
        }
        return value;
    }

    private void LateUpdate()
    {
        Timer += Time.deltaTime;
        if (Timer >= LoggingPeriod)
        {
            Timer = 0f;
            if (isRecording)
            {
                UpdateTracingData();
                LogTracingData();
            }
        }
    }  

    private void UpdateTracingData()
    {
        TracingData.SetStateData(feedbackManager.StateDefined.Value);

        TracingData.SetIndividualData(
            LocalPlayerInfo.Speed.Value,
            LocalPlayerInfo.CycleDuration.Value,
            RemotePlayerInfo.Speed.Value,
            RemotePlayerInfo.CycleDuration.Value
            );

        TracingData.SetPositionData(
            LocalPlayerInfo.transform,
            RemotePlayerInfo.transform,
            LocalPlayerInfo.Direction.Value,
            RemotePlayerInfo.Direction.Value
            );
    }

    private void LogTracingData()
    {
        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.QuantitativeValue, "LocalPlayerData", "LocalSpeed", 
            new QuantitativeValue(TracingData.ExperimenterSpeed));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.WorldPosition, "LocalPlayerData", "LocalPosition",
            new WorldPosition(TracingData.ExperimenterTransform.position, TracingData.ExperimenterTransform.rotation));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.QuantitativeValue, "RemotePlayerData", "RemoteSpeed",
            new QuantitativeValue(TracingData.ParticipantSpeed));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.WorldPosition, "RemotePlayerData", "RemotePosition", 
            new WorldPosition(TracingData.ParticipantTransform.position, TracingData.ParticipantTransform.rotation));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.QuantitativeValue, "GroupData", "SeparationDistance",
            new QuantitativeValue(TracingData.SeparationDistance));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.QuantitativeValue, "GroupData", "SeparationAngle",
            new QuantitativeValue(TracingData.SeparationAngle));

        XPXRManager.Recorder.AddInternalEvent(XPXR.Recorder.Models.SystemType.QuantitativeValue, "PHASE STATE", "CurrentState",
            new QuantitativeValue(feedbackManager.StateDefined.Value));
    }

    private IEnumerator BeginTracingRoutine()
    {
        isRecording = true;
        XPXRManager.Recorder.StartSession();
        Debug.Log("[TRACINGSETUP] Start tracing");
        //XPXRManager.Recorder.StartSession(environmentProperties: new Dictionary<string, string>(){
        //    {"ExperimentState", TracingData.ExperimenterState.ToString()},
        //    {"SeparationDistance", TracingData.SeparationDistance.ToString()},
        //    {"SeparationAngle", TracingData.SeparationAngle.ToString()},
        //    {"ExperimenterSpeed", TracingData.ExperimenterSpeed.ToString()},
        //    {"ParticipantSpeed", TracingData.ParticipantSpeed.ToString()},
        //    //{"ExperimenterFreq", TracingData.ExperimenterFreq.ToString()},
        //    //{"ParticipantFreq", TracingData.ParticipantFreq.ToString()},
        //    {"ExperimenterPosition", TracingData.ExperimenterTransform.ToString()},
        //    {"ParticipantPosition", TracingData.ParticipantTransform.ToString()}
        //});
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator EndTracingRoutine()
    {
        isRecording = false;
        yield return new WaitForSeconds(1f);
        XPXRManager.Recorder.StopSession();

        while (XPXRManager.Recorder.TransfersState() != 0)
        {
            yield return new WaitForSeconds(1);
        }
        XPXRManager.Recorder.EndTracing();
        Debug.Log("[TRACING] End of the record");
    }
}
