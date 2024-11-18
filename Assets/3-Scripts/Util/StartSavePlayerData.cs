using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSavePlayerData : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        PlayerDataManager dataManager = PlayerDataManager.Instance;
        if (dataManager != null)
        {
            dataManager.LoadPlayerData();
            Debug.Log("플레이어 데이터가 성공적으로 로드되었습니다.");
        }
        else
        {
            Debug.LogError("PlayerDataManager 인스턴스가 존재하지 않습니다. 플레이어 데이터를 로드할 수 없습니다.");
        }
    }

}
