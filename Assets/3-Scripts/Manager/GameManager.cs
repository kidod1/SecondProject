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
        // 플레이어 데이터 초기화 및 UI 업데이트
        player.LoadPlayerData(); // 플레이어 데이터 로드
        abilityManager.Initialize(player); // 능력 매니저 초기화
        abilityUIManager.Initialize(abilityManager); // AbilityManager 초기화

        // UI 초기화
        uiManager.Initialize(player);

        // 이벤트 바인딩
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
            abilityUIManager.ShowAbilitySelection(); // T 키를 누르면 능력 선택 창을 띄운다
        }
    }

    private void RestartScene()
    {
        // 씬이 재시작될 때 플레이어 데이터를 저장하고, 씬을 재시작합니다.
        player.SavePlayerData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
