using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalDataLogger : MonoBehaviour
{
    private TracingSetup Tracer;
    private DataToTrace tracingdata;

    // Start is called before the first frame update
    void Start()
    {
        if (Tracer == null)
        {
            Tracer = FindObjectOfType<TracingSetup>();
            return;
        }
    }


    void PassDataToTrace()
    {

    }
}
