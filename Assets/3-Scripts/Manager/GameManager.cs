using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float restartHoldTime = 3f;
    private float restartTimer = 0f;
    private bool isRestarting = false;

    public Player player;
    public PlayerUIManager uiManager;
    public PlayerAbilityManager abilityManager;
    public AbilityManager abilityUIManager;

    private void Start()
    {
        // Null üũ �߰�
        if (player == null)
        {
            Debug.LogError("Player is not assigned in GameManager.");
            return;
        }
        if (uiManager == null)
        {
            Debug.LogError("PlayerUIManager is not assigned in GameManager.");
            return;
        }
        if (abilityManager == null)
        {
            Debug.LogError("PlayerAbilityManager is not assigned in GameManager.");
            return;
        }
        if (abilityUIManager == null)
        {
            Debug.LogError("AbilityManager is not assigned in GameManager.");
            return;
        }


        // �ɷ� �ʱ�ȭ
        abilityManager.ResetAllAbilities(); // Start �� �ʱ�ȭ

        // �÷��̾� ������ �ʱ�ȭ �� UI ������Ʈ
        player.LoadPlayerData(); // �÷��̾� ������ �ε�
        abilityManager.Initialize(player); // �ɷ� �Ŵ��� �ʱ�ȭ
        abilityUIManager.Initialize(abilityManager); // AbilityManager �ʱ�ȭ

        // UI �ʱ�ȭ
        uiManager.Initialize(player);

        // �̺�Ʈ ���ε�
        player.OnTakeDamage.AddListener(() => uiManager.UpdateHealthUI());
        player.OnLevelUp.AddListener(() => uiManager.UpdateExperienceUI());
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            restartTimer += Time.deltaTime;
            if (restartTimer >= restartHoldTime && !isRestarting)
            {
                isRestarting = true;
                RestartScene();
            }
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            restartTimer = 0f;
            isRestarting = false;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            abilityUIManager.ShowAbilitySelection(); // T Ű�� ������ �ɷ� ���� â�� ����
        }
    }

    private void RestartScene()
    {
        // ���� ����۵� �� �÷��̾� �����͸� �����ϰ�, ���� ������մϴ�.
        player.SavePlayerData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
