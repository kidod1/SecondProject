using AK.Wwise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTPCManager : MonoBehaviour
{
    public RTPC pauseRTPC;
    public void PauseRTPCON()
    {
        pauseRTPC.SetGlobalValue(1);
    }
    public void PauseRTPCOFF()
    {
        pauseRTPC.SetGlobalValue(2);
    }
}
