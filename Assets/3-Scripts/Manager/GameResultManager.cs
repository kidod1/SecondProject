using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    [Header("Rank Settings")]
    public Sprite[] rankSprites; // ��ũ �̹������� �迭�� ����
    public Image rankImage; // ��ũ �̹����� ǥ���� UI �̹���

    [Header("UI Text Elements")]
    public TMP_Text playTimeText;
    public TMP_Text monstersKilledText;
    public TMP_Text floorsClearedText;
    public TMP_Text bossesKilledText;
    public TMP_Text moneyEarnedText;
    public TMP_Text totalScoreText;

    [Header("Player Stats")]
    public float playTime; // �÷��� Ÿ�� (�� ������ ����)
    public int monstersKilled;
    public int floorsCleared;
    public int bossesKilled;
    public int moneyEarned;

    [Header("Score Settings")]
    public int scoreForMonsterKill = 10;
    public int scoreForFloorClear = 100;
    public int scoreForBossKill = 500;
    public int scoreForMoney = 1; // 1���� ����

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
        // �÷��� Ÿ�ӿ� ���� �߰� ������ ������ ������ ���⿡ �߰�
    }

    private void DetermineRank()
    {
        // ������ ���� ��ũ�� �����մϴ�.
        // ���÷� ������ ���� ��ũ�� �����մϴ�.
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

    private void UpdateUI()
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

    // �ʿ��� ���, �ٸ� Ŭ�������� ������ ������ �� �ֵ��� public �޼��带 �����մϴ�.
    public void SetStats(float playTime, int monstersKilled, int floorsCleared, int bossesKilled, int moneyEarned)
    {
        this.playTime = playTime;
        this.monstersKilled = monstersKilled;
        this.floorsCleared = floorsCleared;
        this.bossesKilled = bossesKilled;
        this.moneyEarned = moneyEarned;
    }
}
