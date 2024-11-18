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
            Debug.Log("�÷��̾� �����Ͱ� ���������� �ε�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("PlayerDataManager �ν��Ͻ��� �������� �ʽ��ϴ�. �÷��̾� �����͸� �ε��� �� �����ϴ�.");
        }
    }

}
