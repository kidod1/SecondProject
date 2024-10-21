using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    [Header("Rank Settings")]
    public Sprite[] rankSprites; // 랭크 이미지들을 배열로 저장
    public Image rankImage; // 랭크 이미지를 표시할 UI 이미지

    [Header("UI Text Elements")]
    public TMP_Text playTimeText;
    public TMP_Text monstersKilledText;
    public TMP_Text floorsClearedText;
    public TMP_Text bossesKilledText;
    public TMP_Text moneyEarnedText;
    public TMP_Text totalScoreText;

    [Header("Player Stats")]
    public float playTime; // 플레이 타임 (초 단위로 저장)
    public int monstersKilled;
    public int floorsCleared;
    public int bossesKilled;
    public int moneyEarned;

    [Header("Score Settings")]
    public int scoreForMonsterKill = 10;
    public int scoreForFloorClear = 100;
    public int scoreForBossKill = 500;
    public int scoreForMoney = 1; // 1원당 점수

    private int totalScore;

    void Start()
    {
        CalculateTotalScore();
        DetermineRank();
        UpdateUI();
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
        // 예시로 총점에 따른 랭크를 설정합니다.
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

    private void UpdateUI()
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

    // 필요한 경우, 다른 클래스에서 스탯을 설정할 수 있도록 public 메서드를 제공합니다.
    public void SetStats(float playTime, int monstersKilled, int floorsCleared, int bossesKilled, int moneyEarned)
    {
        this.playTime = playTime;
        this.monstersKilled = monstersKilled;
        this.floorsCleared = floorsCleared;
        this.bossesKilled = bossesKilled;
        this.moneyEarned = moneyEarned;
    }
}
