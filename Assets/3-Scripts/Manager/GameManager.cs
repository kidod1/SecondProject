using UnityEngine.SceneManagement;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    private SkeletonAnimation[] skeletonAnimations;

    private float restartHoldTime = 3f;
    private float restartTimer = 0f;
    private bool isRestarting = false;

    public Player player;
    public PlayerUIManager uiManager;
    public PlayerAbilityManager abilityManager;
    public AbilityManager abilityUIManager;
    public SceneChangeSkeleton chanageSkeleton;

    // ���� ���� ������
    [Header("Game Stats")]
    public float playTime;
    public int monstersKilled;
    public int floorsCleared;
    public int bossesKilled;
    public int moneyEarned;

    private float gameStartTime;

    // ��� ȭ���� ���� ������
    [Header("Game Result UI")]
    public GameObject gameResultPanel;
    public Image rankImage;
    public TMP_Text playTimeText;
    public TMP_Text monstersKilledText;
    public TMP_Text floorsClearedText;
    public TMP_Text bossesKilledText;
    public TMP_Text moneyEarnedText;
    public TMP_Text totalScoreText;

    [Header("Rank Settings")]
    public Sprite[] rankSprites; // ��ũ �̹��� �迭
    public int scoreForMonsterKill = 10;
    public int scoreForFloorClear = 100;
    public int scoreForBossKill = 500;
    public int scoreForMoney = 1; // 1���� ����

    private int totalScore;

    private void Start()
    {
        skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
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

        // ���� ���� �ð� ����
        gameStartTime = Time.time;

        // ���� �ʱ�ȭ
        ResetStats();

        // �ɷ� �ʱ�ȭ
        abilityManager.ResetAllAbilities(); // Start �� �ʱ�ȭ

        // �÷��̾� ������ �ʱ�ȭ �� UI ������Ʈ
        player.LoadPlayerData(); // �÷��̾� ������ �ε�
        abilityManager.Initialize(player);
        abilityUIManager.Initialize(abilityManager); // AbilityManager �ʱ�ȭ

        // UI �ʱ�ȭ
        uiManager.Initialize(player);

        // �̺�Ʈ ���ε�
        player.OnTakeDamage.AddListener(() => uiManager.UpdateHealthUI());
        player.OnLevelUp.AddListener(() => uiManager.UpdateExperienceUI());

        // �÷��̾� ��� �� GameOver ȣ��
        player.OnPlayerDeath.AddListener(GameOver);

        // ���� ��� �г� ��Ȱ��ȭ
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameResultPanel�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    private void Update()
    {
        // �÷��� Ÿ�� ������Ʈ
        playTime = Time.time - gameStartTime;

        if (Input.GetKey(KeyCode.R))
        {
            restartTimer += Time.deltaTime;
            if (restartTimer >= restartHoldTime && !isRestarting)
            {
                isRestarting = true;
                RestartScene();
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
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

        // --- �߰��� �κ� ���� ---
        if (Input.GetKeyDown(KeyCode.L))
        {
            // �׽�Ʈ������ LŰ�� ������ ���� �г��� Ȱ��ȭ (���� ���� ���� ȣ������ ����)
            ShowGameResultPanelTest();
        }
        // --- �߰��� �κ� �� ---
    }

    private void RestartScene()
    {
        // ���� ����۵� �� �÷��̾� �����͸� �����ϰ�, ���� ������մϴ�.
        player.SavePlayerData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            PauseAnimations();
        }
        else
        {
            Time.timeScale = 1;
            ResumeAnimations();
        }
    }

    private void PauseAnimations()
    {
        foreach (var skeletonAnimation in skeletonAnimations)
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.timeScale = 0;
            }
        }
    }

    private void ResumeAnimations()
    {
        foreach (var skeletonAnimation in skeletonAnimations)
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.timeScale = 1;
            }
        }
    }

    // ���� ���� ó��
    public void GameOver()
    {
        // �÷��̾� ������ ����
        player.SavePlayerData();

        // ���� ��� �� ��ũ ����
        CalculateTotalScore();
        DetermineRank();

        // ��� UI ������Ʈ
        UpdateResultUI();

        // ��� �г� Ȱ��ȭ
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
        }
    }

    private void CalculateTotalScore()
    {
        totalScore = 0;
        totalScore += monstersKilled * scoreForMonsterKill;
        totalScore += floorsCleared * scoreForFloorClear;
        totalScore += bossesKilled * scoreForBossKill;
        totalScore += moneyEarned * scoreForMoney;
        // �÷��� Ÿ�ӿ� ���� �߰� ������ ������ ������ ���⿡ �߰�
    }

    private void DetermineRank()
    {
        // ������ ���� ��ũ�� �����մϴ�.
        // rankSprites �迭�� �ε����� ��Ī�ǵ��� �մϴ�.

        if (totalScore >= 2000)
        {
            rankImage.sprite = rankSprites[0]; // S ��ũ
        }
        else if (totalScore >= 1500)
        {
            rankImage.sprite = rankSprites[1]; // A ��ũ
        }
        else if (totalScore >= 1000)
        {
            rankImage.sprite = rankSprites[2]; // B ��ũ
        }
        else if (totalScore >= 500)
        {
            rankImage.sprite = rankSprites[3]; // C ��ũ
        }
        else
        {
            rankImage.sprite = rankSprites[4]; // D ��ũ
        }
    }

    private void UpdateResultUI()
    {
        // �÷��� Ÿ���� ��:��:�� �������� ��ȯ
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(playTime);
        playTimeText.text = $"�÷��� Ÿ��: {timeSpan:hh\\:mm\\:ss}";

        monstersKilledText.text = $"óġ�� ���� ��: {monstersKilled}";
        floorsClearedText.text = $"Ŭ������ �� ��: {floorsCleared}";
        bossesKilledText.text = $"óġ�� ���� ��: {bossesKilled}";
        moneyEarnedText.text = $"ȹ���� ��: {moneyEarned}��";
        totalScoreText.text = $"�� ����: {totalScore}";
    }

    // ���� ���� �޼����
    public void AddMonsterKill()
    {
        monstersKilled++;
    }

    public void AddFloorClear()
    {
        floorsCleared++;
    }

    public void AddBossKill()
    {
        bossesKilled++;
    }

    public void AddMoney(int amount)
    {
        moneyEarned += amount;
    }

    // ���� �ʱ�ȭ
    public void ResetStats()
    {
        playTime = 0f;
        monstersKilled = 0;
        floorsCleared = 0;
        bossesKilled = 0;
        moneyEarned = 0;
    }

    public void ShowGameResultPanelTest()
    {
        // ���� ��� �� ��ũ ����
        CalculateTotalScore();
        DetermineRank();

        // ��� UI ������Ʈ
        UpdateResultUI();

        // ��� �г� Ȱ��ȭ
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
        }

        // �׽�Ʈ��: �ؽ�Ʈ�� ����� ������Ʈ�Ǵ��� Ȯ��
        Debug.Log($"�÷��� Ÿ��: {playTimeText.text}");
        Debug.Log($"óġ�� ���� ��: {monstersKilledText.text}");
        Debug.Log($"Ŭ������ �� ��: {floorsClearedText.text}");
        Debug.Log($"óġ�� ���� ��: {bossesKilledText.text}");
        Debug.Log($"ȹ���� ��: {moneyEarnedText.text}");
        Debug.Log($"�� ����: {totalScoreText.text}");
    }

    public void SceneChanageCloseAnimationAddLoad(string NextSceneName)
    {
        chanageSkeleton.PlayCloseAnimation(NextSceneName);
    }
}
