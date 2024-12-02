#if !UNITY_WEBGL // Syncbox
using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class Syncbox : MonoBehaviour 
{
    // is really an inheritance structure, but using injection instead until
    // time for a refactor is available 
    private UPennSyncbox upennSync;
    private FreiburgSyncbox freiburgSync;
    public ScriptedEventReporter scriptedInput = null;

    private bool isInit = false;

    public void Init()
    {
        upennSync = new UPennSyncbox(scriptedInput);
        //freiburgSync = new FreiburgSyncbox(scriptedInput);
        Debug.Log("Begin Init");

        try {
            if(!upennSync.Init()) {
                Debug.Log("Invalid UPenn Handle");
                upennSync = null;
            }
            else { isInit = true; }
        }
        catch {
            Debug.Log("Failed opening Upenn Sync");
        }

        try {
            if(!freiburgSync.Init()) {
                Debug.Log("Invalid Freiburg Handle");
                freiburgSync = null;
            }
            else { isInit = true; }
        }
        catch {
            Debug.Log("Failed opening Freiburg sync");
        }
    }

    public void StartPulse() {
        if (!isInit)
        {
            Debug.Log("Initializing");
            Init();
            isInit = true;
            Debug.Log("Initialized");
        }
        Debug.Log("Starting Pulses");
        upennSync?.StartPulse();
        freiburgSync?.StartPulse();
    }

    public void StopPulse() {
        upennSync?.StopPulse();
        freiburgSync?.StopPulse();
    }

    public void TestPulse() {
        Debug.Log("Testing");
        if (!isInit)
        {
            Debug.Log("Initializing");
            Init();
            isInit = true;
            Debug.Log("Initialized");
        }
        Debug.Log("Sending test pulse");
        upennSync?.TestPulse();
        Debug.Log("Done sending test pulse");
        //freiburgSync?.TestPulse();
    }

    public void OnDisable() {
        upennSync?.OnDisable();
        freiburgSync?.OnDisable();
    }
}
#endif
