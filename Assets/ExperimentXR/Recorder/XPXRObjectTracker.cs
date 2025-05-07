﻿using System;
using UnityEngine;
using XPXR.Recorder.Models;

public class XPXRObjectTracker : MonoBehaviour
{
    public bool TracingEnabled = true;

    [Header("Type of object tracked (Body, Eye, Object etc)")]
    public string Category;

    [Header("Name of the tracker (default: GameObject.Name)")]
    public string ObjectName;
    private string _objectName;

    [Header("For every x frames a record is made")]
    public int TraceFrequency;

    private int _recordCountDown = 0;

    // Set the name of the object tracked
    public void SetObjectName(string name)
    {
        if (name.Length > 0)
        {
            this.ObjectName = name;
            this._objectName = name;
        }
        else
        {
            throw new UnityException("Name length must be > 0");
        }
    }
    public virtual string GetObjectName()
    {
        return this.ObjectName;
    }

    void Reset()
    {
        this.ObjectName = this.gameObject.name;
    }

    void Start()
    {
        this._objectName = this.ObjectName.Length > 0 ? this.ObjectName : this.gameObject.name;
        this._recordCountDown = this.TraceFrequency;
    }

    void LateUpdate()
    {
        // CRITICAL Critical space
        if(this.TraceFrequency != 0 && this._recordCountDown != this.TraceFrequency)
        {
            this._recordCountDown += 1;
            return;
        }
        
        this.Record();
        this._recordCountDown = 1;
    }

    internal virtual void Record()
    {
        if (XPXRManager.IsReady && this.TracingEnabled && XPXRManager.Recorder.isRecording())
        {
            WorldPosition wp = new WorldPosition(this.transform.position, this.transform.rotation);
            XPXRManager.Recorder.AddInternalEvent(SystemType.WorldPosition, this.Category, this._objectName, wp);
        }
    }



}
