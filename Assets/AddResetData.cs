using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class AddResetData : MonoBehaviour
{
    public void ResetDataButton()
    {
        PlayerDataManager.Instance.ResetPlayerData();
    }
}
