using UnityEngine.SceneManagement;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AK.Wwise;
using System.Runtime.CompilerServices;

public class GameManager : MonoBehaviour
{
    public RTPC pauseRTPC;

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

    // 추가된 부분
    public GameObject pauseCanvas;
    private bool isPausedByEscape = false; // ESC 키로 인한 일시정지 여부

    // 게임 스탯 변수들
    [Header("Game Stats")]
    public float playTime;
    public int monstersKilled;
    public int floorsCleared;
    public int bossesKilled;
    public int moneyEarned;

    private float gameStartTime;

    // 결과 화면을 위한 변수들
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
    public Sprite[] rankSprites; // 랭크 이미지 배열
    public int scoreForMonsterKill = 10;
    public int scoreForFloorClear = 100;
    public int scoreForBossKill = 500;
    public int scoreForMoney = 1; // 1원당 점수

    private int totalScore;

    public AK.Wwise.Event pauseSound;

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

        // 게임 시작 시간 설정
        gameStartTime = Time.time;

        // 능력 매니저 초기화
        abilityManager.Initialize(player);
        abilityUIManager.Initialize(abilityManager); // AbilityManager 초기화
        Debug.Log("초기화");

        // 플레이어 사망 시 GameOver 호출
        player.OnPlayerDeath.AddListener(GameOver);

        // 게임 결과 패널 비활성화
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameResultPanel이 할당되지 않았습니다.");
        }

        // 일시정지 캔버스 비활성화
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseCanvas가 할당되지 않았습니다.");
        }
    }

    private void Update()
    {
        // 플레이 타임 업데이트
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
        else if (Input.GetKeyUp(KeyCode.R))
        {
            restartTimer = 0f;
            isRestarting = false;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!PlayManager.I.isPause)
            {
                PauseGameByEscape();
            }
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            abilityUIManager.ShowAbilitySelection(); // T 키를 누르면 능력 선택 창을 띄운다
        }

        if (Time.timeScale == 0)
        {
            pauseRTPC.SetGlobalValue(1);
        }
        else if (Time.timeScale == 1)
        {
            pauseRTPC.SetGlobalValue(2);
        }
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PauseGameByEscape()
    {
        pauseSound.Post(gameObject);
        if (!isPausedByEscape)
        {
            isPausedByEscape = true;
            pauseCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            isPausedByEscape = false;
            pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void RestartScene()
    {
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

    // 게임 종료 처리
    public void GameOver()
    {
        // 총점 계산 및 랭크 결정
        CalculateTotalScore();
        DetermineRank();

        // 결과 UI 업데이트
        UpdateResultUI();

        // 결과 패널 활성화
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
        }
    }

    public void SetGame()
    {
        Time.timeScale = 1;
    }

    private void CalculateTotalScore()
    {
        totalScore = 0;
        totalScore += monstersKilled * scoreForMonsterKill;
        totalScore += floorsCleared * scoreForFloorClear;
        totalScore += bossesKilled * scoreForBossKill;
        totalScore += moneyEarned * scoreForMoney;
        // 플레이 타임에 따른 추가 점수나 감점이 있으면 여기에 추가
    }

    private void DetermineRank()
    {
        // 총점에 따라 랭크를 결정합니다.
        // rankSprites 배열의 인덱스와 매칭되도록 합니다.

        if (totalScore >= 2000)
        {
            rankImage.sprite = rankSprites[0]; // S 랭크
        }
        else if (totalScore >= 1500)
        {
            rankImage.sprite = rankSprites[1]; // A 랭크
        }
        else if (totalScore >= 1000)
        {
            rankImage.sprite = rankSprites[2]; // B 랭크
        }
        else if (totalScore >= 500)
        {
            rankImage.sprite = rankSprites[3]; // C 랭크
        }
        else
        {
            rankImage.sprite = rankSprites[4]; // D 랭크
        }
    }

    private void UpdateResultUI()
    {
        // 플레이 타임을 시:분:초 형식으로 변환
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(playTime);
        playTimeText.text = $"플레이 타임: {timeSpan:hh\\:mm\\:ss}";

        monstersKilledText.text = $"처치한 몬스터 수: {monstersKilled}";
        floorsClearedText.text = $"클리어한 층 수: {floorsCleared}";
        bossesKilledText.text = $"처치한 보스 수: {bossesKilled}";
        moneyEarnedText.text = $"획득한 돈: {moneyEarned}원";
        totalScoreText.text = $"총 점수: {totalScore}";
    }

    // 스탯 증가 메서드들
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

    // 스탯 초기화
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
        // 총점 계산 및 랭크 결정
        CalculateTotalScore();
        DetermineRank();

        // 결과 UI 업데이트
        UpdateResultUI();

        // 결과 패널 활성화
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(true);
        }

        // 테스트용: 텍스트가 제대로 업데이트되는지 확인
        Debug.Log($"플레이 타임: {playTimeText.text}");
        Debug.Log($"처치한 몬스터 수: {monstersKilledText.text}");
        Debug.Log($"클리어한 층 수: {floorsClearedText.text}");
        Debug.Log($"처치한 보스 수: {bossesKilledText.text}");
        Debug.Log($"획득한 돈: {moneyEarnedText.text}");
        Debug.Log($"총 점수: {totalScoreText.text}");
    }

    public void SceneChanageCloseAnimationAddLoad(string NextSceneName)
    {
        chanageSkeleton.PlayCloseAnimation(NextSceneName);
    }
}
